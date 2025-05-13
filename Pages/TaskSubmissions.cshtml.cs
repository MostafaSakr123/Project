//// TaskSubmissions.cshtml.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Project.Models;
//using System.Collections.Generic;

//namespace Project.Pages
//{
//    public class TaskSubmissionsModel : PageModel
//    {
//        private readonly DB _db;

//        public List<TaskSubmission> Submissions { get; set; } = new();
//        public string TaskTitle { get; set; }
//        public string CourseCode { get; set; }
//        public DateTime Deadline { get; set; }

//        public TaskSubmissionsModel(DB db) => _db = db;

//        public void OnGet(int taskId)
//        {
//            Submissions = _db.GetTaskSubmissions(taskId);
//            var taskDetails = _db.GetTaskDetails(taskId);

//            TaskTitle = taskDetails.TaskTitle;
//            CourseCode = taskDetails.CourseCode;
//            Deadline = taskDetails.Deadline;
//        }
//    }
//}