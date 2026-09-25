# Backend

ASP.NET Core Web API za aplikaciju za praćenje studentskih obaveza.

## Pokretanje API-ja

1. U `appsettings.json` postavite PostgreSQL lozinku i `Jwt:Key`.
2. Baza `studentske_obaveze`: `Database/skripta1.sql`, zatim `Database/skripta3_podaci.sql` 
3. Iz foldera `backend` ili korijena projekta:

```bash
dotnet run
```

ili

```bash
dotnet run --project backend
```

- API: `http://localhost:5099/api`
- Swagger: `http://localhost:5099/swagger`

## Frontend


```bash
cd frontend
npm install
npm run dev
```

Otvorite `http://localhost:5173/login`. 

## Testni login

- `ana.maric@fakultet.hr` - student
- `marko.horvat@fakultet.hr` - nastavnik
- `admin@fakultet.hr` - administrator

## Glavne rute

- `POST /api/auth/login`
- `GET /api/student/kolegiji`
- `GET /api/student/kolegiji/{id}`
- `GET /api/student/statistika`
- `GET /api/nastavnik/kolegiji`
- `GET /api/nastavnik/studenti?kolegijId={id}`
- `GET /api/admin/korisnici`
- `GET /api/admin/kolegiji`
- `GET /api/admin/dashboard`
