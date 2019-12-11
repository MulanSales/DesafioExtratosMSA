namespace CommonLibrary.DataAccess.Abstractions {
    public interface IDatabaseConnectorSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        
    }
}