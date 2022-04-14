using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CrevisLibrary;

namespace DCT_Graph
{
    public partial class MeasurementControl
    {
        private void MeasurementControl_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                Double EachWidth = this.BasicInfoView.ActualWidth / 6;

                this.ViewModel.NameWidth1 = EachWidth;
                this.ViewModel.ValueWidth1 = EachWidth * 2;
                this.ViewModel.NameWidth2 = EachWidth;
                this.ViewModel.ValueWidth2 = EachWidth * 2;
                this.ViewModel.PosWidthCursor1 = this.ViewModel.TimeWidthCursor1 = this.ViewModel.DataWidthCursor1 = EachWidth;
                this.ViewModel.PosWidthCursor2 = this.ViewModel.TimeWidthCursor2 = this.ViewModel.DataWidthCursor2 = EachWidth;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
