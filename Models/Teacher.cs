using System.ComponentModel.DataAnnotations;

namespace UniversityPortal.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(50)]
        public string? Degree { get; set; }
        [StringLength(25)]
        public string? AcademicRank { get; set; }
        [StringLength(10)]
        public string? OfficeNumber { get; set; }
        public DateOnly? HireDate { get; set; }
        
        public ICollection<Course>? FirstTeacherCourses { get; set; }
        public ICollection<Course>? SecondTeacherCourses { get; set; }
    }
}
