
namespace Bredinin.AlloyEditor.Contracts.Common.Dictionaries;

public record MechanicalPropertyTypeDto(
    Guid Id,
    string Name,
    string Unit,
    string? Symbol,
    string? Description,
    PropertyValueType ValueType,
    decimal? MinPossible,
    decimal? MaxPossible);