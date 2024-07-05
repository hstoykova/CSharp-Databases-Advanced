namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var context = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            RemoveBooks(context);
        }

        //02
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            if (!Enum.TryParse(command, true, out AgeRestriction ageRestriction)) 
            {
                return string.Empty;
            }

            var bookTitles = context.Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToArray();

            return string.Join(Environment.NewLine, bookTitles);
        }

        //03
        public static string GetGoldenBooks(BookShopContext context)
        {
            var goldenEditionBooks = context.Books
                .Where(b => b.EditionType == EditionType.Gold && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, goldenEditionBooks);
        }

        //04
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .ToArray();

            return string.Join(Environment.NewLine, books.Select(a => $"{a.Title} - ${a.Price:F2}"));
        }

        //05
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.Year != year)
                .Select(b => new
                {
                    b.Title,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToArray();

            return string.Join(Environment.NewLine, books.Select(b => b.Title));
        }

        //06
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] splittedinput = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var books = context.BooksCategories
                .Where(c => splittedinput.Contains(c.Category.Name.ToLower()))
                .Select(b => b.Book.Title)
                .OrderBy(t => t)
                .ToArray();

            return string.Join(Environment.NewLine, books);
        }

        //07
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            //dd-MM-yyyy

            DateTime dt = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books
                .Where(b => b.ReleaseDate < dt)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => $"{b.Title} - {b.EditionType} - ${b.Price:F2}")
                .ToArray();

            return string.Join(Environment.NewLine, books);
        }

        //08
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authorsNames = context.Authors
                .Where(n => n.FirstName.EndsWith(input))
                .Select(n => $"{n.FirstName} {n.LastName}")
                .ToArray()
                .OrderBy(n => n);

            return string.Join(Environment.NewLine, authorsNames);
        }

        //09
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            string lowerInput = input.ToLower();

            var searchedBooks = context.Books
                .Where(b => b.Title.ToLower().Contains(lowerInput))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToArray();

            return string.Join(Environment.NewLine, searchedBooks);
        }

        //10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var info = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(n => $"{n.Title} ({n.Author.FirstName} {n.Author.LastName})")
                .ToArray();

            return string.Join(Environment.NewLine, info);
        }

        //11
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            return context.Books
                .Count(b => b.Title.Length > lengthCheck);
        }

        //12
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var auth = context.Authors
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    Copies = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(a => a.Copies)
                .ToArray();

            return string.Join(Environment.NewLine, auth.Select(ac => $"{ac.FirstName} {ac.LastName} - {ac.Copies}"));
        }

        //13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categoriesProfit = context.Categories
                .Select(c => new
                {
                    c.Name,
                    Profit = c.CategoryBooks.
                        Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .OrderByDescending(c => c.Profit)
                .ThenBy(c => c.Name)
                .ToArray();

            return string.Join(Environment.NewLine, categoriesProfit.Select(c => $"{c.Name} ${c.Profit:F2}"));
        }

        //14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categRecentBooks = context.Categories
                .Select(c => new
                {
                    c.Name,
                    MostRecentBooks = c.CategoryBooks
                        .OrderByDescending(b => b.Book.ReleaseDate)
                        .Take(3)
                        .Select(b => $"{b.Book.Title} ({b.Book.ReleaseDate.Value.Year})")
                })
                .ToArray()
                .OrderBy(c => c.Name)
                .ToArray();

            StringBuilder sb = new();

            foreach (var category in categRecentBooks)
            {
                sb.AppendLine($"--{category.Name}");

                foreach (var book in category.MostRecentBooks)
                {
                    sb.AppendLine(book);
                }
            }

            return sb.ToString().TrimEnd();
        }

        //15
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToArray();

            foreach (var b in books)
            {
                b.Price += 5;
            }

            context.SaveChanges();
        }

        //16
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books.
                Where(b => b.Copies < 4200)
                .ToArray();

            context.RemoveRange(books);

            context.SaveChanges();

            return books.Length;
        }
    }
}


