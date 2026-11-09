# School Admission System

A full‑stack take‑home assignment for school admissions.

## Quick start (Docker only)

Prerequisites: Docker with the Compose plugin.

```bash
cp .env.example .env
docker compose up --build
```

Wait for the backend to finish migrations, then open:

| Service | URL |
| ------- | --- |
| Frontend UI | http://localhost:5173 |
| Backend API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| PostgreSQL | localhost:5433 |

To stop: `docker compose down`.

## Test instructions

You need the .NET 10 SDK installed to run the tests. The test project uses Testcontainers with a real PostgreSQL instance, so Docker must be running.

```bash
dotnet test
```

## API

Base URL: `http://localhost:8080`. All requests/responses are JSON.

The student’s identity is sent in the `X-Student-Id` header (17‑digit birth registration number).

| Method | Path | Description |
| ------ | ---- | ----------- |
| `POST` | `/classes` | Create a class/shift |
| `GET` | `/classes` | List classes with seat counts |
| `GET` | `/classes/{classId}` | Get one class with seat counts |
| `POST` | `/classes/{classId}/holds` | Apply for a seat (requires `X-Student-Id`) |
| `GET` | `/holds/{holdId}` | Get current application status |
| `POST` | `/holds/{holdId}/confirm` | Pay fee / confirm admission (requires `X-Student-Id`) |
| `DELETE` | `/holds/{holdId}` | Cancel application (requires `X-Student-Id`) |
| `GET` | `/me/holds` | List this student's applications (requires `X-Student-Id`) |

### Examples

```bash
# Create a class
curl -X POST http://localhost:8080/classes \
  -H 'Content-Type: application/json' \
  -d '{"name":"Class 6 - Morning Shift","totalSeats":120}'

# Apply for a seat
curl -X POST http://localhost:8080/classes/<classId>/holds \
  -H 'X-Student-Id: 20152691234567890'

# Confirm admission
curl -X POST http://localhost:8080/holds/<holdId>/confirm \
  -H 'X-Student-Id: 20152691234567890'
```

### How overbooking is prevented

All seat decisions for a class are serialised with a PostgreSQL transaction and a row‑level lock (`SELECT ... FOR UPDATE`). The service counts active held/confirmed seats after taking the lock and only then inserts the new hold, so two concurrent requests cannot both see the last free seat. A partial unique index on `(ClassId, StudentId)` where the status is `HELD` or `CONFIRMED` is a database‑level guard against a student holding two active seats in the same class.

### Admin verification password

There is no student login, but creating classes is an admin action. The backend optionally accepts a verification password in the `X-Admin-Password` header on `POST /classes`. If provided, the password is verified against the hash stored in the `AdminCredentials` table; if omitted, the request proceeds without admin protection. This optional header was added as an extra security layer and does not affect the API contract.

- The password is stored only as a PBKDF2‑SHA256 hash in the `AdminCredentials` table; the plain text is never stored or returned.
- On the first startup the row is seeded from `ADMIN_PASSWORD` (default `ChangeMe-Admin1`). Once a password exists it is left untouched on later startups, so a changed password survives restarts.
- There is no session: the Admin tab asks for the password, and the backend optionally requires it in the `X-Admin-Password` header on `POST /classes`. Only someone who knows the password can create classes with admin protection.
- Changing the password (`POST /admin/password`) requires the current password, so no other user can modify it.
- The Admin tab shows the demo password for the reader. In a real deployment that value would never be displayed; it appears here only because this project is a demo.

### Environment variables

| Variable | Where | Default | Purpose |
| -------- | ----- | ------- | ------- |
| `POSTGRES_DB` | compose | `school_admission` | Database name |
| `POSTGRES_USER` | compose | `postgres` | Database user |
| `POSTGRES_PASSWORD` | compose | `postgres` | Database password |
| `HOLD_DURATION_SECONDS` | backend | `600` | How long a seat is held before it expires |
| `ADMIN_PASSWORD` | backend | `ChangeMe-Admin1` | Initial admin verification password, hashed and stored on first startup |
| `VITE_API_BASE_URL` | frontend | `http://localhost:8080` | Backend URL used by the Vite dev server |

### Project structure

```
.
├── backend/
│   ├── Dockerfile
│   ├── SchoolAdmission.slnx
│   ├── src/SchoolAdmission.Api/
│   │   ├── Program.cs            # Bootstrap, migrations, admin seeding
│   │   ├── Controllers/          # Classes, Holds, Me, Admin
│   │   ├── Services/             # ClassService, HoldService, AdminService, expiry sweeper
│   │   ├── Data/                 # AppDbContext
│   │   ├── Domain/               # AdmissionClass, AdminCredential, HoldStatus
│   │   ├── Dtos/                 # Request/response contracts
│   │   ├── Validators/           # FluentValidation + student‑id and admin‑pwd validation
│   │   ├── Middleware/           # Exception handling + validation filter
│   │   └── Migrations/
│   └── tests/SchoolAdmission.Tests/
├── frontend/
│   ├── Dockerfile
│   ├── vite.config.ts
│   ├── src/
│   │   ├── App.tsx               # Admin tab with verification UI
│   │   ├── components/admission/ # Classes, My applications, Admin view
│   │   ├── components/ui/        # shadcn/ui‑style primitives
│   │   ├── hooks/                # API client, polling, countdown
│   │   ├── lib/                  # API client, types, formatting
│   │   └── main.tsx
│   └── package.json
├── nginx/
│   ├── Dockerfile
│   └── nginx.conf             # Reverse proxy in front of Vite
├── docker-compose.yml
├── .env.example
├── ASSUMPTIONS.md
├── AI_USAGE.md
└── README.md