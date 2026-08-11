using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class EnrollmentController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: /Enrollment/Index
        //public async Task<IActionResult> Index()
        //{
        //    var enrollments = await _context.Enrollments
        //        .Include(e => e.Student)
        //        .Include(e => e.Course)
        //        .AsNoTracking()
        //        .ToListAsync();

        //    return View(enrollments);
        //}


        // GET: /Enrollment/Index
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsNoTracking()
                .ToListAsync();

            // Populate dropdown lists for the modal
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

            return View(enrollments);
        }


        // POST: /Enrollment/EnrollStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    // FIX 1: Assign null or enum value if "Pending" exists in your Grade enum
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
        public async Task<IActionResult> AssignGrade(int enrollmentId, string grade)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                // FIX 2: Safely convert incoming string parameter to Grade enum
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