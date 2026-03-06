namespace Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;

public record CreateMechanicalPropertyRequest(
    Guid PropertyTypeId,
    Guid? HeatTreatmentId,
    decimal? ValueMin,
    decimal? ValueMax,
    decimal? ValueExact,
    string? Condition,
    string? Source,
    string? Notes);