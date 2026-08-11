using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .ToListAsync();

            return View(courses);
        }

        // GET: /Course/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Populate Instructors dropdown for the view
            ViewBag.Instructors = new SelectList(await _context.Instructors.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Instructors = new SelectList(await _context.Instructors.ToListAsync(), "Id", "Name", course.InstructorId);
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

            // Find department students not currently enrolled in this course
            var enrolledStudentIds = course.Enrollments.Select(e => e.StudentId).ToList();

            ViewBag.AvailableStudents = await _context.Students
                .Where(s => s.Major == course.Department && !enrolledStudentIds.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync();

            return View(course);
        }

        // POST: /Course/EnrollStudentAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}