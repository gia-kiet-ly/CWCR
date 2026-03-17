# 🌱 CWCR - Citizen Waste Collection & Recycling

> A comprehensive waste management and recycling platform connecting citizens, recycling enterprises, and waste collectors.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4)](https://learn.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%209-512BD4)](https://learn.microsoft.com/en-us/ef/core/)
[![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Database-0078D4?logo=microsoftazure)](https://azure.microsoft.com/en-us/products/azure-sql/database/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [Database Setup](#-database-setup)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Environment Variables](#-environment-variables)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🌟 Overview

**CWCR** is a modern waste management platform that facilitates:

- 📱 **Citizens** reporting waste locations and earning points
- 🏭 **Recycling Enterprises** managing collection operations
- 🚛 **Waste Collectors** handling pickup assignments
- 👨‍💼 **Administrators** overseeing system operations

The platform promotes environmental sustainability through gamification, leaderboards, and efficient waste collection routing.

---

## ✨ Features

### 🔐 Authentication & Authorization

- ✅ User registration with email verification
- ✅ JWT-based authentication
- ✅ Refresh token mechanism
- ✅ Role-based access control (Admin, Citizen, Enterprise, Collector)

### 👥 User Management

- ✅ Multi-role user system
- ✅ Profile management with Ward/District location
- ✅ Account activation/deactivation
- ✅ Email verification workflow

### 📍 Waste Reporting

- ✅ Geo-located waste reports
- ✅ Multiple waste type classification
- ✅ Image upload support
- ✅ Status tracking (Pending → Verified → Collected)
- ✅ Point calculation for verified reports

### 🏢 Enterprise Management

- ✅ Enterprise registration & approval workflow
- ✅ Document upload (licenses, certificates)
- ✅ Service area configuration (District/Ward)
- ✅ Waste type capability management
- ✅ Daily capacity tracking
- ✅ Collector management

### 🚛 Collection System

- ✅ Collection request creation
- ✅ Collector assignment & routing
- ✅ Real-time status updates
- ✅ Collection proof upload
- ✅ Automatic redispatch on failure

### 🎯 Gamification & Leaderboards

- ✅ Point system based on waste collection
- ✅ Configurable point rules per waste type
- ✅ Multi-dimensional leaderboards:
  - Global ranking
  - Ward-based ranking
  - District-based ranking
  - Time-based (Daily/Weekly/Monthly/Yearly)
- ✅ Personal rank tracking
- ✅ Points-to-next-rank calculation

### 📊 Analytics & Reporting

- ✅ Recycling statistics per enterprise
- ✅ Regional waste collection data
- ✅ Point history tracking
- ✅ Complaint & dispute resolution system

---

## 🏗️ Architecture

This project follows **Clean Architecture** principles:

```
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                            │
│  (Controllers, Middleware, Authentication, Authorization)    │
└────────────────┬────────────────────────────────────────────┘
                 │
┌───────────────��▼────────────────────────────────────────────┐
│                    Application Layer                         │
│         (Business Logic, Services, Use Cases)                │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│                      Domain Layer                            │
│          (Entities, Value Objects, Domain Events)            │
└──────────────────────────────────────────────────────────────┘
                 ▲
┌────────────────┴────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (Database, External Services, Email, File Storage, JWT)    │
└──────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **API** | HTTP endpoints, request/response handling | Application |
| **Application** | Business logic, orchestration | Domain, Infrastructure.Contract |
| **Domain** | Core business entities and rules | None (pure) |
| **Infrastructure** | Data persistence, external integrations | Domain, Application.Contract |
| **Core** | Shared utilities, enums, constants | None |

---

## 🛠️ Tech Stack

### Backend

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **ORM**: Entity Framework Core 9.0
- **Database**: Azure SQL Database / SQL Server
- **Authentication**: ASP.NET Core Identity + JWT
- **Email**: SMTP (configurable)
- **File Storage**: Local file system (extensible to Azure Blob)

### Architecture Patterns

- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Dependency Injection
- ✅ CQRS-like separation (DTOs)

### Tools & Libraries

- **Mapping**: Manual mapping (extensible to AutoMapper)
- **Validation**: Data Annotations + FluentValidation (planned)
- **Logging**: Built-in .NET logging (extensible to Serilog/Application Insights)

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [Azure SQL Database](https://azure.microsoft.com/en-us/products/azure-sql/database/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio/) (optional)

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/gia-kiet-ly/CWCR.git
cd CWCR
```

2. **Restore dependencies**

```bash
dotnet restore
```

3. **Update connection string**

Edit `API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=CWCR;User ID=your-user;Password=your-password;TrustServerCertificate=True;"
  }
}
```

4. **Apply database migrations**

```bash
dotnet ef database update -p Infrastructure -s API
```

5. **Configure email settings** (optional)

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "noreply@cwcr.com",
    "EnableSsl": true
  }
}
```

6. **Configure frontend URL**

```json
{
  "Frontend": {
    "BaseUrl": "http://localhost:3000"
  }
}
```

7. **Run the application**

```bash
dotnet run --project API
```

8. **Access the API**

- Swagger UI: `https://localhost:7000/swagger`
- API Base URL: `https://localhost:7000/api`

