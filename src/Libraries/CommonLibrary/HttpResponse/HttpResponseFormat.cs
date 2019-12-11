using Microsoft.AspNetCore.Mvc;

namespace CommonLibrary.HttpResponse 
{
    public class HttpResponseFormat : ControllerBase
    {
        private readonly ResponseDetails responseDetails;
        public HttpResponseFormat()
        {
           this.responseDetails = new ResponseDetails(); 
        }
        public ObjectResult ErrorResponse(string message, int statusCode) {
            responseDetails.Message = message;
            responseDetails.StatusCode = statusCode;
            return StatusCode(statusCode, responseDetails);
        }
    }
}