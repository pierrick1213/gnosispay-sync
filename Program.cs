using gnosispay_sync;
using gnosispay_sync.Configuration;
using gnosispay_sync.Database;
using gnosispay_sync.Services.Auth;
using gnosispay_sync.Services.Transactions;
using gnosispay_sync.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GnosisPayOptions>(
    builder.Configuration.GetSection(GnosisPayOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' not found in configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

builder.Services.AddHttpClient<IGnosisPayAuthService, GnosisPayAuthService>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GnosisPayOptions>>().Value;
    client.BaseAddress = new Uri(opts.ApiBaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("gnosispay-sync/1.0");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<IGnosisPayTransactionsClient, GnosisPayTransactionsClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GnosisPayOptions>>().Value;
    client.BaseAddress = new Uri(opts.ApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("GnosisPaySync/1.0");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddSingleton<IInMemoryJwtTokenProvider, InMemoryJwtTokenProvider>();
builder.Services.AddScoped<ITransactionBackfillService, TransactionBackfillService>();
builder.Services.AddScoped<ITransactionSyncService, TransactionSyncService>();
builder.Services.AddHostedService<JwtTokenStartupHostedService>();
builder.Services.AddHostedService<TransactionBackfillStartupHostedService>();

builder.Services.AddQuartz(q =>
{
    var syncNewKey = new JobKey(nameof(SyncNewTransactionsJob));
    q.AddJob<SyncNewTransactionsJob>(opts => opts.WithIdentity(syncNewKey));
    q.AddTrigger(opts => opts
        .ForJob(syncNewKey)
        .WithIdentity($"{nameof(SyncNewTransactionsJob)}-trigger")
        .WithCronSchedule("0 0 * * * ?"));

    var refreshPendingKey = new JobKey(nameof(RefreshPendingTransactionsJob));
    q.AddJob<RefreshPendingTransactionsJob>(opts => opts.WithIdentity(refreshPendingKey));
    q.AddTrigger(opts => opts
        .ForJob(refreshPendingKey)
        .WithIdentity($"{nameof(RefreshPendingTransactionsJob)}-trigger")
        .WithCronSchedule("0 30 */6 * * ?"));
});

builder.Services.AddQuartzHostedService(opts =>
{
    opts.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();
