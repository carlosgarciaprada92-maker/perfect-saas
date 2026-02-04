variable "aws_region" {
  type    = string
  default = "us-east-2"
}

variable "name_prefix" {
  type    = string
  default = "perfect-dev"
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "jwt_key" {
  type      = string
  sensitive = true
}

variable "image_tag" {
  type    = string
  default = "latest"
}

variable "cors_allowed_origins" {
  type    = string
  default = "*"
}