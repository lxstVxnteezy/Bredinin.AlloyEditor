using Bredinin.AlloyEditor.Domain.Dictionaries;

namespace Bredinin.AlloyEditor.Domain.Alloys
{
    /// <summary>
    /// Механические свойства сплава (для исходного состояния или после ТО)
    /// </summary>
    public class AlloyMechanicalProperty : BaseEntity
    {
        /// <summary>
        /// Идентификатор сплава
        /// </summary>
        public Guid AlloyGradeId { get; set; }

        /// <summary>
        /// Навигационное свойство: сплав
        /// </summary>
        public virtual AlloyGrade AlloyGrade { get; set; } = null!;

        /// <summary>
        /// Идентификатор типа свойства (HRC, МПа и т.д.)
        /// </summary>
        public Guid PropertyTypeId { get; set; }

        /// <summary>
        /// Навигационное свойство: тип свойства
        /// </summary>
        public virtual DictMechanicalPropertyType PropertyType { get; set; } = null!;

        /// <summary>
        /// Идентификатор режима термообработки (null = исходное состояние)
        /// </summary>
        public Guid? HeatTreatmentId { get; set; }

        /// <summary>
        /// Навигационное свойство: режим термообработки
        /// </summary>
        public virtual AlloyHeatTreatment? HeatTreatment { get; set; }

        /// <summary>
        /// Минимальное значение (если диапазон)
        /// </summary>
        public decimal? ValueMin { get; set; }

        /// <summary>
        /// Максимальное значение (если диапазон)
        /// </summary>
        public decimal? ValueMax { get; set; }

        /// <summary>
        /// Точное значение (если не диапазон)
        /// </summary>
        public decimal? ValueExact { get; set; }

        /// <summary>
        /// Дополнительные условия (например: "при 20°C", "продольный образец")
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Источник данных (справочник, эксперимент, ГОСТ)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Примечания
        /// </summary>
        public string? Notes { get; set; }
    }
}