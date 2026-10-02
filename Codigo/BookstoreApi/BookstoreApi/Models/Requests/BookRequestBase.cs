using BookstoreApi.Validation;
using System.ComponentModel.DataAnnotations;

namespace BookstoreApi.Models.Requests;

public abstract class BookRequestBase
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Title must have between 2 and 120 characters.")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "Author is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Author must have between 2 and 120 characters.")]
    public string Author { get; init; } = string.Empty;

    [Required(ErrorMessage = "Genre is required.")]
    [ValidGenre]
    public string Genre { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Price must be greater than or equal to 0.")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0.")]
    public int Stock { get; init; }
}
