using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Statements.API.Models;
using Statements.API.Services;

namespace Statements.API.EventsHandlers
{
    public class DeleteReleaseEventHandler : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IReleasesService releaseService;
        private readonly IRabbitConnector rabbitConnector;
        private IBus bus;

        public DeleteReleaseEventHandler(IConfiguration configuration, IReleasesService releaseService, IRabbitConnector rabbitConnector)
        {
            this.configuration = configuration;
            this.releaseService = releaseService;
            this.rabbitConnector = rabbitConnector;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.bus = RabbitHutch.CreateBus(rabbitConnector.ConnectionString);
            this.bus.Subscribe<ReleaseDeletedEvent>("StatementsGateway", DeleteRelease);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            this.bus.Dispose();
        }

        private async void DeleteRelease(ReleaseDeletedEvent release)
        {
            var updatedEstablishment = new Establishment()
            {
                Id = release.Id
            };

            await releaseService.RemoveById(updatedEstablishment.Id);
        }
    }
}