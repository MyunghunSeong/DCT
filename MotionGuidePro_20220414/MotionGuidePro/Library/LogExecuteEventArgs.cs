using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    /// <summary>
    /// EventArgs 
    /// </summary>
    public class LogExecuteEventArgs : EventArgs
    {
        public LogState State { get; set; }
        public String Message { get; set; }
        public String Trace { get; set; }

        public String Time { get; set; }

        public LogExecuteEventArgs(LogState State, String Message = "", String Time="", String Trace="")
        {
            this.State = State;
            this.Message = Message;
            this.Trace = Trace;
            this.Time = Time;
        }
    }
}
