# Build context: the project folder (gnosispay-sync/)
# Usage: docker build -t gnosispay-sync -f Dockerfile .

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["gnosispay-sync.csproj", "./"]
RUN dotnet restore "./gnosispay-sync.csproj"
COPY . .
RUN dotnet build "./gnosispay-sync.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./gnosispay-sync.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gnosispay-sync.dll"]
