using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrevisLibrary;

namespace DCT_Graph
{
    public class MeasurementControl_ViewModel : ViewModel
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Wnd"></param>
        public MeasurementControl_ViewModel(Object Wnd) : base(Wnd) { }

        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_NameWidth1 = 0.0d;
            this.m_NameWidth2 = 0.0d;
            this.m_ValueWidth1 = 0.0d;
            this.m_ValueWidth2 = 0.0d;
            this.m_PosWidthCursor1 = 0.0d;
            this.m_PosWidthCursor2 = 0.0d;
            this.m_TimeWidthCursor1 = 0.0d;
            this.m_TimeWidthCursor2 = 0.0d;
            this.m_DataWidthCursor1 = 0.0d;
            this.m_DataWidthCursor2 = 0.0d;

            this.m_SelectChannel = 0;
            this.m_SelectCursorGroup = 0;
            this.m_Scale = 0.0d;
            this.m_Min = 0;
            this.m_Max = 0;
            this.m_PeakToPeak = 0;
            this.m_Mean = 0;
            this.m_FirstTime0 = 0;
            this.m_FirstData0 = 0;
            this.m_FirstTime1 = 0;
            this.m_FirstData1 = 0;
            this.m_FirstTimeDistance = 0;
            this.m_FirstDataDistance = 0;
            this.m_SecondTime0 = 0;
            this.m_SecondData0 = 0;
            this.m_SecondTime1 = 0;
            this.m_SecondData1 = 0;
            this.m_SecondTimeDistance = 0;
            this.m_SecondDataDistance = 0;
            this.m_AxisName = String.Empty;
            this.m_Unit = String.Empty;

