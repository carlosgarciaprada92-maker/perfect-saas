# Contributing

## Branching
- `main` is protected and always deployable.
- Use short-lived feature branches: `feat/*`, `fix/*`, `chore/*`.

## Commits
- Keep commits atomic and descriptive.
- Recommended style: Conventional Commits (`feat:`, `fix:`, `chore:`).

## Local checks before PR
1. `cd apps/web && npm ci && npm run build`
2. `cd apps/api && dotnet test Perfect.sln`
3. `cd infra/terraform/envs/dev && terraform init -backend=false && terraform validate`

## Pull Requests
- Explain what changed and why.
- Add screenshots for UI changes.
- Mention any migration or environment variable changes.