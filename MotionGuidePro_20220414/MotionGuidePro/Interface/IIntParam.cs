using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public interface IIntParam
    {
        int IntValue { get; set; }
        int Min { get; set; }
        int Max { get; set; }
        int Inc { get; set; }
    }
}
