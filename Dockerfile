FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ChronoLog.sln", "./"]
COPY ["ChronoLog.Applications/ChronoLog.Applications.csproj", "ChronoLog.Applications/"]
COPY ["ChronoLog.ChronoLogService/ChronoLog.ChronoLogService.csproj", "ChronoLog.ChronoLogService/"]
COPY ["ChronoLog.Core/ChronoLog.Core.csproj", "ChronoLog.Core/"]
COPY ["ChronoLog.SqlDatabase/ChronoLog.SqlDatabase.csproj", "ChronoLog.SqlDatabase/"]
COPY ["ChronoLog.Tests/ChronoLog.Tests.csproj", "ChronoLog.Tests/"]

RUN dotnet restore "ChronoLog.ChronoLogService/ChronoLog.ChronoLogService.csproj"
RUN dotnet restore "ChronoLog.Tests/ChronoLog.Tests.csproj"

COPY . .

# Build and test
RUN dotnet test "ChronoLog.Tests/ChronoLog.Tests.csproj" \
    -c $BUILD_CONFIGURATION \
    --no-restore \
    --logger "trx;LogFileName=test-results.trx"

WORKDIR "/src/ChronoLog.ChronoLogService"
RUN dotnet build "ChronoLog.ChronoLogService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChronoLog.ChronoLogService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChronoLog.ChronoLogService.dll"]
