FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/ScoutHub.Migrations/*.csproj ./src/ScoutHub.Migrations/
RUN dotnet restore ./src/ScoutHub.Migrations/ScoutHub.Migrations.csproj

COPY src/ScoutHub.Migrations/ ./src/ScoutHub.Migrations/
WORKDIR /app/src/ScoutHub.Migrations
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/out .
USER $APP_UID
ENTRYPOINT ["dotnet", "ScoutHub.Migrations.dll"]
