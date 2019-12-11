using Establishments.API.Models;
using FluentValidation;

namespace Establishments.API.Validators
{
    public class EstablishmentRequestValidator: AbstractValidator<EstablishmentRequest>
    {
        public EstablishmentRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(50);
            RuleFor(request => request.Type)
                .NotEmpty()
                .NotNull()
                .MaximumLength(25);
        }
    }
}