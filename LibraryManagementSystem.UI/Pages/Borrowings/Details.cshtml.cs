using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Borrowings
{
    public class DetailsModel : PageModel
    {
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IBorrowingService borrowingService, ILogger<DetailsModel> logger)
        {
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Borrowing? Borrowing { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0) return NotFound();

            try
            {
                var result = await _borrowingService.GetBorrowingByIdAsync(Id);

                if (!result.IsSuccess || result.Data == null)
                {
                    _logger.LogWarning("لم يتم العثور على الاستعارة أو فشل التحميل: {Id}", Id);
                    return NotFound();
                }

                Borrowing = result.Data;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل تفاصيل الاستعارة {Id}", Id);
                return Page();
            }
        }

    }
}
