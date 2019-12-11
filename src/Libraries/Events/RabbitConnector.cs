using System;
using Microsoft.Extensions.Configuration;

namespace Events
{
    public class RabbitConnector : IRabbitConnector
    {
        private readonly IConfiguration Configuration;
        public string ConnectionString { get; private set; }
        public RabbitConnector(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.set();
        }

        private void set() 
        {
            string rabbitConnectionString = Environment.GetEnvironmentVariable("RABBITCONNECTION");
            if (!string.IsNullOrEmpty(rabbitConnectionString))
            {
                ConnectionString = rabbitConnectionString;
            }
            else
            {
                IConfigurationSection rabbitSettings = this.Configuration.GetSection($"{nameof(RabbitConnector)}:{nameof(ConnectionString)}");
                ConnectionString = rabbitSettings.Value;
            }
        }   
    }
}