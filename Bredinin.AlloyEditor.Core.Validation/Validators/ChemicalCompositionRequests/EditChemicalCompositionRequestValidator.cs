using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests
{
    /// <summary>
    /// Валидатор одной строки состава при редактировании.
    /// Правила уровня массива — в <see cref="EditChemicalCompositionsRequestValidator"/>.
    /// </summary>
    public class EditChemicalCompositionRequestValidator
        : ChemicalCompositionItemValidator<ChemicalCompositionsRequest>
    {
        public EditChemicalCompositionRequestValidator()
            : base(x => x.ChemicalElementId, x => x.MinValue, x => x.MaxValue, x => x.ExactValue)
        {
        }
    }
}
