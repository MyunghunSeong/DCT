using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

using DCT_Graph;

namespace CrevisLibrary
{
    public class OscilloscopeContent : IContent, INotifyPropertyChanged
    {
        /// <summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        #region 테마 변경 관련 변수
        private Brush m_ThemaColor;
        public Brush ThemaColor
        {
            get { return this.m_ThemaColor; }
            set
            {
                this.m_ThemaColor = value;
                this.NotifyPropertyChanged("ThemaColor");
            }
        }

        private Brush m_ThemaComboColor;
        public Brush ThemaComboColor
        {
            get { return this.m_ThemaComboColor; }
            set
            {
                this.m_ThemaComboColor = value;
                this.NotifyPropertyChanged("ThemaComboColor");
            }
        }

        private Brush m_ThemaForeColor;
        public Brush ThemaForeColor
        {
            get { return this.m_ThemaForeColor; }
            set
            {
                this.m_ThemaForeColor = value;
                this.NotifyPropertyChanged("ThemaForeColor");
            }
        }

        private Brush m_SettingColor;
        public Brush SettingColor
        {
            get { return this.m_SettingColor; }
            set
            {
                this.m_SettingColor = value;
                this.NotifyPropertyChanged("SettingColor");
            }
        }

        private String m_DownArrowShape;
        public String DownArrowShape
        {
            get { return this.m_DownArrowShape; }
            set
            {
                this.m_DownArrowShape = value;
                this.NotifyPropertyChanged("DownArrowShape");
            }
        }

        private String m_UpArrowShape;
        public String UpArrowShape
        {
            get { return this.m_UpArrowShape; }
            set
            {
                this.m_UpArrowShape = value;
                this.NotifyPropertyChanged("UpArrowShape");
            }
        }

        #endregion

        #region 일반 처리
        public Int32 EndPacketIndex { get; set; }
        #endregion

        #region 그래프 패킷 관련 변수
        //쓰레드 종료 플래그 변수
        private Boolean m_IsExitThread;
        public Boolean IsExitThread
        {
            get { return this.m_IsExitThread; }
            set { this.m_IsExitThread = value; }
        }

        //통신 중지 플래그 변수
        private Boolean m_IsStopComm;
        public Boolean IsStopComm
        {
            get { return this.m_IsStopComm; }
            set { this.m_IsStopComm = value; }
        }

        //데이터 수신 쓰레드 객체
        private Thread m_GraphReceiveThread;
        public Thread GraphReceiveThread
        {
            get { return this.m_GraphReceiveThread; }
            set { this.m_GraphReceiveThread = value; }
        }

        //패킷 파서 쓰레드 객체
        private Thread m_PacketParserThread;
        public Thread PacketParserThread
        {
            get { return this.m_PacketParserThread; }
            set { this.m_PacketParserThread = value; }
        }

        //시그널 업데이트 쓰레드 객체
        private Thread m_UpdateSignalThread;
        public Thread UpdateSignalThread
        {
            get { return this.m_UpdateSignalThread; }
            set { this.m_UpdateSignalThread = value; }
        }

        //큐에서 꺼낸 패킷 데이터
        private Byte[] m_PacketData;

        //패킷 인덱스 정보
        private Int32 m_PacketIndex;
        public Int32 PacketIndex
        {
            get { return this.m_PacketIndex; }
            set { this.m_PacketIndex = value; }
        }
        #endregion

        #region 트리거 관련 변수
        private Signal m_LastSignal;
        public Signal LastSignal
        {
            get { return this.m_LastSignal; }
            set { this.m_LastSignal = value; }
        }
        #endregion

        //시그널 데이터 백업
        private Signal[] m_BackupSignalArray;
        public Signal[] BackupSignalArray
        {
            get { return this.m_BackupSignalArray; }
            set { this.m_BackupSignalArray = value; }
        }

        //데이터 업데이트 프레임 조정 Event
        private AutoResetEvent m_AutoAdjustmentEvent;
        public AutoResetEvent AutoAdjustmentEvent
        {
            get { return this.m_AutoAdjustmentEvent; }
            set { this.m_AutoAdjustmentEvent = value; }
        }

        // 이름
        private String m_Name;
        public String Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        // 컨텐츠 타입
        private ContentType m_Type;
        public ContentType Type
        {
            get { return this.m_Type; }
            set { this.m_Type = value; }
        }

        // 상위 디바이스
        private IDevice m_MyDevice;
        public IDevice MyDevice
        {
            get { return this.m_MyDevice; }
            set { this.m_MyDevice = value; }
        }

        // Y축 컨트롤 화면
        private List<YAxisControl> m_YAxisList;
        public List<YAxisControl> YAxisList
        {
            get { return this.m_YAxisList; }
            set { this.m_YAxisList = value; }
        }

        // Y축 컨트롤 화면
        private SignalDisplayControl m_XAxis;
        public SignalDisplayControl XAxis
        {
            get { return this.m_XAxis; }
            set { this.m_XAxis = value; }
        }

        //축 정보
        private Dictionary<OscilloscopeParameterType, DigitalSignal> m_DigitalSignalMap;
        public Dictionary<OscilloscopeParameterType, DigitalSignal> DigitalSignalMap
        {
            get { return this.m_DigitalSignalMap; }
            set { this.m_DigitalSignalMap = value; }
        }

        //축 및 채널 색 지정
        private Brush[] m_BrushColorArray;
        public Brush[] BrushColorArray
        {
            get { return this.m_BrushColorArray; }
            set { this.m_BrushColorArray = value; }
        }

        //선택되어있는 축의 이름을 저장하는 공간
        private List<String> m_SelectedAxisNameList;
        public List<String> SelectedAxisNameList
        {
            get { return this.m_SelectedAxisNameList; }
            set { this.m_SelectedAxisNameList = value; }
        }

        // 오실로 스코프 컨트롤 넓이
        private Double m_OscilloscopeWidth;
        public Double OscilloscopeWidth
        {
            get { return this.m_OscilloscopeWidth; }
            set
            {
                this.m_OscilloscopeWidth = value;
                this.NotifyPropertyChanged("OscilloscopeWidth");
            }
        }

        // 오실로 스코프 컨트롤 넓이
        private Double m_OscilloscopeHeight;
        public Double OscilloscopeHeight
        {
            get { return this.m_OscilloscopeHeight; }
            set
            {
                this.m_OscilloscopeHeight = value;
                this.NotifyPropertyChanged("OscilloscopeHeight");
            }
        }

