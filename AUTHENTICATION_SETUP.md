# نظام المصادقة والصلاحيات - Authentication & Authorization System

## الأدوار المتاحة - Available Roles

### 1. مستخدم عادي - Regular User (Role = 1)
- البحث عن الكتب - Search books
- استعارة الكتب - Borrow books  
- إرجاع الكتب - Return books
- عرض الاستعارات الخاصة - View own borrowings

### 2. مدير النظام - Administrator (Role = 2)
- جميع صلاحيات المستخدم العادي - All user permissions
- إدارة الكتب (إضافة، تعديل، حذف) - Manage books (add, edit, delete)
- إدارة المستخدمين - Manage users
- إدارة الاستعارات - Manage borrowings
- عرض الإحصائيات - View statistics
- لوحة تحكم المدير - Admin dashboard

## بيانات تسجيل الدخول الافتراضية - Default Login Credentials

### مدير النظام - Administrator
- **البريد الإلكتروني:** admin@library.com
- **كلمة المرور:** admin123

### مستخدم عادي - Regular User
- **البريد الإلكتروني:** ahmed.mohamed@email.com
- **كلمة المرور:** user123

## الصفحات المتاحة - Available Pages

### للجميع - Public Pages
- `/Auth/Login` - تسجيل الدخول
- `/Auth/Register` - إنشاء حساب جديد

### للمستخدمين المسجلين - Authenticated Users
- `/Books` - البحث عن الكتب
- `/Borrowings` - إدارة الاستعارات
- `/Auth/Logout` - تسجيل الخروج

### للمديرين فقط - Administrators Only
- `/Admin/Dashboard` - لوحة تحكم المدير
- `/Users` - إدارة المستخدمين
- `/Auth/Register` (مع خيار تحديد الدور) - إضافة مستخدمين جدد

## كيفية تشغيل النظام - How to Run

1. **تشغيل التطبيق:**
   ```bash
   dotnet run --project LibraryManagementSystem.UI
   ```

2. **الوصول للتطبيق:**
   - افتح المتصفح على: `https://localhost:5001` أو `http://localhost:5000`
   - سيتم توجيهك تلقائياً لصفحة تسجيل الدخول

3. **تسجيل الدخول كمدير:**
   - استخدم البيانات: admin@library.com / admin123
   - ستصل إلى لوحة تحكم المدير

4. **تسجيل الدخول كمستخدم عادي:**
   - استخدم البيانات: ahmed.mohamed@email.com / user123
   - ستصل إلى صفحة البحث عن الكتب

## الميزات المنفذة - Implemented Features

### ✅ نظام المصادقة
- تسجيل الدخول والخروج
- تشفير كلمات المرور
- إدارة الجلسات
- التحقق من الصلاحيات

### ✅ إدارة الأدوار
- دورين: مستخدم عادي ومدير
- التحكم في الوصول للصفحات
- إخفاء الخيارات غير المسموحة

### ✅ واجهة المستخدم
- شريط تنقل ديناميكي
- عرض معلومات المستخدم
- رسائل النجاح والخطأ
- تصميم متجاوب

### ✅ الأمان
- التحقق في طبقة العرض (UI)
- التحقق في طبقة الأعمال (BLL)
- حماية من التلاعب

## ملاحظات مهمة - Important Notes

1. **كلمات المرور:** يتم تشفيرها باستخدام SHA256
2. **الجلسات:** تنتهي بعد 30 دقيقة من عدم النشاط
3. **قاعدة البيانات:** تستخدم SQL Server LocalDB
4. **الأمان:** يتم التحقق من الصلاحيات في كل طلب

## استكشاف الأخطاء - Troubleshooting

### مشكلة الاتصال بقاعدة البيانات
```bash
# تأكد من تشغيل LocalDB
sqllocaldb start mssqllocaldb

# إنشاء المدير إذا لم يكن موجوداً
sqlcmd -S "(localdb)\mssqllocaldb" -d "LibraryManagementSystem" -i "Database\AddAdminUser.sql"
```

### مشكلة في الصلاحيات
- تأكد من تسجيل الدخول بالحساب الصحيح
- تحقق من دور المستخدم في قاعدة البيانات
- امسح الجلسة وسجل دخول مرة أخرى
