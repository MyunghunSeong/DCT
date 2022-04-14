using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MotionGuidePro.Main;
using System.Windows.Input;

namespace CrevisLibrary
{
    // 파라미터 정보
    public class DevParamInfo : IParamInfo
    {
        public String m_ParamName;
        public String ParamName
        {
            get
            {
                return this.m_ParamName;
            }
        }

        public ParamType m_ParamType;
        public ParamType ParamType
        {
            get
            {
                return this.m_ParamType;
            }
        }

        // 파라미터 설명	
        public String m_Description;
        ///<summary>파라미터 설명</summary>
        public String Description
        {
            get { return this.m_Description; }
        }

        // 제어기는 Address, length 기반이므로 필요
        public Int32 m_Address;
        public Int32 Address
        {
            get
            {
                return this.m_Address;
            }
        }

        // 제어기는 Address, length 기반이므로 필요
        public Int32 m_Length;
        public Int32 Length
        {
            get
            {
                return this.m_Length;
            }
        }

        // 어디로 통신 할지
        public PortType m_PortType;
        public PortType PortType
        {
            get
            {
                return this.m_PortType;
            }
        }

        // NA를 위한거 SlotIndex :-1 면 Device로 통신 아니면 Slot으로 통신
        public Int32 m_SlotIndex;
        public Int32 SlotIndex
        {
            get
            {
                return this.m_SlotIndex;
            }
        }

        // 데이터 단위 값
        public String m_Unit;
        public String Unit
        {
            get
            {
                return this.m_Unit;
            }
        }

        // 데이터 기본 값
        public String m_InitValue;
        public String InitValue
        {
            get
            {
                return this.m_InitValue;
            }
        }

        // Max
        public Double m_Max;
        public Double Max
        {
            get
            {
                return this.m_Max;
            }
        }

        // Min
        public Double m_Min;
        public Double Min
        {
            get
            {
                return this.m_Min;
            }
        }

        //Default
        public Boolean m_Default;
        public Boolean Default
        {
            get { return this.m_Default; }
            set { this.m_Default = value; }
        }


        public DevParamInfo(String ParamName, ParamType ParamType, String Description, Int32 Address, Int32 Length, PortType PortType, Int32 SlotIndex,
            String Unit="", String InitValue="", Double Max=0.0d, Double Min=0.0d, Boolean Default=true)
        {
            this.m_ParamName = ParamName;
            this.m_ParamType = ParamType;
            this.m_Description = Description;
            this.m_Address = Address;
            this.m_Length = Length;
            this.m_PortType = PortType;
            this.m_SlotIndex = SlotIndex;
            this.m_Unit = Unit;
            this.m_InitValue = InitValue;
            this.m_Max = Max;
            this.m_Min = Min;
            this.m_Default = Default;
        }
    }

