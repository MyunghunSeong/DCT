
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CrevisLibrary;

namespace DCT_Graph
{
    public partial class SignalDisplayControl
    {
        /// <summary>
        /// 화면이 로드 되었을 경우 호출
        /// </summary>
        private void DisplaySignalControl_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
#if false
                this.m_PathArray = new System.Windows.Shapes.Path[5];
                for (int i = 0; i < this.m_PathArray.Length; i++)
                    this.m_PathArray[i] = new System.Windows.Shapes.Path();

                //줌이 된 상태라면 줌 된 상태의 시작 시간과 TickScale을 설정해준다.
                if (this.MyContent.WheelScale != 1.0d)
                {
                    this.ViewModel.AxisInfoObj.StartValue = this.MyContent.BackUpStartValue;
                    this.ViewModel.AxisInfoObj.TickScale = this.MyContent.BackUpTickScale;
                }

                GetAutoScaleValue();

                DrawCursor();

                DrawTriggerLine();
#endif
#if true
                //캔버스 배열 만들어서 넣기
                this.m_CanvasArray = new Canvas[5];
                for (int i = 0; i < this.m_CanvasArray.Length; i++)
                {
                    this.m_CanvasArray[i] = new Canvas();
                    //this.m_CanvasArray[i].SetValue(Canvas.MarginProperty, new Thickness(0, this.MyContent.XAxis.ViewModel.testVal - 7, 0, (this.MyContent.XAxis.ViewModel.testVal - 7) + 3));
                    Int32 Scale = (Int32)(this.ViewModel.testVal / 7);
                    this.m_CanvasArray[i].SetValue(Canvas.MarginProperty, new Thickness(0, (Int32)this.ViewModel.testVal, 0, 10 * Scale));
                    this.m_CanvasArray[i].Opacity = 1.0d;
                    this.mainGrid.Children.Add(this.m_CanvasArray[i]);
                }

                //줌이 된 상태라면 줌 된 상태의 시작 시간과 TickScale을 설정해준다.
                if (this.MyContent.WheelScale != 1.0d)
                {
                    this.ViewModel.AxisInfoObj.StartValue = this.MyContent.BackUpStartValue;
                    this.ViewModel.AxisInfoObj.TickScale = this.MyContent.BackUpTickScale;
                }

                //화면을 그린다.
                GetAutoScaleValue();

                //커서를 그린다.
                DrawCursor();

