using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityPortal.Models
{
    public class Enrollment
    {
        public long Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
        [StringLength(10)]
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public int? Grade { get; set; }
        [StringLength(255)]
        public string? SeminarUrl { get; set; }
        [StringLength(255)]
        public string? ProjectUrl { get; set; }
        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }
        public DateOnly? FinishDate { get; set; }
    }
}
