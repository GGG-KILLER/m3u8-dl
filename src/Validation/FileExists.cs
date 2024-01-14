using System.ComponentModel.DataAnnotations;

namespace m3u8Dl.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public sealed class FileExists : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && File.Exists(path))
            return ValidationResult.Success;
        return new ValidationResult($"File '{value}' does not exist.");
    }
}