        //X축 헤더 정보
        private String m_XAxisHeader;
        public String XAxisHeader
        {
            get { return this.m_XAxisHeader; }
            set
            {
                this.m_XAxisHeader = value;
                this.NotifyPropertyChanged("XAxisHeader");
            }
        }

        //X축 표시 시간 텍스트 리스트
        private List<String> m_TimeValueList;
        public List<String> TimeValueList
        {
            get { return this.m_TimeValueList; }
            set
            {
                this.m_TimeValueList = value;
                this.NotifyPropertyChanged("TimeValueList");
            }
        }

        //OscilloscopeControl 통신버튼 Enable 여부
        private Boolean m_IsCommBtnEnabled;
        public Boolean IsCommBtnEnabled
        {
            get { return this.m_IsCommBtnEnabled; }
            set
            {
                this.m_IsCommBtnEnabled = value;
                this.NotifyPropertyChanged("IsCommBtnEnabled");
            }
        }

        //OscilloscopeControl 통신 버튼 클릭 여부
        private Boolean m_IsOscilloCommCheck;
        public Boolean IsOscilloCommCheck
        {
            get { return this.m_IsOscilloCommCheck; }
            set
            {
                DevParam Param = new DevParam("그래프 통신", ParamType.ByteArray, null, String.Empty, String.Empty,
                   PortType.None, 0, 1, 0x1110, 1);

                if (value)
                {
                    try
                    {
                        ChangeUIState(true);

                        //통신 정지
                        (Param as IByteArrayParam).ArrayValue = new Byte[2] { 0x00, 0x00 };
                        this.MyDevice.Write(Param);

                        foreach (ShowAxisInformation info in this.AxisInfoList)
                        {
                            info.IsChannelEnabled = true;
                            info.IsEnabledChannelComboBox = true;
                        }

                        //통신이 종료되는 시점에서 Measure를 업데이트 해준다.
                        //변경된 데이터로 Measure를 업데이트 해준다.
                        //SMH5555
                        if (PublicVar.MainWnd.ViewModel.MeasureControl != null)
                        {
                            for (int i = 0; i < PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children.Count; i++)
                            {
                                if (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] is MeasurementControl)
                                    (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] as MeasurementControl).Run();
                            }
                        }

                        #region 쓰레드 종료
                        this.IsExitThread = true;
                        if (this.GraphReceiveThread != null)
                        {
                            //쓰레드 종료
                            if (!this.GraphReceiveThread.Join(1000))
                            {
                                this.GraphReceiveThread.Abort();
                                this.GraphReceiveThread = null;
                            }
                        }

                        if (this.PacketParserThread != null)
                        {
                            //쓰레드 종료
                            if (!this.PacketParserThread.Join(1000))
                            {
                                this.PacketParserThread.Abort();
                                this.PacketParserThread = null;
                            }
                        }

                        if (this.UpdateSignalThread != null)
                        {
                            //쓰레드 종료
                            if (!this.UpdateSignalThread.Join(1000))
                            {
                                this.UpdateSignalThread.Abort();
                                this.UpdateSignalThread = null;
                            }
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        #region 쓰레드 실행
                        this.IsExitThread = false;
                        //그래프 데이터 수신 쓰레드 실행
                        this.GraphReceiveThread = new Thread(() =>
                                    ((this.MyDevice as Device).CommObj as ModBusUdp).OnGraphDataReceive());
                        this.GraphReceiveThread.Start();

                        //파서 쓰레드 실행
                        this.PacketParserThread = new Thread(() => this.PacketParse());
                        this.PacketParserThread.Start();

                        //화면 업데이트 쓰레드 실행
                        this.UpdateSignalThread = new Thread(() => this.UpdateSignal());
                        this.UpdateSignalThread.Start();
                        #endregion

                        ChangeUIState(false);

                        //선택된 축 정보 전달
                        Int32 count = 0;
                        foreach (ShowAxisInformation info in this.AxisInfoList)
                        {
                            //보드에 송신할 채널 파라미터를 만든다.
                            DevParam ChParam = new DevParam("채널" + (count + 1), ParamType.ByteArray, null, String.Empty);

                            #region 채널 정보 초기화
                            info.IsChannelEnabled = false;
                            info.IsEnabledChannelComboBox = false;

                            this.DigitalSignalMap[info.ParamType].Sum = 0;
                            this.DigitalSignalMap[info.ParamType].MeanCount = 0;
                            this.DigitalSignalMap[info.ParamType].BlockCount = 0;
                            this.DigitalSignalMap[info.ParamType].SignalData.Clear();
                            this.XAxis.DrawSignalData(null, count);
                            #endregion

                            //해당 축의 정보를 만들어서 Write
                            (ChParam as IByteArrayParam).ArrayValue = new Byte[2] { (Byte)((Int32)info.ParamType), 0x00 };
                            ChParam.m_DevParamInfo.m_Address = 0x1117 + count;
                            ChParam.m_DevParamInfo.m_Length = 1;
                            this.MyDevice.Write(ChParam);

                            count++;
                        }

                        //통신 실행
                        (Param as IByteArrayParam).ArrayValue = new Byte[2] { 0x01, 0x00 };
                        this.MyDevice.Write(Param);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                this.m_IsOscilloCommCheck = value;
                this.NotifyPropertyChanged("IsOscilloCommCheck");
            }
        }

        //Communication / Stop 표시 텍스트
        private String m_OscilloscopeText;
        public String OscilloscopeText
        {
            get { return this.m_OscilloscopeText; }
            set
            {
                this.m_OscilloscopeText = value;
                this.NotifyPropertyChanged("OscilloscopeText");
            }
        }

        //선택한 시간 Unit
        private String m_Timeunit;
        public String Timeunit
        {
            get { return this.m_Timeunit; }
            set
            {
                //현재 디바이스 존재한다면 화면을 다시 그린다.
                if (this.MyDevice != null)
                    this.OscilloscopeControl.DrawGraphDisplay();

                this.m_Timeunit = value;
                this.XAxisHeader = "Time[" + value + "]";
                this.NotifyPropertyChanged("Timeunit");
            }
        }

        //선택한 시간 Unit 인덱스
        private Int32 m_TimeunitIndex;
        public Int32 TimeunitIndex
        {
            get { return this.m_TimeunitIndex; }
            set
            {
                if (value < 0)
                    return;

                this.Timeunit = this.m_TimeunitList[value];

                this.m_TimeunitIndex = value;
                this.NotifyPropertyChanged("TimeunitIndex");
                this.NotifyPropertyChanged("Timeunit");
            }
        }

        //선택한 시간 UnitList
        private List<String> m_TimeunitList;
        public List<String> TimeunitList
        {
            get { return this.m_TimeunitList; }
            set
            {
                this.m_TimeunitList = value;
                this.NotifyPropertyChanged("TimeunitList");
            }
        }

        //선택한 시간 ZoomMode
        private ZoomMode m_ZoomMode;
        public ZoomMode ZoomMode
        {
            get { return this.m_ZoomMode; }
            set
            {
                this.m_ZoomMode = value;
                this.NotifyPropertyChanged("ZoomMode");
            }
        }

        //선택한 줌 모드 인덱스
        private Int32 m_ZoomModeIndex;
        public Int32 ZoomModeIndex
        {
            get { return this.m_ZoomModeIndex; }
            set
            {
                if (value < 0)
                    return;

                //통신 중이라면 리턴
                if (!this.IsOscilloCommCheck)
                    return;

                switch (this.m_ZoomModeList[value])
                {
                    case "Time[ms]":
                        this.m_ZoomMode = ZoomMode.Only_XAis;
                        break;
                    case "DataScale":
                        this.m_ZoomMode = ZoomMode.Only_YAxis;
                        break;
                    case "Time[ms], DataScale":
                        this.m_ZoomMode = ZoomMode.Both;
                        break;
                }

                this.m_ZoomModeIndex = value;
                this.NotifyPropertyChanged("ZoomModeIndex");
                this.NotifyPropertyChanged("ZoomMode");
            }
        }

        //줌 모드 리스트
        private List<String> m_ZoomModeList;
        public List<String> ZoomModeList
        {
            get { return this.m_ZoomModeList; }
            set
            {
                this.m_ZoomModeList = value;
                this.NotifyPropertyChanged("ZoomModeList");
            }
        }

        //휠 배율
        private Double m_WheelScale;
        public Double WheelScale
        {
            get { return this.m_WheelScale; }
            set { this.m_WheelScale = value; }
        }

        //Backup StartValue
        private Double m_BackUpStartValue;
        public Double BackUpStartValue
        {
            get { return this.m_BackUpStartValue; }
            set { this.m_BackUpStartValue = value; }
        }

        //Backup TickScale
        private Double m_BackUpTickScale;
        public Double BackUpTickScale
        {
            get { return this.m_BackUpTickScale; }
            set { this.m_BackUpTickScale = value; }
        }


        //선택한 X축 스케일 값
        private Int32 m_XAxisScaleValue;
        public Int32 XAxisScaleValue
        {
            get { return this.m_XAxisScaleValue; }
            set
            {
                this.m_XAxisScaleValue = value;
                this.NotifyPropertyChanged("XAxisScaleValue");

                this.XAxis.GetAutoScaleValue();
            }
        }

        //선택한 X축 스케일 인덱스
        private Int32 m_XAxisScaleIndex;
        public Int32 XAxisScaleIndex
        {
            get { return this.m_XAxisScaleIndex; }
            set
            {
                if (value < 0)
                    return;

                //통신중이라면 리턴
                if (!this.IsOscilloCommCheck)
                    return;

                Int32 UnitIndex = this.m_ShowXAxisScaleList[value].IndexOf("ms");
                Int32 TickScale = Int32.Parse(this.m_ShowXAxisScaleList[value].Substring(0, UnitIndex));

                this.TickScaleValue = TickScale * 2;

                this.m_XAxisScaleIndex = value;

                this.XAxisScaleValue = this.m_XAxisScaleList[value];

                this.NotifyPropertyChanged("XAxisScaleIndex");
                this.NotifyPropertyChanged("XAxisScaleValue");
            }
        }

        //선택한 X축 스케일 리스트(표시용)
        private List<String> m_ShowXAxisScaleList;
        public List<String> ShowXAxisScaleList
        {
            get { return this.m_ShowXAxisScaleList; }
            set
            {
                this.m_ShowXAxisScaleList = value;
                this.NotifyPropertyChanged("ShowXAxisScaleList");
            }
        }

        //선택한 X축 스케일 리스트(표시용)
        private Int32 m_TickScaleValue;
        public Int32 TickScaleValue
        {
            get { return this.m_TickScaleValue; }
            set
            {
                this.m_TickScaleValue = value;
                this.NotifyPropertyChanged("TickScaleValue");
            }
        }

        //선택한 X축 스케일 리스트
        private List<Int32> m_XAxisScaleList;
        public List<Int32> XAxisScaleList
        {
            get { return this.m_XAxisScaleList; }
            set
            {
                this.m_XAxisScaleList = value;
                this.NotifyPropertyChanged("XAxisScaleList");
            }
        }

        //선택한 FitMode 값
        private FitMode m_FitModeValue;
        public FitMode FitModeValue
        {
            get { return this.m_FitModeValue; }
            set
            {
                this.m_FitModeValue = value;
                this.NotifyPropertyChanged("FitModeValue");
            }
        }

        //선택한 X축 스케일 인덱스
        private Int32 m_FitModeIndex;
        public Int32 FitModeIndex
        {
            get { return this.m_FitModeIndex; }
            set
            {
                if (value < 0)
                    return;

                this.FitModeValue = this.m_FitModeList[value];

                this.m_FitModeIndex = value;
                this.NotifyPropertyChanged("FitModeIndex");
                this.NotifyPropertyChanged("FitModeValue");
            }
        }

        //선택한 X축 스케일 리스트
        private List<FitMode> m_FitModeList;
        public List<FitMode> FitModeList
        {
            get { return this.m_FitModeList; }
            set
            {
                this.m_FitModeList = value;
                this.NotifyPropertyChanged("FitModeList");
            }
        }

        //그래프 파라미터 타입 변수
        private OscilloscopeParameterType m_OscilloParamType;
        public OscilloscopeParameterType OscilloParamType
        {
            get { return this.m_OscilloParamType; }
            set
            {
                this.m_OscilloParamType = value;
                this.NotifyPropertyChanged("OscilloParamType");
            }
        }

        #region Y축 스케일 조정 컨트롤 Width 프로퍼티
        //Y축 스케일 조정 컨트롤 Width
        private GridLength m_YAxisScaleWidth1;
        public GridLength YAxisScaleWidth1
        {
            get { return this.m_YAxisScaleWidth1; }
            set
            {
                this.m_YAxisScaleWidth1 = value;
                this.NotifyPropertyChanged("YAxisScaleWidth1");
            }
        }

        private GridLength m_YAxisScaleWidth2;
        public GridLength YAxisScaleWidth2
        {
            get { return this.m_YAxisScaleWidth2; }
            set
            {
                this.m_YAxisScaleWidth2 = value;
                this.NotifyPropertyChanged("YAxisScaleWidth2");
            }
        }

        private GridLength m_YAxisScaleWidth3;
        public GridLength YAxisScaleWidth3
        {
            get { return this.m_YAxisScaleWidth3; }
            set
            {
                this.m_YAxisScaleWidth3 = value;
                this.NotifyPropertyChanged("YAxisScaleWidth3");
            }
        }

        private GridLength m_YAxisScaleWidth4;
        public GridLength YAxisScaleWidth4
        {
            get { return this.m_YAxisScaleWidth4; }
            set
            {
                this.m_YAxisScaleWidth4 = value;
                this.NotifyPropertyChanged("YAxisScaleWidth4");
            }
        }

        private GridLength m_YAxisScaleWidth5;
        public GridLength YAxisScaleWidth5
        {
            get { return this.m_YAxisScaleWidth5; }
            set
            {
                this.m_YAxisScaleWidth5 = value;
                this.NotifyPropertyChanged("YAxisScaleWidth5");
            }
        }
        #endregion

        // 오실로 스코프 컨트롤
        public OscilloscopeControl OscilloscopeControl { get; set; }

        //축 정보 리스트
        public ObservableCollection<ShowAxisInformation> AxisInfoList { get; set; }

        //Y축 높이 정보
        public Double YAxisLength { get; set; }

        #region 트리거 관련 프로퍼티
        //트리거 컨트롤
        public TriggerLineControl TriggerLine { get; set; }

        public TriggerControl TriggerObj { get; set; }

        public TriggerControl_ViewModel TriggerControl_ViewModel { get; set; }

        public OscilloscopeParameterType TriggerOccurType { get; set; }

        private Int32 m_TriggerDetectIndex;
        public Int32 TriggerDetectIndex
        {
            get { return this.m_TriggerDetectIndex; }
            set { this.m_TriggerDetectIndex = value; }
        }
        #endregion

        #region 프로퍼티 이벤트
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region 커서 체크

        //버튼 Enable
        private Boolean m_IsButtonEnabled;
        public Boolean IsButtonEnabled
        {
            get { return this.m_IsButtonEnabled; }
            set
            {
                this.m_IsButtonEnabled = value;
                this.NotifyPropertyChanged("IsButtonEnabled");
            }
        }

        //콤보박스 Enable
        private Boolean m_IsComboBoxEnabled;
        public Boolean IsComboBoxEnabled
        {
            get { return this.m_IsComboBoxEnabled; }
            set
            {
                this.m_IsComboBoxEnabled = value;
                this.NotifyPropertyChanged("IsComboBoxEnabled");
            }
        }

        //체크박스 Enable
        private Boolean m_IsCheckBoxEnabled;
        public Boolean IsCheckBoxEnabled
        {
            get { return this.m_IsCheckBoxEnabled; }
            set
            {
                this.m_IsCheckBoxEnabled = value;
                this.NotifyPropertyChanged("IsCheckBoxEnabled");
            }
        }
        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public OscilloscopeContent()
        {
            #region 테마 변경
            this.m_ThemaColor = Brushes.Transparent;
            this.m_ThemaComboColor = Brushes.Transparent;
            this.m_ThemaForeColor = Brushes.Black;
            this.m_SettingColor = Brushes.FloralWhite;
            this.m_DownArrowShape = "▼";
            this.m_UpArrowShape = "▲";
            #endregion

            m_SelectedAxisNameList = new List<String>();

            //Wheel Scale
            this.m_WheelScale = 1.0d;

            //MeasureButton Enable
            this.m_IsButtonEnabled = true;

            //ComboBox Enable
            this.m_IsComboBoxEnabled = true;

            //CheckBox Enable
            this.m_IsCheckBoxEnabled = true;

            //데이터 업데이트 Sleep 시간 조장 Event
            this.m_AutoAdjustmentEvent = new AutoResetEvent(false);

            //통신 종료 시간
            this.EndPacketIndex = 20000;

            //트리거 검출 시작 인덱스
            this.m_TriggerDetectIndex = 0;

            //쓰레드 종료 플래그
            this.m_IsExitThread = false;
            this.m_IsStopComm = true;

            this.m_ZoomMode = new ZoomMode();
            this.m_ZoomModeIndex = 0;
            this.m_ZoomModeList = new List<String>();
            foreach (String name in Enum.GetNames(typeof(ZoomMode)))
            {
                ZoomMode tmpZoomMode = new ZoomMode();
                Enum.TryParse(name, out tmpZoomMode);
                switch (tmpZoomMode)
                {
                    case ZoomMode.Only_XAis:
                        this.m_ZoomModeList.Add("Time[ms]");
                        break;
                    case ZoomMode.Only_YAxis:
                        this.m_ZoomModeList.Add("DataScale");
                        break;
                    case ZoomMode.Both:
                        this.m_ZoomModeList.Add("Time[ms], DataScale");
                        break;
                }
            }

            //Y축 컨트롤 Tick 모드관련 변수
            this.m_FitModeValue = new FitMode();
            this.m_FitModeIndex = 0;
            this.m_FitModeList = new List<FitMode>();
            foreach (String name in Enum.GetNames(typeof(FitMode)))
            {
                FitMode tmpMode = new FitMode();
                Enum.TryParse(name, out tmpMode);
                this.m_FitModeList.Add(tmpMode);
                break;
            }

            this.TickScaleValue = 1000;

            //X축 설정 관련 변수
            this.m_XAxisScaleList = new List<Int32>();
            this.m_XAxisScaleList.Add(2);
            this.m_XAxisScaleList.Add(5);
            this.m_XAxisScaleList.Add(10);
            this.m_XAxisScaleList.Add(20);
            this.m_XAxisScaleList.Add(50);
            this.m_XAxisScaleList.Add(100);

            this.m_ShowXAxisScaleList = new List<String>();
            for (int i = 0; i < m_XAxisScaleList.Count; i++)
                this.m_ShowXAxisScaleList.Add((1000 / m_XAxisScaleList[i]).ToString() + "ms");

            this.m_XAxisScaleValue = 2;
            this.m_XAxisScaleIndex = 0;

            this.m_IsOscilloCommCheck = true;
            this.m_IsCommBtnEnabled = false;
            this.m_OscilloscopeText = "Communication";

            //Y축 스케일 조정 컨트롤 Width
            this.m_YAxisScaleWidth1 = new GridLength(60, GridUnitType.Pixel);
            this.m_YAxisScaleWidth2 = new GridLength(60, GridUnitType.Pixel);
            this.m_YAxisScaleWidth3 = new GridLength(60, GridUnitType.Pixel);
            this.m_YAxisScaleWidth4 = new GridLength(60, GridUnitType.Pixel);
            this.m_YAxisScaleWidth5 = new GridLength(60, GridUnitType.Pixel);

            this.m_Name = String.Empty;
            this.m_Type = ContentType.OscilloscopeContent;
            this.m_MyDevice = null;
            this.m_DigitalSignalMap = new Dictionary<OscilloscopeParameterType, DigitalSignal>();
            this.m_YAxisList = new List<YAxisControl>();
            this.m_XAxis = null;
            this.AxisInfoList = new ObservableCollection<ShowAxisInformation>();
            this.m_BrushColorArray = new Brush[5] { Brushes.Red, Brushes.DarkOrange, Brushes.Purple, Brushes.DarkGreen, Brushes.Blue };

            //X축 정보
            this.m_XAxisHeader = "Time[" + this.m_Timeunit + "]";

            //X축 시간 설정
            this.m_TimeValueList = new List<String>();
            this.m_Timeunit = String.Empty;
            this.m_TimeunitList = new List<String>();
            this.m_TimeunitList.Add("ms");
            this.m_TimeunitList.Add("s");
            this.TimeunitIndex = 0;

            //그래프 파라미터 타입 Enum형 변수
            this.m_OscilloParamType = new OscilloscopeParameterType();

            //쓰레드 관련 변수
            this.m_PacketData = null;
        }

        /// <summary>
        /// 모두 선택
        /// </summary>
        public void AllCheck(Object sender)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                foreach (DevParam param in Model.Root.Children)
                {
                    if (param.ParamInfo.ParamType.Equals(ParamType.Category))
                        continue;

                    param.IsChecked = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 모두 해제
        /// </summary>
        public void AllUnCheck(Object sender)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                foreach (DevParam param in Model.Root.Children)
                {
                    if (param.ParamInfo.ParamType.Equals(ParamType.Category))
                        continue;

                    param.IsChecked = false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 통신 함수
        /// </summary>
        public void Communication(Object sender, Boolean IsUpdate=false)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                IParam recvParam = null;
                for (int i = 0; i < Model.Root.Children.Count; i++)
                {
                    if ((Model.Root.Children[i] as DevParam).IsChecked)
                    {
                        if (IsUpdate)
                        {
                            AccessMode mode = Model.Root.Children[i].AccessMode;
                            ParamRepresentation Representation = Model.Root.Children[i].Representation;
                            recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                            if (recvParam == null)
                            {
                                PublicVar.MainWnd.Process_Connect(false);
                                throw new Exception("Failed to read data");
                            }
                            (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                            recvParam.AccessMode = mode;
                            (recvParam as DevParam).m_Representation = Representation;
                            Model.Root.Children[i] = recvParam;
                        }
                        else
                        {
                            if (Model.Root.Children[i].AccessMode.Equals(AccessMode.ReadOnly))
                            {
                                ParamRepresentation Representation = Model.Root.Children[i].Representation;
                                recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                                (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                                recvParam.AccessMode = AccessMode.ReadOnly;
                                if (recvParam == null)
                                {
                                    PublicVar.MainWnd.Process_Connect(false);
                                    throw new Exception("Failed to read data");
                                }
                                (recvParam as DevParam).m_Representation = Representation;
                                Model.Root.Children[i] = recvParam;
                            }
                            else if (Model.Root.Children[i].AccessMode.Equals(AccessMode.ReadWrite))
                            {
                                //Write 후에 Read한다.
                                AccessMode mode = Model.Root.Children[i].AccessMode;
                                ParamRepresentation Representation = Model.Root.Children[i].Representation;
                                this.MyDevice.Write(Model.Root.Children[i]);
                                recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                                if (recvParam == null)
                                {
                                    PublicVar.MainWnd.Process_Connect(false);
                                    throw new Exception("Failed to read data");
                                }
                                (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                                recvParam.AccessMode = mode;
                                (recvParam as DevParam).m_Representation = Representation;
                                Model.Root.Children[i] = recvParam;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 데이터 초기화 함수 (기본값으로 변경)
        /// </summary>
        public void DataReset(Object sender)
        {
            try
            {
                //현재 기능을 수행하는 Content의 View와 정보를 가져온다.
                ParameterView prop = sender as ParameterView;
                DevParam Param = (prop.Tag as ParameterModel).Root;

                foreach (DevParam subParam in Param.Children)
                {
                    //초기 값(기본 값) 정보를 가져온다.
                    String InitValue = subParam.m_DevParamInfo.m_InitValue;
                    if (InitValue.Equals(String.Empty) && subParam.ParamInfo.ParamType != ParamType.Enum)
                        continue;
                    //파라미터 타입에 따라서 캐스팅을 해서 Value을 바꿔준다.
                    switch (subParam.ParamInfo.ParamType)
                    {
                        case ParamType.Integer:
                            subParam.Value = Convert.ToInt32(InitValue);
                            break;
                        case ParamType.String:
                            subParam.Value = InitValue;
                            break;
                        case ParamType.Byte:
                            subParam.Value = Convert.ToByte(InitValue);
                            break;
                        case ParamType.ByteArray:
                            subParam.Value = Encoding.Default.GetBytes(InitValue);
                            break;
                        case ParamType.Boolean:
                            Int32 tmp = Convert.ToInt32(InitValue);
                            subParam.Value = Convert.ToBoolean(tmp);
                            break;
                        case ParamType.Short:
                            subParam.Value = Convert.ToInt16(InitValue);
                            break;
                        case ParamType.Float:
                            subParam.Value = Convert.ToSingle(InitValue);
                            break;
                        case ParamType.Enum:
                            if (subParam.AccessMode == AccessMode.ReadOnly)
                                return;

                            Int32 enumVal = (InitValue == String.Empty) ? 0 : Convert.ToInt32(InitValue);
                            (subParam as IEnumParam).EnumIntValue = enumVal;
                            break;
                    }
                }

                //초기화 된 값으로 새로운 모델을 생성하고 View를 바꿔준다.
                ParameterModel newModel = new ParameterModel(Param, false);
                prop._tree.Model = newModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터 저장 함수
        /// </summary>
        /// <param name="Param">저장하는 파라미터 정보</param>
        public void SaveData(IParam Param)
        {
            try
            {
                this.MyDevice.Write(Param);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 받은 그래프 패킷을 시그널 데이터로 변경해서 저장하는 함수
        /// </summary>
        public void PacketParse()
        {
            try
            {
                //디바이스 객체
                Device Device = null;
                if (this.MyDevice is Device)
                    Device = this.MyDevice as Device;

                //트리거 이벤트 발생 플래그
                Boolean IsOccuredTriggerEvent = false;

                while (!this.IsExitThread)
                {
                    //통신 중이 아니면 대기
                    if (this.m_IsStopComm)
                        continue;

                    Boolean IsMatchIndex = true;
                    Int32 IndexCount = 0;

                    //패킷 수신 대기
                    Device.NewPacketEvent.WaitOne();

                    lock (Device.PacketQueueLock)
                    {
                        //1개 이하일 때는 Continue
                        if (Device.PacketQueue.Count < 1)
                            continue;
                        //2개 이상일 때는 반복적으로 Queue에서 데이터를 뺀다.
                        else if (Device.PacketQueue.Count > 1)
                            Device.NewPacketEvent.Set();

                        //큐에서 패킷 데이터 하나 빼기
                        this.m_PacketData = Device.PacketQueue.Dequeue();
                    }

                    //패킷 인덱스 정보 가져오기
                    Byte[] indexData = new Byte[2];
                    Array.Copy(this.m_PacketData, 0, indexData, 0, 2);
                    //디바이스로 부터 받은 인덱스 정보
                    Int32 index = BitConverter.ToInt16(indexData, 0);

                    //패킷 인덱스가 일치하는지 확인
                    IsMatchIndex = (index.Equals(this.PacketIndex)) ? true : false;
                    //현재 인덱스와 받은 인덱스의 차이 값
                    IndexCount = index - this.PacketIndex;

                    //패킷 => 시그날 데이터로 변경
                    IsOccuredTriggerEvent = ConvPacketToSignal(IsMatchIndex, IndexCount, IsOccuredTriggerEvent);

                    //시그날 업데이트 이벤트 Set()
                    Device.UpdatePacketEvent.Set();

#if true
                    if (this.PacketIndex == PublicVar.MainWnd.ViewModel.DataReceiveTime * 1000)
                    {
                        if (IsOccuredTriggerEvent)
                        {
                            #region 트리거 데이터 처리
                            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                //시작 시간과 끝 시간을 구한다.
                                Int32 EndTime = GetTriggerTime();

                                //트리거 사용 해제
                                this.TriggerControl_ViewModel.IsEnable = false;

                                Int32 Count = 0;
                                foreach (ShowAxisInformation info in this.AxisInfoList)
                                {
                                    if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                                    {
                                        String EnumName = info.CurrentSelectedAxisName.Replace(" ", "_");
                                        OscilloscopeParameterType type = new OscilloscopeParameterType();
                                        Enum.TryParse(EnumName, out type);

                                        //시그널 데이터 백업
                                        this.BackupSignalArray = new Signal[this.DigitalSignalMap[type].SignalData.Count];
                                        Array.Copy(this.DigitalSignalMap[type].SignalData.ToArray(), 0,
                                        this.BackupSignalArray, 0, this.BackupSignalArray.Length);

                                        //트리거 시간 만큼 데이터를 잘라서 복사해준다.
                                        Signal[] TriggerSignal = new Signal[EndTime * 10];
                                        Array.Copy(this.DigitalSignalMap[type].SignalData.ToArray(),
                                            0, TriggerSignal, 0, TriggerSignal.Length);
                                        this.DigitalSignalMap[type].SignalData = new List<Signal>(TriggerSignal);

                                        //데이터 그리기
                                        SignalOverlay overlay = this.DigitalSignalMap[type].GetOverlay();
                                        this.XAxis.DrawSignalData(overlay, Count);
                                    }
                                    Count++;
                                }
                            });
                            #endregion
                        }

                        //SMH9999 측정 시간중 최대시간 파일 저장
                        //Save_FunctionOccurTime(this.MaxTime);

                        Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            //정지 버튼
                            this.IsOscilloCommCheck = true;
                        });
                    }
#endif
#if false
                    if (IsOccuredTriggerEvent)
                    {
                        if (this.PacketIndex == PublicVar.MainWnd.ViewModel.DataReceiveTime * 1000)
                        {
                            #region 트리거 데이터 처리
                            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                //시작 시간과 끝 시간을 구한다.
                                Int32 EndTime = GetTriggerTime();

                                //트리거 사용 해제
                                this.TriggerControl_ViewModel.IsEnable = false;

                                Int32 Count = 0;
                                foreach (ShowAxisInformation info in this.AxisInfoList)
                                {
                                    if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                                    {
                                        String EnumName = info.CurrentSelectedAxisName.Replace(" ", "_");
                                        OscilloscopeParameterType type = new OscilloscopeParameterType();
                                        Enum.TryParse(EnumName, out type);

                                        //시그널 데이터 백업
                                        this.BackupSignalArray = new Signal[this.DigitalSignalMap[type].SignalData.Count];
                                        Array.Copy(this.DigitalSignalMap[type].SignalData.ToArray(), 0,
                                        this.BackupSignalArray, 0, this.BackupSignalArray.Length);

                                        //트리거 시간 만큼 데이터를 잘라서 복사해준다.
                                        Signal[] TriggerSignal = new Signal[EndTime * 10];
                                        Array.Copy(this.DigitalSignalMap[type].SignalData.ToArray(),
                                            0, TriggerSignal, 0, TriggerSignal.Length);
                                        this.DigitalSignalMap[type].SignalData = new List<Signal>(TriggerSignal);

                                        //데이터 그리기
                                        SignalOverlay overlay = this.DigitalSignalMap[type].GetOverlay();
                                        this.XAxis.DrawSignalData(overlay, Count);
                                    }
                                    Count++;
                                }

                                //TimeScale 조정 임시
                                //this.XAxisScaleIndex = 3;

                                //라인 그리기
                                this.XAxis.GetAutoScaleValue(false);
                            });
                    #endregion
                        }
                    }
#endif
                }// End of While
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 저장된 시그널 데이터를 화면에 업데이트하는 함수
        /// </summary>
        public void UpdateSignal()
        {
            try
            {
                Int32 AutoSleep = 0;
                Int32 StdSleepValue = 120;
                //Int32 FileCount = 0;

                long FunctionOccurTime = 0;

                Device Device = null;
                if (this.MyDevice is Device)
                    Device = this.MyDevice as Device;

                while (!this.IsExitThread)
                {
                    //통신 중이 아니면 대기
                    if (this.m_IsStopComm)
                        continue;

                    //파라미터 타입
                    Stopwatch sw = new Stopwatch();
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        sw.Start();

#if false
                        Int32 Count = 0;
                        foreach (ShowAxisInformation info in this.AxisInfoList)
                        {
                            if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                            {
                                lock (Device.SignalLock)
                                {
                                    //큐에서 list<Signal> 전체 업데이트
                                    //X축 StartTime을 DigitalSignal시간으로 변경해준다.
                                    if (this.DigitalSignalMap[info.ParamType].XAxisInfo.StartValue > 0)
                                        this.XAxis.GetAutoScaleValue(false);
                                }

                                SignalOverlay overlay = this.DigitalSignalMap[info.ParamType].GetOverlay();
                                if (!this.IsOscilloCommCheck)
                                {
                                    this.XAxis.DrawSignalData(overlay, Count);
                                }
                            }
                            Count++;
                        }
#endif

#if true
                        //데이터 그리기
                        this.XAxis.GetAutoScaleValue(true);
#endif
                        AutoAdjustmentEvent.Set();
                        sw.Stop();
                    });
                    sw.Stop();

                    //데이터 그리는 이벤트 종료 대기
                    AutoAdjustmentEvent.WaitOne();

                    //함수 실행시간을 구함
                    FunctionOccurTime = sw.ElapsedMilliseconds;

                    //함수 실행 시간이 프레임시간(40ms)보다 클 경우는 Sleep == 0
                    if (FunctionOccurTime >= StdSleepValue)
                        AutoSleep = 0;
                    //함수 실행 시간이 프레임 시간보다 클 경우는 프레임 시간 - 함수 실행 시간을 Sleep시간으로 지정
                    else
                        AutoSleep = (Int32)(StdSleepValue - FunctionOccurTime);
                    Thread.Sleep(AutoSleep);

                    //SMH9999 측정 시간 파일 저장
                    //Save_FunctionOccurTime(FunctionOccurTime, AutoSleep, FileCount++);
                    //CheckMaxSecond(FunctionOccurTime);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 실행시간 측정 데이터 저장
        private void Save_FunctionOccurTime(long Time, Int32 AutoSleep=-1, Int32 Count=-1)
        {
            StreamWriter sw = null;
            FileStream fs = null;

            try
            {
                String Path = AppDomain.CurrentDomain.BaseDirectory + "\\FunctionOccuredTimeFile.txt";
                fs = new FileStream(Path, FileMode.Append, FileAccess.Write);

                sw = new StreamWriter(fs);
                if (Count == -1)
                {
                    sw.Write("최대 시간 : ");
                    sw.Write(Time.ToString());
                }
                else
                {
                    sw.Write(Count++ + ". 함수 실행 시간 : ");
                    sw.Write(Time.ToString() + "ms");
                    sw.Write("\t");
                    sw.Write("Sleep Time : ");
                    sw.Write(AutoSleep.ToString());
                }
                sw.WriteLine();
                sw.WriteLine();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }

                if (fs != null)
                {
                    fs.Dispose();
                    fs.Close();
                }
            }
        }
        #endregion

        //패킷 데이터를 시그널 데이터로 변환해주는 함수
        private Boolean ConvPacketToSignal(Boolean IsMatchIndex, Int32 IndexCount, Boolean IsOccuredTriggerEvent)
        {
            try
            {
                IsMatchIndex = true;

                if (IndexCount < 0)
                    IndexCount = 0;

                Device Device = (this.MyDevice as Device);
                List<Signal> TriggerSignalList = new List<Signal>();

                Int32 count = 0;
                foreach (ShowAxisInformation info in this.AxisInfoList)
                {
                    if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                    {
                        //현재 선택된 축 이름으로 OscilloscopeParameterType을 구한다.
                        String EnumName = this.AxisInfoList[count].CurrentSelectedAxisName.Replace(" ", "_");
                        OscilloscopeParameterType type = new OscilloscopeParameterType();
                        Enum.TryParse(EnumName, out type);

                        //해당 파라미터의 DigitalSignal데이터를 가져온다.
                        List<Signal> SignalList = new List<Signal>();
                        DigitalSignal DigitSignal = this.DigitalSignalMap[type];

                        //패킷데이터 축 별로 나눔
                        Byte[] Packet = new Byte[40];
                        Array.Copy(this.m_PacketData, (2 + (count * Packet.Length)), Packet, 0, Packet.Length);

                        //패킷 로스가 일어나지 않은 경우
                        if (IsMatchIndex)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                Byte[] tmpData = new Byte[4];
                                Array.Copy(Packet, i * 4, tmpData, 0, 4);
                                Signal signal = new Signal();
                                signal.IsValid = true;
                                signal.Data = Convert.ToDouble(BitConverter.ToInt32(tmpData, 0));
                                SignalList.Add(signal);
                            }
                            lock (Device.SignalLock)
                                this.DigitalSignalMap[type].AddSignal(SignalList);
                        }
                        //패킷 로스가 일어난 경우
                        else
                        {
                            this.PacketIndex += IndexCount;

                            //로스된 횟수만큼 fake 패킷을 만들어서 넣어준다.
                            for (int j = 0; j < IndexCount; j++)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    Signal signal = new Signal();
                                    signal.Data = 0;
                                    signal.IsValid = false;
                                    SignalList.Add(signal);
                                }
                                lock (Device.SignalLock)
                                    this.DigitalSignalMap[type].AddSignal(SignalList);
                            }
                        }

                        if (this.TriggerControl_ViewModel.SelectChannel.Equals(count))
                            TriggerSignalList = SignalList;

                        this.DigitalSignalMap[type].BlockCount++;
                    }
                    count++;
                }
                this.PacketIndex++;

                //트리거 이벤트 발생 체크
                return CheckOccuredTriggerEvent(TriggerSignalList, IsOccuredTriggerEvent);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 트리거 이벤트가 발생하는지 체크해서 결과를 리턴해주는 함수
        /// </summary>
        private Boolean CheckOccuredTriggerEvent(List<Signal> SignalList, Boolean IsOccuredTriggerEvent)
        {
            try
            {
                //트리거 사용시 처리
                if (this.TriggerControl_ViewModel.IsEnable)
                {
                    if (SignalList.Count > 0)
                    {
                        Boolean IsFirstPacket = false;
                        TriggerDectectTool Tool = new TriggerDectectTool(this.TriggerControl_ViewModel);
                        Tool.SignalList = SignalList;

                        if (this.PacketIndex == 1)
                            IsFirstPacket = true;
                        else
                            Tool.LastSignal = (Signal)this.LastSignal.Clone();

                        //첫번째 패킷인 경우만 처리
                        Tool.Run(IsFirstPacket);
                        this.LastSignal = SignalList[SignalList.Count - 1];
                        if (Tool.IsEvent)
                        {
                            //트리거 타입을 구한다.
                            String EnumName =
                                this.AxisInfoList[this.TriggerControl_ViewModel.SelectChannel].CurrentSelectedAxisName.Replace(" ", "_");
                            OscilloscopeParameterType type = new OscilloscopeParameterType();
                            Enum.TryParse(EnumName, out type);

                            //발생한 트리거의 파라미터 타입을 구한다.
                            this.TriggerOccurType = type;
                            //발생한 트리거 패킷의 위치(인덱스)값
                            this.TriggerDetectIndex = this.PacketIndex - 1;
                            IsOccuredTriggerEvent = Tool.IsEvent;
                            //트리거 시간을 더해서 총 받는 시간을 구한다.
                            this.EndPacketIndex = this.TriggerControl_ViewModel.SamplingTime * 1000
                                    + this.TriggerDetectIndex;
                            PublicVar.MainWnd.ViewModel.DataReceiveTime = this.EndPacketIndex / 1000;
                        }
                    }
                }
                return IsOccuredTriggerEvent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Int32 GetTriggerTime()
        {
            try
            {
                //샘플링 시간
                Int32 SamplingTime = this.TriggerControl_ViewModel.SamplingTime;

                //트리거가 발생했던 인덱스를 가져온다.
                Int32 StartTime = 0;
                //시작시간 : 트리거 발생 인덱스 - 샘플링 시간 / 2
                if (this.TriggerDetectIndex * 10 < ((SamplingTime / 2.0) * 10000))
                    StartTime = 0;
                else
                    StartTime = (Int32)(((this.TriggerDetectIndex * 10) - ((SamplingTime / 2.0) * 10000)) / 10);
                this.DigitalSignalMap[this.TriggerOccurType].XAxisInfo.StartValue = StartTime;

                //끝 시간 : 트리거 발생 인덱스 + 샘플링 시간 / 2
                Int32 EndTime = StartTime + ((SamplingTime * 10000) / 10);

                return EndTime;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ChangeUIState(Boolean Flag)
        {
            try
            {
                if (Flag)
                {
                    //버튼 표시 텍스트
                    this.OscilloscopeText = "Communication";

                    //Button Enabled
                    this.IsButtonEnabled = true;

                    //ComboBox Enabled
                    this.IsComboBoxEnabled = true;

                    //CheckBox Enable
                    this.IsCheckBoxEnabled = true;

                    //TriggerControl내 Control Enable
                    this.TriggerControl_ViewModel.IsControlEnabled = true;

                    //쓰레드 중지 플래그
                    this.m_IsStopComm = true;
                }
                else
                {
                    //버튼 표시 텍스트
                    this.OscilloscopeText = "Stop";

                    //쓰레드 실행 플래그
                    this.m_IsStopComm = false;

                    //인덱스 정보 초기화
                    this.PacketIndex = 0;

                    //Button Disable
                    this.IsButtonEnabled = false;

                    //ComboBox Disable
                    this.IsComboBoxEnabled = false;

                    //CheckBox Disable
                    this.IsCheckBoxEnabled = false;

                    //TriggerControl내 Control Enable
                    this.TriggerControl_ViewModel.IsControlEnabled = false;

                    this.XAxis.ViewModel.AxisInfoObj.StartValue = 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 테마 변경시 일어나는 기능을 수행하는 함수 
        /// </summary>
        public void ChangeThema(Boolean IsWhite=true)
        {
            try
            {
                if (this.OscilloscopeControl == null)
                    return;

                if (IsWhite)
                {
                    this.ThemaColor = Brushes.Transparent;
                    this.ThemaForeColor = Brushes.Black;
                    this.ThemaComboColor = Brushes.Transparent;
                    this.SettingColor = Brushes.FloralWhite;
                    this.OscilloscopeControl.Cursor1Check.Style = (Style)this.OscilloscopeControl.FindResource("InspectListCheckBoxStyle");
                    this.OscilloscopeControl.Cursor2Check.Style = (Style)this.OscilloscopeControl.FindResource("InspectListCheckBoxStyle");

                    if (this.TriggerObj != null)
                    {
                        this.TriggerObj.TriggerLine.mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                        this.TriggerObj.TriggerLine.line.Stroke = Brushes.Black;
                    }
                }
                else
                {
                    this.ThemaColor = Brushes.Black;
                    this.ThemaForeColor = Brushes.White;
                    this.ThemaComboColor = Brushes.White;
                    this.SettingColor = Brushes.Black;
                    this.OscilloscopeControl.Cursor1Check.Style = (Style)this.OscilloscopeControl.FindResource("InspectListCheckBoxWhiteStyle");
                    this.OscilloscopeControl.Cursor2Check.Style = (Style)this.OscilloscopeControl.FindResource("InspectListCheckBoxWhiteStyle");

                    if (this.TriggerObj != null)
                    {
                        this.TriggerObj.TriggerLine.mainGrid.Background = Brushes.Black;
                        this.TriggerObj.TriggerLine.line.Stroke = Brushes.White;
                    }
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
