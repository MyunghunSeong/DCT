using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class GridCalibrationTool
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public GridCalibrationTool(GridInformation GridInfo, Int32 Width, Int32 Height)
        {
            this.m_Width = Width;
            this.m_Height = Height;
            this.m_ColumnWidth = GridInfo.Width;
            this.m_RowHeight = GridInfo.Height;
            this.m_RowCount = 0;
            this.m_ColumnCount = 0;
            this.GridInfo = GridInfo;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //컨트롤
        public GridInformation GridInfo { get; set; }

        // 1칸당 가로 크기
        private Int32 m_ColumnWidth;
        public Int32 ColumnWidth
        {
            get { return this.m_ColumnWidth; }
            set { this.m_ColumnWidth = value; }
        }

        // 1칸당 세로 크기
        private Int32 m_RowHeight;
        public Int32 RowHeight
        {
            get { return this.m_RowHeight; }
            set { this.m_RowHeight = value; }
        }

        // 가로 크기
        private Int32 m_Width;
        public Int32 Width
        {
            get { return this.m_Width; }
            set { this.m_Width = value; }
        }

        // 세로 크기
        private Int32 m_Height;
        public Int32 Height
        {
            get { return this.m_Height; }
            set { this.m_Height = value; }
        }

        // Row 개수
        private Int32 m_RowCount;
        public Int32 RowCount
        {
            get { return this.m_RowCount; }
        }

        // Column 개수
        private Int32 m_ColumnCount;
        public Int32 ColumnCount
        {
            get { return this.m_ColumnCount; }
        }

        ///<summary>
        /// 함수
        /// </summary>
        // 자동으로 Row, Column개수를 구해주는 함수
        public void Run(Int32 Scale=1)
        {
            try
            {
                this.m_ColumnCount = this.Width / this.ColumnWidth;

                this.m_ColumnWidth = 40 * Scale;
                this.GridInfo.Width = this.ColumnWidth;

                this.m_RowCount = this.Height / this.RowHeight;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
