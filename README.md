# BeredLink – Kriseberedskap, ressurs- og behovsportal

BeredLink er en digital plattform for kriseberedskap, ressursoversikt og behovsmelding.

Applikasjonen er utviklet med ASP.NET Core MVC (.NET 10), MariaDB og Docker Compose.

## Teknologi

- ASP.NET Core MVC (.NET 10)
- MariaDB 11.4
- Entity Framework Core
- Docker og Docker Compose
- xUnit for testing

## Komme i gang

### Forutsetninger

- Docker Desktop
- Git

### Første oppstart

1. Last ned prosjektet fra GitHub.
2. Åpne prosjektmappen i terminalen.
3. Kontroller at Docker Desktop kjører.
4. Start applikasjonen med: docker compose up --build.


Docker bygger applikasjonen, starter databasen og starter webapplikasjonen.

### Åpne applikasjonen

Når containerne kjører, åpne:

http://localhost:8080

### Stoppe applikasjonen

Trykk `Ctrl + C` i terminalen, og kjør:

```bash
docker compose down
```

## Testing

Testprosjektet ligger i mappen `FeedbackMap.Tests`.

Testene kan kjøres fra terminalen med:

```bash
dotnet test FeedbackMap.Tests/FeedbackMap.Tests.csproj
```

## Prosjektstruktur

- `Controllers/` – Applikasjonens kontrollere
- `Data/` – Database og datatilgang
- `Migrations/` – Databasemigreringer
- `Models/` – Datamodeller
- `Services/` – Applikasjonens tjenester
- `Views/` – MVC-visninger
- `wwwroot/` – CSS, JavaScript og bilder
- `FeedbackMap.Tests/` – Automatiserte tester

## Docker

Prosjektet bruker Docker Compose til å starte webapplikasjonen og MariaDB.

Database og opplastede filer lagres i Docker-volumer, slik at data kan bevares mellom containeroppstarter.
