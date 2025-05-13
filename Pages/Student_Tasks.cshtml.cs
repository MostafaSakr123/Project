using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace Project.Pages
{
    public class Student_TasksModel : PageModel
    {
        private readonly DB db;

        public Student_TasksModel(DB database)
        {
            db = database;
        }

        public List<TaskViewModel> Tasks { get; set; }

        [BindProperty]
        public string SubmissionLink { get; set; }

        [BindProperty]
        public int TaskId { get; set; }

        public IActionResult OnGet()
        {
            int? studentId = HttpContext.Session.GetInt32("studentId");
            if (studentId == null)
                return RedirectToPage("/Login");

            var allTasks = db.GetTasksByStudent(studentId.Value);
            var submissions = db.GetSubmissionsByStudent(studentId.Value);

            Tasks = allTasks.Select(task =>
            {
                var submission = submissions.FirstOrDefault(s => s.TaskNumber == task.TaskNumber);

                return new TaskViewModel
                {
                    TaskNumber = task.TaskNumber,
                    Title = task.Title,
                    SubmissionLink = submission?.SubmissionLink
                };
            }).ToList();

            return Page();
        }


        public IActionResult OnPost()
        {
            var studentId = HttpContext.Session.GetInt32("studentId");
            

            if (!string.IsNullOrWhiteSpace(SubmissionLink))
            {
                db.SubmitTask(new TaskSubmission
                {
                    StudentId = studentId.Value,
                    TaskNumber = TaskId,
                    SubmissionLink = SubmissionLink
                });
            }

            return RedirectToPage();
        }
    }
    public class TaskViewModel
    {
        public int TaskNumber { get; set; }
        public string Title { get; set; }
        public string SubmissionLink { get; set; } // Holds the actual link, or null/empty if not submitted
    }

}

