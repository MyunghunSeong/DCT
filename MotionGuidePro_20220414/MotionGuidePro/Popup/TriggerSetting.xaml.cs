using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DCT_Graph;

namespace CrevisLibrary
{
    /// <summary>
    /// TriggerSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TriggerSetting : Window
    {
        ///<summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        /// <summary>
        /// 확인 / 취소 버튼 클릭 플래그
        /// </summary>
        public Boolean IsOK { get; set; }

        /// <summary>
        /// ViewModel 객체
        /// </summary>
        public TriggerControl_ViewModel ViewModel { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public TriggerSetting(TriggerControl Control)
        {
            InitializeComponent();
            this.IsOK = false;

            //ViewModel을 TriggerControl의 ViewModel로 설정해준다.
            this.DataContext = Control.DataContext;
            this.ViewModel = this.DataContext as TriggerControl_ViewModel;
        }

        /// <summary>
        /// 취소 버튼을 눌렀을 경우 호출
        /// </summary>
        private void CancelButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsOK = false;
                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 확인 버튼을 눌렀을 경우 호출
        /// </summary>
        private void OKButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //프로퍼티 업데이트
                BindingExpression bind = TriggerActionButton.GetBindingExpression(ToggleButton.IsCheckedProperty);
                bind.UpdateSource();

                bind = TriggerLevelBox.GetBindingExpression(TextBox.TextProperty);
                bind.UpdateSource();

                bind = TriggerCombo.GetBindingExpression(ComboBox.SelectedIndexProperty);
                bind.UpdateSource();

                bind = SamplingPeriodCombo.GetBindingExpression(ComboBox.SelectedIndexProperty);
                bind.UpdateSource();

                bind = SamplingTimeCombo.GetBindingExpression(ComboBox.SelectedIndexProperty);
                bind.UpdateSource();

                this.IsOK = true;
                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 트리거 액션 버튼이 Check되었을 때 호출
        /// </summary>
        private void TriggerActionButton_Checked(Object sender, RoutedEventArgs e)
        {
            try
            {
                //TriggerAction 텍스트 Riging Edge로 변경
                this.ViewModel.TriggerActionText = "Riging Edge Mode";
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 트리거 액션 버튼이 UnCheck되었을 때 호출
        /// </summary>
        private void TriggerActionButton_Unchecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                //TriggerAction 텍스트 Falling Edge로 변경
                this.ViewModel.TriggerActionText = "Falling Edge Mode";
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 트리거 레벨 텍스트 박스에서 키보드가 눌려진 경우 호출
        /// </summary>
        private void TriggerLevelBox_OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                //Enter 키를 눌렀을 때 실행
                if (e.Key == Key.Enter)
                    OKButton_OnClick(sender, e);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void TriggerCombo_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox combo = sender as ComboBox;
                String SelectedItem = combo.SelectedItem as String;

                Int32 Channel = Int32.Parse(SelectedItem.Substring(0, 1));

                Double Level = this.ViewModel.DigitalSignalArr[Channel - 1].PixcelToData(this.ViewModel.LastPos.Y);
                this.ViewModel.TriggerLevel = Level;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
