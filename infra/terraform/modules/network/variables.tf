variable "name_prefix" {
  type        = string
  description = "Resource name prefix"
}

variable "vpc_cidr" {
  type        = string
  description = "VPC CIDR block"
  default     = "10.40.0.0/16"
}

variable "az_count" {
  type        = number
  description = "Number of availability zones/subnet pairs"
  default     = 2
}

variable "app_port" {
  type        = number
  description = "API container port"
  default     = 8080
}

variable "tags" {
  type        = map(string)
  description = "Common tags"
  default     = {}
}
