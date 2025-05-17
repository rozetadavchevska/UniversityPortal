using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityPortal.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        public int Credits { get; set; }
        [Required]
        public int Semester { get; set; }
        [StringLength(100)]
        public string? Programme { get; set; }
        [Display(Name = "Education Level")]
        [StringLength(25)]
        public string? EducationLevel { get; set; }
        public string? FirstTeacherId { get; set; }
        [ForeignKey("FirstTeacherId")]
        public Teacher? FirstTeacher { get; set; }
        public string? SecondTeacherId { get; set; }
        [ForeignKey("SecondTeacherId")]
        public Teacher? SecondTeacher { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
