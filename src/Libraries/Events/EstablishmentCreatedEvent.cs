using System;

namespace Events
{
    public class EstablishmentCreatedEvent
    {
        public EstablishmentCreatedEvent(string id, string name, string type, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            Name = name;
            Type = type;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
