using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(CourseFilterViewModel filter)
        {
            var courses = _context.Courses.AsQueryable();

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
        public async Task<IActionResult> Details(int? id)
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
                        IsEnrolled = enrollment != null,
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
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null) return NotFound();

            _context.Enrollments.RemoveRange(course.Enrollments);

            foreach (var student in model.Students)
            {
                if (student.IsEnrolled)
                {
                    var enrollment = new Enrollment
                    {
                        CourseId = model.CourseId,
                        StudentId = student.StudentId,
                        Grade = student.Grade,
                        Semester = student.Semester,
                        Year = student.Year,
                        SeminarUrl = student.SeminarUrl,
                        ProjectUrl = student.ProjectUrl,
                        ExamPoints = student.ExamPoints,
                        SeminarPoints = student.SeminarPoints,
                        ProjectPoints = student.ProjectPoints,
                        AdditionalPoints = student.AdditionalPoints,
                        FinishDate = student.FinishDate
                    };

                    _context.Enrollments.Add(enrollment);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.CourseId });
        }

    }
}
