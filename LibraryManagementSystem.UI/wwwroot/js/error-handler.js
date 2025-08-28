/**
 * معالج الأخطاء العام للجانب العميل
 * Global client-side error handler
 */

(function() {
    'use strict';

    // معالج الأخطاء العام لـ JavaScript
    // Global JavaScript error handler
    window.addEventListener('error', function(event) {
        console.error('JavaScript Error:', {
            message: event.message,
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno,
            error: event.error
        });

        // إرسال الخطأ للخادم (اختياري)
        // Send error to server (optional)
        if (typeof reportError === 'function') {
            reportError({
                type: 'javascript',
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                stack: event.error ? event.error.stack : null,
                userAgent: navigator.userAgent,
                url: window.location.href,
                timestamp: new Date().toISOString()
            });
        }
    });

    // معالج الأخطاء للوعود المرفوضة
    // Unhandled promise rejection handler
    window.addEventListener('unhandledrejection', function(event) {
        console.error('Unhandled Promise Rejection:', event.reason);

        // إرسال الخطأ للخادم (اختياري)
        // Send error to server (optional)
        if (typeof reportError === 'function') {
            reportError({
                type: 'promise',
                message: event.reason ? event.reason.toString() : 'Unhandled promise rejection',
                stack: event.reason && event.reason.stack ? event.reason.stack : null,
                userAgent: navigator.userAgent,
                url: window.location.href,
                timestamp: new Date().toISOString()
            });
        }
    });

    // معالج أخطاء AJAX
    // AJAX error handler
    $(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
        console.error('AJAX Error:', {
            url: ajaxSettings.url,
            type: ajaxSettings.type,
            status: jqXHR.status,
            statusText: jqXHR.statusText,
            responseText: jqXHR.responseText,
            thrownError: thrownError
        });

        // عرض رسالة خطأ للمستخدم
        // Show error message to user
        let errorMessage = 'حدث خطأ في الاتصال - Connection error occurred';
        
        if (jqXHR.status === 0) {
            errorMessage = 'فشل في الاتصال بالخادم - Failed to connect to server';
        } else if (jqXHR.status === 404) {
            errorMessage = 'المورد المطلوب غير موجود - Requested resource not found';
        } else if (jqXHR.status === 500) {
            errorMessage = 'خطأ في الخادم - Server error';
        } else if (jqXHR.status === 403) {
            errorMessage = 'غير مصرح بالوصول - Access forbidden';
        } else if (jqXHR.status === 401) {
            errorMessage = 'يجب تسجيل الدخول - Authentication required';
        }

        // محاولة الحصول على رسالة خطأ من الاستجابة
        // Try to get error message from response
        try {
            const response = JSON.parse(jqXHR.responseText);
            if (response && response.message) {
                errorMessage = response.message;
            }
        } catch (e) {
            // تجاهل أخطاء تحليل JSON
            // Ignore JSON parsing errors
        }

        // عرض التنبيه
        // Show alert
        showErrorAlert(errorMessage);

        // إرسال الخطأ للخادم (اختياري)
        // Send error to server (optional)
        if (typeof reportError === 'function') {
            reportError({
                type: 'ajax',
                url: ajaxSettings.url,
                method: ajaxSettings.type,
                status: jqXHR.status,
                statusText: jqXHR.statusText,
                responseText: jqXHR.responseText,
                thrownError: thrownError,
                userAgent: navigator.userAgent,
                timestamp: new Date().toISOString()
            });
        }
    });

    // دالة عرض تنبيه الخطأ
    // Show error alert function
    function showErrorAlert(message) {
        // إزالة التنبيهات السابقة
        // Remove previous alerts
        $('.alert-ajax-error').remove();

        // إنشاء تنبيه جديد
        // Create new alert
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show alert-ajax-error" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        // إضافة التنبيه في أعلى الصفحة
        // Add alert at top of page
        $('main.container').prepend(alertHtml);

        // إزالة التنبيه تلقائياً بعد 10 ثوان
        // Auto-remove alert after 10 seconds
        setTimeout(function() {
            $('.alert-ajax-error').fadeOut();
        }, 10000);
    }


})();
