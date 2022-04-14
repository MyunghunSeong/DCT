using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class GridInformation
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public GridInformation()
        {
            this.m_RowCount = 0;
            this.m_ColumnCount = 0;
            this.m_Width = 0;
            this.m_Height = 0;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        // Y축 칸 개수
        private Int32 m_RowCount;
        public Int32 RowCount
        {
            get { return this.m_RowCount; }
            set { this.m_RowCount = value; }
        }

        // X축 칸 개수
        private Int32 m_ColumnCount;
        public Int32 ColumnCount
        {
            get { return this.m_ColumnCount; }
            set { this.m_ColumnCount = value; }
        }

        // 1칸당 가로 크기
        private Int32 m_Width;
        public Int32 Width
        {
            get { return this.m_Width; }
            set { this.m_Width = value; }
        }

        // 1칸당 세로 크기
        private Int32 m_Height;
        public Int32 Height
        {
            get { return this.m_Height; }
            set { this.m_Height = value; }
        }
    }
}
