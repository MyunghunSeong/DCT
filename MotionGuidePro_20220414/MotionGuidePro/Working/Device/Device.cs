using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MotionGuidePro.Main;

namespace CrevisLibrary
{
    public class Device : IDevice
    {
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        //디바이스 기본 정보
        private DevParam m_InitParam;
        public DevParam InitParam
        {
            get { return this.m_InitParam; }
            set { this.m_InitParam = value; }
        }

        private Dictionary<String, IContent> m_ContentList;
        public Dictionary<String, IContent> ContentList
        {
            get { return this.m_ContentList; }
            set { this.m_ContentList = value; }
        }

        private String m_Name;
        public String Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        private String m_SvrAddress;
        public String SvrAddress
        {
            get { return this.m_SvrAddress; }
            set { this.m_SvrAddress = value; }
        }

        private String m_MyAddress;
        public String MyAddress
        {
            get { return this.m_MyAddress; }
            set { this.m_MyAddress = value; }
        }

        private Double m_Timeout;
        public Double Timeout
        {
            get { return this.m_Timeout; }
            set { this.m_Timeout = value; }
        }

        private Double m_MonitoringTime;
        public Double MonitoringTime
        {
            get { return this.m_MonitoringTime; }
            set { this.m_MonitoringTime = value; }
        }

        private Double m_ErrorStatePeriod;
        public Double ErrorStatePeriod
        {
            get { return this.m_ErrorStatePeriod; }
            set { this.m_ErrorStatePeriod = value; }
        }

        private Int32 m_Port;
        public Int32 Port
        {
            get { return this.m_Port; }
            set { this.m_Port = value; }
        }

        private DevParam m_MyModel;
        public DevParam MyModel
        {
            get { return this.m_MyModel; }
            set { this.m_MyModel = value; }
        }

        public ModBusCommunication CommObj { get; set; }

        #region 그래프 관련 변수
        //패킷데이터를 담을 큐 버퍼
        private Queue<Byte[]> m_PacketQueue;
        public Queue<Byte[]> PacketQueue
        {
            get { return this.m_PacketQueue; }
            set { this.m_PacketQueue = value; }
        }

        //패킷 추가 관련 이벤트 관리 핸들러
        private AutoResetEvent m_NewPacketEvent;
        public AutoResetEvent NewPacketEvent
        {
            get { return this.m_NewPacketEvent; }
            set { this.m_NewPacketEvent = value; }
        }

        //시그널 업데이트 관련 이벤트 관리 핸들러
        private AutoResetEvent m_UpdatePacketEvent;
        public AutoResetEvent UpdatePacketEvent
        {
            get { return this.m_UpdatePacketEvent; }
            set { this.m_UpdatePacketEvent = value; }
        }

        //패킷 추가 Lock
        private Object m_PacketQueueLock;
        public Object PacketQueueLock
        {
            get { return this.m_PacketQueueLock; }
            set { this.m_PacketQueueLock = value; }
        }

        //시그널 추가 Lock
        private Object m_SignalLock;
        public Object SignalLock
        {
            get { return this.m_SignalLock; }
            set { this.m_SignalLock = value; }
        }
        #endregion

        public Device()
        {
            this.m_Name = String.Empty;
            this.m_SvrAddress = String.Empty;
            this.m_MyAddress = String.Empty;
            this.m_Port = 0;
            this.m_ContentList = new Dictionary<String, IContent>();
            this.CommObj = new ModBusUdp(this);
            this.m_MyModel = null;
            this.m_InitParam = null;
            this.m_PacketQueue = new Queue<Byte[]>();
            this.m_PacketQueueLock = new Object();
            this.m_SignalLock = new Object();
            this.m_NewPacketEvent = new AutoResetEvent(true);
            this.m_UpdatePacketEvent = new AutoResetEvent(false);
        }

        public Device(String Name, String IPAddress, Int32 Port = 0) : this()
        {
            this.m_Name = Name;
            this.m_SvrAddress = IPAddress;
            this.m_Port = Port;
        }

