using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using CommonLibrary.HttpResponse;
using CommonLibrary.StringFormat;
using Establishments.API.Models;
using Establishments.API.Services;
using Events;
using Establishments.API.Events;

namespace Establishments.API.Controllers
{
    [Produces("application/json")]
    [Route("api/establishments")]
    [ApiController]
    public class EstablishmentsController: ControllerBase
    {
        private readonly ILogger<EstablishmentsController> logger;
        private readonly IEstablishmentService establishmentService;
        private readonly IControllerMessages responseMessages;
        private readonly IRabbitConnector rabbitConnector;
        private readonly HttpResponseFormat httpResponseHelper;
        public EstablishmentsController(ILogger<EstablishmentsController> logger, IEstablishmentService establishmentService, IControllerMessages responseMessages, IRabbitConnector rabbitConnector) {
            this.logger = logger;
            this.establishmentService = establishmentService;
            this.responseMessages = responseMessages;
            this.rabbitConnector = rabbitConnector;
            this.httpResponseHelper = new HttpResponseFormat();
        }

        /// <summary>
        /// Returns an array of establishments
        /// </summary>
        /// <returns>An array of all establishments inserted in the past</returns>
        /// <response code="200">Returns the array</response>
        /// <response code="404">If can't find any establishment</response>    
        /// <response code="500">If an error in server side happens</response> 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Establishment>>> Get()
        {
            List<Establishment> establishments;
            try {
                logger.LogInformation("Trying to get establishments from database");
                establishments = await establishmentService.GetAll();

                if (establishments.Count == 0) {
                   string errorMessage = responseMessages.NotFound.Replace("$", "Estabelecimento");
                   logger.LogInformation("Error: " + errorMessage);
                   return httpResponseHelper.ErrorResponse(errorMessage, 404);
               }
            } 
            catch (Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action GET for /api/establishments returns 200");
            return Ok(establishments);
        }
        
        /// <summary>
        /// Creates a new establishment 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/establishments
        ///     {
        ///        "name": "Padaria Stn",
        ///        "type": "Alimentação"
        ///     }
        ///
        /// </remarks>
        /// <returns>The newly release created</returns>
        /// <response code="201">Returns the newly created establishment</response>
        /// <response code="400">If the request is not in correct format</response>    
        /// <response code="500">If an error in server side happens</response>    
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Establishment>> Post([FromBody] EstablishmentRequest body)
        {
            Establishment resultEstablishment;
            try {
                logger.LogInformation("Trying to verify if establishment with given Name exists");
                var establishment = await establishmentService.GetByName(body.Name.FirstCharToUpper());

                if (establishment != null) {
                    string errorMessage = responseMessages.NotAccepted.Replace("$", "estabelecimento"); 
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 406);
                }

                logger.LogInformation("Inserting establishment into database");
                var newEstablishment = new Establishment() {
                    Name = body.Name.FirstCharToUpper(),                       
                    Type = body.Type.FirstCharToUpper(),
                    CreatedAt = DateTime.Now
                };
                
                resultEstablishment = await establishmentService.CreateItem(newEstablishment);

                //Send to RabbitMQ to event handler observer pattern
                Emitter.EstablishmentCreated(resultEstablishment, rabbitConnector.ConnectionString);

            } catch(Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action POST for /api/establishments returns 201");
            return Created("", resultEstablishment);
        }

        /// <summary>
        /// Updates an old establishment 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/establishments/5dcaad2526235a471cfcccaf
        ///     {
        ///        "name": "Padaria Nova Stn",
        ///        "type": "Alimentação"
        ///     }
        ///
        /// </remarks>
        /// <returns>The updated establishment</returns>
        /// <response code="200">Returns the updated establishment</response>
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
        public async Task<ActionResult<Establishment>> Put(string id, [FromBody] EstablishmentRequest body)
        {
            // Validating id
            if (id == null || !Regex.IsMatch(id, "^[0-9a-fA-F]{24}$")) {
                string errorMessage = responseMessages.IncorretIdFormat;
                logger.LogInformation("Error: " + errorMessage);
                return httpResponseHelper.ErrorResponse(errorMessage, 400);
            }

            Establishment updatedEstablishment;
            try 
            {
                logger.LogInformation("Trying to get a establishemnt with given id");
                var actualEstablishment = await establishmentService.GetById(id);

                if (actualEstablishment == null) {
                    string errorMessage = responseMessages.NotFoundGivenId.Replace("$", "estabelecimento") ;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                updatedEstablishment = new Establishment() {
                    Id = id,
                    Name = body.Name.FirstCharToUpper(),
                    Type = body.Type.FirstCharToUpper(),
                    CreatedAt = actualEstablishment.CreatedAt,
                    UpdatedAt = DateTime.Now
                };

                logger.LogInformation("Trying to update establishment with id: " + id);
                var replaceResult = await establishmentService.UpdateById(id, updatedEstablishment);

                if (!replaceResult.IsAcknowledged) {
                    string errorMessage = responseMessages.CantUpdate; 
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 406);
                }

                //Send to RabbitMQ to event handler observer pattern
                Emitter.EstablishmentUpdated(updatedEstablishment, rabbitConnector.ConnectionString);
            } 
            catch (Exception ex) 
            {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action PUT for /api/establishments returns 200");
            return Ok(updatedEstablishment);
        }

        /// <summary>
        /// Deletes an establishment 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/establishments/5dcaad2526235a471cfcccaf
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

            try 
            {
                logger.LogInformation("Trying to get a establishment with given id");
                var actualEstablishment = await establishmentService.GetById(id);

                if (actualEstablishment == null) {
                    string errorMessage = responseMessages.NotFoundGivenId.Replace("$", "estabelecimento");
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 404);
                }

                var deleteResult = await establishmentService.RemoveById(id);
                if(!deleteResult.IsAcknowledged) {
                    string errorMessage = responseMessages.CantRemove;
                    logger.LogInformation("Error: " + errorMessage);
                    return httpResponseHelper.ErrorResponse(errorMessage, 406);
                }

                //Send to RabbitMQ to event handler observer pattern
                Emitter.EstablishmentDeleted(id, rabbitConnector.ConnectionString);

            } catch(Exception ex) {
                logger.LogInformation($"Message: {ex.Message}");
                logger.LogTrace($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            logger.LogInformation("Action DELETE for /api/establishments returns 200");
            return Ok(new ResponseDetails() {Message = responseMessages.DeletedSuccess.Replace("$", "estabelecimento"), StatusCode = 200});
        }
    }
}
