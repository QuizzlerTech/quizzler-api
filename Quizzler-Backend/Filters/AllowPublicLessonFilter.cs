using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Quizzler_Backend.Models;
using System.Linq;
using System.Security.Claims;

namespace Quizzler_Backend.Filters
{
    public class AllowPublicLessonFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the 'id' parameter exists in the action arguments
            if (!context.ActionArguments.ContainsKey("id") || !(context.ActionArguments["id"] is int))
            {
                // If the 'id' parameter is missing or is not an integer, return a 400 Bad Request response with a message
                context.Result = new BadRequestObjectResult(new { Message = "Invalid lesson ID." });
                return;
            }

            // Get the Lesson from the action parameters
            var lessonId = (int)context.ActionArguments["id"];
            var dbContext = context.HttpContext.RequestServices.GetService(typeof(QuizzlerDbContext)) as QuizzlerDbContext;

            // Check if the lesson is public
            var lesson = dbContext.Lesson.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                // If the lesson is null, return a 404 Not Found response with a message
                context.Result = new NotFoundObjectResult(new { Message = "Lesson not found." });
                return;
            }
            else
            {
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isOwner = lesson.OwnerId.ToString() == userId;
                if (!lesson.IsPublic && !context.HttpContext.User.Identity.IsAuthenticated && !isOwner)
                {
                    // If the lesson is not public and the user is not authenticated, return a 401 Unauthorized response with a message
                    context.Result = new ObjectResult(new { Message = "No access to this lesson." })
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
