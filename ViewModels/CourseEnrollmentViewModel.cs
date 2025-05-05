namespace UniversityPortal.ViewModels
{
    public class CourseEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        public List<StudentEnrollmentViewModel> Students { get; set; }
    }

}
