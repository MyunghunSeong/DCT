using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO.Ports;
using System.Windows;
using MotionGuidePro.Main;

namespace CrevisLibrary
{
    public enum ErrItem
    {
        ERROR_FUNCTIONCODE = 0x01,
        ERROR_ADDRESS = 0x02,
        ERROR_DATAVALUE = 0x03,
        ERROR_SERVERFAIL = 0x04,
        ERROR_RECEIVETIMEOUT = 0x05,
        ERROR_SERVERBUSY = 0x06,
        ERROR_GATEWAY_TYPE_1 = 0x0A,
        ERROR_GATEWAY_TYPE_2 = 0x0B
    };

    public abstract class ModBusCommunication
    {
        /// <summary>
        /// 필드
        /// </summary>
        protected Byte[] m_packetData;
        protected Byte[] m_recvData;
        protected String m_errMsg;
        protected Object m_functionLock;
        protected AutoResetEvent m_connectDone;
        protected AutoResetEvent m_disConnectDone;
        protected AutoResetEvent m_recvDone;
        protected AutoResetEvent m_sendDone;

        /// <summary>
        /// 생성자
        /// </summary>
        public ModBusCommunication()
        {
            this.m_packetData = null;
            this.m_recvData = null;
            this.m_functionLock = new Object();
            this.m_connectDone = new AutoResetEvent(false);
            this.m_disConnectDone = new AutoResetEvent(false);
            this.m_recvDone = new AutoResetEvent(false);
            this.m_sendDone = new AutoResetEvent(false);
        }

        /// <summary>
        /// 프로포티
        /// </summary>
        //에러 메세지 : 지금은 사용하지 않음
        public String ErrMessage
        {
            get { return this.m_errMsg; }
            set { this.m_errMsg = value; }
        }

        //수신 메시지
        public Byte[] RecvData
        {
            get { return this.m_recvData; }
            set { this.m_recvData = value; }
        }

        public Byte[] WholePacket
        {
            get { return this.m_packetData; }
            set { this.m_packetData = value; }
        }

