namespace Bredinin.AlloyEditor.Domain.Dictionaries
{
    /// <summary>
    /// Справочник типов механических свойств
    /// </summary>
    public class DictMechanicalPropertyType : BaseEntity
    {
        /// <summary>
        /// Название свойства (например: "Твёрдость HRC", "Предел прочности")
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Единица измерения (HRC, МПа, %, Дж/см²)
        /// </summary>
        public required string Unit { get; set; }

        /// <summary>
        /// Краткое обозначение (HB, HRC, Rm, Rp0.2)
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Тип значения: Range (диапазон) или Exact (точное)
        /// </summary>
        public PropertyValueType ValueType { get; set; } = PropertyValueType.Range;

        /// <summary>
        /// Минимально возможное значение (для валидации)
        /// </summary>
        public decimal? MinPossible { get; set; }

        /// <summary>
        /// Максимально возможное значение (для валидации)
        /// </summary>
        public decimal? MaxPossible { get; set; }
    }

    public enum PropertyValueType
    {
        Range,  // можно указывать min/max
        Exact   // только точное значение
    }
}