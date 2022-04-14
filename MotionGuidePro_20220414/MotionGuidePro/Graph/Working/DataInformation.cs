using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class DataInformation : ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public DataInformation()
        {
            this.m_Unit = String.Empty;
            this.m_DataMax = 0.0d;
            this.m_DataMin = 0.0d;
            this.m_Name = String.Empty;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        // 단위
        private String m_Unit;
        public String Unit
        {
            get { return this.m_Unit; }
            set { this.m_Unit = value; }
        }

        // 데이터 최소 값
        private Double m_DataMin;
        public Double DataMin
        {
            get { return this.m_DataMin; }
            set { this.m_DataMin = value; }
        }

        // 데이터 최대 값
        private Double m_DataMax;
        public Double DataMax
        {
            get { return this.m_DataMax; }
            set { this.m_DataMax = value; }
        }

        // 단위
        private String m_Name;
        public String Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        ///<summary>
        /// 함수
        /// </summary>
        // 디바이스로부터 받은 데이터를 저장하는 함수
        public void SetDeviceBasicInformation(String Name, String Unit, Double Min, Double Max)
        {
            try
            {
                //디바이스로 부터 받은 데이터를 저장한다.
                this.m_Unit = Unit;
                this.m_Name = Name;
                this.m_DataMin = Min;
                this.m_DataMax = Max;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 클론
        public Object Clone()
        {
            try
            {
                //해당 파일 복사
                DataInformation CloneData = new DataInformation();
                CloneData.DataMin = this.DataMin;
                CloneData.DataMax = this.DataMax;
                CloneData.Name = this.Name;
                CloneData.Unit = this.Unit;

                return CloneData;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