            this.BasicInfoList = new ObservableCollection<BasicMeasureInformation>();
            this.CursorList1 = new ObservableCollection<CursorInformation>();
            this.CursorList2 = new ObservableCollection<CursorInformation>();
        }

        ///<summary>
        /// 프로퍼티
        ///</summary>
        //Unit
        private String m_Unit;
        public String Unit
        {
            get { return this.m_Unit; }
            set
            {
                this.m_Unit = value;
                this.NotifyPropertyChanged("Unit");
            }
        }

        //Axis Name
        private String m_AxisName;
        public String AxisName
        {
            get { return this.m_AxisName; }
            set
            {
                this.m_AxisName = value;
                this.NotifyPropertyChanged("AxisName");
            }
        }

        public Double Test { get; set; }

        //채널 선택
        private Int32 m_SelectChannel;
        public Int32 SelectChannel
        {
            get { return this.m_SelectChannel; }
            set
            {
                this.m_SelectChannel = value;
                this.AxisName = (value + 1) + "Channel";
                this.NotifyPropertyChanged("SelectChannel");
            }
        }

        //커서 그룹 선택
        private Int32 m_SelectCursorGroup;
        public Int32 SelectCursorGroup
        {
            get { return this.m_SelectCursorGroup; }
            set
            {
                this.m_SelectCursorGroup = value;
                this.NotifyPropertyChanged("SelectCursorGroup");
            }
        }

        //SelectChannel로 설정된 시그널
        private DigitalSignal m_SelectSignal;
        public DigitalSignal SelectSignal
        {
            get { return this.m_SelectSignal; }
            set
            {
                this.m_SelectSignal = value;
                this.NotifyPropertyChanged("SelectSignal");
            }
        }

        //선택된 채널의 x / div
        private Double m_Scale;
        public Double Scale
        {
            get { return this.m_Scale; }
            set
            {
                this.m_Scale = value;
                this.NotifyPropertyChanged("Scale");
            }
        }

        //시그날 전체의 Min
        private Int32 m_Min;
        public Int32 Min
        {
            get { return this.m_Min; }
            set
            {
                this.m_Min = value;
                this.NotifyPropertyChanged("Min");
            }
        }

        //시그날 전체의 Max
        private Int32 m_Max;
        public Int32 Max
        {
            get { return this.m_Max; }
            set
            {
                this.m_Max = value;
                this.NotifyPropertyChanged("Max");
            }
        }

        //시그날 전체의 Max - Min
        private Int32 m_PeakToPeak;
        public Int32 PeakToPeak
        {
            get { return this.m_PeakToPeak; }
            set
            {
                this.m_PeakToPeak = value;
                this.NotifyPropertyChanged("PeakToPeak");
            }
        }

        //시그날 전체의 평균 값
        private Int32 m_Mean;
        public Int32 Mean
        {
            get { return this.m_Mean; }
            set
            {
                this.m_Mean = value;
                this.NotifyPropertyChanged("Mean");
            }
        }

        //좌측 커서 시간 값
        private Int32 m_FirstTime0;
        public Int32 FirstTime0
        {
            get { return this.m_FirstTime0; }
            set
            {
                this.m_FirstTime0 = value;
                this.NotifyPropertyChanged("FirstTime0");
            }
        }

        //좌측 커서 데이터 값
        private Int32 m_FirstData0;
        public Int32 FirstData0
        {
            get { return this.m_FirstData0; }
            set
            {
                this.m_FirstData0 = value;
                this.NotifyPropertyChanged("FirstData0");
            }
        }

        //우측 커서 시간 값
        private Int32 m_FirstTime1;
        public Int32 FirstTime1
        {
            get { return this.m_FirstTime1; }
            set
            {
                this.m_FirstTime1 = value;
                this.NotifyPropertyChanged("FirstTime1");
            }
        }

        //우측 커서 데이터 값
        private Int32 m_FirstData1;
        public Int32 FirstData1
        {
            get { return this.m_FirstData1; }
            set
            {
                this.m_FirstData1 = value;
                this.NotifyPropertyChanged("FirstData1");
            }
        }

        //시간 간격
        private Int32 m_FirstTimeDistance;
        public Int32 FirstTimeDistance
        {
            get { return this.m_FirstTimeDistance; }
            set
            {
                this.m_FirstTimeDistance = value;
                this.NotifyPropertyChanged("FirstTimeDistance");
            }
        }

        //데이터 차이
        private Int32 m_FirstDataDistance;
        public Int32 FirstDataDistance
        {
            get { return this.m_FirstDataDistance; }
            set
            {
                this.m_FirstDataDistance = value;
                this.NotifyPropertyChanged("FirstDataDistance");
            }
        }

        //좌측 커서 시간 값
        private Int32 m_SecondTime0;
        public Int32 SecondTime0
        {
            get { return this.m_SecondTime0; }
            set
            {
                this.m_SecondTime0 = value;
                this.NotifyPropertyChanged("SecondTime0");
            }
        }

        //좌측 커서 데이터 값
        private Int32 m_SecondData0;
        public Int32 SecondData0
        {
            get { return this.m_SecondData0; }
            set
            {
                this.m_SecondData0 = value;
                this.NotifyPropertyChanged("SecondData0");
            }
        }

        //우측 커서 시간 값
        private Int32 m_SecondTime1;
        public Int32 SecondTime1
        {
            get { return this.m_SecondTime1; }
            set
            {
                this.m_SecondTime1 = value;
                this.NotifyPropertyChanged("SecondTime1");
            }
        }

        //우측 커서 데이터 값
        private Int32 m_SecondData1;
        public Int32 SecondData1
        {
            get { return this.m_SecondData1; }
            set
            {
                this.m_SecondData1 = value;
                this.NotifyPropertyChanged("SecondData1");
            }
        }

        //시간 간격
        private Int32 m_SecondTimeDistance;
        public Int32 SecondTimeDistance
        {
            get { return this.m_SecondTimeDistance; }
            set
            {
                this.m_SecondTimeDistance = value;
                this.NotifyPropertyChanged("SecondTimeDistance");
            }
        }

        //데이터 차이
        private Int32 m_SecondDataDistance;
        public Int32 SecondDataDistance
        {
            get { return this.m_SecondDataDistance; }
            set
            {
                this.m_SecondDataDistance = value;
                this.NotifyPropertyChanged("SecondDataDistance");
            }
        }

        //Min, Max, Mean, PeakToPeak 정보를 담을 리스트
        public ObservableCollection<BasicMeasureInformation> BasicInfoList { get; set; }

        //Cursor1의 정보를 담을 리스트
        public ObservableCollection<CursorInformation> CursorList1 { get; set; }

        //Cursor1의 정보를 담을 리스트
        public ObservableCollection<CursorInformation> CursorList2 { get; set; }

        #region 리스트뷰 Width
        // Name1 Width
        private Double m_NameWidth1;
        public Double NameWidth1
        {
            get { return this.m_NameWidth1; }
            set
            {
                this.m_NameWidth1 = value;
                this.NotifyPropertyChanged("NameWidth1");
            }
        }

        // Value1 Width
        private Double m_ValueWidth1;
        public Double ValueWidth1
        {
            get { return this.m_ValueWidth1; }
            set
            {
                this.m_ValueWidth1 = value;
                this.NotifyPropertyChanged("ValueWidth1");
            }
        }

        // Name2 Width
        private Double m_NameWidth2;
        public Double NameWidth2
        {
            get { return this.m_NameWidth2; }
            set
            {
                this.m_NameWidth2 = value;
                this.NotifyPropertyChanged("NameWidth2");
            }
        }

        // Value2 Width
        private Double m_ValueWidth2;
        public Double ValueWidth2
        {
            get { return this.m_ValueWidth2; }
            set
            {
                this.m_ValueWidth2 = value;
                this.NotifyPropertyChanged("ValueWidth2");
            }
        }

        // Position of Cursor1 Width
        private Double m_PosWidthCursor1;
        public Double PosWidthCursor1
        {
            get { return this.m_PosWidthCursor1; }
            set
            {
                this.m_PosWidthCursor1 = value;
                this.NotifyPropertyChanged("PosWidthCursor1");
            }
        }

        // Time of Cursor1 Width
        private Double m_TimeWidthCursor1;
        public Double TimeWidthCursor1
        {
            get { return this.m_TimeWidthCursor1; }
            set
            {
                this.m_TimeWidthCursor1 = value;
                this.NotifyPropertyChanged("TimeWidthCursor1");
            }
        }

        // Data of Cursor1 Width
        private Double m_DataWidthCursor1;
        public Double DataWidthCursor1
        {
            get { return this.m_DataWidthCursor1; }
            set
            {
                this.m_DataWidthCursor1 = value;
                this.NotifyPropertyChanged("DataWidthCursor1");
            }
        }

        // Position of Cursor2 Width
        private Double m_PosWidthCursor2;
        public Double PosWidthCursor2
        {
            get { return this.m_PosWidthCursor2; }
            set
            {
                this.m_PosWidthCursor2 = value;
                this.NotifyPropertyChanged("PosWidthCursor2");
            }
        }

        // Time of Cursor2 Width
        private Double m_TimeWidthCursor2;
        public Double TimeWidthCursor2
        {
            get { return this.m_TimeWidthCursor2; }
            set
            {
                this.m_TimeWidthCursor2 = value;
                this.NotifyPropertyChanged("TimeWidthCursor2");
            }
        }

        // Data of Cursor2 Width
        private Double m_DataWidthCursor2;
        public Double DataWidthCursor2
        {
            get { return this.m_DataWidthCursor2; }
            set
            {
                this.m_DataWidthCursor2 = value;
                this.NotifyPropertyChanged("DataWidthCursor2");
            }
        }
        #endregion
    }

    /// <summary>
    /// Min, Max, P2P, Mean 값 정보 클래스 정의
    /// </summary>
    public class BasicMeasureInformation : ViewModel
    {
        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_Name1 = String.Empty;
            this.m_Name2 = String.Empty;
            this.m_Value1 = 0;
            this.m_Value2 = 0;
        }

        //Min, Max Name 지정 컬럼
        private String m_Name1;
        public String Name1
        {
            get { return this.m_Name1; }
            set
            {
                this.m_Name1 = value;
                this.NotifyPropertyChanged("Name1");
            }
        }

        //P2P, Mean Name 지정 컬럼
        private String m_Name2;
        public String Name2
        {
            get { return this.m_Name2; }
            set
            {
                this.m_Name2 = value;
                this.NotifyPropertyChanged("Name2");
            }
        }

        //Min, Max Value 지정 컬럼
        private Int32 m_Value1;
        public Int32 Value1
        {
            get { return this.m_Value1; }
            set
            {
                this.m_Value1 = value;
                this.NotifyPropertyChanged("Value1");
            }
        }

        //P2P, Mean Value 지정 컬럼
        private Int32 m_Value2;
        public Int32 Value2
        {
            get { return this.m_Value2; }
            set
            {
                this.m_Value2 = value;
                this.NotifyPropertyChanged("Value2");
            }
        }
    }

    /// <summary>
    /// Cursor 정보 지정
    /// </summary>
    public class CursorInformation : ViewModel
    {
        /// <summary>
        /// 변수 초기화 함수
        /// </summary>
        public override void InitializeViewModel()
        {
            this.m_PosOfCursor = String.Empty;
            this.m_TimeOfCursor = 0;
            this.m_DataOfCursor = 0;
        }

        //Position Of Cursor1
        private String m_PosOfCursor;
        public String PosOfCursor
        {
            get { return this.m_PosOfCursor; }
            set
            {
                this.m_PosOfCursor = value;
                this.NotifyPropertyChanged("PosOfCursor");
            }
        }

        //Time Of Cursor1
        private Int32 m_TimeOfCursor;
        public Int32 TimeOfCursor
        {
            get { return this.m_TimeOfCursor; }
            set
            {
                this.m_TimeOfCursor = value;
                this.NotifyPropertyChanged("TimeOfCursor");
            }
        }

        //Data Of Cursor1
        private Int32 m_DataOfCursor;
        public Int32 DataOfCursor
        {
            get { return this.m_DataOfCursor; }
            set
            {
                this.m_DataOfCursor = value;
                this.NotifyPropertyChanged("DataOfCursor");
            }
        }
    }
}
