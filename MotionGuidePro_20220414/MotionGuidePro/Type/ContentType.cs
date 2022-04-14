using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public enum ContentType
    {
        None                = -1,
        ParameterContent    = 0,
        OscilloscopeContent = 1,
        MonitoringContent   = 2,
        FaultContent        = 3,
        UserManualContent   = 4,
        MeasureContent      = 5
    }
}