                //트리거 라인을 그린다.
                DrawTriggerLine();
#endif
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 해당 컨트롤을 왼쪽 마우스로 클릭할 때 호출
        /// </summary>
        private void SignalDisplay_OnDown(Object sender, MouseEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Hand;

                #region 마우스 이벤트 처리
                if (!this.m_IsDragging)
                {
                    this.m_IsDragging = true;

                    this.m_mouseStartPoint = e.GetPosition(this);

                    Int32 count = 0;
                    foreach (SignalOverlay overlay in this.m_RecvedOverlayArray)
                    {
                        if (overlay != null && overlay.Points.Count > 0)
                        {
                            List<Point> tmpPtList = new List<Point>();
                            foreach (Point p in overlay.Points)
                                tmpPtList.Add(p);
                            this.m_CurrentClickPointListArray[count] = tmpPtList;
                        }

                        count++;
                    }

                    this.m_CurrentClickStartTime = this.ViewModel.AxisInfoObj.StartValue;
                    DigitalSignal signal = new DigitalSignal();

                    //클릭한 좌표의 시간 데이터를 구한다.
                    //this.m_SelectedXPosTime = signal.PixcelToTime(this.m_mouseStartPoint.X);

                    /*
                    //현재 그려진 시그날의 데이터 좌표들을 저장
                    this.m_PointStartPointArray.Clear();
                    for (int j = 0; j < this.m_CanvasArray.Length; j++)
                    {
                        for (int i = 0; i < this.m_CanvasArray[j].Children.Count; i++)
                        {
                            double x = Canvas.GetLeft(this.m_CanvasArray[j].Children[i]);
                            double y = Canvas.GetTop(this.m_CanvasArray[j].Children[i]);

                            x = double.IsNaN(x) ? 0 : x;
                            y = double.IsNaN(y) ? 0 : y;
                            this.m_PointStartPointArray.Add(new Point(x, y));
                        }
                    }
                    */

                    //현재 시간 데이터 좌표를 저장
                    this.m_TimeStartPointArray.Clear();
                    for (int i = 0; i < this.ValueCanvas.Children.Count; i++)
                    {
                        if ((this.ValueCanvas.Children[i] as TextBlock).Text.Contains("Time"))
                            continue;

                        double x = Canvas.GetLeft(this.ValueCanvas.Children[i]);
                        double y = Canvas.GetTop(this.ValueCanvas.Children[i]);

                        x = double.IsNaN(x) ? 0 : x;
                        y = double.IsNaN(y) ? 0 : y;

                        this.m_TimeStartPointArray.Add(new Point(x, y));
                    }

                    this.m_DataStartPointListArray = new List<Point>[this.MyContent.YAxisList.Count];
                    for (int i = 0; i < this.m_DataStartPointListArray.Length; i++)
                        this.m_DataStartPointListArray[i] = new List<Point>();

                    //클릭한 좌표의 각 축의 대한 데이터를 구한다.
                    this.m_SelectedYPosDataList.Clear();
                    count = 0;
                    foreach (YAxisControl control in this.MyContent.YAxisList)
                    {
                        control.SetCurrentMinMaxValue();
                        this.m_SelectedYPosDataList.Add(control.DigitalSignalObj.PixcelToData(this.m_mouseStartPoint.Y));

                        for (int i = 0; i < control.ValueTextCanvas.Children.Count; i++)
                        {
                            double x = Canvas.GetLeft(control.ValueTextCanvas.Children[i]);
                            double y = Canvas.GetTop(control.ValueTextCanvas.Children[i]);

                            x = double.IsNaN(x) ? 0 : x;
                            y = double.IsNaN(y) ? 0 : y;
                            this.m_DataStartPointListArray[count].Add(new Point(x, y));
                        }
                        count++;
                    }

                    this.CaptureMouse();
                }
#endregion
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 마우스를 움직일 때 호출
        /// </summary>
        private void SignalDisplayControl_OnMove(object sender, MouseEventArgs e)
        {
            try
            {
                Vector deltaVector = new Vector();
                //Boolean IsOverZeroTime = false;
                if (e.LeftButton == MouseButtonState.Pressed && this.m_IsDragging == true)
                {
                    deltaVector = e.GetPosition(this) - this.m_mouseStartPoint;

                    #region X축 변화
                    Double TimeEachPixcel = this.ViewModel.AxisInfoObj.TickScale / this.ViewModel.AxisInfoObj.TickLength;
                    Double TimeAccording2Moving = TimeEachPixcel * deltaVector.X;

                    Double tmp = this.m_CurrentClickStartTime - (TimeAccording2Moving);

                    /*if (tmp < 0)
                    {
                        IsOverZeroTime = true;
                        viewModel.AxisInfoObj.StartValue = 0;
                    }
                    else
                    {
                        IsOverZeroTime = false;
                        viewModel.AxisInfoObj.StartValue = tmp;
                    }
                    */

                    this.ViewModel.AxisInfoObj.StartValue = tmp;

                    Boolean IsFromZoom = (this.ViewModel.AxisInfoObj.TickScale == 1000.0d) ? false : true;
                    IsFromZoom = (this.MyContent.XAxisScaleIndex != 0) ? false : true;
                    GetAutoScaleValue(true, true, IsFromZoom);
                    #endregion

                    #region Y축 변화
                    //                    this.Dispatcher.BeginInvoke((Action)delegate ()
                    //                    {
                    //                        foreach (YAxisControl control in this.MyContent.YAxisList)
                    //                        {
                    //                            //현재 마우스 위치를 Y축 컨트롤에 업데이트 한다.
                    //                            control.m_mouseStartPoint = this.m_mouseStartPoint;
                    //                            //Y축의 드레그 플래그를 true로 설정
                    //                            control.m_IsDragging = true;
                    //                            control.YAxisControl_OnMove(sender, e);
                    //                            control.DigitalSignalObj.StartTime = this.ViewModel.AxisInfoObj.StartValue;
                    //                        }
                    //                    });
                    #endregion
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 마우스를 떼었을 때 호출
        /// </summary>
        private void SignalDisplayControl_OnUp(object sender, MouseEventArgs e)
        {
            try
            {
                //커서 모양 바꾸기
                this.Cursor = Cursors.Arrow;

                //변경된 데이터로 Measure를 업데이트 해준다.
                //SMH5555
                if (PublicVar.MainWnd.ViewModel.MeasureControl != null)
                {
                    for (int i = 0; i < PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children.Count; i++)
                    {
                        if (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] is MeasurementControl)
                        {
                            (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] as MeasurementControl).Run();
                        }
                    }
                }
                
                //Y축의 드레그 플래그를 false로 설정
                foreach (YAxisControl control in this.MyContent.YAxisList)
                    control.m_IsDragging = false;

                this.m_IsDragging = false;

                this.ReleaseMouseCapture();

                //this.m_IsDragging = false;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 마우스 휠을 움직였을 때 호출
        /// </summary>
        private void SignalDisplay_OnMouseWheel(Object sender, MouseWheelEventArgs e)
        {
            try
            {
                Double ScaleValue = 0.0d;

                //휠을 위로 올렸을 때 (확대)
                if (e.Delta > 0)
                {
                    this.m_WheelScale = 1.0d;
                    ScaleValue = this.m_WheelScale * this.m_ZoomInterval;
                    this.m_WheelScale = ScaleValue;

                    //휠 스케일 저장(다른 부분에서 쓰일 스케일 값)
                    this.MyContent.WheelScale = this.MyContent.WheelScale * 1.2d;
                    ScaleValue = (ScaleValue > this.m_MaxZoomRate) ? this.m_MaxZoomRate : ScaleValue;
                }
                //휠을 아래로 내렸을 때 (축소)
                else if(e.Delta < 0)
                {
                    this.m_WheelScale = 1.0d;
                    ScaleValue = this.m_WheelScale / this.m_ZoomInterval;
                    this.m_WheelScale = ScaleValue;

                    //휠 스케일 저장(다른 부분에서 쓰일 스케일 값)
                    this.MyContent.WheelScale = this.MyContent.WheelScale / 1.2d;
                    ScaleValue = (ScaleValue < this.m_MinZoomRate) ? this.m_MinZoomRate : ScaleValue;
                }

                //X축 스케일 조정
                #region X축
                //픽셀 당 시간 값을 구한다.
                //Double Time = Convert.ToDouble(this.MyContent.TimeValueList[1]);
                //Double TimePixcel = this.MyContent.DigitalSignalMap[OscilloscopeParameterType.Motor_Feedback_Position].TimeToPixel(Time);
                if (this.MyContent.ZoomMode == ZoomMode.Only_XAis || this.MyContent.ZoomMode == ZoomMode.Both)
                {
                    Double CurrentX = this.ViewModel.AxisInfoObj.TickLength;
                    Int32 aaa = this.MyContent.XAxisScaleValue;
                    CurrentX = CurrentX / (aaa / 2.0);

                    CurrentX = e.GetPosition(this).X;
                    Double TimeEachPixcel = this.ViewModel.AxisInfoObj.TickScale / this.ViewModel.AxisInfoObj.TickLength;
                    Double CurrentTime = this.ViewModel.AxisInfoObj.StartValue + (TimeEachPixcel * CurrentX);
                    Double NewStartTime = CurrentTime - ((TimeEachPixcel / ScaleValue) * CurrentX);
                    this.MyContent.BackUpStartValue = NewStartTime;
                    this.ViewModel.AxisInfoObj.StartValue = NewStartTime;
                    Double NewTickScale = this.ViewModel.AxisInfoObj.TickScale / ScaleValue;
                    this.MyContent.BackUpTickScale = NewTickScale;
                    this.ViewModel.AxisInfoObj.TickScale = NewTickScale;

                    Boolean flag = (this.MyContent.XAxisScaleIndex != 0) ? false : true;
                    GetAutoScaleValue(true, false, flag);
                }
#endregion

                //Y축 스케일 조정
                #region Y축
                //YAxisControl control = this.MyContent.YAxisList[1];
                foreach (YAxisControl control in this.MyContent.YAxisList)
                {
                    YAxisControl_ViewModel controlViewModel = control.ViewModel;

                    if (this.MyContent.ZoomMode == ZoomMode.Both || this.MyContent.ZoomMode == ZoomMode.Only_YAxis)
                    {
                        //픽셀 당 데이터 값을 구한다.
                        Double CurrentY = e.GetPosition(this).Y; //400.0d;
                                                    //this.ViewModel.Test = CurrentY.ToString();
                        Double DataEachPixcel = controlViewModel.AxisInfoObj.TickScale / controlViewModel.AxisInfoObj.TickLength;
                        Double CurrentData = controlViewModel.AxisInfoObj.MinValue +
                            (DataEachPixcel * (controlViewModel.AxisInfoObj.AxisLength - CurrentY));
                        Double NewStartData = CurrentData - ((DataEachPixcel / ScaleValue) * (controlViewModel.AxisInfoObj.AxisLength - CurrentY));
                        controlViewModel.AxisInfoObj.MinValue = NewStartData;
                        Double NewDataScale = controlViewModel.AxisInfoObj.TickScale / ScaleValue;
                        controlViewModel.AxisInfoObj.TickScale = NewDataScale;

                        control.DrawAxisLine();
                    }

                    String EnumName = control.ViewModel.AxisInfoObj.AxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(EnumName, out type);

                    //if (this.MyContent.IsOscilloCommCheck)
                    //{
                    //    //스케일 된 데이터로 다시 그래프를 그린다.
                    SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                    //if (overlay != null)
                    //{
                    //    for (int i = 0; i < overlay.Points.Count; i++)
                    //        overlay.Points[i] = new Point(overlay.Points[i].X + this.DeltaVector.X, overlay.Points[i].Y + this.DeltaVector.Y);
                    //}
                    DrawSignalData(overlay, controlViewModel.AxisInfoObj.Channel);
                    //}
                }
                #endregion

                //변경된 데이터로 Measure를 업데이트 해준다.
                //SMH5555
                if (PublicVar.MainWnd.ViewModel.MeasureControl != null)
                {
                    for (int i = 0; i < PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children.Count; i++)
                    {
                        if (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] is MeasurementControl)
                        {
                            (PublicVar.MainWnd.ViewModel.MeasureControl.mainStack.Children[i] as MeasurementControl).Run();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 그래프 데이터 리셋
        /// </summary>
        private void ResetItem_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.ViewModel.AxisInfoObj.StartValue = 0;
                this.ViewModel.AxisInfoObj.TickScale = 1000;

                if (this.MyContent.IsOscilloCommCheck)
                {
                    Int32 Count = 0;
                    foreach (YAxisControl yControl in this.MyContent.YAxisList)
                    {
                        if (this.MyContent.AxisInfoList[Count].IsChannelSelected
                            && this.MyContent.AxisInfoList[Count++].CurrentSelectedAxisIndex > 0)
                            yControl.RedrawAccording2AutoScale();
                    }
                }

                this.MyContent.XAxisScaleIndex = 0;
                this.m_WheelScale = 1.0d;
                this.MyContent.WheelScale= 1.0d;

                GetAutoScaleValue();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void WhiteThemaItem_Check(Object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = sender as MenuItem;
                item.Header = "Black Thema";

                Image image = item.Icon as Image;
                image.Source = new BitmapImage(new Uri(@"/ICon/ColorInvision.png", UriKind.Relative));

                item.Icon = image;

                PublicVar.MainWnd.ViewModel.ThemaTypeValue = ThemaType.WHITE_THEMA;

                //오실로 스코프 컨텐츠의 테마 색상에 맞게 변경
                this.MyContent.ChangeThema();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void BlackThemaItem_Check(Object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = sender as MenuItem;
                item.Header = "White Thema";

                Image image = item.Icon as Image;
                image.Source = new BitmapImage(new Uri(@"/ICon/ColorInvision_Reverse.png", UriKind.Relative));

                item.Icon = image;

                PublicVar.MainWnd.ViewModel.ThemaTypeValue = ThemaType.BLACK_THEMA;

                //오실로 스코프 컨텐츠의 테마 색상에 맞게 변경
                this.MyContent.ChangeThema(false);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
