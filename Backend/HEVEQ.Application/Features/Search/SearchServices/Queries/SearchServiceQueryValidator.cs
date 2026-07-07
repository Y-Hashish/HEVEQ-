using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Search.SearchServices.Queries;

    public sealed class SearchServicesQueryValidator : AbstractValidator<SearchServicesQuery>
    {
        public SearchServicesQueryValidator()
        {
            RuleFor(x => x.RawQuery)
                .NotEmpty()
                .WithMessage("عفواً، لا يمكن ترك خانة البحث فارغة.")
                .MaximumLength(500)
                .WithMessage("نص البحث طويل جداً، يرجى اختصار الطلب لضمان دقة النتائج.");

            
            RuleFor(x => x.SessionId)
                .MaximumLength(100)
                .WithMessage("معرف الجلسة غير صالح.");

        
            RuleForEach(x => x.ConversationHistory)
                .ChildRules(turn =>
                {
                    turn.RuleFor(t => t.Role)
                        .Must(role => role == "user" || role == "assistant")
                        .WithMessage("دور المتحدث يجب أن يكون 'user' أو 'assistant' فقط.");

                    turn.RuleFor(t => t.Content)
                        .NotEmpty()
                        .WithMessage("محتوى المحادثة السابقة لا يمكن أن يكون فارغاً.");
                });
        }
    }

