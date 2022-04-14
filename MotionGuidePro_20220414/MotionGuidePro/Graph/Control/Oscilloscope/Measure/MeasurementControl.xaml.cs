using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrevisLibrary;
using MotionGuidePro.Main;

namespace DCT_Graph
{
    /// <summary>
    /// MeasurementControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MeasurementControl : UserControl
    {
        /// <summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        public OscilloscopeContent MyContent { get; set; }

        //MeasureControl ViewModel
        public MeasurementControl_ViewModel ViewModel { get; set; }

        //Y축 ViewModel
        public YAxisControl_ViewModel AxisViewModel { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public MeasurementControl(OscilloscopeContent Content)
        {
            InitializeComponent();
            this.MyContent = Content;

            //ViewModel설정
            this.ViewModel = new MeasurementControl_ViewModel(this);
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;
        }

        ///<summary>
        /// 함수
        /// </summary>
        // 값 구하기
        public void Run(Boolean IsUpdate=true)
        {
            try
            {
                //Min, Max, PeakToPeak, Mean값 구하기
                this.ViewModel.BasicInfoList.Clear();
                OscilloscopeParameterType paramType = new OscilloscopeParameterType();

                BasicMeasureInformation basic = new BasicMeasureInformation();
                basic.Name1 = "Min";
                String EnumName = this.MyContent.AxisInfoList[this.ViewModel.SelectChannel].CurrentSelectedAxisName.Replace(" ", "_");
                Enum.TryParse(EnumName, out paramType);
                this.ViewModel.Min = (Int32)this.MyContent.DigitalSignalMap[paramType].Min;
                basic.Value1 = this.ViewModel.Min;

                basic.Name2 = "P2P";
                EnumName = this.MyContent.AxisInfoList[this.ViewModel.SelectChannel].CurrentSelectedAxisName.Replace(" ", "_");
                Enum.TryParse(EnumName, out paramType);
                this.ViewModel.PeakToPeak = (Int32)this.MyContent.DigitalSignalMap[paramType].PeakToPeak;
                basic.Value2 = this.ViewModel.PeakToPeak;
                this.ViewModel.BasicInfoList.Add(basic);

                basic = new BasicMeasureInformation();
                basic.Name1 = "Max";
                EnumName = this.MyContent.AxisInfoList[this.ViewModel.SelectChannel].CurrentSelectedAxisName.Replace(" ", "_");
                Enum.TryParse(EnumName, out paramType);
                this.ViewModel.Max = (Int32)this.MyContent.DigitalSignalMap[paramType].Max;
                basic.Value1 = this.ViewModel.Max;

                basic.Name2 = "Mean";
                EnumName = this.MyContent.AxisInfoList[this.ViewModel.SelectChannel].CurrentSelectedAxisName.Replace(" ", "_");
                Enum.TryParse(EnumName, out paramType);
                this.ViewModel.Mean = (Int32)this.MyContent.DigitalSignalMap[paramType].Mean;
                basic.Value2 = this.ViewModel.Mean;
                this.ViewModel.BasicInfoList.Add(basic);

                //Unit 구하기
                this.ViewModel.Unit = this.MyContent.AxisInfoList[this.ViewModel.SelectChannel].DataInfoObj.Unit;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
