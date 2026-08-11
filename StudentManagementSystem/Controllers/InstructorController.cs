using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InstructorController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Instructor/Index
        public async Task<IActionResult> Index()
        {
            // Eager-load the related Students collection for each instructor
            var instructors = await _context.Instructors
                .Include(i => i.Students)
                .AsNoTracking()
                .ToListAsync();

            return View(instructors);
        }

        // GET: /Instructor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Instructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                // Handle Profile Image Upload
                if (instructor.ImageFile != null)
                {
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "instructors");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(instructor.ImageFile.FileName);
                    string filePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await instructor.ImageFile.CopyToAsync(stream);
                    }

                    instructor.ImageUrl = "/images/instructors/" + uniqueFileName;
                }

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(instructor);
        }

        // GET: /Instructor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var instructor = await _context.Instructors
                .Include(i => i.Students)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null) return NotFound();

            // Query ONLY students matching the instructor's Major/Department who have NO assigned advisor
            ViewBag.UnassignedStudents = await _context.Students
                .Where(s => s.Major == instructor.Major && s.InstructorId == null)
                .AsNoTracking()
                .ToListAsync();

            return View(instructor);
        }

        // POST: /Instructor/AssignStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudent(int instructorId, int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.InstructorId = instructorId; // Set FK relationship
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = instructorId });
        }

        // POST: /Instructor/UnassignStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignStudent(int instructorId, int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.InstructorId = null; // Remove FK relationship
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = instructorId });
        }
    }
}

