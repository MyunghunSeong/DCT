using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class AutoScaleTool
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public AutoScaleTool(FitMode Mode)
        {
            this.m_MinValue = 0.0d;
            this.m_MaxValue = 0.0d;
            this.m_Length = 0;
            this.m_TickLength = 0;
            this.m_FitMode = Mode;
            this.m_TickStartValue = 0.0d;
            this.m_TickCount = 0;
            this.m_TickScale = 0.0d;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        // 데이터 최소 값
        private Double m_MinValue;
        public Double MinValue
        {
            get { return this.m_MinValue; }
            set { this.m_MinValue = value; }
        }

        // 데이터 최대 값
        private Double m_MaxValue;
        public Double MaxValue
        {
            get { return this.m_MaxValue; }
            set { this.m_MaxValue = value; }
        }

        // 좌표축 길이(픽셀)
        private Int32 m_Length;
        public Int32 Length
        {
            get { return this.m_Length; }
            set { this.m_Length = value; }
        }

        // 눈금 간격에 해달하는 픽셀
        private Double m_TickLength;
        public Double TickLength
        {
            get { return this.m_TickLength; }
            set { this.m_TickLength = value; }
        }

        // 매핑 Enum
        private FitMode m_FitMode;
        public FitMode FitMode
        {
            get { return this.m_FitMode; }
            set { this.m_FitMode = value; }
        }

        // 눈금 시작의 데이터 값
        public Double m_TickStartValue;
        public Double TickStartValue
        {
            get { return this.m_TickStartValue; }
        }

        // 눈금 개수
        public Int32 m_TickCount;
        public Int32 TickCount
        {
            get { return this.m_TickCount; }
        }

        // 눈금 한칸 당 데이터 스케일
        private Double m_TickScale;
        public Double TickScale
        {
            get { return this.m_TickScale; }
        }

        ///<summary>
        /// 자동으로 스케일을 맞춰주는 함수
        /// </summary>
        public void Run()
        {
            try
            {
                //모드에 따라서 분기
                switch (this.m_FitMode)
                {
                    case FitMode.ZeroFit:
                    case FitMode.Normal:
                        GetAutoScale(this.m_FitMode);
                        break;
                    case FitMode.DataFit:
                        GetAutoScaleOfDataFit();
                        break;
                }
            }
            catch (Exception)

            {
                throw;
            }
        }

        /// <summary>
        /// FitMode가 Normal이거나 ZeroFit인 경우 Scale을 구하는 함수
        /// </summary>
        private void GetAutoScale(FitMode Mode)
        {
            try
            {
                //범위 구하기
                Double Range = Math.Abs(this.MaxValue - this.MinValue);
                Double TurningScale = 0.0d;
                Double RangeScale = 0.0d;
                Int32 Number = 0;
                Double tmpRange = Range;
                //범위 값이 1보다 크거나 같은 경우
                if (Range >= 1)
                {
                    //Range값이 10보다 작을 때 까지 10으로 나눈다.
                    while (true)
                    {
                        tmpRange = tmpRange / 10;
                        if (tmpRange < 1)
                            break;
                        else
                            Number++;
                    }
                    TurningScale = Math.Pow(10, (Number - 1));
                    RangeScale = Math.Round((Double)(Range / TurningScale));
                    this.m_TickScale = RangeScale * Math.Pow(10, Number - 2);
                }
                else if (Range < 1)
                {
                    //Range값이 1보다 작을 때 까지 10으로 나눈다.
                    while (true)
                    {
                        if (Range == 0)
                            break;

                        if ((Range * 10) >= 1)
                            break;
                        else
                            Number++;
                    }
                    TurningScale = Math.Pow(10, Number);
                    RangeScale = Math.Round((Double)(TurningScale * Range));
                    this.m_TickScale = RangeScale * Math.Pow(10, -Number - 1);
                }

                //눈금 개수 구하기
                this.m_TickCount = (Int32)(this.m_Length / this.m_TickLength);

                //눈금 시작의 데이터 값
                switch (Mode)
                {
                    case FitMode.ZeroFit:
                        Double Temp = this.m_MaxValue - (this.m_TickScale * this.m_TickCount);
                        this.m_TickStartValue = Math.Ceiling(Temp - 1) * this.m_TickScale;
                        break;
                    case FitMode.Normal:
                        this.m_TickStartValue = this.m_MinValue;
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// FitMode가 DataFit인 경우 Scale을 구하는 함수
        /// </summary>
        private void GetAutoScaleOfDataFit()
        {
            try
            {
                //범위 구하기
                Double Range = Math.Abs((this.m_MaxValue - this.m_MinValue) * 1.2);
                this.m_TickScale = Range / this.m_Length;
                this.m_TickCount = (Int32)(this.Length / this.m_TickLength);
                this.m_TickStartValue = this.m_MinValue;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
