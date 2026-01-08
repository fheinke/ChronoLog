FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ChronoLog.csproj", "./"]
RUN dotnet restore "ChronoLog.csproj"
COPY . .
RUN dotnet build "ChronoLog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChronoLog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet tool install --global dotnet-ef --version 10.0.0
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ChronoLog.dll"]
