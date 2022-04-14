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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrevisLibrary
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MeasureControl : UserControl
    {
        /// <summary>
        /// 에러 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        // 상위 콘텐츠
        public MeasureContent MyContent { get; set; }

        //오실로스코프 콘텐츠
        public OscilloscopeContent OscilloContent { get; set; }

        //MeasureControl추가 카운트
        public Int32 AddMeasureControlCount { get; set; }

        // 파라미터를 표시할 화면
        public ParameterView ParamView { get; set; }

        // 루트 DevParam
        public DevParam RootParam { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public MeasureControl(MeasureContent Content, OscilloscopeContent OscilloContent)
        {
            InitializeComponent();
            this.MyContent = Content;
            this.OscilloContent = OscilloContent;
            this.DataContext = OscilloContent;
        }
    }
}
