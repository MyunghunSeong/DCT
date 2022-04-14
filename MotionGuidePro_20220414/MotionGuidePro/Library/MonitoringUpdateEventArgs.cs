using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public class MonitoringUpdateEventArgs : EventArgs
    {
        // 파라미터 정보를 표시할 View
        public ParameterView View { get; set; }

        // 파라미터 정보
        public IParam Param { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public MonitoringUpdateEventArgs(ParameterView View, IParam Param)
        {
            this.View = View;
            this.Param = Param;
        }
    }
}
