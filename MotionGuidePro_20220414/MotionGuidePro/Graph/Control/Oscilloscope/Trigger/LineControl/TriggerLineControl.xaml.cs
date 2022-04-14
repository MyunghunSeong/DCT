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

namespace DCT_Graph
{
    /// <summary>
    /// TriggerLineControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TriggerLineControl : UserControl
    {
        //현재 커서가 이동한 Y축 좌표(부모 캔버스 안에서의 위치)
        public Double ControlYPos { get; set; }

        public TriggerControl_ViewModel ViewModel { get; set; }

        public TriggerLineControl(TriggerControl Control, Double XPos = 0.0d)
        {
            InitializeComponent();

            //ViewModel 설정
            this.DataContext = Control.DataContext;
            this.ViewModel = this.DataContext as TriggerControl_ViewModel;

            //축 길이 설정
            (Control.ViewModel).AxisLength = XPos;
        }

        public void SaveCursorPosition(Double YPos)
        {
            try
            {
                this.ViewModel.LastPos = new Point(0, this.ControlYPos + YPos);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
