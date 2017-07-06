using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactSpa.Extension
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateAttribute : Attribute
    {
        public DateType Type { get; set; }

        public DateAttribute(DateType t)
        {
            Type = t;
        }
    }

    public enum DateType
    {
        Date = 0,
        Time = 1
    }
}