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
    public class UpdateEstablishmentEventHandler : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IEstablishmentService establishmentService;
        private readonly IRabbitConnector rabbitConnector;
        private IBus bus;

        public  UpdateEstablishmentEventHandler(IConfiguration configuration, IEstablishmentService establishmentService, IRabbitConnector rabbitConnector)
        {
            this.configuration = configuration;
            this.establishmentService = establishmentService;
            this.rabbitConnector = rabbitConnector;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.bus = RabbitHutch.CreateBus(rabbitConnector.ConnectionString);
            this.bus.Subscribe<EstablishmentUpdatedEvent>("StatementsGateway", UpdateEstablishment);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            this.bus.Dispose();
        }

        private async void UpdateEstablishment(EstablishmentUpdatedEvent establishment)
        {
            var updatedEstablishment = new Establishment()
            {
                Id = establishment.Id,
                Name = establishment.Name,
                Type = establishment.Type,
                CreatedAt = establishment.CreatedAt,
                UpdatedAt = establishment.UpdatedAt
            };

            await establishmentService.UpdateById(updatedEstablishment.Id, updatedEstablishment);
        }
    }
}