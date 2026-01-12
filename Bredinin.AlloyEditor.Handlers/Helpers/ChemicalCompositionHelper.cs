using System.Linq.Expressions;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Domain.Alloys;

namespace Bredinin.AlloyEditor.Handlers.Helpers
{
    internal static class ChemicalCompositionHelper
    {
        public static Expression<Func<AlloyChemicalCompositions, ChemicalCompositionsDto>> ConvertExpression()
        {
            return x => new ChemicalCompositionsDto(
                x.Id,
                x.MinValue,
                x.MaxValue,
                x.ExactValue,
                x.ChemicalElementId
            );
        }

        public static ChemicalCompositionsDto Convert(AlloyChemicalCompositions x)
       => new(
           x.Id,
           x.MinValue,
           x.MaxValue,
           x.ExactValue,
           x.ChemicalElementId
       );

    }
}
