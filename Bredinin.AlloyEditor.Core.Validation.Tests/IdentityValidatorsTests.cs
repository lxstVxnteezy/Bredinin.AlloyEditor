using Bredinin.AlloyEditor.Core.Validation.Validators.Identity;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class IdentityValidatorsTests
{
    private readonly CreateUserCommandValidator _create = new();
    private readonly EditUserCommandValidator _edit = new();
    private readonly ResetPasswordUserCommandValidator _reset = new();

    private static CreateUserCommand NewUser(
        string login = "ivan.petrov", string password = "Passw0rdX", string first = "Иван", string last = "Петров",
        string second = "Сергеевич", int age = 30, Guid[]? roles = null) =>
        new()
        {
            Login = login, Password = password, FirstName = first, LastName = last, SecondName = second,
            Age = age, RoleIds = roles ?? [Guid.NewGuid()]
        };

    [Fact]
    public void CreateUser_Valid_ShouldPass() => _create.TestValidate(NewUser()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("has space")]
    [InlineData("кириллица")]
    [InlineData("semi;colon")]
    public void CreateUser_BadLogin_ShouldFail(string login) =>
        _create.TestValidate(NewUser(login: login)).ShouldHaveValidationErrorFor(x => x.Login);

    [Fact]
    public void CreateUser_TooLongLogin_ShouldFail() =>
        _create.TestValidate(NewUser(login: new string('a', 61))).ShouldHaveValidationErrorFor(x => x.Login);

    [Theory]
    [InlineData("a_b-c.d")]
    [InlineData("User123")]
    public void CreateUser_AllowedLoginSymbols_ShouldPass(string login) =>
        _create.TestValidate(NewUser(login: login)).ShouldNotHaveValidationErrorFor(x => x.Login);

    [Theory]
    [InlineData("")]
    [InlineData("short1")]
    [InlineData("onlyletters")]
    [InlineData("12345678")]
    public void CreateUser_WeakPassword_ShouldFail(string password) =>
        _create.TestValidate(NewUser(password: password)).ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void CreateUser_PasswordLongerThanBcryptLimit_ShouldFail() =>
        _create.TestValidate(NewUser(password: "a1" + new string('x', 71))).ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void CreateUser_EmptyNames_ShouldFail()
    {
        _create.TestValidate(NewUser(first: "")).ShouldHaveValidationErrorFor(x => x.FirstName);
        _create.TestValidate(NewUser(last: " ")).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void CreateUser_EmptySecondName_ShouldPass() =>
        _create.TestValidate(NewUser(second: "")).ShouldNotHaveValidationErrorFor(x => x.SecondName);

    [Fact]
    public void CreateUser_TooLongNames_ShouldFail()
    {
        var tooLong = new string('a', 101);
        _create.TestValidate(NewUser(first: tooLong)).ShouldHaveValidationErrorFor(x => x.FirstName);
        _create.TestValidate(NewUser(last: tooLong)).ShouldHaveValidationErrorFor(x => x.LastName);
        _create.TestValidate(NewUser(second: tooLong)).ShouldHaveValidationErrorFor(x => x.SecondName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(101)]
    [InlineData(-5)]
    public void CreateUser_AgeOutOfRange_ShouldFail(int age) =>
        _create.TestValidate(NewUser(age: age)).ShouldHaveValidationErrorFor(x => x.Age);

    [Fact]
    public void CreateUser_DuplicateRoles_ShouldFail()
    {
        var role = Guid.NewGuid();

        _create.TestValidate(NewUser(roles: [role, role])).ShouldHaveValidationErrorFor(x => x.RoleIds);
    }

    [Fact]
    public void CreateUser_EmptyRoleGuid_ShouldFail() =>
        _create.TestValidate(NewUser(roles: [Guid.Empty])).ShouldHaveValidationErrorFor(x => x.RoleIds);

    [Fact]
    public void CreateUser_NoRoles_ShouldPass() =>
        _create.TestValidate(NewUser(roles: [])).ShouldNotHaveValidationErrorFor(x => x.RoleIds);

    private static EditUserCommand Edit(Guid? id = null, string first = "Иван", int age = 30, Guid[]? roles = null) =>
        new() { Id = id ?? Guid.NewGuid(), FirstName = first, LastName = "Петров", SecondName = "С", Age = age, RoleIds = roles ?? [] };

    [Fact]
    public void EditUser_Valid_ShouldPass() => _edit.TestValidate(Edit()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EditUser_EmptyId_ShouldFail() => _edit.TestValidate(Edit(id: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.Id);

    [Fact]
    public void EditUser_InvalidFields_ShouldFail()
    {
        _edit.TestValidate(Edit(first: "")).ShouldHaveValidationErrorFor(x => x.FirstName);
        _edit.TestValidate(Edit(age: 5)).ShouldHaveValidationErrorFor(x => x.Age);
        var role = Guid.NewGuid();
        _edit.TestValidate(Edit(roles: [role, role])).ShouldHaveValidationErrorFor(x => x.RoleIds);
    }

    [Fact]
    public void ResetPassword_ShouldValidateUserAndPassword()
    {
        _reset.TestValidate(new ResetPasswordUserCommand { UserId = Guid.NewGuid(), Password = "Passw0rdX" })
            .ShouldNotHaveAnyValidationErrors();
        _reset.TestValidate(new ResetPasswordUserCommand { UserId = Guid.Empty, Password = "Passw0rdX" })
            .ShouldHaveValidationErrorFor(x => x.UserId);
        _reset.TestValidate(new ResetPasswordUserCommand { UserId = Guid.NewGuid(), Password = "weak" })
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_ShouldNotApplyPasswordComplexity()
    {
        var validator = new GetJwtTokenQueryValidator();

        validator.TestValidate(new GetJwtTokenQuery { Login = "admin", Password = "old" }).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GetJwtTokenQuery { Login = "", Password = "x" }).ShouldHaveValidationErrorFor(x => x.Login);
        validator.TestValidate(new GetJwtTokenQuery { Login = "admin", Password = "" }).ShouldHaveValidationErrorFor(x => x.Password);
        validator.TestValidate(new GetJwtTokenQuery { Login = "admin", Password = new string('a', 73) })
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Refresh_And_Revoke_ShouldRequireValues()
    {
        var refresh = new RefreshTokenQueryValidator();
        refresh.TestValidate(new RefreshTokenQuery("token")).ShouldNotHaveAnyValidationErrors();
        refresh.TestValidate(new RefreshTokenQuery("")).ShouldHaveValidationErrorFor(x => x.RefreshToken);
        refresh.TestValidate(new RefreshTokenQuery(new string('a', 201))).ShouldHaveValidationErrorFor(x => x.RefreshToken);

        var revoke = new RevokeTokenQueryValidator();
        revoke.TestValidate(new RevokeTokenQuery(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
        revoke.TestValidate(new RevokeTokenQuery(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
