using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CrevisLibrary;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace DCT_Graph
{
    /// <summary>
    /// Event 처리
    /// </summary>
    public partial class YAxisControl
    {
        /// <summary>
        /// 화면이 로드 되었을 경우 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YAxisControl_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                //축을 그릴 정보를 자동으로 값을 계산해서 가져온다.
                GetAutoScaleValue();

                //축을 그린다.
                DrawAxisLine();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// ▼버튼을 눌렀을 경우 호출
        /// </summary>
        private void DownArrowButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.m_IsManualScale = true;

                this.ViewModel.AxisInfoObj.MinValue = this.ViewModel.AxisInfoObj.MinValue + this.ViewModel.AxisInfoObj.TickScale;
                this.ViewModel.AxisInfoObj.MaxValue = this.ViewModel.AxisInfoObj.MaxValue + this.ViewModel.AxisInfoObj.TickScale;
                //MinValue = viewModel.AxisInfoObj.StartValue + viewModel.AxisInfoObj.TickScale;
                //MaxValue = viewModel.AxisInfoObj.StartValue * viewModel.AxisInfoObj.TickCount;
                //viewModel.AxisInfoObj.StartValue = viewModel.AxisInfoObj.MinValue;

                //축을 그릴 정보를 자동으로 값을 계산해서 가져온다.
                GetAutoScaleValue();

                //축을 그린다.
                DrawAxisLine();

                //데이터 다시 그리기
                this.MyContent.XAxis.RedrawByArrowButton(this.ViewModel.AxisInfoObj.TickLength, this.MyIndex, false);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// ▲버튼을 눌렀을 경우 호출
        /// </summary>
        private void UpArrowButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.m_IsManualScale = true;

                this.ViewModel.AxisInfoObj.MinValue = this.ViewModel.AxisInfoObj.MinValue - this.ViewModel.AxisInfoObj.TickScale;
                this.ViewModel.AxisInfoObj.MaxValue = this.ViewModel.AxisInfoObj.MaxValue - this.ViewModel.AxisInfoObj.TickScale;
                //Double MinValue = viewModel.AxisInfoObj.StartValue - viewModel.AxisInfoObj.TickScale;
                //Double MaxValue = (MinValue * viewModel.AxisInfoObj.TickCount) - viewModel.AxisInfoObj.TickScale;
                //viewModel.AxisInfoObj.StartValue = viewModel.AxisInfoObj.MinValue;

                //축을 그릴 정보를 자동으로 값을 계산해서 가져온다.
                GetAutoScaleValue();

                //축을 그린다.
                DrawAxisLine();

                //데이터 다시 그리기
                this.MyContent.XAxis.RedrawByArrowButton(this.ViewModel.AxisInfoObj.TickLength, this.MyIndex,  true);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 컨트롤을 마우스 왼쪽버튼을로 클릭했을 경우 호출
        /// </summary>
        public void YAxisControl_OnDown(Object sender, MouseEventArgs e)
        {
            try
            {
                //통신 중에는 선택하지 못하도록
                if (!this.MyContent.IsOscilloCommCheck)
                    return;

                #region 컨트롤 선택 UI 변경
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.ViewModel.SelectBorder = this.MyContent.BrushColorArray[this.MyIndex];
                    this.ViewModel.SelectThickness = new Thickness(2);
                }

                Int32 count = -1;
                foreach (YAxisControl control in this.MyContent.YAxisList)
                {
                    count++;

                    if (count.Equals(this.MyIndex))
                        continue;

                    (control.ViewModel).SelectBorder = Brushes.Transparent;
                    (control.ViewModel).SelectThickness = new Thickness(0);
                }
                #endregion

                //커서 모양 변경
                this.Cursor = Cursors.Hand;

                #region 마우스 이벤트 처리
                if (!this.m_IsDragging)
                {
                    this.m_IsDragging = true;

                    this.m_mouseStartPoint = e.GetPosition(this);
                    this.m_CurrentClickMinValue = this.ViewModel.AxisInfoObj.MinValue;
                    this.m_CurrentClickMaxValue = this.ViewModel.AxisInfoObj.MaxValue;

                    this.m_itemStartPointArray.Clear();
                    for (int i = 0; i < this.ValueTextCanvas.Children.Count; i++)
                    {
                        double x = Canvas.GetLeft(this.ValueTextCanvas.Children[i]);
                        double y = Canvas.GetTop(this.ValueTextCanvas.Children[i]);

                        x = double.IsNaN(x) ? 0 : x;
                        y = double.IsNaN(y) ? 0 : y;
                        m_itemStartPointArray.Add(new Point(x, y));
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
        public void YAxisControl_OnMove(object sender, MouseEventArgs e)
        {
            try
            {
                Boolean IsFromXAxis = (sender is SignalDisplayControl) ? true : false;


                if (e.LeftButton == MouseButtonState.Pressed && this.m_IsDragging == true)
                {
                    Vector deltaVector = e.GetPosition(this) - this.m_mouseStartPoint;

                    Double DataEachPixcel = this.ViewModel.AxisInfoObj.TickScale / this.ViewModel.AxisInfoObj.TickLength;

                    Double DataAccording2Moving = DataEachPixcel * deltaVector.Y;
                    this.m_IsManualScale = true;

                    this.ViewModel.AxisInfoObj.MinValue = this.m_CurrentClickMinValue + DataAccording2Moving;
                    this.ViewModel.AxisInfoObj.MaxValue = this.m_CurrentClickMaxValue + DataAccording2Moving;
                    this.ViewModel.AxisInfoObj.StartValue = this.ViewModel.AxisInfoObj.MinValue;

                    //축을 그릴 정보를 자동으로 값을 계산해서 가져온다.
                    GetAutoScaleValue();

                    //축을 그린다.
                    DrawAxisLine();

                    if (this.MyContent.IsOscilloCommCheck)
                    {
                        //시그널 데이터를 이동시킨다.
                        String EnumName = this.ViewModel.AxisInfoObj.AxisName.Replace(" ", "_");
                        OscilloscopeParameterType type = new OscilloscopeParameterType();
                        Enum.TryParse(EnumName, out type);

                        SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                        if (overlay != null && overlay.Points.Count > 0)
                        {
                            //새로운 좌표로 다시 데이터를 그린다.
                            this.MyContent.XAxis.DrawSignalData(overlay, this.ViewModel.AxisInfoObj.Channel, IsFromXAxis);
                        }
                    }
                }
                else
                    this.m_IsDragging = false;
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
        private void YAxisControl_OnUp(object sender, MouseEventArgs e)
        {
            try
            {
                //커서 모양 변경
                this.Cursor = Cursors.Arrow;

                this.m_IsDragging = false;

                this.ReleaseMouseCapture();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void YAxisControl_OnWheel(Object sender, MouseWheelEventArgs e)
        {
            try
            {
                Double ScaleValue = 0;

                //휠을 위로 올렸을 때 (확대)
                if (e.Delta > 0)
                {
                    this.m_WheelScale = 1.0d;
                    ScaleValue = this.m_WheelScale * this.m_ZoomInterval;
                    this.m_WheelScale = ScaleValue;
                    ScaleValue = (ScaleValue > this.m_MaxZoomRate) ? this.m_MaxZoomRate : ScaleValue;
                }
                //휠을 아래로 내렸을 때 (축소)
                else
                {
                    this.m_WheelScale = 1.0d;
                    ScaleValue = this.m_WheelScale / this.m_ZoomInterval;
                    this.m_WheelScale = ScaleValue;
                    ScaleValue = (ScaleValue < this.m_MinZoomRate) ? this.m_MinZoomRate : ScaleValue;
                }

                //축이 선택되었을 때
                if (this.ViewModel.SelectBorder.Equals(this.MyContent.BrushColorArray[this.MyIndex]))
                {
                    //픽셀 당 데이터 값을 구한다.
                    Double CurrentY = e.GetPosition(this).Y; //400.0d;
                                               //this.ViewModel.Test = CurrentY.ToString();
                    Double DataEachPixcel = this.ViewModel.AxisInfoObj.TickScale / this.ViewModel.AxisInfoObj.TickLength;
                    Double CurrentData = this.ViewModel.AxisInfoObj.MinValue +
                        (DataEachPixcel * (this.ViewModel.AxisInfoObj.AxisLength - CurrentY));
                    Double NewStartData = CurrentData - ((DataEachPixcel / ScaleValue) * (this.ViewModel.AxisInfoObj.AxisLength - CurrentY));
                    this.ViewModel.AxisInfoObj.MinValue = NewStartData;
                    Double NewDataScale = this.ViewModel.AxisInfoObj.TickScale / ScaleValue;
                    this.ViewModel.AxisInfoObj.TickScale = NewDataScale;

                    this.DrawAxisLine();

                    String EnumName = this.ViewModel.AxisInfoObj.AxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(EnumName, out type);

                    SignalOverlay overlay = this.MyContent.DigitalSignalMap[type].GetOverlay();
                    this.MyContent.XAxis.DrawSignalData(overlay, this.ViewModel.AxisInfoObj.Channel);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
