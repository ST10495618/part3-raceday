POE PART 3


## About the Project
final part of the project
RaceDay is a web-based race management platform developed using ASP.NET Core 8. The system allows participants to discover running events, register for races and view their race information. Organisers can manage events, monitor registrations and manage race results.

The project demonstrates the use of web APIs, MVC architecture, database integration, authentication, cloud storage and containerisation
## Main Features
### Participant Features
- Register and log in securely.
- Browse available running events.
- View event details and banner images.
- Register for running events.
- Manage personal profile information.
- View race results.

### Organiser Features
- Access an organiser dashboard.
- Create, update and delete events.
- Upload event banner images.
- View participant registrations.
- Manage race results.
- Manage event information.

### Security Features
- JWT-based authentication.
- Role-based authorisation.
- Separate Participant and Organiser roles.
- Protected API endpoints.
- Secure configuration using Visual Studio User Secrets.


## Technologies Used
| Technology | Purpose |
|---|---|
| C# | Main programming language |
| ASP.NET Core 8 Web API | Backend services |
| ASP.NET Core MVC | Frontend application |
| Entity Framework Core | Database operations |
| SQL Server | Database management |
| JWT | Authentication |
| Swagger | API documentation and testing |
| Azure Blob Storage | Event banner image storage |
| Docker | Application containerisation |
| HTML, CSS and Bootstrap | User interface |

## Project Structure

The solution contains two main applications:

**1. RaceDay API**
- Handles application data and business logic.
- Manages authentication and authorisation.
- Provides event, registration, user and results endpoints.
- Connects to SQL Server.
- Integrates with Azure Blob Storage.

**2. RaceDay MVC**
- Provides the user interface.
- Communicates with the API using HTTP requests.
- Displays participant and organiser dashboards.
- Allows users to interact with the platform.

## Database

RaceDay uses SQL Server with Entity Framework Core.

Main database tables include:
- Users
- Roles
- UserRole
- Category
- Event
- Registration
- Results
- Route
- Weather

## Getting Started

### Requirements
- Visual Studio 2022
- .NET 8 SDK
- SQL Server
- SQL Server Management Studio (SSMS)
- Docker Desktop (for containerisation)

### Configuration

Before running the project, configure the required settings.

The API requires:
- SQL Server connection string: `RaceDayConnection`
- JWT signing key, issuer and audience.
- Azure Blob Storage connection string.
- Azure Blob Storage container name.

Store sensitive values in Visual Studio User Secrets or environment variables. Do not commit passwords, access keys or other secrets to GitHub.

### Running the API

1. Open the RaceDay API project in Visual Studio.
2. Configure the database connection string and other required settings.
3. Ensure SQL Server is running and the database is available.
4. Run the API project.
5. Open Swagger to explore and test the endpoints.

Local development Swagger address:

`https://localhost:7246/swagger/index.html`

### Running the MVC Application

1. Open the RaceDayMVC project.
2. Configure its API base URL to point to the running API.
3. Ensure the API is running.
4. Run the MVC project.

Local development address:

`https://localhost:7160/`

### Docker

Both applications have Dockerfiles for containerisation.

Example image build commands:

```powershell
docker build -t racedayapi .
```

```powershell
docker build -t racedaymvc .
```

The containers also require the correct environment configuration, including database connectivity and application secrets.

## Learning Outcomes

This project demonstrates practical skills in:

- Developing RESTful Web APIs.
- Building ASP.NET Core MVC applications.
- Implementing JWT authentication.
- Applying role-based access control.
- Designing and connecting relational databases.
- Uploading and retrieving images using Azure Blob Storage.
- Testing APIs with Swagger.
- Building Docker images and running containers.

## Future Improvements

- Add email notifications for race registrations.
- Improve race result reporting.
- Add additional event analytics.
- Improve automated testing.
- Deploy the application to a cloud hosting environment.

## Developer

**Developed as a Software Development academic project.**

**Project Name:** RaceDay  
**Framework:** ASP.NET Core 8  
**Language:** C#

---

*RaceDay – Run. Register. Track.*
