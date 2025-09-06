using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages
{
    /// <summary>
    /// ����� ������ ��������
    /// Home page model
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// ���� ����� ������
        /// Page model constructor
        /// </summary>
        public IndexModel(
            IBookService bookService,
            IBorrowingService borrowingService,
            ILogger<IndexModel> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// �������� �����
        /// Book statistics
        /// </summary>
        public BookStatistics? BookStatistics { get; set; }

        /// <summary>
        /// �������� ����������
        /// Borrowing statistics
        /// </summary>
        public BorrowingStatistics? BorrowingStatistics { get; set; }

        /// <summary>
        /// ����� ������ �������
        /// Most borrowed books
        /// </summary>
        public IEnumerable<MostBorrowedBook>? MostBorrowedBooks { get; set; }

        /// <summary>
        /// ���������� ������ ������
        /// Most active users
        /// </summary>
        public IEnumerable<MostActiveUser>? MostActiveUsers { get; set; }

        /// <summary>
        /// ����� �����
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// ����� ��� GET - ����� ������ ��������
        /// GET request handler - load home page
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // ������ �� ����� ������ ������ �������
            // Check login status and redirect
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Auth/Login");
            }

            // ����� ������� ��� ���� ����� ���������� ��������
            // Redirect logged-in users to Books page
            return RedirectToPage("/Books/Index");
        }

    }
}
