using System.ComponentModel;

namespace LibraryManagementSystem.DAL.Models.Enums
{
    /// <summary>
    /// أدوار المستخدمين في النظام
    /// User roles in the system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// مستخدم عادي - Regular User
        /// يمكنه البحث والاستعارة والإرجاع
        /// Can search, borrow, and return books
        /// </summary>
        [Description("مستخدم عادي - Regular User")]
        User = 1,

        /// <summary>
        /// مدير النظام - Administrator
        /// يمكنه إدارة المستخدمين والكتب وجميع الصلاحيات
        /// Can manage users, books and has all permissions
        /// </summary>
        [Description("مدير النظام - Administrator")]
        Administrator = 2
    }

    /// <summary>
    /// امتدادات لـ UserRole
    /// Extensions for UserRole
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// الحصول على الوصف
        /// Get description
        /// </summary>
        public static string GetDescription(this UserRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? role.ToString();
        }

        /// <summary>
        /// الحصول على CSS Class للدور
        /// Get CSS class for role
        /// </summary>
        public static string GetCssClass(this UserRole role)
        {
            return role switch
            {
                UserRole.User => "badge bg-primary",
                UserRole.Administrator => "badge bg-danger",
                _ => "badge bg-secondary"
            };
        }

        /// <summary>
        /// الحصول على أيقونة للدور
        /// Get icon for role
        /// </summary>
        public static string GetIcon(this UserRole role)
        {
            return role switch
            {
                UserRole.User => "fas fa-user",
                UserRole.Administrator => "fas fa-user-shield",
                _ => "fas fa-question-circle"
            };
        }

        /// <summary>
        /// التحقق من صلاحية إدارة الكتب
        /// Check if role can manage books
        /// </summary>
        public static bool CanManageBooks(this UserRole role)
        {
            return role == UserRole.Administrator;
        }

        /// <summary>
        /// التحقق من صلاحية إدارة المستخدمين
        /// Check if role can manage users
        /// </summary>
        public static bool CanManageUsers(this UserRole role)
        {
            return role == UserRole.Administrator;
        }

        /// <summary>
        /// التحقق من صلاحية عرض الإحصائيات
        /// Check if role can view statistics
        /// </summary>
        public static bool CanViewStatistics(this UserRole role)
        {
            return role == UserRole.Administrator;
        }

        /// <summary>
        /// التحقق من صلاحية إدارة الاستعارات
        /// Check if role can manage borrowings
        /// </summary>
        public static bool CanManageBorrowings(this UserRole role)
        {
            return role == UserRole.Administrator;
        }

        /// <summary>
        /// التحقق من صلاحية الاستعارة
        /// Check if role can borrow books
        /// </summary>
        public static bool CanBorrowBooks(this UserRole role)
        {
            return true; // جميع الأدوار يمكنها الاستعارة - All roles can borrow
        }

        /// <summary>
        /// الحصول على جميع الصلاحيات للدور
        /// Get all permissions for role
        /// </summary>
        public static List<string> GetPermissions(this UserRole role)
        {
            var permissions = new List<string>();

            // الصلاحيات الأساسية لجميع المستخدمين
            // Basic permissions for all users
            permissions.AddRange(new[]
            {
                "search_books",
                "borrow_books",
                "return_books",
                "view_own_borrowings"
            });

            if (role.CanManageBorrowings())
            {
                permissions.AddRange(new[]
                {
                    "view_all_borrowings",
                    "extend_borrowings",
                    "manage_returns"
                });
            }

            if (role.CanManageBooks())
            {
                permissions.AddRange(new[]
                {
                    "add_books",
                    "edit_books",
                    "delete_books",
                    "manage_book_copies"
                });
            }

            if (role.CanViewStatistics())
            {
                permissions.Add("view_statistics");
            }

            if (role.CanManageUsers())
            {
                permissions.AddRange(new[]
                {
                    "add_users",
                    "edit_users",
                    "delete_users",
                    "manage_user_roles",
                    "view_all_users"
                });
            }

            return permissions;
        }


    }
}
