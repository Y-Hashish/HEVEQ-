using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.Query.GetFieldVerificationDetails
{
    public class GetFieldVerificationDetailsQuery : IRequest<FieldVerificationDetailsDto?>
    {
        public Guid Id { get; set; }

        public GetFieldVerificationDetailsQuery() {}

        public GetFieldVerificationDetailsQuery(Guid id)
        {
            Id = id;
        }
    }
}
