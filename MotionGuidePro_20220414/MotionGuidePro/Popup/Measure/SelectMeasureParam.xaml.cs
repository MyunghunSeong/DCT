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
using System.Windows.Shapes;

namespace CrevisLibrary
{
    /// <summary>
    /// SelectMeasureParam.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectMeasureParam : Window
    {
        /// <summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        // RootParam
        public DevParam RootParam { get; set; }

        // 확인 / 취소 여부
        public Boolean IsOK { get; set; }

        public SelectMeasureParam(DevParam Root)
        {
            InitializeComponent();

            this.RootParam = Root;

            //ViewModel 설정
            SelectMeasureParam_ViewModel viewModel = new SelectMeasureParam_ViewModel(this);
            viewModel.InitializeViewModel();
            this.DataContext = viewModel;
        }
    }
}
