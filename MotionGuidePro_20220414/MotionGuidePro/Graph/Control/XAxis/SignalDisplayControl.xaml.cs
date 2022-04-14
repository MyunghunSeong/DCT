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
using MotionGuidePro.Main;

namespace DCT_Graph
{
    /// <summary>
    /// SignalDisplayControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SignalDisplayControl : UserControl
    {
        /// <summary>
        /// 에러 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        public Double m_lastXPos = 0.0d;

        public OscilloscopeContent MyContent { get; set; }

        public Canvas[] m_CanvasArray;

        //TEST 20220217
        public System.Windows.Shapes.Path[] m_PathArray;
        //public Path DisplayPath = new Path();

        public SignalDisplayControl_ViewModel ViewModel { get; set; }

        #region 마우스 이벤트
        //드레그 여부
        private Boolean m_IsDragging;

        //마우스 시작 위치
        private Point m_mouseStartPoint;

        // 각 좌표들(시그날) 시작 위치
        private List<Point> m_PointStartPointArray;

        // 시간 데이터 시작 위치
        private List<Point> m_TimeStartPointArray;

        // 각 축의 데이터 시작 위치
        private List<Point>[] m_DataStartPointListArray;

        // Y축들 좌표 데이터 값
        private List<Double> m_SelectedYPosDataList;

        // 이동 / 줌에 대해서 새로 그릴 overlay
        private SignalOverlay[] m_RecvedOverlayArray;

        //MouseDown일 때 좌표 값
        private List<Point>[] m_CurrentClickPointListArray;

        //MouseDown일 때 시작 값
        private Double m_CurrentClickStartTime;

        // MouseWheel을 할 때 스케일 값
        public Double m_WheelScale;

        private Double m_ZoomInterval;

        private Double m_MaxZoomRate;

        private Double m_MinZoomRate;

        private Double m_OriginScaleValue;
        #endregion

        public SignalDisplayControl(OscilloscopeContent Content)
        {
            InitializeComponent();

            this.MyContent = Content;

            this.m_WheelScale = 1.0d;
            this.m_ZoomInterval = 1.2d;
            this.m_MaxZoomRate = 4.0d;
            this.m_MinZoomRate = 0.5d;
            this.m_CurrentClickPointListArray = new List<Point>[5];
            this.m_SelectedYPosDataList = new List<Double>();
            this.m_PointStartPointArray = new List<Point>();
            this.m_DataStartPointListArray = null;
            this.m_TimeStartPointArray = new List<Point>();
            this.m_RecvedOverlayArray = new SignalOverlay[5];

            //ViewModel 설정
            this.ViewModel = new SignalDisplayControl_ViewModel(this);
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;

            this.ViewModel.ContentViewModel = this.MyContent;
            this.ViewModel.MainViewModel = PublicVar.MainWnd.ViewModel;

            this.ViewModel.GridInfoObj.Width = 40;
            this.ViewModel.GridInfoObj.Height = 40;

            this.ViewModel.AxisInfoObj.AxisName = "Time";
            this.ViewModel.AxisInfoObj.Channel = 0;
            this.ViewModel.AxisInfoObj.AxisLength = (Int32)(this.MyContent.OscilloscopeWidth);
            this.ViewModel.AxisInfoObj.TickLength = 40;
            this.ViewModel.AxisInfoObj.TickScale = 1000;
            this.m_OriginScaleValue = 1000;

            foreach (ShowAxisInformation info in this.MyContent.AxisInfoList)
            {
                String EnumName = info.CurrentSelectedAxisName.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);
                this.MyContent.DigitalSignalMap[type].XAxisInfo = this.ViewModel.AxisInfoObj;
            }
        }

