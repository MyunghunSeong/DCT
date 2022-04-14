using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CrevisLibrary
{
    public partial class SelectMeasureParam
    {
        /// <summary>
        /// OK를 눌렀을 때 호출
        /// </summary>
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                SelectMeasureParam_ViewModel viewModel = this.DataContext as SelectMeasureParam_ViewModel;

                this.IsOK = true;

                for(int i = 0; i < this.RootParam.Children.Count; i++)
                {
                    (this.RootParam.Children[i] as DevParam).m_DevParamInfo.m_Default =
                        viewModel.ParamInfoList[i].IsDefault;
                    (this.RootParam.Children[i] as DevParam).IsChecked =
                        viewModel.ParamInfoList[i].IsChecked;
                }

                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:dd"), err.StackTrace));
            }
        }

        /// <summary>
        /// CANCLE 를 눌렀을 때 호출
        /// </summary>
        private void CancleBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsOK = false;
                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:dd"), err.StackTrace));
            }
        }

        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            try
            {
                //체크를 모두 해제
                SelectMeasureParam_ViewModel viewModel = this.DataContext as SelectMeasureParam_ViewModel;
                if (viewModel != null)
                {
                    foreach (MeasureParamInformation info in viewModel.ParamInfoList)
                        info.IsChecked = true;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:dd"), err.StackTrace));
            }
        }

        private void CheckBox_Unchecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                //체크를 모두 해제
                SelectMeasureParam_ViewModel viewModel = this.DataContext as SelectMeasureParam_ViewModel;
                foreach (MeasureParamInformation info in viewModel.ParamInfoList)
                    info.IsChecked = false;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:dd"), err.StackTrace));
            }
        }
    }
}
