using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace HEVEQ.Infrastructure.Services.AI.ComplainSummarizing
{
    public class SemanticKernelComplaintAnalysisService : IComplaintAnalysisService
    {
        private readonly Kernel _kernel;

        public SemanticKernelComplaintAnalysisService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<ComplaintAnalysisResult> AnalyzeComplaintAsync(string complaintText, CancellationToken cancellationToken = default)
        {
            string promptTemplate = @"
                أنت مساعد ذكي في منصة HEVEQ لتأجير المعدات الثقيلة. مهمتك هي تحليل شكاوى العملاء والموردين.
                الشكوى الواردة قد تكون طويلة (تصل لـ 2000 حرف) ومفعمة بالمشاعر.

                المطلوب منك أمران:
                1. التلخيص (Summary): قم باختزال الشكوى إلى ملخص مهيكل ومحايد (من 3 إلى 5 جمل كحد أقصى). يجب أن يوضح الملخص بوضوح: المشكلة الأساسية، الأثر المترتب عليها، والأدلة المذكورة في النص (إن وجدت). تجاهل أي عواطف أو انفعالات.
                2. تصنيف الأولوية (Priority): حدد مستوى خطورة الشكوى بناءً على القواعد التالية حصراً:
                   - URGENT: إذا احتوت على قضايا نصب واحتيال، حوادث فيزيائية في الموقع، أو تهديدات صريحة.
                   - HIGH: إذا كانت تتعلق بمشاكل الدفع المالي، أو سوء سلوك صارخ من مشغلي المعدات (العمال) أثناء العمل.
                   - NORMAL: إذا كانت مجرد ملاحظات عامة على جودة الخدمة، تأخير طفيف في المواعيد، أو أعطال بسيطة تم تداركها.

                يجب أن تعيد النتيجة بصيغة JSON فقط بالبنية التالية:
                {
                  ""summary"": ""الملخص المحايد هنا"",
                  ""priority"": ""URGENT أو HIGH أو NORMAL""
                }

                نص الشكوى:
                {{$complaintText}}
                ";

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object",
                Temperature = 1.0
            };

            var arguments = new KernelArguments(executionSettings)
        {
            { "complaintText", complaintText }
        };

            var function = _kernel.CreateFunctionFromPrompt(promptTemplate);
            var response = await _kernel.InvokeAsync(function, arguments, cancellationToken);

            string jsonResult = response.GetValue<string>() ?? "{}";

            try
            {
                return JsonSerializer.Deserialize<ComplaintAnalysisResult>(jsonResult)
                       ?? new ComplaintAnalysisResult { Priority = "NORMAL", Summary = "لم يتمكن النظام من تحليل الشكوى." };
            }
            catch (JsonException)
            {
                // أمان إضافي في حالة فشل الـ Parsing
                return new ComplaintAnalysisResult
                {
                    Priority = "HIGH", // تحويلها لـ High كإجراء احترازي ليراجعها الأدمن
                    Summary = "حدث خطأ في استخراج الملخص من الذكاء الاصطناعي، يرجى قراءة النص الأصلي."
                };
            }
        }
    }
}
