using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;

namespace Bredinin.AlloyEditor.Handlers.Helpers
{
    internal static class MechanicalPropertyValueChecker
    {
        /// <summary>
        /// Проверяет итоговые значения свойства на соответствие справочнику типов:
        /// обязательность значений по типу, согласованность min/max/exact и допустимые границы.
        /// </summary>
        public static void Assert(DictMechanicalPropertyType type, decimal? min, decimal? max, decimal? exact)
        {
            if (type.ValueType == PropertyValueType.Exact && exact == null)
                throw new BusinessException($"Exact value required for property type {type.Name}");

            if (type.ValueType == PropertyValueType.Range && (min == null || max == null))
                throw new BusinessException($"Min and Max values required for property type {type.Name}");

            if (min != null && max != null && min > max)
                throw new BusinessException("ValueMax must be greater than or equal to ValueMin");

            foreach (var value in new[] { min, max, exact })
            {
                if (value == null)
                    continue;

                if (type.MinPossible != null && value < type.MinPossible)
                    throw new BusinessException(
                        $"Value {value} is below the minimum possible {type.MinPossible} for property type {type.Name}");

                if (type.MaxPossible != null && value > type.MaxPossible)
                    throw new BusinessException(
                        $"Value {value} is above the maximum possible {type.MaxPossible} for property type {type.Name}");
            }
        }
    }
}
