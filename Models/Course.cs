using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityPortal.Models
{
    public class Course
    {
        [Key]
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
        [StringLength(25)]
        public string? EducationLevel { get; set; }
        public int? FirstTeacherId { get; set; }
        [ForeignKey("FirstTeacherId")]
        public Teacher? FirstTeacher { get; set; }
        public int? SecondTeacherId { get; set; }
        [ForeignKey("SecondTeacherId")]
        public Teacher? SecondTeacher { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
