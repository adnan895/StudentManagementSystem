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

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}