using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CrevisLibrary;
using DCT_Graph;

namespace MotionGuidePro.Main
{
    public class MainWindow_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Wnd">MainWindow.xaml</param>

        #region 초기 화면 데이터

        //EncoderCountA
        private Int32 m_EncoderCountA;
        public Int32 EncoderCountA
        {
            get { return this.m_EncoderCountA; }
            set
            {
                this.m_EncoderCountA = value;
                this.NotifyPropertyChanged("EncoderCountA");
            }
        }

        //EncoderCountB
        private Int32 m_EncoderCountB;
        public Int32 EncoderCountB
        {
            get { return this.m_EncoderCountB; }
            set
            {
                this.m_EncoderCountB = value;
                this.NotifyPropertyChanged("EncoderCountB");
            }
        }

        public ObservableCollection<InitPageLine> BottomLedList { get; set; }
        public ObservableCollection<BoardLine> BoardLineList { get; set; }
        public ObservableCollection<InitPageLine> FirstLineList { get; set; }
        public ObservableCollection<InitPageLine> SecondLineList { get; set; }
        public ObservableCollection<InitPageLine> PHSLineList { get; set; }

        public ObservableCollection<BoardBaseInfo> BoardBaseInfoList { get; set; }
        #endregion

        public MainWindow_ViewModel(Object Wnd) : base(Wnd)
        {
            this.LogEvent += this.Log_Maker;
            this.BoardBaseInfoList = new ObservableCollection<BoardBaseInfo>();

            this.BottomLedList = new ObservableCollection<InitPageLine>();
            for (int i = 0; i < 5; i++)
            {
                InitPageLine tmp = new InitPageLine();
                tmp.InitializeViewModel();
                this.BottomLedList.Add(tmp);
            }

            this.BoardLineList = new ObservableCollection<BoardLine>();
            for (int i = 0; i < 20; i++)
            {
                BoardLine tmp = new BoardLine();
                tmp.InitializeViewModel();
                this.BoardLineList.Add(tmp);
            }

            this.FirstLineList = new ObservableCollection<InitPageLine>();
            for (int i = 0; i < 8; i++)
            {
                InitPageLine tmp = new InitPageLine();
                tmp.InitializeViewModel();
                this.FirstLineList.Add(tmp);
            }

            this.SecondLineList = new ObservableCollection<InitPageLine>();
            for (int i = 0; i < 8; i++)
            {
                InitPageLine tmp = new InitPageLine();
                tmp.InitializeViewModel();
                this.SecondLineList.Add(tmp);
            }

            this.PHSLineList = new ObservableCollection<InitPageLine>();
            for (int i = 0; i < 8; i++)
            {
                InitPageLine tmp = new InitPageLine();
                tmp.InitializeViewModel();
                this.PHSLineList.Add(tmp);
            }
        }

        //Open/Close 메뉴 아이템 객체
        public MenuItem OpenCloseMenuItem { get; set; }

        //로그 처리 이벤트
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        //스크롤 위치 업데이트 이벤트
        public event EventHandler<LogExecuteEventArgs> ScrollEvent;

        //모니터링 값 업데이트 이벤트
        public event EventHandler<MonitoringUpdateEventArgs> MonitoringUpdateEvent;

        // Measure 컨트롤
        public MeasureControl MeasureControl { get; set; }

        //아이콘 사용여부
        private Boolean m_UseIcon;
        public Boolean UseIcon
        {
            get { return this.m_UseIcon; }
            set
            {
                this.m_UseIcon = value;
                this.IconVisible = (value) ? Visibility.Visible : Visibility.Collapsed;
                this.NotifyPropertyChanged("UseIcon");
            }
        }

