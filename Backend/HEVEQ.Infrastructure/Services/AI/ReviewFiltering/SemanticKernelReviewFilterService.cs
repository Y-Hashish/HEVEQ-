using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace HEVEQ.Infrastructure.Services.AI.ReviewFiltering
{
    public class SemanticKernelReviewFilterService : IReviewFilterService
    {
        private readonly Kernel _kernel;

        public SemanticKernelReviewFilterService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<ReviewFilterResult> FilterReviewAsync(string reviewText, string serviceType, CancellationToken cancellationToken = default)
        {
            var localRuleResult = ApplyLocalSafetyRules(reviewText, serviceType);
            if (localRuleResult is not null)
            {
                return localRuleResult;
            }

            string promptTemplate = @"
                أنت نظام خبير في الإشراف على المحتوى وفلترة تقييمات منصة HEVEQ (منصة مشاركة وتأجير المعدات الثقيلة).
                مهمتك هي تحليل التقييم الوارد والتحقق من أربعة معايير أساسية:
                1. خلو التقييم تماماً من الألفاظ الخارجة، البذيئة، السباب، الإهانات، أو أي لغة غير لائقة.
                2. خلو التقييم من أي محاولة تواصل خارج المنصة، مثل أرقام الهواتف، البريد الإلكتروني، واتساب، تيليجرام، روابط خارجية، أسماء حسابات، أو عبارات مثل اتصل بي أو كلمني خارج المنصة.
                3. ملاءمة السياق (شذوذ التقييم): قارن محتوى التقييم بنوع الخدمة أو المعدة التي يتم تقييمها حالياً.
                   - نوع الخدمة/المعدة الحالي في المنصة هو: {{$serviceType}}
                   - إذا كان التقييم يتحدث عن شيء مختلف تماماً لا علاقة له بالمعدة أو المنصة (مثال: 'الأكل كان ممتاز'، 'مطعم رائع'، 'شراء ملابس'، 'محل أثاث ممتاز')، فهذا يعتبر شذوذاً واضحاً وتقييماً وهمياً ويجب إحالته للأدمن فوراً.
                4. لا ترفض التقييم لمجرد أنه سلبي، ارفضه فقط إذا كان مخالفاً أو خارج السياق أو يحتوي محاولة تواصل خارج المنصة.

                يجب أن تعيد النتيجة بصيغة JSON حصراً بالبنية التالية وبدون أي مقدمات أو نصوص إضافية:
                {
                  ""isApproved"": true/false,
                  ""requiresAdminReview"": true/false,
                  ""reason"": ""سبب تفصيلي باللغة العربية يشرح المشكلة المكتشفة، أو يكتب 'سليم' إذا كان مقبولاً""
                }

                نص التقييم المراد فحصه:
                {{$reviewText}}
                ";

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object",
                Temperature = 1.0 ,
            };

            var arguments = new KernelArguments(executionSettings)
        {
            { "serviceType", serviceType },
            { "reviewText", reviewText }
        };

            var function = _kernel.CreateFunctionFromPrompt(promptTemplate);
            var response = await _kernel.InvokeAsync(function, arguments, cancellationToken);

            string jsonResult = response.GetValue<string>() ?? "{}";

            try
            {
                return JsonSerializer.Deserialize<ReviewFilterResult>(jsonResult)
                       ?? new ReviewFilterResult { RequiresAdminReview = true, Reason = "فشل في معالجة البيانات" };
            }
            catch (JsonException)
            {
                return new ReviewFilterResult
                {
                    IsApproved = false,
                    RequiresAdminReview = true,
                    Reason = "رد الـ AI غير متوافق مع صيغة النظام المعياري."
                };
            }
        }

        private static ReviewFilterResult? ApplyLocalSafetyRules(string reviewText, string serviceType)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
            {
                return null;
            }

            var normalized = reviewText.Trim().ToLowerInvariant();

            var contactPatterns = new[]
            {
                @"\b01[0125]\d{8}\b",
                @"\+?20\s?1[0125]\d{8}",
                @"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}",
                @"https?://",
                @"www\.",
                @"\b(whatsapp|واتساب|واتس|تليجرام|telegram|فيسبوك|facebook|انستجرام|instagram)\b",
                @"(اتصل|كلمني|تواصل|ابعتلي|راسلني|رقمي|رقم الهاتف|خارج المنصة)"
            };

            if (contactPatterns.Any(pattern => Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
            {
                return new ReviewFilterResult
                {
                    IsApproved = false,
                    RequiresAdminReview = true,
                    Reason = "التقييم يحتوي على محاولة تواصل خارج المنصة أو بيانات تواصل."
                };
            }

            var profanityWords = new[]
            {
                "وسخة", "وسخ", "كلب", "حمار", "غبي", "حقير", "زبالة", "قذر",
                "يلعن", "لعنة", "خرا", "عرص", "شرموط", "متناك"
            };

            if (profanityWords.Any(word => normalized.Contains(word)))
            {
                return new ReviewFilterResult
                {
                    IsApproved = false,
                    RequiresAdminReview = true,
                    Reason = "التقييم يحتوي على ألفاظ غير لائقة أو سباب."
                };
            }

            var unrelatedFoodTerms = new[]
            {
                "الأكل", "اكل", "طعام", "مطعم", "وجبة", "ساندوتش", "بيتزا", "كشري", "مشروب", "المذاق", "الطعم"
            };

            var serviceLooksIndustrial = serviceType.Contains("معدات") || serviceType.Contains("تأجير") || serviceType.Contains("سوق") ||
                                         serviceType.Contains("حفر") || serviceType.Contains("ونش") || serviceType.Contains("مولد") ||
                                         serviceType.Contains("لودر") || serviceType.Contains("حفار");

            if (serviceLooksIndustrial && unrelatedFoodTerms.Any(term => normalized.Contains(term)))
            {
                return new ReviewFilterResult
                {
                    IsApproved = false,
                    RequiresAdminReview = true,
                    Reason = "محتوى التقييم يبدو غير متعلق بالخدمة أو المعدة التي تم تقييمها."
                };
            }

            return null;
        }

    }
}
