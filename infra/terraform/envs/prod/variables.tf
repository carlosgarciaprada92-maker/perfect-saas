variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "name_prefix" {
  type    = string
  default = "perfect-prod"
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "api_image_tag" {
  type    = string
  default = "latest"
}

variable "cors_allowed_origins" {
  type    = string
  default = "https://perfect.example.com"
}
