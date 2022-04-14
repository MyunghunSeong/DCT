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
using DCT_Graph;

namespace CrevisLibrary
{
    /// <summary>
    /// ManualScaleSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualScaleSetting : Window
    {
        /// <summary>
        /// 에러 이벤트 처리
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        //확인 / 취소 버튼 여부
        public Boolean IsOK { get; set; }

        public ManualScaleSetting(DCT_Graph.AxisInformation Info)
        {
            InitializeComponent();
            this.DataContext = Info;
            this.IsOK = false;
        }

        /// <summary>
        /// 확인 버튼을 눌렀을 때 호출
        /// </summary>
        /// <param name="e"></param>
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //프로퍼티 값 변경
                BindingExpression binding = tickBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = minBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

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
        /// 취소 버튼을 눌렀을 때 호출
        /// </summary>
        /// <param name="e"></param>
        private void CancelBtn_OnClick(Object sender, RoutedEventArgs e)
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
        /// 키보드가 입력됐을 때 호출
        /// </summary>
        private void ManualSetting_OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                //Enter키를 눌렀을 때만 처리
                if (e.Key.Equals(Key.Enter))
                    OKBtn_OnClick(sender, e);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
