using EasyNetQ;
using Events;
using Releases.API.Models;

namespace Releases.API.Events
{
    public static class Emitter
    {
        public static async void ReleaseCreated(Release result, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new ReleaseCreatedEvent(
                        result.Id,
                        result.Date,
                        result.PaymentMethod.ToString(),
                        result.EstablishmentName,
                        result.Amount,
                        result.CreatedAt,
                        result.UpdatedAt
                    ));
                }
        }

        public static async void ReleaseUpdated(Release result, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new ReleaseUpdatedEvent(
                        result.Id,
                        result.Date,
                        result.PaymentMethod.ToString(),
                        result.EstablishmentName,
                        result.Amount,
                        result.CreatedAt,
                        result.UpdatedAt
                    ));
                }
        }

        public static async void ReleaseDeleted(string id, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new ReleaseDeletedEvent(id));
                }
        }
    }
}