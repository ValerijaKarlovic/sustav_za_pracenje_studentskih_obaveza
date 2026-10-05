# Praćenje studentskih aktivnosti

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

### Frontend

```bash
cd frontend
npm install
npm run dev
```

- Aplikacija: `http://localhost:5173/login`

## Testni korisnici

Nakon pokretanja `skripta3_podaci.sql`:

- **Pri prvoj prijavi** obavezna je promjena lozinke (nova mora biti različita od stare, min. 6 znakova)


| Student | ana.maric@fakultet.hr |
| Nastavnik | marko.horvat@fakultet.hr |
| Administrator | admin@fakultet.hr |

(Novi admin korisnici kroz aplikaciju također dobivaju obaveznu promjenu pri prvoj prijavi.)

## Automatsko vrednovanje

Nastavnik unosi evidenciju (status i bodovi). Sustav automatski:

- zbraja bodove po kolegiju,
- provjerava prag prolaza,
- određuje je li ocjenjivanje završeno (sve aktivnosti ocijenjene, obveze definirane),
- dodjeljuje **cijeli ECTS kolegija** samo kad je kolegij **položen**.

Student na dashboardu vidi graf **kumulativnih bodova kroz vrijeme** (`/api/student/statistika`).

Administrator može **dodijeliti i promijeniti ulogu** korisnika (kreiranje i uređivanje), uz zaštitu (npr. zadnji admin, nastavnik s kolegijima).

Više detalja: `backend/README.md`, `frontend/README.md`.
