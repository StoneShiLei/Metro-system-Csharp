using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTypeEnum
    {
        NORMAL, SENIOR, STUDENT, DISABILITY
    }
}