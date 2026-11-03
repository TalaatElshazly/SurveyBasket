# 📊 SurveyBasket API

**SurveyBasket** is a RESTful Web API built with **ASP.NET Core** designed to manage online surveys and polls. It allows administrators to create polls, manage questions and answers, and analyze voting results, while providing a secure and seamless voting experience for users.

> 🚧 **Project Status:** Work in Progress (Active Development)

## ✨ Core Features (Planned & Implemented)
* **Poll & Question Management:** Full CRUD operations for polls, questions, and multiple-choice answers.
* **Voting System:** Secure endpoints for users to submit votes with validation to prevent duplicate submissions.
* **Analytics & Results:** Real-time aggregation of votes per poll and daily voting statistics.
* **Authentication & Authorization:** Stateless JWT authentication, Refresh Token rotation, and Role/Permission-based access control.

## 🛠️ Tech Stack & Packages
* **Framework:** C# / ASP.NET Core Web API
* **Database & ORM:** SQL Server & Entity Framework Core
* **Object Mapping:** Mapster
* **Validation:** FluentValidation
* **Error Handling:** Result Pattern & Global Exception Handling (`ProblemDetails`)
* **Logging & Monitoring:** Serilog

## 🚀 Getting Started
1. Clone the repository:
   ```bash
   git clone [https://github.com/TalaatElshazly/SurveyBasket.git](https://github.com/TalaatElshazly/SurveyBasket.git)

Update the database connection string in appsettings.Development.json.

Apply EF Core migrations:
dotnet ef database update
