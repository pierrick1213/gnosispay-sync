using gnosispay_sync;
using gnosispay_sync.Configuration;
using gnosispay_sync.Services.Auth;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GnosisPayOptions>(
    builder.Configuration.GetSection(GnosisPayOptions.SectionName));

builder.Services.AddHttpClient<IGnosisPayAuthService, GnosisPayAuthService>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GnosisPayOptions>>().Value;
    client.BaseAddress = new Uri(opts.ApiBaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("gnosispay-sync/1.0");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddSingleton<IInMemoryJwtTokenProvider, InMemoryJwtTokenProvider>();
builder.Services.AddHostedService<JwtTokenStartupHostedService>();

var host = builder.Build();
host.Run();
