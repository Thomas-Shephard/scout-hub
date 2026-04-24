FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/ScoutHub.Api/*.csproj ./src/ScoutHub.Api/
COPY src/ScoutHub.Data/*.csproj ./src/ScoutHub.Data/
RUN dotnet restore ./src/ScoutHub.Api/ScoutHub.Api.csproj

COPY src/ScoutHub.Api/ ./src/ScoutHub.Api/
COPY src/ScoutHub.Data/ ./src/ScoutHub.Data/
WORKDIR /app/src/ScoutHub.Api
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
USER app
ENTRYPOINT ["dotnet", "ScoutHub.Api.dll"]
