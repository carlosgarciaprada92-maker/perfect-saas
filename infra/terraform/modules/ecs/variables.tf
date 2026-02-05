variable "name_prefix" {
  type = string
}

variable "region" {
  type = string
}

variable "subnet_ids" {
  type = list(string)
}

variable "security_group_ids" {
  type = list(string)
}

variable "web_image" {
  type = string
}

variable "api_image" {
  type = string
}

variable "core_image" {
  type = string
}

variable "db_image" {
  type    = string
  default = "postgres:16-alpine"
}

variable "desired_count" {
  type    = number
  default = 1
}

variable "cpu" {
  type    = number
  default = 512
}

variable "memory" {
  type    = number
  default = 1024
}

variable "api_port" {
  type    = number
  default = 8080
}

variable "web_port" {
  type    = number
  default = 80
}

variable "core_port" {
  type    = number
  default = 8081
}

variable "assign_public_ip" {
  type    = bool
  default = true
}

variable "db_name" {
  type    = string
  default = "perfectdb"
}

variable "db_user" {
  type    = string
  default = "perfect"
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "api_environment" {
  type    = map(string)
  default = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
