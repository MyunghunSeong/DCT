using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public enum MOSICodeType
    {
        Servo_Off       = 0x00,
        Servo_On        = 0x01,
        CW_Position     = 0x07,
        CCW_Position    = 0x0F,
        Alarm_Clear     = 0x10,
        CW_Velocity     = 0x05,
        CCW_Velocity    = 0x0D,
    }
}
