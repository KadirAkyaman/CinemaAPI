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
```

### 2. Configure Environment Variables

Create a `.env` file in the root directory of the project and add the following environment variables. These are used for database connection and JWT configuration.

```
POSTGRES_USER=your_postgres_user
POSTGRES_PASSWORD=your_postgres_password
POSTGRES_DB=your_database_name
JWT_KEY=YOUR_SUPER_SECRET_JWT_KEY_THAT_IS_LONG_AND_COMPLEX
JWT_ISSUER=http://localhost:8080
JWT_AUDIENCE=http://localhost:8080
```
**Note:** Replace `your_postgres_user`, `your_postgres_password`, `your_database_name`, and `YOUR_SUPER_SECRET_JWT_KEY_THAT_IS_LONG_AND_COMPLEX` with your desired values. The `JWT_KEY` should be a strong, random string.

### 3. Run with Docker Compose

Build and run the Docker containers using Docker Compose. This will set up the API, PostgreSQL database, and Redis cache.

```bash
docker-compose up --build
```
This command will:
*   Build the Docker images for your services.
*   Start the PostgreSQL database, Redis cache, and the API service.
*   Ensure the API service waits for the database to be healthy before starting.

To stop the services:
```bash
docker-compose down
```

### 4. Apply Database Migrations

Since the database is initialized within Docker, you need to apply Entity Framework Core migrations to create the necessary tables.

First, ensure you have the `dotnet-ef` tool installed globally:
```bash
dotnet tool install --global dotnet-ef
```
Then, from the project root directory (where `CinemaAPI.csproj` is located), run the following commands to create and apply migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
**Note:** If you make changes to your models, you will need to create a new migration (`dotnet ef migrations add [MigrationName]`) and then update the database (`dotnet ef database update`).

### 5. Access and Test the API

Once the services are up and running, the API will be accessible at `http://localhost:8080`. You can use tools like Postman or Insomnia to test the endpoints.

Here are some example endpoints:

*   **User Registration:**
    *   **URL:** `http://localhost:8080/api/auth/register`
    *   **Method:** `POST`
    *   **Body (JSON):**
        ```json
        {
            "username": "testuser",
            "email": "test@example.com",
            "password": "Password123!"
        }
        ```

*   **User Login:**
    *   **URL:** `http://localhost:8080/api/auth/login`
    *   **Method:** `POST`
    *   **Body (JSON):**
        ```json
        {
            "username": "testuser",
            "password": "Password123!"
        }
        ```

*   **Get All Movies (requires authentication):**
    *   **URL:** `http://localhost:8080/api/movies`
    *   **Method:** `GET`
    *   **Headers:** `Authorization: Bearer <your_jwt_token>` (Obtain token from login response)

### Troubleshooting

*   **`Failed to connect to db:5432` or `relation "Users" does not exist`:** This usually indicates that the database is not ready or migrations have not been applied. Ensure you have followed steps 3 and 4.
*   **Checking Container Logs:** To view logs for a specific service (e.g., `api`, `db`, `cache`), use:
    ```bash
    docker logs <container_name>
    ```
    For example: `docker logs filmarsivi_api`
*   **Rebuilding Containers:** If you make changes to your `Dockerfile` or `docker-compose.yml`, you might need to rebuild your containers:
    ```bash
    docker-compose down
    docker-compose up --build
    ```
