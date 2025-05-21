using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeachersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(TeacherFilterViewModel filter)
        {
            var teachers = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(filter.FirstName))
            {
                teachers = teachers.Where(t => t.FirstName.Contains(filter.FirstName));
            }

            if (!string.IsNullOrEmpty(filter.LastName))
            {
                teachers = teachers.Where(t => t.LastName.Contains(filter.LastName));
            }

            if (!string.IsNullOrEmpty(filter.Degree))
            {
                teachers = teachers.Where(t => t.Degree.Contains(filter.Degree));
            }

            if (!string.IsNullOrEmpty(filter.AcademicRank))
            {
                teachers = teachers.Where(t => t.AcademicRank.Contains(filter.AcademicRank));
            }

            var teachersList = await teachers.ToListAsync();

            filter.Teachers = teachersList;

            return View(filter);
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(string? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.FirstTeacherCourses)
                .Include(t => t.SecondTeacherCourses)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(teacher);
        }

        // GET: Teachers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Teacher teacher, string email, string password, IFormFile ProfileImage)
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
                return View(teacher);
            }

            await _userManager.AddToRoleAsync(user, "Teacher");

            teacher.Id = user.Id;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile-images/teachers");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName = teacher.FirstName + "_" + teacher.LastName + Path.GetExtension(ProfileImage.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                teacher.ProfileImageUrl = "images/profile-images/teachers" + fileName;
            }

            _context.Add(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }

        // GET: Teachers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, Teacher teacher, IFormFile ProfileImage)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            try
            {
                var existingTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

                if (existingTeacher == null)
                {
                    return NotFound();
                }

                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile-images/teachers");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fileName = teacher.FirstName + "_" + teacher.LastName + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(uploadFolder, fileName);

                    if (!string.IsNullOrEmpty(teacher.ProfileImageUrl))
                    {
                        string existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, teacher.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(existingFilePath))
                        {
                            System.IO.File.Delete(existingFilePath);
                        }
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    teacher.ProfileImageUrl = "/images/profile-images/teachers/" + fileName;
                }
                else
                {
                    teacher.ProfileImageUrl = existingTeacher.ProfileImageUrl;
                }

                _context.Update(teacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(teacher.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }

        // GET: Teachers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(string id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
