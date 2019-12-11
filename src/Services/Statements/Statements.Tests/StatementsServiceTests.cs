using System;
using System.Collections.Generic;
using CommonLibrary.DataAccess.Abstractions;
using Events;
using Moq;
using Statements.API.Controllers;
using Statements.API.Models;
using Statements.API.Services;
using Statements.Tests.Abstractions;
using Statements.Tests.Fixtures;
using Statements.Tests.Wrappers;
using Xunit;

namespace Statements.Tests
{
    [CollectionDefinition("Statements Service Tests")]
    public class StatementsServiceTests : ControllerTest, IClassFixture<DatabaseSettingsFixture>
    {
        private readonly IDatabaseConnectorSettings dbSettings;
        private readonly LoggerWrapper<StatementsController> loggerWrapper;
        private readonly IEstablishmentService establishmentService;
        private readonly IReleasesService releasesService;
        private readonly IControllerMessages controllerMessages;
        private readonly StatementsController statementsController;
        private readonly IRabbitConnector rabbitConnector;

        public StatementsServiceTests(DatabaseSettingsFixture dbFixture)
        {   
            // 0: Setting wrapper for logger
            loggerWrapper = new LoggerWrapper<StatementsController>();
            rabbitConnector = new RabbitConnectorWrapper();

            // 1: Setting establishment and releases service given db settings
            this.dbSettings = dbFixture.dbSettings;
            this.establishmentService = dbFixture.establishmentService;
            this.releasesService = dbFixture.releasesService;

             // 2: Get controller messages
            this.controllerMessages = GetControllerMessagesProperties();

            // 3: Instantiate of Establishment Controller
            this.statementsController= new StatementsController(loggerWrapper, releasesService, establishmentService, controllerMessages);
        }

        [Fact(DisplayName = "Should returns 404 if there is no releases on database")]
        public async void Get_NoReleasesFound404_Test() 
        {
            // 1: Call GET Action
            var query = await statementsController.Get();

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

            var statementsControllerLocal = new StatementsController(loggerWrapper, releasesServiceMock.Object, establishmentService, controllerMessages);

            // 2: Call GET Action and Expects to throws
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await statementsControllerLocal.Get());
        }

 
        [Fact(DisplayName = "Should return 200 and a list of statements if successful")]
        public async void Get_SuccessStatus200_Test()
        {
            // 0: Create releases and add to list
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

            Release secondRelease = new Release() 
            {
                Id = "5dcaad2526235a471cfcccac",
                Date = "05/05/2019",
                PaymentMethod = PaymentMethod.Credito,
                EstablishmentName = "Test 2",
                Amount = 55.55m,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            List<Release> releaseList = new List<Release>();
            releaseList.Add(firstRelease);
            releaseList.Add(secondRelease);

            // 1: Create establishments and add to list
            Establishment firstEstablishment = new Establishment() 
            {
                Id = "4dcaad2526235a471cfcccac",
                Name = "Test 1",
                Type = "Alimentação"
            };

            Establishment secondEstablishment = new Establishment()
            {
                Id = "4dcaad2526235a471cfcccaa",
                Name = "Test 2",
                Type = "Alimentação"
            };

            List<Establishment> establishmentList = new List<Establishment>();
            establishmentList.Add(firstEstablishment);
            establishmentList.Add(secondEstablishment);

            // 1: Mocking GetAll Method to return fake objects
            var releasesServiceMock = new Mock<ReleasesService>(dbSettings);
            releasesServiceMock.Setup(rs => rs.GetAll()).ReturnsAsync(releaseList);

            var establishmentServiceMock = new Mock<EstablishmentService>(dbSettings);
            establishmentServiceMock.Setup(es => es.GetAll()).ReturnsAsync(establishmentList);

            var statementsControllerLocal = new StatementsController(loggerWrapper, releasesServiceMock.Object, establishmentServiceMock.Object, controllerMessages);

            // 2: Call GET
            var query = await statementsControllerLocal.Get();

            var resultStatusCode = (int)query.Result.GetType().GetProperty("StatusCode").GetValue(query.Result);
            var resultValue = (List<Statement>)query.Result.GetType().GetProperty("Value").GetValue(query.Result);

            Assert.Equal(200, resultStatusCode);
            Assert.Equal(111.10M, resultValue[0].TotalAmount);

        }
    }
}