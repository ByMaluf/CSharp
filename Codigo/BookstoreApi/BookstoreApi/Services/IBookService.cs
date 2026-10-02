using BookstoreApi.Entities;
using BookstoreApi.Models.Requests;

namespace BookstoreApi.Services;

public interface IBookService
{
    IReadOnlyList<Book> GetAll(BookFilterRequest filter);
    Book? GetById(Guid id);
    Book? Create(CreateBookRequest request);
    BookUpdateResult Update(Guid id, UpdateBookRequest request);
    bool Delete(Guid id);
}

public enum BookUpdateStatus
{
    Success,
    NotFound,
    Conflict
}

public sealed record BookUpdateResult(BookUpdateStatus Status, Book? Book);
