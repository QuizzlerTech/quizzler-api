using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Quizzler_Backend.Data;
using Quizzler_Backend.Models;
using System.Security.Claims;

namespace Quizzler_Backend.Filters
{
    public class AllowPublicLessonFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var dbContext = context.HttpContext.RequestServices.GetService(typeof(QuizzlerDbContext)) as QuizzlerDbContext ?? throw new HttpRequestException("dbContext");

            Lesson? lesson = null;
            if (context.ActionArguments.ContainsKey("title") && context.ActionArguments["title"] is string title)
            {
                // Find the lesson by title
                lesson = dbContext.Lesson.FirstOrDefault(l => l.Title == title);
            }
            else if (context.ActionArguments.ContainsKey("id") && int.TryParse(context.ActionArguments["id"]?.ToString(), out int lessonId))
            {
                // Find the lesson by ID
                lesson = dbContext.Lesson.FirstOrDefault(l => l.LessonId == lessonId);
            }
            if (lesson == null)
            {
                context.Result = new NotFoundObjectResult(new { Message = "Lesson not found" });
                return;
            }
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isOwner = lesson.OwnerId.ToString() == userId;

            if (!lesson.IsPublic && !isOwner)
            {
                // If the lesson is not public and the user is not authenticated, return a 401 Unauthorized response with a message
                context.Result = new ObjectResult(new { Message = "No access to this lesson" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
