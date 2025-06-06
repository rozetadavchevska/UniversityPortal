﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityPortal.Areas.Identity.Data;
using UniversityPortal.Models;

namespace UniversityPortal.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EnrollmentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Students
                .Select(s => new { s.Id, FullName = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")" }),
                "Id", "FullName");

            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students
                .Select(s => new { s.Id, FullName = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")" }),
                "Id", "FullName", enrollment.StudentId);

            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students
                .Select(s => new { s.Id, FullName = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")" }),
                "Id", "FullName", enrollment.StudentId);

            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students
                .Select(s => new { s.Id, FullName = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")" }),
                "Id", "FullName", enrollment.StudentId);

            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AttachSeminarFile(IFormFile file, string studentFullName, long enrollmentId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "seminar-files", studentFullName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{enrollmentId}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/seminar-files/{studentFullName}/{fileName}";

            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return NotFound();

            enrollment.SeminarUrl = fileUrl;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = enrollment.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AttachProjectUrl(string projectUrl, long enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(projectUrl) || !Uri.IsWellFormedUriString(projectUrl, UriKind.Absolute))
            {
                ModelState.AddModelError("projectUrl", "Invalid URL.");
                return RedirectToAction("Details", "Courses", new { id = enrollment.CourseId });
            }

            enrollment.ProjectUrl = projectUrl;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = enrollment.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdatePoints(long EnrollmentId, int? ExamPoints, int? SeminarPoints, int? ProjectPoints, int? AdditionalPoints, int? Grade, DateOnly? FinishDate, string StudentId, int CourseId)
        {
            var enrollment = await _context.Enrollments.FindAsync(EnrollmentId);
            if (enrollment == null)
                return NotFound();

            // Update only allowed fields
            enrollment.ExamPoints = ExamPoints;
            enrollment.SeminarPoints = SeminarPoints;
            enrollment.ProjectPoints = ProjectPoints;
            enrollment.AdditionalPoints = AdditionalPoints;
            enrollment.Grade = Grade;
            enrollment.FinishDate = FinishDate;

            await _context.SaveChangesAsync();

            // Redirect back to the student details, preserving course context
            return RedirectToAction("Details", "Students", new { id = StudentId, courseId = CourseId });
        }


        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
