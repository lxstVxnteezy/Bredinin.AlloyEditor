using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Validation.Extensions;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests
{
    /// <summary>
    /// Валидатор состава целиком (тело PUT /alloys/{id}/chemical-compositions): строки + правила уровня массива
    /// </summary>
    public class EditChemicalCompositionsRequestValidator : AbstractValidator<ChemicalCompositionsRequest[]>
    {
        public EditChemicalCompositionsRequestValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("Chemical compositions are required");

            // MVC не валидирует элементы корневого массива сам, поэтому подключаем валидатор строки явно
            RuleForEach(x => x)
                .SetValidator(new EditChemicalCompositionRequestValidator());

            RuleFor(x => x)
                .Must(c => ChemicalCompositionValidationExtensions.HasUniqueElements(c, x => x.ChemicalElementId))
                .WithMessage("Chemical elements must not be repeated")
                .When(x => x is { Length: > 0 });

            RuleFor(x => x)
                .Must(c => ChemicalCompositionValidationExtensions.IsTotalRangeValid(
                    c, x => x.MinValue, x => x.MaxValue, x => x.ExactValue))
                .WithMessage($"The sum of minimum values must not exceed {ChemicalCompositionValidationExtensions.GetMaxPercent()}")
                .When(x => x is { Length: > 0 });
        }
    }
}
