using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Releases.API.Models;
using Releases.API.Services;

namespace Releases.API.EventsHandlers
{
    public class DeleteEstablishmentEventHandler : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IEstablishmentService establishmentService;
        private readonly IRabbitConnector rabbitConnector;
        private IBus bus;

        public DeleteEstablishmentEventHandler(IConfiguration configuration, IEstablishmentService establishmentService, IRabbitConnector rabbitConnector)
        {
            this.configuration = configuration;
            this.establishmentService = establishmentService;
            this.rabbitConnector = rabbitConnector;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.bus = RabbitHutch.CreateBus(rabbitConnector.ConnectionString);
            this.bus.Subscribe<EstablishmentDeletedEvent>("ReleasesGateway", DeleteEstablishment);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            this.bus.Dispose();
        }

        private async void DeleteEstablishment(EstablishmentDeletedEvent establishment)
        {
            var updatedEstablishment = new Establishment()
            {
                Id = establishment.Id
            };

            await establishmentService.RemoveById(updatedEstablishment.Id);
        }
    }
}