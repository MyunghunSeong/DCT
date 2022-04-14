using System;
using System.Collections.Generic;
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

namespace DCT_Graph
{
    /// <summary>
    /// TriggerControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TriggerControl : UserControl
    {
        /// <summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        public OscilloscopeContent MyContent { get; set; }

        public TriggerLineControl TriggerLine { get; set; }

        public TriggerControl_ViewModel ViewModel { get; set; }

        public TriggerControl(OscilloscopeContent Content)
        {
            InitializeComponent();
            this.MyContent = Content;

            //ViewModel설정
            this.ViewModel = new TriggerControl_ViewModel(this);
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;
        }
    }
}
