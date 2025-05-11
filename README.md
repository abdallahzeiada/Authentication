# MediCare API

MediCare API is a robust backend solution for managing user authentication, appointments, and role-based access control for a healthcare platform. Built with **.NET 9** and **C# 13.0**, it leverages modern technologies like **Entity Framework Core**, **JWT Authentication**, and **Swagger** for API documentation.

## Features

- **User Authentication**: Secure registration and login using hashed passwords and JWT tokens.
- **Role-Based Access Control**: Separate roles for doctors and patients with tailored functionalities.
- **Appointment Management**: Patients can book appointments, and doctors can manage their schedules.
- **CORS Support**: Configured for frontend integration with React or other frameworks.
- **Swagger Integration**: Interactive API documentation for testing and exploration.

## Technologies Used

- **.NET 9**
- **Entity Framework Core** with SQL Server
- **JWT Authentication**
- **Swagger (Swashbuckle)**
- **ASP.NET Core Web API**

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server
- A tool like Postman or a frontend application for testing the API.

### Installation

1. Restore dependencies.
   ```bash
   dotnet restore
   dotnet build
   ```
2. Update the connection string in `appsettings.json`.
   ```bash
   dotnet ef migrations add InitialCreate
   ```
3. Apply migrations to set up the database.
   ```bash
   dotnet ef database update
   ```
4. Run the application.
   ```bash
   dotnet run
   ```
### API Endpoints

#### Authentication
- `POST /api/auth/register`: Register a new user.
- `POST /api/auth/login`: Login and receive a JWT token.

#### Appointments
- `GET /api/appointments`: Get all appointments for the logged-in user.
- `POST /api/appointments`: Create a new appointment (patients only).
- `PUT /api/appointments/{id}/status`: Update the status of an appointment.

### Swagger Documentation

Explore and test the API endpoints interactively.

## Project Structure

- **Controllers**: API endpoints for authentication and appointments.
- **Models**: Database entities like `User`, `Doctor`, `Patient`, and `Appointment`.
- **DTOs**: Data Transfer Objects for request and response payloads.
- **Migrations**: Database schema migrations.
- **Program.cs**: Application entry point and service configuration.


