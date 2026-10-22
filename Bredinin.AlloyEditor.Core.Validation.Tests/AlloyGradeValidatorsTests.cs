using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade;
using Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class CreateAlloyGradeRequestValidatorTests
{
    private readonly CreateAlloyGradeRequestValidator _validator = new();

    private static CreateChemicalCompositionRequest Exact(decimal value, Guid? element = null) =>
        new(null, null, value, element ?? Guid.NewGuid());

    private static CreateChemicalCompositionRequest Range(decimal min, decimal max, Guid? element = null) =>
        new(min, max, null, element ?? Guid.NewGuid());

    private static CreateAlloyGradeRequest Valid(
        string name = "Сталь 45",
        string? description = "описание",
        Guid? systemId = null,
        CreateChemicalCompositionRequest[]? compositions = null,
        CreateHeatTreatmentRequest[]? heat = null,
        CreateMechanicalPropertyRequest[]? props = null) =>
        new(name, description, systemId ?? Guid.NewGuid(),
            compositions ?? [Exact(97), Range(0.4m, 0.5m)], heat, props);

    [Fact]
    public void ValidRequest_ShouldPass() => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyName_ShouldFail(string name) =>
        _validator.TestValidate(Valid(name: name)).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void TooLongName_ShouldFail() =>
        _validator.TestValidate(Valid(name: new string('a', 101))).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void NameOfMaxLength_ShouldPass() =>
        _validator.TestValidate(Valid(name: new string('a', 100))).ShouldNotHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void TooLongDescription_ShouldFail() =>
        _validator.TestValidate(Valid(description: new string('a', 1001))).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void EmptySystemId_ShouldFail() =>
        _validator.TestValidate(Valid(systemId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.AlloySystemId);

    [Fact]
    public void EmptyCompositions_ShouldFail() =>
        _validator.TestValidate(Valid(compositions: [])).ShouldHaveValidationErrorFor(x => x.ChemicalCompositions);

    [Fact]
    public void DuplicateElements_ShouldFail()
    {
        var element = Guid.NewGuid();

        var result = _validator.TestValidate(Valid(compositions: [Exact(50, element), Exact(10, element)]));

        result.ShouldHaveValidationErrorFor(x => x.ChemicalCompositions)
            .WithErrorMessage("Chemical elements must not be repeated");
    }

    [Fact]
    public void SumOfMinimumsOver100_ShouldFail() =>
        _validator.TestValidate(Valid(compositions: [Range(60, 70), Range(41, 45)]))
            .ShouldHaveValidationErrorFor(x => x.ChemicalCompositions);

    [Fact]
    public void SumOfMaximumsOver100_ShouldPass() =>
        _validator.TestValidate(Valid(compositions: [Range(90, 99), Range(1, 5), Range(1, 5)]))
            .ShouldNotHaveValidationErrorFor(x => x.ChemicalCompositions);

    [Fact]
    public void BaseElement100Percent_ShouldPass() =>
        _validator.TestValidate(Valid(compositions: [Exact(100)])).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ItemValuesOutOfRange_ShouldFail()
    {
        var element = Guid.NewGuid();
        CreateChemicalCompositionRequest[] invalid =
        [
            new(null, null, -1, element),
            new(null, null, 101, element),
            new(-1, 5, null, element),
            new(1, 101, null, element)
        ];

        foreach (var item in invalid)
            _validator.TestValidate(Valid(compositions: [item])).IsValid.Should().BeFalse($"{item} is out of range");
    }

    [Fact]
    public void ItemWithoutValues_ShouldFail() =>
        _validator.TestValidate(Valid(compositions: [new CreateChemicalCompositionRequest(null, null, null, Guid.NewGuid())]))
            .IsValid.Should().BeFalse();

    [Fact]
    public void ItemWithOnlyMin_ShouldFail() =>
        _validator.TestValidate(Valid(compositions: [new CreateChemicalCompositionRequest(1, null, null, Guid.NewGuid())]))
            .IsValid.Should().BeFalse();

    [Fact]
    public void ItemWithExactAndRange_ShouldFail()
    {
        var result = _validator.TestValidate(Valid(compositions: [new CreateChemicalCompositionRequest(1, 2, 3, Guid.NewGuid())]));

        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot be combined"));
    }

    [Fact]
    public void ItemWithMinGreaterThanMax_ShouldFail()
    {
        var result = _validator.TestValidate(Valid(compositions: [Range(5, 1)]));

        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("greater than or equal"));
    }

    [Fact]
    public void ItemWithEqualMinAndMax_ShouldFail()
    {
        var result = _validator.TestValidate(Valid(compositions: [Range(5, 5)]));

        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must be different"));
    }

    [Fact]
    public void ItemWithEmptyElementId_ShouldFail() =>
        _validator.TestValidate(Valid(compositions: [Exact(1, Guid.Empty)])).IsValid.Should().BeFalse();

    [Fact]
    public void InvalidNestedHeatTreatment_ShouldFail()
    {
        var heat = new CreateHeatTreatmentRequest(Guid.Empty, null, null, null, null, null, null, null, null, null, false);

        _validator.TestValidate(Valid(heat: [heat])).IsValid.Should().BeFalse();
    }

    [Fact]
    public void InvalidNestedMechanicalProperty_ShouldFail()
    {
        var prop = new CreateMechanicalPropertyRequest(Guid.NewGuid(), null, null, null, null, null, null, null);

        _validator.TestValidate(Valid(props: [prop])).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidNestedItems_ShouldPass()
    {
        var heat = new CreateHeatTreatmentRequest(Guid.NewGuid(), 800, 850, null, null, null, null, "Вода", null, 1, true);
        var prop = new CreateMechanicalPropertyRequest(Guid.NewGuid(), null, 500, 600, null, null, null, null);

        _validator.TestValidate(Valid(heat: [heat], props: [prop])).ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateAlloyGradeRequestValidatorTests
{
    private readonly UpdateAlloyGradeRequestValidator _validator = new();

    [Fact]
    public void AllNull_ShouldPass() =>
        _validator.TestValidate(new UpdateAlloyGradeRequest(null, null, null)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmptyName_ShouldFail() =>
        _validator.TestValidate(new UpdateAlloyGradeRequest("  ", null, null)).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void TooLongName_ShouldFail() =>
        _validator.TestValidate(new UpdateAlloyGradeRequest(new string('a', 101), null, null)).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void TooLongDescription_ShouldFail() =>
        _validator.TestValidate(new UpdateAlloyGradeRequest(null, new string('a', 1001), null)).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void EmptyGuidSystem_ShouldFail() =>
        _validator.TestValidate(new UpdateAlloyGradeRequest(null, null, Guid.Empty)).ShouldHaveValidationErrorFor(x => x.AlloySystemId);
}

public class EditChemicalCompositionsValidatorsTests
{
    private readonly EditChemicalCompositionRequestValidator _item = new();
    private readonly EditChemicalCompositionsRequestValidator _array = new();

    private static ChemicalCompositionsRequest Exact(decimal v, Guid? e = null) => new(null, null, v, e ?? Guid.NewGuid());
    private static ChemicalCompositionsRequest Range(decimal a, decimal b, Guid? e = null) => new(a, b, null, e ?? Guid.NewGuid());

    [Fact]
    public void Item_ValidExactAndRange_ShouldPass()
    {
        _item.TestValidate(Exact(5)).ShouldNotHaveAnyValidationErrors();
        _item.TestValidate(Range(1, 2)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Item_WithoutValues_ShouldFail() =>
        _item.TestValidate(new ChemicalCompositionsRequest(null, null, null, Guid.NewGuid())).IsValid.Should().BeFalse();

    [Fact]
    public void Item_EqualMinAndMax_ShouldFail() => _item.TestValidate(Range(2, 2)).IsValid.Should().BeFalse();

    [Fact]
    public void Item_InvertedRange_ShouldFail() => _item.TestValidate(Range(3, 2)).IsValid.Should().BeFalse();

    [Fact]
    public void Item_ExactMixedWithRange_ShouldFail() =>
        _item.TestValidate(new ChemicalCompositionsRequest(1, 2, 3, Guid.NewGuid())).IsValid.Should().BeFalse();

    [Fact]
    public void Item_OutOfRangeValues_ShouldFail()
    {
        _item.TestValidate(Exact(-1)).IsValid.Should().BeFalse();
        _item.TestValidate(Exact(100.5m)).IsValid.Should().BeFalse();
        _item.TestValidate(Range(-1, 5)).IsValid.Should().BeFalse();
        _item.TestValidate(Range(1, 101)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Item_EmptyElementId_ShouldFail() => _item.TestValidate(Exact(1, Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void Array_Valid_ShouldPass() =>
        _array.TestValidate(new[] { Exact(90), Range(1, 2) }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Array_Empty_ShouldFail() =>
        _array.TestValidate(Array.Empty<ChemicalCompositionsRequest>()).IsValid.Should().BeFalse();

    [Fact]
    public void Array_Duplicates_ShouldFail()
    {
        var e = Guid.NewGuid();

        _array.TestValidate(new[] { Exact(10, e), Exact(20, e) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Array_SumOver100_ShouldFail() =>
        _array.TestValidate(new[] { Exact(60), Exact(41) }).IsValid.Should().BeFalse();
}
