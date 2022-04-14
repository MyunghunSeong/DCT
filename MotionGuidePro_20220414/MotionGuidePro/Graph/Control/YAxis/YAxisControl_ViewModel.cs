using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CrevisLibrary;

namespace DCT_Graph
{
    public class YAxisControl_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Wnd"></param>
        public YAxisControl_ViewModel(Object Wnd) : base(Wnd) { }

        public OscilloscopeContent ContentViewModel { get; set; }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_DataInfoObj = new DataInformation();
            this.m_AxisInfoObj = new AxisInformation();
            this.m_TickValues = new List<String>();
            this.m_ControlHeight = 0.0d;
            this.m_AxisFitMode = FitMode.Normal;

            this.m_SelectBorder = Brushes.Transparent;
            this.m_SelectThickness = new Thickness(0);
            this.m_IsFocus = false;

            this.m_AxisVisible = ((m_Wnd as YAxisControl).MyContent.AxisInfoList[this.m_MyIndex].IsChannelSelected)
                ? Visibility.Visible : Visibility.Collapsed;

            this.m_YControlMargin = ((m_Wnd as YAxisControl).MyContent.AxisInfoList[this.MyIndex].IsChannelSelected)
                ? new Thickness(10, 0, 0, 0) : new Thickness(0, 0, 0, 0);
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //Y축 컨트롤 간에 마진 값
        private Thickness m_YControlMargin;
        public Thickness YControlMargin
        {
            get { return this.m_YControlMargin; }
            set
            {
                this.m_YControlMargin = value;
                this.NotifyPropertyChanged("YControlMargin");
            }
        }

        // 컨트롤 BorderBrush
        private Brush m_SelectBorder;
        public Brush SelectBorder
        {
            get { return this.m_SelectBorder; }
            set
            {
                this.m_SelectBorder = value;
                this.NotifyPropertyChanged("SelectBorder");
            }
        }

        private Boolean m_IsFocus;
        public Boolean IsFocus
        {
            get { return this.m_IsFocus; }
            set
            {
                this.m_IsFocus = value;
                this.NotifyPropertyChanged("IsFocus");
            }

        }

        // 컨트롤 BorderThickness
        private Thickness m_SelectThickness;
        public Thickness SelectThickness
        {
            get { return this.m_SelectThickness; }
            set
            {
                this.m_SelectThickness = value;
                this.NotifyPropertyChanged("SelectThickness");
            }
        }

        // 현재 내 축의 인덱스 정보
        private Int32 m_MyIndex;
        public Int32 MyIndex
        {
            get { return this.m_MyIndex; }
            set { this.m_MyIndex = value; }
        }

        // FitMode
        private FitMode m_AxisFitMode;
        public FitMode AxisFitMode
        {
            get { return this.m_AxisFitMode; }
            set { this.m_AxisFitMode = value; }
        }

        // 축 Visible 관련
        private Visibility m_AxisVisible;
        public Visibility AxisVisible
        {
            get { return this.m_AxisVisible; }
            set
            {
                this.m_AxisVisible = value;

                if (value == Visibility.Visible)
                {
                    (m_Wnd as YAxisControl).Width = 50.0d;
                    (m_Wnd as YAxisControl).Height = this.m_ControlHeight;
                }
                else
                {
                    (m_Wnd as YAxisControl).Width = 0.0d;
                    (m_Wnd as YAxisControl).Height = 0.0d;
                }

                if ((m_Wnd as YAxisControl).MyContent.XAxis != null)
                {
                    if (!(this.m_Wnd as YAxisControl).MyContent.IsOscilloCommCheck)
                    {
                        SignalOverlay overlay = (m_Wnd as YAxisControl).DigitalSignalObj.GetOverlay();
                        (m_Wnd as YAxisControl).MyContent.XAxis.DrawSignalData(overlay, this.AxisInfoObj.Channel);
                    }
                }
                this.NotifyPropertyChanged("AxisVisible");
            }
        }

        //컨트롤 높이
        private Double m_ControlHeight;
        public Double ControlHeight
        {
            get { return this.m_ControlHeight; }
            set
            {
                this.m_ControlHeight = value;
                this.NotifyPropertyChanged("ControlHeight");
            }
        }

        // DataInformation 객체
        private DataInformation m_DataInfoObj;
        public DataInformation DataInfoObj
        {
            get { return this.m_DataInfoObj; }
            set { this.m_DataInfoObj = value; }
        }

        // AxisInfo 객체
        private AxisInformation m_AxisInfoObj;
        public AxisInformation AxisInfoObj
        {
            get { return this.m_AxisInfoObj; }
            set { this.m_AxisInfoObj = value; }
        }

        // 눈금에 표시할 텍스트 리스트
        private List<String> m_TickValues;
        public List<String> TickValues
        {
            get { return this.m_TickValues; }
            set { this.m_TickValues = value; }
        }
    }
}
