using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    public class ROException:Exception
    {
        public ROException(String message):base(message)
        {
        }
    }
}