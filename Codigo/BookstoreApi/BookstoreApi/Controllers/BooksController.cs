using BookstoreApi.Entities;
using BookstoreApi.Models.Requests;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreApi.Controllers;

public sealed class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult Create([FromBody] CreateBookRequest request)
    {
        var book = _bookService.Create(request);

        if (book is null)
            return DuplicateBookConflict();

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Book>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] BookFilterRequest filter)
    {
        if (filter.MinPrice.HasValue &&
            filter.MaxPrice.HasValue &&
            filter.MinPrice.Value > filter.MaxPrice.Value)
        {
            ModelState.AddModelError(
                nameof(filter.MinPrice),
                "minPrice cannot be greater than maxPrice.");

            return ValidationProblem(ModelState);
        }

        return Ok(_bookService.GetAll(filter));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var book = _bookService.GetById(id);

        return book is null
            ? BookNotFound(id)
            : Ok(book);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult Update(
        [FromRoute] Guid id,
        [FromBody] UpdateBookRequest request)
    {
        var result = _bookService.Update(id, request);

        return result.Status switch
        {
            BookUpdateStatus.NotFound => BookNotFound(id),
            BookUpdateStatus.Conflict => DuplicateBookConflict(),
            _ => Ok(result.Book)
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] Guid id)
    {
        return _bookService.Delete(id)
            ? NoContent()
            : BookNotFound(id);
    }

    private NotFoundObjectResult BookNotFound(Guid id)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Book not found",
            Detail = $"No book with id '{id}' was found."
        });
    }

    private ConflictObjectResult DuplicateBookConflict()
    {
        return Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Book already exists",
            Detail = "A book with the same title and author is already registered."
        });
    }
}
