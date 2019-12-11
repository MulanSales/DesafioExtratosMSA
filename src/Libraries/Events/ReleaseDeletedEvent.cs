using System;

namespace Events
{
    public class ReleaseDeletedEvent 
    {
        public ReleaseDeletedEvent(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

    }
}
