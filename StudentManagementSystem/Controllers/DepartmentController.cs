using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace StudentManagementSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Department List
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Major))
                .Select(s => s.Major)
                .Distinct()
                .ToListAsync();

            return View(departments);
        }
    }
}