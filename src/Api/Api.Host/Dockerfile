#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /

COPY ["Directory.Build.props", "/"]
COPY ["Directory.Build.targets", "/"]
COPY ["src/Api/Api.Application/Api.Application.csproj", "src/Api/Api.Application/"]
COPY ["src/Api/Api.Contracts/Api.Contracts.csproj", "src/Api/Api.Contracts/"]
COPY ["src/Api/Api.Domain/Api.Domain.csproj", "src/Api/Api.Domain/"]
COPY ["src/Api/Api.Host/Api.Host.csproj", "src/Api/Api.Host/"]
COPY ["src/Api/Api.Infrastructure/Api.Infrastructure.csproj", "src/Api/Api.Infrastructure/"]
COPY ["src/Api/Api.Infrastructure.Migrator/Api.Infrastructure.Migrator.csproj", "src/Api/Api.Infrastructure.Migrator/"]
COPY ["src/Shared/Shared.Authorization/Shared.Authorization.csproj", "src/Shared/Shared.Authorization/"]

RUN dotnet restore "src/Api/Api.Host/Api.Host.csproj" --locked-mode

COPY . .
WORKDIR "src/Api/Api.Host"
RUN dotnet publish "Api.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final

COPY --from=build /app/publish .

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Heroplate.Api.Host.dll"]