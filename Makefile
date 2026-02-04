.PHONY: web-install web-build api-build api-test docker-up docker-down tf-fmt tf-validate-dev tf-validate-prod

web-install:
	cd apps/web && npm ci

web-build:
	cd apps/web && npm run build

api-build:
	cd apps/api && dotnet build Perfect.sln

api-test:
	cd apps/api && dotnet test Perfect.sln

docker-up:
	docker compose up -d --build

docker-down:
	docker compose down

tf-fmt:
	cd infra/terraform && terraform fmt -recursive

tf-validate-dev:
	cd infra/terraform/envs/dev && terraform init -backend=false && terraform validate

tf-validate-prod:
	cd infra/terraform/envs/prod && terraform init -backend=false && terraform validate