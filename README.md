# University Portal 
ASP.NET Core MVC web application for managing university courses, teachers, students, and enrollments.

## Features

### Authentication & Authorization

- Email/password login system using ASP.NET Core Identity.
- Role-based access:
  - **Admin**
  - **Teacher**
  - **Student**
- Admin-only user creation and role assignment.
- Default Admin:
  - Email: `admin1@uni.com`
  - Password: `Admin@123`

## Admin Capabilities

- Add, edit, delete:
  - Courses
  - Teachers
  - Students
- Assign roles to users.
- Enroll/unenroll students.
- Upload profile pictures.
- Manage enrollments.
- View complete system data.

## Teacher Capabilities

- View assigned courses.
- View students enrolled in their courses.
- Filter students by academic year or semester.
- Update grades, project/seminar points, and completion status.
- Cannot manage enrollments.

## Student Capabilities

- View personal course enrollments.
- Upload seminar files (`.doc`, `.docx`, `.pdf`).
- Submit project URL.
- View grades and completion status.
