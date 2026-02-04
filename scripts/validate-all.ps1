$ErrorActionPreference = 'Stop'

Write-Host '==> Web build'
Push-Location "apps/web"
npm ci
npm run build
Pop-Location

Write-Host '==> API tests'
Push-Location "apps/api"
dotnet test Perfect.sln
Pop-Location

Write-Host '==> Terraform checks'
Push-Location "infra/terraform"
terraform fmt -recursive
Pop-Location

Push-Location "infra/terraform/envs/dev"
terraform init -backend=false
terraform validate
Pop-Location

Write-Host 'Validation completed.'