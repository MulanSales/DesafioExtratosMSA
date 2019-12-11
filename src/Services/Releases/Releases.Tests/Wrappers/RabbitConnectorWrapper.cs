using Events;

namespace Releases.Tests
{
    public class RabbitConnectorWrapper : IRabbitConnector
    {
        public string ConnectionString => string.Empty;
    }
}
