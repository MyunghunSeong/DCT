using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CrevisLibrary;

namespace DCT_Graph
{
    public class DigitalSignal : ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public DigitalSignal()
        {
            this.m_DataInformObj = new DataInformation();
            this.m_SignalData = new List<Signal>();
            this.m_StartTime = 0;
            this.m_TimeInterval = 0;
            this.m_BlockSize = 10;
            this.m_BlockCount = 0;
            this.m_MaxBlockCount = 300000;
            this.m_Min = 0.0d;
            this.m_Max = 0.0d;
            this.m_PeakToPeak = 0.0d;
            this.m_Mean = 0.0d;
            this.m_ValidStartTime = 0;
            this.m_ValidEndTime = 0;
            this.m_XAxisInfo = new AxisInformation();
            this.m_YAxisInfo = new AxisInformation();
            this.m_MeanCount = 0;
            this.m_Sum = 0.0d;

            this.TimeIndexData = 0;
        }

        public Double TestWidth { get; set; }
        public Double TestEndTime { get; set; }
        private Double m_Test;

        public Int32 TimeIndexData { get; set; }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //Mean Count
        private Int32 m_MeanCount;
        public Int32 MeanCount
        {
            get { return this.m_MeanCount; }
            set { this.m_MeanCount = value; }
        }

        // ValidStartTime
        private Int32 m_ValidStartTime;
        public Int32 ValidStartTime
        {
            get { return this.m_ValidStartTime; }
            set { this.m_ValidStartTime = value; }
        }

        // ValidStartTime
        private Int32 m_ValidEndTime;
        public Int32 ValidEndTime
        {
            get { return this.m_ValidEndTime; }
            set { this.m_ValidEndTime = value; }
        }

        // 상위 컨텐츠
        public OscilloscopeContent MyContent { get; set; }

        // DataInformation 객체
        private DataInformation m_DataInformObj;
        public DataInformation DataInformObj
        {
            get { return this.m_DataInformObj; }
            set { this.m_DataInformObj = value; }
        }

        // 시간 순서 데이터 리스트
        private List<Signal> m_SignalData;
        public List<Signal> SignalData
        {
            get { return this.m_SignalData; }
            set { this.m_SignalData = value; }
        }

        // 시작 시간
        private Double m_StartTime;
        public Double StartTime
        {
            get { return this.m_StartTime; }
            set { this.m_StartTime = value; }
        }

        //데이터 1개당 시간 간격
        private Double m_TimeInterval;
        public Double TimeInterval
        {
            get { return this.m_TimeInterval; }
            set { this.m_TimeInterval = value; }
        }

        //통신으로 받은 데이터가 한번에 업데이트 되는 크기
        private Int32 m_BlockSize;
        public Int32 BlockSize
        {
            get { return this.m_BlockSize; }
            set { this.m_BlockSize = value; }
        }

        // 저장할 블럭 최대 개수
        private Int32 m_MaxBlockCount;
        public Int32 MaxBlockCount
        {
            get { return this.m_MaxBlockCount; }
            set { this.m_MaxBlockCount = value; }
        }

        // 받은 블럭 총 개수
        private Int32 m_BlockCount;
        public Int32 BlockCount
        {
            get { return this.m_BlockCount; }
            set { this.m_BlockCount = value; }
        }

        //시그널 전체의 합산 값
        private Double m_Sum;
        public Double Sum
        {
            get { return this.m_Sum; }
            set { this.m_Sum = value; }
        }

        //시그널 전체의 Min값
        private Double m_Min;
        public Double Min
        {
            get { return this.m_Min; }
            set { this.m_Min = value; }
        }

        //시그널 전체의 Max값
        private Double m_Max;
        public Double Max
        {
            get { return this.m_Max; }
            set { this.m_Max = value; }
        }

        //시그널 전체의 Max - Min
        private Double m_PeakToPeak;
        public Double PeakToPeak
        {
            get { return this.m_PeakToPeak; }
            set { this.m_PeakToPeak = value; }
        }

        //시그널 전체의 평균 값
        private Double m_Mean;
        public Double Mean
        {
            get { return this.m_Mean; }
            set { this.m_Mean = value; }
        }

        // X축 정보
        private AxisInformation m_XAxisInfo;
        public AxisInformation XAxisInfo
        {
            get { return this.m_XAxisInfo; }
            set { this.m_XAxisInfo = value; }
        }

