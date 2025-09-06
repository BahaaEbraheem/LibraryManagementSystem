using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages
{
    /// <summary>
    /// ‰„Ê–Ã «·’›Õ… «·—∆Ì”Ì…
    /// Home page model
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// „‰‘∆ ‰„Ê–Ã «·’›Õ…
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
        /// ≈Õ’«∆Ì«  «·ﬂ »
        /// Book statistics
        /// </summary>
        public BookStatistics? BookStatistics { get; set; }

        /// <summary>
        /// ≈Õ’«∆Ì«  «·«” ⁄«—« 
        /// Borrowing statistics
        /// </summary>
        public BorrowingStatistics? BorrowingStatistics { get; set; }

        /// <summary>
        /// «·ﬂ » «·√ﬂÀ— «” ⁄«—…
        /// Most borrowed books
        /// </summary>
        public IEnumerable<MostBorrowedBook>? MostBorrowedBooks { get; set; }

        /// <summary>
        /// «·„” Œœ„Ì‰ «·√ﬂÀ— ‰‘«ÿ«
        /// Most active users
        /// </summary>
        public IEnumerable<MostActiveUser>? MostActiveUsers { get; set; }

        /// <summary>
        /// —”«·… «·Œÿ√
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// „⁄«·Ã ÿ·» GET -  Õ„Ì· «·’›Õ… «·—∆Ì”Ì…
        /// GET request handler - load home page
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // «· Õﬁﬁ „‰  ”ÃÌ· «·œŒÊ· Ê≈⁄«œ… «· ÊÃÌÂ
            // Check login status and redirect
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Auth/Login");
            }

            // ≈⁄«œ… «· ÊÃÌÂ ≈·Ï ’›Õ… «·ﬂ » ··„” Œœ„Ì‰ «·„”Ã·Ì‰
            // Redirect logged-in users to Books page
            return RedirectToPage("/Books/Index");
        }

    }
}
