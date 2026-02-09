resource "aws_cloudwatch_log_group" "app" {
  name              = "/ecs/${var.name_prefix}-app"
  retention_in_days = 14

  tags = merge(var.tags, {
    Name = "${var.name_prefix}-app-logs"
  })
}

data "aws_iam_policy_document" "ecs_task_execution_assume" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

resource "aws_iam_role" "ecs_task_execution" {
  name               = "${var.name_prefix}-ecs-task-execution-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_execution_assume.json
  tags               = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role" "ecs_task" {
  name               = "${var.name_prefix}-ecs-task-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_execution_assume.json
  tags               = var.tags
}

resource "aws_ecs_cluster" "this" {
  name = "${var.name_prefix}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = var.tags
}

locals {
  api_environment = [
    for key, value in merge(
      {
        ASPNETCORE_ENVIRONMENT = "Production"
        ASPNETCORE_URLS        = "http://+:8080"
      },
      var.api_environment
    ) : { name = key, value = value }
  ]
}

resource "aws_ecs_task_definition" "app" {
  family                   = "${var.name_prefix}-app"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = tostring(var.cpu)
  memory                   = tostring(var.memory)
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name      = "db"
      image     = var.db_image
      essential = true
      environment = [
        { name = "POSTGRES_DB", value = var.db_name },
        { name = "POSTGRES_USER", value = var.db_user },
        { name = "POSTGRES_PASSWORD", value = var.db_password }
      ]
      healthCheck = {
        command     = ["CMD-SHELL", "pg_isready -U ${var.db_user} -d ${var.db_name}"]
        interval    = 10
        timeout     = 5
        retries     = 6
        startPeriod = 20
      }
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.app.name
          awslogs-region        = var.region
          awslogs-stream-prefix = "db"
        }
      }
    },
    {
      name      = "api"
      image     = var.api_image
      essential = true
      dependsOn = [
        {
          containerName = "db"
          condition     = "HEALTHY"
        }
      ]
      portMappings = [
        {
          containerPort = var.api_port
          hostPort      = var.api_port
          protocol      = "tcp"
        }
      ]
      environment = local.api_environment
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.app.name
          awslogs-region        = var.region
          awslogs-stream-prefix = "api"
        }
      }
    },
    {
      name      = "core"
      image     = var.core_image
      essential = true
      dependsOn = [
        {
          containerName = "api"
          condition     = "START"
        }
      ]
      portMappings = [
        {
          containerPort = var.core_port
          hostPort      = var.core_port
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.app.name
          awslogs-region        = var.region
          awslogs-stream-prefix = "core"
        }
      }
    },
    {
      name      = "web"
      image     = var.web_image
      essential = true
      dependsOn = [
        {
          containerName = "api"
          condition     = "START"
        }
      ]
      portMappings = [
        {
          containerPort = var.web_port
          hostPort      = var.web_port
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.app.name
          awslogs-region        = var.region
          awslogs-stream-prefix = "web"
        }
      }
    }
  ])

  tags = var.tags
}

resource "aws_ecs_service" "app" {
  name                   = "${var.name_prefix}-app"
  cluster                = aws_ecs_cluster.this.id
  task_definition        = aws_ecs_task_definition.app.arn
  desired_count          = var.desired_count
  launch_type            = "FARGATE"
  wait_for_steady_state  = true
  enable_execute_command = true

  dynamic "load_balancer" {
    for_each = var.lb_target_group_arn != "" ? [1] : []
    content {
      target_group_arn = var.lb_target_group_arn
      container_name   = var.lb_container_name
      container_port   = var.lb_container_port
    }
  }

  network_configuration {
    subnets          = var.subnet_ids
    security_groups  = var.security_group_ids
    assign_public_ip = var.assign_public_ip
  }

  deployment_minimum_healthy_percent = 0
  deployment_maximum_percent         = 200

  tags = var.tags
}
