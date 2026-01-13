# My Secrets - Secure Password Manager

A production-grade password manager application built with React (TypeScript) and ASP.NET Core Web API, featuring AES-256-GCM encryption, JWT authentication with refresh tokens, and Google OAuth integration.

![My Secrets](https://img.shields.io/badge/version-1.0.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![React](https://img.shields.io/badge/React-18.2-blue.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.3-blue.svg)

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Features](#features)
- [Security Design](#security-design)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Design Patterns](#design-patterns)
- [Database Schema](#database-schema)

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         FRONTEND (React + TS)                        │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌──────────────────────┐   │
│  │  Auth   │  │ Secrets │  │ Export  │  │   Google OAuth       │   │
│  └─────────┘  └─────────┘  └─────────┘  └──────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                │ HTTPS + JWT Bearer Token
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      API LAYER (Controllers)                         │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────────┐    │
│  │ AuthController │  │SecretsController│ │ ExportController   │    │
│  └────────────────┘  └────────────────┘  └────────────────────┘    │
│                                                                      │
│  Middleware: ExceptionHandling │ RequestLogging │ RateLimiting      │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                │ MediatR (CQRS Pattern)
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER (Use Cases)                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐     │
│  │    Commands     │  │     Queries     │  │   Validators    │     │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER (Core Logic)                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐     │
│  │    Entities     │  │   Interfaces    │  │ Domain Services │     │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  INFRASTRUCTURE LAYER (External)                     │
│  ┌───────────────┐  ┌───────────────┐  ┌────────────────────┐      │
│  │  SQL Server   │  │   MongoDB     │  │  External APIs     │      │
│  │   (Primary)   │  │   (Audit)     │  │  (Google OAuth)    │      │
│  └───────────────┘  └───────────────┘  └────────────────────┘      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## ✨ Features

### Authentication
- ✅ JWT-based authentication with 15-minute access tokens
- ✅ Refresh token rotation with 7-day expiry
- ✅ Secure HttpOnly cookie storage for refresh tokens
- ✅ Google OAuth 2.0 integration
- ✅ Password hashing with BCrypt

### Secrets Management
- ✅ Create, read, update, delete secrets
- ✅ AES-256-GCM encryption for passwords
- ✅ Per-user key derivation using HKDF
- ✅ Explicit password decryption (user action required)
- ✅ Favorite secrets
- ✅ Category organization

### Data Operations
- ✅ Server-side pagination
- ✅ Multi-field sorting
- ✅ Search and filtering
- ✅ Export to CSV/TXT (passwords remain encrypted)

### Security
- ✅ Rate limiting (100 req/min general, 5 req/min auth)
- ✅ CORS configuration
- ✅ XSS protection (HttpOnly cookies, CSP headers)
- ✅ CSRF protection (SameSite cookies)
- ✅ SQL injection protection (parameterized queries)
- ✅ Audit logging to MongoDB

---

## 🔐 Security Design

### Password Encryption

**Algorithm: AES-256-GCM (Galois/Counter Mode)**

| Property | Value |
|----------|-------|
| Key Size | 256 bits |
| IV Size | 96 bits (unique per encryption) |
| Tag Size | 128 bits |
| Mode | Authenticated Encryption |

**Why AES-256-GCM?**
- Industry standard (NIST approved)
- Authenticated encryption (integrity + confidentiality)
- Hardware-accelerated on modern CPUs
- Quantum-resistant for foreseeable future

### Key Management

```
Master Key (env variable)
        │
        ▼
   HKDF Derivation
        │
        ▼
User-Specific Key = HKDF(MasterKey, UserId, "my-secrets-user-key")
        │
        ▼
Per-Secret Encryption with unique IV
```

**Storage Format:** `Base64(IV[12 bytes] || Ciphertext || AuthTag[16 bytes])`

### Token Security

| Token Type | Storage | Lifetime | Security |
|------------|---------|----------|----------|
| Access Token | Memory only | 15 minutes | Short-lived, no persistent storage |
| Refresh Token | HttpOnly cookie | 7 days | Hashed in DB, rotation on use |

---

## 🛠️ Technology Stack

### Backend
- **Framework:** ASP.NET Core 9.0
- **ORM:** Entity Framework Core 9.0
- **CQRS:** MediatR 12.2
- **Validation:** FluentValidation 11.9
- **Auth:** JWT Bearer, Google.Apis.Auth

### Frontend
- **Framework:** React 18.2
- **Language:** TypeScript 5.3
- **Styling:** Tailwind CSS 3.4
- **Forms:** React Hook Form 7.49
- **Routing:** React Router 6.22
- **HTTP Client:** Axios 1.6

### Databases
- **Primary:** SQL Server 2022 (users, secrets, tokens)
- **Secondary:** MongoDB 7.0 (audit logs, export history)

### DevOps
- **Containerization:** Docker Compose
- **API Docs:** Swagger/OpenAPI

---

## 📦 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## 🚀 Quick Start

### 1. Clone and Setup

```bash
# Clone the repository
cd my-secrets

# Start databases
docker-compose up -d

# Wait for SQL Server to be ready (about 30 seconds)
```

### 2. Configure Environment

**Backend** (`backend/src/MySecrets.API/appsettings.Development.json`):
```json
{
  "EncryptionSettings": {
    "MasterKey": "your-64-char-hex-key-here"
  },
  "JwtSettings": {
    "SecretKey": "your-32-char-jwt-secret-here"
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id"
  }
}
```

**Frontend** (create `frontend/.env`):
```env
VITE_API_URL=https://localhost:7001/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

### 3. Generate Encryption Key

```bash
# PowerShell
-join ((48..57) + (97..102) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Or use any hex string generator for 64 characters (32 bytes)
```

### 4. Run Backend

```bash
cd backend
dotnet restore
dotnet ef migrations add InitialCreate --project src/MySecrets.Infrastructure --startup-project src/MySecrets.API
dotnet run --project src/MySecrets.API
```

Backend will start at: `https://localhost:7001`
Swagger UI: `https://localhost:7001/swagger`

### 5. Run Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend will start at: `http://localhost:5173`

---

## ⚙️ Configuration

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ENCRYPTION_MASTER_KEY` | 64-char hex string for AES-256 | Yes |
| `JWT_SECRET_KEY` | 32+ char string for JWT signing | Yes |
| `ConnectionStrings__DefaultConnection` | SQL Server connection | Yes |
| `MongoDB__ConnectionString` | MongoDB connection | Yes |
| `GoogleAuth__ClientId` | Google OAuth client ID | For Google login |

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MySecretsDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MySecretsAudit"
  }
}
```

---

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login with email/password |
| POST | `/api/auth/google` | Login with Google token |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Revoke refresh token |

### Secrets Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/secrets` | List secrets (paginated) |
| GET | `/api/secrets/{id}` | Get secret by ID |
| GET | `/api/secrets/{id}/decrypt` | Decrypt password |
| POST | `/api/secrets` | Create secret |
| PUT | `/api/secrets/{id}` | Update secret |
| DELETE | `/api/secrets/{id}` | Delete secret |

### Export Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/export/csv` | Export as CSV |
| GET | `/api/export/txt` | Export as TXT |

### Query Parameters (GET /api/secrets)

| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number (default: 1) |
| pageSize | int | Items per page (default: 10, max: 50) |
| searchTerm | string | Search in URL, username, notes |
| category | string | Filter by category |
| isFavorite | bool | Filter favorites only |
| sortBy | string | Sort field (createdAt, websiteUrl, etc.) |
| sortDescending | bool | Sort direction |

### Example API Calls

```bash
# Register
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"SecureP@ss1","firstName":"John","lastName":"Doe"}'

# Login
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"SecureP@ss1"}'

# Create Secret (with token)
curl -X POST https://localhost:7001/api/secrets \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"websiteUrl":"https://github.com","username":"myuser","password":"mypassword"}'

# Decrypt Password
curl -X GET https://localhost:7001/api/secrets/{id}/decrypt \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## 📁 Project Structure

```
my-secrets/
├── backend/
│   ├── MySecrets.sln
│   └── src/
│       ├── MySecrets.Domain/           # Entities, Interfaces (no dependencies)
│       ├── MySecrets.Application/      # CQRS Commands/Queries, DTOs, Validators
│       ├── MySecrets.Infrastructure/   # EF Core, MongoDB, Services
│       └── MySecrets.API/              # Controllers, Middleware, Config
├── frontend/
│   ├── src/
│   │   ├── api/                        # Axios instances, API clients
│   │   ├── auth/                       # Auth context, hooks, components
│   │   ├── components/                 # Reusable UI components
│   │   ├── pages/                      # Route pages
│   │   ├── types/                      # TypeScript interfaces
│   │   └── styles/                     # Global CSS
│   ├── package.json
│   └── vite.config.ts
├── docker-compose.yml
└── README.md
```

---

## 🎨 Design Patterns Used

| Pattern | Usage | Justification |
|---------|-------|---------------|
| **Clean Architecture** | Layer separation | Testable, maintainable, framework-agnostic core |
| **CQRS** | Command/Query separation | Independent scaling, clear responsibilities |
| **Repository** | Data access abstraction | Testable, swappable data sources |
| **Unit of Work** | Transaction management | Atomic operations across repositories |
| **Mediator** | Request handling | Loose coupling, pipeline behaviors |
| **Manual Mapping** | DTO conversion | Security: explicit control over exposed data |

### Why NOT AutoMapper?

For a security-sensitive application like a password manager, explicit manual mapping provides:
- Full control over which fields are exposed
- No accidental exposure of sensitive data
- Easier auditing of data flow
- Better debugging and traceability

---

## 🗄️ Database Schema

### SQL Server (Primary)

```sql
-- Users Table
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IsGoogleUser BIT NOT NULL,
    GoogleId NVARCHAR(256) UNIQUE,
    IsActive BIT NOT NULL,
    LastLoginAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2
);

-- Secrets Table
CREATE TABLE Secrets (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
    WebsiteUrl NVARCHAR(2048) NOT NULL,
    Username NVARCHAR(256) NOT NULL,
    EncryptedPassword NVARCHAR(4096) NOT NULL,
    Notes NVARCHAR(4000),
    Category NVARCHAR(100),
    IsFavorite BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2
);

-- RefreshTokens Table
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
    TokenHash NVARCHAR(256) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL,
    RevokedAt DATETIME2,
    RevokedReason NVARCHAR(256),
    ReplacedByTokenHash NVARCHAR(256),
    CreatedByIp NVARCHAR(45),
    RevokedByIp NVARCHAR(45),
    CreatedAt DATETIME2 NOT NULL
);
```

### MongoDB (Audit Logs)

```javascript
// Collections: audit_logs, secret_access_logs, auth_event_logs, export_logs

// Example: secret_access_logs
{
    "_id": ObjectId("..."),
    "userId": UUID("..."),
    "secretId": UUID("..."),
    "action": "PasswordDecrypted",
    "details": "Secret abc123 - PasswordDecrypted",
    "ipAddress": "192.168.1.1",
    "timestamp": ISODate("2024-01-15T10:30:00Z"),
    "category": "SecretAccess"
}
```

---

## 🔒 Security Considerations

### What's Secure
- ✅ Passwords encrypted with AES-256-GCM
- ✅ Per-user encryption keys via HKDF
- ✅ Refresh tokens hashed before storage
- ✅ HttpOnly, Secure, SameSite cookies
- ✅ Rate limiting on auth endpoints
- ✅ SQL injection protection via EF Core
- ✅ Audit logging for sensitive operations

### Production Recommendations
1. Use Azure Key Vault / AWS Secrets Manager for master key
2. Enable HTTPS everywhere
3. Use a proper certificate (not self-signed)
4. Configure CSP headers
5. Set up monitoring and alerting
6. Regular security audits
7. Implement account lockout after failed attempts

---

## 📄 License

MIT License - see LICENSE file for details.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

---

Built with ❤️ using Clean Architecture principles.
