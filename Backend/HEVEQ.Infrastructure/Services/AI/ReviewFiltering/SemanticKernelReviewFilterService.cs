using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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

            string promptTemplate = @"
                أنت نظام خبير في الإشراف على المحتوى وفلترة تقييمات منصة HEVEQ (منصة مشاركة وتأجير المعدات الثقيلة).
                مهمتك هي تحليل التقييم الوارد والتحقق من ثلاثة معايير أساسية:
                1. خلو التقييم تماماً من الألفاظ الخارجة، البذيئة، أو غير اللائقة.
                2. خلو التقييم من أرقام التواصل (مثل أرقام الهواتف، الإيميلات، أو روابط خارجية).
                3. ملاءمة السياق (شذوذ التقييم): قارن محتوى التقييم بنوع الخدمة أو المعدة التي يتم تقييمها حالياً.
                   - نوع الخدمة/المعدة الحالي في المنصة هو: {{$serviceType}}
                   - إذا كان التقييم يتحدث عن شيء مختلف تماماً لا علاقة له بالمعدة أو المنصة (مثال: 'محل أثاث ممتاز'، 'مطعم رائع'، 'شراء ملابس')، فهذا يعتبر شذوذاً واضحاً وتقييماً وهمياً ويجب إحالته للأدمن فوراً.

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
    }
}
