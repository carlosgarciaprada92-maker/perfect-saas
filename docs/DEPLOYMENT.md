# Deployment Guide

## 1) Local checks

```bash
cd apps/web
npm ci
npm run build
```

```bash
cd apps/api
dotnet build Perfect.sln -c Release
dotnet test Perfect.sln -c Release
```

```bash
docker compose up -d --build
```

Validaciones locales:
- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

## 2) AWS dev deploy (ECS Fargate + ECR)

### 2.1 Preparar Terraform
```bash
cd infra/terraform/envs/dev
cp terraform.tfvars.example terraform.tfvars
# editar: db_password, jwt_key, cors_allowed_origins
terraform init -backend=false
terraform fmt -recursive
terraform validate
```

### 2.2 Crear ECR repos
```bash
terraform apply -auto-approve -target=module.ecr_api -target=module.ecr_web
```

### 2.3 Build + push web/api
```bash
API_REPO=$(terraform output -raw ecr_api_repository_url)
WEB_REPO=$(terraform output -raw ecr_web_repository_url)
TAG=deploy1

aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin 546460568778.dkr.ecr.us-east-2.amazonaws.com

docker build -f apps/api/Perfect.Api/Dockerfile -t $API_REPO:$TAG apps/api
docker tag $API_REPO:$TAG $API_REPO:latest
docker push $API_REPO:$TAG
docker push $API_REPO:latest

docker build -f apps/web/Dockerfile -t $WEB_REPO:$TAG apps/web
docker tag $WEB_REPO:$TAG $WEB_REPO:latest
docker push $WEB_REPO:$TAG
docker push $WEB_REPO:latest
```

### 2.4 Apply infraestructura + servicio
```bash
terraform apply -auto-approve -var="image_tag=deploy1"
```

### 2.5 Obtener URL pública del servicio
```bash
TASK_ARN=$(aws ecs list-tasks --cluster perfect-dev-cluster --service-name perfect-dev-app --desired-status RUNNING --region us-east-2 --query 'taskArns[0]' --output text)
ENI_ID=$(aws ecs describe-tasks --cluster perfect-dev-cluster --tasks $TASK_ARN --region us-east-2 --query 'tasks[0].attachments[0].details[?name==`networkInterfaceId`].value' --output text)
PUBLIC_IP=$(aws ec2 describe-network-interfaces --network-interface-ids $ENI_ID --region us-east-2 --query 'NetworkInterfaces[0].Association.PublicIp' --output text)

echo "Frontend: http://$PUBLIC_IP"
echo "Health:   http://$PUBLIC_IP/api/health"
echo "Swagger:  http://$PUBLIC_IP/swagger/"
```

### 2.6 Smoke tests
```bash
curl -I http://$PUBLIC_IP/
curl -I http://$PUBLIC_IP/api/health
curl -I -L http://$PUBLIC_IP/swagger/
```

## 3) Redeploy rápido

```bash
cd infra/terraform/envs/dev
terraform output -raw ecr_api_repository_url
terraform output -raw ecr_web_repository_url

# build/push con TAG nuevo
# luego:
terraform apply -auto-approve -var="image_tag=<nuevo-tag>"
```

## 4) Destroy

```bash
cd infra/terraform/envs/dev
terraform destroy -auto-approve -var="image_tag=deploy1"
```

## 5) Notas de arquitectura actual (dev)

- Endpoint público único en la IP pública de la tarea ECS.
- NGINX (web) enruta `/api/*`, `/health` y `/swagger/*` hacia el contenedor API.
- Base de datos PostgreSQL corre como sidecar dentro de la misma tarea ECS (solo demo/costo mínimo).

## 6) Próximos pasos de producción

1. Separar DB a RDS PostgreSQL privado.
2. Añadir ALB + HTTPS (ACM + Route53).
3. Mover secretos a SSM/Secrets Manager.
4. Habilitar autoscaling por CPU/memoria y despliegues rolling/blue-green.
