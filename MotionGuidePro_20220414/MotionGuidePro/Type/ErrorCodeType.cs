using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public enum ErrorCodeType
    {
        No_Error            = 0x00,
        FOC_Duration        = 0x11,
        Break_In            = 0x12,
        PWC_Open            = 0x20,
        Over_Temp           = 0x24,
        DRV_Over_Current    = 0x27,
        ENC_Error           = 0x32,
        Over_Voltage        = 0x40,
        Under_Voltage       = 0x41,
    }
}
