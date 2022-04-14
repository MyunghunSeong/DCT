using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrevisLibrary;

namespace DCT_Graph
{
    public class TriggerControl_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public TriggerControl_ViewModel(Object Wnd) : base(Wnd) { }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_IsEnable = false;
            this.m_DigitalSignalArr = new DigitalSignal[(this.m_Wnd as TriggerControl).MyContent.AxisInfoList.Count];
            for (int i = 0; i < this.m_DigitalSignalArr.Length; i++)
            {
                String EnumName = (m_Wnd as TriggerControl).MyContent.AxisInfoList[i].CurrentSelectedAxisName;
                EnumName = EnumName.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);

                this.m_DigitalSignalArr[i] = (m_Wnd as TriggerControl).MyContent.DigitalSignalMap[type];
            }
            this.m_SelectChannel = 0;
            this.m_DrawColor = Brushes.Transparent;
            this.m_CursorPositionY = 0.0d;
            this.m_AxisLength = 0.0d;

            //현재 화면에서 선택되어진 축들 이름 리스트
            this.m_CurrentSelectedAxisChannelList = new ObservableCollection<String>();
            for (int i = 0; i < 5; i++)
                this.m_CurrentSelectedAxisChannelList.Add((i + 1).ToString() + "Channel");

            //트리거 관련 변수 초기화
            this.m_TriggerAction = new TriggerActionType();
            this.m_TriggerLevel = 0.0d;
            this.m_IsTriggerActionChanged = true;
            this.m_TriggerActionText = "Riging Edge Mode";

            //샘플링 주기 리스트 값 넣기
            this.m_TriggerPeriodIndex = 0;
            this.m_TriggerSamplingPeriodList = new List<Double>();
            this.m_TriggerSamplingPeriodList.Add(0.1d);
            this.m_TriggerSamplingPeriodList.Add(0.5d);
            this.m_TriggerSamplingPeriodList.Add(1.0d);
            this.m_TriggerSamplingPeriodList.Add(2.0d);
            this.m_TriggerSamplingPeriodList.Add(5.0d);
            this.m_TriggerPeriod = this.m_TriggerSamplingPeriodList[this.m_TriggerPeriodIndex];

            //샘플링 시간 리스트 값 넣기
            this.m_TriggerTimeIndex = 0;
            this.m_TriggerSamplingTimeList = new List<Int32>();
            this.m_TriggerSamplingTimeList.Add(5);
            this.m_TriggerSamplingTimeList.Add(10);
            this.m_TriggerSamplingTimeList.Add(20);
            this.m_TriggerTime = this.m_TriggerSamplingTimeList[this.m_TriggerTimeIndex];
            this.m_DashArray = new DoubleCollection(new List<Double>() { 1, 2, 1, 2 });
            this.m_LineVisible = Visibility.Collapsed;
            this.m_LastPos = new Point(0, 0);

            this.m_CanvasMargin = new Thickness(0, 2, 0, 0);

            //Enable관련 변수 초기화
            this.m_IsControlEnabled = true;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        #region 트리거 관련 변수
        //현재 선택되어있는 축 이름 리스트
        private ObservableCollection<String> m_CurrentSelectedAxisChannelList;
        public ObservableCollection<String> CurrentSelectedAxisChannelList
        {
            get { return this.m_CurrentSelectedAxisChannelList; }
            set { this.m_CurrentSelectedAxisChannelList = value; }
        }

        //샘플링 타임
        private Int32 m_SamplingTime;
        public Int32 SamplingTime
        {
            get { return this.m_SamplingTime; }
            set
            {
                this.m_SamplingTime = value;
                this.NotifyPropertyChanged("SamplingTime");
            }
        }

        //트리거 동작 
        private TriggerActionType m_TriggerAction;
        public TriggerActionType TriggerAction
        {
            get { return this.m_TriggerAction; }
            set
            {
                this.m_TriggerAction = value;
                this.NotifyPropertyChanged("TriggerAction");
            }
        }

        //트리거 레벨
        private Double m_TriggerLevel;
        public Double TriggerLevel
        {
            get { return this.m_TriggerLevel; }
            set
            {
                this.m_TriggerLevel = value;
                this.NotifyPropertyChanged("TriggerLevel");
            }
        }

        //샘프링 주기 리스트
        private List<Double> m_TriggerSamplingPeriodList;
        public List<Double> TriggerSamplingPeriodList
        {
            get { return this.m_TriggerSamplingPeriodList; }
            set
            {
                this.m_TriggerSamplingPeriodList = value;
                this.NotifyPropertyChanged("TriggerSamplingPeriodList");
            }
        }

        //샘플링 주기 인덱스
        private Int32 m_TriggerPeriodIndex;
        public Int32 TriggerPeriodIndex
        {
            get { return this.m_TriggerPeriodIndex; }
            set
            {
                this.m_TriggerPeriodIndex = value;
                this.NotifyPropertyChanged("TriggerPeriodIndex");
            }
        }

        //선택한 샘플링 주기
        private Double m_TriggerPeriod;
        public Double TriggerPeriod
        {
            get { return this.m_TriggerPeriod; }
            set
            {
                this.m_TriggerPeriod = value;
                this.NotifyPropertyChanged("TriggerPeriod");
            }
        }


        //샘프링 시간 리스트
        private List<Int32> m_TriggerSamplingTimeList;
        public List<Int32> TriggerSamplingTimeList
        {
            get { return this.m_TriggerSamplingTimeList; }
            set
            {
                this.m_TriggerSamplingTimeList = value;
                this.NotifyPropertyChanged("TriggerSamplingTimeList");
            }
        }

        //샘플링 시간 인덱스
        private Int32 m_TriggerTimeIndex;
        public Int32 TriggerTimeIndex
        {
            get { return this.m_TriggerTimeIndex; }
            set
            {
                this.m_TriggerTimeIndex = value;
                this.SamplingTime = this.m_TriggerSamplingTimeList[value];
                this.NotifyPropertyChanged("TriggerTimeIndex");
            }
        }

        //선택한 샘플링 주기
        private Int32 m_TriggerTime;
        public Int32 TriggerTime
        {
            get { return this.m_TriggerTime; }
            set
            {
                this.m_TriggerTime = value;
                this.NotifyPropertyChanged("TriggerTime");
            }
        }

        //트리거 동작 텍스트
        private String m_TriggerActionText;
        public String TriggerActionText
        {
            get { return this.m_TriggerActionText; }
            set
            {
                this.m_TriggerActionText = value;
                this.NotifyPropertyChanged("TriggerActionText");
            }
        }

        //트리거 설정창에서 트리거 동작에 대한 변경이 일어났을 때
        private Boolean m_IsTriggerActionChanged;
        public Boolean IsTriggerActionChanged
        {
            get { return this.m_IsTriggerActionChanged; }
            set
            {
                if (value)
                {
                    this.TriggerAction = TriggerActionType.RIGING_EDGE;
                    this.TriggerActionText = "Riging Edge Mode";
                }
                else
                {
                    this.TriggerAction = TriggerActionType.FALLING_EDGE;
                    this.TriggerActionText = "Falling Edge Mode";
                }

                this.m_IsTriggerActionChanged = value;
                this.NotifyPropertyChanged("IsTriggerActionChanged");
            }
        }
        #endregion

        private Thickness m_CanvasMargin;
        public Thickness CanvasMargin
        {
            get { return this.m_CanvasMargin; }
            set
            {
                this.m_CanvasMargin = value;
                this.NotifyPropertyChanged("CanvasMargin");
            }
        }

        private Visibility m_LineVisible;
        public Visibility LineVisible
        {
            get { return this.m_LineVisible; }
            set
            {
                this.m_LineVisible = value;
                this.NotifyPropertyChanged("LineVisible");
            }
        }

        private DoubleCollection m_DashArray;
        public DoubleCollection DashArray
        {
            get { return this.m_DashArray; }
            set
            {
                this.m_DashArray = value;
                this.NotifyPropertyChanged("DashArray");
            }
        }

        //IsEnble
        private Boolean m_IsEnable;
        public Boolean IsEnable
        {
            get { return this.m_IsEnable; }
            set
            {
                if (!(this.m_Wnd as TriggerControl).MyContent.IsOscilloCommCheck)
                    return;

                this.m_IsEnable = value;
                (this.m_Wnd as TriggerControl).TriggerLine.mainGrid.Background = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.Black : new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                (this.m_Wnd as TriggerControl).TriggerLine.line.Stroke = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.White : Brushes.Black;
                this.LineVisible = (value) ? Visibility.Visible : Visibility.Collapsed;
                this.NotifyPropertyChanged("IsEnable");
            }
        }

        //DigitalSignal
        private DigitalSignal[] m_DigitalSignalArr;
        public DigitalSignal[] DigitalSignalArr
        {
            get { return this.m_DigitalSignalArr; }
            set
            {
                this.m_DigitalSignalArr = value;
                this.NotifyPropertyChanged("DigitalSignalArr");
            }
        }

        //선택한 채널
        private Int32 m_SelectChannel;
        public Int32 SelectChannel
        {
            get { return this.m_SelectChannel; }
            set
            {
                this.m_SelectChannel = value;
                this.NotifyPropertyChanged("SelectChannel");
            }
        }

        //CheckBox Eanbled
        private Boolean m_IsControlEnabled;
        public Boolean IsControlEnabled
        {
            get { return this.m_IsControlEnabled; }
            set
            {
                this.m_IsControlEnabled = value;
                this.NotifyPropertyChanged("IsControlEnabled");
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

        //트리거 라인 Y픽셀 좌표
        private Double m_CursorPositionY;
        public Double CursorPositionY
        {
            get { return this.m_CursorPositionY; }
            set
            {
                this.m_CursorPositionY = value;
                this.NotifyPropertyChanged("CursorPositionY");
            }
        }

        //트리거 라인 Y픽셀 좌표
        private Point m_LastPos;
        public Point LastPos
        {
            get { return this.m_LastPos; }
            set
            {
                this.m_LastPos = value;
                this.NotifyPropertyChanged("LastPos");
            }
        }

        //X축 픽셀단위 크기
        private Double m_AxisLength;
        public Double AxisLength
        {
            get { return this.m_AxisLength; }
            set
            {
                this.m_AxisLength = value;
                this.NotifyPropertyChanged("AxisLength");
            }
        }

        ///<summary>
        /// 함수
        /// </summary>
        //가로 라인
        public LineOverlay GetOverlay()
        {
            try
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
