using BookstoreApi.Domain;
using System.ComponentModel.DataAnnotations;

namespace BookstoreApi.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidGenreAttribute : ValidationAttribute
{
    public ValidGenreAttribute()
        : base($"Genre must be one of: {string.Join(", ", BookGenres.All)}.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        return value is string genre && BookGenres.IsValid(genre);
    }
}
