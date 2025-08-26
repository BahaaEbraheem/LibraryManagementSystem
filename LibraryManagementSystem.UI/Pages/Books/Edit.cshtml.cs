using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.UI.Pages.Books
{
    /// <summary>
    /// نموذج صفحة تعديل الكتاب
    /// Edit book page model
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ILogger<EditModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public EditModel(IBookService bookService, ILogger<EditModel> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج إدخال الكتاب
        /// Book input model
        /// </summary>
        [BindProperty]
        public BookEditModel Input { get; set; } = new BookEditModel();

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل بيانات الكتاب للتعديل
        /// GET request handler - load book data for editing
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

                if (id <= 0)
                {
                    ErrorMessage = "معرف الكتاب غير صحيح - Invalid book ID";
                    return Page();
                }

                _logger.LogDebug("تحميل بيانات الكتاب {BookId} للتعديل - Loading book data for editing", id);

                var result = await _bookService.GetBookByIdAsync(id);

                if (result.IsSuccess && result.Data != null)
                {
                    var book = result.Data;
                    Input = new BookEditModel
                    {
                        BookId = book.BookId,
                        Title = book.Title,
                        Author = book.Author,
                        ISBN = book.ISBN,
                        Publisher = book.Publisher,
                        PublicationYear = book.PublicationYear ?? DateTime.Now.Year,
                        Genre = book.Genre,
                        Description = book.Description,
                        TotalCopies = book.TotalCopies,
                        AvailableCopies = book.AvailableCopies
                    };

                    _logger.LogInformation("تم تحميل بيانات الكتاب {BookId} للتعديل بنجاح - Successfully loaded book data for editing", id);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "لم يتم العثور على الكتاب - Book not found";
                    _logger.LogWarning("فشل في تحميل بيانات الكتاب {BookId}: {Error} - Failed to load book data",
                        id, ErrorMessage);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل بيانات الكتاب {BookId} للتعديل - Error loading book data for editing", id);
                ErrorMessage = "حدث خطأ أثناء تحميل بيانات الكتاب - An error occurred while loading book data";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST - تحديث بيانات الكتاب
        /// POST request handler - update book data
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogDebug("تحديث بيانات الكتاب {BookId}: {BookTitle} - Updating book data", Input.BookId, Input.Title);

                // إنشاء كائن الكتاب المحدث
                // Create updated book object
                var book = new Book
                {
                    BookId = Input.BookId,
                    Title = Input.Title,
                    Author = Input.Author,
                    ISBN = Input.ISBN,
                    Publisher = Input.Publisher,
                    PublicationYear = Input.PublicationYear,
                    Genre = Input.Genre,
                    Description = Input.Description,
                    TotalCopies = Input.TotalCopies,
                    AvailableCopies = Input.AvailableCopies,
                    ModifiedDate = DateTime.Now
                };

                var result = await _bookService.UpdateBookAsync(book);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم تحديث بيانات الكتاب {BookId} بنجاح - Successfully updated book data", Input.BookId);

                    TempData["SuccessMessage"] = "تم تحديث بيانات الكتاب بنجاح! - Book data updated successfully!";
                    return RedirectToPage("/Books/Details", new { id = Input.BookId });
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في تحديث بيانات الكتاب - Failed to update book data";
                    if (result.ValidationErrors.Any())
                    {
                        foreach (var error in result.ValidationErrors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    _logger.LogWarning("فشل في تحديث بيانات الكتاب {BookId}: {Error} - Failed to update book data",
                        Input.BookId, ErrorMessage);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث بيانات الكتاب {BookId} - Error updating book data", Input.BookId);
                ErrorMessage = "حدث خطأ أثناء تحديث بيانات الكتاب - An error occurred while updating book data";
                return Page();
            }
        }
    }

    /// <summary>
    /// نموذج تعديل بيانات الكتاب
    /// Book edit data model
    /// </summary>
    public class BookEditModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "عنوان الكتاب مطلوب - Book title is required")]
        [StringLength(200, ErrorMessage = "عنوان الكتاب يجب أن يكون أقل من 200 حرف - Title must be less than 200 characters")]
        [Display(Name = "عنوان الكتاب - Book Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المؤلف مطلوب - Author name is required")]
        [StringLength(100, ErrorMessage = "اسم المؤلف يجب أن يكون أقل من 100 حرف - Author name must be less than 100 characters")]
        [Display(Name = "المؤلف - Author")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم المعياري الدولي مطلوب - ISBN is required")]
        [StringLength(20, ErrorMessage = "الرقم المعياري يجب أن يكون أقل من 20 حرف - ISBN must be less than 20 characters")]
        [Display(Name = "الرقم المعياري الدولي - ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "اسم الناشر يجب أن يكون أقل من 100 حرف - Publisher name must be less than 100 characters")]
        [Display(Name = "الناشر - Publisher")]
        public string? Publisher { get; set; }

        [Range(1000, 2100, ErrorMessage = "سنة النشر يجب أن تكون بين 1000 و 2100 - Publication year must be between 1000 and 2100")]
        [Display(Name = "سنة النشر - Publication Year")]
        public int PublicationYear { get; set; }

        [StringLength(50, ErrorMessage = "نوع الكتاب يجب أن يكون أقل من 50 حرف - Genre must be less than 50 characters")]
        [Display(Name = "نوع الكتاب - Genre")]
        public string? Genre { get; set; }

        [StringLength(1000, ErrorMessage = "الوصف يجب أن يكون أقل من 1000 حرف - Description must be less than 1000 characters")]
        [Display(Name = "الوصف - Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "عدد النسخ الإجمالي مطلوب - Total copies is required")]
        [Range(1, 1000, ErrorMessage = "عدد النسخ يجب أن يكون بين 1 و 1000 - Number of copies must be between 1 and 1000")]
        [Display(Name = "إجمالي النسخ - Total Copies")]
        public int TotalCopies { get; set; }

        [Required(ErrorMessage = "عدد النسخ المتاحة مطلوب - Available copies is required")]
        [Range(0, 1000, ErrorMessage = "عدد النسخ المتاحة يجب أن يكون بين 0 و 1000 - Available copies must be between 0 and 1000")]
        [Display(Name = "النسخ المتاحة - Available Copies")]
        public int AvailableCopies { get; set; }
    }
}
