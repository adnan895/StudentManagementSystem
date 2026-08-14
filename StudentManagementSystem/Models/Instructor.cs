using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace StudentManagementSystem.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        // Add this property
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string Major { get; set; } = string.Empty;

        // Stores relative path in SQL Server (e.g., "/images/instructors/guid.jpg")
        public string? ImageUrl { get; set; }

        // Binds uploaded file from form; NOT mapped to DB table
        [NotMapped]
        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }


        // Navigation Property: One instructor advises many students
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}