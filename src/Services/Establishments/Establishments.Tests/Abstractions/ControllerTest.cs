using Establishments.API.Models;

namespace Establishments.Tests.Abstractions 
{
    public abstract class ControllerTest 
    {
        protected ControllerTest() {}
        protected ControllerMessages GetControllerMessagesProperties() 
        {
            return new ControllerMessages() 
            {
                NotFound = "Não foi possível encontrar nenhum $ no banco de dados.",
                NotAccepted = "Não é permitido inserir $, pois já existe um $ cadastrado com esse nome.",
                IncorretIdFormat = "O paramêtro Id está em formato incorreto. Deve ser hexadecimal com tamanho 24",
                NotFoundGivenId = "Não foi possível encontrar nenhum $ associado com esse id.",
                CantUpdate = "Não foi possível realizar a atualização seguindo os valores passados.",
                CantRemove = "Não foi possível realizar a remoção seguindo os valores passados.",
                DeletedSuccess= "$ deletado com sucesso"
            };
        }
    }
}