output "ecr_api_repository_url" {
  value = module.ecr_api.repository_url
}

output "ecr_web_repository_url" {
  value = module.ecr_web.repository_url
}

output "ecs_cluster_name" {
  value = module.ecs.cluster_name
}

output "ecs_service_name" {
  value = module.ecs.service_name
}

output "web_port" {
  value = module.ecs.web_port
}
