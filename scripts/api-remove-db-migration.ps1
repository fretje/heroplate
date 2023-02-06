$migratorPath = $pwd.path + '/../src/Api/Api.Infrastructure.Migrator/Api.Infrastructure.Migrator.csproj'
$startUpPath = $pwd.path + '/../src/Api/Api.Host/Api.Host.csproj'

Write-Host "Removing Migration..."
dotnet ef migrations remove --project $migratorPath --startup-project $startUpPath --context ApplicationDbContext