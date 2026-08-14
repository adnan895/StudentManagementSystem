using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data
{
    // MUST inherit from IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

     
        public DbSet<Instructor> Instructors { get; set; }

     
        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Department> Departments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // CRITICAL: Must call base.OnModelCreating to map Identity keys & tables
            base.OnModelCreating(builder);
        }

    }
}