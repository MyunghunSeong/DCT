using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class Signal : ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public Signal()
        {
            this.m_Data = 0.0d;
            this.m_IsValid = true;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        // Data
        private Double m_Data;
        public Double Data
        {
            get { return this.m_Data; }
            set { this.m_Data = value; }
        }

        // IsValid
        private Boolean m_IsValid;
        public Boolean IsValid
        {
            get { return this.m_IsValid; }
            set { this.m_IsValid = value; }
        }

        public Object Clone()
        {
            try
            {
                Signal clone = new Signal();
                clone.Data = this.Data;
                clone.IsValid = this.IsValid;

                return clone;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