        protected void inner_ErrCheck(Byte code, Byte errType)
        {
            ErrItem errItem = new ErrItem();
            errItem = (ErrItem)Enum.ToObject(typeof(ErrItem), errType);

            try
            {
                switch (code)
                {
                    case 0x84:
                    case 0x90:
                        switch (errItem)
                        {
                            case ErrItem.ERROR_FUNCTIONCODE:
                                throw new Exception(String.Format("Invalid FunctionCode code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_ADDRESS:
                                throw new Exception(String.Format("Invalid Address. code : {0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_DATAVALUE:
                                throw new Exception(String.Format("Invalid Data Value. code : {0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_SERVERFAIL:
                                throw new Exception(String.Format("The server is not responding. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_RECEIVETIMEOUT:
                                throw new Exception(String.Format("Failed to receive Packet. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_SERVERBUSY:
                                throw new Exception(String.Format("The server is not working properly. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_GATEWAY_TYPE_1:
                                throw new Exception(String.Format("Gateway is not usable. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            case ErrItem.ERROR_GATEWAY_TYPE_2:
                                throw new Exception(String.Format("Module is not responding. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                            default:
                                throw new Exception(String.Format("There's no such type. code : 0x{0} errType : {1}", code.ToString("X"), errItem));
                        }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 추상 메소드
        /// </summary>
        abstract public Boolean ModBus_Open(String boardIP, String localIP);
        abstract public void ModBus_Close();
        abstract public Boolean ModBus_Read(Int32 address, Int32 length, Int32 slaveId = 1);
        abstract public Boolean ModBus_Write(Int32 address, Int32 length, Byte[] writeBuffer, Int32 slaveId = 1);
    }

    public enum ModBusUdpEvent
    {
        Error = 0,
        Warring = 1,
        Abort = 2,
        Done = 3,
        Notify = 4
    }

    public class ModBusCommunicationEventArgs : EventArgs
    {
        public ModBusUdpEvent Type { get; set; }
        public String Msg { get; set; }
        public ModBusCommunicationEventArgs(ModBusUdpEvent Type, String Msg)
        {
            this.Type = Type;
            this.Msg = Msg;
        }
    }

    public class ModBusUdp : ModBusCommunication
    {
        ///<summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        /// <summary>
        /// 필드
        /// </summary>
        private UdpClient m_CommClient;
        private IPEndPoint m_CommRemotePt;
        private Int32 m_port;
        private UdpClient m_StreamClient;
        public Thread m_GraphReceiveThread;
        public AutoResetEvent m_WriteEvent;
        public Boolean IsOpenArea = false;

        //오픈 테스트중인지 아닌지
        public Boolean IsOpenTest { get; set; }

        public Boolean IsCommErrorExecute { get; set; }

        public Boolean AlreadyOpened { get; set; }

        //Device객체
        public Device MyDevice { get; set; }

        public OscilloscopeContent OscilloscopeObj { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public ModBusUdp(Device Device)
        {
            this.m_WriteEvent = new AutoResetEvent(false);
            this.MyDevice = Device;
            this.m_port = 502;
            this.m_CommRemotePt = null;
            this.m_CommClient = null;
            this.m_StreamClient = null;
            this.m_GraphReceiveThread = null;
            this.IsCommErrorExecute = false;
            this.AlreadyOpened = false;
            this.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
        }

        public void OnGraphDataReceive()
        {
            try
            {
                this.MyDevice.PacketQueue.Clear();

                if (this.OscilloscopeObj != null)
                {
                    while (!this.OscilloscopeObj.IsExitThread)
                    {
                        try
                        {
                            //데이터 수신
                            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                            Byte[] recvBuffer = this.m_StreamClient.Receive(ref remoteEP);

                            //True : Stop
                            if (this.OscilloscopeObj.IsOscilloCommCheck)
                                continue;

                            lock (this.MyDevice.PacketQueueLock)
                            {
                                //받은 패킷을 큐에 저장
                                this.MyDevice.PacketQueue.Enqueue(recvBuffer);
                            }

                            //이벤트 Set()
                            this.MyDevice.NewPacketEvent.Set();
                        }
                        catch { }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }


        public override Boolean ModBus_Open(String ip, String localIP)
        {
            try
            {
                this.IsOpenArea = true;
                Boolean WriteEnd = true;
                Double PreTimeOutValue = PublicVar.MainWnd.ViewModel.Timeout;

                this.OscilloscopeObj = this.MyDevice.ContentList["Oscilloscope"] as OscilloscopeContent;

                this.m_CommRemotePt = new IPEndPoint(IPAddress.Parse(ip), 502);
                this.m_CommClient = new UdpClient();

                //오실로스코프 데이터를 받을 켓을 하나 더 생성한다.
                IPEndPoint OsciiloAddress = new IPEndPoint(IPAddress.Parse(localIP), 0);
                this.m_StreamClient = new UdpClient(OsciiloAddress);
                this.m_StreamClient.Client.ReceiveTimeout = 50;
                this.m_port = ((IPEndPoint)this.m_StreamClient.Client.LocalEndPoint).Port;

                String[] ipArr = localIP.Split('.');
                Byte[] HighIPBuffer = new Byte[2] { Convert.ToByte(ipArr[1]), Convert.ToByte(ipArr[0]) };
                Byte[] LowIPBuffer = new Byte[2] { Convert.ToByte(ipArr[3]), Convert.ToByte(ipArr[2]) };
                Byte[] portBuffer = new Byte[2] { (Byte)(this.m_port), (Byte)(this.m_port >> 8) };

                if (CheckConnect())
                {
                    //PublicVar.MainWnd.ViewModel.Timeout = 1;
                    //통신을 위한 정보를 전달한다.(IP, 포트, 그래프 셋팅 옵션 등)
                    this.ModBus_Write(0x1111, HighIPBuffer.Length, HighIPBuffer);
                    WriteEnd ^= this.IsCommErrorExecute;
                    this.ModBus_Write(0x1112, LowIPBuffer.Length, LowIPBuffer);
                    WriteEnd ^= this.IsCommErrorExecute;
                    this.ModBus_Write(0x1113, portBuffer.Length, portBuffer);
                    WriteEnd ^= this.IsCommErrorExecute;

                    this.AlreadyOpened = (this.IsCommErrorExecute) ? false : true;
                    PublicVar.MainWnd.ViewModel.Timeout = PreTimeOutValue;
                }
                else
                    this.AlreadyOpened = false;


                /*
                if (test)
                {
                    if (this.OscilloscopeObj != null)
                    {
                        this.OscilloscopeObj.IsExitThread = false;

                        //그래프 데이터 수신 쓰레드 실행
                        this.OscilloscopeObj.GraphReceiveThread = new Thread(() => this.OnGraphDataReceive());
                        this.OscilloscopeObj.GraphReceiveThread.Start();

                        //파서 쓰레드 실행
                        this.OscilloscopeObj.PacketParserThread = new Thread(() => this.OscilloscopeObj.PacketParse());
                        this.OscilloscopeObj.PacketParserThread.Start();

                        //화면 업데이트 쓰레드 실행
                        this.OscilloscopeObj.UpdateSignalThread = new Thread(() => this.OscilloscopeObj.UpdateSignal());
                        this.OscilloscopeObj.UpdateSignalThread.Start();
                    }
                }
                */

                return this.AlreadyOpened;
            }
            catch (Exception err)
            {
                if (this.m_CommClient != null)
                {
                    this.m_CommClient.Dispose();
                    this.m_CommClient.Close();
                }

                String errMsg = (err is SocketException) ? "Invalid Server Address" : err.Message;

                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, errMsg, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                return false;
            }
            finally
            {
                this.IsOpenArea = false;
            }
        }

        /// <summary>
        /// 연결 확인
        /// </summary>
        private Boolean CheckConnect()
        {
            try
            {
                this.IsOpenTest = true;
                for (int i = 0; i < 3; i++)
                {
                    //0x1209번지 Read (DeviceName)
                    this.ModBus_Read(0x1209, 8);

                    if (this.IsCommErrorExecute)
                        Thread.Sleep(1000);
                    else
                        break;
                }

                if (this.IsCommErrorExecute)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                this.IsOpenTest = false;
            }
        }

        public override void ModBus_Close()
        {
            try
            {
                /*
                this.OscilloscopeObj.IsExitThread = true;
                if (this.OscilloscopeObj != null)
                {
                    if (this.OscilloscopeObj.GraphReceiveThread != null)
                    {
                        //쓰레드 종료
                        if (!this.OscilloscopeObj.GraphReceiveThread.Join(1000))
                        {
                            this.OscilloscopeObj.GraphReceiveThread.Abort();
                            this.OscilloscopeObj.GraphReceiveThread = null;
                        }
                    }
                }

                if (this.OscilloscopeObj != null)
                {
                    if (this.OscilloscopeObj.PacketParserThread != null)
                    {
                        //쓰레드 종료
                        if (!this.OscilloscopeObj.PacketParserThread.Join(1000))
                        {
                            this.OscilloscopeObj.PacketParserThread.Abort();
                            this.OscilloscopeObj.PacketParserThread = null;
                        }
                    }

                    if (this.OscilloscopeObj.UpdateSignalThread != null)
                    {
                        //쓰레드 종료
                        if (!this.OscilloscopeObj.UpdateSignalThread.Join(1000))
                        {
                            this.OscilloscopeObj.UpdateSignalThread.Abort();
                            this.OscilloscopeObj.UpdateSignalThread = null;
                        }
                    }
                }
                */

                //통신 클라이언트 종료
                if (this.m_CommClient != null)
                {
                    this.m_CommClient.Dispose();
                    this.m_CommClient.Close();
                }

                //오실로스코프 데이터 수신용 클라이언트 종료
                if (this.m_StreamClient != null)
                {
                    this.m_StreamClient.Dispose();
                    this.m_StreamClient.Close();
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
            finally
            {
                this.AlreadyOpened = false;
            }
        }

        public override Boolean ModBus_Read(Int32 address, Int32 length, Int32 slaveId=1)
        {
            try
            {
                //1. ModBus패킷 만들기
                inner_MakeModBusPacket(address, length);

                //2. 데이터 전송
                lock (this.m_functionLock)
                    this.m_CommClient.Send(m_packetData, m_packetData.Length, this.m_CommRemotePt);

                //5. 데이터 수신 대기
                ReceiveMessages(this.m_CommClient, true);

                return true;
            }
            catch (Exception err)
            {
                if (!this.IsOpenTest)
                {
                    if (this.LogEvent != null)
                        this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                }
                return false;
            }
        }

        public struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }

        private void ReceiveMessages(UdpClient client, Boolean IsRead=false)
        {
            try
            {
                this.IsCommErrorExecute = false;

                // Receive a message and write it to the console.
                IPEndPoint e = new IPEndPoint(IPAddress.Any, 0);

                UdpState s = new UdpState();
                s.e = e;
                s.u = client;

                String ErrMsg = (IsRead) ? "Read Timeout Error!" : "Write Timeout Error!";

                client.BeginReceive(new AsyncCallback(ReceiveCallBack), s);
                //(PublicVar.MainWnd.ViewModel).Timeout * 1000)
                if (!this.m_recvDone.WaitOne(100))
                    throw new Exception(ErrMsg);
            }
            catch (Exception)
            {
                this.IsCommErrorExecute = true;
                throw;
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            Byte[] rxBuffer = null;

            try
            {
                UdpClient u = ((UdpState)(ar.AsyncState)).u;
                IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

                Byte[] readBytes = u.EndReceive(ar, ref e);
                Int32 readByteSize = readBytes.Length;

                if (readByteSize <= 0)
                    return;
                else
                {
                    rxBuffer = new Byte[readByteSize];
                    Array.Copy(readBytes, rxBuffer, readByteSize);
                    Byte functionCode = rxBuffer[7];
                    Byte errType = rxBuffer[8];

                    Boolean isFunctionCode = inner_CheckFuncCode(functionCode);
                    if (!isFunctionCode)
                    {
                        inner_ErrCheck(functionCode, errType);
                    }

                    Boolean isRead = inner_CheckIsRead(functionCode);
                    if (isRead)
                    {
                        Int32 BufferSize = rxBuffer.Length;
                        this.m_recvData = new Byte[rxBuffer[8]];
                        Array.Copy(rxBuffer, 9, this.m_recvData, 0, rxBuffer[8]);
                        SwapPacket(1, ref this.m_recvData);
                    }
                }

                this.m_recvDone.Set();
            }
            catch (Exception err)
            {
                this.m_recvDone.Set();

                if (err is ObjectDisposedException)
                    return;

                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
            finally
            {
                rxBuffer = null;
            }
        }

        public void SwapPacket(int swapUnit, ref Byte[] arr)
        {
            Byte temp = 0;
            int totLoop = (int)(arr.Count() / swapUnit) / 2;
            int totCount = swapUnit;
            int Lcnt = swapUnit;
            int Ccnt = (swapUnit == 1) ? 2 : 1;
            try
            {
                int jump = 0;
                for (int loop = 0; loop < totLoop; loop++)
                {
                    for (int count = 0; count < totCount; count++)
                    {
                        temp = arr[(jump + count)];
                        arr[(jump + count)] = arr[(jump + count) + swapUnit];
                        arr[(jump + count) + swapUnit] = temp;

                    }
                    jump += Lcnt * 2;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private Boolean inner_CheckFuncCode(Byte funcCode)
        {
            switch (funcCode)
            {
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x08:
                case 0x0F:
                case 0x10:
                    return true;
                default:
                    return false;
            }
        }

        private Boolean inner_CheckIsRead(Byte funcCode)
        {
            try
            {
                switch (funcCode)
                {
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                        return true;
                    case 0x05:
                    case 0x06:
                    case 0x08:
                    case 0x0F:
                    case 0x10:
                        return false;
                    default:
                        throw new Exception(String.Format("FunctionCode to not define : {0}", funcCode));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void inner_MakeModBusPacket(Int32 address, Int32 length, Byte[] data = null, Boolean isWrite = false)
        {
            try
            {
                //1. ModBus패킷 데이터 만들기
                Byte[] dataPacket = inner_MakeModBusDataPacket(data, length, isWrite);

                this.m_packetData = new Byte[dataPacket.Length + 10];

                //2. Fix헤더 값 입력
                this.m_packetData[0] = 0x00; this.m_packetData[1] = 0x01; //Transaction Identifier
                this.m_packetData[2] = 0x00; this.m_packetData[3] = 0x00; //Protocol Identifier
                this.m_packetData[6] = 0x01; //The Unit Identifier

                //4. length 패킷 생성
                Byte[] lenByte = BitConverter.GetBytes(Convert.ToUInt16(dataPacket.Length + 4));
                Array.Reverse(lenByte);
                Array.Copy(lenByte, 0, this.m_packetData, 4, lenByte.Length);

                //3. Address 변환
                Byte[] addrByte = BitConverter.GetBytes((Int16)address);
                Array.Reverse(addrByte); //Swap문제 해결
                Array.Copy(addrByte, 0, this.m_packetData, 8, addrByte.Length); //해더에서 Address데이터 자리에 복사

                //4. FunctionCode 변환
                Byte functionByte = (isWrite) ? (Byte)0x10 : (Byte)0x04;
                this.m_packetData[7] = functionByte;

                Array.Copy(dataPacket, 0, this.m_packetData, 10, dataPacket.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Byte[] inner_MakeModBusDataPacket(Byte[] data, Int32 length, Boolean isWrite)
        {
            Byte[] dataPacket = null;

            try
            {
                if (isWrite)
                {
                    UInt16 ByteLen = Convert.ToUInt16(length);
                    UInt16 wordLen = 0;

                    //1. Byte -> Word로 맞춰주기 위한 알고리즘
                    if ((ByteLen % 2) == 1)
                    {
                        wordLen = Convert.ToUInt16((ByteLen / 2) + 1);
                        ByteLen += 1;
                    }
                    else
                        wordLen = Convert.ToUInt16(ByteLen / 2);

                    //2. 데이터 패킷 할당[3Byte(길이) + 데이터]
                    dataPacket = new Byte[3 + ByteLen];

                    //3. Word 길이 패킷 생성
                    Byte[] wordLenPacket = BitConverter.GetBytes(wordLen);
                    Array.Reverse(wordLenPacket);

                    //4. Byte 길이 패킷 생성
                    Byte ByteLenPacket = Convert.ToByte(ByteLen);

                    //5. 보낼 데이터 패킷 생성
                    Byte[] writeDataPacket = new Byte[ByteLen];
                    Array.Copy(data, 0, writeDataPacket, 0, data.Length);
                    SwapPacket(1, ref writeDataPacket);

                    //6. 데이터 패킷에 복사
                    Array.Copy(wordLenPacket, 0, dataPacket, 0, wordLenPacket.Length);
                    dataPacket[wordLenPacket.Length] = ByteLenPacket;
                    Array.Copy(writeDataPacket, 0, dataPacket, wordLenPacket.Length + 1, writeDataPacket.Length);
                    //SMH3333 임시 211007
                    //Array.Reverse(dataPacket);
                }
                else
                {
                    UInt16 len = Convert.ToUInt16(length);
                    len = ((len % 2) == 1) ? Convert.ToUInt16((len / 2) + 1) : Convert.ToUInt16(len / 2);
                    dataPacket = BitConverter.GetBytes(len);
                    Array.Reverse(dataPacket);
                }

                return dataPacket;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Boolean ModBus_Write(Int32 address, Int32 length, Byte[] writeBuffer, Int32 slaveId = 1)
        {
            try
            {
                //1. ModBus패킷 만들기
                inner_MakeModBusPacket(address, length, writeBuffer, true);

                //2. 데이터 전송
                lock (this.m_functionLock)
                    this.m_CommClient.Send(this.m_packetData, this.m_packetData.Length, this.m_CommRemotePt);

                //3. 데이터 수신 대기
                ReceiveMessages(this.m_CommClient);

                return true;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                return false;
            }
        }
    }

    public class KeepAlive
    {
        public int m_Size;
        public UInt32 m_Onoff;
        public UInt32 m_KeepAliveTime;// Send a packet once every 10 seconds.
        public UInt32 m_KeepAliveInterval;// If no response, resend every second.
        public Byte[] m_InArray;

        /*
         * *constructor
         */
        public KeepAlive(UInt32 OnOff, UInt32 keepAliveTime, UInt32 keepAliveInterval)
        {
            m_Size = sizeof(UInt32);
            m_Onoff = OnOff;
            m_KeepAliveTime = keepAliveTime;
            m_KeepAliveInterval = keepAliveInterval;
            m_InArray = new Byte[m_Size * 3];

            Array.Copy(BitConverter.GetBytes(m_Onoff), 0, m_InArray, 0, m_Size);
            Array.Copy(BitConverter.GetBytes(m_KeepAliveTime), 0, m_InArray, m_Size, m_Size);
            Array.Copy(BitConverter.GetBytes(m_KeepAliveInterval), 0, m_InArray, m_Size * 2, m_Size);
        }

        /*
        public unsafe Byte[] Buffer
        {
            get
            {
                var buf = new Byte[sizeof(KeepAlive)];
                fixed(void* p = &this) Marshal.Copy(new IntPtr(p), buf, 0, buf.Length);
                return buf;
            }
        }*/
    }
}
