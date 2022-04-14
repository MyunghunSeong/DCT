using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrevisLibrary
{
    public class EnumEntry
    {
        public int EnumValue { get; set; }
        public String DisplayName { get; set; }

        public EnumEntry(int Value, String DisplayName)
        {
            this.EnumValue = Value;
            this.DisplayName = DisplayName;
        }
    }
}
