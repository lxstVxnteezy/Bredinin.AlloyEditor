namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;

public record UpdateAlloyGradeRequest(
    string? Name,
    string? Description,
    Guid? AlloySystemId);