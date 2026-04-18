namespace Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;

public record HeatTreatmentTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string? Code,
    int? DefaultTemperatureMin,
    int? DefaultTemperatureMax,
    string? DefaultCoolingMedium);