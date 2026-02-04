variable "name_prefix" {
  type        = string
  description = "Resource name prefix"
}

variable "repository_name" {
  type        = string
  description = "Repository logical name"
  default     = "perfect-api"
}

variable "tags" {
  type    = map(string)
  default = {}
}
