using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.UI.Pages.Books
{
    /// <summary>
    /// نموذج صفحة إضافة كتاب جديد
    /// Create new book page model
    /// </summary>
    public class CreateModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ILogger<CreateModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public CreateModel(IBookService bookService, ILogger<CreateModel> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج إدخال الكتاب
        /// Book input model
        /// </summary>
        [BindProperty]
        public BookInputModel Input { get; set; } = new BookInputModel();

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل صفحة الإضافة
        /// GET request handler - load create page
        /// </summary>
        public IActionResult OnGet()
        {
            // التحقق من أن المستخدم مدير
            // Check if user is admin
            if (HttpContext.Session.GetString("UserRole") != "Administrator")
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            return Page();
        }

        /// <summary>
        /// معالج طلب POST - إضافة كتاب جديد
        /// POST request handler - create new book
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

                _logger.LogDebug("إضافة كتاب جديد: {BookTitle} - Adding new book", Input.Title);

                // إنشاء كائن الكتاب
                // Create book object
                var book = new Book
                {
                    Title = Input.Title,
                    Author = Input.Author,
                    ISBN = Input.ISBN,
                    Publisher = Input.Publisher,
                    PublicationYear = Input.PublicationYear,
                    Genre = Input.Genre,
                    Description = Input.Description,
                    TotalCopies = Input.TotalCopies,
                    AvailableCopies = Input.TotalCopies, // جميع النسخ متاحة في البداية
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                var result = await _bookService.AddBookAsync(book);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم إضافة كتاب جديد بنجاح: {BookTitle} بالمعرف {BookId} - Successfully added new book",
                        Input.Title, result.Data);

                    TempData["SuccessMessage"] = "تم إضافة الكتاب بنجاح! - Book added successfully!";
                    return RedirectToPage("/Books/Details", new { id = result.Data });
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في إضافة الكتاب - Failed to add book";
                    if (result.ValidationErrors.Any())
                    {
                        foreach (var error in result.ValidationErrors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    _logger.LogWarning("فشل في إضافة الكتاب: {Error} - Failed to add book", ErrorMessage);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة كتاب جديد - Error adding new book");
                ErrorMessage = "حدث خطأ أثناء إضافة الكتاب - An error occurred while adding the book";
                return Page();
            }
        }
    }

    /// <summary>
    /// نموذج إدخال بيانات الكتاب
    /// Book input data model
    /// </summary>
    public class BookInputModel
    {
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
        public int PublicationYear { get; set; } = DateTime.Now.Year;

        [StringLength(50, ErrorMessage = "نوع الكتاب يجب أن يكون أقل من 50 حرف - Genre must be less than 50 characters")]
        [Display(Name = "نوع الكتاب - Genre")]
        public string? Genre { get; set; }

        [StringLength(1000, ErrorMessage = "الوصف يجب أن يكون أقل من 1000 حرف - Description must be less than 1000 characters")]
        [Display(Name = "الوصف - Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "عدد النسخ مطلوب - Number of copies is required")]
        [Range(1, 1000, ErrorMessage = "عدد النسخ يجب أن يكون بين 1 و 1000 - Number of copies must be between 1 and 1000")]
        [Display(Name = "عدد النسخ - Number of Copies")]
        public int TotalCopies { get; set; } = 1;
    }
}
