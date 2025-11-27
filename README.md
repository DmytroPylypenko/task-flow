TaskFlow
========

TaskFlow is a full-stack project management application designed to replicate core Kanban functionality. This application enables users to organize projects through boards, columns, and tasks, featuring a modern, responsive user interface and a secure, scalable backend architecture.

Project Overview
----------------

This project was developed to demonstrate proficiency in building modern web applications using the .NET ecosystem and Angular. It focuses on clean architecture, type safety, security best practices, and efficient state management.

Project Status
----------------

TaskFlow is an active, ongoing project.
I am continuously improving its functionality, architecture, and DevOps pipeline to make it production-ready.
Upcoming work includes adding unit tests for the backend, creating final Dockerfiles for both applications, setting up CI/CD pipelines with GitHub Actions to automatically build and test the project, and deploying the live application to Azure.

Screenshots
----------------

Below are preview screenshots of the current UI.

### Login Page

<img width="1920" height="924" alt="Login" src="https://github.com/user-attachments/assets/635b5876-2b2b-4dd4-9358-b5ed9c6b471a" />

### Registration Page

<img width="1920" height="924" alt="Registration" src="https://github.com/user-attachments/assets/657ce196-c3a2-436a-a182-4cfc5ccb7f26" />

### Board List Page

<img width="1920" height="924" alt="Board List" src="https://github.com/user-attachments/assets/f4f91d8d-8847-4886-933c-6397f79dc6ff" />

### Board Detail Page

<img width="1920" height="924" alt="Board Detail" src="https://github.com/user-attachments/assets/317bfc5d-2b4d-431e-91ad-e458bbca0609" />

### Task Detail Modal Window

<img width="1920" height="924" alt=" Task Detail" src="https://github.com/user-attachments/assets/4c05a085-e3c7-4e00-8e50-8869d83ec616" />

### Key Features

*   **Authentication & Security:**
    
    *   Custom JWT (JSON Web Token) authentication flow.
        
    *   Secure password hashing using PBKDF2 (via `Rfc2898DeriveBytes`).
        
    *   Protected API endpoints using Authorization middleware.
        
*   **Kanban Board Management:**
    
    *   Create, read, update, and delete (CRUD) operations for Boards, Columns, and Tasks.
        
    *   **Drag-and-Drop Interface:** Interactive task movement between columns and reordering within columns using the Angular CDK.
        
    *   Optimistic UI updates for a responsive user experience.
        
*   **User Interface:**
    
    *   Professional Dark Mode design implemented with Tailwind CSS.
        
    *   Responsive grid layouts and horizontal scrolling views.
        
    *   Modal-based interactions for data entry and editing.
        
*   **Data Integrity:**
    
    *   Server-side validation using Data Annotations.
        
    *   Entity Framework Core relationships.
        

Technology Stack
----------------

### Backend

| **Technology**            | **Usage**                                                   |
| ------------------------- | ----------------------------------------------------------- |
| **.NET 8**                | Core framework for the RESTful Web API.                     |
| **ASP.NET Core**          | Web API framework for handling HTTP requests.               |
| **Entity Framework Core** | ORM for database interactions.                              |
| **SQL Server**            | Relational database management system (running via Docker). |
| **xUnit**                 | Unit testing framework.                                     |
| **Moq & AutoFixture**     | Mocking and test data generation.                           |
| **FluentAssertions**      | Fluent syntax for test assertions.                          |

### Frontend

| **Technology**     | **Usage**                                                   |
| ------------------ | ----------------------------------------------------------- |
| **Angular (v20)** | Component-based frontend framework (Standalone Components). |
| **TypeScript**     | Strongly-typed programming language.                        |
| **Tailwind CSS**   | Utility-first CSS framework for styling and theming.        |
| **Angular CDK**    | Component Dev Kit for drag-and-drop implementation.         |
| **Reactive Forms** | Handling form inputs and validation.                        |
| **Lucide Angular** | Iconography library.                                        |


Architecture
------------

The application follows a layered architecture to ensure separation of concerns:

1.  **API Layer (Controllers):** Handles HTTP requests, input validation, and response formatting.
    
2.  **Service Layer / Repository Layer:**
    
    *   **Repositories:** Encapsulate data access logic (`UserRepository`, `BoardRepository`, etc.).
        
    *   **Services:** Encapsulate business logic (e.g., `TokenService`, `PasswordHasher`).
        
3.  **Data Layer:** Manages database context and migrations via Entity Framework Core.
    

Getting Started
---------------

Follow these instructions to set up the project locally.

### Prerequisites

*   .NET SDK 8.0
    
*   Node.js (LTS version)
    
*   Docker Desktop (for the database container)
    

### Database Setup

1.  Ensure Docker Desktop is running.
    
2.  Start the SQL Server container:
    
    Bash
    
        docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sql-server-dev -d mcr.microsoft.com/mssql/server:2022-latest
        
    
3.  Configure User Secrets for the API project (to avoid hardcoding credentials):
    
    Bash
    
        cd TaskFlow.Api
        dotnet user-secrets init
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TaskFlowDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
        dotnet user-secrets set "JwtSettings:Key" "Your_Super_Secret_Long_Random_Key_Here"
        
    
4.  Apply database migrations:
    
    Bash
    
        dotnet ef database update
        
    

### Running the Application

**1\. Start the Backend API:**

Bash

    cd TaskFlow.Api
    dotnet run
    

The API will be available at `https://localhost:7264` (or the port specified in your launch settings).

**2\. Start the Angular Frontend:**

Bash

    cd TaskFlow.Web
    npm install
    ng serve
    

The application will be available at `http://localhost:4200`.

Testing
-------

The solution includes a dedicated test project (`TaskFlow.Api.Tests`) covering unit tests for Services and Repositories using an In-Memory database.

To execute the test suite:

Bash

    dotnet test
    

License
-------

This project is licensed under the MIT License.
