using System.ComponentModel.DataAnnotations;

namespace m3u8Dl;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class ValidHeaderAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string header && Regexes.Header().IsMatch(header))
            return ValidationResult.Success;
        return new ValidationResult($"Value '{value}' is not a valid header.");
    }
}
