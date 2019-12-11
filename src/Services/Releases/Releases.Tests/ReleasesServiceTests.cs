using System;
using System.Collections.Generic;
using CommonLibrary.DataAccess.Abstractions;
using CommonLibrary.HttpResponse;
using Events;
using Moq;
using Releases.API.Controllers;
using Releases.API.Models;
using Releases.API.Services;
using Releases.Tests.Abstractions;
using Releases.Tests.Fixtures;
using Releases.Tests.Wrappers;
using Xunit;

namespace Releases.Tests
{
    [CollectionDefinition("Releases Service Tests")]
    public class ReleasesServiceTests : ControllerTest, IClassFixture<DatabaseSettingsFixture>
    {
        private readonly IDatabaseConnectorSettings dbSettings;
        private readonly LoggerWrapper<ReleasesController> loggerWrapper;
        private readonly IEstablishmentService establishmentService;
        private readonly IReleasesService releasesService;
        private readonly ControllerMessages controllerMessages;
        private readonly ReleasesController releasesController;
        private readonly IRabbitConnector rabbitConnector;

        public ReleasesServiceTests(DatabaseSettingsFixture dbFixture)
        {   
            // 0: Setting wrapper for logger
            loggerWrapper = new LoggerWrapper<ReleasesController>();
            rabbitConnector = new RabbitConnectorWrapper();

            // 1: Setting establishment and releases service given db settings
            this.dbSettings = dbFixture.dbSettings;
            this.establishmentService = dbFixture.establishmentService;
            this.releasesService = dbFixture.releasesService;

             // 2: Get controller messages
            this.controllerMessages = GetControllerMessagesProperties();

            // 3: Instantiate of Establishment Controller
            this.releasesController = new ReleasesController(loggerWrapper, releasesService, establishmentService, controllerMessages, rabbitConnector);
        }

        [Fact(DisplayName = "Should returns 404 if there is no releases on database")]
        public async void Get_StatusCode_404_Test() 
        {
            // 0: Remove all releases from database
            await releasesService.RemoveAll();

            // 1: Call GET Action
            var query = await releasesController.Get();

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.NotFound.Replace("$", "Lançamento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while GET")]
        public async void Get_ThrowsException_Test()
        {
            // 1: Mocking GetAll Method to throws
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetAll()).ThrowsAsync(new InvalidOperationException());

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object, establishmentService, controllerMessages, rabbitConnector);

