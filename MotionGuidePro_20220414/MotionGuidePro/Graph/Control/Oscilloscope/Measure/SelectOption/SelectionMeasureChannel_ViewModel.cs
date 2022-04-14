using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public class SelectionMeasureChannel_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public SelectionMeasureChannel_ViewModel(Object Wnd) : base(Wnd) { }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.AxisInfoList = new ObservableCollection<AxisInformation>();
            foreach (var item in (this.m_Wnd as SelectionMeasureChannel).MyContent.AxisInfoList)
            {
                AxisInformation info = new AxisInformation();
                info.IsChecked = item.IsChecked;
                info.AxisName = item.ChannelName;
                this.AxisInfoList.Add(info);
            }
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //축 정보 리스트
        public ObservableCollection<AxisInformation> AxisInfoList { get; set; }
    }

    public class AxisInformation : ViewModel
    {
        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_IsChecked = true;
            this.m_AxisName = String.Empty;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary
        // 선택
        private Boolean m_IsChecked;
        public Boolean IsChecked
        {
            get { return this.m_IsChecked; }
            set
            {
                this.m_IsChecked = value;
                this.NotifyPropertyChanged("IsChecked");
            }
        }

        // 축 이름
        private String m_AxisName;
        public String AxisName
        {
            get { return this.m_AxisName; }
            set
            {
                this.m_AxisName = value;
                this.NotifyPropertyChanged("AxisName");
            }
        }
    }
}
