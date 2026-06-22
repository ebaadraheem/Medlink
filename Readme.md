# MedLink — University Health Portal (6th semester project)

> A full-stack web application for university students to book appointments with campus doctors, manage health records, and interact with a clinic administration panel.

Built with **ASP.NET Core 10 MVC**, **PostgreSQL (Supabase)**, and a fully custom design system — no Bootstrap. Originally structured around the MVC (Model–View–Controller) pattern adapted from an e-commerce codebase and repurposed into a healthcare platform.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Running the Project](#running-the-project)
- [Configuration](#configuration)
- [Default Credentials](#default-credentials)
- [Route Reference](#route-reference)
- [Seed Data](#seed-data)
- [Design System](#design-system)
- [Known Limitations](#known-limitations)

---

## Overview

MedLink is a semester project that demonstrates a real-world ASP.NET Core MVC application with a clean 3-layer architecture. Students register with their university ID, browse doctors by specialty, book time slots, and view their health records after a visit. An admin panel gives clinic staff full control over doctors, appointments, patients, and specialties.

---

## Features

### Patient Side
- Register with university student ID and email
- Browse all doctors with filtering by specialty, sorting by rating or fee
- View full doctor profiles including qualifications, experience, and available time slots
- Book appointments using an AJAX-powered time slot picker
- Cancel upcoming appointments
- View appointment history with status tracking
- View post-visit health records (diagnosis, prescription, vitals)
- Leave star ratings and written reviews for completed appointments
- Email confirmation sent on booking (requires Brevo API key)

### Admin Panel (`/admin`)
- Dashboard with live stats: today's appointments, pending count, total patients and doctors
- Weekly appointment volume chart
- Manage doctors — create, edit, upload photo, deactivate
- Manage appointments — view details, update status, fill in diagnosis, prescription, vitals
- Auto-generates a patient health record when an appointment is marked as Completed with a diagnosis
- Manage patients — view all registered students
- Manage specialties — create and edit medical departments

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10 MVC |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL via Supabase (session pooler) |
| Auth | ASP.NET Core Identity |
| Email | Brevo (formerly Sendinblue) via `sib_api_v3_sdk` |
| Frontend | Razor Views, vanilla JS, custom CSS design system |
| Fonts | Playfair Display + DM Sans + JetBrains Mono (Google Fonts) |
| Icons | Font Awesome 6 |
| Architecture | 3-layer: Model → View → Controller (MVC pattern) |
| Hosting | Railways via Dockerfile |

---

## Project Structure

```
Medlink-main/
│
├── MedLink.Model/                  # Data layer
│   ├── Data/
│   │   ├── AppDbContext.cs         # EF Core DbContext with all entity config
│   │   ├── DesignTimeDbContextFactory.cs
│   │   └── SeedData.cs             # Seeds specialties, doctors, patients, time slots
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   ├── Appointment.cs
│   │   ├── Doctor.cs
│   │   ├── DoctorReview.cs
│   │   ├── HealthRecord.cs
│   │   ├── Patient.cs
│   │   ├── Specialty.cs
│   │   └── TimeSlot.cs
│   ├── Enums/
│   │   └── AppointmentStatus.cs
│   ├── Migrations/                 # 3 EF migrations
│   ├── Repositories/
│   │   ├── Repository.cs           # Generic repository
│   │   └── PatientRepository.cs    # Patient-specific queries
│   └── DependencyInjection.cs
│
├── MedLink.Presenter/              # Business logic layer
│   ├── Presenters/
│   │   ├── AdminAppointmentPresenter.cs
│   │   ├── AdminDashboardPresenter.cs
│   │   ├── AdminDoctorPresenter.cs
│   │   ├── AppointmentPresenter.cs
│   │   ├── BrevoEmailSender.cs
│   │   ├── DoctorListPresenter.cs
│   │   └── ReviewPresenter.cs
│   ├── ViewModels.cs               # All view model classes
│   ├── Views/IViews.cs
│   └── DependencyInjection.cs
│
├── MedLink.Web/                    # Presentation layer
│   ├── Areas/Identity/Pages/       # Scaffolded Identity (Register, Login)
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── AppointmentController.cs
│   │   ├── HomeController.cs
│   │   ├── ImageController.cs
│   │   └── Admin/
│   │       └── AdminControllers.cs # All admin controllers in one file
│   ├── Views/
│   │   ├── Account/                # Profile page
│   │   ├── AdminAppointments/      # Admin appointment list + detail
│   │   ├── AdminDoctors/           # Admin doctor CRUD
│   │   ├── AdminPatients/          # Admin patient list
│   │   ├── AdminSpecialties/       # Admin specialties
│   │   ├── Appointment/            # Book, Confirmation, Index, HealthRecord
│   │   ├── Dashboard/              # Admin dashboard
│   │   ├── Home/                   # Index, Doctors, DoctorDetail, About, Contact
│   │   └── Shared/
│   │       ├── _Layout.cshtml      # Main public layout
│   │       └── _AdminLayout.cshtml # Admin sidebar layout
│   ├── wwwroot/
│   │   ├── css/medlink.css         # Full custom design system
│   │   └── js/site.js              # Mobile menu + AJAX slot picker
│   └── Program.cs
│
└── MedLink.slnx                    # Solution file
```

---

## Database Schema

```
Specialty          Doctor              TimeSlot
─────────          ──────              ────────
Id                 Id                  Id
Name               Name                DoctorId (FK)
Description        SpecialtyId (FK)    DayOfWeek
Icon               Bio                 StartTime
                   Qualifications      EndTime
                   ExperienceYears     IsRecurring
                   ConsultationFee     SpecificDate
                   Phone               IsBooked
                   Email
                   PhotoData           Appointment
                   IsAvailable         ───────────
                   AverageRating       Id
                   ReviewCount         PatientId (FK)
                                       DoctorId (FK)
Patient                                TimeSlotId (FK)
───────                                AppointmentDate
Id                                     Status (enum)
UserId (FK → AspNetUsers)              Reason
FullName                               Notes
StudentId                              Diagnosis
Department                             Prescription
Phone                                  Fee
DateOfBirth                            IsPaid
BloodGroup                             AppointmentNumber
EmergencyContact
MedicalHistory     HealthRecord        DoctorReview
                   ────────────        ────────────
                   Id                  Id
                   AppointmentId (FK)  DoctorId (FK)
                   PatientId (FK)      PatientId (FK)
                   Diagnosis           Rating
                   Prescription        Comment
                   DoctorNotes         IsVerified
                   FollowUpInstructions
                   VisitDate
                   Weight / Height
                   BloodPressure
                   Temperature
```

**AppointmentStatus enum:** `Pending` → `Confirmed` → `Completed` / `Cancelled` / `NoShow`

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (preview)
- A PostgreSQL database — [Supabase free tier](https://supabase.com) recommended
- [Visual Studio Code](https://code.visualstudio.com/) with the **C# Dev Kit** extension

### 1. Clone / Extract

Unzip the project and open the `Medlink-main` folder in VS Code.

### 2. Create `appsettings.json`

Create this file inside `MedLink.Web/`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=postgres;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Admin": {
    "Email": "admin@medlink.edu",
    "Password": "Admin@123"
  },
  "Brevo": {
    "ApiKey": "",
    "SenderEmail": "noreply@medlink.edu",
    "SenderName": "MedLink Health Portal"
  }
}
```

**Getting your Supabase connection values:**
Go to Supabase Dashboard → Your Project → Settings → Database → **Session pooler** connection string. Copy `Host`, `Username`, and `Password` from there. The format for the Npgsql connection string is different from the `postgresql://` URL — use the key=value format shown above.

### 3. Install the EF Core CLI tool (once)

```bash
dotnet tool install --global dotnet-ef
```

---

## Running the Project

Open the `Medlink-main` folder in a VS Code terminal and run these commands in order:

```bash
# 1. Restore all NuGet packages
dotnet restore

# 2. Push the database schema to Supabase (runs all 3 migrations)
dotnet ef database update --project MedLink.Model --startup-project MedLink.Web

# 3. Start the app
dotnet run --project MedLink.Web
```

Open your browser at `https://localhost:5000`.

On **first run**, the app automatically seeds:
- 6 medical specialties
- 18 doctors (3 per specialty) with ratings and reviews
- 50 student patient accounts
- Time slots for every doctor (Mon–Fri, 9am and 10am slots)

> If you see a browser SSL warning, click "Advanced → Proceed" — this is normal for local HTTPS development.

---

## Configuration

| Key | Description | Required |
|-----|-------------|----------|
| `ConnectionStrings:DefaultConnection` | Npgsql PostgreSQL connection string | ✅ Yes |
| `Admin:Email` | Email address for the auto-created admin account | ✅ Yes |
| `Admin:Password` | Password for the admin account (min 6 chars, 1 digit) | ✅ Yes |
| `Brevo:ApiKey` | API key from brevo.com for sending confirmation emails | ⚪ Optional |
| `Brevo:SenderEmail` | From-address on outgoing emails | ⚪ Optional |
| `Brevo:SenderName` | From-name on outgoing emails | ⚪ Optional |

Leaving `Brevo:ApiKey` empty disables email sending — the rest of the app works normally.

---

## Default Credentials

After first run, log in to the admin panel with:

| Field | Value |
|-------|-------|
| Email | `admin@medlink.edu` |
| Password | `Admin@123` |
| URL | `https://localhost:5000/admin` |

To test as a student, register a new account at `/Identity/Account/Register`. No email confirmation is required in development mode.

---

## Route Reference

### Public routes

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/` | Homepage with featured doctors and specialties |
| GET | `/Home/Doctors` | Doctor listing with search and filters |
| GET | `/Home/DoctorDetail/{id}` | Doctor profile with time slots and reviews |
| GET | `/Home/About` | About page |
| GET | `/Home/Contact` | Contact page |
| GET | `/Identity/Account/Register` | Student registration |
| GET | `/Identity/Account/Login` | Login |

### Authenticated patient routes

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/Account/Profile` | View and edit patient profile |
| GET | `/Appointment/Book/{doctorId}` | Booking page for a specific doctor |
| POST | `/Appointment/Book` | Submit a booking |
| GET | `/Appointment/Confirmation/{id}` | Booking confirmation screen |
| GET | `/Appointment/Index` | Patient's appointment history |
| POST | `/Appointment/Cancel` | Cancel an appointment |
| GET | `/Appointment/HealthRecord/{id}` | View a health record |
| POST | `/Appointment/Review` | Submit a doctor review |
| GET | `/Appointment/GetSlots` | AJAX endpoint — returns available slots for a date |

### Admin routes

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/admin` | Admin dashboard |
| GET | `/admin/appointments` | All appointments (filterable by status) |
| GET | `/admin/appointments/{id}` | Appointment detail + status update form |
| POST | `/admin/appointments/update-status` | Update status, notes, diagnosis, vitals |
| GET | `/admin/doctors` | Doctor list |
| GET | `/admin/doctors/create` | Add new doctor form |
| POST | `/admin/doctors/create` | Save new doctor |
| GET | `/admin/doctors/edit/{id}` | Edit doctor form |
| POST | `/admin/doctors/edit/{id}` | Save doctor edits |
| POST | `/admin/doctors/delete/{id}` | Remove a doctor |
| GET | `/admin/patients` | Patient list |
| GET | `/admin/specialties` | Specialty list |
| GET | `/admin/specialties/create` | Add specialty form |
| POST | `/admin/specialties/create` | Save new specialty |

---

## Seed Data

The `SeedData.cs` runs automatically on first startup (skipped if specialties already exist).

| Entity | Count | Details |
|--------|-------|---------|
| Specialties | 6 | General Medicine, Cardiology, Orthopedics, Dermatology, Psychiatry, Ophthalmology |
| Doctors | 18 | 3 per specialty, random names, fees Rs. 500–2000, experience 5–25 years |
| Patients | 50 | Pakistani names, auto-generated student IDs (STU-1001 to STU-1050) |
| Reviews | 270–720 | 15–40 reviews per doctor, 3–5 stars, average synced to Doctor table |
| Time Slots | ~180 | Mon–Fri, 9:00–9:30 and 10:00–10:30 slots per doctor |

**Note:** The 50 seed patients are created directly in the database without hashed passwords — they exist to populate stats and reviews. Real student accounts are created through the registration form.

---

## Design System

The entire UI is built from a single custom CSS file at `wwwroot/css/medlink.css` — no Bootstrap or Tailwind.

**Color palette:**

| Variable | Hex | Usage |
|----------|-----|-------|
| `--teal-800` | `#0D4F5C` | Primary brand, buttons, nav |
| `--teal-500` | `#2299B3` | Accents, icons |
| `--teal-100` | `#E0F7FC` | Tinted backgrounds |
| `--navy-800` | `#0A1628` | Heading text |
| `--ivory` | `#FAFAF7` | Page background |
| `--muted` | `#6B7A8A` | Body text, labels |
| `--accent-coral` | `#E8593C` | Danger, cancel actions |
| `--accent-green` | `#2DB87A` | Success, confirmed status |

**Typography:**
- Headings — `Playfair Display` (serif, Google Fonts)
- Body — `DM Sans` (sans-serif, Google Fonts)
- Data / codes — `JetBrains Mono` (monospace, Google Fonts)

**Key components defined in CSS:**
`.doctor-card`, `.slot-btn`, `.stat-card`, `.admin-sidebar`, `.confirm-card`, `.specialty-card`, `.badge`, `.alert`, `.table`, `.form-control`, `.btn` (primary, outline, ghost, danger)

---

## Known Limitations

- **No real-time slot locking** — two users can attempt to book the same slot simultaneously. The second request will be rejected by the database constraint but there is no live availability update without SignalR.
- **Time slots are weekly recurring** — slots are defined by `DayOfWeek` and `StartTime`, not a specific calendar date. This means booking uses the selected date's day-of-week to find matching slots, not an exact datetime.
- **No payment integration** — `Fee` and `IsPaid` fields are stored but there is no payment gateway. Intended for a university clinic where fees are handled separately.
- **Brevo email is fire-and-forget** — if the API call fails (wrong key, quota exceeded), the booking still succeeds. No retry mechanism.
- **Admin has no pagination** — the patient and appointment tables load all records. Fine for a university clinic scale, not suitable for thousands of records.
- **Photos stored as binary in the database** — doctor photos are stored as `byte[]` in PostgreSQL. For large deployments, move to object storage (Supabase Storage or S3).

---

## Built With

This project was developed as a university semester project, structurally derived from an e-commerce ASP.NET MVC application and repurposed into a healthcare appointment platform. The entity mapping: Products → Doctors, Cart → Appointment Draft, Orders → Appointments, Categories → Specialties, Payment → HealthRecord (new entity), Reviews → DoctorReviews.

**New additions beyond the original structure:**
- `TimeSlot` entity with day-of-week based recurring schedule
- `HealthRecord` entity for post-visit clinical data
- AJAX slot picker with date-based filtering
- Student ID field in registration
- Admin appointment detail with inline vitals entry and automatic health record creation
- Full custom design system replacing Bootstrap
