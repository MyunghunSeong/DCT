using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public class SelectMeasureParam_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public SelectMeasureParam_ViewModel(Object Wnd) : base(Wnd) { }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.ParamInfoList = new ObservableCollection<MeasureParamInformation>();
            foreach(DevParam Param in (this.m_Wnd as SelectMeasureParam).RootParam.Children)
            {
                MeasureParamInformation info = new MeasureParamInformation(Param);
                info.InitializeViewModel();
                ParamInfoList.Add(info);
            }

            this.InfoCount = this.ParamInfoList.Count;
        }

        public Int32 InfoCount { get; set; }

        public ObservableCollection<MeasureParamInformation> ParamInfoList { get; set; }
    }

    public class MeasureParamInformation : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public MeasureParamInformation(DevParam Param)
        {
            this.Param = Param;
        }

        public DevParam Param { get; set; }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.Param.IsChecked = (this.Param.ParamInfo.Default) ? true : false;
            this.m_IsChecked = this.Param.IsChecked;
            this.m_IsDefault = (this.Param.ParamInfo.Default) ? true : false;
            this.m_ParamName = this.Param.ParamInfo.ParamName;
        }

        // 선택 / 해제
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

        // 파라미터 이름
        private String m_ParamName;
        public String ParamName
        {
            get { return this.m_ParamName; }
            set
            {
                this.m_ParamName = value;
                this.NotifyPropertyChanged("ParamName");
            }
        }

        // Default
        private Boolean m_IsDefault;
        public Boolean IsDefault
        {
            get { return this.m_IsDefault; }
            set
            {
                this.m_IsDefault = value;
                this.NotifyPropertyChanged("IsDefault");
            }
        }
    }
}
