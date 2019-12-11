using System;

namespace Events
{
    public class EstablishmentDeletedEvent
    {
        public EstablishmentDeletedEvent(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

    }
}
