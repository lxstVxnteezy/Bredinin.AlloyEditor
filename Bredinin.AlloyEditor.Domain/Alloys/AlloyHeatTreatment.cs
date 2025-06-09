namespace Bredinin.MyPetProject.Domain.Alloys
{
    public class AlloyHeatTreatment : BaseEntity
    {
        public int TemperatureMinValue { get; set; }
        public int TemperatureMaxValue { get; set; }

        public int HoldingTimeMinValue { get; set; }
        public int HoldingTimeMaxValue { get; set; }

        public Guid AlloyGradeId { get; set; }
        public Guid DictTypeOfHeatTreatmentId { get; set; }
    }
}
