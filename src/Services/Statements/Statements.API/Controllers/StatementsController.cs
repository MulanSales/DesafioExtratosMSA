using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.HttpResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Statements.API.Models;
using Statements.API.Services;

namespace Statements.API.Controllers
{
    [Produces("application/json")]
    [Route("api/statements")]
    [ApiController]
    public class StatementsController : ControllerBase
    {
        private readonly ILogger<StatementsController> logger;
        private readonly IReleasesService releasesService;
        private readonly IEstablishmentService establishmentService;
        private readonly IControllerMessages responseMessages;
        private readonly HttpResponseFormat httpResponseHelper;

        public StatementsController(ILogger<StatementsController> logger, IReleasesService releasesService, IEstablishmentService establishmentService, IControllerMessages responseMessages) {
            this.logger = logger;
            this.releasesService = releasesService;
            this.establishmentService = establishmentService;
            this.responseMessages = responseMessages;
            this.httpResponseHelper = new HttpResponseFormat();
        }

        /// <summary>
        /// Returns an array of statements
        /// </summary>
        /// <returns>An array of all statements</returns>
        /// <response code="200">Returns the array</response>
        /// <response code="404">If can't find any statement</response>    
        /// <response code="500">If an error in server side happens</response> 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Statement>>> Get()
        {
            List<Statement> statements;
            try {
                logger.LogInformation("Trying to get associated releases from database");
                List<Release> releases = await releasesService.GetAll();

                if (releases.Count == 0) {
                   string errorMessage = responseMessages.NotFound.Replace("$", "Lançamento");
                   logger.LogInformation("Error: " + errorMessage);
                   return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                var establishments = await establishmentService.GetAll();

                foreach (var release in releases)
                {
                   release.EstablishmentName = establishments.Find(e => e.Name == release.EstablishmentName).Type;
                };

                statements = releases.GroupBy(r => new { r.Date, r.EstablishmentName, r.PaymentMethod }).Select(x => {
                    var statement = new Statement();
                    var curr = x.First();
                    statement.Date = curr.Date;
                    statement.PaymentMethod = curr.PaymentMethod;
                    statement.Type = curr.EstablishmentName;
                    statement.TotalAmount = x.Sum(xa => xa.Amount);
                    return statement;
                }).ToList();

            } 
            catch (Exception ex) {
                logger.LogInformation("Exception: " + ex.Message);
                logger.LogTrace(ex.StackTrace);
                throw;
            }

            logger.LogInformation("Action GET for /api/statements returns 200");
            return Ok(statements);
        }
    }
}