using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade;
using FluentValidation.TestHelper;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class ChemicalCompositionsTests
{
    private CreateAlloyGradeRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenChemicalCompositionSumOver100Percent_ShouldHaveValidationError()
    {
        //Arrange
        CreateChemicalCompositionRequest[] composition =
        [
            new CreateChemicalCompositionRequest(MinValue: null, MaxValue: null, ExactValue: 60, ChemicalElementId: Guid.NewGuid()),
            new CreateChemicalCompositionRequest(MinValue: null, MaxValue: null, ExactValue: 60, ChemicalElementId: Guid.NewGuid())
        ];
        
        var testRequest = new CreateAlloyGradeRequest(
            Name: "test",
            Description: "test",
            AlloySystemId: Guid.NewGuid(),
            ChemicalCompositions: composition,
            HeatTreatments: null,
            DefaultMechanicalProperties: null);

        //Act 
        var result = _validator.TestValidate(testRequest);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.ChemicalCompositions);
    }
    
    [Fact]
    public void Validate_WhenChemicalCompositionSumIsValid_ShouldNotHaveValidationError()
    {
        //Arrange
        CreateChemicalCompositionRequest[] composition =
        [
            new CreateChemicalCompositionRequest(MinValue: null, MaxValue: null, ExactValue: 60, ChemicalElementId: Guid.NewGuid()),
            new CreateChemicalCompositionRequest(MinValue: null, MaxValue: null, ExactValue: 21, ChemicalElementId: Guid.NewGuid())
        ];
        
        var testRequest = new CreateAlloyGradeRequest(
            Name: "test",
            Description: "test",
            AlloySystemId: Guid.NewGuid(),
            ChemicalCompositions: composition,
            HeatTreatments: null,
            DefaultMechanicalProperties: null);

        //Act 
        var result = _validator.TestValidate(testRequest);
        
        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChemicalCompositions);
    }
}