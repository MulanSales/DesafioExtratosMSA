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
    public class UpdateReleaseEventHandler : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IReleasesService releaseService;
        private readonly IRabbitConnector rabbitConnector;
        private IBus bus;

        public UpdateReleaseEventHandler(IConfiguration configuration, IReleasesService releaseService, IRabbitConnector rabbitConnector)
        {
            this.configuration = configuration;
            this.releaseService = releaseService;
            this.rabbitConnector = rabbitConnector;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.bus = RabbitHutch.CreateBus(rabbitConnector.ConnectionString);
            this.bus.Subscribe<ReleaseUpdatedEvent>("StatementsGateway", UpdateRelease);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            this.bus.Dispose();
        }

        private async void UpdateRelease(ReleaseUpdatedEvent release)
        {
            var newRelease = new Release()
            {
               Id = release.Id,
                Date = release.Date,
                PaymentMethod = (PaymentMethod)Enum.Parse(typeof(PaymentMethod), release.PaymentMethod),
                EstablishmentName = release.EstablishmentName,
                Amount = release.Amount,
                CreatedAt = release.CreatedAt,
                UpdatedAt = release.UpdatedAt
            };

            await releaseService.UpdateById(newRelease.Id, newRelease);
        }
    }
}