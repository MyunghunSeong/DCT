using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Shapes;
using DCT_Graph;

namespace CrevisLibrary
{
    /// <summary>
    /// 캔버스 드래그 동작
    /// </summary>
    public class CanvasDragBehavior : Behavior<FrameworkElement>
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Dependency Property
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 기본 엘리먼트 속성 - BaseElementProperty

        /// <summary>
        /// 기본 엘리먼트 속성
        /// </summary>
        public static readonly DependencyProperty BaseElementProperty = DependencyProperty.Register
        (
            "BaseElement",
            typeof(FrameworkElement),
            typeof(CanvasDragBehavior),
            new PropertyMetadata(null)
        );

        #endregion
        #region 타겟 엘리먼트 속성 - TargetElementProperty

        /// <summary>
        /// 타겟 엘리먼트 속성
        /// </summary>
        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register
        (
            "TargetElement",
            typeof(FrameworkElement),
            typeof(CanvasDragBehavior),
            new PropertyMetadata(null)
        );

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 상위 단 TriggerLineControl
        /// </summary>
        TriggerLineControl m_ParentLineControl = null;

        /// <summary>
        /// 드래그 여부
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// 마우스 시작 위치
        /// </summary>
        private Point mouseStartPoint;

        /// <summary>
        /// 항목 시작 위치
        /// </summary>
        private Point itemStartPoint;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 기본 엘리먼트 - BaseElement

        /// <summary>
        /// 기본 엘리먼트
        /// </summary>
        public FrameworkElement BaseElement
        {
            get
            {
                return (FrameworkElement)GetValue(BaseElementProperty);
            }
            set
            {
                SetValue(BaseElementProperty, value);
            }
        }

        #endregion
        #region 타겟 엘리먼트 - TargetElement

        /// <summary>
        /// 타겟 엘리먼트
        /// </summary>
        public FrameworkElement TargetElement
        {
            get
            {
                return (FrameworkElement)GetValue(TargetElementProperty);
            }
            set
            {
                SetValue(TargetElementProperty, value);
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Protected

        #region 부착시 처리하기 - OnAttached()

        /// <summary>
        /// 부착시 처리하기
        /// </summary>
        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;

            base.OnAttached();
        }

        #endregion
        #region 탈착시 처리하기 - OnDetaching()

        /// <summary>
        /// 탈착시 처리하기
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;

            base.OnDetaching();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region 결합 객체 마우스 왼쪽 버튼 DOWN 처리하기 - AssociatedObject_MouseLeftButtonDown(sender, e)

        /// <summary>
        /// 결합 객체 마우스 왼쪽 버튼 DOWN 처리하기
        /// </summary>
        /// <param name="sender">이벤트 발생자</param>
        /// <param name="e">이벤트 인자</param>
        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.isDragging == false)
            {
                FrameworkElement baseElement = (BaseElement ?? AssociatedObject);

                baseElement.MouseMove         += baseElement_MouseMove;
                baseElement.MouseLeftButtonUp += baseElement_MouseLeftButtonUp;

                FrameworkElement targetElement = (TargetElement ?? AssociatedObject);


                if (targetElement.DataContext is Grid)
                {
                    if ((targetElement.DataContext as Grid).Parent != null)
                    {
                        if (((targetElement.DataContext as Grid).Parent as DraggableCanvas).Parent is TriggerLineControl)
                            this.m_ParentLineControl = ((targetElement.DataContext as Grid).Parent as DraggableCanvas).Parent as TriggerLineControl;
                    }
                }

                this.isDragging = true;

                this.mouseStartPoint = e.GetPosition(baseElement);

                double x = Canvas.GetLeft(targetElement);
                double y = Canvas.GetTop (targetElement);

                x = double.IsNaN(x) ? 0 : x;
                y = double.IsNaN(y) ? 0 : y;

                this.itemStartPoint = new Point(x, y);

                baseElement.CaptureMouse();
            }
        }

        #endregion
        #region 기본 엘리먼트 마우스 이동시 처리하기 - captureElement_MouseMove(sender, e)

        /// <summary>
        /// 기본 엘리먼트 마우스 이동시 처리하기
        /// </summary>
        /// <param name="sender">이벤트 발생자</param>
        /// <param name="e">이벤트 인자</param>
        private void baseElement_MouseMove(object sender, MouseEventArgs e)
        {
            //SMH5555
            if(e.LeftButton == MouseButtonState.Pressed && this.isDragging == true)
            {
                FrameworkElement baseElement   = (this.BaseElement ?? this.AssociatedObject);
                FrameworkElement targetElement = (this.TargetElement ?? this.AssociatedObject);

                Vector deltaVector = e.GetPosition(baseElement) - this.mouseStartPoint;
                Canvas.SetTop(targetElement, this.itemStartPoint.Y + deltaVector.Y);
                if (this.m_ParentLineControl != null)
                {
                    this.m_ParentLineControl.SaveCursorPosition(deltaVector.Y);
                }
            }
        }

        #endregion
        #region 기본 엘리먼트 마우스 왼쪽 버튼 UP 처리하기 - baseElement_MouseLeftButtonUp(sender, e)

        /// <summary>
        /// 기본 엘리먼트 마우스 왼쪽 버튼 UP 처리하기
        /// </summary>
        /// <param name="sender">이벤트 발생자</param>
        /// <param name="e">이벤트 인자</param>
        private void baseElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isDragging = false;

            FrameworkElement baseElement = (BaseElement ?? AssociatedObject);

            //if (this.m_ParentCursorControl != null)
                //this.m_ParentCursorControl.UpdateCursorPosX();

            baseElement.ReleaseMouseCapture();

            baseElement.MouseMove         -= baseElement_MouseMove;
            baseElement.MouseLeftButtonUp -= baseElement_MouseLeftButtonUp;
        }
        #endregion
    }
}