        // Y축 정보
        private AxisInformation m_YAxisInfo;
        public AxisInformation YAxisInfo
        {
            get { return this.m_YAxisInfo; }
            set { this.m_YAxisInfo = value; }
        }

        ///<summary>
        /// 함수
        /// </summary>

        ///<summary>
        /// 화면에 표시할 오버레이를 전달하는 함수
        /// </summary>
        /// <returns>화면에 표시할 오버레이</returns>
        public SignalOverlay GetOverlay()
        {
            SignalOverlay overlay = null;
            Double PixcelX = 0.0d;
            Double PixcelY = 0.0d;

            //Double tmpMin = this.YAxisInfo.MaxValue;
            //Double tmpMax = this.YAxisInfo.MinValue;

            this.Min = this.YAxisInfo.MaxValue;
            this.Max = this.YAxisInfo.MinValue;

            try
            {
                //현재 그래프의 높이와 넓이 값을 구한다.
                Double Width = this.XAxisInfo.AxisLength;
                if (Width < 0)
                    return null;

                Double Height = this.YAxisInfo.AxisLength;
                if (Height < 0)
                    return null;

                this.TestWidth = Width;

                //끝 시간을 구한다(틱당 시간 간격 * 틱 개수)
                Double EndTimeData = this.XAxisInfo.StartValue + this.XAxisInfo.TickScale * (Width / this.XAxisInfo.TickLength);
                if (!this.MyContent.IsOscilloCommCheck)
                {
                    if (this.SignalData.Count > EndTimeData * 10)
                        this.XAxisInfo.StartValue = (this.SignalData.Count - (EndTimeData * 10)) / 10.0;
                }

                Double TickCount = this.XAxisInfo.TickCount;
                //Double EndTimeData = (this.XAxisInfo.TickLength > Width)
                //    ? this.XAxisInfo.StartValue + (this.XAxisInfo.TickScale * TickCount)
                //    : this.XAxisInfo.StartValue + this.XAxisInfo.TickScale * (Width / this.XAxisInfo.TickLength);

                //Double EndTimeData = this.XAxisInfo.StartValue + this.XAxisInfo.TickScale * (Int32)(Width / this.XAxisInfo.TickLength);
                this.TestEndTime = EndTimeData;

                //물리적 시간 데이터를 인덱스 데이터로 변경한다.
                //this.m_StartTime = (this.XAxisInfo.StartValue < 0) ? 0 : this.XAxisInfo.StartValue;
                this.m_StartTime = this.XAxisInfo.StartValue;
                Int32 StartTimeIndex = (Int32)(this.m_StartTime * 10);
                Int32 EndTimeIndex = (Int32)(EndTimeData * 10);

                //.시작 인덱스가 현재 받은 Signal데이터의 개수보다 적은 경우에만 처리한다.
                // 시작 인덱스가 현재 받은 Signal데이터보다 크게 되면 그릴수가 없다.
                if (StartTimeIndex < this.SignalData.Count)
                {
                    //시그널 데이터 정보를 담을 Overlay객체 선언
                    overlay = new SignalOverlay();

                    ////현재 받은 Signal데이터 개수에서 현재 시작 인덱스값을 뺀 만큼 배열을 할당
                    //tmpArray = new Signal[this.SignalData.Count - StartTimeIndex];
                    ////Signal데이터에 데이터를 시작 인덱스부터 복사 해준다.
                    //this.SignalData.CopyTo(StartTimeIndex, tmpArray, 0, tmpArray.Length);

                    ////복사된 데이터의 크기만큼 반복해서 데이터를 좌표로 변환해서 리스트에 저장
                    //for (int i = 0; i < tmpArray.Length; i++)
                    //{
                    //    //시간 단위를 맞춰줘야 되는데 시작 시간이랑 끝시간이랑 ms단위로 맞춰놓은 상태
                    //    //i == 1은 100microsecond
                    //    //이걸 ms로 변환해주면 i == 1 => 0.1을 TimeToPixcel에 넘겨줘야 되는건가??
                    //    //그래서 내가 궁금한건 뭐지?
                    //    // 1. tmpArray로 처리한거 같이 처리해도 문제될건 없는가?
                    //    // 2. i == 1 을 ms에 맞춰서 0.1로 변환해서 넘겨주는게 맞는것인가?
                    //    // 3. 그래프 줌이나 움직일 때 시작 인덱스부터 끝 인덱스까지 구해지는데 
                    //    //이와 같은 알고리즘으로 진행했을 때 데이터 신뢰성은 맞는지??
                    //    PixcelX = TimeToPixel(i / 10);
                    //    PixcelY = DataToPixcel(tmpArray[i].Data);

                    //    overlay.Points.Add(new Point(PixcelX, PixcelY));
                    //    overlay.DrawColor = this.MyContent.BrushColorArray[this.YAxisInfo.Channel];
                    //}

                    //for (int i = 0; i < this.SignalData.Count; i++)
                    //{
                    //    this.m_TestIndex = i;
                    //    tmpSum += this.SignalData[i].Data;
                    //}

                    //this.Mean = tmpSum / this.SignalData.Count;

                    //X좌표 구하기
                    this.MeanCount = 0;
                    for (int i = 0; i < Width; i++)
                    {
                        //X좌표
                        PixcelX = i;

                        //Y좌표
                        Double CurrentTime = PixcelToTimeByDouble(PixcelX);
                        this.m_Test = CurrentTime;

                        this.ValidStartTime = (this.BlockCount > this.MaxBlockCount) ? this.BlockCount - this.MaxBlockCount : 0;
                        this.ValidEndTime = this.BlockCount;
                        //this.ValidStartTime = 0;
                        //this.ValidEndTime = (Int32)EndTimeData;

                        if ((Int32)(CurrentTime * 10) < this.SignalData.Count && CurrentTime >= 0)
                        {
                           if (this.ValidStartTime <= (Int32)CurrentTime && this.ValidEndTime >= (Int32)CurrentTime)
                           {
                                PixcelY = this.GetData(CurrentTime);

                                //좌표 값 저장
                                overlay.Points.Add(new Point(PixcelX, PixcelY));
                                overlay.DrawColor = this.MyContent.BrushColorArray[this.YAxisInfo.Channel];
                            }
                        }
                    }

                    UpdateSignalInfo();
                }

                return overlay;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<summary>
        /// 데이터에 따라서 좌표값을 반환해주는 함수
        /// </summary>
        public Double GetData(Double Time, Boolean IsManual=false)
        {
            try
            {
                //리턴할 포인트 객체
                Double yPos = 0.0d;

                //인덱스 구하기
                //Int32 TimeIndex = (Int32)(Time - this.ValidStartTime);
                Int32 ValidTime = (Int32)((Time - this.ValidStartTime) * 10);
                this.TimeIndexData = (Int32)((Time - this.ValidStartTime));
                Int32 TimeIndex = (IsManual) ? (Int32)Time : TimeIndexData;//(Int32)(ValidTime * 10);
                String StrValidNum = (Time * 10).ToString();
                Int32 tmpTime = 0;

                List<Double> aaa = new List<Double>();

                //해당 시간의 데이터가 존재하는지 확인
                if (Int32.TryParse(StrValidNum, out tmpTime))
                {
                    if (this.SignalData.Count > 0)
                    {
                        Double Data = (this.SignalData[TimeIndex * 10].IsValid) ? this.SignalData[TimeIndex * 10].Data : 1000;
                        this.Sum += Data;
                        this.MeanCount++;
                        //Double Data = this.SignalData[TimeIndex * 10].Data;

                        if (Data < this.Min)
                            this.Min = Data;

                        if (Data > Max)
                            this.Max = Data;

                        yPos = DataToPixcel(Data);
                    }
                }
                //인터폴레이션 진행
                else
                {
                    if (this.SignalData.Count > 0)
                    {
                        Boolean IsValid1 = false;
                        Boolean IsValid2 = false;

                        Double y1 = 0;
                        Double y2 = 0;

                        Int32 x1 = ValidTime; //(Int32)(TimeIndex * 10);
                        Int32 x2 = ValidTime + 1;

                        y1 = DataToPixcel(this.SignalData[x1].Data);
                        IsValid1 = this.SignalData[x1].IsValid;
                        if (x2 >= this.SignalData.Count)
                        {
                            y2 = y1;
                            IsValid2 = this.SignalData[x1].IsValid;
                        }
                        else
                        {
                            y2 = DataToPixcel(this.SignalData[x2].Data);
                            IsValid2 = this.SignalData[x2].IsValid;
                        }

                        Double a = (y2 - y1) / (x2 - x1);
                        Double b = y1 - (a * x1);

                        double Pixcel = (IsValid1 && IsValid2) ? y1 + ((y2 - y1) * ((((Time - this.ValidStartTime) * 10) - x1) / (x2 - x1)))
                            : -280;

                        //Double Pixcel = (this.SignalData[x1].IsValid && this.SignalData[x2].IsValid) ? (a * Time) + b : 1000;
                        Double Data = PixcelToData(Pixcel);
                        this.Sum += Data;
                        this.MeanCount++;
                        if (Data > 310 || Data < 290)
                            aaa.Add(Data);

                        if (Data < this.Min)
                            this.Min = Data;

                        if (Data > this.Max)
                            this.Max = Data;

                        yPos = DataToPixcel(Data);
                    }
                }

                return yPos;
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate ()
                {
                    this.MyContent.IsOscilloCommCheck = true;
                });
                throw;
            }
        }

        /// <summary>
        /// 관심 시간에 대한 데이터를 리턴해주는 함수
        /// </summary>
        /// <param name="InterestTime">관심 시간</param>
        /// <returns>관심 시간에 대한 데이터</returns>
        private Double GetInterestValue(Double InterestTime)
        {
            try
            {
                return 0.0d;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 시간을 픽셀 데이터로 변환하는 함수
        /// </summary>
        /// <param name="Time">픽셀로 변환할 시간 데이터</param>
        /// <returns>변환된 픽셀 데이터</returns>
        public Double TimeToPixel(Double Time)
        {
            try
            {
                /*
                Double EndTime = this.XAxisInfo.TickScale * (Int32)(this.XAxisInfo.AxisLength / this.XAxisInfo.TickLength);

                Double bbb = this.XAxisInfo.AxisLength / (EndTime - this.XAxisInfo.StartValue);
                String ccc = (bbb * 1000).ToString().Split('.')[0];
                Int32 len = ccc.Length;
                Int32 ddd = Int32.Parse(ccc);

                Int32 eee = ddd % (10 * len);

                Double TimeScale = 0.0d;
                if (eee > 4)
                {
                    TimeScale = Math.Round(bbb, 3);
                    Int32 fff = (Int32)(TimeScale * 1000);
                    Int32 tmpLen = fff.ToString().Length;
                    Int32 ggg = fff % (10 * tmpLen);
                    TimeScale = (fff - ggg) / 1000.0d;
                }
                else
                    TimeScale = Math.Round(bbb, 2);

                //Double TimeScale = this.XAxisInfo.AxisLength / (EndTime - this.StartTime);
                Double Offset_X = TimeScale * this.StartTime;

                return (Time * TimeScale) - Offset_X;
                */

                Double EndTime = StartTime + this.XAxisInfo.TickScale * (Int32)(this.XAxisInfo.AxisLength / this.XAxisInfo.TickLength);

                Double TimeScale = this.XAxisInfo.AxisLength / (EndTime - this.StartTime);
                Double Offset_X = TimeScale * this.StartTime;

                return (Time * TimeScale) - Offset_X;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터를 픽셀로 변환하는 함수
        /// </summary>
        /// <param name="Data">변환한 데이터</param>
        /// <returns>변환된 픽셀 데이터</returns>
        public Double DataToPixcel(Double Data)
        {
            try
            {
                Double MinValue = this.YAxisInfo.MinValue;
                Double MaxValue = this.YAxisInfo.MaxValue;

                Double ValueScale = this.YAxisInfo.AxisLength / (MaxValue - MinValue);
                Double Offset_Y = MinValue * ValueScale;

                return this.YAxisInfo.AxisLength - ((Data * ValueScale) - Offset_Y);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 픽셀 데이터(X축)를 시간 데이터로 변환하는 함수
        /// </summary>
        /// <param name="PixelX">픽셀데이터(X축)</param>
        /// <returns>변환된 시간 데이터</returns>
        public Int32 PixcelToTime(Double PixcelX)
        {
            try
            {
                Int32 Count = (Int32)(PixcelX / this.XAxisInfo.TickLength);

                Double EndTime = StartTime + this.XAxisInfo.TickScale * (Int32)(this.XAxisInfo.AxisLength / this.XAxisInfo.TickLength);

                Int32 aaa = (1000 - (Int32)this.XAxisInfo.TickScale) * Count;

                Double bbb = this.XAxisInfo.AxisLength / (EndTime - this.XAxisInfo.StartValue);
                String ccc = (bbb * 1000).ToString().Split('.')[0];
                Int32 len = ccc.Length;
                Int32 ddd = Int32.Parse(ccc);

                Int32 eee = ddd % (10 * len);

                Double TimeScale = 0.0d;
                if (eee > 4)
                {
                    TimeScale = Math.Round(bbb, 3);
                    Int32 fff = (Int32)(TimeScale * 1000);
                    Int32 tmpLen = fff.ToString().Length;
                    Int32 ggg = fff % (10 * tmpLen);
                    TimeScale = (fff - ggg) / 1000.0d;
                }
                else
                    TimeScale = Math.Round(bbb, 2);

                Double Offset_X = TimeScale * this.XAxisInfo.StartValue;

                return (Int32)((PixcelX + Offset_X) / TimeScale) - aaa;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Double PixcelToTimeByDouble(Double PixcelX)
        {
            try
            {
                Double Width = this.XAxisInfo.AxisLength;
                Double EndTime = this.XAxisInfo.StartValue + this.XAxisInfo.TickScale * (Width / this.XAxisInfo.TickLength);
                //this.StartTime + this.XAxisInfo.TickScale * (Width / this.XAxisInfo.TickLength);
                Double TimeScale = Width / (EndTime - this.XAxisInfo.StartValue);
                Double Offset_X = TimeScale * this.XAxisInfo.StartValue;

                return (PixcelX + Offset_X) / TimeScale;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 픽셀(Y축)데이터를 데이터로 변환하는 함수
        /// </summary>
        /// <param name="PixcelY">픽셀 데이터(Y축)</param>
        /// <returns>변환된 데이터</returns>
        public Double PixcelToData(Double PixcelY)
        {
            try
            {
                Double MinValue = this.YAxisInfo.MinValue;
                Double MaxValue = this.YAxisInfo.MaxValue;

                Double ValueScale = this.YAxisInfo.AxisLength / (MaxValue - MinValue);
                Double Offset_Y = MinValue * ValueScale;

                return ((this.YAxisInfo.AxisLength - PixcelY) + Offset_Y) / ValueScale;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 그래프 축에 대한 정보(DigitalSignal)를 리턴하는 함수
        /// </summary>
        /// <param name="StartTime">시작 시간</param>
        /// <param name="EndTime">끝 시간</param>
        /// <returns>그래프 축 정보(DigitalSignal)</returns>
        private DigitalSignal GetSignal(Double StartTime, Double EndTime)
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

        /// <summary>
        /// Signal 데이터를 업데이트 하는 함수
        /// </summary>
        private void UpdateSignalInfo()
        {
            try
            {
                this.PeakToPeak = this.Max - this.Min;
                this.Mean = this.Sum / this.MeanCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 시그날 데이터를 추가하는 함수
        /// </summary>
        /// <param name="Signal">추가할 시그널</param>
        public void AddSignal(List<Signal> SignalList)
        {
            try
            {
                //this.m_SignalData의 카운트가 MaxBlockCount보다 큰 경우
                //this.m_SignalData의 첫번째 데이터를 지우고 마지막 데이터를 넣어준다.
                if (this.m_BlockCount >= this.m_MaxBlockCount)
                {
                    foreach (Signal sig in SignalList)
                    {
                        this.m_SignalData.RemoveAt(0);
                        this.m_SignalData.Add(sig);
                    }
                }
                //그렇지 않은 경우 그냥 시그널 데이터를 넣어준다.
                else
                    this.m_SignalData.AddRange(SignalList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Object Clone()
        {
            try
            {
                DigitalSignal clone = new DigitalSignal();
                clone.BlockCount = this.BlockCount;
                clone.BlockSize = this.BlockSize;
                clone.DataInformObj = (DataInformation)this.DataInformObj.Clone();
                clone.Max = this.Max;
                clone.Min = this.Min;
                clone.MyContent = this.MyContent;
                clone.PeakToPeak = this.PeakToPeak;
                clone.SignalData = this.SignalData;
                clone.StartTime = this.StartTime;
                clone.TimeIndexData = this.TimeIndexData;
                clone.TimeInterval = this.TimeInterval;
                clone.ValidEndTime = this.ValidEndTime;
                clone.ValidStartTime = this.ValidStartTime;
                clone.XAxisInfo = (AxisInformation)this.XAxisInfo.Clone();
                clone.YAxisInfo = (AxisInformation)this.YAxisInfo.Clone();
                return clone;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

