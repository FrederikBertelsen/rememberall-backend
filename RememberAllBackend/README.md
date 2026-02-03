# RememberAll Backend

A collaborative todo list API built with .NET 8, featuring clean architecture, comprehensive list sharing, and robust authentication. Designed to showcase modern C# backend development practices with Entity Framework Core and a well-structured domain layer.

## Key Features

- **User Authentication**: Cookie-based auth with secure password hashing
- **Todo List Management**: Full CRUD operations for personal and shared lists  
- **Collaborative Sharing**: Invite system with granular access control
- **Clean Architecture**: Repository pattern, dependency injection, and separation of concerns
- **Global Exception Handling**: Centralized error management with custom exceptions
- **Entity Relationships**: Comprehensive data modeling with EF Core navigation properties

## Technology Stack

- **.NET 8** - Latest LTS framework
- **Entity Framework Core** - ORM with SQLite database
- **ASP.NET Core Identity** - Password hashing and authentication
- **Swagger/OpenAPI** - Interactive API documentation
- **Cookie Authentication** - Secure, stateful sessions
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Built-in IoC container

## Project Structure

```
src/
├── Controllers/     # API endpoints and request handling
├── Services/        # Business logic layer
├── Repositories/    # Data access layer with interfaces
├── Entities/        # Domain models and relationships
├── DTOs/           # Data transfer objects for API contracts
├── Middleware/     # Global exception handling
├── Extensions/     # Validation and mapping utilities
└── Utilities/      # Email and password validation
```

## Quick Start

1. **Clone and navigate to the project**
   ```bash
   git clone <repository-url>
   cd RememberAllBackend
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Access Swagger UI**
   - Navigate to `http://localhost:5000` for interactive API documentation

The SQLite database will be automatically created on first run.

## API Endpoints

### Authentication
| Method   | Endpoint                          | Description         | Body            |
| -------- | --------------------------------- | ------------------- | --------------- |
| `POST`   | `/api/auth/register`              | Register new user   | `CreateUserDto` |
| `POST`   | `/api/auth/login`                 | Login user          | `LoginDto`      |
| `POST`   | `/api/auth/logout`                | Logout user         | -               |
| `GET`    | `/api/auth/me`                    | Get current user    | -               |
| `GET`    | `/api/auth/password-requirements` | Get password rules  | -               |
| `DELETE` | `/api/auth/delete-account`        | Delete user account | -               |

### Todo Lists
| Method   | Endpoint              | Description      | Body                |
| -------- | --------------------- | ---------------- | ------------------- |
| `POST`   | `/api/lists`          | Create new list  | `CreateTodoListDto` |
| `GET`    | `/api/lists`          | Get user's lists | -                   |
| `GET`    | `/api/lists/{listId}` | Get list by ID   | -                   |
| `PATCH`  | `/api/lists`          | Update list      | `UpdateTodoListDto` |
| `DELETE` | `/api/lists/{listId}` | Delete list      | -                   |

### Todo Items
| Method   | Endpoint                           | Description        | Body                |
| -------- | ---------------------------------- | ------------------ | ------------------- |
| `POST`   | `/api/todoitems`                   | Create new item    | `CreateTodoItemDto` |
| `GET`    | `/api/todoitems/bylist/{listId}`   | Get items by list  | -                   |
| `PATCH`  | `/api/todoitems`                   | Update item        | `UpdateTodoItemDto` |
| `PATCH`  | `/api/todoitems/{itemId}/complete` | Mark item complete | -                   |
| `DELETE` | `/api/todoitems/{itemId}`          | Delete item        | -                   |

### Invitations
| Method   | Endpoint                         | Description           | Body              |
| -------- | -------------------------------- | --------------------- | ----------------- |
| `POST`   | `/api/invites`                   | Send invite           | `CreateInviteDto` |
| `GET`    | `/api/invites/sent`              | Get sent invites      | -                 |
| `GET`    | `/api/invites/received`          | Get received invites  | -                 |
| `PATCH`  | `/api/invites/{inviteId}/accept` | Accept invite         | -                 |
| `DELETE` | `/api/invites/{inviteId}`        | Delete/decline invite | -                 |

### List Access
| Method   | Endpoint                     | Description            | Query            |
| -------- | ---------------------------- | ---------------------- | ---------------- |
| `GET`    | `/api/listaccess`            | Get access permissions | `?listId={guid}` |
| `DELETE` | `/api/listaccess/{accessId}` | Remove access          | -                |

## Architecture Highlights

**Repository Pattern**: Clean separation between data access and business logic with comprehensive interfaces.

**Domain-Driven Design**: Rich entity models with proper relationships and encapsulation using EF Core navigation properties.

**Exception Handling**: Custom exception types with global middleware for consistent error responses.

**Security**: Secure cookie authentication with proper CORS configuration and authorization policies.

**Validation**: Input validation through DTOs with extension methods for clean, reusable validation logic.

## Development

The API uses Entity Framework Code First with automatic database creation. All endpoints require authentication except registration and login. Swagger UI provides comprehensive API testing capabilities during development.