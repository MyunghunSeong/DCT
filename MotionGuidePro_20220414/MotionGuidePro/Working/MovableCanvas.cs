using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrevisLibrary
{
    /// <summary>
    /// 이동 가능한 캔버스
    /// </summary>
    public class MovableCanvas : Canvas
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Dependency Property
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 이동 비율 속성 - MovingRatioProperty

        /// <summary>
        /// 이동 비율 속성
        /// </summary>
        public static readonly DependencyProperty MovingRatioProperty = DependencyProperty.Register
        (
            "MovingRatio",
            typeof(double),
            typeof(MovableCanvas),
            new PropertyMetadata(1.0)
        );

        #endregion
        #region 이동시 커서 속성 - CursorOnMoveProperty

        /// <summary>
        /// 이동시 커서 속성
        /// </summary>
        public static readonly DependencyProperty CursorOnMoveProperty = DependencyProperty.Register
        (
            "CursorOnMove",
            typeof(Cursor),
            typeof(MovableCanvas),
            new PropertyMetadata(Cursors.ScrollAll)
        );

        #endregion
        #region 오프셋 X 속성 - OffsetXProperty

        /// <summary>
        /// 오프셋 X 속성
        /// </summary>
        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register
        (
            "OffsetX",
            typeof(double),
            typeof(MovableCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        #endregion
        #region 오프셋 Y 속성 - OffsetYProperty

        /// <summary>
        /// 오프셋 Y 속성
        /// </summary>
        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register
        (
            "OffsetY",
            typeof(double),
            typeof(MovableCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 드래그 여부
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// 이전 커서
        /// </summary>
        private Cursor previousCursor;

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

        #region 이동 비율 - MovingRatio

        /// <summary>
        /// 이동 비율
        /// </summary>
        public double MovingRatio
        {
            get
            {
                return (double)GetValue(MovingRatioProperty);
            }
            set
            {
                SetValue(MovingRatioProperty, value);
            }
        }

        #endregion
        #region 이동시 커서 - CursorOnMove

        /// <summary>
        /// 이동시 커서
        /// </summary>
        public Cursor CursorOnMove
        {
            get
            {
                return (Cursor)GetValue(CursorOnMoveProperty);
            }
            set
            {
                SetValue(CursorOnMoveProperty, value);
            }
        }

        #endregion
        #region 오프셋 X - OffsetX

        /// <summary>
        /// 오프셋 X
        /// </summary>
        public double OffsetX
        {
            get
            {
                return (double)GetValue(OffsetXProperty);
            }
            set
            {
                SetValue(OffsetXProperty, value);
            }
        }

        #endregion
        #region 오프셋 Y - OffsetY

        /// <summary>
        /// 오프셋 Y
        /// </summary>
        public double OffsetY
        {
            get
            {
                return (double)GetValue(OffsetYProperty);
            }
            set
            {
                SetValue(OffsetYProperty, value);
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Protected

        #region 마우스 DOWN 처리하기 - OnMouseDown(e)

        /// <summary>
        /// 마우스 DOWN 처리하기
        /// </summary>
        /// <param name="e">이벤트 인자</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if(this.isDragging == false && e.MiddleButton == MouseButtonState.Pressed)
            {
                this.isDragging = true;

                this.previousCursor = Cursor;

                Cursor = CursorOnMove;

                this.mouseStartPoint = e.GetPosition(this);

                this.itemStartPoint = new Point(this.OffsetX, this.OffsetY);

                CaptureMouse();
            }

            base.OnMouseDown(e);
        }

        #endregion
        #region 마우스 이동시 처리하기 - OnMouseMove(e)

        /// <summary>
        /// 마우스 이동시 처리하기
        /// </summary>
        /// <param name="e">이벤트 인자</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed && this.isDragging == true)
            {
                Vector deltaVector = e.GetPosition(this) - this.mouseStartPoint;

                OffsetX = this.itemStartPoint.X + deltaVector.X * MovingRatio;
                OffsetY = this.itemStartPoint.Y + deltaVector.Y * MovingRatio;
            }

            base.OnMouseMove(e);
        }

        #endregion
        #region 마우스 UP 처리하기 - OnMouseUp(e)

        /// <summary>
        /// 마우스 UP 처리하기
        /// </summary>
        /// <param name="e">이벤트 인자</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.isDragging = false;

            Cursor = this.previousCursor;

            ReleaseMouseCapture();

            base.OnMouseUp(e);
        }

        #endregion

        #region 배열하기 (오버라이드) - ArrangeOverride(arrangeSize)

        /// <summary>
        /// 배열하기 (오버라이드)
        /// </summary>
        /// <param name="arrangeSize">배열 크기</param>
        /// <returns>크기</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach(UIElement element in InternalChildren)
            {
                if(element != null)
                {
                    double x = 0.0;
                    double y = 0.0;

                    double left = Canvas.GetLeft(element);

                    if(!double.IsNaN(left))
                    {
                        x = left * MovingRatio;
                    }
                    else
                    {
                        double right = Canvas.GetRight(element) * MovingRatio;

                        if(!double.IsNaN(right))
                        {
                            x = arrangeSize.Width - element.DesiredSize.Width - right;
                        }
                    }

                    double top = Canvas.GetTop(element);

                    if(!double.IsNaN(top))
                    {
                        y = top * MovingRatio;
                    }
                    else
                    {
                        double bottom = Canvas.GetBottom(element) * MovingRatio;

                        if(!double.IsNaN(bottom))
                        {
                            y = arrangeSize.Height - element.DesiredSize.Height - bottom;
                        }
                    }

                    element.Arrange
                    (
                        new Rect
                        (
                            new Point((x + OffsetX) / MovingRatio, (y + OffsetY) / MovingRatio),
                            element.DesiredSize
                        )
                    );
                }
            }

            return arrangeSize;
        }

        #endregion
    }
}