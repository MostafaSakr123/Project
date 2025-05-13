namespace Project.Models
{
    public class TaskSubmission
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public int TaskNumber { get; set; }
        public string SubmissionLink { get; set; }
    }
}
