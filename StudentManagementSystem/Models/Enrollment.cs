using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public enum Grade { A, B, C, D, F, Pending }

    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public Grade? Grade { get; set; }
    }
}