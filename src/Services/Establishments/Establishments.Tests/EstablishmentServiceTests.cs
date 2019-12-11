using System;
using System.Collections.Generic;
using CommonLibrary.DataAccess.Abstractions;
using CommonLibrary.HttpResponse;
using Establishments.API.Controllers;
using Establishments.API.Models;
using Establishments.API.Services;
using Establishments.Tests.Abstractions;
using Establishments.Tests.Fixtures;
using Events;
using Establishments.Tests.Wrappers;
using Moq;
using Xunit;

namespace Establishments.Tests
{
    [CollectionDefinition("Establishment Service Tests")]
    public class EstablishmentServiceTests : ControllerTest, IClassFixture<DatabaseSettingsFixture>
    {
        private readonly IDatabaseConnectorSettings dbSettings;
        private readonly LoggerWrapper<EstablishmentsController> loggerWrapper;
        private readonly EstablishmentService establishmentService;
        private readonly ControllerMessages controllerMessages;
        private readonly EstablishmentsController establishmentsController;
        private readonly IRabbitConnector rabbitConnector;

        public EstablishmentServiceTests(DatabaseSettingsFixture dbFixture)
        {   
            // 0: Setting wrapper for logger
            loggerWrapper = new LoggerWrapper<EstablishmentsController>();
            rabbitConnector = new RabbitConnectorWrapper();

            // 1: Setting establishment service given db settings
            this.dbSettings = dbFixture.dbSettings;
            this.establishmentService = dbFixture.establishmentService; 

             // 2: Get controller messages
            this.controllerMessages = GetControllerMessagesProperties();

            // 3: Instantiate of Establishment Controller
            this.establishmentsController = new EstablishmentsController(loggerWrapper, establishmentService, controllerMessages, rabbitConnector);
        }

