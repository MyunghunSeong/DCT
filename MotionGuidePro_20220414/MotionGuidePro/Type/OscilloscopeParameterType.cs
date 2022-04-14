using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public enum OscilloscopeParameterType
    {
        Motor_Feedback_Position     = 1,
        Master_Position             = 2,
        Follower_Position           = 3,
        Position_Error              = 4,
        Velocity_Command           = 5,
        Velocity_Feedback           = 6,
        Velocity_Error              = 7,
        Current_Command             = 8,
        Current_Feedback            = 9,
        U_Phase_Current             = 10,
        V_Phase_Current             = 11,
        W_Phase_Current             = 12,
        Commutation_Angle           = 13,
        Mechanical_Angle            = 14,
        Drive_Utilization           = 15,
        Bus_Voltage                 = 16,
        Motor_Utilization           = 17,
        LMMT_MOSI                   = 18,
        LMMT_MISO                   = 19,
        Fault                       = 20,
        Valid                       = 21,
    }
}
