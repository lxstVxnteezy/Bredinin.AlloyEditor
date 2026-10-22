using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Core.Validation.Validators.HeatTreatment;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class HeatTreatmentValidatorsTests
{
    private readonly CreateHeatTreatmentRequestValidator _create = new();
    private readonly UpdateHeatTreatmentRequestValidator _update = new();

    private static CreateHeatTreatmentRequest Create(
        Guid? type = null, int? tMin = 800, int? tMax = 850, int? tExact = null,
        int? hMin = 30, int? hMax = 60, int? hExact = null,
        string? cooling = "Вода", string? description = null, int? step = 1) =>
        new(type ?? Guid.NewGuid(), tMin, tMax, tExact, hMin, hMax, hExact, cooling, description, step, false);

    private static UpdateHeatTreatmentRequest Update(
        Guid? type = null, int? tMin = null, int? tMax = null, int? tExact = null,
        int? hMin = null, int? hMax = null, int? hExact = null,
        string? cooling = null, string? description = null, int? step = null) =>
        new(type, tMin, tMax, tExact, hMin, hMax, hExact, cooling, description, step, null);

    [Fact]
    public void Create_Valid_ShouldPass() => _create.TestValidate(Create()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Create_ExactValues_ShouldPass() =>
        _create.TestValidate(Create(tMin: null, tMax: null, tExact: 550, hMin: null, hMax: null, hExact: 120))
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Create_NoTemperatureAndTime_ShouldPass() =>
        _create.TestValidate(Create(tMin: null, tMax: null, hMin: null, hMax: null)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Create_CryogenicTemperature_ShouldPass() =>
        _create.TestValidate(Create(tMin: null, tMax: null, tExact: -196)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Create_EmptyType_ShouldFail() =>
        _create.TestValidate(Create(type: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.HeatTreatmentTypeId);

    [Theory]
    [InlineData(-274)]
    [InlineData(3001)]
    public void Create_TemperatureOutOfRange_ShouldFail(int temperature)
    {
        _create.TestValidate(Create(tMin: temperature, tMax: 3000)).IsValid.Should().BeFalse();
        _create.TestValidate(Create(tMin: null, tMax: null, tExact: temperature)).ShouldHaveValidationErrorFor(x => x.TemperatureExact);
    }

    [Fact]
    public void Create_NegativeHoldingTime_ShouldFail() =>
        _create.TestValidate(Create(hMin: -1, hMax: 5)).IsValid.Should().BeFalse();

    [Fact]
    public void Create_TooLongHoldingTime_ShouldFail() =>
        _create.TestValidate(Create(hMin: null, hMax: null, hExact: 60 * 24 * 365 + 1)).IsValid.Should().BeFalse();

    [Fact]
    public void Create_InvertedTemperatureRange_ShouldFail() =>
        _create.TestValidate(Create(tMin: 900, tMax: 800)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("TemperatureMax"));

    [Fact]
    public void Create_InvertedHoldingRange_ShouldFail() =>
        _create.TestValidate(Create(hMin: 90, hMax: 30)).Errors
            .Should().Contain(e => e.ErrorMessage.Contains("HoldingTimeMax"));

    [Fact]
    public void Create_ExactMixedWithRange_ShouldFail()
    {
        _create.TestValidate(Create(tExact: 820)).Errors.Should().Contain(e => e.ErrorMessage.Contains("TemperatureExact cannot"));
        _create.TestValidate(Create(hExact: 45)).Errors.Should().Contain(e => e.ErrorMessage.Contains("HoldingTimeExact cannot"));
    }

    [Fact]
    public void Create_OnlyOneRangeBound_ShouldFail()
    {
        _create.TestValidate(Create(tMax: null)).Errors.Should().Contain(e => e.ErrorMessage.Contains("TemperatureMin and TemperatureMax"));
        _create.TestValidate(Create(hMin: null)).Errors.Should().Contain(e => e.ErrorMessage.Contains("HoldingTimeMin and HoldingTimeMax"));
    }

    [Fact]
    public void Create_TextTooLong_ShouldFail()
    {
        _create.TestValidate(Create(cooling: new string('a', 101))).ShouldHaveValidationErrorFor(x => x.CoolingMedium);
        _create.TestValidate(Create(description: new string('a', 1001))).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Create_StepOrderBelowOne_ShouldFail() =>
        _create.TestValidate(Create(step: 0)).ShouldHaveValidationErrorFor(x => x.StepOrder);

    [Fact]
    public void Update_AllNull_ShouldPass() => _update.TestValidate(Update()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Update_OnlyOneBound_ShouldPass_BecauseOtherComesFromDb() =>
        _update.TestValidate(Update(tMax: 900)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Update_EmptyType_ShouldFail() =>
        _update.TestValidate(Update(type: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.HeatTreatmentTypeId);

    [Fact]
    public void Update_InvertedRange_ShouldFail() =>
        _update.TestValidate(Update(tMin: 900, tMax: 800)).IsValid.Should().BeFalse();

    [Fact]
    public void Update_ExactMixedWithRange_ShouldFail() =>
        _update.TestValidate(Update(tMin: 800, tExact: 810)).IsValid.Should().BeFalse();

    [Fact]
    public void Update_OutOfRangeValues_ShouldFail()
    {
        _update.TestValidate(Update(tExact: 5000)).IsValid.Should().BeFalse();
        _update.TestValidate(Update(hExact: -5)).IsValid.Should().BeFalse();
        _update.TestValidate(Update(step: -1)).IsValid.Should().BeFalse();
    }
}
