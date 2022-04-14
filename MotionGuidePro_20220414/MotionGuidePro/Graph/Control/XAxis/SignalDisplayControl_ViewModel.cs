using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CrevisLibrary;
using MotionGuidePro.Main;

namespace DCT_Graph
{
    public class SignalDisplayControl_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public SignalDisplayControl_ViewModel(Object Wnd) : base(Wnd) { }

        public Double testVal { get; set; }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_GridInfoObj = new GridInformation();
            this.m_AxisInfoObj = new AxisInformation();
            this.m_XAxisTimeValueOffset = 10;
            this.m_PointGridWidth = 1000;
            this.m_PointGridHeight = 600;
            this.m_ParentsCanvasWidth = 1000;
            this.m_ParentsCanvasHeight = 600;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        public Double m_Test;
        public Double Test
        {
            get { return this.m_Test; }
            set
            {
                this.m_Test = value;
                this.NotifyPropertyChanged("Test");
            }
        }

        //ParentsCanvasWidth
        private Double m_ParentsCanvasWidth;
        public Double ParentsCanvasWidth
        {
            get { return this.m_ParentsCanvasWidth; }
            set
            {
                this.m_ParentsCanvasWidth = value;
                this.NotifyPropertyChanged("ParentsCanvasWidth");
            }
        }

        //ParentsCanvasHeight
        private Double m_ParentsCanvasHeight;
        public Double ParentsCanvasHeight
        {
            get { return this.m_ParentsCanvasHeight; }
            set
            {
                this.m_ParentsCanvasHeight = value;
                this.NotifyPropertyChanged("ParentsCanvasHeight");
            }
        }

        //좌표 캔버스 넓이
        private Double m_PointGridWidth;
        public Double PointGridWidth
        {
            get { return this.m_PointGridWidth; }
            set
            {
                this.m_PointGridWidth = value;
                this.NotifyPropertyChanged("PointGridWidth");
            }
        }

        //좌표 캔버스 높이
        private Double m_PointGridHeight;
        public Double PointGridHeight
        {
            get { return this.m_PointGridHeight; }
            set
            {
                this.m_PointGridHeight = value;
                this.NotifyPropertyChanged("PointGridHeight");
            }
        }

        public MainWindow_ViewModel MainViewModel { get; set; }
        
        public OscilloscopeContent ContentViewModel { get; set; }

        //TimeValue Offset
        private Double m_XAxisTimeValueOffset;
        public Double XAxisTimeValueOffset
        {
            get { return this.m_XAxisTimeValueOffset; }
            set
            {
                this.m_XAxisTimeValueOffset = value;
                this.NotifyPropertyChanged("XAxisTimeValueOffset");
            }
        }

        // GridInformation 객체
        private GridInformation m_GridInfoObj;
        public GridInformation GridInfoObj
        {
            get { return this.m_GridInfoObj; }
            set { this.m_GridInfoObj = value; }
        }

        // GridInformation 객체
        private AxisInformation m_AxisInfoObj;
        public AxisInformation AxisInfoObj
        {
            get { return this.m_AxisInfoObj; }
            set { this.m_AxisInfoObj = value; }
        }
    }
}
