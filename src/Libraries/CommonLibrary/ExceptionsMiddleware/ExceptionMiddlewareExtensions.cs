using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CommonLibrary.ExceptionsMiddleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError => {
                appError.Run(async context => {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null) {
                        await context.Response.WriteAsync(new {
                            StatusCode = context.Response.StatusCode,
                            Message= "Internal Server Error. We're currently working to fix it."
                        }.ToString());
                    }
                });
            });
        }
    }
}