        public void RedrawByArrowButton(Double MovePixcel, Int32 Index, Boolean IsUp)
        {
            try
            {
                SignalOverlay overlay = this.m_RecvedOverlayArray[Index];
                if (overlay != null && overlay.Points.Count > 0)
                {
                    #region 시그널 변화
                    for (int i = 0; i < overlay.Points.Count; i++)
                    {
                        Double newYPos = (IsUp) ? overlay.Points[i].Y - MovePixcel : overlay.Points[i].Y + MovePixcel;
                        Point newPoint = new Point(overlay.Points[i].X, newYPos);
                        overlay.Points[i] = newPoint;
                    }
                    #endregion
                }
                DrawSignalData(overlay, Index);
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<summary>
        /// 트리거 라인 그리기
        /// </summary>
        public void DrawTriggerLine()
        {
            try
            {
                //트리거 축의 넓이를 현재 캔버스의 넓이로 설정
                Double ControlWidth = this.MyContent.OscilloscopeWidth;
                this.TriggerCanvas.Children.Clear();

                TriggerLineControl TriggerLine = new TriggerLineControl(this.MyContent.TriggerObj, ControlWidth);
                this.TriggerCanvas.Children.Add(TriggerLine);
                if ((TriggerLine.ViewModel).LastPos.X == 0
                    && (TriggerLine.ViewModel).LastPos.Y == 0)
                    (TriggerLine.ViewModel).LastPos = new Point(0, this.ViewModel.AxisInfoObj.AxisLength / 4);
                Canvas.SetLeft(TriggerLine, (this.MyContent.TriggerObj.ViewModel).LastPos.X);
                Canvas.SetTop(TriggerLine, (this.MyContent.TriggerObj.ViewModel).LastPos.Y);
                //TriggerLine.SetBinding(Canvas.LeftProperty, new Binding("LastPos.X"));
                //TriggerLine.SetBinding(Canvas.TopProperty, new Binding("LastPos.Y"));

                TriggerLine.ControlYPos = Canvas.GetTop(TriggerLine);
                this.MyContent.TriggerObj.TriggerLine = TriggerLine;

                this.MyContent.TriggerObj.TriggerLine.mainGrid.Background = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.Black : new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                this.MyContent.TriggerObj.TriggerLine.line.Stroke = (PublicVar.MainWnd.ViewModel.ThemaTypeValue == ThemaType.BLACK_THEMA) ? Brushes.White : Brushes.Black;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<summary>
        /// 커서 그리기
        /// </summary>
        public void DrawCursor()
        {
            try
            {
                /*
                this.CursorCanvas.Children.Clear();

                #region 커서 그룹1
                //Double[] tmp = new Double[2] { 120, 320 };
                //OscilloscopeContent에 있는 CursorGroup으로 Cursor를 추가한다.
                for (int i = 0; i < this.MyContent.CursorGroup1.Length; i++)
                {
                    if (this.MyContent.CursorGroup1[i] == null)
                    {
                        CursorControl cursor = new CursorControl(this.MyContent, this);
                        cursor.Name = "Cursor" + (i + 1);
                        cursor.Visibility = (this.MyContent.IsCheckedCursor1) ? Visibility.Visible : Visibility.Collapsed;
                        //커서의 높이
                        //Cursor.AxisLength = 현재Y축 높이로 설정
                        (cursor.ViewModel).DashArray  = DoubleCollection.Parse("3, 5, 3, 5");
                        this.MyContent.CursorGroup1[i] = cursor;
                    }
                    else
                    {
                        if (this.MyContent.CursorGroup1[i].Parent != null)
                            (this.MyContent.CursorGroup1[i].Parent as Canvas).Children.Clear();
                    }

                    //커서를 그릴때 필요한 데이터
                    //초기 커서의 위치
                    String EnumName = this.MyContent.AxisInfoList[1].CurrentSelectedAxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(EnumName, out type);
                    //X1은 3000ms, X2는 8000ms
                    //CursorPosX의 초기값은 -1로 초기에만 3000(+4000ms)으로 설정한다.
                    Int32 InitTime = (i == 0) ? 3000 : 8000;
                    if ((this.MyContent.CursorGroup1[i].ViewModel).CursorPosX < 0)
                    {
                        (this.MyContent.CursorGroup1[i].ViewModel).CursorPosX
                            = this.MyContent.DigitalSignalMap[type].TimeToPixel(InitTime);
                        Console.WriteLine();
                    }
                    else
                    {
                        //현재 좌표위치를 통해서 시간을 구한다.
                        Double CurrentTime = this.MyContent.DigitalSignalMap[type].PixcelToTimeByDouble(
                            (this.MyContent.CursorGroup1[i].ViewModel).CursorPosX);

                        //위에서 구한 시간으로 좌표 위치를 업데이트 해준다.
                        (this.MyContent.CursorGroup1[i].ViewModel).CursorPosX 
                            =this.MyContent.DigitalSignalMap[type].TimeToPixel(CurrentTime);
                        this.MyContent.CursorGroup1[i].Update(0);
                    }

                    //this.CursorCanvas에 커서를 추가한다.
                    this.CursorCanvas.Children.Add(this.MyContent.CursorGroup1[i]);
                    //this.MyContent.CursorGroup1[i].SetBinding(Canvas.LeftProperty, new Binding("CursorPosX"));
                    Canvas.SetLeft(this.MyContent.CursorGroup1[i], (this.MyContent.CursorGroup1[i].ViewModel).CursorPosX);
                    //Canvas.SetLeft(this.MyContent.CursorGroup1[i], tmp[i]);
                    Canvas.SetTop(this.MyContent.CursorGroup1[i], 0);
                }
                #endregion

                #region 커서 그룹2
                //tmp = new Double[2] { 600, 800 };
                //OscilloscopeContent에 있는 CursorGroup으로 Cursor를 추가한다.
                for (int i = 0; i < this.MyContent.CursorGroup2.Length; i++)
                {
                    if (this.MyContent.CursorGroup2[i] == null)
                    {
                        CursorControl cursor = new CursorControl(this.MyContent, this);
                        cursor.Name = "Cursor" + (i + 3);
                        cursor.Visibility = (this.MyContent.IsCheckedCursor2) ? Visibility.Visible : Visibility.Collapsed;
                        //커서의 높이
                        //Cursor.AxisLength = 현재Y축 높이로 설정
                        (cursor.ViewModel).DashArray = DoubleCollection.Parse("1, 2, 1, 2, 1, 2, 1, 2");
                        this.MyContent.CursorGroup2[i] = cursor;
                    }
                    else
                    {
                        if (this.MyContent.CursorGroup2[i].Parent != null)
                            (this.MyContent.CursorGroup2[i].Parent as Canvas).Children.Clear();
                    }

                    //초기 커서의 위치
                    String EnumName = this.MyContent.AxisInfoList[1].CurrentSelectedAxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(EnumName, out type);
                    //X1은 15000ms, X2는 000ms
                    //CursorPosX의 초기값은 -1로 초기에만 3000(+4000ms)으로 설정한다.
                    Int32 InitTime = (i == 0) ? 15000 : 20000;
                    if ((this.MyContent.CursorGroup2[i].ViewModel).CursorPosX < 0)
                    {
                        (this.MyContent.CursorGroup2[i].ViewModel).CursorPosX
                            = this.MyContent.DigitalSignalMap[type].TimeToPixel(InitTime);
                    }
                    else
                    {
                        //현재 좌표위치를 통해서 시간을 구한다.
                        Double CurrentTime = this.MyContent.DigitalSignalMap[type].PixcelToTimeByDouble(
                            (this.MyContent.CursorGroup2[i].ViewModel).CursorPosX);

                        //위에서 구한 시간으로 좌표 위치를 업데이트 해준다.
                        (this.MyContent.CursorGroup2[i].ViewModel).CursorPosX
                            = this.MyContent.DigitalSignalMap[type].TimeToPixel(CurrentTime);
                        this.MyContent.CursorGroup2[i].Update(0);
                    }

                    //this.CursorCanvas에 커서를 추가한다.
                    this.CursorCanvas.Children.Add(this.MyContent.CursorGroup2[i]);
                    Canvas.SetLeft(this.MyContent.CursorGroup2[i], (this.MyContent.CursorGroup2[i].ViewModel).CursorPosX);
                    //Canvas.SetLeft(this.MyContent.CursorGroup2[i], tmp[i]);
                    Canvas.SetTop(this.MyContent.CursorGroup2[i], 0);
                }
                #endregion
                */
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 구한 좌표 값을 이용해 캔버스에 그리는 함수
        /// </summary>
        public void DrawSignalData(SignalOverlay overlay, Int32 chIndex, Boolean IsFromMove=false)
        {
            try
            {
#if false
                if (overlay == null || overlay.Points.Count <= 0)
                    return;

                this.LineCanvas.Children.Remove(m_PathArray[chIndex]);

                this.m_RecvedOverlayArray[chIndex] = overlay;

                PathFigure pathFig = new PathFigure();
                pathFig.StartPoint = overlay.Points[0];
                PathSegmentCollection pathCollect = new PathSegmentCollection();

                //라인 세그먼트
                //overlay.Points.Count
                for (int i = 0; i < overlay.Points.Count; i++)
                {
                    LineSegment line = new LineSegment();
                    line.IsSmoothJoin = true;
                    line.Point = overlay.Points[i];
                    pathCollect.Add(line);
                }

                pathFig.Segments = pathCollect;

                PathFigureCollection pathfigureCollect = new PathFigureCollection();
                pathfigureCollect.Add(pathFig);

                PathGeometry myGeometry = new PathGeometry();
                myGeometry.Figures = pathfigureCollect;

                this.m_PathArray[chIndex].StrokeThickness = 2.0f;
                this.m_PathArray[chIndex].Stroke = overlay.DrawColor;
                this.m_PathArray[chIndex].Data = myGeometry;

                this.m_PathArray[chIndex].Margin = new Thickness(0, 7, 0, 0);
                this.m_PathArray[chIndex].Visibility = (this.MyContent.YAxisList[chIndex].ViewModel).AxisVisible;
                this.m_PathArray[chIndex].ClipToBounds = true;
                this.LineCanvas.Children.Add(this.m_PathArray[chIndex]);
#endif
#if true
                this.m_CanvasArray[chIndex].Children.Clear();
                this.m_CanvasArray[chIndex].ClipToBounds = true;

                if (overlay == null || overlay.Points.Count <= 0)
                    return;

                this.m_RecvedOverlayArray[chIndex] = overlay;

                PathFigure pathFig = new PathFigure();
                pathFig.StartPoint = overlay.Points[0];
                PathSegmentCollection pathCollect = new PathSegmentCollection();

                //라인 세그먼트
                for (int i = 0; i < overlay.Points.Count; i++)
                {
                    LineSegment line = new LineSegment();
                    line.IsSmoothJoin = true;
                    line.Point = overlay.Points[i];
                    pathCollect.Add(line);
                }

                pathFig.Segments = pathCollect;

                PathFigureCollection pathfigureCollect = new PathFigureCollection();
                pathfigureCollect.Add(pathFig);

                PathGeometry myGeometry = new PathGeometry();
                myGeometry.Figures = pathfigureCollect;

                Path path = new Path();
                path.StrokeThickness = 3.0f;
                path.Stroke = this.MyContent.BrushColorArray[chIndex];
                path.Data = myGeometry;

                //path.Visibility = (this.MyContent.YAxisList[chIndex].ViewModel).AxisVisible;

                this.m_CanvasArray[chIndex].Children.Add(path);
#endif
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        //TEST
        public void TEST()
        {
            try
            {
                //이동한 시간
                Double TimeAccording2Moving = 12.5 * 400;

                //새로운 시간
                this.ViewModel.AxisInfoObj.StartValue = 0 - (TimeAccording2Moving);

                //시간 그리기
                this.GetAutoScaleValue(true, true);

                Int32 count = 0;
                foreach (ShowAxisInformation info in this.MyContent.AxisInfoList)
                {
                    SignalOverlay overlay = this.MyContent.DigitalSignalMap[info.ParamType].GetOverlay();
                    /*
                    if (info.IsChannelSelected && info.CurrentSelectedAxisIndex > 0)
                    {
                        SignalOverlay overlay = this.MyContent.DigitalSignalMap[info.ParamType].GetOverlay();
                        Double FirstTime = this.MyContent.DigitalSignalMap[info.ParamType].PixcelToTimeByDouble(overlay.Points[0].X);
                        if (FirstTime > 0)
                        {
                            Point[] ptArr = new Point[Convert.ToInt32(FirstTime)];
                            Signal[] sigArr = new Signal[ptArr.Length];
                            Array.Copy(this.MyContent.DigitalSignalMap[info.ParamType].SignalData.ToArray(),
                                0, sigArr, 0, sigArr.Length);

                            for (int i = 0; i < sigArr.Length; i++)
                            {
                                Double PixcelX = this.MyContent.DigitalSignalMap[info.ParamType].TimeToPixel(i);
                                Double PixcelY = this.MyContent.DigitalSignalMap[info.ParamType].PixcelToData(sigArr[i].Data);
                                ptArr[i] = new Point(PixcelX, PixcelY);
                            }

                            overlay.Points.InsertRange(0, ptArr);
                        }*/
                    DrawSignalData(overlay, count++, true);
                    //}
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        // 축을 그릴 정보를 자동으로 구해주는 함수
        public void GetAutoScaleValue(Boolean IsReDrawData=true, Boolean IsFromMove=false, Boolean IsFromZoom=false)
        {
            try
            {
                this.LineCanvas.Children.Clear();
                this.ValueCanvas.Children.Clear();
                this.ViewModel.GridInfoObj.Width = 40;
                this.ViewModel.GridInfoObj.Height = 40;

                ////데이터 넣기
                //for (int i = 0; i < this.m_PathArray.Length; i++)
                //{
                //    //데이터가 NULL이 아닌 경우에 Path를 넣어준다.
                //    if (this.m_PathArray[i] != null)
                //        this.LineCanvas.Children.Add(this.m_PathArray[i]);
                //}

                //그리드의 칸 개수 구하기
                GridCalibrationTool Tool = new GridCalibrationTool(this.ViewModel.GridInfoObj,
                    (Int32)this.MyContent.OscilloscopeWidth, (Int32)this.ActualHeight);
                Tool.Run(this.MyContent.XAxisScaleValue);

                this.ViewModel.AxisInfoObj.TickLength = 40 * this.MyContent.XAxisScaleValue;

                //구한 값 가져오기
                this.ViewModel.GridInfoObj.RowCount = Tool.RowCount;
                this.ViewModel.GridInfoObj.ColumnCount = Tool.ColumnCount;
                this.ViewModel.AxisInfoObj.TickCount = Tool.ColumnCount;

                //X라인 그리기
                DrawXAxisLine(IsFromZoom);

                //Y라인 그리기
                DrawYAxisLine();

                //다시 그리는 경우에만 시그널 데이터를 그린다.
                if (IsReDrawData)
                {
                    Int32 count = 0;
                    foreach (ShowAxisInformation info in this.MyContent.AxisInfoList)
                    {
                        if (info.IsChannelSelected)
                        {
                            String EnumName = info.CurrentSelectedAxisName.Replace(" ", "_");
                            OscilloscopeParameterType type = new OscilloscopeParameterType();
                            Enum.TryParse(EnumName, out type);

                            this.MyContent.DigitalSignalMap[type].XAxisInfo.TickLength = this.ViewModel.AxisInfoObj.TickLength;
                            SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                            this.DrawSignalData(overlay, count, IsFromMove);
                        }
                        count++;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DrawYAxisLine()
        {
            try
            {
                //this.DummyYLineCanvas.Children.Clear();
                Double Height = (this.MyContent.YAxisList[0].ViewModel).AxisInfoObj.AxisLength;

                //X축의 마지막 Tick의 위치를 구한다.
                Double LastXPos = 0.0d;
                Double InitLastXPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.ColumnCount; i++)
                {
                    //마지막 X축 Tick의 위치정보를 가지고 있는다.
                    if (i == this.ViewModel.GridInfoObj.ColumnCount - 1)
                    {
                        //InitLastXPos = i * 40 + (this.ActualWidth / 2);
                        Double EndTime = 1000 * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / this.ViewModel.AxisInfoObj.TickLength);
                        Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                        Double Offset_X = TimeScale * 0;
                        Double TickPixcel = ((i * 1000) * TimeScale) - Offset_X;
                        //InitLastXPos = TickPixcel; //i * 40;
                        //InitLastXPos = TickPixcel;

                        InitLastXPos = i * 40;
                        LastXPos = this.m_lastXPos;
                    }
                }

                //Double lastXPos = 0.0d;
                //for (int i = 0; i < (viewModel.GridInfoObj.ColumnCount / this.MyContent.XAxisScaleValue); i++)
                //{
                //    //마지막 X축 Tick의 위치정보를 가지고 있는다.
                //    if (i == (viewModel.GridInfoObj.ColumnCount / this.MyContent.XAxisScaleValue) - 1)
                //    {
                //        //lastXPos = i * viewModel.GridInfoObj.Width + (this.ActualWidth / 2);
                //        lastXPos = i * viewModel.GridInfoObj.Width + 20;
                //    }
                //}

                //Y축 그리기
                Double lastYPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.RowCount; i++)
                {
                    //Y축 Tick 정보
                    Path pathDummyTick = new Path();
                    Path pathTick = new Path();
                    pathTick.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                    pathTick.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                    //pathTick.Stroke = Brushes.White;
                    pathDummyTick.Stroke = Brushes.Black;
                    pathTick.StrokeThickness = 1.0f;
                    pathDummyTick.StrokeThickness = 1.0f;
                    LineGeometry lineTick = new LineGeometry();
                    LineGeometry lineDummyTick = new LineGeometry();
                    if (i == this.ViewModel.GridInfoObj.RowCount - 1)
                        Console.WriteLine();
                    Double YPos = (this.ActualHeight - 25) - (i * this.ViewModel.GridInfoObj.Height);
                    if (YPos < 0)
                        YPos = 10;
                    //lineTick.StartPoint = new Point((this.ActualWidth / 2) - 5, YPos);
                    //lineTick.EndPoint = new Point((this.ActualWidth / 2) + 5, YPos);
                    lineTick.StartPoint = new Point(-5, YPos);
                    lineTick.EndPoint = new Point(5, YPos);
                    lineDummyTick.StartPoint = new Point(15, YPos);
                    lineDummyTick.EndPoint = new Point(25, YPos);
                    pathTick.Data = lineTick;
                    pathDummyTick.Data = lineDummyTick;
                    this.LineCanvas.Children.Add(pathTick);
                    //this.DummyYLineCanvas.Children.Add(pathDummyTick);

                    //Y축 마지막 Tick 위치 정보를 가지고 있는다.
                    if (i == this.ViewModel.GridInfoObj.RowCount - 1)
                    {
                        lastYPos = (this.ActualHeight - 80) - (i * this.ViewModel.GridInfoObj.Height);
                        //lastYPos = (Height - 60) - (i * viewModel.GridInfoObj.Height);
                        if (lastYPos < 0)
                            lastYPos = 10;
                    }

                    //X축 그리드 라인 그리기
                    System.Windows.Shapes.Path pathXGrid = new System.Windows.Shapes.Path();
                    pathXGrid.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                    pathXGrid.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                    //pathXGrid.Stroke = Brushes.White;
                    pathXGrid.Opacity = 0.7d;
                    pathXGrid.StrokeThickness = 1.0f;
                    pathXGrid.StrokeDashArray = DoubleCollection.Parse("2, 2");
                    LineGeometry lineGrid = new LineGeometry();
                    YPos = (this.ActualHeight - 25) - (i * this.ViewModel.GridInfoObj.Height);
                    //YPos =  (Height - 60) - (i * viewModel.GridInfoObj.Height);
                    if (YPos < 0)
                        YPos = 10;

                    //lineGrid.StartPoint = new Point((this.ActualWidth / 2) - 5, YPos);
                    lineGrid.StartPoint = new Point(0, YPos);
                    lineGrid.EndPoint = new Point(LastXPos, YPos);
                    //lineGrid.EndPoint = new Point(this.MyContent.OscilloscopeWidth, YPos);
                    pathXGrid.Data = lineGrid;
                    this.LineCanvas.Children.Add(pathXGrid);
                }

                //viewModel.ParentsCanvasHeight = viewModel.GridInfoObj.Height * viewModel.GridInfoObj.RowCount;
                //viewModel.PointGridHeight = viewModel.GridInfoObj.Height * viewModel.GridInfoObj.RowCount - 10;

                //Y축 라인을 그린다.
                Path yDummyPath = new Path();
                Path yPath = new Path();
                yPath.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                yPath.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                //yPath.Stroke = Brushes.White;
                yDummyPath.Stroke = Brushes.Black;
                yPath.StrokeThickness = 2.0f;
                yDummyPath.StrokeThickness = 2.0f;
                LineGeometry line = new LineGeometry();
                LineGeometry DummyLine = new LineGeometry();
                //line.StartPoint = new Point((this.ActualWidth / 2), this.ActualHeight - 60);
                //line.EndPoint = new Point((this.ActualWidth / 2), lastYPos);
                line.StartPoint = new Point(0, this.ActualHeight - 25);
                DummyLine.StartPoint = new Point(20, this.ActualHeight - 40);
                //line.StartPoint = new Point(20, (Height - 60));
                line.EndPoint = new Point(0, lastYPos);
                DummyLine.EndPoint = new Point(20, lastYPos);
                yPath.Data = line;
                yDummyPath.Data = DummyLine;
                this.LineCanvas.Children.Add(yPath);
                //this.DummyYLineCanvas.Children.Add(yDummyPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ChangeStartTime()
        {
            try
            {
                this.ValueCanvas.Children.Clear();

                this.ViewModel.GridInfoObj.Width = 40;
                this.ViewModel.GridInfoObj.Height = 40;

                //그리드의 칸 개수 구하기
                GridCalibrationTool Tool = new GridCalibrationTool(this.ViewModel.GridInfoObj,
                    (Int32)this.MyContent.OscilloscopeWidth, (Int32)this.ActualHeight);
                Tool.Run(this.MyContent.XAxisScaleValue);

                this.ViewModel.AxisInfoObj.TickLength = 40 * this.MyContent.XAxisScaleValue;

                //구한 값 가져오기
                this.ViewModel.GridInfoObj.RowCount = Tool.RowCount;
                this.ViewModel.GridInfoObj.ColumnCount = Tool.ColumnCount;
                this.ViewModel.AxisInfoObj.TickCount = Tool.ColumnCount;

                this.MyContent.TimeValueList.Clear();

                this.ViewModel.GridInfoObj.Width = 40;
                this.ViewModel.GridInfoObj.Height = 40;

                Double lastXPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.ColumnCount; i++)
                {
                    //마지막 X축 Tick의 위치정보를 가지고 있는다.
                    if (i == this.ViewModel.GridInfoObj.ColumnCount - 1)
                    {
                        Double EndTime = 1000 * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / this.ViewModel.AxisInfoObj.TickLength);
                        Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                        Double Offset_X = TimeScale * 0;
                        Double TickPixcel = ((i * 1000) * TimeScale) - Offset_X;
                        lastXPos = TickPixcel;
                        //lastXPos = i * 40;
                    }
                }

                Double HeaderLastPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.ColumnCount / 2; i++)
                {
                    Double EndTime = this.ViewModel.AxisInfoObj.TickScale * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / 80);
                    Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                    Double TickPixcel = ((i * this.ViewModel.AxisInfoObj.TickScale) * TimeScale);

                    if (i == (this.ViewModel.GridInfoObj.ColumnCount / this.MyContent.XAxisScaleValue) - 1)
                    {
                        if (TickPixcel > lastXPos)
                            break;
                    }

                    //시간 Value값 넣기
                    //시간 Value값 넣기
                    Double Time = 0;
                    if (this.MyContent.Timeunit.Equals("ms"))
                    {
                        Double Offset = this.MyContent.TickScaleValue;
                        Time = this.ViewModel.AxisInfoObj.StartValue + (i * Offset);
                        this.MyContent.TimeValueList.Add(Time.ToString("F1"));
                        this.ViewModel.XAxisTimeValueOffset = 10;
                    }

                    TextBlock Text = new TextBlock();
                    Text.HorizontalAlignment = HorizontalAlignment.Center;
                    Text.VerticalAlignment = VerticalAlignment.Center;
                    Text.FontSize = 12.0f;
                    Text.Foreground = Brushes.Black;
                    Text.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    Text.SetBinding(TextBlock.TextProperty, new Binding("TimeValueList[" + i + "]")
                    { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    this.ValueCanvas.Children.Add(Text);
                    Int32 XLocOffset = (Int32)(Text.FontSize / 2);
                    Int32 YLocOffset = (Int32)(Text.FontSize / 3);
                    //Canvas.SetLeft(Text, ((i * viewModel.GridInfoObj.Width) +
                    //    (this.ActualWidth / 2) - (this.MyContent.TimeValueList[i].Length * XLocOffset) + viewModel.XAxisTimeValueOffset));
                    //Canvas.SetLeft(Text, ((i * 80) - (this.MyContent.TimeValueList[i].Length * XLocOffset) + this.ViewModel.XAxisTimeValueOffset));
                    Canvas.SetLeft(Text, (TickPixcel - (this.MyContent.TimeValueList[i].Length * XLocOffset) + this.ViewModel.XAxisTimeValueOffset));
                    Canvas.SetTop(Text, 0);

                    //시간 헤더 정보 넣기
                    TextBlock HeaderText = new TextBlock();
                    HeaderText.HorizontalAlignment = HorizontalAlignment.Center;
                    HeaderText.VerticalAlignment = VerticalAlignment.Center;
                    HeaderText.FontSize = 15.0f;
                    HeaderText.Foreground = Brushes.Black;
                    HeaderText.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    HeaderText.SetBinding(TextBlock.TextProperty, new Binding("XAxisHeader"));
                    this.ValueCanvas.Children.Add(HeaderText);
                    XLocOffset = (Int32)(Text.FontSize / 2);
                    YLocOffset = (Int32)(Text.FontSize / 3);
                    HeaderLastPos = this.MyContent.OscilloscopeWidth / 2; //(lastXPos - (lastXPos / 2));
                    Canvas.SetLeft(HeaderText, HeaderLastPos);
                    HeaderLastPos = HeaderLastPos + HeaderText.Text.Length + 30;
                    Canvas.SetTop(HeaderText, 20);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DrawXAxisLine(Boolean IsFromZoom = false)
        {
            try
            {
                //this.DummyXLineCanvas.Children.Clear();
                Double Height = (this.MyContent.YAxisList[0].ViewModel).AxisInfoObj.AxisLength;

                //Y축의 마지막 Tick의 위치를 구한다.
                Double lastYPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.RowCount; i++)
                {
                    if (i == this.ViewModel.GridInfoObj.RowCount - 1)
                    {
                        lastYPos = (this.ActualHeight - 80) - (i * this.ViewModel.GridInfoObj.Height);
                        if (lastYPos < 0)
                            lastYPos = 10;
                    }
                }

                Double lastXPos = 0.0d;
                Int32 ColCount = this.ViewModel.GridInfoObj.ColumnCount;
                for (int i = 0; i < ColCount; i++)
                {
                    if ((i * 40) > this.MyContent.OscilloscopeWidth)
                        break;

                    //마지막 X축 Tick의 위치정보를 가지고 있는다.
                    if (i == ColCount - 1)
                    {
                        //lastXPos = i * 40 + (this.ActualWidth / 2);
                        //Double EndTime = viewModel.AxisInfoObj.TickScale * (Int32)(viewModel.AxisInfoObj.AxisLength / viewModel.AxisInfoObj.TickLength);
                        //Double EndTime = 1000 * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / this.ViewModel.AxisInfoObj.TickLength);
                        Double EndTime = this.ViewModel.AxisInfoObj.StartValue + this.ViewModel.AxisInfoObj.TickScale *
                            (Int32)(this.ViewModel.AxisInfoObj.AxisLength / this.ViewModel.AxisInfoObj.TickLength);
                        Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                        Double Offset_X = TimeScale * 0;
                        Double TickPixcel = ((i * 1000) * TimeScale) - Offset_X;
                        lastXPos = i * 40;
                        //lastXPos = TickPixcel;
                    }
                }
                this.ViewModel.PointGridWidth = lastXPos;
                this.ViewModel.ParentsCanvasWidth = lastXPos;
                this.ViewModel.PointGridHeight = this.MyContent.YAxisLength;

                Double EndLength = this.ViewModel.AxisInfoObj.TickScale * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / 80);
                Double Scale = this.ViewModel.AxisInfoObj.AxisLength / (EndLength - 0);
                this.ViewModel.AxisInfoObj.TickLength = (this.ViewModel.AxisInfoObj.TickScale * Scale) * (this.MyContent.XAxisScaleValue / 2.0);

                //X축 그리기
                this.MyContent.TimeValueList.Clear();
                Double HeaderLastPos = 0.0d;
                for (int i = 0; i < this.ViewModel.GridInfoObj.ColumnCount / 2; i++)
                {
                    Double EndTime = this.ViewModel.AxisInfoObj.TickScale * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / 80);
                    Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                    Double TickPixcel = ((i * this.ViewModel.AxisInfoObj.TickScale) * TimeScale);

                    //builder2.AppendLine(i + "번째 TickPixcel : " + TickPixcel);

                    if (i == (this.ViewModel.GridInfoObj.ColumnCount / this.MyContent.XAxisScaleValue) - 1)
                    {
                        if (TickPixcel > lastXPos)
                            break;
                    }

                    //X축 Tick 라인 정보
                    Path pathDummyTick = new Path();
                    Path pathTick = new Path();
                    pathTick.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                    pathTick.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                    //pathTick.Stroke = Brushes.White;
                    pathDummyTick.Stroke = Brushes.Black;
                    pathTick.StrokeThickness = 1.0f;
                    pathDummyTick.StrokeThickness = 1.0f;
                    LineGeometry lineTick = new LineGeometry();
                    LineGeometry lineDummyTick = new LineGeometry();
                    //lineTick.StartPoint = new Point((i * 80), (this.ActualHeight - 60) - 3);
                    //lineTick.EndPoint = new Point((i * 80), (this.ActualHeight - 60) + 3);
                    lineTick.StartPoint = new Point(TickPixcel, (this.ActualHeight - 25) - 3);
                    lineTick.EndPoint = new Point(TickPixcel, (this.ActualHeight - 25) + 3);
                    lineDummyTick.StartPoint = new Point(TickPixcel, -13);
                    lineDummyTick.EndPoint = new Point(TickPixcel, -7);
                    pathTick.Data = lineTick;
                    pathDummyTick.Data = lineDummyTick;
                    this.LineCanvas.Children.Add(pathTick);
                    //this.DummyXLineCanvas.Children.Add(pathDummyTick);
                    //시간 Value값 넣기
                    Double Time = 0;
                    if (this.MyContent.Timeunit.Equals("ms"))
                    {
                        Double Offset = (IsFromZoom) ? this.ViewModel.AxisInfoObj.TickScale : this.MyContent.TickScaleValue / this.MyContent.WheelScale;
                        Time = this.ViewModel.AxisInfoObj.StartValue + (i * Offset);
                        this.MyContent.TimeValueList.Add(Time.ToString("F1"));
                        this.ViewModel.XAxisTimeValueOffset = 10;
                    }
                    else
                    {
                        Time = this.ViewModel.AxisInfoObj.StartValue + (i * this.MyContent.TickScaleValue);
                        Time = Time / 1000;
                        this.MyContent.TimeValueList.Add(((Time)).ToString());
                        this.ViewModel.XAxisTimeValueOffset = 0;
                    }

                    TextBlock Text = new TextBlock();
                    Text.HorizontalAlignment = HorizontalAlignment.Center;
                    Text.VerticalAlignment = VerticalAlignment.Center;
                    Text.FontSize = 10.0f;
                    Text.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    Text.SetBinding(TextBlock.ForegroundProperty, new Binding("ThemaForeColor"));
                    //Text.Foreground = Brushes.White;
                    Text.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    Text.SetBinding(TextBlock.TextProperty, new Binding("TimeValueList[" + i + "]")
                    { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    this.ValueCanvas.Children.Add(Text);
                    Int32 XLocOffset = (Int32)(Text.FontSize / 2);
                    Int32 YLocOffset = (Int32)(Text.FontSize / 3);
                    //Canvas.SetLeft(Text, ((i * viewModel.GridInfoObj.Width) +
                    //    (this.ActualWidth / 2) - (this.MyContent.TimeValueList[i].Length * XLocOffset) + viewModel.XAxisTimeValueOffset));
                    //Canvas.SetLeft(Text, ((i * 80) - (this.MyContent.TimeValueList[i].Length * XLocOffset) + this.ViewModel.XAxisTimeValueOffset));
                    Canvas.SetLeft(Text, (TickPixcel - (this.MyContent.TimeValueList[i].Length * XLocOffset) + this.ViewModel.XAxisTimeValueOffset));
                    Canvas.SetTop(Text, -10);

                    //시간 헤더 정보 넣기
                    TextBlock HeaderText = new TextBlock();
                    HeaderText.HorizontalAlignment = HorizontalAlignment.Center;
                    HeaderText.VerticalAlignment = VerticalAlignment.Center;
                    HeaderText.FontSize = 10.0f;
                    HeaderText.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    HeaderText.SetBinding(TextBlock.ForegroundProperty, new Binding("ThemaForeColor"));
                    //HeaderText.Foreground = Brushes.White;
                    HeaderText.SetBinding(TextBlock.DataContextProperty, new Binding("ContentViewModel"));
                    HeaderText.SetBinding(TextBlock.TextProperty, new Binding("XAxisHeader"));
                    this.ValueCanvas.Children.Add(HeaderText);
                    XLocOffset = (Int32)(Text.FontSize / 2);
                    YLocOffset = (Int32)(Text.FontSize / 3);
                    HeaderLastPos = this.MyContent.OscilloscopeWidth / 2;
                    Canvas.SetLeft(HeaderText, HeaderLastPos);
                    HeaderLastPos = HeaderLastPos + HeaderText.Text.Length + 30;
                    //Canvas.SetTop(HeaderText, 12);
                    Canvas.SetTop(HeaderText, 0);
                }

                Console.WriteLine();
                for (int i = 0; i < this.ViewModel.GridInfoObj.ColumnCount; i++)
                {
                    Double EndTime = this.ViewModel.AxisInfoObj.TickScale * (Int32)(this.ViewModel.AxisInfoObj.AxisLength / 80);
                    Double TimeScale = this.ViewModel.AxisInfoObj.AxisLength / (EndTime - 0);
                    Double Offset_X = TimeScale * 0;
                    Double TickPixcel = ((i * this.ViewModel.AxisInfoObj.TickScale) * TimeScale) - Offset_X;
                    Double Div = 2.0d; //this.MyContent.XAxisScaleValue;
                    TickPixcel = TickPixcel / Div;

                    //if (TickPixcel >= lastXPos)
                        //TickPixcel = this.MyContent.OscilloscopeWidth;

                    //if(i == (this.ViewModel.GridInfoObj.ColumnCount - 1)
                    //    && TickPixcel < this.MyContent.OscilloscopeWidth)
                    //    TickPixcel = lastXPos;
                    //else if (TickPixcel >= this.MyContent.OscilloscopeWidth)
                    //    TickPixcel = lastXPos;

                    //Y축 그리드 라인 그리기
                    Path pathYGrid = new Path();
                    //pathYGrid.Stroke = Brushes.White;
                    pathYGrid.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                    pathYGrid.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                    pathYGrid.Opacity = 0.7d;
                    pathYGrid.StrokeThickness = 1.0f;
                    pathYGrid.StrokeDashArray = DoubleCollection.Parse("2, 2");
                    LineGeometry lineGrid = new LineGeometry();
                    //lineGrid.StartPoint = new Point((i * 40), (this.ActualHeight - 60) - 3);
                    //lineGrid.EndPoint = new Point((i * 40), lastYPos);
                    lineGrid.StartPoint = new Point(TickPixcel, (this.ActualHeight - 25));
                    lineGrid.EndPoint = new Point(TickPixcel, lastYPos);
                    pathYGrid.Data = lineGrid;
                    this.LineCanvas.Children.Add(pathYGrid);

                    if (TickPixcel >= lastXPos)
                    {
                        lastXPos = TickPixcel;
                        this.m_lastXPos = lastXPos;
                        break;
                    }
                }

                //X축 라인을 그린다.
                Path xDummyPath = new Path();
                Path xPath = new Path();
                xPath.SetBinding(Path.DataContextProperty, new Binding("ContentViewModel"));
                xPath.SetBinding(Path.StrokeProperty, new Binding("ThemaForeColor"));
                //xPath.Stroke = Brushes.White;
                xDummyPath.Stroke = Brushes.Black;
                xPath.StrokeThickness = 2.0f;
                xDummyPath.StrokeThickness = 1.0f;
                LineGeometry line = new LineGeometry();
                LineGeometry DummyLine = new LineGeometry();
                //line.StartPoint = new Point((this.ActualWidth / 2), this.ActualHeight - 60);
                line.StartPoint = new Point(0, this.ActualHeight - 25);
                DummyLine.StartPoint = new Point(0, -10);
                line.EndPoint = new Point(lastXPos, this.ActualHeight - 25);
                //line.EndPoint = new Point(this.MyContent.OscilloscopeWidth, this.ActualHeight - 40);
                DummyLine.EndPoint = new Point(this.MyContent.OscilloscopeWidth, -10);
                //line.StartPoint = new Point(20, (Height - 60));
                //line.EndPoint = new Point(lastXPos, (Height- 60));
                xPath.Data = line;
                xDummyPath.Data = DummyLine;

                this.LineCanvas.Children.Add(xPath);
                //this.DummyXLineCanvas.Children.Add(xDummyPath);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
