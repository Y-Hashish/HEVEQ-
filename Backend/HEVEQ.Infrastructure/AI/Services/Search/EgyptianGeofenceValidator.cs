using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEVEQ.Infrastructure.AI.Services.Search;

public sealed class EgyptianGeofenceValidator : IGeofenceValidator
{
    private static readonly string[] CanonicalNames =
    {
        "Cairo",        "Alexandria",    "Giza",         "Qalyubia",
        "Port Said",    "Suez",          "Ismailia",     "Dakahlia",
        "Gharbia",      "Menoufia",      "Beheira",      "Kafr El Sheikh",
        "Damietta",     "Sharqia",       "Faiyum",       "Beni Suef",
        "Minya",        "Asyut",         "Sohag",        "Qena",
        "Aswan",        "Luxor",         "Red Sea",      "New Valley",
        "Matrouh",      "North Sinai",   "South Sinai"
    };

    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "القاهرة",        "Cairo"         }, { "Cario",    "Cairo" },
            { "Kahira",        "Cairo"          }, { "Al Qahira","Cairo" },
            { "الإسكندرية",    "Alexandria"    }, { "Alex",  "Alexandria" },
            { "Eskendereyya",  "Alexandria"    }, { "Iskandariyya", "Alexandria" },
            { "الجيزة",        "Giza"          }, { "Gizah",  "Giza"     },
            { "القليوبية",     "Qalyubia"      }, { "Qalyubiyya","Qalyubia"},
            { "Qalioubia",     "Qalyubia"      }, { "Qualyubia",  "Qalyubia"},
            { "بورسعيد",       "Port Said"     }, { "Borsaid", "Port Said"},
            { "السويس",        "Suez"          }, { "Sewes",   "Suez"     },
            { "الإسماعيلية",   "Ismailia"      }, { "Ismailiya","Ismailia" },
            { "الدقهلية",      "Dakahlia"      }, { "Daqahliyya","Dakahlia"},
            { "Dakahleya",     "Dakahlia"      },
            { "الغربية",       "Gharbia"       }, { "Gharbiyya","Gharbia"  },
            { "طنطا",          "Gharbia"       }, { "Tanta",    "Gharbia"  },
            { "المنوفية",      "Menoufia"      }, { "Menofia", "Menoufia" },
            { "Munufiyya",     "Menoufia"      }, { "Menufiya","Menoufia"  },
            { "البحيرة",       "Beheira"       }, { "El Beheira","Beheira" },
            { "Buhaira",       "Beheira"       },
            { "كفر الشيخ",     "Kafr El Sheikh"}, { "Kafr Elshikh","Kafr El Sheikh"},
            { "Kafr El Shiekh","Kafr El Sheikh"},
            { "دمياط",         "Damietta"      }, { "Dumyat", "Damietta"  },
            { "الشرقية",       "Sharqia"       }, { "Sharkia","Sharqia"   },
            { "Sharqeya",      "Sharqia"       },
            { "الفيوم",        "Faiyum"        }, { "Fayoum", "Faiyum"    },
            { "Fayum",         "Faiyum"        },
            { "بني سويف",      "Beni Suef"     }, { "Bni Swef","Beni Suef"},
            { "Beni Sweif",    "Beni Suef"     },
            { "المنيا",        "Minya"         }, { "Minia",  "Minya"     },
            { "أسيوط",         "Asyut"         }, { "Assiut", "Asyut"     },
            { "Assiout",       "Asyut"         }, { "Asyyut", "Asyut"     },
            { "سوهاج",         "Sohag"         }, { "Suhaj",  "Sohag"     },
            { "قنا",           "Qena"          }, { "Kena",   "Qena"      },
            { "أسوان",         "Aswan"         }, { "Aswān",  "Aswan"     },
            { "الأقصر",        "Luxor"         }, { "Luksor", "Luxor"     },
            { "Al Uqsur",      "Luxor"         },
            { "البحر الأحمر",  "Red Sea"       }, { "Bahr Al Ahmar","Red Sea"},
            { "Redsea",        "Red Sea"       },
            { "الوادي الجديد", "New Valley"    }, { "Wadi El Gedid","New Valley"},
            { "مطروح",         "Matrouh"       }, { "Matruh",   "Matrouh"},
            { "Marsa Matrouh", "Matrouh"       },
            { "شمال سيناء",    "North Sinai"   }, { "Shamal Sinai","North Sinai"},
            { "جنوب سيناء",    "South Sinai"   }, { "Ganob Sinai", "South Sinai"}
        };

    private static readonly HashSet<string> CanonicalSet =
        new(CanonicalNames, StringComparer.OrdinalIgnoreCase);

    public GeofenceValidationResult Validate(string? rawLocation)
    {
        if (string.IsNullOrWhiteSpace(rawLocation))
            return new GeofenceValidationResult(false, null, rawLocation);

        var trimmed = rawLocation.Trim();

        if (CanonicalSet.TryGetValue(trimmed, out var canonical))
            return new GeofenceValidationResult(true, canonical, rawLocation);

        if (Aliases.TryGetValue(trimmed, out var fromAlias))
            return new GeofenceValidationResult(true, fromAlias, rawLocation);

        var prefixMatch = CanonicalNames.FirstOrDefault(g =>
            trimmed.StartsWith(g, StringComparison.OrdinalIgnoreCase) ||
            g.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase));

        if (prefixMatch is not null)
            return new GeofenceValidationResult(true, prefixMatch, rawLocation);

        return new GeofenceValidationResult(false, null, rawLocation);
    }
}