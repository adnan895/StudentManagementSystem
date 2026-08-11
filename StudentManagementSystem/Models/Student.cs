using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        // Automatically combines FirstName and LastName
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public string Major { get; set; } = string.Empty;

    // Optional fields used by views/controllers: Department and Status
    // Keep both for compatibility with existing views; you may consolidate with Major later.
   

    public string Status { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        // Stores the relative path to the image in wwwroot/uploads
        [Display(Name = "Profile Photo")]
        public string? ProfilePicturePath { get; set; }



        
        //public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Foreign Key property in SQL Server (Nullable if advisor is optional)
        public int? InstructorId { get; set; }

        // Navigation Property: Reference back to the single assigned Instructor
        public Instructor? Instructor { get; set; }
    
}
}