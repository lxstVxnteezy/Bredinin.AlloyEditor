namespace Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;

public record UpdateMechanicalPropertyRequest(
    Guid? PropertyTypeId,
    decimal? ValueMin,
    decimal? ValueMax,
    decimal? ValueExact,
    string? Condition,
    string? Source,
    string? Notes);