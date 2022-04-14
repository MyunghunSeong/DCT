using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aga.Controls.Tree;
using CrevisLibrary;
using DCT_Graph;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;

namespace MotionGuidePro.Main
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        /// <summary>
        /// 에러 처리 이벤트 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        //모든 파라미터 정보를 담을 루트
        private DevParam m_AllParam;

        //모니터링 쓰레드
        private Thread m_MonitorThread;

        //모니터링 쓰레드 종료 플래그
        private Boolean m_IsMonitorThread;

        //모니터링 쓰레드 동작 플래그
        private Boolean m_IsThreadStop;

        //ViewModel 객체
        public MainWindow_ViewModel ViewModel { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //MainWindow 객체를 전역변수에 저장
            PublicVar.MainWnd = this;

            //모니터링 쓰레드
            this.m_IsMonitorThread = true;
            this.m_MonitorThread = null;
            this.m_IsThreadStop = false;

            //변수 초기화
            this.m_AllParam = null;

            //ViewModel 설정
            this.ViewModel = new MainWindow_ViewModel(this);
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;

            //에러처리 등록
            this.LogEvent += this.ViewModel.Log_Maker;
        }

        /// <summary>
        /// 스크롤 위치 업데이트 함수
        /// </summary>
        public void LogView_ScrollEvent(Object sender, LogExecuteEventArgs e)
        {
            try
            {
                //로그 정보를 가져온다.
                LogInformation info = sender as LogInformation;
                //해당 로그 정보 위치로 스크롤 이동
                this.ErrorLstView.ScrollIntoView(info);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// 모니터링으로 가져온 정보 업데이트 함수
        /// </summary>
        public void MonitoringInformationUpdate(Object sender, MonitoringUpdateEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    //모니터링으로 읽어온 데이터로 새로운 모델을 만들고
                    ParameterModel Model = new ParameterModel(e.Param, false);
                    //화면에 모델을 업데이트된걸로 교체한다.
                    e.View._tree.Model = Model;
                    //트리구조 확장해서 보여주기
                    ExpandAll(e.View._tree.Nodes);
                });
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// TreeView에 XML문서로 부터 가져온 Content 목록들을 추가하는 함수
        /// </summary>
        /// <param name="_TreeView">Content 목록을 추가할 TreeView 객체</param>
        private void Make_TreeViewControl(IDevice device)
        {
            try
            {
                //Content에 맞는 이미지 경로 정보를 담는 리스트를 초기화한다.
                this.ViewModel.ImagePathList.Clear();

                //하위 아이템의 스타일을 지장
                Style SubItemStyle = (Style)this.FindResource("treeViewItemStyle");

                // TreeView 하위에 들어갈 아이템을 만든다.(루트는 현재 디바이스의 이름)
                TreeViewItem DeviceItem = new TreeViewItem();
                ContextMenu DeviceContextMenu = new ContextMenu();

                //통신 연결기능 메뉴버튼
                this.ViewModel.OpenCloseMenuItem = new MenuItem();
                this.ViewModel.OpenCloseMenuItem.Click += OpenCloseMenu_Click;
                this.ViewModel.OpenCloseMenuItem.Header = "Connect";
                DeviceContextMenu.Items.Add(this.ViewModel.OpenCloseMenuItem);
                DeviceItem.ContextMenu = DeviceContextMenu;
                DeviceItem.FontSize = 18.0f;
                DeviceItem.Style = SubItemStyle;
                DeviceItem.FontFamily = new FontFamily("Binggrae");
                DeviceItem.Selected += TreeViewItem_Selected;
                DeviceItem.Unselected += TreeViewItem_Unselected;
                this.ViewModel.ImagePathList.Add(device.Name, "pack://application:,,/ICon/PCI_OFF.ico");
                DeviceItem.Header = SetTreeViewItemICon(device.Name);
                //DeviceItem.Header = device.Name;
                DeviceItem.IsExpanded = true;
                this.treeView.Items.Add(DeviceItem);

                //디바이스 아이템 붙여넣기
                foreach (IContent content in device.ContentList.Values)
                {
                    //컨텐츠 항목 이름으로 하위 아이템을 만들어서 추가한다.
                    TreeViewItem item = new TreeViewItem();
                    item.Selected += TreeViewItem_Selected;
                    item.Unselected += TreeViewItem_Unselected;
                    item.Style = SubItemStyle;
                    item.FontSize = 16.0f;
                    item.FontFamily = new FontFamily("Binggrae");
                    //해당 컨텐츠에 맞는 이미지 파일 이름을 가져온다.

                    if (content.Name.Equals("Encoder"))
                        continue;

                    String ImageFile = GetIConOfItem(content.Name);
                    String ToolTip = GetItemToolTip(content.Name);
                    this.ViewModel.ImagePathList.Add(content.Name, "pack://application:,,/ICon/" + ImageFile);
                    //item.Header = content.Name;
                    //해당 컨텐츠에 맞는 이미지 파일이 없는 경우에는 이름으로만 설정
                    if (ImageFile.Equals(String.Empty))
                        item.Header = content.Name;
                    else
                        item.Header = SetTreeViewItemICon(content.Name);
                    item.Margin = new Thickness(0, -3, 0, -3);
                    item.ToolTip = ToolTip;

                    //이벤트 등록
                    item.MouseDoubleClick += Content_MouseDoubleClick;
                    if (content.Name.Equals("Measure"))
                    {
                        foreach (TreeViewItem subItem in DeviceItem.Items)
                        {
                            if (subItem.Header is Grid)
                            {
                                if (((subItem.Header as Grid).Children[0] as Label).Content.Equals("Oscilloscope"))
                                {
                                    subItem.Items.Add(item);
                                    break;
                                }
                            }
                        }
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                        DeviceItem.Items.Add(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void TreeViewItem_Unselected(Object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem item = sender as TreeViewItem;
                ((item.Header as Grid).Children[0] as Label).FontWeight = FontWeights.Regular;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void TreeViewItem_Selected(Object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem item = sender as TreeViewItem;
                ((item.Header as Grid).Children[0] as Label).FontWeight = FontWeights.Bold;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private String GetItemToolTip(String ItemName)
        {
            try
            {
                //해당 이미지 파일의 이름
                String ToopTip = String.Empty;

                //switch문을 이용해 ItemName에 따라 이미지 파일을 할당해준다.
                switch (ItemName)
                {
                    case "Configuration":
                        ToopTip = "Configuration";
                        break;
                    case "Tuning":
                        ToopTip = "Tuning";
                        break;
                    case "LMMT":
                        ToopTip = "LMMT";
                        break;
                    case "Monitor":
                        ToopTip = "Monitor";
                        break;
                    case "Faults":
                        ToopTip = "Faults";
                        break;
                    case "Jog Mode":
                        ToopTip = "Jog Mode";
                        break;
                    case "Oscilloscope":
                        ToopTip = "Oscilloscope";
                        break;
                    case "Manual":
                        ToopTip = "Manual";
                        break;
                    case "ALL":
                        ToopTip = "ALL";
                        break;
                    default:
                        break;
                }

                return ToopTip;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 컨텐츠에 해당하는 이미지 파일 이름을 찾는 함수
        /// </summary>
        /// <param name="ItemName">컨텐츠 이름</param>
        /// <returns>컨텐츠에 맞는 이미지 파일 이름</returns>
        private String GetIConOfItem(String ItemName)
        {
            try
            {
                //해당 이미지 파일의 이름
                String ImageFile = String.Empty;

                //switch문을 이용해 ItemName에 따라 이미지 파일을 할당해준다.
                switch (ItemName)
                {
                    case "Configuration":
                        ImageFile = "Configuration_OFF.ico";
                        break;
                    case "Tuning":
                        ImageFile = "Tuning_OFF.ico";
                        break;
                    case "LMMT":
                        ImageFile = "LMMT_OFF.ico";
                        break;
                    case "Monitor":
                        ImageFile = "Monitor_OFF.ico";
                        break;
                    case "Faults":
                        ImageFile = "Faults_OFF.ico";
                        break;
                    case "Jog Mode":
                        ImageFile = "Jog Mode_OFF.ico";
                        break;
                    case "Oscilloscope":
                        ImageFile = "Oscilloscope_OFF.ico";
                        break;
                    case "Measure":
                        ImageFile = "Measure_OFF.ico";
                        break;
                    case "Manual":
                        ImageFile = "Manual_OFF.ico";
                        break;
                    case "ALL":
                        ImageFile = "ALL_OFF.ico";
                        break;
                    default:
                        break;
                }

                return ImageFile;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 컨텐츠에 아이콘을 설정하는 함수
        /// </summary>
        /// <param name="imagePath">아이콘(이미지) 파일 경로</param>
        /// <param name="Header">컨텐츠 이름</param>
        /// <returns>아이콘 + 컨텐츠로 만들어진 Control</returns>
        public Grid SetTreeViewItemICon(String Header)
        {
            try
            {
                //Grid 컨트롤을 하나 만든다.
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //컨텐츠 이름을 표시 할 Label객체 생성
                Label header = new Label();
                //컨텐츠 이름 설정
                header.Content = Header;
                grid.Children.Add(header);
                Grid.SetColumn(header, 2);

                //이미지 소스 예외처리를 위해 쓴 문장
                String ImageSource = this.ViewModel.ImagePathList[Header];

                //아이콘을 표시할 Imag객체 생성
                Image image = new Image();
                image.SetBinding(Image.VisibilityProperty, new Binding("IconVisible"));
                grid.ColumnDefinitions[0].SetBinding(ColumnDefinition.WidthProperty, new Binding("IconWidth"));
                //image.SetBinding(Image.WidthProperty, new Binding("IconWidth"));
                image.Width = 15;
                image.Height = 15;

                //아이콘 설정
                if (Header.Equals("LMMT"))
                {
                    image.Source = new BitmapImage(new Uri("../ICon/LMMT.ico", UriKind.Relative));
                    image.Width = 20;
                    image.Height = 20;
                    grid.Children.Add(image);
                    Grid.SetColumn(image, 0);
                }
                else
                {
                    image.SetBinding(Image.SourceProperty, new Binding("ImagePathList[" + Header + "]"));
                    grid.Children.Add(image);
                    Grid.SetColumn(image, 0);
                }

                return grid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Tree 확장
        /// </summary>
        /// <param name="Nodes"></param>
        public void ExpandAll(ReadOnlyCollection<TreeNode> Nodes)
        {
            try
            {
                foreach (TreeNode Node in Nodes)
                {
                    Node.IsExpanded = true;
                    if (Node.HasChildren)
                    {
                        ExpandAll(Node.Nodes);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 컨텐츠 정보를 화면에 출력하는 함수
        /// </summary>
        private void Add_Content(Object ContentHeader)
        {
            try
            {
                //선택한 컨텐츠 정보를 담을 객체
                IContent TargetContent = null;

                //Docking을 위한 패널을 생성
                LayoutPanel ContentPane = new LayoutPanel();
                ContentPane.FloatOnDoubleClick = true;
                //현재 컨텐츠의 헤더 정보
                ContentPane.Caption = ContentHeader;

                //Content이름으로 해당 Content 정보를 가져온다.
                ContentType type = new ContentType();
                type = this.ViewModel.CurrentDevice.ContentList[ContentHeader as String].Type;
                TargetContent = this.ViewModel.CurrentDevice.ContentList[ContentHeader as String];

                //선택한 컨텐츠가 보기 메뉴에 있는지 확인하고 있는 경우 제거해준다.
                MenuItem RemoveItem = null;
                foreach (MenuItem ShowItem in this.ShowMenuItem.Items)
                {
                    if (ShowItem.Header.Equals(ContentPane.Caption))
                    {
                        RemoveItem = ShowItem;
                        break;
                    }
                }

                //선택한 컨텐츠가 보기 메뉴에 있는 경우는 메뉴아이템에서 지워준다.
                if (RemoveItem != null)
                    this.ShowMenuItem.Items.Remove(RemoveItem);

                //해당 컨텐츠의 파라미터 정보를 가져온다.
                IParam Param = GetParameterByContent(ContentPane);

                //파라미터 정보을 나타낼 컨트롤을 생성한다.
                MakeViewByContentType(type, ContentPane, TargetContent, Param);

                //화면 연동 유효성 체크
                ValidCheckContentView(ContentPane, type);

                //해당 컨텐츠의 정보를 읽어서 표시
                ReadContentParameters(ContentPane.Caption as String, Param);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 컨텐츠 타입에 따라서 추가 기능을 추가하는 함수
        /// </summary>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="type">컨텐츠 타입</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <returns></returns>
        private Grid MakeAdditionalFunctionByContentType(LayoutPanel ContentPane, ContentType type, IContent TargetContent)
        {
            try
            {
                //기능 버튼을 담을 Grid 선언
                Grid AdditionalGrid = new Grid();

                //타입에 따라서 해당 타입의 기능을 추가해준다.
                switch (type)
                {
                    case ContentType.ParameterContent:
                        AdditionalGrid = AddDataResetFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddDataSaveFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddCommunicationFunction(AdditionalGrid, ContentPane, TargetContent);
                        break;
                    case ContentType.OscilloscopeContent:
                        AdditionalGrid = AddDataResetFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddDataSaveFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddCommunicationFunction(AdditionalGrid, ContentPane, TargetContent);
                        break;
                    case ContentType.MonitoringContent:
                        AdditionalGrid = AddDataResetFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddDataSaveFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddCommunicationFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddMonitoringFunction(AdditionalGrid, ContentPane);
                        break;
                    case ContentType.FaultContent:
                        AdditionalGrid = AddFaultResetFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddDataSaveFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddCommunicationFunction(AdditionalGrid, ContentPane, TargetContent);
                        break;
                    case ContentType.UserManualContent:
                        AdditionalGrid = AddDataResetFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddDataSaveFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddCommunicationFunction(AdditionalGrid, ContentPane, TargetContent);
                        AdditionalGrid = AddUserManualFunction(AdditionalGrid, ContentPane);
                        break;
                    default:
                        throw new Exception(type.ToString() + "is not supported ContentType.");
                }

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터 초기화 기능 추가 함수
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        private Grid AddDataResetFunction(Grid AdditionalGrid, LayoutPanel ContentPane, IContent TargetContent)
        {
            try
            {
                //공간 만들기
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //데이터 초기화 버튼
                Button DataResetBtn = new Button();
                //DataResetBtn.BorderBrush = Brushes.LightBlue;
                DataResetBtn.BorderThickness = new Thickness(2);
                DataResetBtn.Content = "Data Reset";
                DataResetBtn.FontSize = 20.0f;
                //DataResetBtn.Style = (Style)this.FindResource("CustomButtonStyle");
                DataResetBtn.Margin = new Thickness(5);
                DataResetBtn.Tag = ContentPane;
                DataResetBtn.Click += DataReset_OnClick;
                AdditionalGrid.Children.Add(DataResetBtn);
                Grid.SetColumn(DataResetBtn, 0);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 통신 기능 추가
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <param name="Index">해당 기능이 들어갈 위치 값</param>
        private Grid AddCommunicationFunction(Grid AdditionalGrid, LayoutPanel ContentPane, IContent TargetContent, Int32 Index = 2)
        {
            try
            {
                //공간 만들기
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //통신 버튼
                Button CommBtn = new Button();
                CommBtn.BorderThickness = new Thickness(2);
                CommBtn.Tag = ContentPane;
                CommBtn.Click += CommBtn_Click;
                CommBtn.Margin = new Thickness(5);
                CommBtn.Content = "Communication";
                CommBtn.FontSize = 20.0f;
                AdditionalGrid.Children.Add(CommBtn);
                Grid.SetColumn(CommBtn, Index);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 파라미터 값 저장(보드에) 기능 
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <param name="Index">해당 기능이 들어갈 위치 값</param>
        private Grid AddDataSaveFunction(Grid AdditionalGrid, LayoutPanel ContentPane, IContent TargetContent, Int32 Index = 1)
        {
            try
            {
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //데이터 저장 버튼
                Button SaveDataBtn = new Button();
                SaveDataBtn.BorderThickness = new Thickness(2);
                SaveDataBtn.Content = "Save Data";
                SaveDataBtn.FontSize = 20.0f;
                SaveDataBtn.Margin = new Thickness(5);
                SaveDataBtn.Tag = ContentPane;
                SaveDataBtn.Click += SaveDataBtn_OnClick;
                AdditionalGrid.Children.Add(SaveDataBtn);
                Grid.SetColumn(SaveDataBtn, Index);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region FaultContent 전용 기능
        /// <summary>
        /// Error History Clear 기능
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <param name="Index">해당 기능이 들어갈 위치 값</param>
        private Grid AddFaultResetFunction(Grid AdditionalGrid, LayoutPanel ContentPane, IContent TargetContent)
        {
            try
            {
                //공간 만들기
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //데이터 저장 버튼
                Button HistoryClearBtn = new Button();
                HistoryClearBtn.BorderThickness = new Thickness(2);
                HistoryClearBtn.Content = "History Clear";
                HistoryClearBtn.FontSize = 20.0f;
                HistoryClearBtn.Margin = new Thickness(5);
                HistoryClearBtn.Tag = ContentPane;
                HistoryClearBtn.Click += HistoryClearBtn_OnClick;
                AdditionalGrid.Children.Add(HistoryClearBtn);
                Grid.SetColumn(HistoryClearBtn, 0);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region MonitoringContent 전용 기능
        /// <summary>
        /// 모니터링 기능 추가
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <param name="Index">해당 기능이 들어갈 위치 값</param>
        private Grid AddMonitoringFunction(Grid AdditionalGrid, LayoutPanel ContentPane, Int32 Index = 3)
        {
            try
            {
                //공간 만들기
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //파라미터 선택 버튼
                ToggleButton MonitoringStartButton = new ToggleButton();
                MonitoringStartButton.SetBinding(ToggleButton.IsEnabledProperty, new Binding("IsMonitoringEnabled"));
                MonitoringStartButton.BorderThickness = new Thickness(2);
                MonitoringStartButton.SetBinding(ToggleButton.ContentProperty, new Binding("MonitoringMessage"));
                MonitoringStartButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsMonitoringStart"));
                MonitoringStartButton.FontSize = 20.0f;
                MonitoringStartButton.Margin = new Thickness(5);
                MonitoringStartButton.Tag = ContentPane;
                MonitoringStartButton.Checked += MonitoringStartButton_Checked;
                MonitoringStartButton.Unchecked += MonitoringStartButton_Unchecked; ;
                AdditionalGrid.Children.Add(MonitoringStartButton);
                Grid.SetColumn(MonitoringStartButton, Index);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region UserManualContent 전용 기능
        /// <summary>
        /// 원하는 파라미터를 선택해서 볼 수 있는 기능 추가
        /// </summary>
        /// <param name="AdditionalGrid">기능 추가 공간 컨트롤</param>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <param name="TargetContent">컨텐츠</param>
        /// <param name="Index">해당 기능이 들어갈 위치 값</param>
        private Grid AddUserManualFunction(Grid AdditionalGrid, LayoutPanel ContentPane, Int32 Index = 3)
        {
            try
            {
                AdditionalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                //파라미터 선택 버튼
                Button ParamSelectBtn = new Button();
                ParamSelectBtn.BorderThickness = new Thickness(2);
                ParamSelectBtn.Content = "Select Parameter";
                ParamSelectBtn.FontSize = 20.0f;
                ParamSelectBtn.Margin = new Thickness(5);
                ParamSelectBtn.Tag = ContentPane;
                ParamSelectBtn.Click += ParamSelectBtn_OnClick;
                AdditionalGrid.Children.Add(ParamSelectBtn);
                Grid.SetColumn(ParamSelectBtn, Index);

                return AdditionalGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 모델 정보(XML파일)을 로드하는 함수
        /// </summary>
        /// <param name="FilePath">파일 경로</param>
        private void LoadModel(String FilePath)
        {
            try
            {
                //현재 디바이스를 만들고 그에 대한 정보를 가져온다.
                Device device = new Device();
                device.Load(FilePath);

                // 생성한 디바이스 정보를 저장
                this.ViewModel.CurrentDevice = device;
                (this.ViewModel.CurrentDevice as Device).LogEvent += this.ViewModel.Log_Maker;

                //정보를 저장한다.
                device.Name = (device.MyModel.GetParamNode("디바이스 이름") as IStringParam).StrValue;

                //컨텐츠 목록의 이름을 리스트에 저장한다.
                IParam ContentListParam = device.MyModel.GetParamNode("컨텐츠 목록");

                //ALL-Parameter를 만든다.
                this.m_AllParam = Make_AllParameter(ContentListParam);
                Type type = Type.GetType("CrevisLibrary." + ContentType.ParameterContent.ToString());
                IContent content = Activator.CreateInstance(type) as IContent;
                content.Name = this.m_AllParam.ParamInfo.ParamName;
                content.MyDevice = device;
                device.ContentList.Add("ALL", content);
                foreach (IParam ContentParam in ContentListParam.Children)
                {
                    //컨텐츠 타입을 구한다.
                    String strContentType = ContentParam.Value as String;
                    ContentType contentType = new ContentType();
                    Enum.TryParse(strContentType, out contentType);
                    //FaultContentType인 경우에 FaultConfig 파일 정보 가져오기
                    if (contentType.Equals(ContentType.FaultContent))
                        this.ViewModel.FaultConfigList = GetFaultConfigInformation();

                    //컨텐츠 객체를 생성 후 리스트에 저장한다.
                    String ContentName = contentType.ToString();
                    String FullName = "CrevisLibrary." + ContentName;
                    type = Type.GetType(FullName);
                    content = Activator.CreateInstance(type) as IContent;
                    content.Name = ContentParam.ParamInfo.ParamName;
                    content.MyDevice = device;
                    device.ContentList.Add(content.Name, content);

                    //OscilloscopeContent의 경우 Axis의 정보를 가져온다.
                    if (contentType.Equals(ContentType.OscilloscopeContent))
                        XmlParser.GetAxisInformation(content as OscilloscopeContent);
                }

                //하위 트리뷰 아이템 초기화
                this.treeView.Items.Clear();

                // 트리뷰를 만든다.
                Make_TreeViewControl(device);

                //로드 완료 메세지 출력
                String[] fileArr = this.ViewModel.FilePath.Split('\\');
                String FileName = fileArr[fileArr.Length - 1].Split('.')[0];
                this.ViewModel.Log_Maker(this, new LogExecuteEventArgs(LogState.Done, FileName + " Load Completed", DateTime.Now.ToString("HH:mm:ss")));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// FaultContent의 Config 파일 정보를 가져와서 리턴하는 함수
        /// </summary>
        private Dictionary<String, String> GetFaultConfigInformation()
        {
            try
            {
                Dictionary<String, String> List = new Dictionary<String, String>();

                //파일 경로
                String FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\resource\\FaultConfig\\FaultConfig.xml";

                //정보 가져오기
                List = XmlParser.FaultConfigParse(List, FilePath);

                return List;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 모든 컨텐츠의 파라미터의 정보를 보여주는 Content를 만드는 함수
        /// </summary>
        /// <param name="Param">모든 컨텐츠의 정보를 가지고 있는 DevParam 객체</param>
        private DevParam Make_AllParameter(IParam Param)
        {
            try
            {
                //모든 컨텐츠 파라미터의 정보를 담을 루트
                DevParam AllParam = new DevParam("ALL", ParamType.Category, null, String.Empty);

                foreach (DevParam SubParam in Param.Children)
                {
                    //컨텐츠 이름
                    String ContentName = SubParam.ParamInfo.ParamName;

                    //UserManualType과 OscilloscopeType은 제외
                    if (ContentName.Equals("Manual") || ContentName.Equals("Oscilloscope"))
                        continue;

                    //해당 컨텐츠의 이름으로 파라미터 정보를 담을 공간을 만든다.
                    DevParam ContentParam = new DevParam(ContentName, ParamType.Category, null, String.Empty);

                    //컨텐츠의 파라미터 정보를 가져와서 위에서 만든 공간에 저장한다.
                    DevParam ParameterInfo = SubParam.GetParamNode("파라미터 정보") as DevParam;
                    foreach (DevParam InfoParam in ParameterInfo.Children)
                        ContentParam.Children.Add(InfoParam);

                    AllParam.Children.Add(ContentParam);
                }

                return AllParam;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 컨텐츠 화면에 대한 연동 유효성 체크
        /// </summary>
        /// <param name="ContentPane">컨텐츠 화면</param>
        private void ValidCheckContentView(LayoutPanel ContentPane, ContentType type)
        {
            try
            {
                LayoutPanel InClosedPanel = null;
                Boolean IsExistInClosedPanel = false;
                foreach (LayoutPanel panel in this.Mananger.ClosedPanels)
                {
                    //만약에 ClosedPanels에 있는 패널중에 현재 선택한 패널의 캡션과 같은 경우
                    if (panel.Caption.Equals(ContentPane.Caption))
                    {
                        //해당 패널을 저장한다.
                        InClosedPanel = panel;
                        break;
                    }
                }

                //ClosePanels에 현재 추가하려는 패널이 존재한다면
                if (InClosedPanel != null)
                {
                    //DocumentGroup에 Panel이 하나도 없을 경우
                    if (this.DocGroup.GetChildrenCount() <= 0)
                        IsExistInClosedPanel = true;
                    //DocumentGroup에 Panel이 하나라도 있을 경우는 ClosePanel에 있는 목록을 지운다.
                    else
                        this.Mananger.ClosedPanels.Remove(InClosedPanel);
                }

                if (this.Mananger.FloatGroups.Count == 0)
                    this.Mananger.FloatGroups.Add(this.floatGroup);

                LayoutPanel InFloatingPanel = null;
                Boolean IsExistInFloatingPanel = false;
                foreach (var item in this.Mananger.FloatGroups)
                {
                    FloatGroup floatGroup = item as FloatGroup;
                    for (int i = 0; i < floatGroup.GetChildrenCount(); i++)
                    {
                        //만약에 ClosedPanels에 있는 패널중에 현재 선택한 패널의 캡션과 같은 경우
                        if (floatGroup[i].Caption.Equals(ContentPane.Caption))
                        {
                            //해당 패널을 저장한다.
                            InFloatingPanel = floatGroup[i] as LayoutPanel;
                            break;
                        }
                    }
                }

                //Floating(MeasureControl의 경우) 지워준다.
                LayoutPanel RemoveFloatPane = null;
                foreach (LayoutPanel floatPane in this.floatGroup.Items)
                {
                    if (type == ContentType.MeasureContent)
                    {
                        RemoveFloatPane = floatPane;
                        break;
                    }
                }
                if (RemoveFloatPane != null)
                    this.floatGroup.Remove(RemoveFloatPane);

                //Floating된 패널이 없는 경우에는 IsExistInFloatingPanel True처리
                    if (InFloatingPanel != null)
                    IsExistInFloatingPanel = true;

                //TreeView의 Content와 Document에 Content 인덱스 동기화 작업
                Int32 tabIndex = 0;
                foreach (LayoutPanel panel in this.DocGroup.Items)
                {
                    //만약에 TabGroup에 있는 패널중에 현재 선택한 패널의 캡션과 같은 경우
                    if (panel.Caption.Equals(ContentPane.Caption))
                    {
                        //현재 선택된 DockGrop의 패널인덱스를 설정
                        this.DocGroup.SelectedTabIndex = tabIndex;
                        return;
                    }

                    tabIndex++;
                }

                //현재 화면이 종료되지 않았고 화면에 띄워지지 않은 상태라면 Docking
                if (!IsExistInClosedPanel && !IsExistInFloatingPanel)
                {
                    if (type == ContentType.MeasureContent)
                    {
                        LayoutGroup grp = new LayoutGroup();
                        grp.Name = "MeasureGrp";
                        grp.ItemWidth = new GridLength(0.35, GridUnitType.Star);
                        ContentPane.FloatSize = new Size(450, 550);
                        grp.Add(ContentPane);
                        if (this.MeasurePanel.Items.Count <= 1)
                        {
                            this.MeasurePanel.Add(grp);
                        }
                        else if (this.MeasurePanel.Items.Count > 1)
                        {
                            this.MeasurePanel.Items.Clear();
                            this.MeasurePanel.Add(grp);
                        }
                    }
                    else
                    {
                        this.DocGroup.Add(ContentPane);
                        this.DocGroup.SelectedTabIndex = this.DocGroup.GetChildrenCount() - 1;
                        //}
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 현재 컨텐츠의 파라미터 정보를 읽는 함수
        /// </summary>
        private void ReadContentParameters(String ContentCaption, IParam Param)
        {
            try
            {
                //통신 연결이 된 경우에만 처리
                if (this.ViewModel.IsConnect)
                {
                    //Content가 ALL이거나 타입이 userManualContentType인 경우는 여기 처리
                    if (ContentCaption.Equals("ALL") ||
                        this.ViewModel.CurrentDevice.ContentList[ContentCaption].Type.Equals(ContentType.UserManualContent))
                    {
                        //해당 파라미터를 읽어서 최신 정보로 업데이트
                        for (int i = 0; i < Param.Children.Count; i++)
                        {
                            for (int j = 0; j < Param.Children[i].Children.Count; j++)
                            {
                                Param.Children[i].Children[j].MyDevice = this.ViewModel.CurrentDevice;
                                AccessMode BackupMode = Param.Children[i].Children[j].AccessMode;
                                if (Param.Children[i].Children[j].ParamInfo.ParamType.Equals(ParamType.Category))
                                    continue;
                                DevParam RecvParam = this.ViewModel.CurrentDevice.Read(Param.Children[i].Children[j]) as DevParam;
                                Param.Children[i].Children[j].AccessMode = AccessMode.ReadWrite;
                                Param.Children[i].Children[j].Value = RecvParam.Value;
                                Param.Children[i].Children[j].AccessMode = BackupMode;
                            }
                        }
                    }
                    else
                    {
                        //해당 파라미터를 읽어서 최신 정보로 업데이트
                        for (int i = 0; i < Param.Children.Count; i++)
                        {
                            Param.Children[i].MyDevice = this.ViewModel.CurrentDevice;
                            AccessMode BackupMode = Param.Children[i].AccessMode;
                            if (Param.Children[i].ParamInfo.ParamType.Equals(ParamType.Category))
                                continue;
                            DevParam RecvParam = this.ViewModel.CurrentDevice.Read(Param.Children[i]) as DevParam;
                            Param.Children[i].AccessMode = AccessMode.ReadWrite;
                            Param.Children[i].Value = RecvParam.Value;
                            Param.Children[i].AccessMode = BackupMode;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 현재 선택된 컨텐츠의 정보를 가져오는 함수
        /// </summary>
        /// <param name="ContentPane">컨텐츠 화면</param>
        /// <returns></returns>
        private IParam GetParameterByContent(LayoutPanel ContentPane)
        {
            try
            {
                IParam Param = null;
                //선택한 컨텐츠가 ALL인 경우에 처리
                if (ContentPane.Caption.Equals("ALL"))
                    Param = this.m_AllParam as DevParam;
                //그 외 나머지 컨텐츠인 경우
                else
                {
                    Param = this.ViewModel.CurrentDevice.MyModel.GetParamNode(ContentPane.Caption as String).GetParamNode("파라미터 정보");
                }

                //해당 파라미터가 없는 경우 처리
                if (Param == null)
                    throw new Exception("Not found " + "Parameter Information of" + ContentPane.Caption as String);

                return Param;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region UI 만들기

        /// <summary>
        /// 컨텐츠 타입에 따라서 화면을 만드는 함수
        /// </summary>
        /// <param name="type">컨텐츠 타입</param>
        private void MakeViewByContentType(ContentType type, LayoutPanel ContentPane, IContent TargetContent, IParam Param)
        {
            try
            {
                //컨텐츠 타입으로 분기
                switch (type)
                {
                    //Oscilloscope타입의 경우 전용 화면으로 만든다.
                    case ContentType.OscilloscopeContent:
                        OscilloscopeContent OscilContent = TargetContent as OscilloscopeContent;
                        OscilContent.LogEvent += this.ViewModel.Log_Maker;
                        OscilloscopeControl control = new OscilloscopeControl(TargetContent as OscilloscopeContent);
                        //로그 이벤트 등록
                        control.LogEvent += this.ViewModel.Log_Maker;
                        ContentPane.Content = control;
                        break;
                    case ContentType.MeasureContent:
                        MeasureContent measureContent = TargetContent as MeasureContent;
                        measureContent.LogEvent += this.ViewModel.Log_Maker;
                        MeasureControl MeasureCtr = new MeasureControl(measureContent, 
                            this.ViewModel.CurrentDevice.ContentList["Oscilloscope"] as OscilloscopeContent);
                        this.ViewModel.MeasureControl = MeasureCtr;
                        MeasureCtr.LogEvent += this.ViewModel.Log_Maker;
                        ContentPane.Content = MeasureCtr;
                        break;
                    //그 외 컨텐츠
                    default:
                        MakeParameterView(type, ContentPane, TargetContent, Param);
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 파라미터 뷰를 만드는 함수
        /// </summary>
        /// <param name="Type">컨텐츠 타입</param>
        /// <param name="ContentPane">컨텐츠를 포함하는 화면</param>
        /// <param name="TargetContent">해당 컨텐츠</param>
        /// <param name="Param">파라미터 정보</param>
        private void MakeParameterView(ContentType Type, LayoutPanel ContentPane, IContent TargetContent, IParam Param)
        {
            try
            {
                //파라미터 정보를 담을 모델을 생성
                CrevisLibrary.ParameterModel ParamModel = new CrevisLibrary.ParameterModel(Param, false);
                ParameterView prop = new ParameterView();
                prop.LogEvent += this.ViewModel.Log_Maker;
                prop.Tag = ParamModel;
                prop._tree.Model = ParamModel;
                ExpandAll(prop._tree.Nodes);

                //컨텐츠 에러 처리 등록
                switch (TargetContent.Type)
                {
                    case ContentType.FaultContent:
                        (TargetContent as FaultContent).LogEvent += this.ViewModel.Log_Maker;
                        break;
                    case ContentType.MonitoringContent:
                        (TargetContent as MonitoringContent).LogEvent += this.ViewModel.Log_Maker;
                        break;
                    case ContentType.ParameterContent:
                        (TargetContent as ParameterContent).LogEvent += this.ViewModel.Log_Maker;
                        break;
                    case ContentType.UserManualContent:
                        (TargetContent as UserManualContent).LogEvent += this.ViewModel.Log_Maker;
                        break;
                }

                //UserManual의 경우 파라미터 변경 이벤트 등록해주기
                if (Type.Equals(ContentType.UserManualContent))
                    RegisterChangeEvent(ParamModel.Root);

                //컨트롤을 담을 Grid 생성 (파라미터 정보 및 기능)
                Grid DisplayGrid = new Grid();
                DisplayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                DisplayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Pixel) });
                DisplayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                DisplayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50, GridUnitType.Pixel) });
                DisplayGrid.Children.Add(prop);
                Grid.SetRow(prop, 0);

                //GridSplitter
                DisplayGrid = MakeSplitter(DisplayGrid);

                // 모두선택/취소 버튼 그리드 만들기
                Grid SelectFunctionGrid = AttachSelectButton(ContentPane);
                DisplayGrid.Children.Add(SelectFunctionGrid);
                Grid.SetRow(SelectFunctionGrid, 2);

                //선택 / 일반모드에 필요한 정보
                Object[] ParamArr = new Object[3];
                ParamArr[0] = prop;
                ParamArr[1] = SelectFunctionGrid;
                ParamArr[2] = TargetContent;

                // 선택/일반 모드 기능 추가
                DisplayGrid = MakeSeletMode(DisplayGrid, ParamArr);

                //각 컨텐츠타입에 따른 추가 기능을 추가
                Grid AdditionalGrid = MakeAdditionalFunctionByContentType(ContentPane, Type, TargetContent);
                DisplayGrid.Children.Add(AdditionalGrid);
                Grid.SetRow(AdditionalGrid, 3);

                //해당 컨텐츠의 내용에 위에서 생성한 컨트롤을 넣어준다.
                ContentPane.Content = DisplayGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 선택 / 일반모드 추가
        /// </summary>
        /// <param name="grid">부모 그리드</param>
        private Grid MakeSeletMode(Grid grid, Object[] ParamArr)
        {
            try
            {
                ContextMenu GridContextMenu = new ContextMenu();
                //선택/일반모드 기능 메뉴아이템 생성
                MenuItem SelectModeMenuItem = new MenuItem();
                SelectModeMenuItem.Tag = ParamArr;
                SelectModeMenuItem.Header = "Select Mode";
                SelectModeMenuItem.Click += AdminButton_Click;
                GridContextMenu.Items.Add(SelectModeMenuItem);
                grid.ContextMenu = GridContextMenu;

                return grid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// GridSplitter를 추가
        /// </summary>
        /// <param name="grid">부모 그리드</param>
        /// <returns></returns>
        private Grid MakeSplitter(Grid grid)
        {
            try
            {
                GridSplitter splitter = new GridSplitter();
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.Background = Brushes.Transparent;
                grid.Children.Add(splitter);
                Grid.SetRow(splitter, 1);

                return grid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 모두선택 / 취소 버튼을 만드는 함수
        /// </summary>
        /// <param name="ContentPane">컨텐츠가 포함된 화면</param>
        private Grid AttachSelectButton(LayoutPanel ContentPane)
        {
            try
            {
                //버튼을 담을 그리드를 생성
                Grid SelectFunctionGrid = new Grid();
                SelectFunctionGrid.Height = 0.0d;

                //버튼을 생성한다.
                Button AllCheckBtn = new Button();
                Button AllUnCheckBtn = new Button();

                //버튼을 담을 위치를 정의한다.
                SelectFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                SelectFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                //모두 선택 버튼 정의
                AllCheckBtn.BorderBrush = Brushes.LightBlue;
                AllCheckBtn.BorderThickness = new Thickness(2);
                AllCheckBtn.Tag = ContentPane;
                AllCheckBtn.Click += AllCheckBtn_Click;
                AllCheckBtn.Style = (Style)this.FindResource("CustomButtonStyle");
                AllCheckBtn.Margin = new Thickness(5);
                AllCheckBtn.Content = "All Check";
                AllCheckBtn.FontSize = 20.0f;
                SelectFunctionGrid.Children.Add(AllCheckBtn);
                Grid.SetColumn(AllCheckBtn, 0);

                //모두 취소 버튼 정의
                AllUnCheckBtn.BorderBrush = Brushes.LightBlue;
                AllUnCheckBtn.BorderThickness = new Thickness(2);
                AllUnCheckBtn.Tag = ContentPane;
                AllUnCheckBtn.Click += AllUnCheckBtn_Click;
                AllUnCheckBtn.Style = (Style)this.FindResource("CustomButtonStyle");
                AllUnCheckBtn.Margin = new Thickness(5);
                AllUnCheckBtn.Content = "All Uncheck";
                AllUnCheckBtn.FontSize = 20.0f;
                SelectFunctionGrid.Children.Add(AllUnCheckBtn);
                Grid.SetColumn(AllUnCheckBtn, 1);

                return SelectFunctionGrid;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        private void RegisterChangeEvent(DevParam Param)
        {
            try
            {
                //모델의 컨텐츠 목록을 가져온다.
                IParam ContentListParam = this.ViewModel.CurrentDevice.MyModel.GetParamNode("컨텐츠 목록");
                foreach (DevParam ContentParam in ContentListParam.Children)
                {
                    //각 컨텐츠의 파라미터 항목과 현재 userManualContent의 파라미터와 변경이벤트 등록을 해준다.
                    IParam ParamInfoParam = ContentParam.GetParamNode(ContentParam.ParamInfo.ParamName).GetParamNode("파라미터 정보");
                    foreach (DevParam SubParam in ParamInfoParam.Children)
                        CheckIncludeParam(SubParam, Param);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckIncludeParam(IParam Target, IParam Standard)
        {
            try
            {
                //Target : 모델의 파라미터, Standard : UserManualParam
                Boolean result = false;
                foreach (IParam CategoryParam in Standard.Children)
                {
                    foreach (IParam Param in CategoryParam.Children)
                    {
                        //같은 카테고리의 모델이고
                        //선택된 모델의 파라미터 정보와 UserManual에 선택한 파라미터와 같은 경우
                        if (Target.ParamInfo.ParamName.Equals(Param.ParamInfo.ParamName)
                            && Target.Parent.Parent.ParamInfo.ParamName.Equals(CategoryParam.ParamInfo.ParamName))
                        {
                            //파라미터 값이 변경될 때 CallBack 함수를 등록한다.
                            Target.DependencyParamUpdate += this.ViewModel.ParamUpdate;
                            Target.DependencyParams.Add(Param);
                            Param.DependencyParamUpdate += this.ViewModel.ParamUpdate;
                            Param.DependencyParams.Add(Target);
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 디렉토리 하위 파일을 확인하여 정보를 로드하는 함수
        /// </summary>
        /// <param name="di">파일이 있는 디렉토리 정보</param>
        private DirectoryInfo LoadFileInformation(DirectoryInfo di)
        {
            try
            {
                //설정한 파일에 디렉토리가 존재하지 않는 경우
                if (!di.Exists)
                {
                    //현재 프로그램의 기본 위치를 지정
                    this.ViewModel.BaseFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\resource\\XML";
                    di = new DirectoryInfo(this.ViewModel.BaseFilePath);
                    //기본경로에 파일이 존재하는 경우
                    if (di.GetFiles().Count() > 0)
                    {
                        //해당 디렉토리의 첫번째 파일을 가져와서 로드한다.
                        this.ViewModel.FilePath = di.GetFiles()[0].FullName;
                        this.ViewModel.BaseFileName = di.GetFiles()[0].Name;
                        LoadModel(this.ViewModel.FilePath);
                    }

                    //파일을 위의 정보로 다시 저장
                    XmlParser.SetFileConfiguration(this.ViewModel.FilePath, this.ViewModel.BaseFilePath);
                }
                //기본경로에 파일이 존재하는 경우
                else
                {
                    //파일의 개수가 1개 이상인 경우
                    if (di.GetFiles().Count() > 0)
                    {
                        foreach (FileInfo info in di.GetFiles())
                        {
                            //설정한 파일의 경로와 일치하는 경우 파일을 로드한다.
                            if (info.FullName.Equals(this.ViewModel.FilePath))
                            {
                                this.ViewModel.BaseFileName = info.Name;
                                LoadModel(this.ViewModel.FilePath);
                                break;
                            }
                        }
                    }
                }

                return di;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 파일 기본 정보를 저장하는 함수
        /// </summary>
        private void SaveBasicInformation()
        {
            try
            {
                //디바이스 파라미터를 가져온다.
                String ServerIP = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("Server IP") as IStringParam).StrValue;
                String ClientIP = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("Client IP") as IStringParam).StrValue;
                Int32 Port = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("Port") as IIntParam).IntValue;
                Single Timeout = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("Timeout") as IFloatParam).FloatValue;
                Int32 ErrorStatePeriod = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("ErrorStatePeriod") as IIntParam).IntValue;
                Double MonitoringTime = (this.ViewModel.CurrentDevice.MyModel.GetParamNode("MonitoringTime") as IFloatParam).FloatValue;
                this.ViewModel.MonitoringMinValue = this.ViewModel.CurrentDevice.MyModel.GetParamNode("MonitoringTime").ParamInfo.Min;

                //디바이스 정보를 ViewModel에 저장
                this.ViewModel.ServerAddress = ServerIP;
                this.ViewModel.ClientAddress = ClientIP;
                this.ViewModel.CurrentDevice.Port = Port;
                Int32 TimeoutLength = 0;
                if (Timeout.ToString().Contains("."))
                {
                    String[] tmpArr = Timeout.ToString().Split('.');
                    TimeoutLength = tmpArr[1].Length;
                }
                this.ViewModel.Timeout = Timeout;
                this.ViewModel.Timeout = Math.Round(this.ViewModel.Timeout, TimeoutLength);
                this.ViewModel.ErrorStatePeriod = ErrorStatePeriod;
                this.ViewModel.Timer.Interval = TimeSpan.FromSeconds(this.ViewModel.ErrorStatePeriod);
                this.ViewModel.MonitoringTime = MonitoringTime;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void aaa(Object sender, RoutedEventArgs e)
        {
            UserControl control = (UserControl)this.FindResource("test");
            RenderTargetBitmap rtb = new RenderTargetBitmap(400, 150, 96, 96, PixelFormats.Pbgra32);
            Rect bounds = VisualTreeHelper.GetDescendantBounds(control);
            Rect aaa = new Rect(new Size(1000, 1000));
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(control);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), aaa.Size));
            }
            rtb.Render(dv);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            using (Stream fileStream = new FileStream(@"C:\Users\성명훈\Desktop\새 폴더\test.png", FileMode.Create))
            {
                png.Save(fileStream);
            }
        }

        private void MonitoringStart()
        {
            try
            {
                if (!this.ViewModel.IsConnect)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        this.MonitoringBtn.IsChecked = false;
                    });
                    throw new Exception("Device is not connect");
                }

                while (this.m_IsMonitorThread)
                {
                    if (this.m_IsThreadStop)
                        continue;

                    DevParam Param = null;
                    this.Dispatcher.Invoke((Action)delegate() {
                        Param = this.ViewModel.CurrentDevice.MyModel.GetParamNode("Encoder").GetParamNode("파라미터 정보") as DevParam;
                    });

                    foreach (DevParam subParam in Param.Children)
                    {
                        IParam ReadParam = null;
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            ReadParam = this.ViewModel.CurrentDevice.Read(subParam);
                        });

                        if (ReadParam == null)
                        {
                            this.Dispatcher.Invoke((Action)delegate () 
                            {
                                Process_Connect(false);
                                this.MonitoringBtn.IsChecked = false;
                            });
                            throw new Exception("Failed to read data");
                        }

                        this.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            switch (ReadParam.ParamInfo.ParamName)
                            {
                                case "Encoder A Valid Data":
                                    //왼쪽 LED 값
                                    //Int32 LeftSide = (Param.GetParamNode("Encoder A Valid Data") as IIntParam).IntValue;
                                    Int32 LeftSide = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (LeftSide == 0)
                                    {
                                        this.LeftSide.Source = new BitmapImage(new Uri(@"../ICon/PCB.png", UriKind.Relative));
                                        this.ViewModel.FirstLineList[0].LightColor = Brushes.White;
                                    }
                                    else
                                    {
                                        this.LeftSide.Source = new BitmapImage(new Uri(@"../ICon/PCB_LeftON.png", UriKind.Relative));
                                        this.ViewModel.FirstLineList[0].LightColor = Brushes.Lime;
                                    }
                                    break;
                                case "Encoder B Valid Data":
                                    //오른쪽 LED 값
                                    //Int32 RightSide = (Param.GetParamNode("Encoder B Valid Data") as IIntParam).IntValue;
                                    Int32 RightSide = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (RightSide == 0)
                                    {
                                        this.RightSide.Source = new BitmapImage(new Uri(@"../ICon/PCB.png", UriKind.Relative));
                                        this.ViewModel.SecondLineList[0].LightColor = Brushes.White;
                                    }
                                    else
                                    {
                                        this.RightSide.Source = new BitmapImage(new Uri(@"../ICon/PCB_RightON.png", UriKind.Relative));
                                        this.ViewModel.SecondLineList[0].LightColor = Brushes.Lime;
                                    }
                                    break;
                                case "Encoder A Data":
                                    //Int32 EncoderCount = (Param.GetParamNode("Encoder A Data") as IIntParam).IntValue;
                                    Int32 EncoderCount = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    this.ViewModel.EncoderCountA = EncoderCount;
                                    break;
                                case "Encoder B Data":
                                    //Int32 EncoderCount2 = (Param.GetParamNode("Encoder B Data") as IIntParam).IntValue;
                                    Int32 EncoderCount2 = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    this.ViewModel.EncoderCountB = EncoderCount2;
                                    break;
                                case "Encoder A Left Data":
                                    //Int32 LeftDataA = (Param.GetParamNode("Encoder A Left Data") as IIntParam).IntValue;
                                    Int32 LeftDataA = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (LeftDataA == 0)
                                        this.ViewModel.FirstLineList[3].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.FirstLineList[3].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder B Left Data":
                                    //Int32 LeftDataB = (Param.GetParamNode("Encoder B Left Data") as IIntParam).IntValue;
                                    Int32 LeftDataB = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (LeftDataB == 0)
                                        this.ViewModel.SecondLineList[3].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.SecondLineList[3].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder A W Data":
                                    //Int32 WDataA = (Param.GetParamNode("Encoder A W Data") as IIntParam).IntValue;
                                    Int32 WDataA = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (WDataA == 0)
                                        this.ViewModel.FirstLineList[4].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.FirstLineList[4].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder B W Data":
                                    //Int32 WDataB = (Param.GetParamNode("Encoder B W Data") as IIntParam).IntValue;
                                    Int32 WDataB = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (WDataB == 0)
                                        this.ViewModel.SecondLineList[4].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.SecondLineList[4].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder A V Data":
                                    //Int32 VDataA = (Param.GetParamNode("Encoder A V Data") as IIntParam).IntValue;
                                    Int32 VDataA = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (VDataA == 0)
                                        this.ViewModel.FirstLineList[5].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.FirstLineList[5].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder B V Data":
                                    //Int32 VDataB = (Param.GetParamNode("Encoder B V Data") as IIntParam).IntValue;
                                    Int32 VDataB = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (VDataB == 0)
                                        this.ViewModel.SecondLineList[5].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.SecondLineList[5].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder A U Data":
                                    //Int32 UDataA = (Param.GetParamNode("Encoder A U Data") as IIntParam).IntValue;
                                    Int32 UDataA = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (UDataA == 0)
                                        this.ViewModel.FirstLineList[6].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.FirstLineList[6].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder B U Data":
                                    //Int32 UDataB = (Param.GetParamNode("Encoder B U Data") as IIntParam).IntValue;
                                    Int32 UDataB = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (UDataB == 0)
                                        this.ViewModel.SecondLineList[6].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.SecondLineList[6].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder A Right Data":
                                    //Int32 RightDataA = (Param.GetParamNode("Encoder A Right Data") as IIntParam).IntValue;
                                    Int32 RightDataA = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (RightDataA == 0)
                                        this.ViewModel.FirstLineList[7].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.FirstLineList[7].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder B Right Data":
                                    //Int32 RightDataB = (Param.GetParamNode("Encoder B Right Data") as IIntParam).IntValue;
                                    Int32 RightDataB = ((ReadParam as DevParam) as IIntParam).IntValue;
                                    if (RightDataB == 0)
                                        this.ViewModel.SecondLineList[7].LightColor = Brushes.White;
                                    else
                                        this.ViewModel.SecondLineList[7].LightColor = Brushes.Lime;
                                    break;
                                case "Encoder PHS Position":
                                    //String PHSValue = (Param.GetParamNode("Encoder PHS Position") as IEnumParam).EnumStrValue;
                                    String PHSValue = ((ReadParam as DevParam) as IEnumParam).EnumStrValue;
                                    for (int i = 0; i < PHSValue.Length; i++)
                                    {
                                        if (PHSValue[i] == '1')
                                        {
                                            this.ViewModel.PHSLineList[3 + i].LightColor = Brushes.Lime;
                                            this.ViewModel.BottomLedList[i].LightColor = Brushes.Lime;
                                        }
                                        else
                                        {
                                            this.ViewModel.PHSLineList[3 + i].LightColor = Brushes.White;
                                            this.ViewModel.BottomLedList[i].LightColor = Brushes.Silver;
                                        }
                                    }
                                    break;
                            }
                        });
                    }
                    Thread.Sleep(200);
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));

                //쓰레드 종료 루틴
                if (this.m_MonitorThread != null)
                {
                    this.m_IsMonitorThread = false;
                    if (!this.m_MonitorThread.Join(1000))
                    {
                        if (this.m_MonitorThread != null)
                        {
                            this.m_MonitorThread.Abort();
                            this.m_MonitorThread = null;
                        }
                    }
                }
            }
        }

        //연결 / 해제 루틴 실행
        public void Process_Connect(Boolean IsConnect)
        {
            try
            {
                //연결 상태로 변경
                if (IsConnect)
                {
                    WorkspacePanel.Caption = "Workspace(Online)";

                    this.ViewModel.OnVisible = Visibility.Collapsed;

                    this.ViewModel.OffVisible = Visibility.Visible;

                    //Menu의 Header를 해제로 변경
                    this.ViewModel.OpenCloseMenuItem.Header = "Disconnect";

                    //모니터링 버튼 Disable
                    this.ViewModel.IsMonitoringEnabled = true;
                }
                //해제 상태로 변경
                else
                {
                    this.ViewModel.IsMonitoringEnabled = false;

                    this.ViewModel.OnVisible = Visibility.Visible;

                    this.ViewModel.OffVisible = Visibility.Collapsed;

                    WorkspacePanel.Caption = "Workspace(Offline)";

                    //Menu의 Header를 연결로 변경
                    this.ViewModel.OpenCloseMenuItem.Header = "Connect";

                    //연결 해제
                    this.ViewModel.CurrentDevice.Close();

                    //해제 상태로 변경
                    this.ViewModel.IsConnect = false;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 현재 프로그램이 관리자 권한으로 실행됐는지 확인하는 함수
        /// </summary>
        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false;
        }

        private void BoardDataList_SizeChanged(Object sender, SizeChangedEventArgs e)
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
