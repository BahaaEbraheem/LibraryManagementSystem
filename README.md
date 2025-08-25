# نظام إدارة المكتبة - Library Management System

## نظرة عامة - Overview

نظام شامل لإدارة المكتبات مبني باستخدام معمارية ثلاثية الطبقات مع .NET، ADO.NET، Razor Pages، والتخزين المؤقت.

A comprehensive library management system built using 3-tier architecture with .NET, ADO.NET, Razor Pages, and caching.

## الميزات المنجزة - Completed Features

### 🗄️ طبقة الوصول للبيانات (DAL)
- ✅ **نماذج البيانات** - Data Models with Arabic comments
  - `User` - نموذج المستخدم
  - `Book` - نموذج الكتاب  
  - `Borrowing` - نموذج الاستعارة
  - DTOs للبحث والتقسيم على صفحات

- ✅ **إدارة قاعدة البيانات** - Database Management
  - `DatabaseConnectionFactory` - مصنع الاتصالات
  - `DatabaseHelper` - مساعد العمليات
  - إدارة الاتصالات والمعاملات

- ✅ **نمط المستودع** - Repository Pattern
  - `IBookRepository` & `BookRepository` - مستودع الكتب
  - عمليات CRUD كاملة
  - البحث المتقدم مع التقسيم على صفحات
  - التحقق من التوفر

- ✅ **نظام التخزين المؤقت** - Caching System
  - `ICacheService` & `MemoryCacheService`
  - مفاتيح التخزين المؤقت المنظمة
  - إستراتيجيات إلغاء الصحة
  - تحسين الأداء

### 🎨 طبقة العرض (UI) - Razor Pages
- ✅ **تكوين المشروع** - Project Configuration
  - تحويل إلى Razor Pages
  - حقن التبعيات
  - إعدادات التخزين المؤقت

- ✅ **التخطيط والتصميم** - Layout & Design
  - تخطيط رئيسي مع دعم RTL
  - Bootstrap 5 مع دعم العربية
  - تصميم متجاوب
  - رسائل التنبيه والأخطاء

- ✅ **صفحة البحث عن الكتب** - Book Search Page
  - نموذج بحث متقدم
  - عرض النتائج مع التقسيم
  - ترتيب وتصفية
  - واجهة مستخدم تفاعلية

### 🗃️ قاعدة البيانات والفهرسة
- ✅ **مخطط قاعدة البيانات** - Database Schema
  - جداول Books, Users, Borrowings
  - القيود والعلاقات
  - المشغلات التلقائية
  - بيانات تجريبية

- ✅ **الفهرسة المتقدمة** - Advanced Indexing
  - فهارس مركبة للبحث
  - فهارس تغطية للأداء
  - فهارس مصفاة للاستعلامات المحددة
  - فهارس فريدة للقيود

