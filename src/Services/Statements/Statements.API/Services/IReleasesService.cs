using CommonLibrary.DataAccess;
using Statements.API.Models;

namespace Statements.API.Services
{
    public interface IReleasesService : IDatabaseServicesSchema<Release>
    {
    }
}