---

## 🗄️ Database Setup

### Migration Commands

```bash
# Create a new migration
dotnet ef migrations add MigrationName -p Infrastructure -s API

# Update database to latest migration
dotnet ef database update -p Infrastructure -s API

# Remove last migration (if not applied)
dotnet ef migrations remove -p Infrastructure -s API

# Drop database (⚠️ WARNING: Deletes all data)
dotnet ef database drop -p Infrastructure -s API --force
```

### Seed Data

The system automatically seeds:

- **Roles**: Admin, Citizen, Enterprise, Collector
- **Admin User**: `admin@system.com` / `String@123`
- **Test Citizen**: `citizen@system.com` / `String@123`
- **Test Enterprise**: `enterprise@system.com` / `String@123`

---

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | Public |
| POST | `/api/auth/login` | Login | Public |
| GET | `/api/auth/verify-email` | Verify email | Public |
| POST | `/api/auth/refresh` | Refresh access token | Public |

### User Management

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| GET | `/api/admin/users` | Get all users (with filters) | Admin |
| PUT | `/api/admin/users/{id}/status` | Activate/deactivate user | Admin |

### Waste Reporting

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| POST | `/api/waste-reports` | Create waste report | Citizen |
| GET | `/api/waste-reports` | Get all reports (with filters) | All |
| GET | `/api/waste-reports/{id}` | Get report by ID | All |
| PUT | `/api/waste-reports/{id}` | Update report | Citizen |

### Leaderboard & Points

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| GET | `/api/CitizenPoint/leaderboard` | Get leaderboard (with filters) | Citizen, Admin |
| GET | `/api/CitizenPoint/my-rank` | Get my rank & stats | Citizen |
| GET | `/api/CitizenPoint/{citizenId}` | Get citizen points | Citizen, Admin |
| GET | `/api/CitizenPoint/{citizenId}/history` | Get point history | Citizen, Admin |

**Leaderboard Query Parameters:**

- `wardId` (GUID): Filter by ward
- `districtId` (GUID): Filter by district
- `period` (enum): `0=AllTime`, `1=Daily`, `2=Weekly`, `3=Monthly`, `4=Yearly`
- `topCount` (int): Number of top users (default: 10)

### Enterprise Management

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| POST | `/api/recycling-enterprises/me/profile` | Create/update enterprise profile | Enterprise |
| GET | `/api/recycling-enterprises/me/profile` | Get my enterprise profile | Enterprise |
| POST | `/api/recycling-enterprises/me/documents` | Upload documents | Enterprise |
| POST | `/api/recycling-enterprises/me/submit` | Submit for approval | Enterprise |
| GET | `/api/admin/enterprise-approvals` | Get all enterprises (for approval) | Admin |
| POST | `/api/admin/enterprise-approvals/approve` | Approve enterprise | Admin |
| POST | `/api/admin/enterprise-approvals/reject` | Reject enterprise | Admin |

### Point Rules (Admin)

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| GET | `/api/PointRule` | Get all point rules | Admin, Enterprise |
| POST | `/api/PointRule` | Create point rule | Admin |
| PUT | `/api/PointRule/{wasteTypeId}` | Update point rule | Admin |

---

