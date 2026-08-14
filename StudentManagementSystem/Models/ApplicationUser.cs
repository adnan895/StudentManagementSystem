using Microsoft.AspNetCore.Identity;

namespace StudentManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public Student? Student { get; set; }
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
    }
}