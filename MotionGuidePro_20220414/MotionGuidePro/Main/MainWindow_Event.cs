using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Aga.Controls.Tree;
using CrevisLibrary;
using DCT_Graph;
using DevExpress.Xpf.Docking;
using Microsoft.Win32;

namespace MotionGuidePro.Main
{
    public partial class MainWindow
    {
        /// <summary>
        /// 윈도우가 로드될 때 호출
        /// </summary>
        private void MotionGuidePro_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsAdministrator())
                {
                    String CmdPath = Environment.CurrentDirectory + "\\firewall_Program_On.cmd";
                    Process.Start(CmdPath);
                }

                this.ViewModel.UseIcon = Properties.Settings.Default.UseIcon;

                this.ConnectBtn.ToolTip = "Connect";

                //XML파일이 있는 기본 경로를 설정해준다.
                //File 설정 정보를 가져온다.
                String[] FileConfig = XmlParser.GetFileConfiguration();
                this.ViewModel.FilePath = FileConfig[0];
                this.ViewModel.BaseFilePath = FileConfig[1];
                DirectoryInfo di = new DirectoryInfo(FileConfig[1]);

                //해당 디렉토리를 확인하여 파일의 정보를 로드
                di = LoadFileInformation(di);

                //Model의 기본 정보를 가져와서 저장한다.
                SaveBasicInformation();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 윈도우가 종료될 때 호출
        /// </summary>
        private void MotionGuidePro_OnClosed(Object sender, EventArgs e)
        {
            try
            {
                if (IsAdministrator())
                {
                    String CmdPath = Environment.CurrentDirectory + "\\firewall_Program_Off.cmd";
                    Process.Start(CmdPath);
                }

                Properties.Settings.Default.UseIcon = this.ViewModel.UseIcon;

                //쓰레드 종료 플래그
                this.ViewModel.IsThreadExit = false;

                //그래프 통신 중단
                if (this.ViewModel.IsConnect)
                {
                    DevParam Param = new DevParam("그래프 통신 중단", ParamType.ByteArray, null, String.Empty);
                    Param.m_DevParamInfo.m_Address = 0x1110;
                    Param.m_DevParamInfo.m_Length = 1;
                    (Param as IByteArrayParam).ArrayValue = new Byte[2] { 0x00, 0x00 };
                    this.ViewModel.CurrentDevice.Write(Param);
                }

                //모니터링 쓰레드 종료
                this.ViewModel.IsMonitoringStart = false;
                if (this.ViewModel.MonitoringThread != null)
                {
                    if (!this.ViewModel.MonitoringThread.Join(1000))
                    {
                        //쓰레드 중지
                        this.ViewModel.MonitoringThread.Abort();
                        this.ViewModel.MonitoringThread = null;
                    }
                }

                //그래프 쓰레드 종료
                OscilloscopeContent Content = this.ViewModel.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent;
                //쓰레드 종료 플래그 설정
                Content.IsExitThread = false;
                if (Content.GraphReceiveThread != null)
                {
                    if (!Content.GraphReceiveThread.Join(1000))
                    {
                        Content.GraphReceiveThread.Abort();
                        Content.GraphReceiveThread = null;
                    }
                }

                if (Content.PacketParserThread != null)
                {
                    if (!Content.PacketParserThread.Join(1000))
                    {
                        Content.PacketParserThread.Abort();
                        Content.PacketParserThread = null;
                    }
                }

                if (Content.UpdateSignalThread != null)
                {
                    if (!Content.UpdateSignalThread.Join(1000))
                    {
                        Content.UpdateSignalThread.Abort();
                        Content.UpdateSignalThread = null;
                    }
                }

                //서버와 연결 해제
                this.ViewModel.CurrentDevice.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 콘텐츠 아이템을 클릭했을 때 호출
        /// </summary>
        private void Content_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            try
            {
                //클릭한 컨텐츠가 아이콘이 존재하는 경우
                if ((sender as TreeViewItem).Header is Grid)
                {
                    Grid headerGrid = (sender as TreeViewItem).Header as Grid;
                    //컨텐츠의 정보를 화면에 출려한다.
                    Add_Content((headerGrid.Children[0] as Label).Content);
                }
                //아이콘 없이 문자열로만 이루어진 경우
                else
                    Add_Content((sender as TreeViewItem).Header);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 아이템이 도킹될 때 호출
        /// </summary>
        private void Manager_DockItemDocked(Object sender, DevExpress.Xpf.Docking.Base.DockItemDockingEventArgs e)
        {
            try
            {
                LayoutPanel pane = e.Item as LayoutPanel;
                if (pane.Caption as String == "Measure")
                {
                    (pane.Content as MeasureControl).Width = Double.NaN;
                    (pane.Content as MeasureControl).Height = Double.NaN;
                }
             }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 컨텐츠 창을 닫을 때 호출
        /// </summary>
        private void Mananger_DockItemClosed(Object sender, DevExpress.Xpf.Docking.Base.DockItemClosedEventArgs e)
        {
            try
            {
                //현재 종료된 패널이 Oscilloscope인지 확인
                Boolean HasOscilloContentInClosePane = false;
                String CompareCaption = (e.Item as LayoutPanel).Caption as String;
                if (CompareCaption == "Oscilloscope")
                    HasOscilloContentInClosePane = true;

                if (HasOscilloContentInClosePane)
                {
                    //오실로스코프 컨텐츠가 닫힐 때 Measure를 보이지 않게 설정
                    foreach (TreeViewItem subItem in (this.treeView.Items[0] as TreeViewItem).Items)
                    {
                        if (subItem.Header is Grid)
                        {
                            if (((subItem.Header as Grid).Children[0] as Label).Content.Equals("Oscilloscope"))
                            {
                                subItem.IsExpanded = false;
                                (subItem.Items[0] as TreeViewItem).Visibility = Visibility.Collapsed;
                                //subItem.Visibility = Visibility.Collapsed;
                                break;
                            }
                        }
                    }

                    LayoutGroup MeasurePanelGroup = null;
                    foreach (LayoutGroup grp in this.MeasurePanel.Items)
                    {
                        if ((grp.Name as String).Equals("MeasureGrp"))
                        {
                            MeasurePanelGroup = grp;
                            break;
                        }
                    }

                    if (MeasurePanelGroup != null)
                        this.MeasurePanel.Remove(MeasurePanelGroup);

                    OscilloscopeContent OscilloContent = this.ViewModel.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent;
                    OscilloContent.WheelScale = 1.0d;

                    //현재 선택된 축정보를 가져온다.
                    foreach (ShowAxisInformation info in OscilloContent.AxisInfoList)
                    {
                        //채널 선택 초기화
                        info.IsChannelSelected = true;
                        //그래프 넓이 값 설정
                        OscilloContent.OscilloscopeWidth += 50;
                        //받은 데이터 초기화
                        OscilloContent.DigitalSignalMap[info.ParamType].SignalData.Clear();
                    }
                }

                //종료한 패널을 담고 있는 리스트를 돌면서 확인
                foreach (var panel in this.Mananger.ClosedPanels)
                {
                    //패널을 가져온다.
                    String PanelCaption = panel.Caption as String;
                    IContent content = this.ViewModel.CurrentDevice.ContentList[PanelCaption];
                    if (content.Type.Equals(ContentType.MonitoringContent))
                        this.ViewModel.IsMonitoringStart = false;

                    Boolean HasItem = false;
                    foreach (MenuItem subItem in this.ShowMenuItem.Items)
                    {
                        //보기 메뉴 아이템 목록중에 현재 추가하려는 항목이 있다면 
                        //HasItem Flag를 바꿔주고 루프 탈출
                        if ((subItem.Header as String).Equals(PanelCaption))
                        {
                            HasItem = true;
                            break;
                        }
                    }

                    //종료된 패널이 보기 메뉴 아이템이 없는 경우만 해당
                    if (!HasItem)
                    {
                        Image Image = null;
                        String ContentName = String.Empty;
                        foreach (TreeViewItem menuItem in (this.treeView.Items[0] as TreeViewItem).Items)
                        {
                            String header = ((menuItem.Header as Grid).Children[0] as Label).Content as String;
                            //String header = menuItem.Header as String;
                            if (header.Equals(PanelCaption))
                            {
                                Image = ((menuItem.Header as Grid).Children[1] as Image);
                                ContentName = header;
                                break;
                            }
                        }

                        if (ContentName.Equals(String.Empty))
                            return;

                        //메뉴 아이템을 하나 만든다.
                        MenuItem item = new MenuItem();

                        //MenuItem의 아이콘
                        Image PanelICon = new Image();
                        PanelICon.Width = 20;
                        PanelICon.Height = 20;
                        PanelICon.Source = Image.Source;
                        item.Icon = PanelICon;

                        //이벤트 등록을 해주고
                        item.Click += ShowMenuItem_Click;
                        //닫은 패널의 이름으로 메뉴 아이템의 이름을 설정한다.
                        item.Header = PanelCaption;
                        //보기 메뉴 아이템에 추가한다.
                        this.ShowMenuItem.Items.Add(item);
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
        /// 보기 메뉴 아이템을 클릭했을 때 호출
        /// </summary>
        private void ShowMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                //보기란에서 선택한 아이템을 가져온다.
                MenuItem item = sender as MenuItem;
                //선택한 컨텐츠를 화면에 표시한다.
                Add_Content(item.Header);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// DocumentGroup의 패널이 선택되었을 때 호출
        /// </summary>
        private void DocGroup_SelectedItemChanged(Object sender, DevExpress.Xpf.Docking.Base.SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.Item == null)
                    return;

                //선택된 패널을 가져온다.
                LayoutPanel panel = e.Item as LayoutPanel;
                String caption = panel.Caption as String;
                if (caption.Equals(" "))
                    panel.Visibility = Visibility.Collapsed;

                if (!caption.Equals("HOMEPAGE"))
                {
                    //오실로스코프 컨텐츠가 Docking될 경우 Measure를 보이게 설정
                    if (panel.Content is OscilloscopeControl)
                    {
                        foreach (TreeViewItem subTreeItem in (this.treeView.Items[0] as TreeViewItem).Items)
                        {
                            if (subTreeItem.Header is Grid)
                            {
                                if (((subTreeItem.Header as Grid).Children[0] as Label).Content.Equals("Oscilloscope"))
                                {
                                    (subTreeItem.Items[0] as TreeViewItem).Visibility = Visibility.Visible;
                                    //subTreeItem.Visibility = Visibility.Visible;
                                    break;
                                }
                            }
                        }

                        //MeasureControl이 추가되어있을 경우 Measure를 보이게 설정
                        foreach (LayoutGroup grp in this.MeasurePanel.Items)
                        {
                            if (grp.Name.Equals("MeasureGrp"))
                            {
                                grp.Visibility = Visibility.Visible;
                                break;
                            }
                        }

                    }
                    //다른 탭을 선택한 경우 Measure창을 보이지 않게 설정
                    else
                    {
                        foreach (LayoutGroup grp in this.MeasurePanel.Items)
                        {
                            if (grp.Name.Equals("MeasureGrp"))
                            {
                                grp.Visibility = Visibility.Collapsed;
                                break;
                            }
                        }
                    }

                    if (this.ViewModel.IsConnect)
                    {
                        if ((this.DocGroup.Items[this.DocGroup.SelectedTabIndex] as LayoutPanel).Content is Grid)
                        {
                            ParameterView View = ((this.DocGroup.Items[this.DocGroup.SelectedTabIndex] as LayoutPanel).Content as Grid).Children[0] as ParameterView;
                            this.ViewModel.CurrentDevice.ContentList[caption].Communication(View, true);
                        }
                    }

                    this.m_IsThreadStop = true;
                }
                else
                    this.m_IsThreadStop = false;

                //TreeView에 하위 아이템이 존재하는 경우
                if (this.treeView.HasItems)
                {
                    TreeViewItem targetItem = null;
                    foreach (TreeViewItem item in (this.treeView.Items[0] as TreeViewItem).Items)
                    {
                        Grid headerGrid = item.Header as Grid;
                        String header = (headerGrid.Children[0] as Label).Content as String;
                        //String header = item.Header as String;
                        //선택된 패널의 헤더와 TreeViewItem의 헤더가 일치하면 
                        if (header.Equals(caption))
                        {
                            //targetItem에 저장하고 루프 탈출
                            targetItem = item;
                            break;
                        }
                    }

                    //TreeViewItem을 현재 선택된 패널의 아이템으로 선택한다.
                    if (targetItem != null)
                        targetItem.IsSelected = true;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss")));
            }
        }

        /// <summary>
        /// 키보드가 눌려졌을 때 호출
        /// </summary>
        private void MotionGuidePro_OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                //새로고침
                if (e.Key.Equals(Key.F5))
                    RefreshBtn_OnClick(sender, e);
                //저장
                else if (e.Key == Key.S && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
                    SaveButton_OnClick(sender, e);
                //오픈
                else if (e.Key == Key.O && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
                    OpenButton_OnClick(sender, e);
                //다른 이름으로 저장
                else if (e.Key == Key.A && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
                    AnotherSaveButton_OnClick(sender, e);
                //설정 창
                else if (e.Key == Key.S && (Keyboard.Modifiers & (ModifierKeys.Shift)) == (ModifierKeys.Shift))
                    SettingBtn_OnClick(sender, e);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 로그 리스트의 사이즈가 변경되었을 때 호출
        /// </summary>
        private void LogList_OnSizeChanged(Object sender, SizeChangedEventArgs e)
        {
            try
            {
                ListView view = sender as ListView;
                //ListView의 넓이 값을 가져온다.
                Double lstWidth = view.ActualWidth;

                //ListView 넓이 값에 비율에 따라서 사이즈를 재 조정한다.
                this.ViewModel.StateWidth = lstWidth * 0.1;
                this.ViewModel.MessageWidth = lstWidth * 0.75;
                this.ViewModel.TimeWidth = lstWidth * 0.25;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss")));
            }
        }


        /// <summary>
        /// Alarm Clear 버튼을 눌렀을 경우 호출
        /// </summary>
        private void AlarmClearBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //AlaramClear 파라미터를 만든다.
                DevParam AlarmClearParam = new DevParam("AlarmClear", ParamType.Short, null, String.Empty,
                    String.Empty, PortType.None, 0, 0, 0x1500, 2);

                //AlarmClear 실행
                this.ViewModel.CurrentDevice.Write(AlarmClearParam);

                //State 정보 업데이트
                this.ViewModel.SetErrorStateInformation();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        #region 메뉴 클릭 이벤트
        /// <summary>
        /// 오픈버튼(Ctrl + O)를 눌렀을 경우 호출
        /// </summary>
        private void OpenButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //파일 선택하기
                OpenFileDialog OpenDlg = new OpenFileDialog();
                //파일 형식이 XML것만 보여지게 설정
                OpenDlg.Filter = "XML-File | *.xml";
                OpenDlg.DefaultExt = ".xml"; // Default file extension
                //선택한 파일 경로를 저장한다.
                if (OpenDlg.ShowDialog() == true)
                    this.ViewModel.FilePath = OpenDlg.FileName;

                //선택한 파일 경로의 모델 정보를 로드한다.
                LoadModel(this.ViewModel.FilePath);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 저장버튼(Ctrl + S)를 눌렀을 경우 호출
        /// </summary>
        private void SaveButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //선택한 파일 경로가 없는 경우 처리
                if (this.ViewModel.FilePath.Equals(String.Empty))
                    throw new Exception("The file has not been loaded.");

                //현재 파일 경로에 변경된 정보를 파일로 저장한다.
                this.ViewModel.CurrentDevice.Save(this.ViewModel.CurrentDevice.MyModel as DevParam, this.ViewModel.FilePath);

                //파일 저장 메세지 띄우기
                String[] fileArr = this.ViewModel.FilePath.Split('\\');
                String FileName = fileArr[fileArr.Length - 1].Split('.')[0];
                this.ViewModel.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, FileName + " Save Completed", DateTime.Now.ToString("HH:mm:ss")));
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 다른 이름으로 저장버튼(Ctrl + A)를 눌렀을 경우 호출
        /// </summary>
        private void AnotherSaveButton_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ViewModel.FilePath.Equals(String.Empty))
                    throw new Exception("The file has not been loaded.");

                //새로운 이름의 파일 경로
                String SelectedFilePath = String.Empty;

                //저장할 위치를 고를 수 있도록 Dialog 띄우기
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "newFile"; // Default file name
                dlg.DefaultExt = ".xml"; // Default file extension
                dlg.Filter = "XML-File | *.xml";

                // 저장할 파일 경로 선택 창 띄우기
                Nullable<bool> result = dlg.ShowDialog();

                // 확인을 누른 경우
                if (result == true)
                {
                    // Save File
                    SelectedFilePath = dlg.FileName;
                    this.ViewModel.FilePath = SelectedFilePath;
                }

                if (this.ViewModel.CurrentDevice.MyModel != null)
                {
                    if (!SelectedFilePath.Equals(String.Empty))
                        this.ViewModel.CurrentDevice.Save(this.ViewModel.CurrentDevice.MyModel as DevParam, SelectedFilePath);
                }

                //파일 저장 메세지 띄우기
                String[] fileArr = SelectedFilePath.Split('\\');
                String FileName = fileArr[fileArr.Length - 1].Split('.')[0];
                this.ViewModel.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, FileName + " Save Completed", DateTime.Now.ToString("HH:mm:ss")));
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        //새로고침(F5) 버튼을 눌렀을 때 호출
        private void RefreshBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //새로고침 메세지 띄울지 말지
                Boolean ShowMessage = true;
                if (sender is Button)
                {
                    Button Btn = sender as Button;
                    //버튼이 HistoryClear인 경우에는 메세지를 띄우지 않는다.
                    if (Btn.Content.Equals("History Clear"))
                        ShowMessage = false;
                }

                //현재 선택된 컨텐츠의 인덱스 정보
                Int32 index = this.DocGroup.SelectedTabIndex;

                //현재 DocumentGroup 하위에 패널들을 저장한다.
                List<LayoutPanel> panelList = new List<LayoutPanel>();
                foreach (LayoutPanel panel in this.DocGroup.Items)
                    panelList.Add(panel);

                //모델 정보 로드
                this.ViewModel.CurrentDevice.MyModel = XmlParser.LoadParam(this.ViewModel.FilePath);

                foreach (LayoutPanel panel in panelList)
                {
                    //패널의 이름이 HOMEPAGE인 경우는 제외
                    if ((panel.Caption as String).Equals("HOMEPAGE"))
                        continue;

                    //패널을 지우고
                    this.DocGroup.Remove(panel);
                    //다시 추가해준다.
                    Add_Content(panel.Caption);
                }

                //새로고침 하기 전에 선택된 인덱스로 설정
                this.DocGroup.SelectedTabIndex = index;

                //새로고침 메세지 띄우기
                if (ShowMessage)
                {
                    String[] fileArr = this.ViewModel.FilePath.Split('\\');
                    String FileName = fileArr[fileArr.Length - 1].Split('.')[0];
                    this.ViewModel.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, FileName + " Reload", DateTime.Now.ToString("HH:mm:ss")));
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// TreeView에서 오른쪽 마우스를 클릭하고 연결버튼을 눌렀을 경우 호출
        /// </summary>
        private void OpenCloseMenu_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                //MenuItem 가져오기
                MenuItem Sender = sender as MenuItem;
                Image img = this.ConnectBtn.Content as Image;

                //연결이 되지 않은 경우에만 실행
                if (!this.ViewModel.IsConnect)
                {
                    Connect_OnClick(sender, e);
                    //연결이 완료됐다면
                    if (!this.ViewModel.IsConnect)
                        throw new Exception("Device is offline");
                }
                else
                    Disconnect_OnClick(sender, e);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 로그 리스트 뷰에서 마우스 오른쪽 버튼을 클릭 후 모두 지우기를 클릭했을 때 호출
        /// </summary>
        private void LogView_OnClearClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //로그 리스트를 모두 지운다.
                this.ViewModel.LogList.Clear();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 설정 버튼을 눌렀을 경우 호출
        /// </summary>
        private void SettingBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //설정 창을 띄운다.
                Setting settingDlg = new Setting();
                settingDlg.Owner = this;
                settingDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                settingDlg.ShowDialog();

                if (settingDlg.IsOK)
                {
                    //설정한 데이터를 저장
                    SaveButton_OnClick(sender, e);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
        #endregion

        #region 추가 기능 버튼 이벤트
        /// <summary>
        /// 데이터 초기화 버튼을 눌렀을 경우 호출
        /// </summary>
        private void DataReset_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //현재 기능을 수행하는 Content의 View와 정보를 가져온다.
                Button btn = sender as Button;
                //화면 정보를 가져온다.
                ParameterView view = (((btn.Tag as LayoutPanel).Content) as Grid).Children[0] as ParameterView;
                String caption = (btn.Tag as LayoutPanel).Caption as String;

                //컨텐츠 이름을 통해서 해당 콘텐츠를 가져온다.
                IContent targetContent = this.ViewModel.CurrentDevice.ContentList[caption];

                //데이터 리셋 실행
                targetContent.DataReset(view);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 데이터 저장 버튼을 눌렀을 경우 호출
        /// </summary>
        private void SaveDataBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //현재 기능을 수행하는 Content의 View와 정보를 가져온다.
                Button btn = sender as Button;
                String caption = (btn.Tag as LayoutPanel).Caption as String;
                IContent Content = this.ViewModel.CurrentDevice.ContentList[caption];

                //데이터 저장 파라미터 만들기
                IParam Param = this.ViewModel.CurrentDevice.MyModel.GetParamNode("Store Parameter");
                //데잍터 저장 실행
                Content.SaveData(Param);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// FaultContent History Clear 버튼을 눌렀을 경우 호출
        /// </summary>
        private void HistoryClearBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                //현재 기능을 수행하는 Content의 View와 정보를 가져온다.
                Button btn = sender as Button;
                ParameterView view = (((btn.Tag as LayoutPanel).Content) as Grid).Children[0] as ParameterView;
                String caption = (btn.Tag as LayoutPanel).Caption as String;
                ParameterModel Model = (view.Tag) as ParameterModel;
                IContent Content = this.ViewModel.CurrentDevice.ContentList[caption];

                //컨텐츠가 FaultContent인 경우
                if (Content is FaultContent)
                {
                    //FalutContent의 History정보를 리셋
                    FaultContent faultContent = Content as FaultContent;
                    faultContent.ClearHistory();
                    RefreshBtn_OnClick(sender, e);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모니터링 버튼이 언체크되었을 때 호출
        /// </summary>
        private void MonitoringStartButton_Unchecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                //현재 모니터링 버튼을 누른 컨텐츠의 파라미터 정보와 화면을 저장한다.
                ToggleButton button = sender as ToggleButton;
                this.ViewModel.MonitoringView = ((button.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                ParameterModel Model = this.ViewModel.MonitoringView.Tag as ParameterModel;
                this.ViewModel.MonitoringParam = Model.Root;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모니터링 버튼이 체크되었을 때 호출
        /// </summary>
        private void MonitoringStartButton_Checked(Object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton button = sender as ToggleButton;

                //연결되지 않은 상태라면 플래그를 false로 바꾸고 리턴
                if (!this.ViewModel.IsConnect)
                {
                    button.IsChecked = false;
                    return;
                }

                this.ViewModel.MonitoringView = ((button.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                ParameterModel Model = this.ViewModel.MonitoringView.Tag as ParameterModel;
                this.ViewModel.MonitoringParam = Model.Root;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 파라미터 선택 버튼을 누른경우 호출
        /// </summary>
        private void ParamSelectBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ParameterView prop = ((btn.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;

                //파라미터 선택 버튼 창 띄우기
                SelectionParameter dlg = new SelectionParameter();
                //현재 해당 파라미터를 넘겨준다.
                dlg.SettingParam = Model.Root.Clone() as DevParam;
                dlg.Owner = this;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.ShowDialog();

                //확인 버튼을 누른경우
                if (dlg.SettingParam != null)
                {
                    //선택한 파라미터로 다시 설정
                    DevParam ModelParam = this.ViewModel.CurrentDevice.MyModel.GetParamNode("Manual").GetParamNode("파라미터 정보") as DevParam;
                    ModelParam.Children.Clear();
                    foreach (IParam subParam in dlg.SettingParam.Children)
                        ModelParam.AddChild(subParam);

                    //설정한 파라미터로 화면을 업데이트 해준다.
                    Model = new ParameterModel(dlg.SettingParam, false);
                    prop.Tag = Model;
                    prop._tree.Model = Model;
                    ExpandAll(prop._tree.Nodes);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 통신 버튼을 눌렀을 경우 호출
        /// </summary>
        private void CommBtn_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ParameterView prop = ((btn.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                String caption = (btn.Tag as LayoutPanel).Caption as String;
                IContent Content = this.ViewModel.CurrentDevice.ContentList[caption];

                //통신 실행
                Content.Communication(prop);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모두 취소 버튼을 눌렀을 경우 호출
        /// </summary>
        private void AllUnCheckBtn_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ParameterView prop = ((btn.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                String caption = (btn.Tag as LayoutPanel).Caption as String;
                IContent Content = this.ViewModel.CurrentDevice.ContentList[caption];

                //모든 파라미터 해제
                Content.AllUnCheck(prop);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모두 선택 버튼을 눌렀을 경우 호출
        /// </summary>
        private void AllCheckBtn_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ParameterView prop = ((btn.Parent as Grid).Parent as Grid).Children[0] as ParameterView;
                String caption = (btn.Tag as LayoutPanel).Caption as String;
                IContent Content = this.ViewModel.CurrentDevice.ContentList[caption];

                //모든 파라미터 선택
                Content.AllCheck(prop);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 선택 / 일반 모드를 클릭했을 때 호출
        /// </summary>
        private void AdminButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                //현재 메뉴 아이템 객체를 가져온다.
                MenuItem target = sender as MenuItem;
                //위에서 설정한 정보들을 가져온다.
                Object[] paramArr = target.Tag as Object[];

                ParameterView prop = paramArr[0] as ParameterView;
                Grid functionGrid = paramArr[1] as Grid;
                IContent Content = paramArr[2] as IContent;

                //버튼의 높이가 0인 경우 => 일반모드 (버튼이 보이면 안되서)
                if (prop.CheckParamWidth.Equals(0.0d))
                {
                    //모두 선택으로 변경
                    Content.AllCheck(prop);

                    //모드 이름 및 높이 변경
                    target.Header = "Normal Mode";
                    functionGrid.Height = 50.0d;
                    prop.CheckParamWidth = 50.0d;
                }
                else
                {
                    target.Header = "Select Mode";
                    functionGrid.Height = 0.0d;
                    prop.CheckParamWidth = 0.0d;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
        #endregion

        /// <summary>
        /// 블랙 테마
        /// </summary>
        private void ThemaUnChecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;
                Image img = btn.Content as Image;
                img.Source = new BitmapImage(new Uri(@"/ICon/ColorInvision_Reverse.png", UriKind.Relative));

                this.ViewModel.ThemaTypeValue = ThemaType.BLACK_THEMA;

                //오실로 스코프 컨텐츠의 테마 색상에 맞게 변경
                OscilloscopeContent Content = this.ViewModel.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent;

                Content.ChangeThema(false);

                /*
                Content.ThemaColor = Brushes.Black;
                Content.ThemaForeColor = Brushes.White;
                Content.ThemaComboColor = Brushes.White;
                Content.SettingColor = Brushes.Black;
                Content.OscilloscopeControl.Cursor1Check.Style = (Style)Content.OscilloscopeControl.FindResource("InspectListCheckBoxWhiteStyle");
                Content.OscilloscopeControl.Cursor2Check.Style = (Style)Content.OscilloscopeControl.FindResource("InspectListCheckBoxWhiteStyle");

                Content.TriggerObj.TriggerLine.mainGrid.Background = Brushes.Black;
                Content.TriggerObj.TriggerLine.line.Stroke = Brushes.White;
                Content.CursorGroup1[0].mainGrid.Background = Brushes.Black;
                Content.CursorGroup1[0].CursorLine.Stroke = Brushes.White;
                Content.CursorGroup1[1].mainGrid.Background = Brushes.Black;
                Content.CursorGroup1[1].CursorLine.Stroke = Brushes.White;
                Content.CursorGroup2[0].mainGrid.Background = Brushes.Black;
                Content.CursorGroup2[0].CursorLine.Stroke = Brushes.White;
                Content.CursorGroup2[1].mainGrid.Background = Brushes.Black;
                Content.CursorGroup2[1].CursorLine.Stroke = Brushes.White;
                */
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 화이트 테마
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThemaChecked(Object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;
                Image img = btn.Content as Image;
                img.Source = new BitmapImage(new Uri(@"/ICon/ColorInvision.png", UriKind.Relative));

                this.ViewModel.ThemaTypeValue = ThemaType.WHITE_THEMA;

                //오실로 스코프 컨텐츠의 테마 색상에 맞게 변경
                OscilloscopeContent Content = this.ViewModel.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent;

                Content.ChangeThema();

                /*
                Content.ThemaColor = Brushes.Transparent;
                Content.ThemaForeColor = Brushes.Black;
                Content.ThemaComboColor = Brushes.Transparent;
                Content.SettingColor = Brushes.FloralWhite;
                Content.OscilloscopeControl.Cursor1Check.Style = (Style)Content.OscilloscopeControl.FindResource("InspectListCheckBoxStyle");
                Content.OscilloscopeControl.Cursor2Check.Style = (Style)Content.OscilloscopeControl.FindResource("InspectListCheckBoxStyle");

                Content.TriggerObj.TriggerLine.mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                Content.TriggerObj.TriggerLine.line.Stroke = Brushes.Black;
                Content.CursorGroup1[0].mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                Content.CursorGroup1[0].CursorLine.Stroke = Brushes.Black;
                Content.CursorGroup1[1].mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                Content.CursorGroup1[1].CursorLine.Stroke = Brushes.Black;
                Content.CursorGroup2[0].mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                Content.CursorGroup2[0].CursorLine.Stroke = Brushes.Black;
                Content.CursorGroup2[1].mainGrid.Background = new SolidColorBrush(Color.FromRgb(0xE9, 0xEE, 0xFF));
                Content.CursorGroup2[1].CursorLine.Stroke = Brushes.Black;
                */
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모니터링 스타트
        /// </summary>
        private void Monitoring_OnStart(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.m_IsMonitorThread = true;
                this.MonitoringBtn.Content = "Monitoring Stop";
                this.m_MonitorThread = new Thread(() => MonitoringStart());
                this.m_MonitorThread.Start();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 모니터링 스탑
        /// </summary>
        private void Monitoring_OnStop(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.m_IsMonitorThread = false;
                this.MonitoringBtn.Content = "Monitoring Start";
                if (!this.m_MonitorThread.Join(1000))
                {
                    this.m_MonitorThread.Abort();
                    this.m_MonitorThread = null;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 연결 버튼을 눌렀을 때 호출
        /// </summary>
        private void Connect_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem item = this.treeView.Items[0] as TreeViewItem;
                MenuItem menuItem = item.ContextMenu.Items[0] as MenuItem;
                Image img = this.ConnectBtn.Content as Image;

                if (!this.ViewModel.IsConnect)
                {
                    //연결 상태로 변경
                    this.ViewModel.IsConnect = this.ViewModel.CurrentDevice.Open();

                    //연결이 완료됐다면
                    if (this.ViewModel.IsConnect)
                    {
                        Process_Connect(true);
                        //현재 Dock되어있는 Document창의 파라미터 정보를 읽는다.
                        foreach (LayoutPanel panel in this.DocGroup.Items)
                        {
                            if ((panel.Caption as String).Equals("HOMEPAGE"))
                                continue;

                            if (panel.Content is Grid)
                            {
                                ParameterView View = ((this.DocGroup.Items[this.DocGroup.SelectedTabIndex] as LayoutPanel).Content as Grid).Children[0] as ParameterView;
                                if (View != null)
                                    this.ViewModel.CurrentDevice.ContentList[panel.Caption as String].Communication(View, true);
                            }
                        }
                    }
                    else
                        throw new Exception("Device is offline");
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 해제 버튼을 눌렀을 때 호출
        /// </summary>
        private void Disconnect_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Image img = this.ConnectBtn.Content as Image;
                TreeViewItem item = this.treeView.Items[0] as TreeViewItem;
                MenuItem menuItem = item.ContextMenu.Items[0] as MenuItem;

                //모니터링 버튼 Enable
                if (this.ViewModel.IsConnect)
                    Process_Connect(false);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void MainWindow_OnRendered(Object sender, EventArgs e)
        {
            try
            {
                Double width = this.BoardDataList.ActualWidth;

                Int32 ColumnCount = this.BoardDataGridView.Columns.Count;
                foreach (GridViewColumn column in this.BoardDataGridView.Columns)
                    column.Width = width / ColumnCount;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }
    }
}
