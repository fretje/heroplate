param(
     [Parameter(Mandatory=$true)]
     [ValidateNotNullOrEmpty()]
     [string]$commitMessage  
)

$migratorPath = $pwd.path + '/../src/Api/Api.Infrastructure.Migrator/Api.Infrastructure.Migrator.csproj'
$startUpPath = $pwd.path + '/../src/Api/Api.Host/Api.Host.csproj'

Write-Host "Adding Migration..."
dotnet ef migrations add $commitMessage --project $migratorPath --startup-project $startUpPath --context ApplicationDbContext -o Migrations/Application