        public void Close()
        {
            try
            {
                // 통신 해제
                this.CommObj.ModBus_Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Load(String FilePath)
        {
            try
            {
                //파일 경로에 있는 정보를 로드 후 리턴한다.
                this.m_MyModel = XmlParser.LoadParam(FilePath);
                if (this.m_MyModel == null)
                    throw new Exception("File is not Exist.");
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        public Boolean Open()
        {
            try
            {
                // 통신 연결
                return this.CommObj.ModBus_Open(this.SvrAddress, this.MyAddress);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IParam Read(IParam Param)
        {
            try
            {
                //연결 되지 않았으면 null을 리턴
                if (!PublicVar.MainWnd.ViewModel.IsConnect)
                    return null;

                Boolean IsReadEnd = this.CommObj.ModBus_Read(Param.ParamInfo.Address, Param.ParamInfo.Length);
                if (!IsReadEnd)
                    return null;

                DevParam RecvParam = new DevParam("Receive Parameter", Param.ParamInfo.ParamType, null, String.Empty);
                RecvParam.m_DevParamInfo.m_ParamName = Param.ParamInfo.ParamName;
                Byte[] tmpBuffer = new Byte[Param.ParamInfo.Length];
                switch (Param.ParamInfo.ParamType)
                {
                    case ParamType.Boolean:
                        Boolean boolData = new Boolean();
                        boolData = BitConverter.ToBoolean(this.CommObj.RecvData, 0);
                        (RecvParam as IBooleanParam).BoolValue = boolData;
                        break;
                    case ParamType.Byte:
                        Byte byteData = new Byte();
                        byteData = this.CommObj.RecvData[0];
                        (RecvParam as IByteParam).ByteValue = byteData;
                        break;
                    case ParamType.ByteArray:
                        Byte[] ArrayData = this.CommObj.RecvData;
                        (RecvParam as IByteArrayParam).ArrayValue = ArrayData;
                        break;
                    case ParamType.Enum:
                        Int32 enumValue = 0;
                        Array.Copy(this.CommObj.RecvData, 0, tmpBuffer, 0, tmpBuffer.Length);
                        if (tmpBuffer.Length.Equals(1))
                            enumValue = Convert.ToInt32(tmpBuffer[0]);
                        else if (tmpBuffer.Length.Equals(2))
                            enumValue = BitConverter.ToInt16(tmpBuffer, 0);
                        else
                            enumValue = BitConverter.ToInt32(tmpBuffer, 0);
                        foreach (EnumEntry entry in (Param as IEnumParam).Entries)
                            RecvParam.Entries.Add(entry);
                        (RecvParam as IEnumParam).EnumIntValue = enumValue;
                        break;
                    case ParamType.Float:
                        Array.Copy(this.CommObj.RecvData, 0, tmpBuffer, 0, tmpBuffer.Length);
                        Single floatValue = BitConverter.ToSingle(tmpBuffer, 0);
                        (RecvParam as IFloatParam).FloatValue = floatValue;
                        break;
                    case ParamType.Integer:
                        Array.Copy(this.CommObj.RecvData, 0, tmpBuffer, 0, tmpBuffer.Length);
                        //Array.Reverse(tmpBuffer);
                        Int32 IntValue = BitConverter.ToInt32(tmpBuffer, 0);
                        (RecvParam as IIntParam).IntValue = IntValue;
                        break;
                    case ParamType.Short:
                        Array.Copy(this.CommObj.RecvData, 0, tmpBuffer, 0, tmpBuffer.Length);
                        Int16 ShortValue = BitConverter.ToInt16(tmpBuffer, 0);
                        (RecvParam as IShortParam).ShortValue = ShortValue;
                        break;
                    case ParamType.String:
                        Array.Copy(this.CommObj.RecvData, 0, tmpBuffer, 0, tmpBuffer.Length);
                        (this.CommObj as ModBusUdp).SwapPacket(1, ref tmpBuffer);
                        //Array.Reverse(tmpBuffer);
                        String StrValue = Encoding.Default.GetString(tmpBuffer);
                        //BitConverter.ToString(tmpBuffer, 0).Replace("-", "");
                        (RecvParam as IStringParam).StrValue = StrValue;
                        break;
                    default:
                        throw new Exception(Param.ParamInfo.ParamType.ToString() + "은 지원하지 않는 타입입니다.");
                }
 
                return RecvParam;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Save(IParam Param, String FilePath)
        {
            try
            {
                if (Param != null)
                {
                    this.SetDeviceParam(Param);
                    XmlParser.SaveParam(Param as DevParam, FilePath);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Write(IParam Param)
        {
            try
            {
                Byte[] tmpBuffer = new Byte[Param.ParamInfo.Length];
                switch (Param.ParamInfo.ParamType)
                {
                    case ParamType.Boolean:
                        Byte[] boolBuffer = BitConverter.GetBytes((Param as IBooleanParam).BoolValue);
                        Array.Copy(boolBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.Byte:
                        tmpBuffer[0] = (Param as IByteParam).ByteValue;
                        break;
                    case ParamType.ByteArray:
                        Byte[] arrayBuffer = (Param as IByteArrayParam).ArrayValue;
                        Array.Copy(arrayBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.Enum:
                        Byte[] enumBuffer = BitConverter.GetBytes((Param as IEnumParam).EnumIntValue);
                        Array.Copy(enumBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.Float:
                        Byte[] floatBuffer = BitConverter.GetBytes((Param as IFloatParam).FloatValue);
                        Array.Copy(floatBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.Integer:
                        Byte[] intBuffer = BitConverter.GetBytes((Param as IIntParam).IntValue);
                        Array.Copy(intBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.Short:
                        Byte[] shortBuffer = BitConverter.GetBytes((Param as IShortParam).ShortValue);
                        Array.Copy(shortBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    case ParamType.String:
                        Byte[] strBuffer = Encoding.Default.GetBytes((Param as IStringParam).StrValue);
                        Array.Copy(strBuffer, 0, tmpBuffer, 0, tmpBuffer.Length);
                        break;
                    default:
                        break;
                }

                // 데이터 전송
                this.CommObj.ModBus_Write(Param.ParamInfo.Address, Param.ParamInfo.Length, tmpBuffer);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetDeviceParam(IParam Param)
        {
            try
            {
                //디바이스 이름
                (Param.GetParamNode("디바이스 이름") as IStringParam).StrValue = this.Name;

                //Server IP
                (Param.GetParamNode("Server IP") as IStringParam).StrValue = this.SvrAddress;

                //Client IP
                (Param.GetParamNode("Client IP") as IStringParam).StrValue = this.MyAddress;

                //포트
                (Param.GetParamNode("Port") as IIntParam).IntValue = this.Port;

                //모니터링 업데이트 시간
                Int32 len = 0;
                if (this.MonitoringTime.ToString().Contains('.'))
                    len = ((Single)this.MonitoringTime).ToString().Split('.')[1].Length;
                (Param.GetParamNode("MonitoringTime") as IFloatParam).FloatValue = (Single)this.MonitoringTime;

                //에러 상태 주기
                (Param.GetParamNode("ErrorStatePeriod") as IIntParam).IntValue = (Int32)this.ErrorStatePeriod;

                //Timeout
                (Param.GetParamNode("Timeout") as IFloatParam).FloatValue = (Single)this.Timeout;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
