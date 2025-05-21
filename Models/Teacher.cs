using System.ComponentModel.DataAnnotations;

namespace UniversityPortal.Models
{
    public class Teacher
    {
        public string Id { get; set; }
        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Display(Name = "Second Name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(50)]
        public string? Degree { get; set; }
        [Display(Name = "Academic Rank")]
        [StringLength(25)]
        public string? AcademicRank { get; set; }
        [Display(Name = "Office Number")]
        [StringLength(10)]
        public string? OfficeNumber { get; set; }
        [Display(Name = "Hire Date")]
        public DateOnly? HireDate { get; set; }
        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }

        public ICollection<Course>? FirstTeacherCourses { get; set; }
        public ICollection<Course>? SecondTeacherCourses { get; set; }
    }
}
