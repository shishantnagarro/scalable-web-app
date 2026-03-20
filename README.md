# AWS Advance Assignment – .NET 8 Starter

This repository contains a ready-to-run .NET 8 Web API integrated with AWS (S3, DynamoDB, optional RDS) and deployment scaffolding for Elastic Beanstalk, Lambda, and GitHub Actions.

> Default DB provider is **MySQL** (Pomelo). To switch to **PostgreSQL**, replace provider as explained below.

## Local run
```bash
dotnet restore
dotnet build
dotnet run --project ScalableWebApp
```
Navigate to `https://localhost:5001/swagger` for API docs.

## Endpoints
- `GET /` root info
- `GET /health` health probe
- `POST /api/files/upload` (form-data: file)
- `GET  /api/files/list`
- `DELETE /api/files/{key}`
- `GET  /api/logs/dynamo`
- `GET  /api/logs/rds`
- `POST /api/logs/rds` (JSON body of ApplicationLog)

## Configuration
Set values in `appsettings.json` or environment variables in `.ebextensions/01-environment.config`:
- `AWS:Region`, `AWS:S3BucketName`, `AWS:DynamoDBTableName`, `AWS:SecretsManagerSecretName`
- `ConnectionStrings:DefaultConnection` (for RDS)

## Switch to PostgreSQL
1. Update packages in `ScalableWebApp.csproj`:
   - Remove `Pomelo.EntityFrameworkCore.MySql`
   - Add `<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.2" />`
2. Change `Program.cs` registration:
```csharp
options.UseNpgsql(connectionString);
```
3. Update connection string: `Host=...;Port=5432;Database=...;Username=...;Password=...;SslMode=Require;` (or as needed).
4. Add EF design-time factory or tools as required; run migrations with `dotnet ef`.

## EB deploy (manual)
1. `dotnet publish -c Release -o ./publish`
2. Zip the *contents* of `publish/` and upload in EB console when creating a .NET on Amazon Linux 2 platform environment.

## CI/CD (GitHub Actions)
- See `.github/workflows/deploy-dotnet.yml` (expects AWS creds + `EB_S3_BUCKET`).


## AWS DATA
- VPS : vpc-0efaa0500b9076f2b 

|           DescribeSecurityGroups           |
+-----------------------+--------------------+
|  sg-08c43b46f45fc7eef |  load-balancer-sg  |
|  sg-0d9e604ff7c5278e1 |  default           |
|  sg-07084c3777e47a499 |  database-sg       |
|  sg-0866abe59037c78b0 |  web-app-sg        |