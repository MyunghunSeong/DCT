using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using Aga.Controls.Tree;
using CrevisLibrary;
using MotionGuidePro.Main;

namespace CrevisLibrary
{
    /// <summary>
    /// SelectionParameter.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectionParameter : Window, INotifyPropertyChanged
    {
        //파라미터 이름 넓이
        private Double m_NameWidth;
        public Double NameWidth
        {
            get { return this.m_NameWidth; }
            set
            {
                this.m_NameWidth = value;
                this.NotifyPropertyChanged("NameWidth");
            }
        }

        //파라미터 설명 값 넓이
        private Double m_DescWidth;
        public Double DescWidth
        {
            get { return this.m_DescWidth; }
            set
            {
                this.m_DescWidth = value;
                this.NotifyPropertyChanged("DescWidth");
            }
        }

        //선택한 파라미터
        public DevParam SettingParam { get; set; }

        //파라미터 루트
        public DevParam Root { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public SelectionParameter()
        {
            InitializeComponent();
            this.m_NameWidth = 250.0d;
            this.m_DescWidth = 250.0d;
            this.DataContext = this;
            this.Root = null;
            this.SettingParam = null;
        }

        /// <summary>
        /// 창이 로드 되었을 때 호출
        /// </summary>
        private void SelectionView_OnRendered(Object sender, EventArgs e)
        {
            try
            {
                //ViewModel설정
                MainWindow_ViewModel viewModel = PublicVar.MainWnd.ViewModel;

                //보여줄 정보를 트리형식으로 만든다.
                DevParam Param = MakeTreeItems(viewModel.CurrentDevice.MyModel);
                ParameterModel model = new ParameterModel(Param, false);
                _tree.Model = model;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// 파라미터 정보를 트리형식으로 만드는 함수
        /// </summary>
        /// <param name="Param">파라미터 정보</param>
        public DevParam MakeTreeItems(IParam Param)
        {
            try
            {
                MainWindow_ViewModel viewModel = PublicVar.MainWnd.ViewModel;
                this.Root = new DevParam("최상위 노드", ParamType.Category, null, String.Empty);

                //컨텐츠 목록들의 파라미터
                IParam ContentListParamList = Param.GetParamNode("컨텐츠 목록");
                foreach (IParam contentParam in ContentListParamList.Children)
                {
                    //UserManualContent타입은 제외
                    if ((contentParam.Value as String).Equals("UserManualContent"))
                        continue;

                    //보여줄 파라미터를 만든다.
                    DevParam ContentNameParam = new DevParam(contentParam.ParamInfo.ParamName, ParamType.Category, null, String.Empty);

                    //기존 모델에 있는 콘텐츠들의 파라미터 정보를 가져온다.
                    IParam ContentParam = contentParam.GetParamNode(contentParam.ParamInfo.ParamName).GetParamNode("파라미터 정보");
                    Int32 count = 0;
                    foreach (IParam subParam in ContentParam.Children)
                    {
                        //기존에 넘어온 파라미터는 체크 표시
                        //subParam : 모델의 파라미터, SettingParam : UserManualParam
                        Boolean result = CheckIncludeParam(subParam, SettingParam);
                        DevParam ParamClone = subParam.Clone() as DevParam;
                        ParamClone.IsChecked = result;

                        //파라미터 변경 이벤트 등록
                        subParam.DependencyParamUpdate += viewModel.ParamUpdate;
                        subParam.DependencyParams.Add(ParamClone);
                        ParamClone.DependencyParamUpdate += viewModel.ParamUpdate;
                        ParamClone.m_DependencyParams.Add(subParam);
                        ContentNameParam.Children.Add(ParamClone);

                        count++;
                    }
                    this.Root.Children.Add(ContentNameParam);
                }

                return this.Root;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Boolean CheckIncludeParam(IParam Target, IParam Standard)
        {
            try
            {
                //Target : 모델의 파라미터, Standard : UserManualParam
                Boolean result = false;
                foreach (IParam CategoryParam in Standard.Children)
                {
                    foreach (IParam Param in CategoryParam.Children)
                    {
                        if (Target.ParamInfo.ParamName.Equals(Param.ParamInfo.ParamName)
                            && Target.Parent.Parent.ParamInfo.ParamName.Equals(CategoryParam.ParamInfo.ParamName))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

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
        /// Tree 확장
        /// </summary>
        /// <param name="Nodes">TreeView 하위 노드들</param>
        public void ExpandAll(ReadOnlyCollection<TreeNode> Nodes, Boolean IsOpen=true)
        {
            try
            {
                foreach (TreeNode Node in Nodes)
                {
                    Node.IsExpanded = IsOpen;
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
        /// 해당 파라미터를 클릭했을 때 호출
        /// </summary>
        private void OnClick(Object sender, MouseButtonEventArgs e)
        {
            this.MySelectedItem = (sender as TreeListItem).Node;
        }

        /// <summary>
        /// 트리의 사이즈가 변경됐을 때 호출
        /// </summary>
        private void _tree_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            try
            {
                //새로운 넓이 값
                Double Width = e.NewSize.Width;
                this.NameWidth = Width * 0.5;
                this.DescWidth = Width * 0.5;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// 확인 버튼을 눌렀을 때 호출
        /// </summary>
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow_ViewModel viewModel = PublicVar.MainWnd.ViewModel;
                //선택된 파라미터의 최상위단
                SettingParam = new DevParam("파라미터 정보", ParamType.Category, null, String.Empty);
                DevParam CategoryParam = null;
                //현재 보여지는 트리의 노드
                foreach (TreeNode node in _tree.Nodes)
                {
                    DevParam param = node.Tag as DevParam;
                    //카테고리가 될 파라미터를 만들고
                    CategoryParam = new DevParam(param.ParamInfo.ParamName, ParamType.Category, null, String.Empty);
                    foreach (DevParam subParam in param.Children)
                    {
                        //파라미터가 체크되어있다면 리스트에 넣는다.
                        if (subParam.IsChecked)
                            CategoryParam.Children.Add(subParam);
                    }

                    if (CategoryParam.Children.Count > 0)
                        SettingParam.Children.Add(CategoryParam);
                }

                this.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// 취소 버튼을 눌렀을 때 호출
        /// </summary>
        private void CancelBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                this.Root = null;
                this.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
