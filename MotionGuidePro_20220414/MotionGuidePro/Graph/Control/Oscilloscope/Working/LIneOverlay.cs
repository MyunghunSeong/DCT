using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DCT_Graph
{
    public class LineOverlay
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public LineOverlay()
        {
            this.m_Start = new Point();
            this.m_End = new Point();
            this.m_DrawColor = Brushes.Transparent;
            this.m_Tooltip = String.Empty;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //시작 위치
        private Point m_Start;
        public Point Start
        {
            get { return this.m_Start; }
            set { this.m_Start = value; }
        }

        //끝 위치
        private Point m_End;
        public Point End
        {
            get { return this.m_End; }
            set { this.m_End = value; }
        }

        //표시할 컬러
        private Brush m_DrawColor;
        public Brush DrawColor
        {
            get { return this.m_DrawColor; }
            set { this.m_DrawColor = value; }
        }

        //툴팁 정보
        private String m_Tooltip;
        public String Tooltip
        {
            get { return this.m_Tooltip; }
            set { this.m_Tooltip = value; }
        }
    }
}
