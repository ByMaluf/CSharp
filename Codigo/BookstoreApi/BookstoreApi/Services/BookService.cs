using BookstoreApi.Domain;
using BookstoreApi.Entities;
using BookstoreApi.Models.Requests;

namespace BookstoreApi.Services;

public sealed class BookService : IBookService
{
    private readonly List<Book> _books = [];
    private readonly object _sync = new();

    public IReadOnlyList<Book> GetAll(BookFilterRequest filter)
    {
        lock (_sync)
        {
            IEnumerable<Book> query = _books;

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                var title = filter.Title.Trim();
                query = query.Where(book =>
                    book.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Author))
            {
                var author = filter.Author.Trim();
                query = query.Where(book =>
                    book.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Genre))
            {
                var genre = BookGenres.Normalize(filter.Genre);
                query = query.Where(book =>
                    string.Equals(book.Genre, genre, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.MinPrice.HasValue)
                query = query.Where(book => book.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(book => book.Price <= filter.MaxPrice.Value);

            if (filter.InStock.HasValue)
            {
                query = filter.InStock.Value
                    ? query.Where(book => book.Stock > 0)
                    : query.Where(book => book.Stock == 0);
            }

            return query
                .OrderBy(book => book.Title)
                .ThenBy(book => book.Author)
                .Select(Clone)
                .ToList();
        }
    }

    public Book? GetById(Guid id)
    {
        lock (_sync)
        {
            var book = _books.FirstOrDefault(book => book.Id == id);
            return book is null ? null : Clone(book);
        }
    }

    public Book? Create(CreateBookRequest request)
    {
        lock (_sync)
        {
            var title = request.Title.Trim();
            var author = request.Author.Trim();

            if (HasDuplicate(title, author))
                return null;

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = title,
                Author = author,
                Genre = BookGenres.Normalize(request.Genre),
                Price = request.Price,
                Stock = request.Stock,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _books.Add(book);
            return Clone(book);
        }
    }

    public BookUpdateResult Update(Guid id, UpdateBookRequest request)
    {
        lock (_sync)
        {
            var book = _books.FirstOrDefault(book => book.Id == id);

            if (book is null)
                return new BookUpdateResult(BookUpdateStatus.NotFound, null);

            var title = request.Title.Trim();
            var author = request.Author.Trim();

            if (HasDuplicate(title, author, id))
                return new BookUpdateResult(BookUpdateStatus.Conflict, null);

            book.Title = title;
            book.Author = author;
            book.Genre = BookGenres.Normalize(request.Genre);
            book.Price = request.Price;
            book.Stock = request.Stock;
            book.UpdatedAt = DateTimeOffset.UtcNow;

            return new BookUpdateResult(BookUpdateStatus.Success, Clone(book));
        }
    }

    public bool Delete(Guid id)
    {
        lock (_sync)
        {
            var book = _books.FirstOrDefault(book => book.Id == id);

            if (book is null)
                return false;

            _books.Remove(book);
            return true;
        }
    }

    private bool HasDuplicate(string title, string author, Guid? ignoredId = null)
    {
        return _books.Any(book =>
            (!ignoredId.HasValue || book.Id != ignoredId.Value) &&
            string.Equals(book.Title, title, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(book.Author, author, StringComparison.OrdinalIgnoreCase));
    }

    private static Book Clone(Book book)
    {
        return new Book
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Genre = book.Genre,
            Price = book.Price,
            Stock = book.Stock,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }
}