## 📁 Project Structure

```
CWCR/
├── API/                          # Web API Layer
│   ├── Controllers/              # API Controllers
│   ├── Middlewares/              # Custom middlewares
│   ├── appsettings.json          # Configuration
│   └── Program.cs                # Application entry point
│
├── Application/                  # Application Layer
│   ├── Services/                 # Business logic services
│   └── DependencyInjection.cs    # Service registration
│
├── Application.Contract/         # Application contracts
│   ├── DTOs/                     # Data Transfer Objects
│   └── Interfaces.Services/      # Service interfaces
���
├── Domain/                       # Domain Layer
│   ├── Entities/                 # Domain entities
│   └── Base/                     # Base entities & exceptions
│
├── Infrastructure/               # Infrastructure Layer
│   ├── DbContext/                # EF Core DbContext
│   ├── Migrations/               # Database migrations
│   ├── Repo/                     # Repository implementations
│   ├── DataSeeds/                # Seed data
│   └── DependencyInjection.cs    # Infrastructure services
│
├── Infrastructure.Contract/      # Infrastructure contracts
│   └── Interfaces/               # Infrastructure interfaces
│
└── Core/                         # Shared utilities
    ├── Enum/                     # Enumerations
    └── Utils/                    # Helper classes
```

---

## ⚙️ Environment Variables

### Required Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=CWCR;..."
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "CWCR",
    "Audience": "CWCR-API",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "noreply@cwcr.com",
    "EnableSsl": true
  },
  "Frontend": {
    "BaseUrl": "https://your-frontend.com"
  },
  "FileStorage": {
    "BasePath": "wwwroot/uploads"
  }
}
```

### Azure Configuration (for production)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=CWCR;User ID=your-user;Password=your-password;Encrypt=True;"
  }
}
```

---

## 🚀 Deployment

### Deploy to Azure App Service

1. **Create Azure resources**

```bash
# Login to Azure
az login

# Create resource group
az group create --name CWCR-RG --location southeastasia

# Create App Service plan
az appservice plan create --name CWCR-Plan --resource-group CWCR-RG --sku B1

# Create Web App
az webapp create --name cwcr-api --resource-group CWCR-RG --plan CWCR-Plan --runtime "DOTNET|8.0"

# Create Azure SQL Database
az sql server create --name cwcr-sql-server --resource-group CWCR-RG --location southeastasia --admin-user sqladmin --admin-password YourPassword123!

az sql db create --name CWCR --server cwcr-sql-server --resource-group CWCR-RG --service-objective S0
```

2. **Publish from Visual Studio**

- Right-click on `API` project → **Publish**
- Select **Azure** → **Azure App Service (Windows/Linux)**
- Select your subscription and app service
- Click **Publish**

3. **Or publish via CLI**

```bash
# Build and publish
dotnet publish API -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip --resource-group CWCR-RG --name cwcr-api --src ./publish.zip
```

4. **Configure connection string on Azure**

```bash
az webapp config connection-string set \
  --name cwcr-api \
  --resource-group CWCR-RG \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:..."
```

5. **Run migrations on Azure**

```bash
dotnet ef database update -p Infrastructure -s API --connection "Server=tcp:..."
```

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

### Development Workflow

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Standards

- Follow C# naming conventions
- Write XML documentation for public APIs
- Add unit tests for new features
- Ensure all tests pass before submitting PR
- Keep commits atomic and well-described

### Branch Naming

- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Urgent production fixes
- `refactor/*` - Code refactoring
- `docs/*` - Documentation updates

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Team

- **Project Lead**: [Gia Kiet Ly](https://github.com/gia-kiet-ly)
- **Contributors**: [See all contributors](https://github.com/gia-kiet-ly/CWCR/graphs/contributors)

---

## 📞 Support

For questions or issues:

- 📧 Email: giakiet.ly1234@gmail.com
- 🐛 Issues: [GitHub Issues](https://github.com/gia-kiet-ly/CWCR/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/gia-kiet-ly/CWCR/discussions)

---

## 🙏 Acknowledgments

- Thanks to all contributors who have helped shape this project
- Inspired by sustainable waste management initiatives worldwide
- Built with ❤️ for a cleaner environment

---