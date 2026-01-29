using Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEditor.Core.Validation
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddValidation(this IServiceCollection service)
        {
            service.AddValidatorsFromAssemblyContaining<CreateAlloyGradeRequestValidator>();
            service.AddFluentValidationAutoValidation();

            return service;
        }

    }
}