        //아이콘 Visible
        private Visibility m_IconVisible;
        public Visibility IconVisible
        {
            get { return this.m_IconVisible; }
            set
            {
                this.m_IconVisible = value;
                this.IconWidth = (value == Visibility.Visible) ?
                    new GridLength(20, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
                this.NotifyPropertyChanged("IconVisible");
            }
        }

        //아이콘 Width
        private GridLength m_IconWidth;
        public GridLength IconWidth
        {
            get { return this.m_IconWidth; }
            set
            {
                this.m_IconWidth = value;
                this.NotifyPropertyChanged("IconWidth");
            }
        }

        //에러상태 타이머
        private DispatcherTimer m_Timer;
        public DispatcherTimer Timer
        {
            get { return this.m_Timer; }
            set { this.m_Timer = value; }
        }

        //쓰레드 종료 플래그
        private Boolean m_IsThreadExit;
        public Boolean IsThreadExit
        {
            get { return this.m_IsThreadExit; }
            set { this.m_IsThreadExit = value; }
        }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            //초기화면 관련 변수 초기화
            this.m_EncoderCountA = 0;
            this.m_EncoderCountB = 0;

            //아이콘 Visibility
            this.m_IconVisible = (this.UseIcon) ? Visibility.Visible : Visibility.Collapsed;
            this.m_IconWidth = (this.UseIcon) ?
                new GridLength(20, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);

            //기본 정보
            this.DeviceName = String.Empty;
            this.m_ImagePathList = new Dictionary<String, String>();
            this.m_IsConnect = false;
            this.m_LogList = new ObservableCollection<LogInformation>();
            this.m_StateWidth = 0.0d;
            this.m_MessageWidth = 0.0d;
            this.m_ConnectEnd = false;
            this.m_FilePath = String.Empty;
            this.m_FaultConfigList = new Dictionary<String, String>();
            this.m_IsThreadExit = true;

            //모니터링 관련 변수
            this.m_MonitoringEvent = new AutoResetEvent(false);
            this.m_IsMonitoringStart = false;
            this.m_MonitoringMessage = "Monitoring Start";
            this.m_MonitoringView = null;
            this.m_MonitoringParam = null;
            this.m_IsMonitoringEnabled = false;

            //설정창 프로퍼티
            this.m_OnVisible = Visibility.Visible;
            this.m_OffVisible = Visibility.Collapsed;
            this.m_BaseFileName = String.Empty;
            this.m_BaseFilePath = String.Empty;
            this.m_Timeout = 0.0d;
            this.m_ServerAddress = String.Empty;
            this.m_ClientAddress = String.Empty;
            this.m_DataReceiveTime = 20;

            //에러 상태 읽어오기 위한 타이머
            this.m_Timer = new DispatcherTimer();
            this.m_Timer.Tick += StateViewUpdate;

            //에러상태 프로퍼티
            this.m_ErrorCode = "0x00";
            this.m_ErrorMessage = "No Error";
            this.m_ErrorDescription = "정상 동작";
            this.m_MotorStateCode = "0x00";
            this.m_MotorStateMessage = "Servo Off";

            //테마 타입
            this.m_ThemaTypeValue = ThemaType.WHITE_THEMA;

            //로그창에 스크롤을 자동으로 마지막으로 위치하게 하기위한 이벤트 등록
            this.ScrollEvent += (this.m_Wnd as MainWindow).LogView_ScrollEvent;
            this.MonitoringUpdateEvent += (this.m_Wnd as MainWindow).MonitoringInformationUpdate;
        }

        ///<summary>
        /// 변수
        /// </summary>
        //연결 확인 변수
        private Boolean m_ConnectEnd;

        private ThemaType m_ThemaTypeValue;
        public ThemaType ThemaTypeValue
        {
            get { return this.m_ThemaTypeValue; }
            set { this.m_ThemaTypeValue = value; }
        }

        /// <summary>
        /// 프로퍼티
        /// </summary>
        #region 모니터링 관련 변수
        //모니터링 버튼 Enable
        private Boolean m_IsMonitoringEnabled;
        public Boolean IsMonitoringEnabled
        {
            get { return this.m_IsMonitoringEnabled; }
            set
            {
                this.m_IsMonitoringEnabled = value;
                this.NotifyPropertyChanged("IsMonitoringEnabled");
            }
        }

        //모니터링 쓰레드
        private Thread m_MonitoringThread;
        public Thread MonitoringThread
        {
            get { return this.m_MonitoringThread; }
            set { this.m_MonitoringThread = value; }
        }

        //모니터링 이벤트
        private AutoResetEvent m_MonitoringEvent;
        public AutoResetEvent MonitoringEvent
        {
            get { return this.m_MonitoringEvent; }
            set { this.m_MonitoringEvent = value; }
        }

        // 모니터링 시작 / 종료 여부
        private Boolean m_IsMonitoringStart;
        public Boolean IsMonitoringStart
        {
            get { return this.m_IsMonitoringStart; }
            set
            {
                if (value)
                    this.m_MonitoringMessage = "Monitoring Stop";
                else
                    this.m_MonitoringMessage = "Monitoring Start";

                this.m_IsMonitoringStart = value;
                this.NotifyPropertyChanged("IsMonitoringStart");
                this.NotifyPropertyChanged("MonitoringMessage");
            }
        }