    // 범용 파라미터 클래스
    public class DevParam : IParam, IIntParam, IEnumParam, IBooleanParam, IFloatParam, IStringParam, IByteParam, 
        IShortParam, IByteArrayParam, IFileSelectParam, ITextModifyParam, INotifyPropertyChanged
    {
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        private Visibility m_Visible;
        public Visibility Visible
        {
            get { return this.m_Visible; }
            set
            {
                this.m_Visible = value;
                this.NotifyPropertyChanged("Visible");
            }
        }

        private Visibility m_BlockVisible;
        public Visibility BlockVisible
        {
            get { return this.m_BlockVisible; }
            set
            {
                this.m_BlockVisible = value;
                this.NotifyPropertyChanged("BlockVisible");
            }
        }

        private Visibility m_BoxVisible;
        public Visibility BoxVisible
        {
            get { return this.m_BoxVisible; }
            set
            {
                this.m_BoxVisible = value;
                this.NotifyPropertyChanged("BoxVisible");
            }
        }


        private IDevice m_MyDevice;
        public IDevice MyDevice
        {
            get { return this.m_MyDevice; }
            set { this.m_MyDevice = value; }
        }

        private DevParam m_Parent;
        public IParam Parent
        {
            get
            {
                return this.m_Parent as IParam;
            }
            set
            {
                this.m_Parent = value as DevParam;
            }
        }

        //통신 완료 BrushColor
        private Brush m_CommEndBrush;
        public Brush CommEndBrush
        {
            get
            {
                return this.m_CommEndBrush;
            }
            set
            {
                this.m_CommEndBrush = value;
                NotifyPropertyChanged("CommEndBrush");
            }
        }

        // IsChecked
        private Boolean m_IsChecked;
        public Boolean IsChecked
        {
            get
            {
                return this.m_IsChecked;
            }
            set
            {
                this.m_IsChecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }

        // IsFocuse
        private Boolean m_IsFocuse;
        public Boolean IsFocuse
        {
            get
            {
                return this.m_IsFocuse;
            }
            set
            {
                this.m_IsFocuse = value;
                NotifyPropertyChanged("IsFocuse");
            }
        }

        // 파라미터에대한 속성
        public DevParamInfo m_DevParamInfo;
        public IParamInfo ParamInfo
        {
            get
            {
                return this.m_DevParamInfo as IParamInfo;
            }
        }

        // 파라미터 값
        public Object m_Value;
        public Object Value
        {
            get
            {
                return this.m_Value;
            }
            set
            {
                if(this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.m_Value = value;

                // 파라미터 변경시 CallBack
                if (this.DependencyParamUpdate != null)
                {
                    if (this.DependencyParams.Count > 0)
                    {
                        // 종속된 파라미터 있으면 전부 호추
                        foreach (var param in this.DependencyParams)
                        {
                            if (this.DependencyParamUpdate != null)
                                this.DependencyParamUpdate(this, param);
                        }
                    }
                    else
                    {
                        // 종속된 파라미터 없으면 업데이트 함수 만 호출
                        this.DependencyParamUpdate(this, null);
                    }
                }
            }
        }

        public AccessMode m_AccessMode;
        public AccessMode AccessMode
        {
            get
            {
                return this.m_AccessMode;
            }
            set
            {
                this.m_AccessMode = value;
                this.NotifyPropertyChanged("AccessMode");
                this.NotifyPropertyChanged("IsEnable");
            }
        }

        // 파라미터 보호
        public Boolean m_IsProtected;
        public Boolean IsProtected
        {
            get
            {
                return this.m_IsProtected;
            }
            set
            {
                this.m_IsProtected = value;
                this.NotifyPropertyChanged("IsProtected");
            }
        }

        // 표시 방법
        public ParamRepresentation m_Representation;
        public ParamRepresentation Representation
        {
            get
            {
                return this.m_Representation;
            }
            set
            {
                this.m_Representation = value;
                this.NotifyPropertyChanged("ParamRepresentation");
            }
        }

        // 컨트롤 활성화/비활성화
        public Boolean IsEnable
        {
            get
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    return false;
                else
                    return true;
            }
        }


        // 종속 파라미터
        public List<IParam> m_DependencyParams;
        /// <summary>종속 파라미터</summary>
        public List<IParam> DependencyParams 
        {
            get
            {
                return this.m_DependencyParams;
            }
        }

        /// <summary>
        /// 파라미터 변경되면 호출
        /// </summary>
        public DependencyCallBack DependencyParamUpdate { get; set; }

        public Int32 IntValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Integer)
                    throw (new Exception(this.ParamPath + "는 ParamType.Integer 형식이 아닙니다."));
                return (this.m_Value == null) ?  0 : (Int32)this.m_Value; 
            }
            set
            {
                try
                {
                    if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                        throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                    this.Value = value;
                    this.NotifyPropertyChanged("IntValue");
                }
                catch (Exception err)
                {
                    if (this.LogEvent != null)
                        this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                }
            }
        }

        public Int32 m_Min;
        public Int32 Min
        {
            get
            {
                return this.m_Min;
            }
            set
            {
                this.m_Min = value;
                this.NotifyPropertyChanged("Min");
            }
        }

        public Int32 m_Max;
        public Int32 Max 
        {
            get
            {
                return this.m_Max;
            }
            set
            {
                this.m_Max = value;
                this.NotifyPropertyChanged("Max");
            }
        }

        public Int32 m_Inc;
        public Int32 Inc 
        {
            get
            {
                return this.m_Inc;
            }
            set
            {
                this.m_Inc = value;
                this.NotifyPropertyChanged("Inc");
            }
        }

        // Float
        public Single FloatValue 
        { 
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Float)
                    throw (new Exception(this.ParamPath + "는 ParamType.Float 형식이 아닙니다."));

                return (this.m_Value == null) ? 0 : (Single)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("FloatValue");
            }
        }

        // String
        public String StrValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.String)
                    throw (new Exception(this.ParamPath + "는 ParamType.String 형식이 아닙니다."));

                foreach (KeyValuePair<String, String> dic in (PublicVar.MainWnd.DataContext as MainWindow_ViewModel).FaultConfigList)
                {
                    String key = dic.Key;
                    if ((this.m_Value as String).Trim('\0').Equals(key))
                        this.m_DevParamInfo.m_Description = dic.Value;
                }

                return (this.m_Value == null) ? String.Empty : (String)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("StrValue");
            }
        }

        // FileSelectValue
        public String FileSelectValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.FileSelect)
                    throw (new Exception(this.ParamPath + "는 ParamType.FileSelect 형식이 아닙니다."));

                return (this.m_Value == null) ? String.Empty : (String)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("FileSelectValue");
            }
        }

        // TextModifyValue
        public String TextModifyValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.TextModify)
                    throw (new Exception(this.ParamPath + "는 ParamType.TextModify 형식이 아닙니다."));

                return (this.m_Value == null) ? String.Empty : (String)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("TextModifyValue");
            }
        }

        // Command
        public Int32 CommandValue
        {
            set
            {
                this.Value = value;
                // 필요하면 함수도 등록 
                this.NotifyPropertyChanged("CommandValue");
            }
        }

        // Entry 값만 허용
        public Boolean m_IsEntryValueOnly;
        public Boolean IsEntryValueOnly 
        {
            get
            {
                return this.m_IsEntryValueOnly;
            }
        }

        // Enum Parameter 위한 
        public Int32 EnumIntValue 
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Enum)
                    throw (new Exception(this.ParamPath + "는 ParamType.Enum 형식이 아닙니다."));

                if (this.Value.GetType().Equals(typeof(Int32)))
                    return (this.m_Value == null) ? 0 : (Int32)this.m_Value;
                else
                    return GetEnumEntryIntValue((String)this.Value); 
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                //Enum 변경됐을 때 통신
                if (this.MyDevice != null)
                    this.MyDevice.Write(this);

                this.Value = value;
                this.NotifyPropertyChanged("EnumIntValue");
            }
        }

        //  선택된 DiplayName 
        public String EnumStrValue 
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Enum)
                    throw (new Exception(this.ParamPath + "는 ParamType.Enum 형식이 아닙니다."));

                if (this.Value.GetType().Equals(typeof(Int32)))
                    return GetEnumEntryStrValue((Int32)this.Value);
                else
                    return (this.m_Value == null) ? String.Empty : (String)this.m_Value;
            }
            set
            {
                if (value == null)
                    value = String.Empty;

                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.TextBoxString = value;

                if (this.Value.Equals(value))
                    return;

                this.Value = value;
                this.NotifyPropertyChanged("EnumStrValue");
            }
        }

        // Enum리스트
        private IList<EnumEntry> m_Entries;
        public IList<EnumEntry> Entries
        {
            get
            {
                return this.m_Entries;
            }
        }
        
        // 콤보 박스을 위한 
        private CollectionView m_EnumEntries;
        public CollectionView EnumEntries 
        {
            get
            {
                return this.m_EnumEntries;
            }
        }
        
        // Boolean
        public Boolean BoolValue 
        { 
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Boolean)
                    throw (new Exception(this.ParamPath + "는 ParamType.Boolean 형식이 아닙니다."));

                return (this.m_Value == null) ? false : (Boolean)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                //Boolean값이 변경되면 통신하기
                if(this.MyDevice != null)
                    this.MyDevice.Write(this);

                this.Value = value;
                this.NotifyPropertyChanged("BoolValue");
            }
        }

        // Byte
        public Byte ByteValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Byte)
                    throw (new Exception(this.ParamPath + "는 ParamType.Byte 형식이 아닙니다."));

                return (this.m_Value == null) ? (Byte)0 : Convert.ToByte(this.m_Value);
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("ByteValue");
            }
        }

        // Int16
        public Int16 ShortValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.Short)
                    throw (new Exception(this.ParamPath + "는 ParamType.Short 형식이 아닙니다."));

                return (this.m_Value == null) ? (Int16)0 : (Int16)this.m_Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("ShortValue");
            }
        }

        // ByteArray
        public Byte[] ArrayValue
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.ByteArray)
                    throw (new Exception(this.ParamPath + "는 ParamType.Short 형식이 아닙니다."));

                return (this.m_Value == null) ? new Byte[0] : (Byte[])this.Value;
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "는 AccessMode.ReadOnly 입니다."));

                this.Value = value;
                this.NotifyPropertyChanged("ArrayValue");
                this.NotifyPropertyChanged("ArrayValueString");
            }
        }

        // ByteArray
        public String ArrayValueString
        {
            get
            {
                if (this.ParamInfo.ParamType != CrevisLibrary.ParamType.ByteArray)
                    throw (new Exception(this.ParamPath + "는 ParamType.Short 형식이 아닙니다."));

                if (this.m_Value == null)
                    return String.Empty;

                if (this.Representation == ParamRepresentation.HexNumber)
                {
                    return BitConverter.ToString(this.ArrayValue).Replace("-", " ");
                }
                else
                    return String.Join(" ", this.ArrayValue);
            }
            set
            {
                if (this.AccessMode == CrevisLibrary.AccessMode.ReadOnly)
                    throw (new Exception(this.ParamPath + "is AccessMode.ReadOnly"));

                if(value == null)
                {
                    this.m_Value = value;
                }
                else
                {
                    // 크기 체크
                    String[] SplitVal = value.Split(' ');

                    if (SplitVal.Length != this.ParamInfo.Length) 
                        throw new Exception("Not match Length of " + this.ParamInfo.ParamName);

                    // m_Value 할당 체크
                    if (this.m_Value == null)
                        this.m_Value = new Byte[this.ParamInfo.Length];

                    // 데이터 입력
                    for (Int32 i = 0; i < SplitVal.Length; i++)
                    {
                        if (this.Representation == ParamRepresentation.HexNumber)
                            this.ArrayValue[i] = Convert.ToByte(SplitVal[i], 16);
                        else
                            this.ArrayValue[i] = Convert.ToByte(SplitVal[i]);
                    }
                }
                
                this.NotifyPropertyChanged("ArrayValueString");
                this.NotifyPropertyChanged("ArrayValue");
            }
        }


        // 하위 파라미터
        private ObservableCollection<IParam> m_Children;
        public ObservableCollection<IParam> Children
        {
            get
            {
                return this.m_Children;
            }
        }

        // 파라미터 위치
        public String ParamPath
        {
            get
            {
                return GetPath();
            }
        }

        
        // 파라미터 검색용
        private List<IParam> m_SearchParams;

        // 프로퍼티 이벤트
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        // 생성자
        public DevParam()
        {
            this.m_Visible = Visibility.Visible;
            this.m_DevParamInfo = new DevParamInfo(String.Empty, ParamType.Category, String.Empty, 0, 0, PortType.None, -1);
            this.m_Value = null;
            this.AccessMode = AccessMode.ReadWrite;
            this.m_IsProtected = false;
            this.m_Representation = ParamRepresentation.PureNumber;
            this.m_DependencyParams = new List<IParam>();
            this.m_Children = new ObservableCollection<IParam>();
            this.m_SearchParams = new List<IParam>();
            this.m_IsEntryValueOnly = true;
            this.m_Entries = new List<EnumEntry>();
            this.m_EnumEntries = new CollectionView(this.m_Entries);
            this.m_Min = Int32.MinValue;
            this.m_Max = Int32.MaxValue;
            this.m_Inc = 1;
            this.m_IsChecked = true;
            this.m_CommEndBrush = Brushes.Transparent;

            // shseol85
            this.m_searchList = new ObservableCollection<String>();
            this.m_TextBoxString = String.Empty;
        }

        public DevParam(String ParamName, ParamType ParamType, Object Value, String Description, String InitValue="",
            PortType PortType = PortType.None, Double Min=0.0d, Double Max=0.0d, 
            Int32 Address = 0, Int32 Length = 0)
            : this()
        {
            this.m_BlockVisible = Visibility.Visible;
            this.m_BoxVisible = Visibility.Collapsed;

            this.m_DevParamInfo.m_ParamName = ParamName;
            this.m_DevParamInfo.m_ParamType = ParamType;
            this.m_DevParamInfo.m_Description = Description;
            this.m_DevParamInfo.m_Address = Address;
            this.m_DevParamInfo.m_Length = Length;
            this.m_DevParamInfo.m_InitValue = InitValue;
            this.m_DevParamInfo.m_Min = Min;
            this.m_DevParamInfo.m_Max = Max;
            this.m_DevParamInfo.m_PortType = PortType;
            this.m_Value = Value;

            // 포트타입이 Enum이면 this.m_TextBoxString 업데이트 해줘야 화면에 표시됨
            if (ParamType.Equals(ParamType.Enum))
            {
                this.m_TextBoxString = (this.m_Value == null) ? String.Empty : this.EnumStrValue;
            }
         
            // 통신하는 파라미터는 길이 필수 입력이니깐 길이 체크 패스
            if (PortType != PortType.None)
                return;

            // 기본값으로 생성할때
            switch (ParamType)
            {
                case ParamType.Category:
                    break;
                case ParamType.Integer:
                    this.m_DevParamInfo.m_Length = 4;
                    break;
                case ParamType.Float:
                    this.m_DevParamInfo.m_Length = 4;
                    break;
                case ParamType.String:
                    if (Value != null)
                       this.m_DevParamInfo.m_Length = Value.ToString().Length;
                    break;
                case ParamType.Command:
                    this.m_DevParamInfo.m_Length = 4;
                    break;
                case ParamType.Enum:
                    //  Enum  리스트가 1개일때
                    // 초기값이 "" 이면 0번째 인덱스를 기본으로 하기 때문에 콤보 박스에서 선택을 못함
                    // 따라서 기본값을 0으로 바꿔줘야함.
                    if (Value is String)
                    {
                        if (Value.Equals(String.Empty))
                        {
                            this.m_Value = 0;
                        }
                    }
                    this.m_DevParamInfo.m_Length = 4;
                    break;
                case ParamType.Boolean:
                    this.m_DevParamInfo.m_Length = 1;
                    break;
                case ParamType.Byte:
                    this.m_DevParamInfo.m_Length = 1;
                    break;
                case ParamType.Short:
                    this.m_DevParamInfo.m_Length = 2;
                    break;
                case ParamType.ByteArray:
                    if (Value != null)
                        this.m_DevParamInfo.m_Length = ((Byte[])Value).Length;
                    break;
            }
        }

        // 하위 파라미터 있는지?
        public Boolean HasChildren()
        {
            if (this.Children.Count > 0)
                return true;
            else
                return false;
        }

        // Binding 할때 표시되는 이름
        public override String ToString()
        {
            return this.m_DevParamInfo.ParamName;
        }

        // 클론
        public Object Clone()
        {
            try
            {
                DevParam clone = new DevParam(  this.ParamInfo.ParamName,
                                                this.ParamInfo.ParamType,
                                                null,
                                                this.ParamInfo.Description,
                                                this.ParamInfo.InitValue,
                                                this.ParamInfo.PortType,
                                                this.ParamInfo.Min,
                                                this.ParamInfo.Max,
                                                this.ParamInfo.Address,
                                                this.ParamInfo.Length);
                
                // 값 복사 되게
                if (this.ParamInfo.ParamType.Equals(ParamType.ByteArray))
                {
                    if(this.ArrayValue != null)
                        clone.m_Value = this.ArrayValue.ToArray();
                }
                else
                    clone.m_Value = this.m_Value;

                //ParamType이 Enum일 경우 EnumStrValue를 복사
                if (this.m_DevParamInfo.ParamType.Equals(ParamType.Enum))
                    clone.EnumStrValue = this.EnumStrValue;

                //this.Value
                clone.m_DevParamInfo.m_SlotIndex = this.ParamInfo.SlotIndex;
                clone.m_DevParamInfo.m_Default = this.ParamInfo.Default;
                clone.m_DevParamInfo.m_InitValue = this.ParamInfo.InitValue;
                clone.m_DevParamInfo.m_Min = this.ParamInfo.Min;
                clone.m_DevParamInfo.m_Max = this.ParamInfo.Max;
                clone.m_DevParamInfo.m_ParamName = this.ParamInfo.ParamName;
                clone.m_DevParamInfo.m_ParamType = this.ParamInfo.ParamType;
                clone.m_DevParamInfo.m_PortType = this.ParamInfo.PortType;
                clone.m_DevParamInfo.m_Length = this.ParamInfo.Length;
                clone.m_DevParamInfo.m_Address = this.ParamInfo.Address;
                clone.m_DevParamInfo.m_Unit = this.m_DevParamInfo.m_Unit;

                clone.IsChecked = this.IsChecked;
                clone.IsProtected = this.IsProtected;
                clone.AccessMode = this.AccessMode;
                clone.Representation = this.Representation;
                clone.m_IsEntryValueOnly = this.m_IsEntryValueOnly;


                // clone.m_Children;  
                foreach (var child in this.m_Children)
                {
                    clone.AddChild(child.Clone() as IParam);
                }

                //clone.m_SearchParams; 할꺼 없구
                
                // clone.m_Entries; 
                foreach (var entry in this.m_Entries)
                {
                    clone.AddEnumEntry(entry.EnumValue, entry.DisplayName);
                }

                //clone.m_EnumEntries; clone.m_Entries 만들면 자동

                clone.Min = this.Min;
                clone.Max = this.Max;
                clone.Inc = this.Inc;

                return clone as Object;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파리미터의 위지
        private String GetPath()
        {
            try
            {
                String Path = String.Empty;
                if(this.m_Parent != null)
                {
                    Path = this.m_Parent.GetPath() + "." + this.ParamInfo.ParamName;
                }
                else
                {
                    Path = this.ParamInfo.ParamName;
                }

                return Path;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파라미터 추가
        public void AddChild(IParam param)
        {
            param.Parent = this;
            this.Children.Add(param);
        }

        //파라미터 제거
        public void RemoveChild(IParam param)
        {
            param.Parent = this;
            this.Children.Remove(param);
        }

        // 파라미터 이름으로 IParam 받기
        public List<IParam> GetParamNodes(String ParamName)
        {
            try
            {
                // 기존꺼 날리고
                this.m_SearchParams.Clear();

                // 내꺼 기준으로 추가
                AddParamNode(this, ParamName);
                
                // 검색된거 넘겨
                return this.m_SearchParams;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파라미터 이름으로 첫번째 검색된 IParam 받기 
        public IParam GetParamNode(String ParamName)
        {
            try
            {
                // 이름으로 검색
                IParam Find = FindParam(this, ParamName);
               
                // 검색된거 넘겨
                return Find;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파라미터만(카테고리 제외) 받기
        public List<IParam> GetParamNodes()
        {
            try
            {
                // 기존꺼 날리고
                this.m_SearchParams.Clear();

                // 내꺼 기준으로 추가
                AddParamNodeExceptCategory(this);

                // 검색된거 넘겨
                return this.m_SearchParams;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<summary>Root 가져오기</summary>
        public IParam GetRootParam()
        {
            try
            {
                return GetRootParam(this);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IParam GetRootParam(IParam Param)
        {
            try
            {
                if (Param.Parent == null)
                    return Param;
                else
                    return GetRootParam(Param.Parent);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void AddParamNode(IParam Param, String ParamName)
        {
            try
            {
                // 내꺼 확인
                if (Param.ParamInfo.ParamName.Equals(ParamName))
                {
                    this.m_SearchParams.Add(Param);
                }

                // 하위 검색
                foreach (var clield in Param.Children)
                {
                    AddParamNode(clield, ParamName);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 카테고리는 빼고 추가
        private void AddParamNodeExceptCategory(IParam Param)
        {
            try
            {
                // 내꺼 확인
                if (Param.ParamInfo.ParamType != ParamType.Category)
                {
                    this.m_SearchParams.Add(Param);
                }

                // 하위 검색
                foreach (var clield in Param.Children)
                {
                    AddParamNodeExceptCategory(clield);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IParam FindParam(IParam Param, String ParamName)
        {
            try
            {
                // 내꺼 확인
                if (Param.ParamInfo.ParamName.Equals(ParamName))
                {
                    return Param;
                }
                
                // 하위 검색
                IParam Find = null;
                foreach (var clield in Param.Children)
                {
                    Find = FindParam(clield, ParamName);
                    if (Find != null)
                        break;
                }
                return Find;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Enum 데이터 추가
        public void AddEnumEntry(Int32 Value, String DisplayName)
        {
            try
            {
                if(this.ParamInfo.ParamType != ParamType.Enum)
                    throw new Exception("AddEnumEntry() is supported only ParamType.Enum");

                this.m_Entries.Add(new EnumEntry(Value, DisplayName));

            }
            catch (Exception)
            {
                throw;
            }
        }

        // Int32 값에 해당하는 String값 
        public String GetEnumEntryStrValue(Int32 Value)
        {
            try
            {
                String StrValue = String.Empty;

                foreach (var Entry in this.m_Entries)
                {
                    if (Entry.EnumValue == Value)
                    {
                        StrValue = Entry.DisplayName;
                        break;
                    }
                }

                return StrValue;
            }
            catch (Exception)
            {
                throw;
            }
        }


        // String 값에 해당하는 Int32 
        public Int32 GetEnumEntryIntValue(String Value)
        {
            try
            {
                Int32 IntValue = 0;

                foreach (var Entry in this.m_Entries)
                {
                    if (Entry.DisplayName.Equals(Value))
                    {
                        IntValue = Entry.EnumValue;
                        break;
                    }
                }

                return IntValue;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private System.Windows.Input.ICommand m_ExcuteButton;
        /// <summary>
        /// 텍스트 박스 마우스 클릭 이벤트
        /// </summary>
        public System.Windows.Input.ICommand ExcuteButton
        {
            get
            {
                if (this.m_ExcuteButton == null)
                {
                    this.m_ExcuteButton = new RelayCommand(Excute_ExcuteButton, CanExcute_ExcuteButton);
                }

                return this.m_ExcuteButton;
            }
            set
            {
                this.m_ExcuteButton = value;
            }
        }

        public void Excute_ExcuteButton(Object obj)
        {
            try
            {
                DevParam devparam = obj as DevParam;

                this.CommandValue = Convert.ToInt32(devparam.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool CanExcute_ExcuteButton(Object obj)
        {
            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// shseol85 popupcontrol
        /// /////////////////////////////////////////////////////////
        
        
        private Boolean m_PopupIsOpen = false;
        /// <summary>
        /// 핍업 컨트롤 닫기/열기
        /// </summary>
        public Boolean PopupIsOpen
        {
            get
            {
                return this.m_PopupIsOpen;
            }
            set
            {
                this.m_PopupIsOpen = value;
                if (this.m_PopupIsOpen == false)
                {
                    this.ListViewFocus = false;
                }
                NotifyPropertyChanged("PopupIsOpen");
            }
        }

        
        private Boolean m_ListViewFocus;
        /// <summary>
        /// 팝업 안에 있는 리스트뷰 포커스 설정
        /// </summary>
        public Boolean ListViewFocus
        {
            get
            {
                return this.m_ListViewFocus;
            }
            set
            {
                this.m_ListViewFocus = value;
                NotifyPropertyChanged("ListViewFocus");
            }
        }

        private String m_ListViewSelectedItem;
        /// <summary>
        /// 리스트뷰 셀렉티드 아이템 
        /// </summary>
        public String ListViewSelectedItem
        {
            get
            {
                return this.m_ListViewSelectedItem;
            }
            set
            {
                if (value == null)
                {
                    NotifyPropertyChanged("ListViewSelectedItem");
                    return;
                }

                this.m_ListViewSelectedItem = value;
                NotifyPropertyChanged("ListViewSelectedItem");
            }
        }

        // 텍스트 박스 텍스트
        private String m_TextBoxString;
        /// <summary>
        /// 텍스트박스 문자열
        /// </summary>
        public String TextBoxString
        {
            get
            {
                return this.m_TextBoxString;
            }
            set
            {
                if (this.m_TextBoxString.Equals(value))
                    return;

                this.m_TextBoxString = value;
                NotifyPropertyChanged("TextBoxString");


                if (this.PopupIsOpen)
                {
                    // 팝업 열려 있으면 리스트 업데이트
                    UpdateSearchList();
                }
                else
                {
                    // EnumStrValue 업데이트
                    if (this.ParamInfo.ParamType == ParamType.Enum)
                        this.EnumStrValue = value;
                }
            }
        }

        
        private ObservableCollection<String> m_searchList;
        /// <summary>
        /// 검색 된 문자열 표시 될 리스트
        /// </summary>
        public ObservableCollection<String> SearchList
        {
            get
            {
                return this.m_searchList;
            }
            set
            {
                this.m_searchList = value;
                this.NotifyPropertyChanged("SearchList");
            }
        }

        private System.Windows.Input.ICommand m_PreviewMouseUpCommand;
        /// <summary>
        /// 텍스트 박스 마우스 클릭 이벤트
        /// </summary>
        public System.Windows.Input.ICommand PreviewMouseUpCommand
        {
            get
            {
                if (this.m_PreviewMouseUpCommand == null)
                {
                    this.m_PreviewMouseUpCommand = new RelayCommand(Excute_PreviewMouseUpCommand, CanExcute_PreviewMouseUpCommand);
                }

                return this.m_PreviewMouseUpCommand;
            }
            set
            {
                this.m_PreviewMouseUpCommand = value;

            }
        }

        /// <summary>
        /// 텍스트 박스 마우스 클릭 이벤트 : 팝업 오픈, 리스트뷰 업데이트
        /// </summary>
        public void Excute_PreviewMouseUpCommand(Object obj)
        {
            try
            {
                this.PopupIsOpen = true;
                this.ListViewFocus = false;

                UpdateSearchList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool CanExcute_PreviewMouseUpCommand(Object obj)
        {
            return true;
        }


        private System.Windows.Input.ICommand m_LostFocusCommand;
        /// <summary>
        /// 텍스트박스 포커스 잃어버렷을 때 이벤트 (현재 안쓰임)
        /// </summary>
        public System.Windows.Input.ICommand LostFocusCommand
        {
            get
            {
                if (this.m_LostFocusCommand == null)
                {
                    this.m_LostFocusCommand = new RelayCommand(Excute_LostFocusCommandCommand, CanExcute_LostFocusCommandCommand);
                }

                return this.m_LostFocusCommand;
            }
            set
            {
                this.m_LostFocusCommand = value;

            }
        }

        /// <summary>
        /// 텍스트박스 포커스 잃어버렷을 때 이벤트 (현재 안쓰임)
        /// </summary>
        public void Excute_LostFocusCommandCommand(Object obj)
        {
            try
            {
                // Enum파라미터일때 값 업데이트 위해
                // this.PopupIsOpen = false;
                // this.m_ListViewFocus = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CanExcute_LostFocusCommandCommand(Object obj)
        {
            return true;
        }


        private System.Windows.Input.ICommand m_CmdTextBoxKeyDown;
        /// <summary>
        /// 텍스트박스 키보드 다운 이벤트 
        /// </summary>
        public System.Windows.Input.ICommand CmdTextBoxKeyDown
        {
            get
            {
                if (this.m_CmdTextBoxKeyDown == null)
                {
                    this.m_CmdTextBoxKeyDown = new RelayCommand(Excute_CmdTextBoxKeyDown, CanExcute_CmdTextBoxKeyDown);
                }
                return this.m_CmdTextBoxKeyDown;
            }
            set
            {
                this.m_CmdTextBoxKeyDown = value;
            }
        }

        /// <summary>
        /// 텍스트박스 키보드 다운 이벤트 : 리스트뷰 포커스 설정
        /// </summary>
        public void Excute_CmdTextBoxKeyDown(Object obj)
        {
            try
            {
                this.ListViewFocus = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CanExcute_CmdTextBoxKeyDown(Object obj)
        {
            return true;
        }

        
        private System.Windows.Input.ICommand m_CmdTextBoxKeyEnter;
        /// <summary>
        /// 텍스트박스에서 엔터 이벤트
        /// </summary>
        public System.Windows.Input.ICommand CmdTextBoxKeyEnter
        {
            get
            {
                if (this.m_CmdTextBoxKeyEnter == null)
                {
                    this.m_CmdTextBoxKeyEnter = new RelayCommand(Excute_CmdTextBoxKeyEnter, CanExcute_CmdTextBoxKeyEnter);
                }
                return this.m_CmdTextBoxKeyEnter;
            }
            set
            {
                this.m_CmdTextBoxKeyEnter = value;
            }
        }

        /// <summary>
        /// 텍스트박스에서 엔터 이벤트 : 팝업 닫기
        /// </summary>
        public void Excute_CmdTextBoxKeyEnter(Object obj)
        {
            try
            {
                // 텍스트 검사
                CheckText();

                this.ListViewSelectedItem = this.TextBoxString;

                // Popup 닫기
                this.PopupIsOpen = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool CanExcute_CmdTextBoxKeyEnter(Object obj)
        {
            return true;
        }

        private System.Windows.Input.ICommand m_CmdListViewKeyDownEnter;
        /// <summary>
        /// 리스트뷰에서 엔터 이벤트
        /// </summary>
        public System.Windows.Input.ICommand CmdListViewKeyDownEnter
        {
            get
            {
                if (this.m_CmdListViewKeyDownEnter == null)
                {
                    this.m_CmdListViewKeyDownEnter = new RelayCommand(Excute_CmdListViewKeyDownEnter, CanExcute_CmdListViewKeyDownEnter);
                }
                return this.m_CmdListViewKeyDownEnter;
            }
            set
            {
                this.m_CmdListViewKeyDownEnter = value;
            }
        }

        /// <summary>
        /// 리스트뷰에서 엔터 이벤트 : 텍스트박스 문자열 대입, 팝업닫기, 리스트뷰 포커스 설정
        /// </summary>
        public void Excute_CmdListViewKeyDownEnter(Object obj)
        {
            try
            {
                if (this.TextBoxString == null)
                    return;

                // Popup 닫기
                this.PopupIsOpen = false;
                this.ListViewFocus = false;

                // 텍스트 바꿔
                this.TextBoxString = this.ListViewSelectedItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool CanExcute_CmdListViewKeyDownEnter(Object obj)
        {
            return true;
        }

        private System.Windows.Input.ICommand m_ListViewPreviewMouseUpCommand;
        /// <summary>
        /// 리스트뷰에서 마우스 업 이벤트 
        /// </summary>
        public System.Windows.Input.ICommand ListViewPreviewMouseUpCommand
        {
            get
            {
                if (this.m_ListViewPreviewMouseUpCommand == null)
                {
                    this.m_ListViewPreviewMouseUpCommand = new RelayCommand(Excute_ListViewPreviewMouseUpCommand, CanExcute_ListViewPreviewMouseUpCommand);
                }

                return this.m_ListViewPreviewMouseUpCommand;
            }
            set
            {
                this.m_ListViewPreviewMouseUpCommand = value;

            }
        }

        /// <summary>
        /// 리스트뷰에서 마우스 업 이벤트 : 텍스트박스 문자열 대입, 팝업 닫기, 리스트뷰 포커스 설정
        /// </summary>
        public void Excute_ListViewPreviewMouseUpCommand(Object obj)
        {
            try
            {
                this.PopupIsOpen = false;
                this.ListViewFocus = false;

                this.TextBoxString = this.ListViewSelectedItem;
            }
            catch (Exception ex)
            {
                String str = ex.Message;
            }
        }
        public bool CanExcute_ListViewPreviewMouseUpCommand(Object obj)
        {
            return true;
        }

        /// <summary>
        /// 리스트뷰 검색된 리스트 업데이트
        /// </summary>
        public void UpdateSearchList()
        {
            try
            {
                if (this.Entries == null || this.TextBoxString == null)
                    return;

                // SearchList 지워주고 시작!
                this.SearchList.Clear();

                // 무엇을 입력했나?
                String InputText = this.TextBoxString;

                // viewModel.SearchList에 추가
                if (InputText == String.Empty)
                {
                    // 다 넣어
                    foreach (EnumEntry item in this.Entries)
                    {
                        this.SearchList.Add(item.DisplayName);
                    }
                }
                else
                {
                    // 몇개만 넣어
                    foreach (EnumEntry item in this.Entries)
                    {
                        if (item.DisplayName.ToLower().Contains(InputText.ToLower()))
                            this.SearchList.Add(item.DisplayName);
                        else
                            continue;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CheckText()
        {
            try
            {
                if (this.Entries == null)
                    return;
                
                if (this.IsEntryValueOnly == true) // 허용 X : EnumEntry에 있는 거만 
                {
                    // 검색된 문자열이 EnumEntry 항목에 없다면 첫번째거
                    if (IsStringInLists(this.TextBoxString) == false)
                    {
                        this.TextBoxString = this.Entries[0].DisplayName;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 현재 텍스트박스가 EnumEntry에 항목이 있는지 확인하는 함수
        /// </summary>
        public Boolean IsStringInLists(String str)
        {
            try
            {
                Boolean ret = false;
                // str과 같은거 찾기
                foreach (EnumEntry item in this.Entries)
                {
                    if (item.DisplayName.Equals(str))
                    {
                        ret = true;
                    }
                }
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Byte Array TextBox Mask
        /// </summary>
        public String ByteArrayMask
        {
            get
            {
                String strMask = String.Empty;
                for (int i = 0; i < this.ParamInfo.Length; i++)
                {
                    if (i == this.ParamInfo.Length - 1)
                    {
                        strMask += "AA";
                        break;
                    }
                    strMask += "AA ";
                }
                return strMask;
            }            
        }

        /// //////////////////////////////////////////////////////////
        /// shseol85 popupcontrol
        /// //////////////////////////////////////////////////////////


    }//end class

    /// <summary>
    /// shseol85 : 리스트뷰 포커스 설정해주는 클랙스
    /// </summary>
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
                if (d is TextBox)
                    (d as TextBox).SelectAll();
            }
        }
    }

}// end namesapce
