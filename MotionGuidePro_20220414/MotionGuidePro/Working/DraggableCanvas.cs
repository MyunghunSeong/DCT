using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xaml;
using DCT_Graph;

namespace CrevisLibrary
{
    /// <summary>
    /// 드래그 가능한 캔버스
    /// </summary>
    [TemplatePart(Name = "PART_RootCanvas", Type = typeof(MovableCanvas))]
    public class DraggableCanvas : TreeView
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Dependency Property
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 확대/축소 비율 속성 - ZoomRateProperty

        /// <summary>
        /// 확대/축소 비율 속성
        /// </summary>
        public static readonly DependencyProperty ZoomRateProperty = DependencyProperty.Register
        (
            "ZoomRate",
            typeof(double),
            typeof(DraggableCanvas),
            new PropertyMetadata(1.0)
        );

        #endregion
        #region 확대/축소 간격 속성 - ZoomIntervalProperty

        /// <summary>
        /// 확대/축소 간격 속성
        /// </summary>
        public static readonly DependencyProperty ZoomIntervalProperty = DependencyProperty.Register
        (
            "ZoomInterval",
            typeof(double),
            typeof(DraggableCanvas),
            new PropertyMetadata(0.02)
        );

        #endregion
        #region 최소 확대/축소 비율 속성 - MinimumZoomRateProperty

        /// <summary>
        /// 최소 확대/축소 비율 속성
        /// </summary>
        public static readonly DependencyProperty MinimumZoomRateProperty = DependencyProperty.Register
        (
            "MinimumZoomRate",
            typeof(double),
            typeof(DraggableCanvas),
            new PropertyMetadata(0.5)
        );

        #endregion
        #region 최대 확대/축소 비율 속성 - MaximumZoomRateProperty

        /// <summary>
        /// 최대 확대/축소 비율 속성
        /// </summary>
        public static readonly DependencyProperty MaximumZoomRateProperty = DependencyProperty.Register
        (
            "MaximumZoomRate",
            typeof(double),
            typeof(DraggableCanvas),
            new PropertyMetadata(3.0)
        );

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 이동 가능한 캔버스
        /// </summary>
        private MovableCanvas movableCanvas = null;
        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 확대/축소 비율 - ZoomRate

        /// <summary>
        /// 확대/축소 비율
        /// </summary>
        public double ZoomRate
        {
            get
            {
                return (double)GetValue(ZoomRateProperty);
            }
            set
            {
                SetValue(ZoomRateProperty, value);
            }
        }

        #endregion
        #region 확대/축소 간격 - ZoomInterval

        /// <summary>
        /// 확대/축소 간격
        /// </summary>
        public double ZoomInterval
        {
            get
            {
                return (double)GetValue(ZoomIntervalProperty);
            }
            set
            {
                SetValue(ZoomIntervalProperty, value);
            }
        }

        #endregion
        #region 최소 확대/축소 비율 - MinimumZoomRate

        /// <summary>
        /// 최소 확대/축소 비율
        /// </summary>
        public double MinimumZoomRate
        {
            get
            {
                return (double)GetValue(MinimumZoomRateProperty);
            }
            set
            {
                SetValue(MinimumZoomRateProperty, value);
            }
        }

        #endregion
        #region 최대 확소/축소 비율 - MaximumZoomRate

        /// <summary>
        /// 최대 확소/축소 비율
        /// </summary>
        public double MaximumZoomRate
        {
            get
            {
                return (double)GetValue(MaximumZoomRateProperty);
            }
            set
            {
                SetValue(MaximumZoomRateProperty, value);
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Static

        #region 생성자 - DraggableCanvas()

        /// <summary>
        /// 생성자
        /// </summary>
        static DraggableCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata
            (
                typeof(DraggableCanvas),
                new FrameworkPropertyMetadata(typeof(DraggableCanvas))
            );
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 템플리트 적용시 처리하기 - OnApplyTemplate()

        /// <summary>
        /// 템플리트 적용시 처리하기
        /// </summary>
        public override void OnApplyTemplate()
        {
            DependencyObject dependencyObject = this.GetTemplateChild("PART_RootCanvas");

            if (dependencyObject is MovableCanvas == false)
            {
                throw new XamlParseException("'PART_RootCanvas' cannot be found.");
            }

            this.movableCanvas = dependencyObject as MovableCanvas;

            base.OnApplyTemplate();
        }

        #endregion
        #region 이동시키기 - MoveTo(item)

        /// <summary>
        /// 이동시키기
        /// </summary>
        /// <param name="item">항목</param>
        public void MoveTo(FrameworkElement item)
        {
            if(item == null)
            {
                return;
            }

            Point itemPoint = new Point();

            if(item is TreeViewItem && (item as TreeViewItem).HasHeader == true && (item as TreeViewItem).Header is FrameworkElement)
            {
                FrameworkElement itemHeader = (item as TreeViewItem).Header as FrameworkElement;

                itemPoint = item.TranslatePoint
                (
                    new Point(itemHeader.ActualWidth / 2, itemHeader.ActualHeight / 2),
                    this
                );
            }
            else
            {
                Rect boundRectangle = VisualTreeHelper.GetDescendantBounds(item);

                itemPoint = item.TranslatePoint
                (
                    new Point(boundRectangle.Width / 2, boundRectangle.Height / 2),
                    this
                );
            }

            this.movableCanvas.OffsetX = (this.movableCanvas.ActualWidth  / 2) - (itemPoint.X - this.movableCanvas.OffsetX);
            this.movableCanvas.OffsetY = (this.movableCanvas.ActualHeight / 2) - (itemPoint.Y - this.movableCanvas.OffsetY);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Protected

        /*
        #region 마우스 WHEEL 처리하기 - OnMouseWheel(e)

        /// <summary>
        /// 마우스 WHEEL 처리하기
        /// </summary>
        /// <param name="e">이벤트 발생자</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                double zoomRate = ZoomRate + ZoomInterval;

                ZoomRate = (zoomRate > MaximumZoomRate) ? MaximumZoomRate : zoomRate;
            }
            else if(e.Delta < 0)
            {
                double zoomRate = ZoomRate - ZoomInterval;

                ZoomRate = (zoomRate < MinimumZoomRate) ? MinimumZoomRate : zoomRate;
            }

            base.OnMouseWheel(e);
        }
        #endregion
    */
    }
}