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
using System.Windows.Shapes;
using MotionGuidePro.Main;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CrevisLibrary
{
    /// <summary>
    /// SettingDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Setting : Window
    {
        /// <summary>
        /// 로그 이벤트 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        // 파일 이름 백업
        private String m_BackUpFileName;

        // 파일 경로 백업
        private String m_BackUpFilePath;

        //확인 / 취소 버튼 클릭 여부
        public Boolean IsOK { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public Setting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 창이 로드되었을 때 호출
        /// </summary>
        private void SettingDiaglog_OnLoaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                //DataContext를 MainWindow의 DataContext로 설정
                this.DataContext = (this.Owner as MainWindow).DataContext;

                //파일 이름과 경로를 백업한다.
                this.m_BackUpFileName = ((this.Owner as MainWindow).DataContext as MainWindow_ViewModel).BaseFileName;
                this.m_BackUpFilePath = ((this.Owner as MainWindow).DataContext as MainWindow_ViewModel).BaseFilePath;
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 확인 버튼을 눌렀을 경우 호출
        /// </summary>
        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow_ViewModel viewModel = (this.DataContext) as MainWindow_ViewModel;

                //프로퍼티 값 업데이트
                BindingExpression binding = LoadFileText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = FilePathText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = ServerAddressText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = ClientAddressText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = TimeoutText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                binding = ErrorStateTimeText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                //모니터일 업데이트 시간 범위 지정 처리
                Double MonitoringTimeValue = Convert.ToDouble(MonitoringTimeText.Text);
                if (MonitoringTimeValue >= PublicVar.MainWnd.ViewModel.MonitoringMinValue)
                {
                    binding = MonitoringTimeText.GetBindingExpression(TextBox.TextProperty);
                    binding.UpdateSource();
                }
                else
                {
                    MonitoringTimeText.Text = PublicVar.MainWnd.ViewModel.MonitoringMinValue.ToString();
                    binding = MonitoringTimeText.GetBindingExpression(TextBox.TextProperty);
                    binding.UpdateSource();
                }


                binding = DataReceiveTimeText.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();

                //파일 경로를 변경
                XmlParser.SetFileConfiguration(viewModel.BaseFilePath + "\\" + viewModel.BaseFileName, viewModel.BaseFilePath);

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
        /// 취소 버튼을 눌렀을 경우 호출
        /// </summary>
        private void CancelBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow_ViewModel viewModel = this.DataContext as MainWindow_ViewModel;

                //설정하기전 파일이름과 경로로 다시 설정해준다.
                viewModel.BaseFileName = this.m_BackUpFileName;
                viewModel.BaseFilePath = this.m_BackUpFilePath;

                this.IsOK = false;
                this.Close();
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        
        /// <summary>
        /// 로드 파일 버튼을 눌렀을 때 호출
        /// </summary>
        private void LoadFileBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow_ViewModel viewModel = this.DataContext as MainWindow_ViewModel;

                //파일 선택하기
                OpenFileDialog OpenDlg = new OpenFileDialog();
                OpenDlg.InitialDirectory = viewModel.BaseFilePath;
                OpenDlg.Filter = "XML-File | *.xml";
                OpenDlg.DefaultExt = ".xml"; // Default file extension
                if (OpenDlg.ShowDialog() == true)
                    viewModel.FilePath = OpenDlg.FileName;

                //사용자가 설정한 파일 경로의 파일 하위를 확인
                DirectoryInfo di = new DirectoryInfo(OpenDlg.InitialDirectory);
                foreach (FileInfo fi in di.GetFiles())
                {
                    //파일 이름이 현재 설정한 파일 이름과 동일한 경우
                    if (fi.FullName.Equals(OpenDlg.FileName))
                    {
                        //기본 파일 이름으로 설정
                        viewModel.BaseFileName = fi.Name;
                        break;
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
        /// 파일 경로 버튼을 눌렀을 때 호출
        /// </summary>
        private void BaseFilePathBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow_ViewModel viewModel = this.DataContext as MainWindow_ViewModel;

                //파일 경로를 선택하는 창을 호출
                CommonOpenFileDialog dlg = new CommonOpenFileDialog();
                dlg.InitialDirectory = viewModel.BaseFilePath;
                dlg.IsFolderPicker = true;

                //확인 버튼을 눌렀을 경우
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    //사용자가 선택한 파일 경로를 기본 파일 경로로 설정
                    viewModel.BaseFilePath = dlg.FileName;
                    if (!viewModel.BaseFilePath.Equals(this.m_BackUpFilePath))
                        viewModel.BaseFileName = String.Empty;
                }

            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        /// <summary>
        /// 키보드가 눌려졌을 때 호출
        /// </summary>
        private void SettingDiaglog_OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                //Enter키가 눌려졌을 때만 실행
                if (e.Key == Key.Enter)
                    OKBtn_OnClick(sender, e);
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        private void TimeoutText_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }
    }
}
