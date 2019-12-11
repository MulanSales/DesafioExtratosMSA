using System;
using System.Globalization;
using FluentValidation;
using FluentValidation.Validators;
using Releases.API.Models;

namespace Releases.API.Validators
{
    public class ReleaseRequestValidator: AbstractValidator<ReleaseRequest>
    {
        public ReleaseRequestValidator()
        {
            RuleFor(request => request.Date)
                .NotEmpty()
                .NotNull()
                .ValidDateFormat();
            RuleFor(request => request.EstablishmentName)
                .NotEmpty()
                .NotNull();
            RuleFor(request => request.Amount)
                .NotEmpty()
                .NotNull()
                .ScalePrecision(2, 50);
            RuleFor(request => request.PaymentMethod)
                .NotNull()
                .IsInEnum();
        }
    }

    public class ReleaseRequestDateValidator : PropertyValidator
    {
        public ReleaseRequestDateValidator() : base("Campo 'Date' não está em formato válido. Tente o formato dia/mês/ano. Ex: 05/05/2019") {}

        protected override bool IsValid(PropertyValidatorContext context)
        {
            string date = (string)context.PropertyValue;

            DateTime temp;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp)) return true;

            return false;
        }
    }

    public static class CustomValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidDateFormat<T>(this IRuleBuilder<T, string> ruleBuilder) {
            return ruleBuilder.SetValidator(new ReleaseRequestDateValidator());
        }
    }
}