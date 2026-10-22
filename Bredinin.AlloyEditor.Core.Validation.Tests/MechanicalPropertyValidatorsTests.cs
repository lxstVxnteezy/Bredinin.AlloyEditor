using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.Core.Validation.Validators.MechanicalProperty;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class MechanicalPropertyValidatorsTests
{
    private readonly CreateMechanicalPropertyRequestValidator _create = new();
    private readonly UpdateMechanicalPropertyRequestValidator _update = new();

    private static CreateMechanicalPropertyRequest Create(
        Guid? type = null, Guid? heat = null, decimal? min = 500, decimal? max = 600, decimal? exact = null,
        string? condition = null, string? source = null, string? notes = null) =>
        new(type ?? Guid.NewGuid(), heat, min, max, exact, condition, source, notes);

    private static UpdateMechanicalPropertyRequest Update(
        Guid? type = null, decimal? min = null, decimal? max = null, decimal? exact = null,
        string? condition = null, string? source = null, string? notes = null) =>
        new(type, min, max, exact, condition, source, notes);

    [Fact]
    public void Create_RangeAndExact_ShouldPass()
    {
        _create.TestValidate(Create()).ShouldNotHaveAnyValidationErrors();
        _create.TestValidate(Create(min: null, max: null, exact: 45)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_NegativeValue_ShouldPass_BecauseLimitsAreDictionaryDriven() =>
        _create.TestValidate(Create(min: -10, max: 5)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Create_EmptyType_ShouldFail() =>
        _create.TestValidate(Create(type: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.PropertyTypeId);

    [Fact]
    public void Create_EmptyHeatTreatmentGuid_ShouldFail() =>
        _create.TestValidate(Create(heat: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.HeatTreatmentId);

    [Fact]
    public void Create_NoValues_ShouldFail() =>
        _create.TestValidate(Create(min: null, max: null)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("Either ValueExact"));

    [Fact]
    public void Create_OnlyOneBound_ShouldFail() =>
        _create.TestValidate(Create(max: null)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("specified together"));

    [Fact]
    public void Create_InvertedRange_ShouldFail() =>
        _create.TestValidate(Create(min: 700, max: 600)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("ValueMax must be"));

    [Fact]
    public void Create_ExactMixedWithRange_ShouldFail() =>
        _create.TestValidate(Create(exact: 550)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("cannot be combined"));

    [Fact]
    public void Create_ValueBeyondColumnPrecision_ShouldFail() =>
        _create.TestValidate(Create(min: null, max: null, exact: 1e15m)).IsValid.Should().BeFalse();

    [Fact]
    public void Create_TextTooLong_ShouldFail()
    {
        _create.TestValidate(Create(condition: new string('a', 501))).ShouldHaveValidationErrorFor(x => x.Condition);
        _create.TestValidate(Create(source: new string('a', 501))).ShouldHaveValidationErrorFor(x => x.Source);
        _create.TestValidate(Create(notes: new string('a', 1001))).ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Create_TextOfMaxLength_ShouldPass() =>
        _create.TestValidate(Create(condition: new string('a', 500), source: new string('a', 500), notes: new string('a', 1000)))
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Update_AllNull_ShouldPass() => _update.TestValidate(Update()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Update_SingleBound_ShouldPass() => _update.TestValidate(Update(max: 650)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Update_EmptyType_ShouldFail() =>
        _update.TestValidate(Update(type: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.PropertyTypeId);

    [Fact]
    public void Update_InvertedRange_ShouldFail() => _update.TestValidate(Update(min: 9, max: 1)).IsValid.Should().BeFalse();

    [Fact]
    public void Update_ExactMixedWithRange_ShouldFail() =>
        _update.TestValidate(Update(min: 1, exact: 5)).IsValid.Should().BeFalse();

    [Fact]
    public void Update_TextTooLong_ShouldFail() =>
        _update.TestValidate(Update(notes: new string('a', 1001))).ShouldHaveValidationErrorFor(x => x.Notes);
}
