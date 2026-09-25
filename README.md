# Praćenje studentskih obaveza

Vue 3 frontend + ASP.NET Core API + PostgreSQL.

## Baza (jednom)

U pgAdminu na bazi `studentske_obaveze` pokrenite redom:

1. `backend/Database/skripta1.sql` 
2. `backend/Database/skripta3_podaci.sql`

U `backend/appsettings.json` postavite PostgreSQL lozinku i `Jwt:Key`.

### Backend

```bash
dotnet run --project backend
```

- API: `http://localhost:5099/api`
- Swagger: `http://localhost:5099/swagger`

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

- Aplikacija: `http://localhost:5173/login`

## Testni korisnici

 Student - ana.maric@fakultet.hr 
 Nastavnik -  marko.horvat@fakultet.hr 
 Administrator - admin@fakultet.hr 

Više detalja: `backend/README.md`, `frontend/README.md`.
