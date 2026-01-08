FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["*.sln", "./"]
COPY ["ChronoLog.Applications/ChronoLog.Applications.csproj", "ChronoLog.Applications/"]
COPY ["ChronoLog.ChronoLogService/ChronoLog.ChronoLogService.csproj", "ChronoLog.ChronoLogService/"]
COPY ["ChronoLog.Core/ChronoLog.Core.csproj", "ChronoLog.Core/"]
COPY ["ChronoLog.SqlDatabase/ChronoLog.SqlDatabase.csproj", "ChronoLog.SqlDatabase/"]
RUN dotnet restore

COPY . .

WORKDIR "/src/ChronoLog.ChronoLogService"
RUN dotnet build "ChronoLog.ChronoLogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChronoLog.ChronoLogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet tool install --global dotnet-ef --version 10.0.0
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ChronoLog.ChronoLogService.dll"]
