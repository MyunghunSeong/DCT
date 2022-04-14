using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using CrevisLibrary;
using System.Reflection;
using System.Windows.Shapes;
using System.Windows.Controls;
using DevExpress.Xpf.Docking;

namespace DCT_Graph
{
    public class ShowAxisInformation : ViewModel, ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Wnd"></param>
        public ShowAxisInformation(Object Wnd) : base(Wnd) { }

        public event EventHandler<LogExecuteEventArgs> LogEvent;

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_ChannelName = String.Empty;
            this.m_IsChannelSelected = true;
            this.m_AxisNameList = new List<String>();
            this.m_CurrentSelectedAxisName = String.Empty;
            this.m_CurrentSelectedAxisIndex = 0;
            this.m_CurrentAxisColor = Brushes.Transparent;
            this.m_Control = null;
            this.m_DataInfoObj = new DataInformation();
            this.m_AxisInfoObj = new AxisInformation();
            this.m_IsEnabledChannelComboBox = true;
            this.m_MyContent = m_Wnd as OscilloscopeContent;
            this.m_IsChecked = true;
            this.m_IsChannelEnabled = true;
            this.m_ParamType = new OscilloscopeParameterType();
        }

        public Object Clone()
        {
            ShowAxisInformation clone = new ShowAxisInformation(PublicVar.MainWnd);
            clone.AxisInfoObj = this.AxisInfoObj;
            clone.AxisNameList = this.AxisNameList;
            clone.Channel = this.Channel;
            clone.ChannelName = this.ChannelName;
            clone.Control = this.Control;
            clone.CurrentAxisColor = this.CurrentAxisColor;
            clone.CurrentSelectedAxisIndex = this.CurrentSelectedAxisIndex;
            clone.CurrentSelectedAxisName = this.CurrentSelectedAxisName;
            clone.DataInfoObj = this.DataInfoObj;
            clone.IsChannelEnabled = this.IsChannelEnabled;
            clone.IsChannelSelected = this.IsChannelSelected;
            clone.IsChecked = this.IsChecked;
            clone.IsEnabledChannelComboBox = this.IsEnabledChannelComboBox;
            clone.MyContent = this.MyContent;
            clone.ParamType = this.ParamType;

            return clone;
        }

        ///<summary>
        /// 프로퍼티
        /// </summary>
        //OscilloscopeParameterType
        private OscilloscopeParameterType m_ParamType;
        public OscilloscopeParameterType ParamType
        {
            get { return this.m_ParamType; }
            set { this.m_ParamType = value; }
        }

        // AxisInfo 객체
        private AxisInformation m_AxisInfoObj;
        public AxisInformation AxisInfoObj
        {
            get { return this.m_AxisInfoObj; }
            set { this.m_AxisInfoObj = value; }
        }

        // 상위 컨텐츠
        private OscilloscopeContent m_MyContent;
        public OscilloscopeContent MyContent
        {
            get { return this.m_MyContent; }
            set { this.m_MyContent = value; }
        }

        //컨트롤
        private YAxisControl m_Control;
        public YAxisControl Control
        {
            get { return this.m_Control; }
            set
            {
                this.m_Control = value;
                this.NotifyPropertyChanged("Control");
            }
        }
    
        //채널 이름
        private String m_ChannelName;
        public String ChannelName
        {
            get { return this.m_ChannelName; }
            set
            {
                this.m_ChannelName = value;
                this.NotifyPropertyChanged("ChannelName");
            }
        }

        //채널
        private Int32 m_Channel;
        public Int32 Channel
        {
            get { return this.m_Channel; }
            set
            {
                this.m_Channel = value;
                this.NotifyPropertyChanged("Channel");
            }
        }

        //DataInformation
        private DataInformation m_DataInfoObj;
        public DataInformation DataInfoObj
        {
            get { return this.m_DataInfoObj; }
            set { this.m_DataInfoObj = value; }
        }

        //채널 선택 콤보박스 Enabled
        private Boolean m_IsEnabledChannelComboBox;
        public Boolean IsEnabledChannelComboBox
        {
            get { return this.m_IsEnabledChannelComboBox; }
            set
            {
                this.m_IsEnabledChannelComboBox = value;
                this.NotifyPropertyChanged("IsEnabledChannelComboBox");
            }
        }

        //채널 선택 Enable
        private Boolean m_IsChannelEnabled;
        public Boolean IsChannelEnabled
        {
            get { return this.m_IsChannelEnabled; }
            set
            {
                this.m_IsChannelEnabled = value;
                this.NotifyPropertyChanged("IsChannelEnabled");
            }
        }

        //채널 선택
        private Boolean m_IsChannelSelected;
        public Boolean IsChannelSelected
        {
            get { return this.m_IsChannelSelected; }
            set
            {
                try
                {
                    Int32 index = Convert.ToInt32(this.m_ChannelName.Substring(0, 1));
                    if (value)
                    {
                        //종료된 패널중에 오실로 스코프가 있는지 확인
                        Boolean HasOscilloContent = false;
                        foreach (LayoutPanel pane in PublicVar.MainWnd.Mananger.ClosedPanels)
                        {
                            if (pane.Caption as String == "Oscilloscope")
                            {
                                HasOscilloContent = true;
                                break;
                            }
                        }

                        //오실로 스코프 패널이 종료된 상태라면 중복 체크를 하지 않는다.
                        if (!HasOscilloContent)
                        {
                            Boolean IsOverlapping = this.CheckOverlappingAxis(this.CurrentSelectedAxisIndex);
                            if (IsOverlapping)
                            {
                                MessageBox.Show("There are overlapping axes. Please choose again.");
                                return;
                            }
                        }

                        if (this.Control != null)
                        {
                            this.IsEnabledChannelComboBox = true;
                            (this.Control.ViewModel).AxisVisible = Visibility.Visible;
                        }

                        switch (index)
                        {
                            case 1:
                                this.MyContent.YAxisScaleWidth1 = new GridLength(60, GridUnitType.Pixel);
                                break;
                            case 2:
                                this.MyContent.YAxisScaleWidth2 = new GridLength(60, GridUnitType.Pixel);
                                break;
                            case 3:
                                this.MyContent.YAxisScaleWidth3 = new GridLength(60, GridUnitType.Pixel);
                                break;
                            case 4:
                                this.MyContent.YAxisScaleWidth4 = new GridLength(60, GridUnitType.Pixel);
                                break;
                            case 5:
                                this.MyContent.YAxisScaleWidth5 = new GridLength(60, GridUnitType.Pixel);
                                break;
                        }

                        this.MyContent.OscilloscopeWidth -= 50;
                        //if (this.MyContent.OscilloscopeControl != null)
                        //    this.MyContent.OscilloscopeControl.DrawGraphDisplay(true);

                        if (this.MyContent.XAxis != null)
                        {
                            this.MyContent.YAxisList[index - 1].Width = 50;
                            this.MyContent.YAxisList[index - 1].Margin = new Thickness(10, 0, 0, 0);
                            this.MyContent.XAxis.m_CanvasArray[index - 1].Visibility = Visibility.Visible;
                            this.MyContent.XAxis.GetAutoScaleValue(true);
                        }
                    }
                    else
                    {
                        if (this.Control != null)
                        {
                            this.IsEnabledChannelComboBox = false;
                            (this.Control.ViewModel).AxisVisible = Visibility.Collapsed;
                        }

                        switch (index)
                        {
                            case 1:
                                this.MyContent.YAxisScaleWidth1 = new GridLength(0, GridUnitType.Pixel);
                                break;
                            case 2:
                                this.MyContent.YAxisScaleWidth2 = new GridLength(0, GridUnitType.Pixel);
                                break;
                            case 3:
                                this.MyContent.YAxisScaleWidth3 = new GridLength(0, GridUnitType.Pixel);
                                break;
                            case 4:
                                this.MyContent.YAxisScaleWidth4 = new GridLength(0, GridUnitType.Pixel);
                                break;
                            case 5:
                                this.MyContent.YAxisScaleWidth5 = new GridLength(0, GridUnitType.Pixel);
                                break;
                        }

                        this.MyContent.OscilloscopeWidth += 50;
                        if (this.MyContent.XAxis != null)
                        {
                            this.MyContent.YAxisList[index - 1].Width = 0;
                            this.MyContent.YAxisList[index - 1].Margin = new Thickness(-10, 0, 0, 0);
                            this.MyContent.XAxis.m_CanvasArray[index - 1].Visibility = Visibility.Collapsed;
                            this.MyContent.XAxis.GetAutoScaleValue(true);
                        }
                    }

                    this.m_IsChannelSelected = value;
                    this.NotifyPropertyChanged("IsChannelSelected");
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }
        }

        //축 이름 리스트
        private List<String> m_AxisNameList;
        public List<String> AxisNameList
        {
            get { return this.m_AxisNameList; }
            set
            {
                this.m_AxisNameList = value;
                this.NotifyPropertyChanged("AxisNameList");
            }
        }

        //현재 내가 선택한 축 이름
        private String m_CurrentSelectedAxisName;
        public String CurrentSelectedAxisName
        {
            get { return this.m_CurrentSelectedAxisName; }
            set
            {
                this.m_CurrentSelectedAxisName = value;
                this.NotifyPropertyChanged("CurrentSelectedAxisName");
            }
        }

        private Boolean m_IsOverlap;
        public Boolean IsOverlap
        {
            get { return this.m_IsOverlap; }
            set
            {
                this.m_IsOverlap = value;
                this.NotifyPropertyChanged("IsOverlap");
            }
        }

        //현재 선택한 축 인덱스
        private Int32 m_CurrentSelectedAxisIndex;
        public Int32 CurrentSelectedAxisIndex
        {
            get { return this.m_CurrentSelectedAxisIndex; }
            set
            {
                try
                {
                    this.IsOverlap = CheckOverlappingAxis(value);
                    if (this.IsOverlap)
                    {
                        MessageBox.Show("There are overlapping axes. Please choose again.");
                        return;
                    }

                    this.m_CurrentSelectedAxisIndex = value;
                    this.CurrentSelectedAxisName = this.AxisNameList[value];
                    this.NotifyPropertyChanged("CurrentSelectedAxisIndex");

                    //현재 축 변경을 한 채널 인덱스를 구한다.
                    Int32 index = -1;
                    for (int i = 0; i < this.MyContent.BrushColorArray.Length; i++)
                    {
                        if (this.CurrentAxisColor.Equals(this.MyContent.BrushColorArray[i]))
                        {
                            index = i;
                            break;
                        }
                    }

                    //에러 처리
                    if (index < 0)
                        return;

                    //변경전 축 정보를 가져온다.
                    ShowAxisInformation info = this.MyContent.AxisInfoList[index];
                    //Visible관련 설정해주고 ===> None으로 선택했을 때는 보이지 않게 설정
                    //info.IsChannelSelected = (value == 0) ? false : true;

                    //OscilloscopeParameterType을 구한 후 정보를 가져와서 재 설정해준다.
                    String EnumName = this.CurrentSelectedAxisName.Replace(" ", "_");
                    OscilloscopeParameterType type = new OscilloscopeParameterType();
                    Enum.TryParse(EnumName, out type);
                    info.m_DataInfoObj = this.MyContent.DigitalSignalMap[type].DataInformObj;
                    info.m_IsEnabledChannelComboBox = true;
                    info.ParamType = type;

                    //변경한 축의 인덱스 정보를 구한다.
                    Int32 count = 0;
                    foreach (String AxisName in AxisNameList)
                    {
                        if (AxisName.Equals(this.m_CurrentSelectedAxisName))
                            break;

                        count++;
                    }

                    //새롭게 정보를 설정해준다.
                    info.m_CurrentSelectedAxisIndex = count;
                    String CurrentAxisName = AxisNameList[count];
                    info.CurrentAxisColor = this.MyContent.BrushColorArray[index];
                    info.m_CurrentSelectedAxisName = CurrentAxisName;

                    //Min, Max값 및 정보를 재 설정해준다.
                    (this.MyContent.YAxisList[index].ViewModel).AxisInfoObj.AxisName =
                        info.DataInfoObj.Name;
                    (this.MyContent.YAxisList[index].ViewModel).AxisInfoObj.MinValue =
                        info.DataInfoObj.DataMin;
                    (this.MyContent.YAxisList[index].ViewModel).AxisInfoObj.MaxValue =
                        info.DataInfoObj.DataMax;
                    (this.MyContent.YAxisList[index].ViewModel).AxisVisible = Visibility.Visible;
                    this.MyContent.YAxisList[index].Height = (this.MyContent.YAxisList[index].ViewModel).ControlHeight;
                    (this.MyContent.YAxisList[index]).SetAnotherAxisValueRange(info.DataInfoObj.DataMin, info.DataInfoObj.DataMax);

                    //축 이름을 None으로 설정한 경우 Visible을 보이지 않게 설정하고 리턴
                    if (value == 0 || !this.IsChannelSelected)
                    {
                        this.IsChannelSelected = false;
                        //(this.MyContent.YAxisList[index].ViewModel).AxisVisible = Visibility.Collapsed;
                        return;
                    }

                    //축을 다시 그린다.
                    Signal[] CopySignalArr = new Signal[this.MyContent.DigitalSignalMap[type].SignalData.Count];
                    this.MyContent.DigitalSignalMap[type].SignalData.CopyTo(CopySignalArr);

                    //CopySignalArr.CopyTo(this.MyContent.YAxisList[index].DigitalSignalObj.SignalData.ToArray(), 0);
                    this.MyContent.YAxisList[index].DigitalSignalObj.SignalData = new List<Signal>(CopySignalArr);
                    this.MyContent.YAxisList[index].GetAutoScaleValue();
                    this.MyContent.YAxisList[index].DrawAxisLine();

                    this.MyContent.DigitalSignalMap[type].YAxisInfo = (this.MyContent.YAxisList[index].ViewModel).AxisInfoObj;
                    //this.MyContent.YAxisList[index].DigitalSignalObj.YAxisInfo = this.MyContent.DigitalSignalMap[type].YAxisInfo;
                    this.MyContent.DigitalSignalMap[type].XAxisInfo = (this.MyContent.XAxis.ViewModel).AxisInfoObj;
                    //this.MyContent.YAxisList[index].DigitalSignalObj.XAxisInfo = this.MyContent.DigitalSignalMap[type].XAxisInfo;
                }
                catch (Exception err)
                {
                    if (this.LogEvent != null)
                        this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
                }
            }
        }

        //현재 선택한 축 색상
        private Brush m_CurrentAxisColor;
        public Brush CurrentAxisColor
        {
            get { return this.m_CurrentAxisColor; }
            set
            {
                this.m_CurrentAxisColor = value;
                this.NotifyPropertyChanged("CurrentAxisColor");
            }
        }


        //MeasurementControl에서 추가 되어있는지 체크 여부
        private Boolean m_IsChecked;
        public Boolean IsChecked
        {
            get { return this.m_IsChecked; }
            set
            {
                this.m_IsChecked = value;
                this.NotifyPropertyChanged("IsChecked");
            }
        }

        //SMH3333 TEST
        private Boolean CheckOverlappingAxis(Int32 value)
        {
            try
            {
                Boolean result = false;
                //중복 선택한 축의 정보를 저장하는 List 형식의 변수
                List<Int32> StoreOverlappingInfoList = new List<Int32>();

                for (int j = 0; j < this.MyContent.AxisInfoList.Count; j++)
                {
                    //현재 선택된 축의 인덱스 정보를 저장
                    //Int32 PreIndex = this.MyContent.AxisInfoList[j].CurrentSelectedAxisIndex;

                    //for (int i = 0; i < this.MyContent.AxisInfoList.Count; i++)
                    //{
                    //    //같은 채널인 경우는 패스
                    //    if (i == j)
                    //        continue;

                    if (this.MyContent.AxisInfoList[j].CurrentSelectedAxisIndex == 0
                        || value == 0)
                        continue;

                    //현재 채널과 다른 채널인 경우 선택된 축의 인덱스 정보가 같은지 확인해서 
                    //같은 인덱스인 경우(같은 축인 경우) 해당 채널의 정보를 저장
                    if (this.MyContent.AxisInfoList[j].IsChannelSelected)
                    {
                        if (this.MyContent.AxisInfoList[j].CurrentSelectedAxisIndex.Equals(value))
                            StoreOverlappingInfoList.Add(j);
                    }
                    //}
                }

                //중복된 축이 하나라도 있는 경우에는 true 하나도 없는 경우에는 false를 저장
                result = (StoreOverlappingInfoList.Count > 0) ? true : false;

                /*
                Int32 RandomIndex = 0;
                List<Int32> RandomIndexList = new List<Int32>();
                //중복된 축 정보만큼 반복해서 실행
                foreach (Int32 key in StoreOverlappingInfoList)
                {
                    //현재 축 인덱스 정보를 저장
                    Int32 BackupCurrentIndex = this.MyContent.AxisInfoList[key].CurrentSelectedAxisIndex;
                    List<Int32> tmpList = new List<Int32>();
                    for (int i = 0; i < this.MyContent.AxisInfoList.Count; i++)
                    {
                        //현재 채널과 같은 경우는 실행하지 않는다.
                        if (key.Equals(i))
                            continue;

                        //현재 채널과 다른 경우에 해당 채널의 축 인덱스 정보를 임시 리스트에 저장
                        tmpList.Add(this.MyContent.AxisInfoList[i].CurrentSelectedAxisIndex);
                    }

                    while (true)
                    {
                        //현재 축 개수 중에 랜덤으로 하나의 인덱스를 추출
                        Random rand = new Random();
                        Int32 tmp = rand.Next(0, 21);

                        //추출한 인덱스가 중복된 축과 같은 인덱스라면 Continue
                        if (tmpList.Contains(tmp))
                            continue;
                        //추출한 인덱스가 중복된 축과 다른 축이라면 해당 인덱스를 저장
                        else
                        {
                            //if (RandomIndexList.Contains(tmp))
                            //{
                            //    rand = new Random();
                            //    tmp = rand.Next(0, 21);
                            //}

                            RandomIndex = tmp;
                            RandomIndexList.Add(RandomIndex);
                            break;
                        }
                    }

                    //this.MyContent.AxisInfoList[key].CurrentSelectedAxisIndex = RandomIndex;
                    //this.MyContent.AxisInfoList[key].CurrentSelectedAxisIndex = BackupCurrentIndex;
                }
                */

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
