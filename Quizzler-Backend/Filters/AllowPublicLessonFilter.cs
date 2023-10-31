using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Quizzler_Backend.Data;
using System.Security.Claims;

namespace Quizzler_Backend.Filters
{
    public class AllowPublicLessonFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the 'id' parameter exists in the action arguments
            if (!context.ActionArguments.ContainsKey("id") || context.ActionArguments["id"] is not int)
            {
                context.Result = new BadRequestObjectResult(new { Message = "Invalid lesson ID" });
                return;
            }

            // Get the Lesson from the action parameters
            var dbContext = context.HttpContext.RequestServices.GetService(typeof(QuizzlerDbContext)) as QuizzlerDbContext ?? throw new HttpRequestException("dbContext");

            // Get the ID from the action arguments and ensure it's not null before casting
            if (!int.TryParse(context.ActionArguments["id"]?.ToString(), out int lessonId))
            {
                context.Result = new BadRequestObjectResult(new { Message = "Invalid lesson ID" });
                return;
            }
            var lesson = dbContext.Lesson.FirstOrDefault(l => l.LessonId == lessonId);

            if (lesson == null)
            {
                // If the lesson is null, return a 404 Not Found response with a message
                context.Result = new NotFoundObjectResult(new { Message = "Lesson not found" });
                return;
            }
            else
            {
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isOwner = lesson.OwnerId.ToString() == userId;

                if (!lesson.IsPublic && (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated) && !isOwner)

                {
                    // If the lesson is not public and the user is not authenticated, return a 401 Unauthorized response with a message
                    context.Result = new ObjectResult(new { Message = "No access to this lesson" })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

    }
}
