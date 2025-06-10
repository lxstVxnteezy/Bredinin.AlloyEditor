using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Domain.Alloys;

namespace Bredinin.AlloyEditor.Handlers.Helpers
{
    internal static class ChemicalCompositionHelper
    {
        public static ChemicalCompositionsResponse Convert(this AlloyChemicalCompositions compositionsResponse)
        {
            return new ChemicalCompositionsResponse(
                Id: compositionsResponse.Id,
                MinValue: compositionsResponse.MinValue,
                MaxValue: compositionsResponse.MaxValue,
                ExactValue: compositionsResponse.ExactValue,
                ChemicalElementId: compositionsResponse.ChemicalElementId
            );
        }
    }
}
