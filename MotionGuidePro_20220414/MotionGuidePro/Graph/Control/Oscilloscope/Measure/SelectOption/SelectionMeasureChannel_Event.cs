using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrevisLibrary;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace CrevisLibrary
{
    public partial class SelectionMeasureChannel
    {
        /// <summary>
        /// OK 버튼을 눌렀을 때 호출
        /// </summary>
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsOK = true;
                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Cancel 버튼을 눌렀을 때 호출
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

        private void SelectionMeasureControl_OnPreviewLeftMouseDown(Object sender, MouseButtonEventArgs e)
        {
            try
            {
                //현재 마우스의 위치정보를 가져온다.
                this.m_startPoint = e.GetPosition(null);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void SelectionMeasureControl_OnMouseMove(Object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = this.m_startPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                       (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                              Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    /*
                    // Get the dragged ListViewItem
                    ListView listView = sender as ListView;
                    ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                    if (listViewItem == null) return;           // Abort
                                                                // Find the data behind the ListViewItem
                    AxisInformation item = (AxisInformation)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    if (item == null) return;                   // Abort
                                                                // Initialize the drag & drop operation
                    this.m_startIndex = lstView.SelectedIndex;
                    DataObject dragData = new DataObject("AxisInformation", item);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                    */
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void SelectionMeasureControl_OnDragEnter(Object sender, DragEventArgs e)
        {
            try
            {
                //현재 CvsInspectItem로 데이터를 변환할 수 없을 때
                if (!e.Data.GetDataPresent("AxisInformation") || sender != e.Source)
                {
                    //Effect를 None으로 설정
                    e.Effects = DragDropEffects.None;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void SelectionMeasureControl_OnDrop(Object sender, DragEventArgs e)
        {
            try
            {
                Int32 index = -1;

                if (e.Data.GetDataPresent("AxisInformation") && sender == e.Source)
                {
                    // Get the drop ListViewItem destination
                    ListView listView = sender as ListView;
                    ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                    if (listViewItem == null)
                    {
                        // Abort
                        e.Effects = DragDropEffects.None;
                        return;
                    }
                    // Find the data behind the ListViewItem
                    AxisInformation item = (AxisInformation)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    // Move item into observable collection 
                    // (this will be automatically reflected to lstView.ItemsSource)
                    e.Effects = DragDropEffects.Move;
                    index = this.ViewModel.AxisInfoList.IndexOf(item);
                    if (this.m_startIndex >= 0 && index >= 0)
                    {
                        this.ViewModel.AxisInfoList.Move(this.m_startIndex, index);
                    }

                    //동기화 작업해주기

                    //검사항목 리스트 변경 플래그 활성화 시키기
                    //this.ViewModel.IsChangedInspectSequence = true;

                    this.m_startIndex = -1;        // Done!
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Select all가 체크되었을 때 호출
        /// </summary>
        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ViewModel != null)
                {
                    foreach (AxisInformation info in this.ViewModel.AxisInfoList)
                        info.IsChecked = true;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Select all가 체크 해제 되었을 때 호출
        /// </summary>
        private void CheckBox_Unchecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (AxisInformation info in this.ViewModel.AxisInfoList)
                    info.IsChecked = false;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
