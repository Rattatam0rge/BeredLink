# BeredLink – System Architecture

## 1. Introduction

BeredLink is a web-based platform for emergency preparedness, resource management and coordination of needs and available resources.

The system allows users to register, manage resources, report needs and use map functionality to support coordination between different actors.

This document describes the system architecture, main components, technologies and data flow within the application.

## 2. Architecture Overview

BeredLink is built using ASP.NET Core MVC and follows a layered application structure.

The main architectural components are:

- **Presentation layer:** Handles the user interface and interaction with users.
- **Application layer:** Handles application logic and processes user requests.
- **Data layer:** Handles database access and persistence.
- **Infrastructure:** Provides the database and application hosting through Docker.

The application and database run as separate Docker services.

## 3. Technologies

The system uses the following technologies:

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| .NET 10 | Application framework |
| ASP.NET Core MVC | Web application framework |
| Entity Framework Core | Database access and ORM |
| MariaDB | Relational database |
| Docker | Containerization |
| Docker Compose | Container orchestration |
| HTML, CSS and JavaScript | Frontend technologies |
| xUnit | Unit testing framework |

## 4. Application Structure

The application is organized into several folders, each with a specific responsibility.

### Controllers

Controllers receive HTTP requests, coordinate application operations and return appropriate views or responses.

### Models

Contains the application's domain models and entities. These represent the data structures used throughout the system.

### Views

Contains the Razor views responsible for presenting information to users.

### ViewModels

Contains models designed to transfer and present data between controllers and views.

### Services

Contains application logic and business operations. Services handle operations related to resources, needs, assignments, notifications, auditing and image storage.

### Data

Contains the database context and database-related configuration.

### Migrations

Contains Entity Framework Core migrations used to manage database schema changes.

### ViewComponents

Contains reusable components used in the presentation layer.

### wwwroot

Contains static frontend resources such as CSS, JavaScript and images.

## 5. Database Architecture

BeredLink uses MariaDB as its relational database.

Entity Framework Core is used as the Object-Relational Mapper (ORM), allowing the application to communicate with the database through C# entities and the database context.

Database migrations are used to manage changes to the database structure.

The application connects to the database through the database service defined in Docker Compose.

## 6. Application Data Flow

A typical interaction follows this process:

1. A user performs an action through the web interface.
2. The request is received by an MVC controller.
3. The controller calls the relevant application service.
4. The service performs the required business logic.
5. Entity Framework Core communicates with MariaDB when database operations are required.
6. The result is returned through the controller.
7. The user receives an updated view or response.

This structure separates presentation, business logic and database operations.

## 7. Docker Deployment Architecture

BeredLink uses Docker Compose to run the application and database as separate services.

The deployment consists of:

- **Web service:** Runs the ASP.NET Core application.
- **Database service:** Runs MariaDB.
- **Persistent volumes:** Used to retain database data and uploaded files.

The services communicate through Docker's internal network.

The web application is exposed on port 8080 by default.

To build and start the application:

```powershell
docker compose up --build
```

To stop the application:

```powershell
docker compose down
```

## 8. Security Architecture

Security measures are implemented as part of the application configuration and request handling.

These include:

- ASP.NET Core Identity for user identity management.
- Authentication and authorization.
- Anti-forgery protection (CSRF).
- Rate limiting.
- Security headers and Content Security Policy (CSP).
- Validation of uploaded images.
- Audit logging of important operations.

These measures help protect user accounts, application functionality and stored information.

## 9. Testing Architecture

The project contains a separate test project named `FeedbackMap.Tests`.

The test project uses xUnit and Entity Framework Core InMemory to test application services and selected functionality.

The tests cover areas including:

- Resource management.
- Need management.
- Resource assignments.
- Notifications and audit logging.
- Image storage.
- Geographic calculations and resource categories.

The test project is maintained separately from the main application.

Test execution and results are documented separately.

## 10. Summary

BeredLink uses a layered ASP.NET Core MVC architecture with MariaDB as its database and Docker for deployment.

The separation of controllers, services, models and data access helps organize the application and maintain a clear distinction between user interaction, business logic and data persistence.

Docker Compose provides a consistent way to run the application and database together.
