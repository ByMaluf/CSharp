namespace BookstoreApi.Domain;

public static class BookGenres
{
    private static readonly string[] Genres =
    [
        "ficção",
        "romance",
        "mistério",
        "fantasia",
        "terror",
        "biografia",
        "história",
        "tecnologia",
        "autoajuda",
        "infantil"
    ];

    public static IReadOnlyList<string> All => Genres;

    public static bool IsValid(string? genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return false;

        return Genres.Any(validGenre =>
            string.Equals(validGenre, genre.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string genre)
    {
        var normalizedGenre = Genres.FirstOrDefault(validGenre =>
            string.Equals(validGenre, genre.Trim(), StringComparison.OrdinalIgnoreCase));

        return normalizedGenre
            ?? throw new ArgumentException("Invalid genre.", nameof(genre));
    }
}
