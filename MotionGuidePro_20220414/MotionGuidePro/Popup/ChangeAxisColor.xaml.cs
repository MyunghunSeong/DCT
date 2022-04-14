using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// ChangeAxisColor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChangeAxisColor : Window
    {
        /// <summary>
        /// 에러처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        public OscilloscopeContent MyContent { get; set; }

        public Boolean IsOK { get; set; }

        public Brush SelectedColor { get; set; }

        public Int32 SelectedChannel { get; set; }

        public Int32 Channel { get; set; }

        public ChangeAxisColor(OscilloscopeContent Content, Int32 Channel)
        {
            InitializeComponent();
            this.MyContent = Content;
            this.IsOK = false;
            this.Channel = Channel;
        }

        /// <summary>
        /// 콤보박스 채널을 변경했을 때 호출
        /// </summary>
        private void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            try
            {

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
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsOK = true;

                Int32 Channel = Convert.ToInt32((this.ChanneComboBox.Text).Substring(0, 1)) - 1;
                this.SelectedChannel = Channel;

                for(int i = 0; i < this.MyContent.BrushColorArray.Length; i++)
                {
                    if (this.SelectedChannel.Equals(i))
                        continue;

                    Color targetColor = (this.SelectedColor as SolidColorBrush).Color;
                    Color stdColor = (this.MyContent.BrushColorArray[i] as SolidColorBrush).Color;
                    if (targetColor.R.Equals(stdColor.R)
                        && targetColor.G.Equals(stdColor.G)
                        && targetColor.B.Equals(stdColor.B))
                    {
                        MessageBox.Show("There are overlapping colors. Please choose again.");
                        this.IsOK = false;
                        return;
                    }
                }

                this.MyContent.BrushColorArray[Channel] = this.SelectedColor;

                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 취소 버튼을 눌렀을 경우 호출
        /// </summary>
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
        /// 창이 로드 되었을 때 호출
        /// </summary>
        private void ChangeAxisColor_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<String, Color> ColorArray = new Dictionary<String, Color>();
                PropertyInfo[] infoArr = typeof(Brushes).GetProperties();

                ColorArray.Add("Black", Brushes.Black.Color);
                ColorArray.Add("Gray", Brushes.Gray.Color);
                ColorArray.Add("RosyBrown", Brushes.RosyBrown.Color);
                ColorArray.Add("DarkGreen", Brushes.DarkGreen.Color);
                ColorArray.Add("Olive", Brushes.Olive.Color);
                ColorArray.Add("Blue", Brushes.Blue.Color);
                ColorArray.Add("Purple", Brushes.Purple.Color);
                ColorArray.Add("Red", Brushes.Red.Color);
                ColorArray.Add("Magenta", Brushes.Magenta.Color);
                ColorArray.Add("DarkOrange", Brushes.DarkOrange.Color);
                ColorArray.Add("DarkSalmon", Brushes.DarkSalmon.Color);
                ColorArray.Add("Aqua", Brushes.Aqua.Color);
                ColorArray.Add("DarkGoldenrod", Brushes.DarkGoldenrod.Color);
                ColorArray.Add("White", Brushes.White.Color);

                foreach(String key in ColorArray.Keys)
                {
                    //Object obj = ColorArray[i].GetValue(ColorArray[i], null);
                    //Brush Color = (Brush)obj;

                    Button panel = new Button();
                    panel.Tag = key;
                    panel.Click += Panel_Click;
                    panel.BorderBrush = Brushes.Black;
                    panel.Margin = new Thickness(3);
                    panel.BorderThickness = new Thickness(1);
                    panel.Background = new SolidColorBrush(ColorArray[key]);
                    this.ColorPalette.Children.Add(panel);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void Panel_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                String ColorName = btn.Tag as String;
                foreach (Button target in this.ColorPalette.Children)
                    target.BorderThickness = new Thickness(1);

                btn.BorderThickness = new Thickness(3);
                this.ColorNameText.Text = ColorName;
                this.CurrentRect.Fill = btn.Background;

                this.SelectedColor = btn.Background;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 창이 로드되고 컨트롤들이 랜딩 된 이후에 호출
        /// </summary>
        private void ChangeAxisColor_OnRendered(Object sender, EventArgs e)
        {
            try
            {
                this.ChanneComboBox.SelectedIndex = this.Channel;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 키보드가 눌려졌을 때 호출
        /// </summary>
        private void ChangeAxisColor_OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                //EnterKey가 눌려졌을 때 OK 버튼 이벤트 실행
                if (e.Key == Key.Enter)
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
