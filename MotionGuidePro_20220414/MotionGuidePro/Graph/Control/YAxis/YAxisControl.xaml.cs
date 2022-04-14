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
    /// YAxisControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class YAxisControl : UserControl
    {
        ///<summary>
        /// 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        private ShowAxisInformation m_AxisInfo;

        private Boolean m_IsManualScale;
        public Boolean IsManualScale
        {
            get { return this.m_IsManualScale; }
            set { this.m_IsManualScale = value; }
        }

        public OscilloscopeContent MyContent { get; set; }

        public DigitalSignal DigitalSignalObj { get; set; }

        public Int32 MyIndex { get; set; }

        public YAxisControl_ViewModel ViewModel { get; set; }

        private Double m_OriginMin;
        private Double m_OriginMax;

        // MouseWheel을 할 때 스케일 값
        private Double m_WheelScale;

        private Double m_ZoomInterval;

        private Double m_MaxZoomRate;

        private Double m_MinZoomRate;

        #region 마우스 이벤트
        //드레그 여부
        public Boolean m_IsDragging;

        //마우스 시작 위치
        public Point m_mouseStartPoint;

        // 항목 시작 위치
        private List<Point> m_itemStartPointArray;

        // 마우스를 클릭했을 때 최소 값
        private Double m_CurrentClickMinValue;

        // 마우스를 클릭했을 때 최대 값
        private Double m_CurrentClickMaxValue;
        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public YAxisControl(OscilloscopeContent Content, Int32 Index)
        {
            InitializeComponent();

            this.m_itemStartPointArray = new List<Point>();

            //Zoom관련 변수 초기화
            this.m_WheelScale = 1.0d;
            this.m_ZoomInterval = 1.2d;
            this.m_MaxZoomRate = 4.0d;
            this.m_MinZoomRate = 0.5d;

            this.m_IsManualScale = false;
            this.MyContent = Content;
            this.MyIndex = Index;

            //ViewModel 설정
            this.ViewModel = new YAxisControl_ViewModel(this);
            this.ViewModel.ContentViewModel = MyContent;
            this.ViewModel.MyIndex = Index;
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;

            //해당 축의 정보
            Content.AxisInfoList[Index].Control = this;
            this.m_AxisInfo = Content.AxisInfoList[Index];

            //Data정보 가져오기
            this.ViewModel.DataInfoObj = (DataInformation)this.m_AxisInfo.DataInfoObj.Clone();
            this.m_OriginMin = this.ViewModel.DataInfoObj.DataMin;
            this.m_OriginMax = this.ViewModel.DataInfoObj.DataMax;

            //Axis정보 채우기
            this.ViewModel.AxisInfoObj.AxisName = this.m_AxisInfo.CurrentSelectedAxisName;
            this.ViewModel.AxisInfoObj.Channel = Index;
            this.ViewModel.AxisInfoObj.TickLength = 40;
            this.ViewModel.AxisInfoObj.MinValue = this.m_OriginMin;
            this.ViewModel.AxisInfoObj.MaxValue = this.m_OriginMax;
            this.m_AxisInfo.AxisInfoObj = this.ViewModel.AxisInfoObj;

            String EnumName = this.m_AxisInfo.CurrentSelectedAxisName.Replace(" ", "_");
            OscilloscopeParameterType type = new OscilloscopeParameterType();
            Enum.TryParse(EnumName, out type);

            DigitalSignalObj = Content.DigitalSignalMap[type];
            DigitalSignalObj.YAxisInfo = this.ViewModel.AxisInfoObj;
        }

        ///<summary>
        /// 함수
        /// </summary>
        // 축을 변경할 때 초기 Min, Max를 다시 설정하는 함수
        public void SetAnotherAxisValueRange(Double Min, Double Max)
        {
            try
            {
                //Origin 값을 변경해준다.
                this.m_OriginMin = Min;
                this.m_OriginMax = Max;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 축을 그릴 정보를 자동으로 구해주는 함수
        public void GetAutoScaleValue()
        {
            try
            {
                //값을 구해주기 위해서 필요한 데이터를 넣고 Run
                AutoScaleTool Tool = new AutoScaleTool(this.MyContent.FitModeValue);
                if (this.ViewModel.AxisVisible == Visibility.Visible)
                {
                    if (this.ViewModel.ControlHeight == 0.0d)
                    {
                        this.ViewModel.AxisInfoObj.AxisLength = (Int32)this.ActualHeight;
                        this.ViewModel.ControlHeight = this.ActualHeight;
                    }
                    else
                        this.ViewModel.AxisInfoObj.AxisLength = (Int32)this.ViewModel.ControlHeight;
                }
                else
                {
                    this.ViewModel.AxisInfoObj.AxisLength = (Int32)this.ActualHeight;
                    this.ViewModel.ControlHeight = this.ActualHeight;
                }
                Tool.MinValue = this.ViewModel.AxisInfoObj.MinValue;
                Tool.MaxValue = this.ViewModel.AxisInfoObj.MaxValue;
                Tool.Length = this.ViewModel.AxisInfoObj.AxisLength;
                Tool.TickLength = this.ViewModel.AxisInfoObj.TickLength;
                Tool.m_TickStartValue = this.ViewModel.AxisInfoObj.StartValue;
                Tool.m_TickCount = this.ViewModel.AxisInfoObj.TickCount;
                Tool.Run();

                //구해진 데이터를 가져온다.
                if(!this.m_IsManualScale)
                    this.ViewModel.AxisInfoObj.TickScale = Tool.TickScale;

                this.ViewModel.AxisInfoObj.StartValue = Tool.TickStartValue;
                this.ViewModel.AxisInfoObj.TickCount = Tool.TickCount;

                this.ViewModel.AxisInfoObj.MaxValue = this.ViewModel.AxisInfoObj.MinValue +
                    (this.ViewModel.AxisInfoObj.TickScale * this.ViewModel.AxisInfoObj.TickCount);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 축 라인을 그려주는 함수
        /// </summary>
        public void DrawAxisLine(Boolean IsManual=false, Int32 StdIndex=0)
        {
            try
            {
                this.ValueTextCanvas.Children.Clear();
                this.LineCanvas.Children.Clear();
                this.ButtonCanvas.Children.Clear();

                Double YAxisControlHeight = this.ViewModel.ControlHeight;

                Double StdValue = 0.0d;
                if (this.ViewModel.TickValues.Count > 0)
                    StdValue = Double.Parse(this.ViewModel.TickValues[StdIndex]);

                this.ViewModel.TickValues.Clear();

                //Tick과 데이터를 표시한다.
                Double lastTickLoc = 0.0d;
                for (int i = 0; i < this.ViewModel.AxisInfoObj.TickCount; i++)
                {
                    //TickLine 정보
                    Path pathTick = new Path();
                    pathTick.Stroke = this.m_AxisInfo.CurrentAxisColor;
                    pathTick.StrokeThickness = 1.0f;
                    LineGeometry lineTick = new LineGeometry();
                    Double YPos = (YAxisControlHeight - 25) - (i * this.ViewModel.AxisInfoObj.TickLength);
                    if (YPos < 0)
                        YPos = 10;
                    //lineTick.StartPoint = new Point((this.ActualWidth / 2) - 5, YPos);
                    //lineTick.EndPoint = new Point(this.ActualWidth / 2 + 5, YPos);
                    lineTick.StartPoint = new Point(-5, YPos);
                    lineTick.EndPoint = new Point(5, YPos);
                    pathTick.Data = lineTick;
                    this.LineCanvas.Children.Add(pathTick);

                    //수동 스케일
                    if (IsManual)
                    {
                        String Value = String.Empty;
                        if (i >= StdIndex)
                        {
                            Value = (StdValue + ((i - StdIndex) * this.ViewModel.AxisInfoObj.TickScale)).ToString("F1");
                            this.ViewModel.TickValues.Add(Value);
                        }
                        else
                        {
                            Value = (StdValue - ((StdIndex - i) * this.ViewModel.AxisInfoObj.TickScale)).ToString("F1");
                            this.ViewModel.TickValues.Add(Value);
                        }
                    }
                    //자동 스케일
                    else
                    {
                        //표시 데이터 값 구하기
                        String Value = (this.ViewModel.AxisInfoObj.MinValue + (i * this.ViewModel.AxisInfoObj.TickScale)).ToString("F1");
                        this.ViewModel.TickValues.Add(Value);
                    }


                    //Canvas에 표시할 데이터 만들어서 붙이기
                    TextBlock Text = new TextBlock();
                    Text.HorizontalAlignment = HorizontalAlignment.Center;
                    Text.VerticalAlignment = VerticalAlignment.Center;
                    Text.FontSize = 12.0f;
                    Text.Foreground = this.m_AxisInfo.CurrentAxisColor;
                    Text.Text = this.ViewModel.TickValues[i];
                    this.ValueTextCanvas.Children.Add(Text);
                    Int32 XLocOffset = (Int32)(Text.FontSize / 2);
                    Int32 YLocOffset = (Int32)(Text.FontSize / 3);
                    //Canvas.SetLeft(Text, (this.ActualWidth / 2) - (viewModel.TickValues[i].Length * XLocOffset) + 18);
                    if(this.ViewModel.TickValues[i].Length == 1)
                        Canvas.SetLeft(Text, 16);
                    else
                        Canvas.SetLeft(Text, 5 - (this.ViewModel.TickValues[i].Length));

                    YPos = ((YAxisControlHeight - 15) - (YLocOffset * 5)) - (i * this.ViewModel.AxisInfoObj.TickLength);
                    if (YPos < 0)
                        YPos = 10;
                    //Canvas.SetTop(Text, YPos);
                    Canvas.SetTop(Text, YPos);

                    //마지막 Tick의 위치를 백업해놓는다.
                    if (i == this.ViewModel.AxisInfoObj.TickCount - 1)
                    {
                        lastTickLoc = (YAxisControlHeight - 80) - (i * this.ViewModel.AxisInfoObj.TickLength);
                        if (lastTickLoc < 0)
                            lastTickLoc = 10;
                    }
                }

                Console.WriteLine();

                if (this.ViewModel.TickValues.Count > 0)
                {
                    this.ViewModel.AxisInfoObj.MinValue = Convert.ToDouble(this.ViewModel.TickValues[0]);
                    this.ViewModel.AxisInfoObj.MaxValue = Convert.ToDouble(this.ViewModel.TickValues[this.ViewModel.AxisInfoObj.TickCount - 1]);
                }

                DigitalSignalObj.DataInformObj = this.ViewModel.DataInfoObj;

                //실린더 붙이기
                Button UpArrowButton = new Button();
                UpArrowButton.Tag = this.m_AxisInfo.DataInfoObj;
                UpArrowButton.Width = 20.0d;
                UpArrowButton.Height = 20.0d;
                //UpArrowButton.Content = "△";
                UpArrowButton.SetBinding(Button.ContentProperty, new Binding("UpArrowShape"));
                UpArrowButton.Click += UpArrowButton_Click;
                UpArrowButton.SetBinding(Button.ForegroundProperty, new Binding("ThemaForeColor"));
                UpArrowButton.SetBinding(Button.BorderBrushProperty, new Binding("ThemaForeColor"));
                UpArrowButton.SetBinding(Button.DataContextProperty, new Binding("ContentViewModel"));
                UpArrowButton.SetBinding(Button.IsEnabledProperty, new Binding("IsButtonEnabled"));
                UpArrowButton.Style = (Style)Application.Current.FindResource("CustomButtonStyle");
                UpArrowButton.FontSize = 10.0f;
                this.ButtonCanvas.Children.Add(UpArrowButton);
                //Canvas.SetLeft(UpArrowButton, (this.ActualWidth / 2) - 10);
                Canvas.SetLeft(UpArrowButton, 0);
                Canvas.SetTop(UpArrowButton, 10);

                Button DownArrowButton = new Button();
                DownArrowButton.Tag = this.m_AxisInfo.DataInfoObj;
                DownArrowButton.Width = 20.0d;
                DownArrowButton.Height = 20.0d;
                DownArrowButton.SetBinding(Button.ContentProperty, new Binding("DownArrowShape"));
                DownArrowButton.Click += DownArrowButton_Click;
                DownArrowButton.SetBinding(Button.ForegroundProperty, new Binding("ThemaForeColor"));
                DownArrowButton.SetBinding(Button.BorderBrushProperty, new Binding("ThemaForeColor"));
                //DownArrowButton.Foreground = Brushes.White;
                //DownArrowButton.BorderBrush = Brushes.White;
                DownArrowButton.SetBinding(Button.DataContextProperty, new Binding("ContentViewModel"));
                DownArrowButton.SetBinding(Button.IsEnabledProperty, new Binding("IsButtonEnabled"));
                DownArrowButton.Style = (Style)Application.Current.FindResource("CustomButtonStyle");
                DownArrowButton.FontSize = 10.0f;
                this.ButtonCanvas.Children.Add(DownArrowButton);
                //Canvas.SetLeft(DownArrowButton, (this.ActualWidth / 2) - 10);
                Canvas.SetLeft(DownArrowButton, 20);
                Canvas.SetTop(DownArrowButton, 10);

                //Y축 라인을 그린다.
                Path path = new Path();
                path.Stroke = this.m_AxisInfo.CurrentAxisColor;
                path.StrokeThickness = 2.0f;
                LineGeometry line = new LineGeometry();
                line.StartPoint = new Point(0, YAxisControlHeight - 25);
                line.EndPoint = new Point(0, lastTickLoc);
                path.Data = line;
                this.LineCanvas.Children.Add(path);

                //viewModel.AxisInfoObj.AxisLength =  (viewModel.AxisInfoObj.TickCount - 1) * viewModel.AxisInfoObj.TickLength;
                //this.MyContent.YAxisLength = viewModel.AxisInfoObj.TickCount * viewModel.AxisInfoObj.TickLength;
                this.ViewModel.AxisInfoObj.AxisLength = (Int32)((YAxisControlHeight - 15) - lastTickLoc);
                this.MyContent.YAxisLength = this.ViewModel.AxisInfoObj.AxisLength;

                this.MyContent.TriggerControl_ViewModel.CanvasMargin = new Thickness(0, lastTickLoc - 5, 0, 0);
                this.MyContent.XAxis.ViewModel.testVal = lastTickLoc;

                #region 축 헤더
                //현재 Line에 대한 이름을 표시한다.
                //Canvas에 표시할 데이터 만들어서 붙이기
                //TextBlock Header = new TextBlock();
                //Header.HorizontalAlignment = HorizontalAlignment.Center;
                //Header.VerticalAlignment = VerticalAlignment.Center;
                //Header.FontSize = 15.0f;
                //Header.Foreground = this.m_AxisInfo.CurrentAxisColor;
                //Header.Text = this.m_AxisInfo.CurrentSelectedAxisName;
                //RotateTransform transForm = new RotateTransform();
                //transForm.Angle = 90;
                //Header.RenderTransform = transForm;
                //this.HeaderCanvas.Children.Add(Header);
                //Canvas.SetLeft(Header, (this.ActualWidth / 2) + Header.FontSize - 18);
                //Canvas.SetTop(Header, (viewModel.AxisInfoObj.AxisLength / 2) - (viewModel.AxisInfoObj.AxisName.Length / 2));
                #endregion
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 사용자가 설정한 스케일에 따라서 다시 축을 그리는 함수
        /// </summary>
        public void RedrawAccording2Scale(Int32 StdIndex)
        {
            try
            {
                this.m_IsManualScale = true;

                //선택된 인덱스에 따라서 다르게 호출
                //0 : 최소 / 최대 값을 이용해 변경
                //1 : Tick 간격을 이용해서 변경
                //if (SelectManualSetIndex == 0)
                //{
                //    this.m_IsManualScale = false;
                //    GetAutoScaleValue();
                //    DrawAxisLine();
                //}
                //else
                DrawAxisLine(true, StdIndex);

                //변경된 스케일로 데이터를 그린다.
                String EnumName = this.ViewModel.AxisInfoObj.AxisName.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);

                SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                if (overlay != null)
                    this.MyContent.XAxis.DrawSignalData(overlay, this.ViewModel.AxisInfoObj.Channel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 사용자가 설정한 드로우 타입에 따라서 다시 축을 그리는 함수
        /// </summary>
        public void RedrawAccording2AutoScale()
        {
            try
            {
                this.m_IsManualScale = false;

                this.ViewModel.AxisInfoObj.MinValue = this.m_OriginMin;
                this.ViewModel.AxisInfoObj.MaxValue = this.m_OriginMax;
                //viewModel.AxisInfoObj.StartValue = this.m_OriginMin;

                GetAutoScaleValue(); 

                //새로 라인을 그린다.
                DrawAxisLine();

                //변경된 스케일로 데이터를 그린다.
                String EnumName = this.ViewModel.AxisInfoObj.AxisName.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);

                SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                if (overlay != null)
                    this.MyContent.XAxis.DrawSignalData(overlay, this.ViewModel.AxisInfoObj.Channel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// SignalDisplayControl에서 MouseDown이벤트가 일어날 때 축의 최소 최대값을 저장하기 위한 함수
        /// </summary>
        public void SetCurrentMinMaxValue()
        {
            try
            {
                this.m_CurrentClickMinValue = this.ViewModel.AxisInfoObj.MinValue;
                this.m_CurrentClickMaxValue = this.ViewModel.AxisInfoObj.MaxValue;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
