using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using HEVEQ.Application.Features.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace HEVEQ.Application.Common.Services;

public class NotificationHelper
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationHelper> _logger;
    private readonly IRealtimeEventPublisher _realtimeEventPublisher;

    public NotificationHelper(IApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<NotificationHelper> logger, IRealtimeEventPublisher realtimeEventPublisher)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _realtimeEventPublisher = realtimeEventPublisher;
    }

    public void BookingRequested(Guid providerUserId, Guid bookingId, string bookingNumber, string serviceTitle)
        => Add(
            providerUserId,
            "BookingRequested",
            "طلب حجز جديد",
            $"لديك طلب حجز جديد رقم {bookingNumber} على خدمة \"{serviceTitle}\".",
            bookingId,
            "Booking");

    public void BookingAccepted(Guid customerId, Guid bookingId, string bookingNumber)
        => Add(
            customerId,
            "BookingAccepted",
            "تم قبول الحجز",
            $"تم قبول الحجز رقم {bookingNumber} من قبل مزود الخدمة.",
            bookingId,
            "Booking");

    public void BookingRejected(Guid customerId, Guid bookingId, string bookingNumber)
        => Add(
            customerId,
            "BookingRejected",
            "تم رفض الحجز",
            $"تم رفض الحجز رقم {bookingNumber} من قبل مزود الخدمة.",
            bookingId,
            "Booking");

    public void BookingStarted(Guid customerId, Guid bookingId, string bookingNumber)
        => Add(
            customerId,
            "BookingStarted",
            "بدأ تنفيذ الخدمة",
            $"تم بدء تنفيذ الحجز رقم {bookingNumber} من قبل مزود الخدمة.",
            bookingId,
            "Booking");

    public void BookingPaymentCaptured(Guid providerUserId, Guid bookingId, string bookingNumber)
        => Add(
            providerUserId,
            "BookingPaymentCaptured",
            "تم تأكيد الدفع",
            $"تم تأكيد الدفع للحجز رقم {bookingNumber}. يمكنك الآن البدء في تنفيذ الخدمة.",
            bookingId,
            "Booking");

    public void BookingCancelled(Guid userId, Guid bookingId, string bookingNumber, string cancelledBy)
        => Add(
            userId,
            "BookingCancelled",
            "تم إلغاء الحجز",
            $"تم إلغاء الحجز رقم {bookingNumber} بواسطة {cancelledBy}.",
            bookingId,
            "Booking");

    public void BookingCompletionSubmitted(Guid customerId, Guid bookingId, string bookingNumber)
        => Add(
            customerId,
            "BookingCompletionSubmitted",
            "بانتظار تأكيد إكمال الخدمة",
            $"قام مزود الخدمة بتحديد الحجز رقم {bookingNumber} كمكتمل. برجاء مراجعة الخدمة وتأكيد الإكمال.",
            bookingId,
            "Booking");

    public void BookingCompleted(Guid providerUserId, Guid bookingId, string bookingNumber)
        => Add(
            providerUserId,
            "BookingCompleted",
            "تم تأكيد إكمال الخدمة",
            $"قام العميل بتأكيد إكمال الحجز رقم {bookingNumber}.",
            bookingId,
            "Booking");

    public void BookingAutoCompleted(Guid providerUserId, Guid bookingId, string bookingNumber)
        => Add(
            providerUserId,
            "BookingAutoCompleted",
            "تم تأكيد إكمال الخدمة تلقائيًا",
            $"تم تأكيد إكمال الحجز رقم {bookingNumber} تلقائيًا بعد انتهاء مهلة مراجعة العميل.",
            bookingId,
            "Booking");

    public void BookingDisputed(Guid providerUserId, Guid bookingId, string bookingNumber)
        => Add(
            providerUserId,
            "BookingDisputed",
            "تم فتح نزاع على الحجز",
            $"تم فتح نزاع على الحجز رقم {bookingNumber}.",
            bookingId,
            "Booking");

    public async Task BookingDisputedForAdminsAsync(Guid bookingId, string bookingNumber)
        => await NotifyAdminsAsync(
            "NewBookingDispute",
            "نزاع جديد على حجز",
            $"تم فتح نزاع جديد على الحجز رقم {bookingNumber}.",
            bookingId,
            "Booking");

    public void TimeAdjustmentRequested(Guid customerId, Guid requestId, string bookingNumber, decimal additionalHours)
        => Add(
            customerId,
            "TimeAdjustmentRequested",
            "طلب تعديل وقت الخدمة",
            $"طلب مزود الخدمة إضافة {additionalHours} ساعة للحجز رقم {bookingNumber}.",
            requestId,
            "BookingTimeAdjustment");

    public void TimeAdjustmentApproved(Guid providerUserId, Guid requestId, string bookingNumber)
        => Add(
            providerUserId,
            "TimeAdjustmentApproved",
            "تمت الموافقة على تعديل الوقت",
            $"وافق العميل على طلب تعديل الوقت للحجز رقم {bookingNumber}.",
            requestId,
            "BookingTimeAdjustment");

    public void TimeAdjustmentRejected(Guid providerUserId, Guid requestId, string bookingNumber)
        => Add(
            providerUserId,
            "TimeAdjustmentRejected",
            "تم رفض تعديل الوقت",
            $"رفض العميل طلب تعديل الوقت للحجز رقم {bookingNumber}.",
            requestId,
            "BookingTimeAdjustment");

    public async Task DocumentUploadedForAdminsAsync(Guid documentId, string documentType)
        => await NotifyAdminsAsync(
            "DocumentUploaded",
            "مستند جديد بانتظار المراجعة",
            $"تم رفع مستند جديد من نوع {documentType} ويحتاج إلى المراجعة.",
            documentId,
            "Document");

    public void DocumentApproved(Guid userId, Guid documentId, string documentType)
        => Add(
            userId,
            "DocumentApproved",
            "تم قبول المستند",
            $"تم قبول المستند الخاص بك من نوع {documentType}.",
            documentId,
            "Document");

    public void DocumentRejected(Guid userId, Guid documentId, string documentType, string? reason)
        => Add(
            userId,
            "DocumentRejected",
            "تم رفض المستند",
            string.IsNullOrWhiteSpace(reason)
                ? $"تم رفض المستند الخاص بك من نوع {documentType}. برجاء مراجعته ورفعه مرة أخرى."
                : $"تم رفض المستند الخاص بك من نوع {documentType}. السبب: {reason}",
            documentId,
            "Document");

    public async Task ServiceListingSubmittedForAdminsAsync(Guid listingId, string listingTitle)
        => await NotifyAdminsAsync(
            "ServiceListingSubmittedForReview",
            "خدمة جديدة بانتظار المراجعة",
            $"تم إرسال الخدمة \"{listingTitle}\" للمراجعة.",
            listingId,
            "ServiceListing");

    public void ServiceListingApproved(Guid providerUserId, Guid listingId, string listingTitle)
        => Add(
            providerUserId,
            "ServiceListingApproved",
            "تم قبول الخدمة",
            $"تم قبول الخدمة \"{listingTitle}\" وأصبحت متاحة الآن على المنصة.",
            listingId,
            "ServiceListing");

    public void ServiceListingRejected(Guid providerUserId, Guid listingId, string listingTitle)
        => Add(
            providerUserId,
            "ServiceListingRejected",
            "تم رفض الخدمة",
            $"تم رفض الخدمة \"{listingTitle}\". برجاء مراجعة الملاحظات وإعادة الإرسال.",
            listingId,
            "ServiceListing");

    public async Task MarketplaceListingSubmittedForAdminsAsync(Guid listingId, string listingTitle)
        => await NotifyAdminsAsync(
            "MarketplaceListingSubmittedForReview",
            "منتج جديد بانتظار المراجعة",
            $"تم إرسال المنتج \"{listingTitle}\" للمراجعة.",
            listingId,
            "MarketplaceListing");

    public void MarketplaceListingApproved(Guid sellerId, Guid listingId, string listingTitle)
        => Add(
            sellerId,
            "MarketplaceListingApproved",
            "تم قبول المنتج",
            $"تم قبول المنتج \"{listingTitle}\" وأصبح متاحًا الآن في السوق.",
            listingId,
            "MarketplaceListing");

    public void MarketplaceListingRejected(Guid sellerId, Guid listingId, string listingTitle)
        => Add(
            sellerId,
            "MarketplaceListingRejected",
            "تم رفض المنتج",
            $"تم رفض المنتج \"{listingTitle}\". برجاء مراجعة الملاحظات وإعادة الإرسال.",
            listingId,
            "MarketplaceListing");

    public void MarketplaceOrderPaid(Guid sellerId, Guid orderId, string orderNumber)
        => Add(
            sellerId,
            "MarketplaceOrderPaid",
            "تم دفع قيمة الطلب",
            $"تم دفع قيمة الطلب رقم {orderNumber}. برجاء مراجعة الطلب وتأكيده.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderConfirmed(Guid buyerId, Guid orderId, string orderNumber)
        => Add(
            buyerId,
            "MarketplaceOrderConfirmed",
            "تم تأكيد الطلب",
            $"قام البائع بتأكيد الطلب رقم {orderNumber}.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderDispatched(Guid buyerId, Guid orderId, string orderNumber)
        => Add(
            buyerId,
            "MarketplaceOrderDispatched",
            "تم شحن الطلب",
            $"تم شحن الطلب رقم {orderNumber}.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderDelivered(Guid buyerId, Guid orderId, string orderNumber)
        => Add(
            buyerId,
            "MarketplaceOrderDelivered",
            "تم تسليم الطلب",
            $"تم تحديد الطلب رقم {orderNumber} كطلب تم تسليمه.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderCompleted(Guid sellerId, Guid orderId, string orderNumber)
        => Add(
            sellerId,
            "MarketplaceOrderCompleted",
            "تم إكمال الطلب",
            $"قام المشتري بتأكيد إكمال الطلب رقم {orderNumber}.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderAutoCompleted(Guid sellerId, Guid orderId, string orderNumber)
        => Add(
            sellerId,
            "MarketplaceOrderAutoCompleted",
            "تم إكمال الطلب تلقائيًا",
            $"تم تأكيد إكمال الطلب رقم {orderNumber} تلقائيًا بعد انتهاء مهلة مراجعة المشتري.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderCancelled(Guid userId, Guid orderId, string orderNumber, string cancelledBy)
        => Add(
            userId,
            "MarketplaceOrderCancelled",
            "تم إلغاء الطلب",
            $"تم إلغاء الطلب رقم {orderNumber} بواسطة {cancelledBy}.",
            orderId,
            "MarketplaceOrder");

    public void MarketplaceOrderDisputed(Guid sellerId, Guid orderId, string orderNumber)
        => Add(
            sellerId,
            "MarketplaceOrderDisputed",
            "تم فتح نزاع على الطلب",
            $"تم فتح نزاع على الطلب رقم {orderNumber}.",
            orderId,
            "MarketplaceOrder");

    public async Task MarketplaceOrderDisputedForAdminsAsync(Guid orderId, string orderNumber)
        => await NotifyAdminsAsync(
            "NewMarketplaceOrderDispute",
            "نزاع جديد على طلب",
            $"تم فتح نزاع جديد على الطلب رقم {orderNumber}.",
            orderId,
            "MarketplaceOrder");

    public async Task TicketCreatedForAdminsAsync(Guid ticketId, string ticketNumber, string subject)
        => await NotifyAdminsAsync(
            "TicketCreated",
            "تذكرة دعم جديدة",
            $"تم إنشاء تذكرة دعم جديدة رقم {ticketNumber}. الموضوع: {subject}.",
            ticketId,
            "Ticket");

    public void TicketReplied(Guid ticketOwnerId, Guid ticketId, string ticketNumber)
        => Add(
            ticketOwnerId,
            "TicketReplied",
            "تم الرد على تذكرتك",
            $"تم إضافة رد جديد على تذكرة الدعم رقم {ticketNumber}.",
            ticketId,
            "Ticket");

    public async Task TicketUserRepliedForAdminsAsync(Guid ticketId, string ticketNumber)
        => await NotifyAdminsAsync(
            "TicketUserReplied",
            "رد جديد على تذكرة دعم",
            $"قام المستخدم بإضافة رد جديد على تذكرة الدعم رقم {ticketNumber}.",
            ticketId,
            "Ticket");

    public void TicketAssigned(Guid staffUserId, Guid ticketId, string ticketNumber, string? assignedBy)
        => Add(
            staffUserId,
            "TicketAssigned",
            "تم تعيين تذكرة دعم لك",
            $"تم تعيين تذكرة الدعم رقم {ticketNumber} لك بواسطة {assignedBy ?? "النظام"}.",
            ticketId,
            "Ticket");

    public void TicketUnassigned(Guid staffUserId, Guid ticketId, string ticketNumber, string? unassignedBy)
        => Add(
            staffUserId,
            "TicketUnassigned",
            "تم إلغاء تعيين تذكرة دعم منك",
            $"تم إلغاء تعيين تذكرة الدعم رقم {ticketNumber} منك بواسطة {unassignedBy ?? "النظام"}.",
            ticketId,
            "Ticket");

    public void TicketRepliedToStaff(Guid staffUserId, Guid ticketId, string ticketNumber)
        => Add(
            staffUserId,
            "TicketUserReplied",
            "رد جديد على تذكرة دعم",
            $"قام المستخدم بإضافة رد جديد على تذكرة الدعم رقم {ticketNumber}.",
            ticketId,
            "Ticket");

    public void TicketResolved(Guid ticketOwnerId, Guid ticketId, string ticketNumber)
        => Add(
            ticketOwnerId,
            "TicketResolved",
            "تم حل تذكرة الدعم",
            $"تم حل تذكرة الدعم رقم {ticketNumber}.",
            ticketId,
            "Ticket");

    public void ReviewReceived(Guid userId, Guid reviewId, string referenceType)
        => Add(
            userId,
            "ReviewReceived",
            "تقييم جديد",
            "وصل إليك تقييم جديد من أحد المستخدمين.",
            reviewId,
            referenceType);

    public void NewMessageReceived(Guid receiverId, Guid messageId, string senderName)
        => Add(
            receiverId,
            "NewMessageReceived",
            "رسالة جديدة",
            $"لديك رسالة جديدة من {senderName}.",
            messageId,
            "Message");

    public void UserStatusChanged(Guid userId, bool isActive)
        => Add(
            userId,
            "UserStatusChanged",
            isActive ? "تم تفعيل حسابك" : "تم تعطيل حسابك",
            isActive
                ? "تم تفعيل حسابك مرة أخرى ويمكنك استخدام المنصة."
                : "تم تعطيل حسابك من قبل الإدارة. برجاء التواصل مع الدعم إذا كنت تعتقد أن هناك خطأ.",
            userId,
            "User");

    public void FieldVerificationAssigned(Guid employeeUserId, Guid formId, string bookingNumber)
        => Add(
            employeeUserId,
            "FieldVerificationAssigned",
            "تم تعيين مهمة تحقق ميداني",
            $"تم تعيينك لمراجعة تحقق ميداني خاصة بالحجز رقم {bookingNumber}.",
            formId,
            "FieldVerification");

    public void FieldVerificationDecisionMade(Guid userId, Guid formId, string bookingNumber)
        => Add(
            userId,
            "FieldVerificationDecisionMade",
            "تم إصدار قرار التحقق الميداني",
            $"تم إصدار قرار بخصوص التحقق الميداني المرتبط بالحجز رقم {bookingNumber}.",
            formId,
            "FieldVerification");

    public void DisputeResolved(Guid userId, Guid disputeId, string referenceNumber)
        => Add(
            userId,
            "DisputeResolved",
            "تم حل النزاع",
            $"تم حل النزاع المرتبط بالمرجع رقم {referenceNumber}.",
            disputeId,
            "Dispute");

    public void DisputeRefunded(Guid userId, Guid disputeId, string referenceNumber)
        => Add(
            userId,
            "DisputeRefunded",
            "تم رد المبلغ",
            $"تم رد المبلغ الخاص بالمرجع رقم {referenceNumber}.",
            disputeId,
            "Dispute");

    public void DisputeReleased(Guid userId, Guid disputeId, string referenceNumber)
        => Add(
            userId,
            "DisputeReleased",
            "تم تحرير المبلغ",
            $"تم تحرير المبلغ الخاص بالمرجع رقم {referenceNumber}.",
            disputeId,
            "Dispute");

    public void DisputePartiallySettled(Guid userId, Guid disputeId, string referenceNumber)
        => Add(
            userId,
            "DisputePartiallySettled",
            "تمت تسوية النزاع جزئيًا",
            $"تمت التسوية الجزئية للنزاع المرتبط بالمرجع رقم {referenceNumber}.",
            disputeId,
            "Dispute");

    public void EscrowReleased(Guid providerUserId, Guid escrowId, string referenceNumber)
        => Add(
            providerUserId,
            "EscrowReleased",
            "تم تحرير المبلغ المستحق",
            $"تم تحرير المبلغ المستحق الخاص بالمرجع رقم {referenceNumber}.",
            escrowId,
            "Escrow");

    public void ProviderUnresponsive(Guid customerId, Guid bookingId, string bookingNumber)
        => Add(
            customerId,
            "ProviderUnresponsive",
            "لم يتم الرد على الحجز",
            $"لم يقم مزود الخدمة بالرد على الحجز رقم {bookingNumber} خلال المهلة المحددة.",
            bookingId,
            "Booking");

    private async Task NotifyAdminsAsync(
        string eventType,
        string title,
        string body,
        Guid referenceId,
        string referenceType)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");

        foreach (var admin in admins.Where(x => x.IsActive))
        {
            Add(
                admin.Id,
                eventType,
                title,
                body,
                referenceId,
                referenceType);
        }
    }

    private void Add(Guid userId, string eventType, string title, string body, Guid referenceId, string referenceType)
    {
        var notification = new Notification
        {
            UserId = userId,
            EventType = eventType,
            Title = title,
            Body = body,
            ReferenceId = referenceId.ToString(),
            ReferenceType = referenceType,
            IsRead = false,
            Channel = NotificationChannel.InApp,
            SentAt = DateTime.UtcNow,
            ReadAt = null
        };

        _context.Notifications.Add(notification);

        var realtimeNotification = new NotificationItemDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Body = notification.Body,
            EventType = notification.EventType,
            ReferenceId = notification.ReferenceId,
            ReferenceType = notification.ReferenceType,
            IsRead = notification.IsRead,
            SentAt = notification.SentAt
        };

        _ = PublishNotificationSafelyAsync(userId, realtimeNotification);
    }

    private async Task PublishNotificationSafelyAsync(Guid userId, NotificationItemDto notification)
    {
        try
        {
            await _realtimeEventPublisher.SendNotificationToUserAsync(userId, notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish realtime notification {NotificationId} to user {UserId}.", notification.Id, userId);
        }
    }
}