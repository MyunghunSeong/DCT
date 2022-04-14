using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotionGuidePro.Main;

namespace CrevisLibrary
{
    public class LogInformation : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Wnd">MainWindow 객체</param>
        public LogInformation(MainWindow Wnd) : base(Wnd) { }

        public override void InitializeViewModel()
        {
            this.m_LogStatus = new LogState();
            this.m_Message = String.Empty;
            this.m_StackTrace = String.Empty;
            this.m_LogTime = String.Empty;
        }

        //상태
        private LogState m_LogStatus;
        public LogState LogStatus
        {
            get { return this.m_LogStatus; }
            set
            {
                this.m_LogStatus = value;
                this.NotifyPropertyChanged("LogStatus");
            }
        }

        //메세지
        private String m_Message;
        public String Message
        {
            get { return this.m_Message; }
            set
            {
                this.m_Message = value;
                this.NotifyPropertyChanged("Message");
            }
        }

        //시간
        private String m_LogTime;
        public String LogTime
        {
            get { return this.m_LogTime; }
            set
            {
                this.m_LogTime = value;
                this.NotifyPropertyChanged("LogTime");
            }
        }

        //위치
        private String m_StackTrace;
        public String StackTrace
        {
            get { return this.m_StackTrace; }
            set
            {
                this.m_StackTrace = value;
                this.NotifyPropertyChanged("StackTrace");
            }
        }
    }
}
