$project = $pwd.path + '/../src/Admin/Admin.Infrastructure/Admin.Infrastructure.csproj'

Write-Host "Make sure the Heroplate Api is running on port 5001.`n"
Write-Host "Press any key to continue...`n"
$null = [Console]::Read()

dotnet build -t:NSwag $project
