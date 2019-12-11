using System;
using CommonLibrary.DataAccess.Abstractions;
using Establishments.API.Services;
using Moq;

namespace Establishments.Tests.Fixtures
{
    public class DatabaseSettingsFixture : IDisposable
    {
        public IDatabaseConnectorSettings dbSettings { get; private set; }
        public EstablishmentService establishmentService;
        public DatabaseSettingsFixture()
        {
            var mockDatabaseSettings = new Mock<IDatabaseConnectorSettings>();
            mockDatabaseSettings.SetupGet(mi => mi.DatabaseName).Returns("extratos-api-establishmentDB-test");
            mockDatabaseSettings.SetupGet(mi => mi.ConnectionString).Returns("mongodb+srv://rw-permission-user:CgMedcmoNYcJCaYJ@nodecourse-tnt0c.mongodb.net/?ssl=true&authSource=admin&w=majority");
            mockDatabaseSettings.SetupGet(mi => mi.CollectionName).Returns("");

            this.dbSettings = mockDatabaseSettings.Object;
            this.establishmentService = new EstablishmentService(dbSettings);
        }
        public async void Dispose()
        {
            await establishmentService.RemoveAll();
        }
    }
}