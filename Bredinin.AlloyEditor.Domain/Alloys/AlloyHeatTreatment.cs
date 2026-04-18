using Bredinin.AlloyEditor.Domain.Dictionaries;

namespace Bredinin.AlloyEditor.Domain.Alloys
{
    /// <summary>
    /// Режим термообработки для конкретного сплава
    /// </summary>
    public class AlloyHeatTreatment : BaseEntity
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
        /// Идентификатор типа термообработки (из справочника)
        /// </summary>
        public Guid HeatTreatmentTypeId { get; set; }

        /// <summary>
        /// Навигационное свойство: тип термообработки
        /// </summary>
        public virtual DictTypeOfHeatTreatment HeatTreatmentType { get; set; } = null!;

        /// <summary>
        /// Минимальная температура нагрева (в градусах Цельсия)
        /// </summary>
        public int? TemperatureMin { get; set; }

        /// <summary>
        /// Максимальная температура нагрева (в градусах Цельсия)
        /// </summary>
        public int? TemperatureMax { get; set; }

        /// <summary>
        /// Точная температура нагрева (если не диапазон)
        /// </summary>
        public int? TemperatureExact { get; set; }

        /// <summary>
        /// Минимальное время выдержки (в минутах)
        /// </summary>
        public int? HoldingTimeMin { get; set; }

        /// <summary>
        /// Максимальное время выдержки (в минутах)
        /// </summary>
        public int? HoldingTimeMax { get; set; }

        /// <summary>
        /// Точное время выдержки (если не диапазон)
        /// </summary>
        public int? HoldingTimeExact { get; set; }

        /// <summary>
        /// Среда охлаждения (вода, масло, воздух, печь и т.д.)
        /// </summary>
        public string? CoolingMedium { get; set; }

        /// <summary>
        /// Дополнительное описание или примечания
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Приоритет или порядок применения (для многоступенчатой обработки)
        /// </summary>
        public int? StepOrder { get; set; }

        /// <summary>
        /// Флаг, указывающий, является ли этот режим рекомендуемым по умолчанию
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Механические свойства, полученные после этого режима термообработки
        /// </summary>
        public virtual ICollection<AlloyMechanicalProperty> MechanicalProperties { get; set; }
            = new List<AlloyMechanicalProperty>();

    }
}
    

