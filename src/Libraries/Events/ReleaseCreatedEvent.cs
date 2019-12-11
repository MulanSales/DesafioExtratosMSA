using System;

namespace Events
{
    public class ReleaseCreatedEvent
    {
        public ReleaseCreatedEvent(string id, string date, string paymentMethod, string establishmentName, decimal amount, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            Date = date;
            PaymentMethod = paymentMethod;
            EstablishmentName = establishmentName;
            Amount = amount;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public string Id { get; set; }

        public string Date { get; set; }

        public string PaymentMethod { get; set; }

        public string EstablishmentName { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
