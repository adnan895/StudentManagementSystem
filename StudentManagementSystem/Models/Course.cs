using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty; // e.g., "CS-301"

        [Required]
        public string Title { get; set; } = string.Empty; // e.g., "Database Systems"

        [Range(1, 6)]
        public int Credits { get; set; } = 3;

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty; // Matches Major/Department

        // Foreign Key: Assigned Instructor teaching this course
        [Display(Name = "Assigned Instructor")]
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }

        // Navigation Property: Students enrolled in this course
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}