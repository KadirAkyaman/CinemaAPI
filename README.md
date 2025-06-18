# CinemaAPI

CinemaAPI is a .NET Core Web API for managing a film archive. It utilizes PostgreSQL for data storage, Redis for user session management (JWT blacklisting), and is designed to run in Docker containers orchestrated by Docker Compose.

## Features

*   **Film Management:** CRUD operations for movies.
*   **Director Management:** CRUD operations for directors.
*   **User Management:** User registration and login.
*   **Authentication:** JWT (JSON Web Token) based authentication.
*   **Authorization:** Role-based access control for certain endpoints.
*   **Session Management:** JWT blacklisting on logout using Redis.
*   **Containerized:** Fully containerized setup using Docker and Docker Compose for easy deployment and development.

## Technologies Used

*   **.NET 9.0 **  - API Framework
*   **ASP.NET Core Web API** - For building RESTful APIs
*   **Entity Framework Core** - ORM for database interaction (Code-First approach)
*   **PostgreSQL** - Relational Database
*   **Redis** - In-memory data store (used for JWT blacklisting)
*   **Docker** - Containerization platform
*   **Docker Compose** - Tool for defining and running multi-container Docker applications
*   **JWT (JSON Web Tokens)** - For authentication
*   **BCrypt.Net** - For password hashing
*   **ILogger** - For logging

## Prerequisites

*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed.
*   [.NET SDK](https://dotnet.microsoft.com/download) (matching the version used in the project, e.g., .NET 9.0) - For understanding the codebase or contributing, not strictly necessary for just running with Docker.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### 1. Clone the Repository

```bash
git clone https://github.com/KadirAkyaman/CinemaAPI.git
cd CinemaAPI
