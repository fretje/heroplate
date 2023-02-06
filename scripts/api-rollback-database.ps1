param(
     [Parameter(Mandatory=$true)]
     [ValidateNotNullOrEmpty()]
     [string]$commit
)
 
$migratorPath = $pwd.path + '/../src/Api/Api.Infrastructure.Migrator/Api.Infrastructure.Migrator.csproj'
$startUpPath = $pwd.path + '/../src/Api/Api.Host/Api.Host.csproj'

Write-Host "Rolling back database to Migration $commit..."
dotnet ef database update $commit --project $migratorPath --startup-project $startUpPath --context ApplicationDbContext