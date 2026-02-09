param(
  [string]$Cluster = "perfect-dev-cluster",
  [string]$Service = "perfect-dev-app",
  [string]$Region = "us-east-2",
  [string]$TfDir = "infra/terraform/envs/dev"
)

$ErrorActionPreference = "Stop"

$tasks = aws ecs list-tasks --cluster $Cluster --service-name $Service --region $Region | ConvertFrom-Json
if (-not $tasks.taskArns -or $tasks.taskArns.Count -eq 0) {
  throw "No running tasks found for service $Service in $Cluster"
}

$taskArn = $tasks.taskArns[0]
$task = aws ecs describe-tasks --cluster $Cluster --tasks $taskArn --region $Region | ConvertFrom-Json
$eni = ($task.tasks[0].attachments[0].details | Where-Object { $_.name -eq 'networkInterfaceId' }).value
if (-not $eni) {
  throw "No ENI found for task $taskArn"
}

$ip = (aws ec2 describe-network-interfaces --network-interface-ids $eni --region $Region | ConvertFrom-Json).NetworkInterfaces[0].Association.PublicIp
if (-not $ip) {
  throw "No public IP found for ENI $eni"
}

$tfvarsPath = Join-Path $TfDir "terraform.tfvars"
$tfvars = Get-Content -Path $tfvarsPath -Raw
$updated = $tfvars -replace 'app_public_ip\s*=\s*\"[^\"]+\"', "app_public_ip        = `"$ip`""
Set-Content -Path $tfvarsPath -Value $updated

terraform -chdir=$TfDir apply -auto-approve

Write-Host "Updated app_public_ip to $ip and applied Terraform."