        [Fact(DisplayName = "Should returns 404 if there is no establishment on database")]
        public async void Get_StatusCode_404_Test() 
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Call GET Action
            var query = await establishmentsController.Get();

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.NotFound.Replace("$", "Estabelecimento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while GET")]
        public async void Get_ThrowsException_Test()
        {
            // 1: Mocking GetAll Method to throws
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetAll()).ThrowsAsync(new InvalidOperationException());

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            // 2: Call GET Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await establishmentsControllerLocal.Get());
        }

        [Fact(DisplayName = "Should return 200 and a list of establishments if successful")]
        public async void Get_SuccessStatus200_Test()
        {
             // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Creating testing objects
            Establishment firstEstablishment = new Establishment() 
            {
                Name = "Test 1",
                Type = "Alimentação"
            };

            Establishment secondEstablishment = new Establishment()
            {
                Name = "Test 2",
                Type = "Alimentação"
            };

            // 2: Adding to database
            await establishmentService.CreateItem(firstEstablishment);
            await establishmentService.CreateItem(secondEstablishment);

            var query = await establishmentsController.Get();

            var result = (List<Establishment>)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            // 3: Check if result is a valid Establishment
            Assert.Equal("Test 1", result[0].Name);
            Assert.Equal("Test 2", result[1].Name);
        }

        [Fact(DisplayName = "Should returns 406 if an establishment with given name already exists")]
        public async void Post_NotAccepted406_Test()
        {
            // 1: Creating testing objects
            Establishment testEstablishment = new Establishment() 
            {
                Name = "Test 1",
                Type = "Alimentação"
            };

            // 2: Adding to database synchronously
            await establishmentService.CreateItem(testEstablishment);

            // 3: Call POST Action passing body request with an establishment which already exists
            var query = await establishmentsController.Post(new EstablishmentRequest {
                Name = "Test 1",
                Type = "Alimentação"
            });

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(406, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.NotAccepted.Replace("$", "estabelecimento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while POST")]
        public async void Post_ThrowsException_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "Alimentação"
            };

            // 2: Mocking GetByName Method to throws
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetByName(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            // 3: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await establishmentsControllerLocal.Post(requestBody));
        }

        [Fact(DisplayName = "Should return 201 and the establishment created")]
        public async void Post_SuccessStatus201_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "alimentação"
            };

            // 2: Call POST Action passing body request with a new establishment
            var query = await establishmentsController.Post(requestBody);

            var resultStatusCode = query.Result.GetType().GetProperty("StatusCode").GetValue(query.Result);
            var resultValue = (Establishment)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(201, (int)resultStatusCode);
            Assert.Equal("Test 1", resultValue.Name);
            Assert.Equal("Alimentação", resultValue.Type);
        }

        [Theory(DisplayName="Should returns 400 if id is not in correct format")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456")]
        [InlineData("5dcaad2526235a471cfcccar")]
        public async void Put_InvalidId400_Test(string id) 
        {
            // 1: Request body
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "alimentação"
            };

            // 2: Call PUT Action passing body request with an updated establishment
            var query = await establishmentsController.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(400, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.IncorretIdFormat, (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while PUT")]
        public async void Put_ThrowsException_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "Alimentação"
            };

            // 2: Mocking GetByName Method to throws
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            // 3: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await establishmentsControllerLocal.Put(id, requestBody));
        }

        [Fact(DisplayName="Should return 404 if can't find any record with given Id on database")]
        public async void Put_Returns404_IdNotFound_Test()
        {
             // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body, given id is not found on database
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "Alimentação"
            };

            // 2: Call PUT Action passing body request with an updated establishment
            var query = await establishmentsController.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.NotFoundGivenId.Replace("$", "estabelecimento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should returns 406 if modification is not accepted by the database")]
        public async void Put_Returns406_NotAcknowledged_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "Alimentação"
            };

            // 2: Mocking GetById Method to return fake data
            var fakeEstablishment = new Establishment{
                Id = id,
                Name = requestBody.Name,
                Type = requestBody.Type
            };
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ReturnsAsync(fakeEstablishment);

            var replaceOneResultWrapper = new ReplaceOneResultWrapper();
            establishmentServiceMock.Setup(es => es.UpdateById(It.IsAny<string>(), It.IsAny<Establishment>())).ReturnsAsync(replaceOneResultWrapper);

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            var query = await establishmentsControllerLocal.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(406, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.CantUpdate, (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should return 200 and the updated establishment")]
        public async void Put_SuccessStatus200_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: POST Request body
            var postRequestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "alimentação"
            };

            // 2: Call POST Action passing body request with a new establishment
            var postQuery = await establishmentsController.Post(postRequestBody);

            var postResultValue = (Establishment)postQuery.Result.GetType().GetProperty("Value").GetValue(postQuery.Result);

            // 3: PUT Request body
            string id = postResultValue.Id;
            var requestBody = new EstablishmentRequest {
                Name = "Test 2",
                Type = "Alimentação"
            };

            var query = await establishmentsController.Put(id, requestBody);

            var statusCode = (int)query.Result.GetType().GetProperty("StatusCode").GetValue(query.Result);
            var result = (Establishment)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(200, statusCode);
            Assert.Equal("Test 2", result.Name);
        }

        [Theory(DisplayName="Should returns 400 if id is not in correct format while DELETE")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456")]
        [InlineData("5dcaad2526235a471cfcccar")]
        public async void Delete_InvalidId400_Test(string id) 
        {
            // 1: Call DELETE Action passing id of establishment to be deleted
            var query = await establishmentsController.Delete(id);

            ResponseDetails result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(controllerMessages.IncorretIdFormat, result.Message);
        }

        [Fact(DisplayName = "Should throws if any exception occurs while DELETE")]
        public async void Delete_ThrowsException_Test()
        {
            // 1: Request id
            string id = "5dcaad2526235a471cfcccad";

            // 2: Mocking GetByName Method to throws
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            // 3: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await establishmentsControllerLocal.Delete(id));
        }

        [Fact(DisplayName="Should return 404 if can't find any record with given Id on database while DELETE")]
        public async void Delete_Returns404_IdNotFound_Test()
        {
             // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body, given id is not found on database
            string id = "5dcaad2526235a471cfcccad";

            // 2: Call DELETE Action passing id request of establishment to be deleted
            var query = await establishmentsController.Delete(id);

            ResponseDetails result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(404, result.StatusCode);
            Assert.Equal(controllerMessages.NotFoundGivenId.Replace("$", "estabelecimento"), result.Message);
        }

        [Fact(DisplayName = "Should returns 406 if deletion is not accepted by the database")]
        public async void Delete_Returns406_NotAcknowledged_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";

            // 2: Mocking GetById Method to return fake data
            var fakeEstablishment = new Establishment{
                Id = id,
                Name = "Test 1",
                Type = "Tipo 1"
            };

            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ReturnsAsync(fakeEstablishment);

            var deleteResultWrapper = new DeleteResultWrapper();
            establishmentServiceMock.Setup(es => es.RemoveById(It.IsAny<string>())).ReturnsAsync(deleteResultWrapper);

            var establishmentsControllerLocal = new EstablishmentsController(loggerWrapper, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            var query = await establishmentsControllerLocal.Delete(id);

            var result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(406, result.StatusCode);
            Assert.Equal(controllerMessages.CantRemove, result.Message);
        }

        [Fact(DisplayName = "Should return 200 after establishment deletion")]
        public async void Delete_SuccessStatus200_Test()
        {
            // 1: POST Request body
            var postRequestBody = new EstablishmentRequest {
                Name = "Test 1",
                Type = "alimentação"
            };

            // 2: Call POST Action passing body request with a new establishment
            var postQuery = await establishmentsController.Post(postRequestBody);

            var postResultValue = (Establishment)postQuery.Result.GetType().GetProperty("Value").GetValue(postQuery.Result);

            // 3: DELETE given Id param
            string id = postResultValue.Id;

            var query = await establishmentsController.Delete(id);

            var result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(controllerMessages.DeletedSuccess.Replace("$", "estabelecimento"), result.Message);
        }
    }
}