## البنية التقنية - Technical Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    طبقة العرض - UI Layer                     │
│                     (Razor Pages)                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Books     │ │ Borrowings  │ │    Users    │           │
│  │   Pages     │ │    Pages    │ │    Pages    │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 طبقة منطق الأعمال - BLL                      │
│                   (Business Logic)                         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ BookService │ │BorrowService│ │ UserService │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                طبقة الوصول للبيانات - DAL                   │
│                 (Data Access Layer)                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │Book         │ │Borrowing    │ │User         │           │
│  │Repository   │ │Repository   │ │Repository   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
│                                                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │Cache        │ │Database     │ │Models &     │           │
│  │Service      │ │Connection   │ │DTOs         │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    قاعدة البيانات - Database                 │
│                      (SQL Server)                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Books     │ │ Borrowings  │ │    Users    │           │
│  │   Table     │ │    Table    │ │    Table    │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
```

## التقنيات المستخدمة - Technologies Used

- **Framework**: .NET 9.0
- **UI**: ASP.NET Core Razor Pages
- **Data Access**: ADO.NET with SQL Server
- **Caching**: IMemoryCache
- **Database**: SQL Server with LocalDB
- **Frontend**: Bootstrap 5 RTL, Font Awesome
- **Architecture**: 3-Tier Architecture
- **Patterns**: Repository Pattern, Dependency Injection

## الميزات الرئيسية - Key Features

### 🔍 البحث المتقدم - Advanced Search
- البحث بالعنوان، المؤلف، ISBN، النوع
- تصفية حسب التوفر
- ترتيب متعدد المعايير
- تقسيم النتائج على صفحات
- تخزين مؤقت للنتائج

### 📚 إدارة الكتب - Book Management
- عرض تفاصيل الكتب
- تتبع النسخ المتاحة والمستعارة
- إحصائيات شاملة
- اقتراحات البحث التلقائي

### 💾 التخزين المؤقت الذكي - Intelligent Caching
- تخزين مؤقت هرمي
- إلغاء صحة تلقائي
- مفاتيح منظمة
- تحسين الأداء

### 🗃️ فهرسة متقدمة - Advanced Indexing
- فهارس مركبة للبحث السريع
- فهارس تغطية للاستعلامات المعقدة
- فهارس مصفاة للبيانات المحددة
- تحسين أداء قاعدة البيانات

## التشغيل - Getting Started

### المتطلبات - Prerequisites
- .NET 9.0 SDK
- SQL Server أو LocalDB
- Visual Studio 2022 أو VS Code

### خطوات التشغيل - Setup Steps

1. **استنساخ المشروع** - Clone the project
```bash
git clone [repository-url]
cd LibraryManagementSystem
```

2. **إنشاء قاعدة البيانات** - Create database
```sql
-- تشغيل ملف Database/CreateDatabase.sql
-- Run Database/CreateDatabase.sql

-- تشغيل ملف Database/SampleData.sql  
-- Run Database/SampleData.sql

-- تشغيل ملف Database/AdvancedIndexing.sql
-- Run Database/AdvancedIndexing.sql
```

3. **تحديث سلسلة الاتصال** - Update connection string
```json
// في appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementSystem;Trusted_Connection=true"
}
```

4. **تشغيل التطبيق** - Run the application
```bash
cd LibraryManagementSystem.UI
dotnet run
```

## المهام المتبقية - Remaining Tasks

### 🔄 طبقة منطق الأعمال - Business Logic Layer
- [ ] إنشاء خدمات الأعمال (BookService, UserService, BorrowingService)
- [ ] تنفيذ قواعد العمل والتحقق
- [ ] إدارة المعاملات

### 👥 إدارة المستخدمين - User Management  
- [ ] مستودع المستخدمين
- [ ] صفحات إدارة المستخدمين
- [ ] نظام المصادقة

### 📖 إدارة الاستعارات - Borrowing Management
- [ ] مستودع الاستعارات  
- [ ] صفحات الاستعارة والإرجاع
- [ ] تتبع المواعيد والغرامات

### 📊 الإحصائيات والتقارير - Statistics & Reports
- [ ] لوحة تحكم الإحصائيات
- [ ] تقارير الاستعارات
- [ ] تحليلات الاستخدام

### 🧪 الاختبارات - Testing
- [ ] اختبارات الوحدة
- [ ] اختبارات التكامل
- [ ] اختبارات الأداء

### 🔧 التحسينات - Enhancements
- [ ] معالجة الأخطاء الشاملة
- [ ] التسجيل المتقدم
- [ ] التوثيق الكامل

## المساهمة - Contributing

نرحب بالمساهمات! يرجى اتباع الإرشادات التالية:

1. Fork المشروع
2. إنشاء فرع للميزة الجديدة
3. إضافة التعليقات باللغتين العربية والإنجليزية
4. اختبار التغييرات
5. إرسال Pull Request

## الترخيص - License

هذا المشروع مرخص تحت رخصة MIT - انظر ملف LICENSE للتفاصيل.

---

**تم تطوير هذا النظام باستخدام أفضل الممارسات في البرمجة والتعليقات ثنائية اللغة لضمان سهولة الفهم والصيانة.**

*This system was developed using best programming practices and bilingual comments to ensure ease of understanding and maintenance.*
