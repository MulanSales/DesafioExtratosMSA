using EasyNetQ;
using Establishments.API.Models;
using Events;

namespace Establishments.API.Events
{
    public static class Emitter
    {
        public static async void EstablishmentCreated(Establishment result, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new EstablishmentCreatedEvent(
                        result.Id,
                        result.Name,
                        result.Type,
                        result.CreatedAt,
                        result.UpdatedAt
                    ));
                }
        }

        public static async void EstablishmentUpdated(Establishment result, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new EstablishmentUpdatedEvent(
                        result.Id,
                        result.Name,
                        result.Type,
                        result.CreatedAt,
                        result.UpdatedAt
                    ));
                }
            
        }

        public static async void EstablishmentDeleted(string id, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                    await bus.PublishAsync(new EstablishmentDeletedEvent(id));
                }
        }
    }
}