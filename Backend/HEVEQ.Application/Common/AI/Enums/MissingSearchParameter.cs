using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Enums
{
    /// <summary>Which mandatory search parameter is still missing after intent extraction,
    /// or was extracted but could not be validated against the supported service area.</summary>
    public enum MissingSearchParameter
    {
        SearchTarget,
        EquipmentType,  // equipment_type is null — cannot route to a category
        Location,       // location is null — cannot check service zones
        InvalidLocation // location was extracted but is not a recognised Egyptian governorate
    }

}
