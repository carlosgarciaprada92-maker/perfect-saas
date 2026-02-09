locals {
  common_tags = {
    Project     = "Perfect"
    Environment = "dev"
    ManagedBy   = "terraform"
  }
}

module "network" {
  source      = "../../modules/network"
  name_prefix = var.name_prefix
  app_port    = 80
  tags        = local.common_tags
}

module "ecr_api" {
  source          = "../../modules/ecr"
  name_prefix     = var.name_prefix
  repository_name = "api"
  tags            = local.common_tags
}

module "ecr_web" {
  source          = "../../modules/ecr"
  name_prefix     = var.name_prefix
  repository_name = "web"
  tags            = local.common_tags
}

module "ecr_core" {
  source          = "../../modules/ecr"
  name_prefix     = var.name_prefix
  repository_name = "core"
  tags            = local.common_tags
}

resource "aws_apigatewayv2_api" "app" {
  name          = "${var.name_prefix}-http"
  protocol_type = "HTTP"

  cors_configuration {
    allow_credentials = false
    allow_headers     = ["*"]
    allow_methods     = ["*"]
    allow_origins     = ["*"]
    expose_headers    = ["*"]
    max_age           = 3600
  }

  tags = local.common_tags
}

resource "aws_apigatewayv2_integration" "app" {
  api_id                 = aws_apigatewayv2_api.app.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "ANY"
  payload_format_version = "1.0"
  integration_uri        = "http://${var.app_public_ip}"
}

resource "aws_apigatewayv2_route" "app_default" {
  api_id    = aws_apigatewayv2_api.app.id
  route_key = "$default"
  target    = "integrations/${aws_apigatewayv2_integration.app.id}"
}

resource "aws_apigatewayv2_stage" "app" {
  api_id      = aws_apigatewayv2_api.app.id
  name        = "$default"
  auto_deploy = true
  tags        = local.common_tags
}

module "ecs" {
  source             = "../../modules/ecs"
  name_prefix        = var.name_prefix
  region             = var.aws_region
  subnet_ids         = module.network.public_subnet_ids
  security_group_ids = [module.network.ecs_security_group_id]
  web_image          = "${module.ecr_web.repository_url}:${var.image_tag}"
  api_image          = "${module.ecr_api.repository_url}:${var.image_tag}"
  core_image         = "${module.ecr_core.repository_url}:${var.image_tag}"
  db_password        = var.db_password
  desired_count      = 1
  cpu                = 1024
  memory             = 2048
  assign_public_ip   = true
  api_environment = {
    ASPNETCORE_ENVIRONMENT     = "Production"
    ASPNETCORE_URLS            = "http://+:8080"
    ConnectionStrings__Default = "Host=127.0.0.1;Port=5432;Database=perfectdb;Username=perfect;Password=${var.db_password}"
    Jwt__Issuer                = "Perfect.Api"
    Jwt__Audience              = "Perfect.Client"
    Jwt__Key                   = var.jwt_key
    Tenant__Mode               = "mixed"
    Tenant__HeaderName         = "X-Tenant-Id"
    Tenant__SlugHeaderName     = "X-Tenant-Slug"
    Cors__AllowedOrigins       = var.cors_allowed_origins
    Platform__BootstrapKey     = ""
    Platform__AllowDemoSeed    = "true"
    Swagger__Enabled           = "true"
    CORE_DEFAULT_MODULE_BASEURL = aws_apigatewayv2_api.app.api_endpoint
  }
  tags = local.common_tags
}

module "observability" {
  source       = "../../modules/observability"
  name_prefix  = var.name_prefix
  cluster_name = module.ecs.cluster_name
  service_name = module.ecs.service_name
  tags         = local.common_tags
}
