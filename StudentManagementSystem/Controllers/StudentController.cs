using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Student
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            // 1. Keep track of current sort orders for the View
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            // 2. Start with base IQueryable (does not hit DB yet)
            var studentsQuery = _context.Students.AsQueryable();

            // 3. Apply Search Filter (LINQ .Where)
            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FirstName.Contains(searchString) ||
                    s.LastName.Contains(searchString) ||
                    s.Email.Contains(searchString) ||
                    s.Major.Contains(searchString));
            }

            // 4. Apply Sorting (LINQ .OrderBy / .OrderByDescending)
            studentsQuery = sortOrder switch
            {
                "name_desc" => studentsQuery.OrderByDescending(s => s.LastName),
                "Date" => studentsQuery.OrderBy(s => s.EnrollmentDate),
                "date_desc" => studentsQuery.OrderByDescending(s => s.EnrollmentDate),
                _ => studentsQuery.OrderBy(s => s.LastName), // Default sort by Last Name
            };

            // 5. Execute LINQ query against SQL Server
            var students = await studentsQuery.ToListAsync();

            return View(students);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student created successfully!" });
        }

        // POST: Student/EditAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fix validation errors." });
            }

            // 1. Begin Database Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Data Integrity Check: Ensure student exists
                var existingStudent = await _context.Students.FindAsync(student.Id);
                if (existingStudent == null)
                {
                    return Json(new { success = false, message = "Student record not found." });
                }

                // 3. Business Rule Check: Ensure email isn't stolen by another student record
                bool duplicateEmail = await _context.Students
                    .AnyAsync(s => s.Email == student.Email && s.Id != student.Id);

                if (duplicateEmail)
                {
                    return Json(new { success = false, message = "This email is already in use by another student." });
                }

                // 4. Update Properties
                existingStudent.FirstName = student.FirstName;
                existingStudent.LastName = student.LastName;
                existingStudent.Email = student.Email;
                existingStudent.Major = student.Major;
                existingStudent.EnrollmentDate = student.EnrollmentDate;

                _context.Students.Update(existingStudent);
                await _context.SaveChangesAsync();

                // 5. Commit Transaction
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"Student #{student.Id} updated successfully!"
                });
            }
            catch (Exception ex)
            {
                // 6. Rollback on Exception
                await transaction.RollbackAsync();
                return Json(new
                {
                    success = false,
                    message = "Update failed and rolled back: " + ex.Message
                });
            }
        }

        // POST: Student/DeleteAjax/5
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            // 1. Begin Database Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Retrieve student
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student record not found or already deleted." });
                }

                // 3. Remove and Save
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // 4. Commit Transaction
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"Student #{id} deleted successfully."
                });
            }
            catch (Exception ex)
            {
                // 5. Rollback on Exception
                await transaction.RollbackAsync();
                return Json(new
                {
                    success = false,
                    message = "Delete operation failed and rolled back: " + ex.Message
                });
            }
        }

        // GET: Student/CreatePartial
        [HttpGet]
        public IActionResult CreatePartial()
        {
            return PartialView("_CreatePartial", new Student());
        }

        // GET: Student/EditPartial/5
        [HttpGet]
        public async Task<IActionResult> EditPartial(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return PartialView("_EditPartial", student);
        }

        // GET: Student/GetTablePartial
        [HttpGet]
        public async Task<IActionResult> GetTablePartial(string searchString, string sortOrder)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            var studentsQuery = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FirstName.Contains(searchString) ||
                    s.LastName.Contains(searchString) ||
                    s.Email.Contains(searchString) ||
                    s.Major.Contains(searchString));
            }

            studentsQuery = sortOrder switch
            {
                "name_desc" => studentsQuery.OrderByDescending(s => s.LastName),
                "Date" => studentsQuery.OrderBy(s => s.EnrollmentDate),
                "date_desc" => studentsQuery.OrderByDescending(s => s.EnrollmentDate),
                _ => studentsQuery.OrderBy(s => s.LastName),
            };

            var students = await studentsQuery.ToListAsync();
            return PartialView("_StudentTablePartial", students);
        }
    }
}