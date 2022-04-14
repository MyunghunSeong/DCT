using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public interface IByteArrayParam
    {
        Byte[] ArrayValue { get; set; }
        String ArrayValueString { get; set; }
    }
}
