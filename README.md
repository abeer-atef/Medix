# Medix 🏥

**Medix** is a full-featured medical appointment and healthcare management platform built with ASP.NET Core MVC. It connects patients, doctors, and pharmacies in one system — handling everything from booking appointments to AI-assisted prescription reading.

> 🎓 This project was built as a graduation project for the **Digital Egypt Pioneers Initiative (DEPI) — Full Stack .NET Track**.

🔗 **Live Demo:** [medix-app-2026-ewaha0f0ayeggucf.japanwest-01.azurewebsites.net](https://medix-app-2026-ewaha0f0ayeggucf.japanwest-01.azurewebsites.net/)

> ⚠️ This is a graduation/academic project hosted on a free-tier plan. The live demo may be slow to respond on first load (cold start).

---

## ✨ Features

### For Patients
- Browse doctors by specialization and book appointments
- Real-time appointment notifications (via SignalR)
- Online payment for consultations (Stripe integration)
- Upload prescriptions and get **AI-powered prescription reading** (Groq Vision AI)
- View medical records and prescription history
- Rate and review doctors after appointments
- In-app chat with doctors

### For Doctors
- Manage availability and working hours
- View and manage patient appointments (confirm, cancel, mark as done)
- Access patient medical history and prescriptions
- View ratings and reviews from patients

### For Admins
- Manage doctors, staff, and specializations
- Monitor all appointments and financial activity across the platform
- User management (activate/deactivate accounts)

### Other
- Pharmacy directory with distance-based search
- Email notifications (appointment confirmations, password reset, etc.)
- Role-based authentication with ASP.NET Core Identity (Admin / Doctor / Patient)

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| Database | SQL Server (Azure SQL) with Entity Framework Core |
| Auth | ASP.NET Core Identity |
| Real-time | SignalR |
| Payments | Stripe |
| AI | Groq Vision API (prescription reading) |
| Email | MailKit (SMTP) |
| Hosting | Azure App Service |
| Frontend | Razor Views, Bootstrap, jQuery |

---

## 🚀 Getting Started (Local Setup)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Azure SQL)
- Visual Studio 2022 (recommended) or VS Code

### 1. Clone the repository
```bash
git clone https://github.com/abeer-atef/Medix.git
cd Medix
```

### 2. Configure secrets
This project keeps all sensitive values out of `appsettings.json`. Instead, use **User Secrets** for local development:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-connection-string>"
dotnet user-secrets set "OpenAiSettings:ApiKey" "<your-groq-api-key>"
dotnet user-secrets set "Stripe:PublishableKey" "<your-stripe-publishable-key>"
dotnet user-secrets set "Stripe:SecretKey" "<your-stripe-secret-key>"
dotnet user-secrets set "EmailSettings:Username" "<your-email>"
dotnet user-secrets set "EmailSettings:Password" "<your-email-app-password>"
```

### 3. Apply database migrations
Using .NET CLI:
```bash
dotnet ef database update
```

Or, if you're using Visual Studio, run this in the **Package Manager Console** instead:
```powershell
Update-Database
```

### 4. Run the project
Using .NET CLI:
```bash
dotnet run
```

Or, if you're using Visual Studio, just press **F5** (or the green "Run" button).

The seed data automatically creates:
- Default specializations (Cardiology, Neurology, Dentistry, etc.)

---

## 🔐 Security Note

No API keys, database credentials, or secrets are stored in this repository. All configuration values are injected via environment variables (in production) or User Secrets (in local development). If you fork this project, make sure to generate your own keys for Stripe, Groq, and email services.

---

## 📄 License

This project was built for academic purposes as a graduation project for the **Digital Egypt Builders Initiative (DEPI) — Full Stack .NET Track**.
