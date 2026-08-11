using System.Collections.Generic;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalDepartments { get; set; }
        public int SeniorCount { get; set; }
        public int JuniorCount { get; set; }
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
        public List<Student> RecentEnrollments { get; set; } = new();
    }
}