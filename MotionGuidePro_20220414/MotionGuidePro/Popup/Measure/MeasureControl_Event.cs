using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DCT_Graph;

namespace CrevisLibrary
{
    public partial class MeasureControl
    {
        private void MeasureControl_OnLoaded(Object sender, RoutedEventArgs e)
        {
            //Tunning Tab 파라미터 뷰 그리기
            #region Tunning Tab
            //파라미터 뷰
            IParam Param = this.MyContent.MyDevice.MyModel.GetParamNode(this.MyContent.Name).GetParamNode("파라미터 정보");

            this.RootParam = Param as DevParam;

            DevParam ShowParam = new DevParam("파라미터 정보", ParamType.Category, null, String.Empty);
            foreach (DevParam subParam in (Param.Clone() as DevParam).Children)
            {
                if (subParam.ParamInfo.Default)
                    ShowParam.Children.Add(subParam);
            }

            ParameterModel ParamModel = new CrevisLibrary.ParameterModel(ShowParam, false);
            this.ParamView = new ParameterView();
            this.ParamView.ContentType = this.MyContent.Type;
            this.ParamView.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
            this.ParamView.Tag = ParamModel;
            this.ParamView._tree.Model = ParamModel;
            PublicVar.MainWnd.ExpandAll(this.ParamView._tree.Nodes);
            this.TunningGrid.Children.Add(this.ParamView);
            Grid.SetRow(this.ParamView, 2);
            #endregion
        }

        /// <summary>
        /// +버튼을 눌렀을 때 호출
        /// </summary>
        private void MeasureAddBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                SelectionMeasureChannel measure = new SelectionMeasureChannel(this.OscilloContent);
                //로그 이벤트 처리 등록
                measure.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
                measure.Owner = PublicVar.MainWnd;
                measure.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                measure.ShowDialog();

                //OK버튼을 눌렀을 경우
                if (measure.IsOK)
                {
                    Int32 Count = 0;
                    //사용자가 선택한 보여줄 정보리스트를 받아온다.
                    var item = (measure.ViewModel).AxisInfoList;
                    foreach (var subItem in item)
                    {
                        //Check 동기화
                        this.OscilloContent.AxisInfoList[Count].IsChecked = subItem.IsChecked;

                        //선택한 경우
                        if (subItem.IsChecked)
                        {
                            //현재 선택한 채널을 구한다.
                            Int32 SelectChannel = Convert.ToInt32(subItem.AxisName.Substring(0, 1)) - 1;

                            //기존 StackPanel에 추가되었는지 확인
                            Boolean IsAlreadyAdded = false;

                            //Measure 정보를 보여줄 컨트롤을 생성
                            MeasurementControl measureControl = new MeasurementControl(this.OscilloContent);
                            //현재 채널의 축 이름을 Tag로 넣어준다.(추가된 컨트롤인지 비교하기 위해서)
                            measureControl.Tag = this.OscilloContent.AxisInfoList[Count].CurrentSelectedAxisName;
                            //보여줄 정보를 구한다.
                            (measureControl.ViewModel).SelectChannel = SelectChannel;
                            measureControl.Run();

                            //StackPanel을 자식들중에서 MeasurementControl만 리스트에 임시 저장한다.
                            List<String> MeasureList = new List<String>();
                            for(int i = 0; i < this.mainStack.Children.Count; i++)
                            {
                                if (this.mainStack.Children[i] is MeasurementControl)
                                {
                                    if ((this.mainStack.Children[i] as MeasurementControl).Tag.Equals(measureControl.Tag))
                                    {
                                        this.mainStack.Children.RemoveAt(i);
                                        this.mainStack.Children.Insert(i, measureControl);
                                    }
                                    MeasureList.Add((this.mainStack.Children[i] as MeasurementControl).Tag as String);
                                }
                            }
                            

                            //위에서 저장한 리스트중에서 Tag로 저장한 축 이름이랑 동일한 경우 플래그 True로 설정
                            foreach (String addItem in MeasureList)
                            {
                                if (addItem.Equals(measureControl.Tag as String))
                                {
                                    IsAlreadyAdded = true;
                                    break;
                                }
                            }
                            

                            //생성된 measurementcontrol이 이미 추가되지 않았다면 수행
                            if (!IsAlreadyAdded)
                            {
                                //MeasureControl 추가 카운트를 +1 한다.
                                this.AddMeasureControlCount++;

                                //MeasureControl의 높이를 구하고 Grid에 추가해준다.
                                Double ControlHeight = this.mainStack.ActualHeight / 4;
                                if (MeasureList.Count >= Count)
                                    this.mainStack.Children.Insert(Count + 1, measureControl);
                                else
                                    this.mainStack.Children.Add(measureControl);
                            }
                        }
                        //선택되지 않았을 경우
                        else
                        {
                            //Tag가 같은 컨트롤들을 리스트에 저장한다.
                            List<MeasurementControl> removeList = new List<MeasurementControl>();
                            foreach (var control in this.mainStack.Children)
                            {
                                if (control is MeasurementControl)
                                {
                                    if ((control as MeasurementControl).Tag.Equals(this.OscilloContent.AxisInfoList[Count].CurrentSelectedAxisName))
                                        removeList.Add(control as MeasurementControl);
                                }
                            }

                            //저장한 리스트를 StackPanel에서 지워준다.
                            foreach (var removeItem in removeList)
                                this.mainStack.Children.Remove(removeItem);
                        }
                        Count++;
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
        /// SaveParameter 버튼을 눌렀을 때 호출
        /// </summary>
        private void SaveParamBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.MyContent.SaveData((this.ParamView.Tag as ParameterModel).Root);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Communication 버튼을 눌렀을 때 호출
        /// </summary>
        private void CommBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.MyContent.Communication(this.ParamView);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Select Parameter 버튼을 눌렀을 때 호출
        /// </summary>
        private void SelectParamBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //파라미터 선택 창 띄우기
                SelectMeasureParam ParamDlg = new SelectMeasureParam(this.RootParam);
                ParamDlg.Owner = PublicVar.MainWnd;
                ParamDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ParamDlg.ShowDialog();

                //OK버튼을 눌렀을 때만 처리
                if (ParamDlg.IsOK)
                {
                    this.RootParam = ParamDlg.RootParam;
                    PublicVar.MainWnd.ViewModel.CurrentDevice.Save(PublicVar.MainWnd.ViewModel.CurrentDevice.MyModel, PublicVar.MainWnd.ViewModel.FilePath);

                    DevParam ShowParam = new DevParam("파라미터 정보", ParamType.Category, null, String.Empty);
                    foreach (DevParam subParam in (this.RootParam.Clone() as DevParam).Children)
                    {
                        if (subParam.IsChecked)
                            ShowParam.Children.Add(subParam);
                    }

                    //설정한 파라미터로 업데이트
                    ParameterModel ParamModel = new ParameterModel(ShowParam, false);
                    this.ParamView = new ParameterView();
                    this.ParamView.ContentType = this.MyContent.Type;
                    this.ParamView.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
                    this.ParamView.Tag = ParamModel;
                    this.ParamView._tree.Model = ParamModel;
                    PublicVar.MainWnd.ExpandAll(this.ParamView._tree.Nodes);

                    Int32 count = 0;
                    foreach (var item in this.TunningGrid.Children)
                    {
                        if (item is ParameterView)
                            break;

                        count++;
                    }

                    this.TunningGrid.Children.RemoveAt(count);
                    this.TunningGrid.Children.Add(this.ParamView);
                    Grid.SetRow(this.ParamView, 2);
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
