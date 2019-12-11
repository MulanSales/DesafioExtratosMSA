using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using CommonLibrary.HttpResponse;
using Releases.API.Models;
using Releases.API.Services;
using Releases.API.Events;
using Events;

namespace Releases.API.Controllers
{
    [Produces("application/json")]
    [Route("api/releases")]
    [ApiController]
    public class ReleasesController: ControllerBase
    {
        private readonly ILogger<ReleasesController> logger;
        private readonly IReleasesService releasesService;
        private readonly IEstablishmentService establishmentService;
        private readonly IControllerMessages responseMessages;
        private readonly IRabbitConnector rabbitConnector;
        private readonly HttpResponseFormat httpResponseHelper;
        public ReleasesController(ILogger<ReleasesController> logger, IReleasesService releasesService, IEstablishmentService establishmentService, IControllerMessages responseMessages, IRabbitConnector rabbitConnector) {
            this.logger = logger;
            this.releasesService = releasesService;
            this.establishmentService = establishmentService;
            this.responseMessages = responseMessages;
            this.rabbitConnector = rabbitConnector;
            this.httpResponseHelper = new HttpResponseFormat();
        }

        /// <summary>
        /// Returns an array of releases
        /// </summary>
        /// <returns>An array of all releases inserted in the past</returns>
        /// <response code="200">Returns the array</response>
        /// <response code="404">If can't find any release</response>    
        /// <response code="500">If an error in server side happens</response> 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Release>>> Get()
        {
            List<Release> releases;
            try {
                logger.LogInformation("Trying to get releases from database");
                releases = await releasesService.GetAll();

                if (releases.Count == 0) {
                   string errorMessage = responseMessages.NotFound.Replace("$", "Lançamento");
                   logger.LogInformation("Error: " + errorMessage);
                   return httpResponseHelper.ErrorResponse(errorMessage, 404);
               }
            } 
            catch (Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action GET for /api/releases returns 200");
            return Ok(releases);
        }

        /// <summary>
        /// Creates a new release
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/releases
        ///     {
        ///        "date": "05/05/2019",
        ///        "paymentMethod": "Credito",
        ///        "establishmentName": "Padaria Stn",
        ///        "amount": 34.88
        ///     }
        ///
        /// </remarks>
        /// <returns>The newly release created</returns>
        /// <response code="201">Returns the newly created release</response>
        /// <response code="400">If the request is not in correct format</response>    
        /// <response code="404">If the resource is not in the database</response>    
        /// <response code="500">If an error in server side happens</response>    
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Release>> Post([FromBody] ReleaseRequest body)
        {
            Release resultRelease;
            try {

                logger.LogInformation("Trying to get associated establishment");
                var establishment = await establishmentService.GetByName(body.EstablishmentName);

                if (establishment == null) {
                    string errorMessage = responseMessages.CantFoundGivenName.Replace("$", "estabelecimento") ;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                logger.LogInformation("Inserting release into database");
                var newRelease = new Release() {
                    Date = body.Date,
                    PaymentMethod = body.PaymentMethod,
                    Amount = body.Amount,
                    EstablishmentName = establishment.Name,
                    CreatedAt = DateTime.Now
                };
                
                resultRelease = await releasesService.CreateItem(newRelease);

                //Send to RabbitMQ to event handler observer pattern
                Emitter.ReleaseCreated(resultRelease, rabbitConnector.ConnectionString);

            } catch(Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action POST for /api/releases returns 201");
            return Created("", resultRelease);
        }

        /// <summary>
        /// Updates an old release
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/releases/5dcaad2526235a471cfcccaf
        ///     {
        ///        "date": "06/05/2019",
        ///        "paymentMethod": "Credito",
        ///        "establishmentName": "Padaria Stn",
        ///        "amount": 56.88
        ///     }
        ///
        /// </remarks>
        /// <returns>The updated release</returns>
        /// <response code="200">Returns the updated release</response>
        /// <response code="400">If the request is not in correct format</response>    
        /// <response code="404">If the resource is not in the database</response>    
        /// <response code="406">If the resource is not acceptable</response>    
        /// <response code="500">If an error in server side happens</response>    
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Release>> Put(string id, [FromBody] ReleaseRequest body)
        {
            // Validating id
            if (id == null || !Regex.IsMatch(id, "^[0-9a-fA-F]{24}$")) {
                string errorMessage = responseMessages.IncorretIdFormat;
                logger.LogInformation("Error: " + errorMessage);
                return httpResponseHelper.ErrorResponse(errorMessage, 400);
            }

            Release updatedRelease;
            try {
                logger.LogInformation("Trying to get a release with given id");
                var actualRelease = await releasesService.GetById(id);

                if (actualRelease == null) {
                    string errorMessage = responseMessages.NotFoundGivenId.Replace("$", "lançamento") ;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);;
                }

                logger.LogInformation("Trying to get associated establishment");
                var establishment = await establishmentService.GetByName(body.EstablishmentName);

                if (establishment == null) {
                    string errorMessage = responseMessages.CantFoundGivenName.Replace("$", "estabelecimento") ;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                updatedRelease = new Release() {
                    Id = id,
                    Date = body.Date,
                    PaymentMethod = body.PaymentMethod,
                    Amount = body.Amount,
                    EstablishmentName = establishment.Name,
                    CreatedAt = actualRelease.CreatedAt,
                    UpdatedAt = DateTime.Now
                };

                logger.LogInformation("Trying to update release with id: " + id);
                var replaceResult = await releasesService.UpdateById(id, updatedRelease);

                if (!replaceResult.IsAcknowledged) {
                    string errorMessage = responseMessages.CantUpdate; 
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 406);
                }

                 //Send to RabbitMQ to event handler observer pattern
                Emitter.ReleaseUpdated(updatedRelease, rabbitConnector.ConnectionString);

            } catch (Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action PUT for /api/releases returns 200");
            return Ok(updatedRelease);
        }

        /// <summary>
        /// Deletes a release
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/releases/5dcaad2526235a471cfcccaf
        ///
        /// </remarks>
        /// <returns>The response message with success</returns>
        /// <response code="200">Returns sucess response message</response>
        /// <response code="400">If the request is not in correct format</response>    
        /// <response code="404">If the resource is not in the database</response>    
        /// <response code="406">If the resource is not acceptable</response>    
        /// <response code="500">If an error in server side happens</response>    
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDetails>> Delete(string id)
        {
            if (id == null || !Regex.IsMatch(id, "^[0-9a-fA-F]{24}$")) {
                string errorMessage = responseMessages.IncorretIdFormat;
                logger.LogInformation("Error: " + errorMessage);
                return httpResponseHelper.ErrorResponse(errorMessage, 400);
            }

            try {
                logger.LogInformation("Trying to get a release with given id");
                var actualRelease = await releasesService.GetById(id);

                if (actualRelease == null) {
                    string errorMessage = responseMessages.NotFoundGivenId.Replace("$", "lançamento");
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                var deleteResult = await releasesService.RemoveById(id);
                if(!deleteResult.IsAcknowledged) {
                    string errorMessage = responseMessages.CantRemove;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 406);
                }

                //Send to RabbitMQ to event handler observer pattern
                Emitter.ReleaseDeleted(id, rabbitConnector.ConnectionString);

            } catch(Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action DELETE for /api/releases returns 200");
            return Ok(new ResponseDetails() { Message = responseMessages.DeletedSuccess.Replace("$", "lançamento"), StatusCode = 200 });
        }
    }
}
