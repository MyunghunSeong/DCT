using System;
using System.Collections.Generic;
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

namespace CrevisLibrary
{
    /// <summary>
    /// SelectionMeasureChannel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectionMeasureChannel : Window
    {
        /// <summary>
        /// 이벤트 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        /// <summary>
        /// 상위 컨텐츠
        /// </summary>
        public OscilloscopeContent MyContent { get; set; }

        ///<summary>
        /// OK, Cancel 버튼 확인
        /// </summary>
        public Boolean IsOK { get; set; }

        private Point m_startPoint;
        private Int32 m_startIndex;

        /// <summary>
        /// ViewModel 객체
        /// </summary>
        public SelectionMeasureChannel_ViewModel ViewModel { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public SelectionMeasureChannel(OscilloscopeContent Content)
        {
            InitializeComponent();
            this.MyContent = Content;
            this.IsOK = false;

            //ViewModel 설정
            this.ViewModel = new SelectionMeasureChannel_ViewModel(this);
            this.ViewModel.InitializeViewModel();
            this.DataContext = this.ViewModel;
        }

        public static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
