namespace Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;

public record MechanicalPropertyDto(
    Guid Id,
    Guid PropertyTypeId,
    string PropertyTypeName,
    string PropertyTypeUnit,
    string? PropertyTypeSymbol,
    decimal? ValueMin,
    decimal? ValueMax,
    decimal? ValueExact,
    string? Condition,
    string? Source,
    Guid? HeatTreatmentId  // null = исходное состояние
);