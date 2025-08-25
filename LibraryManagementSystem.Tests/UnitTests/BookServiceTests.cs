using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests.UnitTests
{
    /// <summary>
    /// اختبارات وحدة خدمة الكتب
    /// Book service unit tests
    /// </summary>
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly Mock<ILogger<BookService>> _mockLogger;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockLogger = new Mock<ILogger<BookService>>();
            _bookService = new BookService(_mockBookRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetBookByIdAsync_ValidId_ReturnsBook()
        {
            // Arrange
            var bookId = 1;
            var expectedBook = new Book
            {
                BookId = bookId,
                Title = "كتاب تجريبي",
                Author = "مؤلف تجريبي",
                ISBN = "978-1234567890",
                TotalCopies = 5,
                AvailableCopies = 3
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(expectedBook);

            // Act
            var result = await _bookService.GetBookByIdAsync(bookId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedBook.BookId, result.Data.BookId);
            Assert.Equal(expectedBook.Title, result.Data.Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_InvalidId_ReturnsFailure()
        {
            // Arrange
            var invalidId = -1;

            // Act
            var result = await _bookService.GetBookByIdAsync(invalidId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Contains("معرف الكتاب غير صحيح", result.ErrorMessage);
        }

        [Fact]
        public async Task SearchBooksAsync_ValidCriteria_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new BookSearchDto
            {
                SearchTerm = "كتاب",
                PageNumber = 1,
                PageSize = 10
            };

            var expectedBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "كتاب الأول", Author = "مؤلف أول" },
                new Book { BookId = 2, Title = "كتاب الثاني", Author = "مؤلف ثاني" }
            };

            var expectedResult = new PagedResult<Book>
            {
                Items = expectedBooks,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockBookRepository.Setup(repo => repo.SearchAsync(searchCriteria))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _bookService.SearchBooksAsync(searchCriteria);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(2, result.Data.Items.Count());
        }

        //[Fact]
        //public async Task GetAvailableBooksAsync_ReturnsOnlyAvailableBooks()
        //{
        //    // Arrange
        //    var availableBooks = new List<Book>
        //    {
        //        new Book { BookId = 1, Title = "كتاب متاح", AvailableCopies = 3 },
        //        new Book { BookId = 2, Title = "كتاب متاح آخر", AvailableCopies = 1 }
        //    };

        //    _mockBookRepository.Setup(repo => repo.GetAvailableBooksAsync())
        //        .ReturnsAsync(availableBooks);

        //    // Act
        //    var result = await _bookService.GetAvailableBooksAsync();

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(2, result.Data.Count());
        //    Assert.All(result.Data, book => Assert.True(book.AvailableCopies > 0));
        //}

        [Fact]
        public async Task IsBookAvailableAsync_AvailableBook_ReturnsTrue()
        {
            // Arrange
            var bookId = 1;
            _mockBookRepository.Setup(repo => repo.IsAvailableAsync(bookId))
                .ReturnsAsync(true);

            // Act
            var result = await _bookService.IsBookAvailableAsync(bookId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task IsBookAvailableAsync_UnavailableBook_ReturnsFalse()
        {
            // Arrange
            var bookId = 1;
            _mockBookRepository.Setup(repo => repo.IsAvailableAsync(bookId))
                .ReturnsAsync(false);

            // Act
            var result = await _bookService.IsBookAvailableAsync(bookId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task IsBookAvailableAsync_InvalidId_ReturnsFailure(int invalidId)
        {
            // Act
            var result = await _bookService.IsBookAvailableAsync(invalidId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("معرف الكتاب غير صحيح", result.ErrorMessage);
        }

        //[Fact]
        //public async Task GetBooksByAuthorAsync_ValidAuthor_ReturnsBooks()
        //{
        //    // Arrange
        //    var author = "نجيب محفوظ";
        //    var expectedBooks = new List<Book>
        //    {
        //        new Book { BookId = 1, Title = "أولاد حارتنا", Author = author },
        //        new Book { BookId = 2, Title = "الثلاثية", Author = author }
        //    };

        //    _mockBookRepository.Setup(repo => repo.GetByAuthorAsync(author))
        //        .ReturnsAsync(expectedBooks);

        //    // Act
        //    var result = await _bookService.GetBooksByAuthorAsync(author);

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(2, result.Data.Count());
        //    Assert.All(result.Data, book => Assert.Equal(author, book.Author));
        //}

        //[Theory]
        //[InlineData("")]
        //[InlineData("   ")]
        //[InlineData(null)]
        //public async Task GetBooksByAuthorAsync_InvalidAuthor_ReturnsFailure(string invalidAuthor)
        //{
        //    // Act
        //    var result = await _bookService.GetBooksByAuthorAsync(invalidAuthor);

        //    // Assert
        //    Assert.False(result.IsSuccess);
        //    Assert.Contains("اسم المؤلف مطلوب", result.ErrorMessage);
        //}

        //[Fact]
        //public async Task GetBooksByGenreAsync_ValidGenre_ReturnsBooks()
        //{
        //    // Arrange
        //    var genre = "أدب";
        //    var expectedBooks = new List<Book>
        //    {
        //        new Book { BookId = 1, Title = "رواية أدبية", Genre = genre },
        //        new Book { BookId = 2, Title = "مجموعة قصصية", Genre = genre }
        //    };

        //    _mockBookRepository.Setup(repo => repo.GetByGenreAsync(genre))
        //        .ReturnsAsync(expectedBooks);

        //    // Act
        //    var result = await _bookService.GetBooksByGenreAsync(genre);

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(2, result.Data.Count());
        //    Assert.All(result.Data, book => Assert.Equal(genre, book.Genre));
        //}
    }
}
