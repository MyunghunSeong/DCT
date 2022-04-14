using Aga.Controls.Tree;
using CrevisLibrary;
using MotionGuidePro.Main;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Threading;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;
using System.Text.RegularExpressions;

namespace CrevisLibrary
{
    /// <summary>
    /// UserProperty.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ParameterView : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        #region 컨텐츠 타입 프로퍼티
        public static readonly DependencyProperty ContentTypeProperty = DependencyProperty.Register("ContentType",
            typeof(ContentType), typeof(ParameterView), new PropertyMetadata(null));
        public ContentType ContentType
        {
            get { return (ContentType)GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }
        #endregion

        #region ColumnWidth 프로퍼티
        //파라미터 이름 넓이
        public static readonly DependencyProperty ParamNameWidthProperty = DependencyProperty.Register("ParamNameWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(200.0d));
        public Double ParamNameWidth
        {
            get { return (Double)GetValue(ParamNameWidthProperty); }
            set { SetValue(ParamNameWidthProperty, value); }
        }

        //파라미터 값 넓이
        public static readonly DependencyProperty ParamValueWidthProperty = DependencyProperty.Register("ParamValueWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(200.0d));
        public Double ParamValueWidth
        {
            get { return (Double)GetValue(ParamValueWidthProperty); }
            set { SetValue(ParamValueWidthProperty, value); }
        }

        //파라미터 단위 넓이
        public static readonly DependencyProperty ParamUnitWidthProperty = DependencyProperty.Register("ParamUnitWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(120.0d));
        public Double ParamUnitWidth
        {
            get { return (Double)GetValue(ParamUnitWidthProperty); }
            set { SetValue(ParamUnitWidthProperty, value); }
        }

        //파라미터 기본 값 넓이
        public static readonly DependencyProperty ParamInitValueWidthProperty = DependencyProperty.Register("ParamInitValueWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(120.0d));
        public Double ParamInitValueWidth
        {
            get { return (Double)GetValue(ParamInitValueWidthProperty); }
            set { SetValue(ParamInitValueWidthProperty, value); }
        }

        //파라미터 최대 값 넓이
        public static readonly DependencyProperty ParamMaxWidthProperty = DependencyProperty.Register("ParamMaxWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(120.0d));
        public Double ParamMaxWidth
        {
            get { return (Double)GetValue(ParamMaxWidthProperty); }
            set { SetValue(ParamMaxWidthProperty, value); }
        }

        //파라미터 최소 값 넓이
        public static readonly DependencyProperty ParamMinWidthProperty = DependencyProperty.Register("ParamMinWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(120.0d));
        public Double ParamMinWidth
        {
            get { return (Double)GetValue(ParamMinWidthProperty); }
            set { SetValue(ParamMinWidthProperty, value); }
        }

        //파라미터 설명 값 넓이
        public static readonly DependencyProperty DescriptionWidthProperty = DependencyProperty.Register("DescriptionWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(120.0d));
        public Double DescriptionWidth
        {
            get { return (Double)GetValue(DescriptionWidthProperty); }
            set { SetValue(DescriptionWidthProperty, value); }
        }

        //파라미터 선택 값 넓이
        public static readonly DependencyProperty CheckParamWidthProperty = DependencyProperty.Register("CheckParamWidth",
            typeof(Double), typeof(ParameterView), new PropertyMetadata(0.0d));
        public Double CheckParamWidth
        {
            get { return (Double)GetValue(CheckParamWidthProperty); }
            set { SetValue(CheckParamWidthProperty, value); }
        }
        #endregion

        public delegate void SelectedItemChangeCallback(TreeNode SelectedItem);

        /// <summary>
        /// SelectedItem 변경 콜백
        /// </summary>
        public SelectedItemChangeCallback SelectedItemChange { get; set; }

        //통신 완료 표시를 위한 타이머
        private DispatcherTimer m_timer;

        //private Object m_PreValue;

        public TreeNode m_MySelectedItem = null;
        // Tree뷰에서 선택된 노드
        public TreeNode MySelectedItem
        {
            get
            {
                return m_MySelectedItem;
            }
            set
            {
                // 같으면 패스
                if (this.m_MySelectedItem == value)
                    return;

                // 변경된거 적용
                this.m_MySelectedItem = value;
                this.NotifyPropertyChanged("MySelectedItem");

                // 변경 콜백 함수 호출
                if (this.SelectedItemChange != null)
                    this.SelectedItemChange(this.m_MySelectedItem);
            }
        }

        #region 프로퍼티 변경 이벤트
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public ParameterView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.m_timer = new DispatcherTimer();
            this.m_timer.Interval = TimeSpan.FromMilliseconds(500);
            this.m_timer.Tick += DisplayCommEndSignal;
            //this.LogEvent += (PublicVar.MainWnd.ViewModel).Log_Maker;
        }

        /// <summary>
        /// 통신 완료 시그널 표시
        /// </summary>
        private void DisplayCommEndSignal(Object sender, EventArgs e)
        {
            try
            {
                DispatcherTimer timer = sender as DispatcherTimer;
                DevParam Param = timer.Tag as DevParam;
                //통신(Enter)를 눌렀을 때 초록색으로 표시
                Param.CommEndBrush = Brushes.Transparent;
                this.m_timer.Stop();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// Param 노드 GUI 업데이트
        /// </summary>
        public void UpdateNode(IParam Param)
        {
            try
            {
                TreeNode Node = null;
                foreach (var node in this._tree.Nodes)
                {
                    Node = FindNode(node, Param);
                    if (Node != null)
                        break;
                }

                // 업데이트 노드 없으면 그냥 리턴되게....
                if (Node == null)
                    return;

                // GUI 업데이트
                Node.IsExpanded = Node.IsExpanded ? false : true;
                Node.IsExpanded = Node.IsExpanded ? false : true;
                Node.IsExpanded = true;  // 일단 무조건 확장

                // 포커스 이동
                this._tree.SelectedItem = Node;
                // 스크롤 이동
                this._tree.ScrollIntoView(Node);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Param 이 있는 노드 찾기
        /// </summary>
        /// <param name="Node">검색 노드</param>
        /// <param name="Param">찾는 파라미터</param>
        /// <returns>Param가 있는 노드 </returns>
        private TreeNode FindNode(TreeNode Node, IParam Param)
        {
            try
            {
                // 전달 받은 Node 확인 
                IParam Compare = Node.Tag as IParam;
                if (Param.Equals(Compare))
                    return Node;

                // 자식 노들 있으면 확인
                if (!Node.HasChildren)
                    return null;

                TreeNode Find = null;
                //Node.IsExpanded = true; // 확장 해줘야지 검색 가능
                foreach (var chield in Node.Nodes)
                {
                    Find = FindNode(chield, Param);

                    // 찾았으면 break
                    if (Find != null)
                        break;
                }
                return Find;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Tree리스트에서 클릭 하면 들어오는 곳
        private void OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.MySelectedItem = (sender as TreeListItem).Node;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 파일 선택 템플릿에서 파일 선택 버튼을 누른경우 호출
        /// </summary>
        private void FileSelectBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                DevParam param = btn.Tag as DevParam;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    (param as IFileSelectParam).FileSelectValue = openFileDialog.FileName;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private ICommand m_CmdEnter;
        public ICommand CmdEnter
        {
            get
            {
                if (this.m_CmdEnter == null)
                {
                    this.m_CmdEnter = new RelayCommand(Excute_CmdEnter, CanExcute_CmdEnter);
                }
                return this.m_CmdEnter;
            }
            set
            {
                this.m_CmdEnter = value;
            }
        }

        /// <summary>
        /// Enter 키를 눌렀을 경우 이쪽으로 호출됨
        /// </summary>
        /// <param name="obj">컨트롤을 소유하고 있는 객체</param>
        public void Excute_CmdEnter(Object obj)
        {
            try
            {
                //해당 객체(TreeNode)의 정보(DevParam)를 가져온다.
                TreeNode target = obj as TreeNode;
                DevParam Param = target.Tag as DevParam;
                Byte[] WriteBuffer = null;

                //Min, Max 유효성 체크해서 벗어나면 리턴
                if (Param.ParamInfo.ParamType == ParamType.Integer)
                {
                    if (Param.IntValue > Param.ParamInfo.Max
                        || Param.IntValue < Param.ParamInfo.Min)
                    {
                        Param.IntValue = Convert.ToInt32(Param.ParamInfo.InitValue);
                        return;
                    }
                }
                else if (Param.ParamInfo.ParamType == ParamType.Short)
                {
                    if (Param.ShortValue > Param.ParamInfo.Max
                        || Param.ShortValue < Param.ParamInfo.Min)
                    {
                        Param.ShortValue = Convert.ToInt16(Param.ParamInfo.InitValue);
                        return;
                    }
                }

                //ReadOnly인 경우 리턴처리한다.
                if (Param.AccessMode.Equals(AccessMode.ReadOnly))
                    return;

                //파라미터 데이터 형식에 따라서 다르게 처리해준다.
                switch (Param.ParamInfo.ParamType)
                {
                    case ParamType.Integer:
                        Int32 intValue = (Param as IIntParam).IntValue;
                        Byte[] tmp = BitConverter.GetBytes(intValue);
                        WriteBuffer = new Byte[Param.ParamInfo.Length];
                        Array.Copy(tmp, 0, WriteBuffer, 0, WriteBuffer.Length);
                        break;
                    case ParamType.String:
                        String strValue = (Param as IStringParam).StrValue;
                        WriteBuffer = Encoding.Default.GetBytes(strValue);
                        break;
                    case ParamType.Enum:
                        Int32 enumValue = (Param as IEnumParam).EnumIntValue;
                        WriteBuffer = BitConverter.GetBytes(enumValue);
                        break;
                    case ParamType.Byte:
                        Byte byteValue = (Param as IByteParam).ByteValue;
                        WriteBuffer = new Byte[1] { byteValue };
                        break;
                    case ParamType.ByteArray:
                        Byte[] arrayValue = (Param as IByteArrayParam).ArrayValue;
                        WriteBuffer = arrayValue;
                        break;
                    case ParamType.Short:
                        Int16 shortValue = (Param as IShortParam).ShortValue;
                        WriteBuffer = BitConverter.GetBytes(shortValue);
                        break;
                }

                //통신 하기
                MainWindow_ViewModel viewModel = PublicVar.MainWnd.ViewModel;
                viewModel.CurrentDevice.Write(Param);
                Thread.Sleep(50);
                IParam RecvParam = viewModel.CurrentDevice.Read(Param);
                if (RecvParam == null)
                {
                    PublicVar.MainWnd.Process_Connect(false);
                    throw new Exception("Failed to read data");
                }
                Param.Value = RecvParam.Value;

                this.m_timer.Start();
                this.m_timer.Tag = Param;
                Param.CommEndBrush = Brushes.Green;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        public bool CanExcute_CmdEnter(object obj)
        {
            return true;
        }

        private ICommand m_CmdTab;
        public ICommand CmdTab
        {
            get
            {
                if (this.m_CmdTab == null)
                {
                    this.m_CmdTab = new RelayCommand(Excute_CmdTab, CanExcute_CmdTab);
                }
                return this.m_CmdTab;
            }
            set
            {
                this.m_CmdTab = value;
            }
        }

        public void Excute_CmdTab(object obj)
        {
            try
            {
                if (this.MySelectedItem == null)
                    return;

                Int32 selectedIndex = this._tree.SelectedIndex;

                // Child가 있으면 Child로 이동
                // 없으면 NextNode로 포커스 이동
                if (this.MySelectedItem.HasChildren == true)
                {
                    this.MySelectedItem = this.MySelectedItem.Nodes[0];
                }
                else
                {
                    // NextNode도 null이면 Parent의 NextNode로 이동
                    if (this.MySelectedItem.NextNode == null)
                    {
                        // Parent의 NextNode
                        this.MySelectedItem = this.MySelectedItem.Parent.NextNode;
                        if (this.MySelectedItem == null)
                            this._tree.SelectedIndex = selectedIndex;
                    }
                    else
                    {
                        this.MySelectedItem = this.MySelectedItem.NextNode;
                    }
                }

                // 속성 값 DataTemplate에 Focus 주기
                if (this.MySelectedItem != null)
                {
                    if ((this.MySelectedItem.Tag as DevParam) != null)
                    {
                        (this.MySelectedItem.Tag as DevParam).IsFocuse = false;
                        (this.MySelectedItem.Tag as DevParam).IsFocuse = true;
                    }
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        public bool CanExcute_CmdTab(object obj)
        {
            return true;
        }

        private ICommand m_CmdShiftTab;
        public ICommand CmdShiftTab
        {
            get
            {
                if (this.m_CmdShiftTab == null)
                {
                    this.m_CmdShiftTab = new RelayCommand(Excute_CmdShiftTab, CanExcute_CmdShiftTab);
                }
                return this.m_CmdShiftTab;
            }
            set
            {
                this.m_CmdShiftTab = value;
            }
        }

        public void Excute_CmdShiftTab(object obj)
        {
            try
            {
                this._tree.SelectedIndex = this._tree.SelectedIndex - 1;

                if (this._tree.SelectedIndex < 0)
                    this._tree.SelectedIndex = 0;

                // 속성 값 DataTemplate에 Focus 주기
                (this.MySelectedItem.Tag as DevParam).IsFocuse = false;
                (this.MySelectedItem.Tag as DevParam).IsFocuse = true;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        public bool CanExcute_CmdShiftTab(object obj)
        {
            return true;
        }

        //TreeView의 크기가 변경되었을 때 호출
        private void _tree_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            try
            {
                TreeList treeList = sender as TreeList;
                if (treeList.Tag == null)
                    return;

                ContentType type = (ContentType)treeList.Tag;

                if (treeList.Tag == null)
                    type = ContentType.ParameterContent;

                //새로운 넓이 값
                Double Width = e.NewSize.Width;

                switch (type)
                {
                    case ContentType.MeasureContent:
                        this.ParamNameWidth = Width * 0.5;
                        this.ParamValueWidth = Width * 0.5;
                        this.ParamUnitWidth = Width * 0.2;
                        this.ParamUnitWidth = Width * 0.2;
                        this.ParamInitValueWidth = Width * 0.25;
                        this.ParamMaxWidth = Width * 0.2;
                        this.ParamMinWidth = Width * 0.2;
                        this.DescriptionWidth = Width * 0.3;
                        break;
                    default:
                        this.ParamNameWidth = Width * 0.2;
                        this.ParamValueWidth = Width * 0.2;
                        this.ParamUnitWidth = Width * 0.05;
                        this.ParamInitValueWidth = Width * 0.06;
                        this.ParamMaxWidth = Width * 0.1;
                        this.ParamMinWidth = Width * 0.1;
                        this.DescriptionWidth = Width * 0.29;
                        break;
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void TextBox_TextChanged(Object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                DevParam Param = textBox.Tag as DevParam;
                if (!String.IsNullOrWhiteSpace(textBox.Text))
                {
                    String Str = textBox.Text;
                    Int32 value = 0;
                    if (Int32.TryParse(Str, out value))
                    {
                        if (Int32.Parse(Str) < Param.ParamInfo.Min)
                        {
                            textBox.Text = Param.ParamInfo.InitValue;
                            throw new Exception("The Entered Value is less than set minimum value");
                        }
                        else if (Int32.Parse(Str) > Param.ParamInfo.Max)
                        {
                            textBox.Text = Param.ParamInfo.InitValue;
                            throw new Exception("The Entered value is greater than the set maximum value.");
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

        private void TextBlock_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                //Do Something
                TextBlock txt = sender as TextBlock;
                DevParam param = txt.Tag as DevParam;
                param.BlockVisible = Visibility.Collapsed;
                param.BoxVisible = Visibility.Visible;

                StackPanel stack = txt.Parent as StackPanel;
                TextBox txtBox = stack.Children[1] as TextBox;

                this.Dispatcher.BeginInvoke((Action)delegate () 
                {
                    txtBox.Focus();
                    Int32 len = txtBox.Text.Length;
                    txtBox.Select(len, 0);
                });
            }
        }

        private void Stack_LostFocus(Object sender, RoutedEventArgs e)
        {
            StackPanel stack = sender as StackPanel;
            DevParam param = stack.Tag as DevParam;
            param.BlockVisible = Visibility.Visible;
            param.BoxVisible = Visibility.Collapsed;
        }

        private void OnClick(Object sender, MouseButtonEventArgs e)
        {
        }

        private void TreeListItem_Selected(Object sender, RoutedEventArgs e)
        {
            TreeListItem item = sender as TreeListItem;
        }

        //#region ShortUpDown Event
        //private void ShortUpDown_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        //{
        //    try
        //    {
        //        var updown = sender as ShortUpDown;
        //        DevParam Param = updown.Tag as DevParam;

        //        this.m_PreValue = null;
        //        if (updown.Value < Param.ParamInfo.Min)
        //        {
        //            //this.m_PreValue = (Int16)e.OldValue;
        //            this.m_PreValue = Convert.ToInt16(Param.ParamInfo.InitValue);
        //            //Param.ShortValue = Convert.ToInt16(Param.ParamInfo.InitValue);
        //            throw new Exception("The entered value is less than the minimum value.");
        //        }

        //        if (updown.Value > Param.ParamInfo.Max)
        //        {
        //            //this.m_PreValue = (Int16)e.OldValue;
        //            this.m_PreValue = Convert.ToInt16(Param.ParamInfo.InitValue);
        //            //Param.ShortValue = Convert.ToInt16(Param.ParamInfo.InitValue);
        //            throw new Exception("The entered value is greater than the maximum value.");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        if (this.LogEvent != null)
        //            this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, err.StackTrace));
        //    }
        //}

        //private void ShortUpDown_LostFocus(Object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var updown = sender as ShortUpDown;
        //        DevParam Param = updown.Tag as DevParam;

        //        if (this.m_PreValue != null)
        //            (Param as IShortParam).ShortValue = (Int16)this.m_PreValue;
        //    }
        //    catch (Exception err)
        //    {
        //        if (this.LogEvent != null)
        //            this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, err.StackTrace));
        //    }
        //}
        //#endregion

        //#region IntegerUpDown Event
        //private void IntegerUpDown_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        //{
        //    try
        //    {
        //        var updown = sender as IntegerUpDown;
        //        DevParam Param = updown.Tag as DevParam;

        //        this.m_PreValue = null;
        //        if (updown.Value < Param.ParamInfo.Min)
        //        {
        //            //this.m_PreValue = (Int32)e.OldValue;
        //            this.m_PreValue = Convert.ToInt32(Param.ParamInfo.InitValue);
        //            throw new Exception("The entered value is less than the minimum value.");
        //        }

        //        if (updown.Value > Param.ParamInfo.Max)
        //        {
        //            //this.m_PreValue = (Int32)e.OldValue;
        //            this.m_PreValue = Convert.ToInt32(Param.ParamInfo.InitValue);
        //            throw new Exception("The entered value is greater than the maximum value.");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        if (this.LogEvent != null)
        //            this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, err.StackTrace));
        //    }
        //}

        //private void IntegerDown_LostFocus(Object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var updown = sender as IntegerUpDown;
        //        DevParam Param = updown.Tag as DevParam;

        //        if (this.m_PreValue != null)
        //            (Param as IIntParam).IntValue = (Int32)this.m_PreValue;
        //    }
        //    catch (Exception err)
        //    {
        //        if (this.LogEvent != null)
        //            this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, err.StackTrace));
        //    }
        //}
        //#endregion
    }
}