        //모니터링을 누른 View 데이터
        private ParameterView m_MonitoringView;
        public ParameterView MonitoringView
        {
            get { return this.m_MonitoringView; }
            set { this.m_MonitoringView = value; }
        }

        //모니터링 정보를 저장할 DevParam
        private DevParam m_MonitoringParam;
        public DevParam MonitoringParam
        {
            get { return this.m_MonitoringParam; }
            set { this.m_MonitoringParam = value; }
        }
        #endregion

        //파일 경로
        private String m_FilePath;
        public String FilePath
        {
            get { return this.m_FilePath; }
            set { this.m_FilePath = value; }
        }

        // 이미지 경로 Dictionary
        private Dictionary<String, String> m_ImagePathList;
        public Dictionary<String, String> ImagePathList
        {
            get { return this.m_ImagePathList; }
            set { this.m_ImagePathList = value; }
        }

        //연결 상태
        private Boolean m_IsConnect;
        public Boolean IsConnect
        {
            get { return this.m_IsConnect; }
            set
            {
                this.m_IsConnect = value;

                //오실로스코프 컨텐츠의 버튼 Enable 설정
                //연결 됐을 경우 : true
                //연결 되지 않았을 경우 : false
                (this.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent).IsCommBtnEnabled = value;

                //연결 됐을 경우
                if (value)
                {
                    //모니터링 쓰레드 시작
                    this.m_MonitoringThread = new Thread(() => MonitoringStart());
                    this.m_MonitoringThread.Start();

                    Dictionary<String, String> tmpDic = new Dictionary<String, String>();

                    TreeViewItem RootItem = (this.m_Wnd as MainWindow).treeView.Items[0] as TreeViewItem;

                    //DEVICE 아이콘 변경
                    ((RootItem.Header as Grid).Children[1] as Image).Source
                        = new BitmapImage(new Uri("../ICon/PCI.ico", UriKind.Relative));

                    SetInitParamDatas(true);

                    #region 아이콘 컬러 처리
                    Int32 index = -1;
                    foreach (String key in ImagePathList.Keys)
                    {
                        tmpDic.Add(key, String.Empty);
                        index = ImagePathList[key].IndexOf("/ICon");
                        String ImageFileFullName = ImagePathList[key].Substring(index + 6,
                            ImagePathList[key].Length - (index + 6));
                        String ImageFile = ImageFileFullName.Split('.')[0];
                        ImageFile = ImageFile.Split('_')[0];
                        String ContentName = ImageFile;
                        ImageFile = ImageFile + "." + ImageFileFullName.Split('.')[1];
                        tmpDic[key] = "pack://application:,,/ICon/" + ImageFile;

                        foreach (TreeViewItem subItem in RootItem.Items)
                        {
                            if (((subItem.Header as Grid).Children[0] as Label).Content.Equals(ContentName))
                            {
                                if (subItem.HasItems)
                                {
                                    (subItem.Items[0] as TreeViewItem).Header = (this.m_Wnd as MainWindow).SetTreeViewItemICon("Measure");
                                }

                                subItem.Header = (this.m_Wnd as MainWindow).SetTreeViewItemICon(ContentName);
                                break;
                            }
                        }
                    }

                    ImagePathList = tmpDic;
                    #endregion

                    //연결 확인
                    this.m_ConnectEnd = true;
                    //연결 완료 로그 메세지
                    this.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, "연결 완료", DateTime.Now.ToString("HH:mm:ss")));

                    //에러 상태 값 읽어오기
                    SetErrorStateInformation();

                    //타이머 동작
                    this.m_Timer.Start();
                }
                //연결 되지 않은 경우
                else
                {
                    //모니터링 쓰레드 종료
                    this.m_IsThreadExit = false;
                    if (this.m_MonitoringThread != null)
                    {
                        if (this.m_MonitoringThread.Join(1000))
                        {
                            this.m_MonitoringThread.Abort();
                            this.m_MonitoringThread = null;
                        }
                    }

                    Dictionary<String, String> tmpDic = new Dictionary<String, String>();

                    TreeViewItem RootItem = (this.m_Wnd as MainWindow).treeView.Items[0] as TreeViewItem;

                    //DEVICE 아이콘 변경
                    ((RootItem.Header as Grid).Children[1] as Image).Source
                        = new BitmapImage(new Uri("../ICon/PCI_OFF.png", UriKind.Relative));

                    SetInitParamDatas(false);

                    #region 아이콘 흑백 처리
                    Int32 index = -1;
                    foreach (String key in ImagePathList.Keys)
                    {
                        tmpDic.Add(key, String.Empty);
                        index = ImagePathList[key].IndexOf("/ICon");
                        String ImageFileFullName = ImagePathList[key].Substring(index + 6,
                            ImagePathList[key].Length - (index + 6));
                        String ImageFile = ImageFileFullName.Split('.')[0];
                        String ContentName = ImageFile;
                        ImageFile = ImageFile + "_OFF." + ImageFileFullName.Split('.')[1];
                        tmpDic[key] = "pack://application:,,/ICon/" + ImageFile;

                        foreach (TreeViewItem subItem in RootItem.Items)
                        {
                            if (((subItem.Header as Grid).Children[0] as Label).Content.Equals(ContentName))
                            {
                                if (subItem.HasItems)
                                {
                                    (subItem.Items[0] as TreeViewItem).Header = (this.m_Wnd as MainWindow).SetTreeViewItemICon("Measure");
                                }

                                subItem.Header = (this.m_Wnd as MainWindow).SetTreeViewItemICon(ContentName);
                                break;
                            }
                        }
                    }

                    ImagePathList = tmpDic;
                    #endregion

                    //이미 연결된 경우에 연결해제 로그 메세지 추가
                    if (this.m_ConnectEnd)
                        this.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, "연결 해제", DateTime.Now.ToString("HH:mm:ss")));

                    //타이머 중지
                    this.m_Timer.Stop();

                    //연결 확인 false
                    this.m_ConnectEnd = false;
                }
            }
        }

        // Device 객체
        public IDevice CurrentDevice { get; set; }

        // 모니터링 시작 / 종료 여부
        private String m_MonitoringMessage;
        public String MonitoringMessage
        {
            get { return this.m_MonitoringMessage; }
            set
            {
                this.m_MonitoringMessage = value;
                this.NotifyPropertyChanged("MonitoringMessage");
            }
        }

        //디바이스 이름
        private String m_DeviceName;
        public String DeviceName
        {
            get { return this.m_DeviceName; }
            set
            {
                this.m_DeviceName = value;
                this.NotifyPropertyChanged("DeviceName");
            }
        }

        //로그 정보 리스트
        private ObservableCollection<LogInformation> m_LogList;
        public ObservableCollection<LogInformation> LogList
        {
            get { return this.m_LogList; }
            set
            {
                this.m_LogList = value;
                this.NotifyPropertyChanged("LogList");
            }
        }

        //상태 Width값
        private Double m_StateWidth;
        public Double StateWidth
        {
            get { return m_StateWidth; }
            set
            {
                this.m_StateWidth = value;
                this.NotifyPropertyChanged("StateWidth");
            }
        }

        //메세지 Width값
        private Double m_MessageWidth;
        public Double MessageWidth
        {
            get { return m_MessageWidth; }
            set
            {
                this.m_MessageWidth = value;
                this.NotifyPropertyChanged("MessageWidth");
            }
        }

        //메세지 Width값
        private Double m_TimeWidth;
        public Double TimeWidth
        {
            get { return m_TimeWidth; }
            set
            {
                this.m_TimeWidth = value;
                this.NotifyPropertyChanged("TimeWidth");
            }
        }

        //Fault Config 정보
        private Dictionary<String, String> m_FaultConfigList;
        public Dictionary<String, String> FaultConfigList
        {
            get { return this.m_FaultConfigList; }
            set { this.m_FaultConfigList = value; }
        }

        #region 에러 상태 프로퍼티
        //에러 코드
        private String m_ErrorCode;
        public String ErrorCode
        {
            get { return this.m_ErrorCode; }
            set
            {
                this.m_ErrorCode = value;
                this.NotifyPropertyChanged("ErrorCode");
            }
        }

        //에러 코드
        private String m_ErrorMessage;
        public String ErrorMessage
        {
            get { return this.m_ErrorMessage; }
            set
            {
                this.m_ErrorMessage = value;
                this.NotifyPropertyChanged("ErrorMessage");
            }
        }

        //모터 상태 코드
        private String m_MotorStateCode;
        public String MotorStateCode
        {
            get { return this.m_MotorStateCode; }
            set
            {
                this.m_MotorStateCode = value;
                this.NotifyPropertyChanged("MotorStateCode");
            }
        }

        //모터 상태 코드
        private String m_MotorStateMessage;
        public String MotorStateMessage
        {
            get { return this.m_MotorStateMessage; }
            set
            {
                this.m_MotorStateMessage = value;
                this.NotifyPropertyChanged("MotorStateMessage");
            }
        }

        //에러 상태 설명
        private String m_ErrorDescription;
        public String ErrorDescription
        {
            get { return this.m_ErrorDescription; }
            set
            {
                this.m_ErrorDescription = value;
                this.NotifyPropertyChanged("ErrorDescription");
            }
        }
        #endregion

        #region 설정 창 프로퍼티
        //Connect Button Visible
        private Visibility m_OnVisible;
        public Visibility OnVisible
        {
            get { return this.m_OnVisible; }
            set
            {
                this.m_OnVisible = value;
                this.NotifyPropertyChanged("OnVisible");
            }
        }

        //Disconnect Button Visible
        private Visibility m_OffVisible;
        public Visibility OffVisible
        {
            get { return this.m_OffVisible; }
            set
            {
                this.m_OffVisible = value;
                this.NotifyPropertyChanged("OffVisible");
            }
        }

        //데이터 수신 시간
        private Int32 m_DataReceiveTime;
        public Int32 DataReceiveTime
        {
            get { return this.m_DataReceiveTime; }
            set
            {
                this.m_DataReceiveTime = value;
                this.NotifyPropertyChanged("DataReceiveTime");
            }
        }

        //기본 설정 모델
        private String m_BaseFileName;
        public String BaseFileName
        {
            get { return this.m_BaseFileName; }
            set
            {
                this.m_BaseFileName = value;
                this.NotifyPropertyChanged("BaseFileName");
            }
        }

        //기본 파일 저장 파일 경로
        private String m_BaseFilePath;
        public String BaseFilePath
        {
            get { return this.m_BaseFilePath; }
            set
            {
                this.m_BaseFilePath = value;
                this.NotifyPropertyChanged("BaseFilePath");
            }
        }

        //서버 IP주소
        private String m_ServerAddress;
        public String ServerAddress
        {
            get { return this.m_ServerAddress; }
            set
            {
                this.m_ServerAddress = value;
                if(this.CurrentDevice != null)
                    this.CurrentDevice.SvrAddress = value;
                this.NotifyPropertyChanged("ServerAddress");
            }
        }

        //클라이언트 IP주소
        private String m_ClientAddress;
        public String ClientAddress
        {
            get { return this.m_ClientAddress; }
            set
            {
                this.m_ClientAddress = value;
                if (this.CurrentDevice != null)
                    this.CurrentDevice.MyAddress = value;
                this.NotifyPropertyChanged("ClientAddress");
            }
        }

        //Write Timeout
        private Double m_Timeout;
        public Double Timeout
        {
            get { return this.m_Timeout; }
            set
            {
                this.m_Timeout = value;
                if (this.CurrentDevice != null)
                    this.CurrentDevice.Timeout = value;
                this.NotifyPropertyChanged("Timeout");
            }
        }

        //에러 상태 주기(ms)
        private Int32 m_ErrorStatePeriod;
        public Int32 ErrorStatePeriod
        {
            get { return this.m_ErrorStatePeriod; }
            set
            {
                this.m_ErrorStatePeriod = value;
                if (this.CurrentDevice != null)
                    (this.CurrentDevice as Device).ErrorStatePeriod = value;
                if (this.IsConnect)
                {
                    this.m_Timer.Stop();
                    this.m_Timer.Interval = TimeSpan.FromSeconds(this.m_ErrorStatePeriod);
                    this.m_Timer.Start();
                }
                this.NotifyPropertyChanged("ErrorStatePeriod");
            }
        }

        //모니터링 업데이트 시간
        private Double m_MonitoringTime;
        public Double MonitoringTime
        {
            get { return this.m_MonitoringTime; }
            set
            {
                this.m_MonitoringTime = value;
                if (this.CurrentDevice != null)
                    (this.CurrentDevice as Device).MonitoringTime = value;
                this.NotifyPropertyChanged("MonitoringTime");
            }
        }

        //Monitoring MinValue
        private Double m_MonitoringMinValue;
        public Double MonitoringMinValue
        {
            get { return this.m_MonitoringMinValue; }
            set { this.m_MonitoringMinValue = value; }
        }
        #endregion

        /// <summary>
        /// 함수
        /// </summary>
        // 파라미터 변경 CallBack 함수
        public void ParamUpdate(IParam ParamMain, IParam ParamDepend)
        {
            try
            {
                //넘어온 파라미터의 값이 서로 같은 경우는 Return;
                if (ParamMain.ParamInfo.ParamName.Equals(ParamDepend.ParamInfo.ParamName)
                    && ParamMain.ParamInfo.ParamType.Equals(ParamDepend.ParamInfo.ParamType)
                    && ParamMain.ParamInfo.InitValue.Equals(ParamDepend.ParamInfo.InitValue)
                    && ParamMain.ParamInfo.Length.Equals(ParamDepend.ParamInfo.Length)
                    && ParamMain.ParamInfo.Min.Equals(ParamDepend.ParamInfo.Min)
                    && ParamMain.ParamInfo.Max.Equals(ParamDepend.ParamInfo.Max)
                    && ParamMain.ParamInfo.PortType.Equals(ParamDepend.ParamInfo.PortType)
                    && ParamMain.ParamInfo.Address.Equals(ParamDepend.ParamInfo.Address)
                    && ParamMain.ParamInfo.Description.Equals(ParamDepend.ParamInfo.Description)
                    && ParamMain.Representation.Equals(ParamDepend.Representation)
                    && ParamMain.Value.Equals(ParamDepend.Value)
                    && ParamMain.AccessMode.Equals(ParamDepend.AccessMode))
                    return;

                //데이터를 동기화 해준다.
                (ParamDepend as DevParam).m_AccessMode = (ParamMain as DevParam).m_AccessMode;
                (ParamDepend as DevParam).m_DevParamInfo.m_ParamName = (ParamMain as DevParam).m_DevParamInfo.m_ParamName;
                (ParamDepend as DevParam).m_DevParamInfo.m_ParamType = (ParamMain as DevParam).m_DevParamInfo.m_ParamType;
                (ParamDepend as DevParam).m_DevParamInfo.m_InitValue = (ParamMain as DevParam).m_DevParamInfo.m_InitValue;
                (ParamDepend as DevParam).m_DevParamInfo.m_Length = (ParamMain as DevParam).m_DevParamInfo.m_Length;
                (ParamDepend as DevParam).m_DevParamInfo.m_Min = (ParamMain as DevParam).m_DevParamInfo.m_Min;
                (ParamDepend as DevParam).m_DevParamInfo.m_Max = (ParamMain as DevParam).m_DevParamInfo.m_Max;
                (ParamDepend as DevParam).m_DevParamInfo.m_PortType = (ParamMain as DevParam).m_DevParamInfo.m_PortType;
                (ParamDepend as DevParam).m_DevParamInfo.m_Address = (ParamMain as DevParam).m_DevParamInfo.m_Address;
                (ParamDepend as DevParam).m_DevParamInfo.m_Description = (ParamMain as DevParam).m_DevParamInfo.m_Description;
                (ParamDepend as DevParam).m_Representation = (ParamMain as DevParam).m_Representation;
                switch (ParamMain.ParamInfo.ParamType)
                {
                    case ParamType.Integer:
                        (ParamDepend as IIntParam).IntValue = (ParamMain as IIntParam).IntValue;
                        break;
                    case ParamType.Boolean:
                        (ParamDepend as IBooleanParam).BoolValue = (ParamMain as IBooleanParam).BoolValue;
                        break;
                    case ParamType.String:
                        (ParamDepend as IStringParam).StrValue = (ParamMain as IStringParam).StrValue;
                        break;
                    case ParamType.Enum:
                        (ParamDepend as IEnumParam).EnumIntValue = (ParamMain as IEnumParam).EnumIntValue;
                        break;
                    case ParamType.Float:
                        (ParamDepend as IFloatParam).FloatValue = (ParamMain as IFloatParam).FloatValue;
                        break;
                    case ParamType.Short:
                        (ParamDepend as IShortParam).ShortValue = (ParamMain as IShortParam).ShortValue;
                        break;
                    case ParamType.Byte:
                        (ParamDepend as IByteParam).ByteValue = (ParamMain as IByteParam).ByteValue;
                        break;
                    case ParamType.ByteArray:
                        (ParamDepend as IByteArrayParam).ArrayValue = (ParamMain as IByteArrayParam).ArrayValue;
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 모니터링 함수
        private void MonitoringStart()
        {
            try
            {
                DevParam MonitoringCopyParam = null;
                IParam RecvParam = null;

                List<Int32> tmp = new List<Int32>();

                while (this.m_IsThreadExit)
                {
                    //모니터링 중지이면 대기
                    if (!this.m_IsMonitoringStart)
                        continue;

                    //연결된 상태에서만 실행
                    if (PublicVar.MainWnd.ViewModel.IsConnect)
                    {
                        //모니터링 시작 버튼이 눌린경우
                        if (this.m_IsMonitoringStart)
                        {
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();

                            PublicVar.MainWnd.Dispatcher.Invoke((Action)delegate ()
                            {
                                if (this.m_MonitoringParam != null)
                                {
                                    //현재 모니터링 파라미터 복사본을 가져온다.
                                    MonitoringCopyParam = (DevParam)this.m_MonitoringParam.Clone();

                                    //데이터 읽기
                                    for (int i = 0; i < MonitoringCopyParam.Children.Count; i++)
                                    {
                                        AccessMode Mode = MonitoringCopyParam.Children[i].AccessMode;
                                        ParamRepresentation representation = MonitoringCopyParam.Children[i].Representation;
                                        RecvParam = this.CurrentDevice.Read(MonitoringCopyParam.Children[i]);
                                        if (RecvParam == null)
                                            throw new Exception("Failed to read data");
                                        (RecvParam as DevParam).m_DevParamInfo = MonitoringCopyParam.Children[i].ParamInfo as DevParamInfo;
                                        MonitoringCopyParam.Children[i].AccessMode = AccessMode.ReadWrite;
                                        MonitoringCopyParam.Children[i] = RecvParam;
                                        MonitoringCopyParam.Children[i].AccessMode = Mode;
                                    }
                                }
                            });

                            if (MonitoringCopyParam != null)
                            {
                                MonitoringUpdateEventArgs monitoringEvent = new MonitoringUpdateEventArgs(this.MonitoringView, MonitoringCopyParam);
                                MonitoringUpdateEvent(this, monitoringEvent);
                                Thread.Sleep((Int32)(this.m_MonitoringTime * 1000));
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                PublicVar.MainWnd.Dispatcher.Invoke((Action)delegate ()
                {
                    this.IsMonitoringStart = false;
                    PublicVar.MainWnd.Process_Connect(false);
                });

                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));

                //쓰레드 종료 루틴
                this.m_IsThreadExit = false;
                if (this.m_MonitoringThread != null)
                {
                    if (!this.m_MonitoringThread.Join(1000))
                    {
                        this.m_MonitoringThread.Abort();
                        this.m_MonitoringThread = null;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 설정한 시간이 경과하면 이쪽으로 분기되서 에러상태를 읽는다.
        /// </summary>
        private void StateViewUpdate(Object sender, EventArgs e)
        {
            try
            {
                //보드 상태 값 가져오기
                SetErrorStateInformation();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                (this.m_Wnd as MainWindow).Process_Connect(false);
                this.m_Timer.Stop();
            }
        }

        /// <summary>
        /// 에러 상태 정보를 가져와서 UI에 업데이트 해주는 함수
        /// </summary>
        public void SetErrorStateInformation()
        {
            try
            {
                //에러 상태를 받을 정보
                DevParam ErrStateParam = new DevParam("ErrorState", ParamType.Short, null, "에러 상태 값",
                    String.Empty, PortType.None, 0, 0, 0x1270, 2);

                DevParam MOSIStateParam = new DevParam("MOSI", ParamType.Short, null, "모터 상태 값",
                    String.Empty, PortType.None, 0, 0, 0x12C1, 2);

                //에러 상태 수신
                IParam ErrorCodeParam = this.CurrentDevice.Read(ErrStateParam);
                if (ErrorCodeParam == null)
                    throw new Exception("Failed to read data");
                Int16 errCode = (ErrorCodeParam as IShortParam).ShortValue;

                //모터 상태 수신
                IParam MOSIParam = this.CurrentDevice.Read(MOSIStateParam);
                if (MOSIParam == null)
                    throw new Exception("Failed to read data");
                Int16 MOSICode = (MOSIParam as IShortParam).ShortValue;

                //에러코드와 메세지 UI 업데이트
                this.ErrorCode = "0x" + errCode.ToString("X2");
                Object Message = Enum.ToObject(typeof(ErrorCodeType), errCode);
                this.ErrorMessage = Message.ToString();

                //모터 상태 UI 업데이트
                this.MotorStateCode = "0x" + MOSICode.ToString("X2");
                Message = Enum.ToObject(typeof(MOSICodeType), MOSICode);
                this.MotorStateMessage = Message.ToString();

                if (errCode.Equals(0x00))
                    this.ErrorDescription = "정상 동작";
                else
                {
                    //에러코드로부터 Description 구해서 UI 업데이트 진행
                    foreach (String key in this.FaultConfigList.Keys)
                    {
                        String target = key.Split(' ')[0].Substring(2, 2);
                        if (target.Contains(errCode.ToString()))
                        {
                            this.ErrorDescription = this.FaultConfigList[key];
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<summary>
        /// 에러 로그 만드는 함수
        /// </summary>
        public void Log_Maker(Object sender, LogExecuteEventArgs e)
        {
            try
            {
                //로그 정보를 담을 객체 생성
                LogInformation info = new LogInformation(m_Wnd as MainWindow);
                info.InitializeViewModel();

                //로그 상태와 메세지 데이터를 넣어준다.
                info.LogStatus = e.State;
                info.Message = e.Message;
                info.LogTime = e.Time;

                if (App.Current != null)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate () 
                    {
                        //로그를 추가하고 스크롤 위치를 조정한다.
                        this.LogList.Add(info);

                        ObservableCollection<LogInformation> tmpList = new ObservableCollection<LogInformation>();
                        var items = LogList.GroupBy(a => a.Message).Select(grp => grp.Last());
                        foreach (LogInformation newInfo in items)
                            tmpList.Add(newInfo);
                        this.LogList = tmpList;

                        tmpList = new ObservableCollection<LogInformation>();
                        items = this.LogList.OrderBy(a => a.LogTime);
                        foreach (LogInformation newInfo in items)
                            tmpList.Add(newInfo);
                        this.LogList = tmpList;
                        Console.WriteLine();

                        ScrollEvent(info, e);
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetInitParamDatas(Boolean IsConnect=false)
        {
            try
            {
                this.BoardBaseInfoList.Clear();

                BoardBaseInfo info = new BoardBaseInfo();
                info.InitializeViewModel();
                info.Name = "Device Name";
                IParam ContentParam = this.CurrentDevice.MyModel.GetParamNode("Configuration").GetParamNode("파라미터 정보").GetParamNode("Device Name");
                DevParam DeviceNameParam = this.CurrentDevice.Read(ContentParam) as DevParam;
                info.Value = (IsConnect) ? (String)DeviceNameParam.Value : String.Empty;
                this.BoardBaseInfoList.Add(info);

                info = new BoardBaseInfo();
                info.InitializeViewModel();
                info.Name = "H/W Version";
                ContentParam = this.CurrentDevice.MyModel.GetParamNode("Configuration").GetParamNode("파라미터 정보").GetParamNode("H/W Version");
                DevParam HWVersionParam = this.CurrentDevice.Read(ContentParam) as DevParam;
                info.Value = (IsConnect) ? (String)HWVersionParam.Value : String.Empty;
                this.BoardBaseInfoList.Add(info);

                info = new BoardBaseInfo();
                info.InitializeViewModel();
                info.Name = "S/W Version";
                ContentParam = this.CurrentDevice.MyModel.GetParamNode("Configuration").GetParamNode("파라미터 정보").GetParamNode("S/W Version");
                DevParam SWVersionParam = this.CurrentDevice.Read(ContentParam) as DevParam;
                info.Value = (IsConnect) ? (String)SWVersionParam.Value : String.Empty;
                this.BoardBaseInfoList.Add(info);

                info = new BoardBaseInfo();
                info.InitializeViewModel();
                info.Name = "Alias ID";
                ContentParam = this.CurrentDevice.MyModel.GetParamNode("Configuration").GetParamNode("파라미터 정보").GetParamNode("Alias ID");
                DevParam AliasIDParam = this.CurrentDevice.Read(ContentParam) as DevParam;
                info.Value = (IsConnect) ? (AliasIDParam as IShortParam).ShortValue.ToString() : String.Empty;
                this.BoardBaseInfoList.Add(info);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class InitPageLine : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public InitPageLine() { }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_LightColor = Brushes.White;
        }

        private Brush m_LightColor;
        public Brush LightColor
        {
            get { return this.m_LightColor; }
            set
            {
                this.m_LightColor = value;
                this.NotifyPropertyChanged("LightColor");
            }
        }
    }

    public class BoardLine : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public BoardLine(){ }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_LightColor = Brushes.Silver;
        }

        private Brush m_LightColor;
        public Brush LightColor
        {
            get { return this.m_LightColor; }
            set
            {
                this.m_LightColor = value;
                this.NotifyPropertyChanged("LightColor");
            }
        }
    }

    public class BoardBaseInfo : ViewModel
    {
        public override void InitializeViewModel()
        {
            this.m_Name = String.Empty;
            this.m_Value = String.Empty;
        }

        private String m_Name;
        public String Name
        {
            get { return this.m_Name; }
            set
            {
                this.m_Name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        private String m_Value;
        public String Value
        {
            get { return this.m_Value; }
            set
            {
                this.m_Value = value;
                this.NotifyPropertyChanged("Value");
            }
        }
    }
}
