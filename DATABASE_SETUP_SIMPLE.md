# إعداد قاعدة البيانات البسيط - Simple Database Setup

## نظرة عامة - Overview

تم تصميم هذا النظام لإنشاء قاعدة البيانات تلقائياً عند أول اتصال بدون الحاجة لمهيئ معقد.
This system is designed to automatically create the database on first connection without needing a complex initializer.

## المتطلبات - Requirements

1. **SQL Server LocalDB** أو **SQL Server Express** أو **SQL Server**
2. **.NET 9.0**
3. **Visual Studio 2022** أو **VS Code**

## الإعداد السريع - Quick Setup

### 1. تحديث سلسلة الاتصال - Update Connection String

في ملف `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementSystem;Integrated Security=true;TrustServerCertificate=True"
  }
}
```

### 2. تشغيل التطبيق - Run Application

```bash
dotnet run --project LibraryManagementSystem.UI
```

## كيف يعمل - How It Works

1. **الاتصال التلقائي**: عند أول محاولة اتصال، إذا لم تكن قاعدة البيانات موجودة، سيتم إنشاؤها تلقائياً
2. **إنشاء الجداول**: سيتم إنشاء الجداول الأساسية تلقائياً:
   - `Users` - جدول المستخدمين
   - `Books` - جدول الكتب  
   - `Borrowings` - جدول الاستعارات
3. **الفهارس**: سيتم إنشاء فهارس أساسية للبحث السريع
4. **البيانات الأولية**: سيتم إدراج بيانات تجريبية أولية

## الجداول المنشأة - Created Tables

### جدول المستخدمين - Users Table
- `UserId` (Primary Key)
- `FirstName`, `LastName`
- `Email` (Unique)
- `PhoneNumber`, `Address`
- `MembershipDate`, `IsActive`
- `CreatedDate`, `ModifiedDate`

### جدول الكتب - Books Table
- `BookId` (Primary Key)
- `Title`, `Author`, `ISBN` (Unique)
- `Publisher`, `PublicationYear`, `Genre`
- `TotalCopies`, `AvailableCopies`
- `Description`
- `CreatedDate`, `ModifiedDate`

### جدول الاستعارات - Borrowings Table
- `BorrowingId` (Primary Key)
- `UserId` (Foreign Key), `BookId` (Foreign Key)
- `BorrowDate`, `DueDate`, `ReturnDate`
- `IsReturned`, `LateFee`, `Notes`
- `CreatedDate`, `ModifiedDate`

## البيانات الأولية - Initial Data

سيتم إدراج البيانات التالية تلقائياً:

### المستخدمين - Users
- Ahmed Mohamed (ahmed.mohamed@email.com)
- Fatima Ali (fatima.ali@email.com)
- Mohamed Alsaeed (mohamed.alsaeed@email.com)

### الكتب - Books
- Clean Code by Robert C. Martin
- Design Patterns by Gang of Four
- The Pragmatic Programmer by Andrew Hunt

## استكشاف الأخطاء - Troubleshooting

### مشكلة: قاعدة البيانات لا تُنشأ
**الحل**: تأكد من أن SQL Server LocalDB مثبت ويعمل:
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

### مشكلة: خطأ في الاتصال
**الحل**: تحقق من سلسلة الاتصال في `appsettings.json`

### مشكلة: الجداول لا تُنشأ
**الحل**: احذف قاعدة البيانات وأعد تشغيل التطبيق:
```bash
sqllocaldb delete mssqllocaldb
sqllocaldb create mssqllocaldb
```

## الميزات - Features

✅ **إنشاء تلقائي**: لا حاجة لتشغيل سكريبت منفصل
✅ **بيانات أولية**: بيانات تجريبية جاهزة للاختبار
✅ **فهارس محسنة**: للبحث السريع
✅ **مقاوم للأخطاء**: يتعامل مع الأخطاء بأمان
✅ **بسيط**: بدون تعقيدات غير ضرورية

## ملاحظات مهمة - Important Notes

- سيتم إنشاء قاعدة البيانات في أول اتصال فقط
- إذا كانت قاعدة البيانات موجودة، لن يتم تعديلها
- البيانات الأولية تُدرج فقط إذا كانت الجداول فارغة
- النظام يعمل مع أي مستخدم Windows بدون إعدادات خاصة

## الدعم - Support

إذا واجهت أي مشاكل، تأكد من:
1. تثبيت SQL Server LocalDB
2. صحة سلسلة الاتصال
3. صلاحيات الكتابة في مجلد التطبيق
