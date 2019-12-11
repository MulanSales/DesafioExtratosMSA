using Statements.API.Models;

namespace Statements.Tests.Abstractions 
{
    public abstract class ControllerTest 
    {
        protected ControllerTest() {}
        protected ControllerMessages GetControllerMessagesProperties() 
        {
            return new ControllerMessages() 
            {
                NotFound = "Não foi possível encontrar nenhum $ no banco de dados.",
            };
        }
    }
}