using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using QuestPDF.Fluent;
using StudentManagementSystem.Reports;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Instructor,Student")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: /Course/Index
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            // Simple == translates cleanly to SQL WHERE Email = @userEmail
            var matchedInstructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.Email == userEmail);

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .ToListAsync();

            return View(courses);
        }

        // GET: /Course/Index
        //public async Task<IActionResult> Index()
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    var userEmail = currentUser?.Email;

        //    if (string.IsNullOrEmpty(userEmail))
        //    {
        //        return Challenge();
        //    }

        //    IQueryable<Course> query = _context.Courses
        //        .Include(c => c.Instructor)
        //        .Include(c => c.Enrollments)
        //        .AsNoTracking();

        //    // Data Isolation: Filter courses based on logged-in role
        //    if (User.IsInRole("Instructor"))
        //    {
        //        // If the ApplicationUser isn't linked to an Instructor record, try to auto-link by email
        //        if (currentUser != null && !currentUser.InstructorId.HasValue && !string.IsNullOrEmpty(userEmail))
        //        {
        //            var matchedInstructor = await _context.Instructors
        //                .AsNoTracking()
        //                .FirstOrDefaultAsync(i => i.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));

        //            if (matchedInstructor != null)
        //            {
        //                currentUser.InstructorId = matchedInstructor.Id;
        //                // Persist the link so future requests use the FK path
        //                var updateResult = await _userManager.UpdateAsync(currentUser);
        //                // If update fails, we silently continue and fallback to email matching below
        //            }
        //        }

        //        // Prefer matching by linked InstructorId stored on ApplicationUser (more reliable)
        //        var instructorId = currentUser?.InstructorId;
        //        if (instructorId.HasValue)
        //        {
        //            query = query.Where(c => c.InstructorId == instructorId.Value);
        //        }
        //        else if (!string.IsNullOrEmpty(userEmail))
        //        {
        //            // Fallback: match by email on Instructor record
        //            query = query.Where(c => c.Instructor != null && c.Instructor.Email == userEmail);
        //        }
        //        else
        //        {
        //            // If we cannot determine instructor identity, show none
        //            query = query.Where(c => false);
        //        }
        //    }
        //    if (User.IsInRole("Student"))
        //    {
        //        // Students see courses in which they are currently enrolled
        //        query = query.Where(c => c.Enrollments.Any(e => e.Student != null && e.Student.Email == userEmail));
        //    }

        //    var courses = await query.ToListAsync();
        //    return View(courses);
        //}

        // GET: /Course/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Instructors = new SelectList(
                await _context.Instructors.AsNoTracking().ToListAsync(),
                "Id",
                "Name"
            );
            return View();
        }

        // POST: /Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Instructors = new SelectList(
                await _context.Instructors.AsNoTracking().ToListAsync(),
                "Id",
                "Name",
                course.InstructorId
            );
            return View(course);
        }

        // GET: /Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            if (User.IsInRole("Admin") || User.IsInRole("Instructor"))
            {
                var enrolledStudentIds = course.Enrollments
                    .Select(e => e.StudentId)
                    .ToList();

                ViewBag.AvailableStudents = await _context.Students
                    .Where(s => s.Major == course.Department && !enrolledStudentIds.Contains(s.Id))
                    .AsNoTracking()
                    .ToListAsync();
            }

            return View(course);
        }

        // POST: /Course/EnrollStudentAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> EnrollStudentAjax(int courseId, int studentId)
        {
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (alreadyEnrolled)
            {
                return Json(new { success = false, message = "Student is already enrolled in this course." });
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                Grade = Grade.Pending
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student enrolled successfully!" });
        }

        // POST: /Course/UpdateGradeAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateGradeAjax(int enrollmentId, Grade grade)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return Json(new { success = false, message = "Enrollment record not found." });
            }

            enrollment.Grade = grade;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Grade updated successfully!" });
        }

        // POST: /Course/RemoveEnrollmentAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> RemoveEnrollmentAjax(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return Json(new { success = false, message = "Record not found." });
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student removed from course roster." });
        }

        // Fixed return type: Task<IActionResult> allows returning File(...)
        public async Task<IActionResult> ExportPdf()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .ToListAsync();

            var report = new CourseCatalogReport(courses);
            byte[] pdfBytes = report.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Course_Catalog_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}