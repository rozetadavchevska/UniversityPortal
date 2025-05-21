using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using UniversityPortal.Areas.Identity.Data;
using UniversityPortal.Models;
using UniversityPortal.ViewModels;

namespace UniversityPortal.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Students
        public async Task<IActionResult> Index(StudentFilterViewModel filter)
        {
            var students = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(filter.FirstName))
            {
                students = students.Where(t => t.FirstName.Contains(filter.FirstName));
            }

            if (!string.IsNullOrEmpty(filter.LastName))
            {
                students = students.Where(t => t.LastName.Contains(filter.LastName));
            }

            if (!string.IsNullOrEmpty(filter.StudentId))
            {
                students = students.Where(t => t.StudentId.Contains(filter.StudentId));
            }

            var studentsList = await students.ToListAsync();

            filter.Students = studentsList;

            return View(filter);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(string? id, int? courseId, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (courseId != null)
            {
                var enrollment = await _context.Enrollments
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.StudentId == id && c.CourseId == courseId);

                ViewData["CourseEnrollment"] = enrollment;
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Student student, string email, string password, IFormFile ProfileImage)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(student);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            student.Id = user.Id;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile-images/students");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName = student.FirstName + "_" + student.LastName + Path.GetExtension(ProfileImage.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                student.ProfileImageUrl = "/images/profile-images/students/" + fileName;
            }

            _context.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = student.Id });
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, Student student, IFormFile ProfileImage)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            try
            {
                var existingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

                if (existingStudent == null)
                {
                    return NotFound();
                }

                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile-images/students");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fileName = student.FirstName + "_" + student.LastName + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(uploadFolder, fileName);

                    if (!string.IsNullOrEmpty(student.ProfileImageUrl))
                    {
                        string existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, student.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(existingFilePath))
                        {
                            System.IO.File.Delete(existingFilePath);
                        }
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    student.ProfileImageUrl = "/images/profile-images/students/" + fileName;
                }
                else
                {
                    student.ProfileImageUrl = existingStudent.ProfileImageUrl;
                }

                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = student.Id });
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
