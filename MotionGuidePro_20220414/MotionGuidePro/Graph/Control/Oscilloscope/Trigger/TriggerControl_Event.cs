using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrevisLibrary;

namespace DCT_Graph
{
    public partial class TriggerControl
    {
        /// <summary>
        /// 트리거 옆에 톱니바퀴 모양을 눌렀을 때 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerSettingBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Double Level = this.ViewModel.DigitalSignalArr[this.ViewModel.SelectChannel].PixcelToData(this.ViewModel.LastPos.Y);
                this.ViewModel.TriggerLevel = Level;

                //트리거 설정 창을 띄운다.
                TriggerSetting SetDlg = new TriggerSetting(this);
                SetDlg.Owner = PublicVar.MainWnd;
                SetDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                SetDlg.ShowDialog();

                //this.MyContent.Timer.Interval = TimeSpan.FromSeconds(viewModel.SamplingTime);

                Double YPos = this.ViewModel.DigitalSignalArr[this.ViewModel.SelectChannel].DataToPixcel(this.ViewModel.TriggerLevel);
                this.ViewModel.LastPos = new Point(this.ViewModel.LastPos.X, YPos);
                this.MyContent.XAxis.DrawTriggerLine();

                this.TriggerLine.mainGrid.Background = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.Black : new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                this.TriggerLine.line.Stroke = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.White : Brushes.Black;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void TriggerBox_OnSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox Combo = sender as ComboBox;

                Int32 SelectedIndex = this.ViewModel.SelectChannel;
                Combo.Foreground = Brushes.Black;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
