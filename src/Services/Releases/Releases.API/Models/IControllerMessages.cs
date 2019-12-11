namespace Releases.API.Models 
{
    public interface IControllerMessages 
    {
        string NotFound { get; set; }
        string NotAccepted { get; set; }
        string IncorretIdFormat { get; set; }
        string NotFoundGivenId { get; set; }
        string CantUpdate{ get; set; }
        string CantRemove { get; set; }
        string CantFoundGivenName { get; set; }
        string DeletedSuccess { get; set; }
    }
}