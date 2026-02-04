FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Perfect.sln ./
COPY Perfect.Domain/Perfect.Domain.csproj Perfect.Domain/
COPY Perfect.Application/Perfect.Application.csproj Perfect.Application/
COPY Perfect.Infrastructure/Perfect.Infrastructure.csproj Perfect.Infrastructure/
COPY Perfect.Api/Perfect.Api.csproj Perfect.Api/
RUN dotnet restore Perfect.Api/Perfect.Api.csproj

COPY . .
RUN dotnet publish Perfect.Api/Perfect.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Perfect.Api.dll"]