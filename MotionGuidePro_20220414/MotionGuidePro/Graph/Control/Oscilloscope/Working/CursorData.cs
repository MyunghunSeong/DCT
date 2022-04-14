using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrevisLibrary;

namespace DCT_Graph
{
    public class CursorData : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public CursorData()
        {
            this.m_Channel = 0;
            this.m_Signal = new DigitalSignal();
            this.m_TextBoxPosition = new Point();
            this.m_LinePosition = new Point();
            this.m_TextBoxWidth = 0;
            this.m_TextBoxHeight = 0;
            this.m_DrawColor = Brushes.Transparent;
            this.m_DataPosition = new Point();
            this.m_CursorTime = 0.0d;
            this.m_DataOfCursor = 0.0d;
            this.m_DisplayValue = String.Empty;
            this.m_AxisCount = 0;
            this.m_TextBoxVisible = Visibility.Collapsed;
        }

        public CursorData(Object Wnd) : base(Wnd) { }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        // 축 개수
        private Int32 m_AxisCount;
        public Int32 AxisCount
        {
            get { return this.m_AxisCount; }
            set { this.m_AxisCount = value; }
        }

        //채널 인덱스
        private Int32 m_Channel;
        public Int32 Channel
        {
            get { return this.m_Channel; }
            set { this.m_Channel = value; }
        }

        //신호 데이터
        private DigitalSignal m_Signal;
        public DigitalSignal Signal
        {
            get { return this.m_Signal; }
            set { this.m_Signal = value; }
        }

        //텍스트 박스 visible
        private Visibility m_TextBoxVisible;
        public Visibility TextBoxVisible
        {
            get { return this.m_TextBoxVisible; }
            set
            {
                this.m_TextBoxVisible = value;
                this.NotifyPropertyChanged("TextBoxVisible");
            }
        }

        //데이터 표시할 텍스트 블럭의 픽셀 좌표 (X는 CursorPositionX + Margin)
        private Point m_TextBoxPosition;
        public Point TextBoxPosition
        {
            get { return this.m_TextBoxPosition; }
            set
            {
                this.m_TextBoxPosition = value;
                this.NotifyPropertyChanged("TextBoxPosition");
            }
        }

        //데이터 표시할 라인 좌표
        private Point m_LinePosition;
        public Point LinePosition
        {
            get { return this.m_LinePosition; }
            set
            {
                this.m_LinePosition = value;
                this.NotifyPropertyChanged("LinePosition");
            }
        }

        //텍스트 박스 가로 길이
        private Int32 m_TextBoxWidth;
        public Int32 TextBoxWidth
        {
            get { return this.m_TextBoxWidth; }
            set
            {
                this.m_TextBoxWidth = value;
                this.NotifyPropertyChanged("TextBoxWidth");
            }
        }

        //텍스트 박스 세로 길이
        private Int32 m_TextBoxHeight;
        public Int32 TextBoxHeight
        {
            get { return this.m_TextBoxHeight; }
            set
            {
                this.m_TextBoxHeight = value;
                this.NotifyPropertyChanged("TextBoxHeight");
            }
        }

        //표시할 컬러
        private Brush m_DrawColor;
        public Brush DrawColor
        {
            get { return this.m_DrawColor; }
            set
            {
                this.m_DrawColor = value;
                this.NotifyPropertyChanged("DrawColor");
            }
        }

        //커서와 데이터가 교차하는 지정 픽셀 좌표
        private Point m_DataPosition;
        public Point DataPosition
        {
            get { return this.m_DataPosition; }
            set
            {
                this.m_DataPosition = value;
                this.m_TextBoxPosition.X = value.X + 10;
                this.m_TextBoxPosition.Y = value.Y;

                this.NotifyPropertyChanged("DataPosition");
                this.NotifyPropertyChanged("TextBoxPosition");
            }
        }

        //CursorPositionX에 해당하는 시간 값
        private Double m_CursorTime;
        public Double CursorTime
        {
            get { return this.m_CursorTime; }
            set
            {
                this.m_CursorTime = value;
                this.NotifyPropertyChanged("CurrentTime");
            }
        }

        //CursorPositionX에 해당하는 데이터 값
        private Double m_DataOfCursor;
        public Double DataOfCursor
        {
            get { return this.m_DataOfCursor; }
            set
            {
                this.m_DataOfCursor = value;
                this.NotifyPropertyChanged("DataOfCursor");
            }
        }

        //텍스트 박스에 표시할 내용
        private String m_DisplayValue;
        public String DisplayValue
        {
            get { return this.m_DisplayValue; }
            set
            {
                this.m_DisplayValue = value;
                this.NotifyPropertyChanged("DisplayValue");
            }
        }

        ///<summary>
        /// 함수
        /// </summary>
        //Update
        public void Update()
        {
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        //정보 가져오기
        public LineOverlay GetOverlay()
        {
            try
            {
                LineOverlay overlay = new LineOverlay();
                return overlay;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void InitializeViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
