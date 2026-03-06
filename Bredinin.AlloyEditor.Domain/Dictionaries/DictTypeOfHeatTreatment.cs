
namespace Bredinin.AlloyEditor.Domain.Dictionaries
{
    public class DictTypeOfHeatTreatment : BaseEntity
    {
        public required string Name { get; set; }

        public required string Description { get; set; }

        public string? Code { get; set; }

        public int? DefaultTemperatureMin { get; set; }
        public int? DefaultTemperatureMax { get; set; }

        public string? DefaultCoolingMedium { get; set; }
    }
}
