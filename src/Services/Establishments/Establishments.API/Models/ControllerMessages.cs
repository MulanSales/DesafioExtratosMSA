namespace Establishments.API.Models 
{
    public class ControllerMessages : IControllerMessages
    {
        public string NotFound { get; set; }
        public string NotAccepted { get; set; }
        public string IncorretIdFormat { get; set; }
        public string NotFoundGivenId { get; set; }
        public string CantUpdate{ get; set; }
        public string CantRemove { get; set; }
        public string DeletedSuccess { get; set; }
    }
}