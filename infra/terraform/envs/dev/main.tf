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
  app_port    = 8080
  tags        = local.common_tags
}

module "ecr" {
  source      = "../../modules/ecr"
  name_prefix = var.name_prefix
  tags        = local.common_tags
}

module "rds" {
  source             = "../../modules/rds"
  name_prefix        = var.name_prefix
  subnet_ids         = module.network.private_subnet_ids
  security_group_ids = [module.network.rds_security_group_id]
  db_password        = var.db_password
  tags               = local.common_tags
}

module "ecs" {
  source             = "../../modules/ecs"
  name_prefix        = var.name_prefix
  region             = var.aws_region
  subnet_ids         = module.network.public_subnet_ids
  security_group_ids = [module.network.ecs_security_group_id]
  container_image    = "${module.ecr.repository_url}:${var.api_image_tag}"
  desired_count      = 1
  cpu                = 256
  memory             = 512
  assign_public_ip   = true
  environment = {
    ASPNETCORE_ENVIRONMENT     = "Production"
    ASPNETCORE_URLS            = "http://+:8080"
    ConnectionStrings__Default = "Host=${module.rds.endpoint};Port=${module.rds.port};Database=${module.rds.db_name};Username=perfect;Password=${var.db_password}"
    Jwt__Issuer                = "Perfect.Api"
    Jwt__Audience              = "Perfect.Client"
    Jwt__Key                   = "CHANGE_ME_IN_SSM_OR_SECRETS_MANAGER"
    Tenant__Mode               = "mixed"
    Tenant__HeaderName         = "X-Tenant-Id"
    Cors__AllowedOrigins       = var.cors_allowed_origins
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
