# Usage: .\build.ps1 -version "1.0"
param (
    [string]$version = "1.1"
)

$ErrorActionPreference = "Stop"

$packageName = "TeamCon2025.Customer.GrpcContracts"

$profilesProto = "https://raw.githubusercontent.com/vchistov/team-con2025-protos/refs/heads/master/customer/profiles.proto"
$avatarsProto = "https://raw.githubusercontent.com/vchistov/team-con2025-protos/refs/heads/master/customer/avatars.proto"

Write-Host "Starting package '$packageName' building..." -ForegroundColor DarkGreen

$tmpRootDir = [System.IO.Path]::GetTempPath()
$tmpDirName = [System.IO.Path]::GetRandomFileName()
$tmpDir = [System.IO.Path]::Combine($tmpRootDir, $tmpDirName)
$projectFullName = [System.IO.Path]::Combine($tmpDir, "$packageName.csproj")

# Create temp directory to build contracts
New-Item -Path $tmpRootDir -Name $tmpDirName -ItemType Directory | Out-Null

# Create contracts project
dotnet new classlib --language "C#" --name $packageName --framework "NET9.0" --no-update-check --output $tmpDir

# Get rid of redundant 'Class1.cs' file from template
Remove-Item ([System.IO.Path]::Combine($tmpDir, "Class1.cs")) -Force -Confirm:$false

# Add contracts
dotnet grpc add-url --services Both --project $projectFullName --output ./profiles.proto $profilesProto
dotnet grpc add-url --services Both --project $projectFullName --output ./avatars.proto $avatarsProto

# Build and pack the project
dotnet pack -c Release -o ./packages/ $projectFullName /p:Version=$version

Write-Host "Package has been built" -ForegroundColor DarkGreen
