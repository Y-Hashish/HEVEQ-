# FINAL PATCH NOTES v12

## التعديلات المنفذة

1. **منع إنهاء الخدمة أثناء وجود طلب زيادة وقت غير محسوم**
   - تم تعديل Backend `CompleteBookingByProviderCommandHandler` حتى يمنع المزود من إرسال إثبات الإكمال إذا كان هناك طلب زيادة وقت بالحالات:
     - `Pending`: بانتظار رد العميل
     - `PendingPayment`: وافق العميل لكن الدفع لم يتم بعد
   - تمت إضافة رسائل عربية واضحة للفرونت بدل رسالة عامة.

2. **توضيح حالة طلب زيادة الوقت في صفحة الأعمال النشطة**
   - تم تعديل `GetProviderActiveJobsQueryHandler` لإرجاع:
     - `canRequestTimeAdjustment`
     - `hasBlockingTimeAdjustment`
     - `timeAdjustmentBlockReasonAr`
   - صفحة `/active-jobs` أصبحت تخفي قسم طلب زيادة الوقت إذا كانت الحالة `PendingCustomerConfirmation` أو بعد ذلك.
   - زر إنهاء الخدمة لا يظهر إذا كان هناك طلب زيادة وقت بانتظار رد العميل أو بانتظار الدفع.

3. **إصلاح قيمة الحجز والمزود في صفحة حجوزات العميل**
   - صفحة `/bookings` أصبحت تعرض قيمة الحجز من `estimatedTotal` إذا لم تكن `finalPrice` أو `totalPrice` موجودة.
   - بيانات المزود أصبحت تستخدم `providerCompany` إذا لم تكن `providerName` موجودة.

4. **حماية الحسابات غير النشطة**
   - تمت إضافة Middleware جديد `InactiveAccountGuardMiddleware`.
   - أي حساب Customer أو Provider غير نشط لا يمكنه تنفيذ عمليات شراء أو حجز أو إجراءات مزود.
   - المسموح للحساب غير النشط فقط:
     - التصفح والبحث GET
     - توثيق الحساب ورفع المستندات
     - التواصل مع الدعم عبر التذاكر
     - تحديث بيانات الملف المطلوبة للتوثيق

5. **إصلاح تعديل خدمات المزود من صفحة المعدات**
   - عند حفظ البيانات الأساسية في وضع التعديل، ينتقل المستخدم تلقائياً إلى خطوة الصور.
   - عند تعديل خدمة مرفوضة، لا يتم إرسالها تلقائياً للمراجعة. تعود إلى Draft حتى يضغط المزود صراحة على زر إرسال للمراجعة.
   - زر إرسال للمراجعة أصبح يعرض سبب النقص إذا كانت المتطلبات غير مكتملة بدل أن يبدو غير شغال.
   - تم تحويل متطلبات المراجعة إلى رسائل عربية.

## التحقق

- تم تشغيل TypeScript check بنجاح:

```bash
cd Frontend
./node_modules/.bin/tsc -p tsconfig.app.json --noEmit
```

- لم يتم تشغيل dotnet build لأن بيئة العمل الحالية لا تحتوي على .NET SDK.

## ملاحظات تشغيل

شغل محلياً:

```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
dotnet run --project HEVEQ.Api
```

ثم:

```bash
cd Frontend
npm install
npm start
```
