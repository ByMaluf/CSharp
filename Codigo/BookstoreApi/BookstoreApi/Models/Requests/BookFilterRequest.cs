using BookstoreApi.Validation;
using System.ComponentModel.DataAnnotations;

namespace BookstoreApi.Models.Requests;

public sealed class BookFilterRequest
{
    public string? Title { get; init; }
    public string? Author { get; init; }

    [ValidGenre]
    public string? Genre { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "MinPrice must be greater than or equal to 0.")]
    public decimal? MinPrice { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "MaxPrice must be greater than or equal to 0.")]
    public decimal? MaxPrice { get; init; }

    public bool? InStock { get; init; }
}
