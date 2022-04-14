using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using DevExpress.Xpf.Docking;
using MotionGuidePro;
using MotionGuidePro.Main;

namespace DCT_Graph
{
    /// <summary>
    /// OscilloscopeControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OscilloscopeControl : UserControl

    {
        //이벤트 처리 핸들러
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        //oscilloscopeContent 객체
        public OscilloscopeContent MyContent { get; set; }

        //Measure 컨트롤 넓이
        private Double m_MeasurementControlWidth;

        //오실로스코프 크기 변경 플래그
        private Boolean m_IsOscilloDisplaySizeChanged;

        //MeasureControl추가 카운트
        public Int32 AddMeasureControlCount { get; set; }

        //Test Thread SMH9999
        public Thread TestThread { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Content"></param>
        public OscilloscopeControl(OscilloscopeContent Content)
        {
            InitializeComponent();
            this.MyContent = Content;
            this.MyContent.OscilloscopeControl = this;
            this.DataContext = MyContent;
            this.m_IsOscilloDisplaySizeChanged = false;
            this.AddMeasureControlCount = 0;
        }

        /// <summary>
        /// X축 그리기
        /// </summary>
        public void DrawXAxisDisplay()
        {
            try
            {
                this.MyContent.XAxis = new SignalDisplayControl(this.MyContent);
                this.MyContent.XAxis.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
                this.myStackPane.Children.Add(this.MyContent.XAxis);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 그래프 화면을 그리는 함수
        /// </summary>
        public void DrawGraphDisplay(Boolean IsOnlyYAxis=false)
        {
            try
            {
                //패널에 있는 컨트롤 모두 지우고
                this.myStackPane.Children.Clear();

                //ViewModel에 있는 YAxisControl 리스트 지워주고
                MyContent.YAxisList.Clear();

                //Y축 다시 그리기
                Grid subGrid = new Grid();
                for (int i = 0; i < 5; i++)
                {
                    YAxisControl yControl = new YAxisControl(this.MyContent, i);
                    this.MyContent.YAxisList.Add(yControl);
                    this.myStackPane.Children.Add(this.MyContent.YAxisList[i]);

                    this.MyContent.YAxisList[i].Width = (this.MyContent.AxisInfoList[i].IsChannelSelected)
                            ? 50.0d : 0.0d;

                    Grid ScaleGrid = new Grid();
                    ScaleGrid.SetBinding(Grid.BackgroundProperty, new Binding("ThemaColor"));
                    ScaleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                    ScaleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
                    //ScaleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.7, GridUnitType.Star) });

                    Button AutoScaleButton = new Button();
                    AutoScaleButton.SetBinding(Button.BorderBrushProperty, new Binding("ThemaForeColor"));
                    AutoScaleButton.SetBinding(Button.ForegroundProperty, new Binding("ThemaForeColor"));
                    AutoScaleButton.SetBinding(Button.IsEnabledProperty, new Binding("IsButtonEnabled"));
                    AutoScaleButton.Content = "AUTO";
                    AutoScaleButton.Margin = new Thickness(1);
                    AutoScaleButton.Name = "AutoBtn_" + i.ToString();
                    AutoScaleButton.FontSize = 8.0f;
                    AutoScaleButton.Click += AutoScaleBtn_OnClick;
                    AutoScaleButton.Style = (Style)Application.Current.FindResource("CustomButtonStyle");
                    ScaleGrid.Children.Add(AutoScaleButton);
                    Grid.SetRow(AutoScaleButton, 0);

                    #region 틱 조정
                    /*
                    TextBox Text = new TextBox();
                    Text.Name = "Text_" + i.ToString();
                    Text.KeyDown -= ScaleText_OnKeyDown;
                    Text.KeyDown += ScaleText_OnKeyDown;
                    Text.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Text.VerticalContentAlignment = VerticalAlignment.Center;
                    Text.Text = "100";
                    Text.BorderBrush = Brushes.Black;
                    Text.BorderThickness = new Thickness(1);
                    Text.Margin = new Thickness(3, 10, 3, 10);
                    ScaleGrid.Children.Add(Text);
                    Grid.SetRow(Text, 1);

                    Button ApplyButton = new Button();
                    ApplyButton.Visibility = Visibility.Collapsed;
                    ApplyButton.BorderBrush = Brushes.Black;
                    ApplyButton.Name = "btn_" + i.ToString();
                    Binding bind = new Binding();
                    bind.Source = Text;
                    bind.Path = new PropertyPath("Text");
                    ApplyButton.SetBinding(Button.TagProperty, bind);
                    ApplyButton.Content = "Change";
                    ApplyButton.FontSize = 10.0f;
                    ApplyButton.Click += ChangeScaleBtn_OnClick;
                    ApplyButton.Style = (Style)Application.Current.FindResource("CustomButtonStyle");
                    ScaleGrid.Children.Add(ApplyButton);
                    Grid.SetRow(ApplyButton, 2);
                    */
                    #endregion

                    Button ManualScaleButton = new Button();
                    ManualScaleButton.SetBinding(Button.BorderBrushProperty, new Binding("ThemaForeColor"));
                    ManualScaleButton.SetBinding(Button.ForegroundProperty, new Binding("ThemaForeColor"));
                    ManualScaleButton.SetBinding(Button.IsEnabledProperty, new Binding("IsButtonEnabled"));
                    ManualScaleButton.Content = "MANUAL";
                    ManualScaleButton.Name = "ManualBtn_" + i.ToString();
                    ManualScaleButton.Margin = new Thickness(1);
                    ManualScaleButton.FontSize = 8.0f;
                    ManualScaleButton.Click += ManualScaleBtn_OnClick;
                    ManualScaleButton.Style = (Style)Application.Current.FindResource("CustomButtonStyle");
                    ScaleGrid.Children.Add(ManualScaleButton);
                    Grid.SetRow(ManualScaleButton, 1);

                    ColumnDefinition colDef = new ColumnDefinition();
                    colDef.SetBinding(ColumnDefinition.WidthProperty, new Binding("YAxisScaleWidth" + (i + 1).ToString()));
                    subGrid.ColumnDefinitions.Add(colDef);
                    subGrid.VerticalAlignment = VerticalAlignment.Center;
                    subGrid.Margin = new Thickness(0, 0, 0, 0);
                    subGrid.Children.Add(ScaleGrid);
                    Grid.SetColumn(ScaleGrid, i);
                }

                this.SettingAxisGrid.Children.Add(subGrid);
                Grid.SetColumn(subGrid, 0);

                Label dummyLabel = new Label();
                dummyLabel.Width = 20.0d;
                this.myStackPane.Children.Add(dummyLabel);

                if (!IsOnlyYAxis)
                {
                    //X축 다시 그리기
                    DrawXAxisDisplay();
                }
                //Y축만 다시 그리는 경우는 X축을 다시 그리지 않고 기존에 있던 컨트롤에 변경된 넓이만 적용한다.
                else
                {
                    this.MyContent.XAxis.ViewModel.AxisInfoObj.AxisLength = (Int32)(this.MyContent.OscilloscopeWidth);
                    this.myStackPane.Children.Add(this.MyContent.XAxis);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 새로운 사이즈로 조정하여 다시 화면을 그리는 함수
        /// </summary>
        /// <param name="NewSize">새로운 사이즈</param>
        private void SizeUpdate(Size NewSize)
        {
            try
            {
                Double AddYControlWidth = 0.0d;

                //변경된 사이즈 업데이트 해주고
                Double YControlWidth = 0.0d;
                foreach (ShowAxisInformation info in this.MyContent.AxisInfoList)
                {
                    if (!info.IsChannelSelected)
                        AddYControlWidth += 50;
                }

                if (YControlWidth == 0)
                    YControlWidth = 300.0d;

                //if (this.m_MeasurementControlWidth == 0)
                    //this.m_MeasurementControlWidth = 320.0d;

                MyContent.OscilloscopeWidth = (NewSize.Width - (this.m_MeasurementControlWidth + 300)) + AddYControlWidth;
                MyContent.OscilloscopeHeight = ((NewSize.Height / 9) * 8) - 100;

                DrawGraphDisplay();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// UI 초기화 함수
        /// </summary>
        private void InitializeUI()
        {
            try
            {
                /* SMH9999 사이즈 변경될때 로드가 다시 불려져서 시그널 데이터가 날아가는 현상 때문에 지움
                //시그널 데이터 초기화
                foreach (DigitalSignal signal in this.MyContent.DigitalSignalMap.Values)
                    signal.SignalData.Clear();
                */
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Test_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
#if false
                #region 수동 데이터
                Double[] aArray = new Double[5] { 35100, 1000, 72000, 13500, 10500 };
                Double[] bArray = new Double[5] { 0, -600, 24000, 1500, 0 };

                for (int j = 0; j < 5; j++)
                {
                    String Name = this.MyContent.AxisInfoList[j].CurrentSelectedAxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(Name, out type);
                    this.MyContent.DigitalSignalMap[type].SignalData.Clear();

                    Int32 Width = 100000;
                    Double a = aArray[j] / Width;
                    Double b = bArray[j];
                    for (int i = 0; i < 270000; i++)
                    {
                        Int32 idx = i % Width;

                        Signal sig = new Signal();
                        sig.Data = (a * idx) + b;
                        this.MyContent.DigitalSignalMap[type].SignalData.Add(sig);
                    }

                    this.MyContent.DigitalSignalMap[type].StartTime = 0;

                    this.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                        this.MyContent.XAxis.DrawSignalData(overlay, this.MyContent.DigitalSignalMap[type].YAxisInfo.Channel);
                    });
                }
                #endregion
#endif

#if true
                #region 쓰레드 수동 데이터
                this.TestThread = new Thread(ShowManualData);
                this.TestThread.Start();
                #endregion
#endif

#if false
                //this.MyContent.XAxis.TEST();
                this.MyContent.XAxis.GetAutoScaleValue(true);
#endif
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void ShowManualData()
        {
            try
            {
                Int32 count = 0;
                foreach (ShowAxisInformation info in this.MyContent.AxisInfoList)
                {
                    if (count == 1)
                    {
                        if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                        {
                            String Name = info.CurrentSelectedAxisName.Replace(" ", "_");
                            OscilloscopeParameterType type = new OscilloscopeParameterType();
                            Enum.TryParse(Name, out type);
                            this.MyContent.DigitalSignalMap[type].SignalData.Clear();

                            for (int j = 0; j < 10; j++)
                            {
                                Int32 Width = 100000;
                                Double a = (info.AxisInfoObj.MaxValue - (info.AxisInfoObj.TickLength * 2)) / Width;
                                Double b = (info.AxisInfoObj.MinValue + (info.AxisInfoObj.TickLength * 10));
                                for (int i = j * 27000; i < 27000 * (j + 1); i++)
                                {
                                    Int32 idx = i % Width;

                                    Signal sig = new Signal();
                                    sig.Data = (a * idx) + b;
                                    this.MyContent.DigitalSignalMap[type].SignalData.Add(sig);
                                }

                                this.MyContent.DigitalSignalMap[type].StartTime = (j * 27000) / 100;

                                this.Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                                    this.MyContent.XAxis.DrawSignalData(overlay, count);
                                });
                                Thread.Sleep(10);
                            }
                        }
                    }
                    count++;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
