FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ChronoLog.sln", "./"]
COPY ["ChronoLog.Applications/", "ChronoLog.Applications/"]
COPY ["ChronoLog.ChronoLogService/", "ChronoLog.ChronoLogService/"]
COPY ["ChronoLog.Core/", "ChronoLog.Core/"]
COPY ["ChronoLog.SqlDatabase/", "ChronoLog.SqlDatabase/"]
RUN ls -la /src
RUN ls -la /src/ChronoLog.Applications
RUN ls -la /src/ChronoLog.ChronoLogService
RUN ls -la /src/ChronoLog.Core
RUN ls -la /src/ChronoLog.SqlDatabase
RUN dotnet restore "ChronoLog.ChronoLogService/ChronoLog.ChronoLogService.csproj"

COPY . .

WORKDIR "/src/ChronoLog.ChronoLogService"
RUN dotnet build "ChronoLog.ChronoLogService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChronoLog.ChronoLogService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet tool install --global dotnet-ef --version 10.0.0
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ChronoLog.ChronoLogService.dll"]
