using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers
{
    // Primary Constructor syntax removes boilerplate constructor fields
    public class HomeController(ILogger<HomeController> logger, ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalStudents = await context.Students.CountAsync(),
                TotalCourses = await context.Courses.CountAsync(),
                TotalInstructors = await context.Instructors.CountAsync(),
                TotalDepartments = await context.Departments.CountAsync(),
                TotalEnrollments = await context.Enrollments.CountAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}