namespace UniversityPortal.ViewModels
{
    public class StudentEnrollmentViewModel
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }

        public bool IsEnrolled { get; set; }

        public int? Grade { get; set; }
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public string? SeminarUrl { get; set; }
        public string? ProjectUrl { get; set; }
        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }
        public DateOnly? FinishDate { get; set; }
    }

}
