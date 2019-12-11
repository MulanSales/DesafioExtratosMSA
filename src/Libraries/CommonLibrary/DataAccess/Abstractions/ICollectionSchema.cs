using System;

namespace CommonLibrary.DataAccess.Abstractions {
    public interface ICollectionSchema 
    {
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt{ get; set; }
    }
}