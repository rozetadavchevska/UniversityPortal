using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityPortal.Areas.Identity.Data;
using UniversityPortal.Models;
using UniversityPortal.ViewModels;

namespace UniversityPortal.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Courses
        public async Task<IActionResult> Index(CourseFilterViewModel filter)
        {
            var courses = _context.Courses.AsQueryable();

            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Student"))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.Id);
                if (student != null)
                {
                    var enrolledCourses = _context.Enrollments.Where(e => e.StudentId == student.Id)
                        .Select(e => e.CourseId);
                    courses = courses.Where(c => enrolledCourses.Contains(c.Id));
                }
            }
            else if (User.IsInRole("Teacher"))
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == user.Id);
                if (teacher != null)
                {
                    courses = courses.Where(c => c.FirstTeacherId == teacher.Id || c.SecondTeacherId == teacher.Id);
                }
            }

            if (!string.IsNullOrEmpty(filter.Title))
            {
                courses = courses.Where(c => c.Title.Contains(filter.Title));
            }

            if (filter.Semester.HasValue)
            {
                courses = courses.Where(s => s.Semester == filter.Semester);
            }

            if (!string.IsNullOrEmpty(filter.Programme))
            {
                courses = courses.Where(c => c.Programme.Contains(filter.Programme));
            }

            var coursesList = await courses.ToListAsync();

            filter.Courses = coursesList;

            return View(filter);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id, int? year)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var years = course.Enrollments
                .Where(e => e.Year.HasValue)
                .Select(e => e.Year.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            int selectedYear = year ?? DateTime.Now.Year;

            if (User.IsInRole("Student"))
            {
                var user = await _userManager.GetUserAsync(User);
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.Id);
                var enrollment = course.Enrollments.FirstOrDefault(e => e.StudentId == student.Id);
                ViewData["StudentEnrollment"] = enrollment;
            }
            else if (User.IsInRole("Teacher"))
            {
                course.Enrollments = course.Enrollments
                    .Where(e => e.Year == selectedYear)
                    .ToList();
            }

            ViewData["AvailableYears"] = years;
            ViewData["SelectedYear"] = selectedYear;

            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.FirstTeacherId = _context.Teachers
               .Select(t => new SelectListItem
               {
                   Value = t.Id.ToString(),
                   Text = t.FirstName + " " + t.LastName
               })
               .ToList();

            ViewBag.SecondTeacherId = _context.Teachers
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToList();

            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FirstTeacherId = _context.Teachers
               .Select(t => new SelectListItem
               {
                   Value = t.Id.ToString(),
                   Text = t.FirstName + " " + t.LastName
               })
               .ToList();

            ViewBag.SecondTeacherId = _context.Teachers
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToList();

            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (course == null)
            {
                return NotFound();
            }

            ViewBag.FirstTeacherId = _context.Teachers
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToList();

            ViewBag.SecondTeacherId = _context.Teachers
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToList();

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        public async Task<IActionResult> EnrollStudents(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var allStudents = await _context.Students.ToListAsync();

            var model = new CourseEnrollmentViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Students = allStudents.Select(student =>
                {
                    var enrollment = course.Enrollments.FirstOrDefault(e => e.StudentId == student.Id);
                    return new StudentEnrollmentViewModel
                    {
                        StudentId = student.Id,
                        FullName = student.FirstName + " " + student.LastName,
                        IsEnrolled = enrollment != null && enrollment.FinishDate == null,
                        Grade = enrollment?.Grade,
                        Semester = enrollment?.Semester,
                        Year = enrollment?.Year,
                        ProjectUrl = enrollment?.ProjectUrl,
                        ExamPoints = enrollment?.ExamPoints,
                        SeminarPoints = enrollment?.SeminarPoints,
                        ProjectPoints = enrollment?.ProjectPoints,
                        AdditionalPoints = enrollment?.AdditionalPoints,
                        FinishDate = enrollment?.FinishDate
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudents(CourseEnrollmentViewModel model)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null) return NotFound();

            foreach (var student in model.Students)
            {
                var existingEnrollment = course.Enrollments.FirstOrDefault(e => e.StudentId == student.StudentId);

                if (existingEnrollment != null)
                {
                    if (student.IsEnrolled)
                    {
                        existingEnrollment.Semester = student.Semester;
                        existingEnrollment.Year = student.Year;
                        existingEnrollment.FinishDate = student.FinishDate;
                    }
                    else
                    {
                        if (existingEnrollment.FinishDate == null)
                            existingEnrollment.FinishDate = DateOnly.FromDateTime(DateTime.Today);
                    }
                }
                else if (student.IsEnrolled)
                {
                    var enrollment = new Enrollment
                    {
                        CourseId = model.CourseId,
                        StudentId = student.StudentId,
                        Semester = student.Semester,
                        Year = student.Year
                    };
                    _context.Enrollments.Add(enrollment);
                }
            }
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.CourseId });
        }

    }
}
