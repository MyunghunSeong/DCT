using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CrevisLibrary;
using MotionGuidePro.Main;

namespace DCT_Graph
{
    public partial class OscilloscopeControl
    {
        /// <summary>
        /// 해당 화면이 로드 되었을 때 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsciiloscopeControl_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                //UI 초기화 작업
                InitializeUI();

                //TriggerControl
                if (this.MyContent.TriggerObj == null)
                {
                    TriggerControl trigger = new TriggerControl(this.MyContent);
                    this.triggerBorder.Child = trigger;
                    this.MyContent.TriggerObj = trigger;
                    this.MyContent.TriggerControl_ViewModel = trigger.ViewModel;
                }
                else
                {
                    if (this.MyContent.TriggerObj.Parent != null)
                        (this.MyContent.TriggerObj.Parent as Border).Child = null;
                    this.triggerBorder.Child = this.MyContent.TriggerObj;
                }

                DrawGraphDisplay();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 화면의 크기가 변경되었을 경우 호출
        /// </summary>
        private void OscControl_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            try
            {
                this.m_IsOscilloDisplaySizeChanged = true;
                //새로운 사이즈로 화면을 다시그린다.
                SizeUpdate(e.NewSize);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 그래프 화면의 크기가 변경됐을 때 호출
        /// </summary>
        private void Grid_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (this.m_IsOscilloDisplaySizeChanged)
                    return;

                Size newSize = new Size();

                //MeasurementControl의 넓이값을 저장
                this.m_MeasurementControlWidth = this.ActualWidth - e.NewSize.Width;

                newSize.Width = this.ActualWidth;
                newSize.Height = this.ActualHeight;

                //새로운 사이즈로 화면을 다시그린다.
                SizeUpdate(newSize);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
            finally
            {
                this.m_IsOscilloDisplaySizeChanged = false;
            }
        }

        /// <summary>
        /// MANUAL 버튼을 눌렀을 경우 호출
        /// </summary>
        private void ManualScaleBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button Btn = sender as Button;
                Int32 Index = Int32.Parse(Btn.Name.Split('_')[1]);

                //클릭한 버튼이 위치한 Y축의 정보를 가져온다.
                AxisInformation info = this.MyContent.YAxisList[Index].ViewModel.AxisInfoObj;

                ManualScaleSetting ManualScale = new ManualScaleSetting(info);
                ManualScale.Owner = PublicVar.MainWnd;
                ManualScale.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ManualScale.ShowDialog();

                if (ManualScale.IsOK)
                {
                    if (info.TickScale == 0)
                        throw new Exception("Can't set tick value to '0'");

                    //변경된 정보(Min, TickScale)를 가져온다.
                    this.MyContent.YAxisList[Index].ViewModel.AxisInfoObj = ManualScale.DataContext as AxisInformation;

                    //변경된 TickScale을 백업해두고
                    Double ChangeTickScale = this.MyContent.YAxisList[Index].ViewModel.AxisInfoObj.TickScale;

                    //변경된 Min값으로 간격을 구하고 다시 Y축을 그린다.
                    this.MyContent.YAxisList[Index].GetAutoScaleValue();
                    this.MyContent.YAxisList[Index].DrawAxisLine();

                    //값이 0에 가장 가까운 인덱스를 구한다.
                    #region 0에 가까운 인덱스 구하기
                    Double ZeroNum = Math.Abs(info.MaxValue);
                    Int32 ZeroIndex = -1;
                    Int32 Count = 0;
                    for (Double val = info.MinValue; val < info.MaxValue; val += info.TickScale)
                    {
                        Double Data = val - 0;
                        if (Data < ZeroNum)
                        {
                            ZeroNum = Data;
                            ZeroIndex = Count;
                        }
                        Count++;
                    }

                    if (ZeroIndex < 0)
                        ZeroIndex = Count - 1;
                    #endregion

                    //아까 변경된 TickScale로 바꿔준다.(GetAutoScale하면서 TickScale이 자동 할당 되었기 때문)
                    this.MyContent.YAxisList[Index].ViewModel.AxisInfoObj.TickScale = ChangeTickScale;
                    //설정한 스케일 값으로 다시 축을 그린다.
                    this.MyContent.YAxisList[Index].RedrawAccording2Scale(ZeroIndex);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// AUTO 버튼을 눌렀을 경우 호출
        /// </summary>
        private void AutoScaleBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                Int32 Index = Convert.ToInt32(btn.Name.Split('_')[1]);
                String ScaleValue = btn.Tag as String;

                //설정한 스케일 값으로 다시 축을 그린다.
                this.MyContent.YAxisList[Index].RedrawAccording2AutoScale();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 오실로스코프 컨트롤을 왼쪽 마우스버튼으로 클릭했을 경우 호출
        /// </summary>
        private void OscControl_PreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach (YAxisControl control in this.MyContent.YAxisList)
                {
                    (control.ViewModel).SelectBorder = Brushes.Transparent;
                    (control.ViewModel).SelectThickness = new Thickness(0);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 채널 축 선택이 변경됐을 때 호출
        /// </summary>
        private void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox combo = sender as ComboBox;

                //현재 선택한 채널 정보를 가져온다.
                Int32 Channel = Convert.ToInt32((combo.Tag as String).Substring(0, 1)) - 1;

                //중복 선택된 경우에 처리
                if (this.MyContent.AxisInfoList[Channel].IsOverlap)
                {
                    //기존에 선택된 축으로 변경해주고
                    if(!combo.SelectedItem.Equals(combo.SelectionBoxItem))
                        combo.SelectedItem = combo.SelectionBoxItem;
                    //중복 설정을 해제한다.
                    this.MyContent.AxisInfoList[Channel].IsOverlap = false;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 축 색상을 변경할 때 호출
        /// </summary>
        private void ChangeAxisColorButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                Int32 Channel = (Int32)btn.Tag;

                ChangeAxisColor ColorDlg = new ChangeAxisColor(this.MyContent, Channel);
                ColorDlg.LogEvent += PublicVar.MainWnd.ViewModel.Log_Maker;
                ColorDlg.Owner = PublicVar.MainWnd;
                ColorDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ColorDlg.ShowDialog();

                if (ColorDlg.IsOK)
                {
                    this.MyContent.AxisInfoList[ColorDlg.SelectedChannel].CurrentAxisColor = ColorDlg.SelectedColor;

                    this.MyContent.YAxisList[ColorDlg.SelectedChannel].DrawAxisLine();

                    SignalOverlay overlay = this.MyContent.DigitalSignalMap[this.MyContent.AxisInfoList[ColorDlg.SelectedChannel].ParamType].GetOverlay();
                    this.MyContent.XAxis.DrawSignalData(overlay, ColorDlg.SelectedChannel);

                    Console.WriteLine();
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
