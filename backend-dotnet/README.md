# Clinic Backend (.NET)

Step-by-step migration target for the existing TypeScript backend.

## Current step

Implemented in this step:

- ASP.NET Core Web API scaffold
- Global response envelope (`success/message/data/error`)
- Global error middleware
- Supabase HTTP clients (Auth + PostgREST)
- Auth endpoints:
  - `POST /auth/login`
  - `POST /auth/logout`
  - `POST /auth/refresh-token`
  - `POST /auth/me`
- User endpoint:
  - `POST /users/add-user` (requires `admin`)
- Doctors endpoints:
  - `GET /doctors`
  - `GET /doctors/:id`
  - `PUT /doctors/:id`
  - `DELETE /doctors/:id`
- Receptionists endpoints:
  - `GET /receptionists`
  - `GET /receptionists/:id`
  - `PUT /receptionists/:id`
  - `DELETE /receptionists/:id`
- Patients endpoints:
  - `POST /patients/add-patient`
  - `GET /patients/:id`
  - `DELETE /patients/:id`
  - `GET /patients`
  - `PUT /patients/:id`
- Medical files endpoints:
  - `POST /medical-files`
  - `GET /medical-files/patient/:patientId`
  - `PUT /medical-files/:id`
  - `DELETE /medical-files/:id`
- Cookie behavior parity:
  - `accessToken` cookie path `/` (6 days)
  - `refreshToken` cookie path `/auth/refresh-token` (7 days)
- Auth flow parity:
  - read token from cookie first, then bearer header
  - resolve profile from `profiles` table and attach user context

## Configuration

Set these in `appsettings.json` or user secrets:

- `Supabase:Url`
- `Supabase:PublishableDefaultKey`
- `Supabase:ServiceKey`

## Run

```bash
dotnet restore
dotnet run --project backend-dotnet/ClinicBackend.Api.csproj
```

Swagger is available in development.

## Next step

Port remaining route groups with parity for controllers/repositories/use-cases:

- `/rooms`
- `/machines`
- `/appointments`
- `/stats`
