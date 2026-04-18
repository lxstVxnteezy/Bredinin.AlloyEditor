namespace Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;

public record HeatTreatmentDto(
    Guid Id,
    Guid HeatTreatmentTypeId,
    string HeatTreatmentTypeName,
    int? TemperatureMin,
    int? TemperatureMax,
    int? TemperatureExact,
    int? HoldingTimeMin,
    int? HoldingTimeMax,
    int? HoldingTimeExact,
    string? CoolingMedium,
    string? Description,
    int? StepOrder,
    bool IsDefault);