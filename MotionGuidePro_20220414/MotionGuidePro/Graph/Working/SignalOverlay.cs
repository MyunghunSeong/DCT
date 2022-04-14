using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DCT_Graph
{
    /// <summary>
    /// 그리드에 표시할 Digital 선
    /// </summary>
    public class SignalOverlay
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public SignalOverlay()
        {
            this.m_Points = new List<Point>();
            this.m_DrawColor = Brushes.Black;
            this.m_ToolTip = String.Empty;
        }

        ///<summary>
        /// 프로퍼티
        ///</summary>
        // 좌표 리스트
        private List<Point> m_Points;
        public List<Point> Points
        {
            get { return this.m_Points; }
            set { this.m_Points = value; }
        }

        // 선 색상
        private Brush m_DrawColor;
        public Brush DrawColor
        {
            get { return this.m_DrawColor; }
            set { this.m_DrawColor = value; }
        }

        // ToolTip
        private String m_ToolTip;
        public String ToolTip
        {
            get { return this.m_ToolTip; }
            set { this.m_ToolTip = value; }
        }
    }
}
