using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.Domain.Alloys;

namespace Bredinin.AlloyEditor.Handlers.Validators
{
    public static class AlloyChemicalCompositionValidator
    {
        /// <summary>
        /// Проверяет, что сумма MinValue и MaxValue всех элементов не превышает 100
        /// </summary>
        /// <param name="compositions">Массив химических составов</param>

        public static void ValidateTotalRange(AlloyChemicalCompositions[] compositions)
        {
            if (compositions.Length == 0)
                return;

            var totalMin = compositions
                .Where(x => x.MinValue.HasValue)
                .Sum(x => x.MinValue!.Value);

            var totalMax = compositions
                .Where(x => x.MaxValue.HasValue)
                .Sum(x => x.MaxValue!.Value);

            if (totalMin > 100)
                throw new BusinessException($"The sum of all minimum valuesof elements must not exceed 100 (current: {totalMin})");

            if (totalMax > 100)
                throw new BusinessException($"The sum of all maximum values of elements must not exceed 100 (current: {totalMax})");
        }

    }
}
