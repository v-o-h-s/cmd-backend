# Clinic Management Dashboard API

A robust backend REST API for managing clinic operations, including patient records, doctor schedules, appointments, medical equipment, and administrative tasks.

## 🚀 Features

- **User Authentication**: Secure JWT-based authentication with Access and Refresh tokens stored in HTTP-only cookies.
- **Role-Based Access Control (RBAC)**: Distinct permissions for Admins, Doctors, and Receptionists.
- **Patient Management**: Full CRUD operations for patient records, including medical history and contact details.
- **Appointment System**: Schedule, track, and manage patient appointments with real-time room availability checks.
- **Medical File Management**: Secure storage and retrieval of patient medical files and history.
- **Resource Tracking**: Manage clinic rooms and medical machines/equipment.
- **Dashboard Analytics**: Consolidated statistics for clinic performance, patient flow, and resource utilization.
- **API Documentation**: Integrated Swagger UI for interactive API exploration.

## 🛠 Tech Stack

- **Runtime**: Node.js
- **Framework**: Express.js
- **Language**: TypeScript
- **Database**: Supabase (PostgreSQL)
- **Validation**: Zod
- **Documentation**: Swagger (swagger-jsdoc & swagger-ui-express)
- **Security**: Helmet, CORS, Cookie-parser

## 📋 Prerequisites

- Node.js (v18 or higher)
- npm or yarn
- Supabase account and project

## ⚙️ Installation & Setup

1. **Clone the repository**:

   ```bash
   git clone <repository-url>
   cd cmd-backend
   ```

2. **Install dependencies**:

   ```bash
   cd backend
   npm install
   ```

3. **Configure Environment Variables**:
   Create a `.env` file in the `backend` directory and add your configuration:

   ```env
   PORT=3000
   SUPABASE_URL=your_supabase_url
   SUPABASE_ANON_KEY=your_supabase_anon_key
   JWT_SECRET=your_jwt_secret
   # Add any other required environment variables
   ```

4. **Run the application**:
   - **Development mode**:
     ```bash
     npm run dev
     ```
   - **Production build**:
     ```bash
     npm run build
     npm start
     ```

## 📚 API Documentation

Once the server is running, you can access the interactive Swagger documentation at:
`http://localhost:3000/api-docs`

## 📂 Project Structure

```text
backend/
├── src/
│   ├── application/      # DTOs and Application logic
│   ├── config/           # Dependency injection and configuration
│   ├── infrastructure/   # Database repositories and external services
│   ├── interface/        # Express routes, controllers, and middlewares
│   ├── shared/           # Utilities, constants, and shared types
│   └── index.ts          # Application entry point
├── docs/                 # Additional documentation
└── tsconfig.json         # TypeScript configuration
```

## 🔐 Security

- All sensitive endpoints are protected by `authMiddleware`.
- Role-specific actions are enforced using `requireRole` middleware.
- Input validation is handled via Zod schemas to ensure data integrity.
- Security headers are managed by `helmet`.

---

_Developed by Dilmi Abderrahmane_
