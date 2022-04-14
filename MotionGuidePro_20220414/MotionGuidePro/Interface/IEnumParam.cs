using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public interface IEnumParam
    {
        Boolean IsEntryValueOnly { get; }
        int EnumIntValue { get; set; }
        String EnumStrValue { get; set; }
        IList<EnumEntry> Entries { get; }
    }
}
