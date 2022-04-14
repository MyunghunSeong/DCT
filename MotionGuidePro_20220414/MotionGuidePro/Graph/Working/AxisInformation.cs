using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrevisLibrary;

namespace DCT_Graph
{
    public class AxisInformation : ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public AxisInformation()
        {
            this.m_AxisName = String.Empty;
            this.m_Channel = 0;
            this.m_TickScale = 0.0d;
            this.m_TickCount = 0;
            this.m_AxisLength = 0;
            this.m_StartValue = 0.0d;
            this.m_TickCount = 0;
            this.m_TickLength = 40;
            this.m_MinValue = 0.0d;
            this.m_MaxValue = 0.0d;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>

        // 좌표축 표시 이름
        private String m_AxisName;
        public String AxisName
        {
            get { return this.m_AxisName; }
            set { this.m_AxisName = value; }
        }

        // 채널 인덱스
        private Int32 m_Channel;
        public Int32 Channel
        {
            get { return this.m_Channel; }
            set { this.m_Channel = value; }
        }

        // 눈금 한칸 당 데이터 스케일
        private Double m_TickScale;
        public Double TickScale
        {
            get { return this.m_TickScale; }
            set { this.m_TickScale = value; }
        }

        // 좌표축 길이(픽셀)
        private Int32 m_AxisLength;
        public Int32 AxisLength
        {
            get { return this.m_AxisLength; }
            set { this.m_AxisLength = value; }
        }

        // 좌표축 시작 값
        private Double m_StartValue;
        public Double StartValue
        {
            get { return this.m_StartValue; }
            set { this.m_StartValue = value; }
        }

        // 축 눈금 간격에 해당하는 픽셀
        private Double m_TickLength;
        public Double TickLength
        {
            get { return this.m_TickLength; }
            set { this.m_TickLength = value; }
        }

        // 눈금 개수
        private Int32 m_TickCount;
        public Int32 TickCount
        {
            get { return this.m_TickCount; }
            set { this.m_TickCount = value; }
        }

        // 최소 값(보여지는 값)
        private Double m_MinValue;
        public Double MinValue
        {
            get { return this.m_MinValue; }
            set
            {
                this.m_MinValue = value;
            }
        }

        // 최대 값(보여지는 값)
        private Double m_MaxValue;
        public Double MaxValue
        {
            get { return this.m_MaxValue; }
            set { this.m_MaxValue = value; }
        }

        public Object Clone()
        {
            try
            {
                AxisInformation clone = new AxisInformation();
                clone.AxisLength = this.AxisLength;
                clone.AxisName = this.AxisName;
                clone.Channel = this.Channel;
                clone.MaxValue = this.MaxValue;
                clone.MinValue = this.MinValue;
                clone.StartValue = this.StartValue;
                clone.TickCount = this.TickCount;
                clone.TickLength = this.TickLength;
                clone.TickScale = this.TickScale;

                return clone;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
