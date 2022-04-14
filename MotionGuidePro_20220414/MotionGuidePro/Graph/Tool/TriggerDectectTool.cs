using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_Graph
{
    public class TriggerDectectTool
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public TriggerDectectTool(TriggerControl_ViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.m_LastSignal = new Signal();
            this.m_IsEvent = false;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //TriggerControl_ViewModel
        public TriggerControl_ViewModel ViewModel { get; set; }

        //Signal 값 + 패킷으로 받은 1개 블록
        private List<Signal> m_SignalList;
        public List<Signal> SignalList
        {
            get { return this.m_SignalList; }
            set { this.m_SignalList = value; }
        }

        //패킷의 마지막 데이터(다음 패킷의 첫번째 비교에 쓰인다)
        private Signal m_LastSignal;
        public Signal LastSignal
        {
            get { return this.m_LastSignal; }
            set { this.m_LastSignal = value; }
        }

        private Boolean m_IsEvent;
        public Boolean IsEvent
        {
            get { return this.m_IsEvent; }
            set { this.m_IsEvent = value; }
        }

        private Int32 m_EventTimeIndex;
        public Int32 EventTimeIndex
        {
            get { return this.m_EventTimeIndex; }
            set { this.m_EventTimeIndex = value; }
        }

        public void Run(Boolean IsFirstPacket=false)
        {
            try
            {
                Int32 count = 0;
                Boolean Condition1 = false;

                //첫번째 패킷이 아닌 경우는 첫번채 리스트에 전 데이터 마지막 부분을 넣는다.
                if (!IsFirstPacket)
                    this.SignalList.Insert(0, this.LastSignal);

                switch (this.ViewModel.TriggerAction)
                {
                    //Falling Action
                    case CrevisLibrary.TriggerActionType.FALLING_EDGE:
                        foreach (Signal sig in this.SignalList)
                        {
                            //현재 데이터가 설정한 Trigger값보다 크면 Condition1 충족
                            if (sig.Data > this.ViewModel.TriggerLevel)
                                Condition1 = true;

                            //Condition1이 true이고 현재 데이터 값이 설정한 Trigger값보다 작은 경우
                            //Falling Trigger 발생
                            if (sig.Data <= this.ViewModel.TriggerLevel && Condition1)
                            {
                                this.IsEvent = true;
                                this.EventTimeIndex = count;
                                break;
                            }
                            count++;
                        }
                        break;
                    //Rising Action
                    case CrevisLibrary.TriggerActionType.RIGING_EDGE:
                        foreach (Signal sig in this.SignalList)
                        {
                            //현재 데이터가 설정한 Trigger값보다 작으면 Condition1 충족
                            if (sig.Data < this.ViewModel.TriggerLevel)
                                Condition1 = true;

                            //Condition1이 true이고 현재 데이터 값이 설정한 Trigger값보다 큰 경우
                            //Rising Trigger 발생
                            if (sig.Data >= this.ViewModel.TriggerLevel && Condition1)
                            {
                                this.IsEvent = true;
                                this.EventTimeIndex = count;
                                break;
                            }
                            count++;
                        }
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
