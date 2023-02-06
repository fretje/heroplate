using System.Diagnostics.CodeAnalysis;
using FluentValidation.Validators;

namespace Heroplate.Api.Application.Common.Validation;

// SetValidator doesn't work when dealing with a nullable reference type
// Use this SetNonNullableValidator extension method instead
// For more info see https://github.com/FluentValidation/FluentValidation/issues/1648
public static class SetNonNullableValidatorExtension
{
    public static IRuleBuilderOptions<T, TProperty?> SetNonNullableValidator<T, TProperty>(this IRuleBuilder<T, TProperty?> ruleBuilder, IValidator<TProperty> validator, params string[] ruleSets)
    {
        var adapter = new NullableChildValidatorAdaptor<T, TProperty>(validator, validator.GetType())
        {
            RuleSets = ruleSets
        };

        return ruleBuilder.SetAsyncValidator(adapter);
    }

    [SuppressMessage("Roslynator", "RCS1182:Remove redundant base interface", Justification = "These base interfaces are needed as they change the nullability of TProperty")]
    [SuppressMessage("Roslynator", "RCS1132:Remove redundant overriding member", Justification = "IsValid and IsValidAsync are needed as they change the nullability of the value parameter")]
    private sealed class NullableChildValidatorAdaptor<T, TProperty> : ChildValidatorAdaptor<T, TProperty>, IPropertyValidator<T, TProperty?>, IAsyncPropertyValidator<T, TProperty?>
    {
        public NullableChildValidatorAdaptor(IValidator<TProperty> validator, Type validatorType)
            : base(validator, validatorType)
        {
        }

        public override bool IsValid(ValidationContext<T> context, TProperty? value) =>
            base.IsValid(context, value!);

        public override Task<bool> IsValidAsync(ValidationContext<T> context, TProperty? value, CancellationToken ct) =>
            base.IsValidAsync(context, value!, ct);
    }
}