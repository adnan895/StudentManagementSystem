

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StudentController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Student
        public async Task<IActionResult> Index(
            string searchString,
            string selectedDepartment,
            string statusFilter,
            string sortOrder,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["SelectedDepartment"] = selectedDepartment;
            ViewData["StatusFilter"] = statusFilter;

            // Load distinct departments dynamically for the filter dropdown
            ViewData["Departments"] = await _context.Students
                .Select(s => s.Major)
                .Distinct()
                .Where(m => !string.IsNullOrEmpty(m))
                .ToListAsync();

            var studentsQuery = BuildFilteredQuery(searchString, selectedDepartment, statusFilter, sortOrder);

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;

            var paginatedStudents = await PaginatedList<Student>.CreateAsync(studentsQuery, pageIndex, pageSize);
            return View(paginatedStudents);
        }

        // GET: Student/GetTablePartial
        [HttpGet]
        public async Task<IActionResult> GetTablePartial(
            string searchString,
            string selectedDepartment,
            string statusFilter,
            string sortOrder,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["SelectedDepartment"] = selectedDepartment;
            ViewData["StatusFilter"] = statusFilter;

            var studentsQuery = BuildFilteredQuery(searchString, selectedDepartment, statusFilter, sortOrder);

            int pageSize = 5;
            int pageIndex = pageNumber ?? 1;

            var paginatedStudents = await PaginatedList<Student>.CreateAsync(studentsQuery, pageIndex, pageSize);
            return PartialView("_StudentTablePartial", paginatedStudents);
        }


        
        // GET: Student/CreatePartial
        [HttpGet]
        public IActionResult CreatePartial()
        {
            return PartialView("_CreatePartial", new Student());
        }

        // POST: Student/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromForm] Student student, IFormFile? ProfilePicture)
        {
            ModelState.Remove("ProfilePicture");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Validation failed: " + string.Join(" | ", errors)
                });
            }

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                student.ProfilePicturePath = await SaveImageFileAsync(ProfilePicture);
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student saved successfully!" });
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

        // POST: Student/EditAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax([FromForm] Student student, IFormFile? ProfilePicture)
        {
            ModelState.Remove("ProfilePicture");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Validation errors: " + string.Join(" | ", errors) });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingStudent = await _context.Students.FindAsync(student.Id);
                if (existingStudent == null)
                {
                    return Json(new { success = false, message = "Student record not found." });
                }

                bool duplicateEmail = await _context.Students
                    .AnyAsync(s => s.Email == student.Email && s.Id != student.Id);

                if (duplicateEmail)
                {
                    return Json(new { success = false, message = "This email is already in use by another student." });
                }

                existingStudent.FirstName = student.FirstName;
                existingStudent.LastName = student.LastName;
                existingStudent.Email = student.Email;
                existingStudent.Major = student.Major;
                existingStudent.EnrollmentDate = student.EnrollmentDate;

                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    DeleteExistingFile(existingStudent.ProfilePicturePath);
                    existingStudent.ProfilePicturePath = await SaveImageFileAsync(ProfilePicture);
                }

                _context.Students.Update(existingStudent);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = $"Student #{student.Id} updated successfully!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }

        // POST: Student/DeleteAjax/5
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student record not found or already deleted." });
                }

                DeleteExistingFile(student.ProfilePicturePath);

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = $"Student #{id} deleted successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Delete operation failed: " + ex.Message });
            }
        }

        #region Helper Methods

        private IQueryable<Student> BuildFilteredQuery(
            string searchString,
            string selectedDepartment,
            string statusFilter,
            string sortOrder)
        {
            var studentsQuery = _context.Students.AsNoTracking();

            // 1. Text Search (FirstName, LastName, Email)
            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FirstName.Contains(searchString) ||
                    s.LastName.Contains(searchString) ||
                    s.Email.Contains(searchString));
            }

            // 2. Department / Major Dropdown Filter
            if (!string.IsNullOrEmpty(selectedDepartment))
            {
                studentsQuery = studentsQuery.Where(s => s.Major == selectedDepartment);
            }

            // 3. Status Filter (Active vs Graduated)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                var cutoffDate = DateTime.Now.AddYears(-4);
                if (statusFilter == "Active")
                {
                    studentsQuery = studentsQuery.Where(s => s.EnrollmentDate >= cutoffDate);
                }
                else if (statusFilter == "Graduated")
                {
                    studentsQuery = studentsQuery.Where(s => s.EnrollmentDate < cutoffDate);
                }
            }

            // 4. Sort Direction
            return sortOrder switch
            {
                "name_desc" => studentsQuery.OrderByDescending(s => s.FirstName),
                "Date" => studentsQuery.OrderBy(s => s.EnrollmentDate),
                "date_desc" => studentsQuery.OrderByDescending(s => s.EnrollmentDate),
                _ => studentsQuery.OrderBy(s => s.FirstName),
            };
        }

        private async Task<string> SaveImageFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/" + uniqueFileName;
        }

        private void DeleteExistingFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        #endregion
    }
}