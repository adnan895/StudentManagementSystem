using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Instructor,Student")]
    public class EnrollmentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // GET: /Enrollment/Index
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            IQueryable<Enrollment> query = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsNoTracking();

            // 1. Data Isolation: Filter records based on role
            if (User.IsInRole(""))
            {
                if (currentUser.StudentId == null)
                {
                    return Forbid();
                }

                // Students only see their own enrollments & grades
                query = query.Where(e => e.StudentId == currentUser.StudentId.Value);
            }
            else if (User.IsInRole(""))
            {
                if (currentUser.InstructorId == null)
                {
                    return Forbid();
                }

                // Instructors only see enrollments for courses they teach
                if (currentUser.InstructorId.HasValue)
                {
                    var instrId = currentUser.InstructorId.Value;
                    query = query.Where(e => e.Course.InstructorId == instrId);
                }
                else
                {
                    // Current user is in Instructor role but not linked to an Instructor record:
                    // return no records rather than accessing a missing Email property.
                    query = query.Where(e => false);
                }
            }

            var enrollments = await query.ToListAsync();

            // 2. Populate dropdown lists ONLY for Admin and Instructor modals
            if (User.IsInRole("Admin") || User.IsInRole("Instructor"))
            {
                ViewBag.StudentId = new SelectList(
                    await _context.Students
                        .Select(s => new { s.Id, Name = (s.FirstName + " " + s.LastName).Trim() })
                        .ToListAsync(),
                    "Id",
                    "Name"
                );

                ViewBag.CourseId = new SelectList(
                    await _context.Courses.ToListAsync(),
                    "Id",
                    "Title"
                );
            }

            return View(enrollments);
        }

        // POST: /Enrollment/EnrollStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> EnrollStudent(int studentId, int courseId)
        {
            bool exists = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Grade = null
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Enrollment/AssignGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> AssignGrade(int enrollmentId, string grade)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                if (Enum.TryParse<Grade>(grade, ignoreCase: true, out var parsedGrade))
                {
                    enrollment.Grade = parsedGrade;
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Enrollment/DropStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DropStudent(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}