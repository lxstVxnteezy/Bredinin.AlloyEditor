using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Validation;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class RegistrationTests
{
    [Fact]
    public void AddValidation_ShouldRegisterPublicValidators()
    {
        using var provider = new ServiceCollection().AddValidation().BuildServiceProvider();

        provider.GetService<IValidator<CreateAlloyGradeRequest>>().Should().NotBeNull();
        provider.GetService<IValidator<UpdateAlloyGradeRequest>>().Should().NotBeNull();
        provider.GetService<IValidator<ChemicalCompositionsRequest>>().Should().NotBeNull();
        provider.GetService<IValidator<ChemicalCompositionsRequest[]>>().Should().NotBeNull();
        provider.GetService<IValidator<CreateUserCommand>>().Should().NotBeNull();
    }

    [Fact]
    public void AddValidation_ShouldNotRegisterNestedItemValidatorForCreateRequest()
    {
        using var provider = new ServiceCollection().AddValidation().BuildServiceProvider();

        // иначе MVC провалидирует вложенные элементы второй раз и ошибки задвоятся
        provider.GetService<IValidator<CreateChemicalCompositionRequest>>().Should().BeNull();
    }
}
