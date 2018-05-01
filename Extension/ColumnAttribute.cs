using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactSpa.Extension
{
    [AttributeUsage(AttributeTargets.All)]
    public class ColumnAttribute : Attribute
    {
        public int ColumnIndex { get; set; }

        public ColumnAttribute(int column)
        {
            ColumnIndex = column;
        }
    }
}