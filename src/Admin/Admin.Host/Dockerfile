#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /

COPY ["Directory.Build.props", "/"]
COPY ["Directory.Build.targets", "/"]
COPY ["src/Admin/Admin.Application/Admin.Application.csproj", "src/Admin/Admin.Application/"]
COPY ["src/Admin/Admin.Host/Admin.Host.csproj", "src/Admin/Admin.Host/"]
COPY ["src/Admin/Admin.Infrastructure/Admin.Infrastructure.csproj", "src/Admin/Admin.Infrastructure/"]
COPY ["src/Api/Api.Contracts/Api.Contracts.csproj", "src/Api/Api.Contracts/"]
COPY ["src/Shared/Shared.Authorization/Shared.Authorization.csproj", "src/Shared/Shared.Authorization/"]

RUN dotnet restore "src/Admin/Admin.Host/Admin.Host.csproj" --locked-mode

COPY . .
WORKDIR "src/Admin/Admin.Host"

RUN dotnet publish "Admin.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Heroplate.Admin.Host.dll"]