            // 2: Call GET Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await releasesControllerLocal.Get());
        }

        [Fact(DisplayName = "Should return 200 and a list of releases if successful")]
        public async void Get_SuccessStatus200_Test()
        {
            // 1: Creating testing objects
            Release firstRelease = new Release() 
            {
                Id = "5dcaad2526235a471cfcccad",
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 2: Adding to database
            await releasesService.CreateItem(firstRelease);

            var query = await releasesController.Get();

            var result = (List<Release>)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            // 3: Remove all services from database
            await releasesService.RemoveAll();
            
            // 4: Check if result is a valid Release
            Assert.Equal("5dcaad2526235a471cfcccad", result[0].Id);
        }

        [Fact(DisplayName = "Should returns 404 if there is no establishment given name in request body")]
        public async void Post_NoEstablishment_StatusCode_404_Test()
        {
            // 0: Remove all establishments from database
            await establishmentService.RemoveAll();

            // 1: Creating testing objects
            ReleaseRequest testRelease = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Trying to POST a new release
            var query = await releasesController.Post(testRelease);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            // 3: Check if result is an error
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.CantFoundGivenName.Replace("$", "estabelecimento"), (string)resultType.GetProperty("Message").GetValue(result));
        } 

        [Fact(DisplayName = "Should throws if any exception occurs while POST")]
        public async void Post_ThrowsException_Test()
        {
            // 0: Creating testing objects
            ReleaseRequest testRelease = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 1: Mocking GetByName Method to throws
            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetByName(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesService, establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            // 2: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await releasesControllerLocal.Post(testRelease));
        }

        [Fact(DisplayName = "Should return 201 and the created release")]
        public async void Post_SuccessStatus201_Test()
        {
            // 1: Add Existing establishment to validate name param on request body
            await establishmentService.CreateItem(new Establishment() {
                Name = "Test 1",
                Type = "Alimentação"
            });

            // 2: Request body
            ReleaseRequest requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 3: Call POST Action passing body request with a new release
            var query = await releasesController.Post(requestBody);

            var resultStatusCode = query.Result.GetType().GetProperty("StatusCode").GetValue(query.Result);
            var resultValue = (Release)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            // 4: Remove all establishments and releases from database
            await establishmentService.RemoveAll();
            await releasesService.RemoveAll();

            Assert.Equal(201, (int)resultStatusCode);
            Assert.Equal("Test 1", resultValue.EstablishmentName);
            Assert.NotNull(resultValue.Id);
        }

        [Theory(DisplayName="Should returns 400 if id is not in correct format")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456")]
        [InlineData("5dcaad2526235a471cfcccar")]
        public async void Put_InvalidId400_Test(string id) 
        {
            // 1: Request body
            var requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Call PUT Action passing body request with an updated release 
            var query = await releasesController.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(400, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.IncorretIdFormat, (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while PUT")]
        public async void Put_ThrowsException_Test()
        {
            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Mocking GetByName Method to throws
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object,establishmentService, controllerMessages, rabbitConnector);

            // 3: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await releasesControllerLocal.Put(id, requestBody));
        }

        [Fact(DisplayName="Should return 404 if can't find any record with given Id on database")]
        public async void Put_Returns404_IdNotFound_Test()
        {
            // 1: Request body, given id is not found on database
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Call PUT Action passing body request with an updated release
            var query = await releasesController.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.NotFoundGivenId.Replace("$", "lançamento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName="Should return 404 if can't find any establishment on database")]
        public async void Put_Returns404_EstablishmentNotFound_Test()
        {
            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Mocking GetById to return a fake release object
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ReturnsAsync(new Release() {
                Date = requestBody.Date,
                PaymentMethod = requestBody.PaymentMethod,
                EstablishmentName =requestBody.EstablishmentName,
                Amount = requestBody.Amount,
                CreatedAt = DateTime.Now
            });

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object,establishmentService, controllerMessages, rabbitConnector);
            
            var query = await releasesControllerLocal.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(404, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.CantFoundGivenName.Replace("$", "estabelecimento"), (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should returns 406 if modification is not accepted by the database")]
        public async void Put_Returns406_NotAcknowledged_Test()
        {
            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";
            var requestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            // 2: Mocking GetById Method to return fake data
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ReturnsAsync(new Release() {
                Date = requestBody.Date,
                PaymentMethod = requestBody.PaymentMethod,
                EstablishmentName =requestBody.EstablishmentName,
                Amount = requestBody.Amount,
                CreatedAt = DateTime.Now
            });

            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetByName(It.IsAny<string>())).ReturnsAsync(new Establishment(){
                Name = "Test 1",
                Type = "Alimentação"
            });

            var replaceOneResultWrapper = new ReplaceOneResultWrapper();
            releasesServiceMock.Setup(es => es.UpdateById(It.IsAny<string>(), It.IsAny<Release>())).ReturnsAsync(replaceOneResultWrapper);

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object,establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            var query = await releasesControllerLocal.Put(id, requestBody);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(406, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.CantUpdate, (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should return 200 and the updated release")]
        public async void Put_SuccessStatus200_Test()
        {
            // 1: POST Request body
            var postRequestBody = new ReleaseRequest() 
            {
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetByName(It.IsAny<string>())).ReturnsAsync(
            new Establishment()
            {
                Name = "Test 1",
                Type = "Alimentação"
            });

            // 2: Call POST Action passing body request with a new release
            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesService,establishmentServiceMock.Object, controllerMessages, rabbitConnector);

            var postQuery = await releasesControllerLocal.Post(postRequestBody);

            var postResultValue = (Release)postQuery.Result.GetType().GetProperty("Value").GetValue(postQuery.Result);

            // 3: PUT Request body
            string id = postResultValue.Id;
            var requestBody = new ReleaseRequest {
                Date = "06/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m
            };

            var query = await releasesControllerLocal.Put(id, requestBody);

            var statusCode = (int)query.Result.GetType().GetProperty("StatusCode").GetValue(query.Result);
            var result = (Release)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(200, statusCode);
            Assert.Equal("06/05/2019", result.Date);
        }

        [Theory(DisplayName="Should returns 400 if id is not in correct format while DELETE")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456")]
        [InlineData("5dcaad2526235a471cfcccar")]
        public async void Delete_InvalidId400_Test(string id) 
        {
            // 1: Call PUT Action passing body request with an updated release 
            var query = await releasesController.Delete(id);

            var result = query.Result.GetType().GetProperty("Value").GetValue(query.Result);
            Type resultType = result.GetType();

            Assert.Equal(400, (int)resultType.GetProperty("StatusCode").GetValue(result));
            Assert.Equal(controllerMessages.IncorretIdFormat, (string)resultType.GetProperty("Message").GetValue(result));
        }

        [Fact(DisplayName = "Should throws if any exception occurs while DELETE")]
        public async void Delete_ThrowsException_Test()
        {
            // 1: Request id
            string id = "5dcaad2526235a471cfcccad";

            // 2: Mocking GetByName Method to throws
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object,establishmentService, controllerMessages, rabbitConnector);

            // 3: Call POST Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await releasesControllerLocal.Delete(id));
        }

        [Fact(DisplayName="Should return 404 if can't find any record with given Id on database while DELETE")]
        public async void Delete_Returns404_IdNotFound_Test()
        {
            await releasesService.RemoveAll();

            // 1: Request body, given id is not found on database
            string id = "5dcaad2526235a471cfcccad";

            // 2: Call DELETE Action passing id request of release to be deleted
            var query = await releasesController.Delete(id);

            ResponseDetails result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(404, result.StatusCode);
            Assert.Equal(controllerMessages.NotFoundGivenId.Replace("$", "lançamento"), result.Message);
        }

        [Fact(DisplayName = "Should returns 406 if deletion is not accepted by the database")]
        public async void Delete_Returns406_NotAcknowledged_Test()
        {
            // 1: Request body
            string id = "5dcaad2526235a471cfcccad";

            // 2: Mocking GetById Method to return fake data
            var fakeRelease = new Release{
                Id = id,
                Date = "06/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(es => es.GetById(It.IsAny<string>())).ReturnsAsync(fakeRelease);

            var deleteResultWrapper = new DeleteResultWrapper();
            releasesServiceMock.Setup(es => es.RemoveById(It.IsAny<string>())).ReturnsAsync(deleteResultWrapper);

            var releasesControllerLocal = new ReleasesController(loggerWrapper, releasesServiceMock.Object ,establishmentService, controllerMessages, rabbitConnector);

            var query = await releasesControllerLocal.Delete(id);

            var result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(406, result.StatusCode);
            Assert.Equal(controllerMessages.CantRemove, result.Message);
        }

        [Fact(DisplayName = "Should return 200 after establishment deletion")]
        public async void Delete_SuccessStatus200_Test()
        {
            // 1: Create test release
            string id = "5dcaad2526235a471cfcccad";
            await releasesService.CreateItem(new Release{
                Id = id,
                Date = "06/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 1",
                Amount = 55.55m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            // 2: DELETE given Id param
            var query = await releasesController.Delete(id);

            var result = (ResponseDetails)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(controllerMessages.DeletedSuccess.Replace("$", "lançamento"), result.Message);
        }
    }
}