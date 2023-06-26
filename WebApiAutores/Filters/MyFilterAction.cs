using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filters
{
    public class MyFilterAction : IActionFilter
    {
        private readonly ILogger<MyFilterAction> logger;

        public MyFilterAction(ILogger<MyFilterAction> logger)
        {
            this.logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)//cuando ya se ejecuto
        {
            logger.LogInformation("despues de ejecutar la accion");
        }

        public void OnActionExecuting(ActionExecutingContext context)//antes de ejecutar la accion 
        {
            logger.LogInformation("antes de ejecutar la accion");
        }
    }
}
