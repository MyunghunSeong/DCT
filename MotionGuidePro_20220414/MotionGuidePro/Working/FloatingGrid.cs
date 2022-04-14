using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrevisLibrary
{
    /// <summary>
    /// 플로팅 그리드
    /// </summary>
    public class FlottingGrid : Grid
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Dependency Property
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 그리드 오프셋 X 속성 - GridOffsetXProperty

        /// <summary>
        /// 그리드 오프셋 X 속성
        /// </summary>
        public static readonly DependencyProperty GridOffsetXProperty = DependencyProperty.Register
        (
            "GridOffsetX",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 그리드 오프셋 Y 속성 - GridOffsetYProperty

        /// <summary>
        /// 그리드 오프셋 Y 속성
        /// </summary>
        public static readonly DependencyProperty GridOffsetYProperty = DependencyProperty.Register
        (
            "GridOffsetY",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 확대/축소 비율 속성 - ZoomRateProperty

        /// <summary>
        /// 확대/축소 비율 속성
        /// </summary>
        public static readonly DependencyProperty ZoomRateProperty = DependencyProperty.Register
        (
            "ZoomRate",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 확대/축소 중심 X 속성 - ZoomCenterXProperty

        /// <summary>
        /// 확대/축소 중심 X 속성
        /// </summary>
        public static readonly DependencyProperty ZoomCenterXProperty = DependencyProperty.Register
        (
            "ZoomCenterX",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 확대/축소 중심 Y 속성 - ZoomCenterYProperty

        /// <summary>
        /// 확대/축소 중심 Y 속성
        /// </summary>
        public static readonly DependencyProperty ZoomCenterYProperty = DependencyProperty.Register
        (
            "ZoomCenterY",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 셀 카운트 속성 - CellCountProperty

        /// <summary>
        /// 셀 카운트 속성
        /// </summary>
        public static readonly DependencyProperty CellCountProperty = DependencyProperty.Register
        (
            "CellCount",
            typeof(int),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 셀 크기 속성 - CellSizeProperty

        /// <summary>
        /// 셀 크기 속성
        /// </summary>
        public static readonly DependencyProperty CellSizeProperty = DependencyProperty.Register
        (
            "CellSize",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 그리드 브러시 속성 - GridBrushProperty

        /// <summary>
        /// 그리드 브러시 속성
        /// </summary>
        public static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register
        (
            "GridBrush",
            typeof(Brush),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata
            (
                new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

        #endregion
        #region 그리드 테두리 속성 - GridBorderProperty

        /// <summary>
        /// 그리드 테두리 속성
        /// </summary>
        public static readonly DependencyProperty GridBorderProperty = DependencyProperty.Register
        (
            "GridBorder",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion
        #region 셀 브러시 속성 - CellBrushProperty

        /// <summary>
        /// 셀 브러시 속성
        /// </summary>
        public static readonly DependencyProperty CellBrushProperty = DependencyProperty.Register
        (
            "CellBrush",
            typeof(Brush),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata
            (
                new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                FrameworkPropertyMetadataOptions.AffectsRender
            )
        );

        #endregion
        #region 셀 테두리 속성 - CellBorderProperty

        /// <summary>
        /// 셀 테두리 속성
        /// </summary>
        public static readonly DependencyProperty CellBorderProperty = DependencyProperty.Register
        (
            "CellBorder",
            typeof(double),
            typeof(FlottingGrid),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 그리드 오프셋 X - GridOffsetX

        /// <summary>
        /// 그리드 오프셋 X
        /// </summary>
        public double GridOffsetX
        {
            get
            {
                return (double)GetValue(GridOffsetXProperty);
            }
            set
            {
                SetValue(GridOffsetXProperty, value);
            }
        }

        #endregion
        #region 그리드 오프셋 Y - GridOffsetY

        /// <summary>
        /// 그리드 오프셋 Y
        /// </summary>
        public double GridOffsetY
        {
            get
            {
                return (double) GetValue(GridOffsetYProperty);
            }
            set
            {
                SetValue(GridOffsetYProperty, value);
            }
        }

        #endregion
        #region 확대/축소 비율 - ZoomRate

        /// <summary>
        /// 확대/축소 비율
        /// </summary>
        public double ZoomRate
        {
            get
            {
                return (double) GetValue(ZoomRateProperty);
            }
            set
            {
                SetValue(ZoomRateProperty, value);
            }
        }

        #endregion
        #region 확대/축소 중심 X - ZoomCenterX

        /// <summary>
        /// 확대/축소 중심 X
        /// </summary>
        public double ZoomCenterX
        {
            get
            {
                return (double)GetValue(ZoomCenterXProperty);
            }
            set
            {
                SetValue(ZoomCenterXProperty, value);
            }
        }

        #endregion
        #region 확대/축소 중심 Y - ZoomCenterY

        /// <summary>
        /// 확대/축소 중심 Y
        /// </summary>
        public double ZoomCenterY
        {
            get
            {
                return (double)GetValue(ZoomCenterYProperty);
            }
            set
            {
                SetValue(ZoomCenterYProperty, value);
            }
        }

        #endregion
        #region 셀 카운트 - CellCount

        /// <summary>
        /// 셀 카운트
        /// </summary>
        public int CellCount
        {
            get
            {
                return (int) GetValue(CellCountProperty);
            }
            set
            {
                SetValue(CellCountProperty, value);
            }
        }

        #endregion
        #region 셀 크기 - CellSize

        /// <summary>
        /// 셀 크기
        /// </summary>
        public double CellSize
        {
            get
            {
                return (double) GetValue(CellSizeProperty);
            }
            set
            {
                SetValue(CellSizeProperty, value);
            }
        }

        #endregion
        #region 그리드 브러시 - GridBrush

        /// <summary>
        /// 그리드 브러시
        /// </summary>
        public Brush GridBrush
        {
            get
            {
                return (Brush) GetValue(GridBrushProperty);
            }
            set
            {
                SetValue(GridBrushProperty, value);
            }
        }

        #endregion
        #region 그리드 테두리 - GridBorder

        /// <summary>
        /// 그리드 테두리
        /// </summary>
        public double GridBorder
        {
            get
            {
                return (double) GetValue(GridBorderProperty);
            }
            set
            {
                SetValue(GridBorderProperty, value);
            }
        }

        #endregion
        #region 셀 브러시 - CellBrush

        /// <summary>
        /// 셀 브러시
        /// </summary>
        public Brush CellBrush
        {
            get
            {
                return (Brush) GetValue(CellBrushProperty);
            }
            set
            {
                SetValue(CellBrushProperty, value);
            }
        }

        #endregion
        #region 셀 테두리 - CellBorder

        /// <summary>
        /// 셀 테두리
        /// </summary>
        public double CellBorder
        {
            get
            {
                return (double) GetValue(CellBorderProperty);
            }
            set
            {
                SetValue(CellBorderProperty, value);
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Protected

        #region 최대 확대/축소 비율 - MaximumZoomRate

        /// <summary>
        /// 최대 확대/축소 비율
        /// </summary>
        protected virtual double MaximumZoomRate
        {
            get
            {
                return 4.0;
            }
        }

        #endregion
        #region 최소 확대/축소 비율 - MinimumZoomRate

        /// <summary>
        /// 최소 확대/축소 비율
        /// </summary>
        protected virtual double MinimumZoomRate
        {
            get
            {
                return 0.5;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Protected

        #region 렌더링시 처리하기 - OnRender(context)

        /// <summary>
        /// 렌더링시 처리하기
        /// </summary>
        /// <param name="context">드로잉 컨텍스트</param>
        protected override void OnRender(DrawingContext context)
        {
            /*
            base.OnRender(context);

            double zoomRateInterval = MaximumZoomRate - MinimumZoomRate;

            double translatedZoomRate = ZoomRate;

            if(ZoomRate <= MinimumZoomRate)
            {
                translatedZoomRate += zoomRateInterval;
            }
            else if(ZoomRate >= MaximumZoomRate)
            {
                translatedZoomRate -= zoomRateInterval;
            }

            double scaledCellSize = CellSize * translatedZoomRate;
            double scaledGridSize = scaledCellSize * CellCount;
            double templateSize   = scaledGridSize + (GridBorder / 2);

            double offsetX = 0.0;
            double offsetY = 0.0;
            */

            /*
            DrawingBrush imageBrush = new DrawingBrush()
            {
                TileMode      = TileMode.Tile,
                Viewport      = new Rect(offsetX + GridOffsetX, offsetY + GridOffsetY, templateSize, templateSize),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox       = new Rect(0, 0, templateSize, templateSize),
                ViewboxUnits  = BrushMappingMode.Absolute
            };

            DrawingGroup drawingGroup = new DrawingGroup();

            Brush cellBrush = CellBrush.Clone();

            double cellBrushOpacity = cellBrush.Opacity * (1.0 - ((MaximumZoomRate - translatedZoomRate) / zoomRateInterval));

            cellBrush.Opacity = cellBrushOpacity;

            Brush borderBrush = GridBrush.Clone();

            double borderBrushOpacity = borderBrush.Opacity * (this.MaximumZoomRate - translatedZoomRate) / zoomRateInterval;

            borderBrush.Opacity = borderBrushOpacity;

            Pen gridPen = new Pen(GridBrush, GridBorder);
            Pen cellPen = new Pen(cellBrush, CellBorder);

            using(DrawingContext groupContext = drawingGroup.Open())
            {
                groupContext.DrawLine(gridPen, new Point(scaledGridSize, 0), new Point(scaledGridSize, templateSize));
                groupContext.DrawLine(gridPen, new Point(0, scaledGridSize), new Point(templateSize, scaledGridSize));

                for(int i = 1; i < this.CellCount; i++)
                {
                    double offset = (double)((int)(scaledCellSize * i));

                    groupContext.DrawLine(cellPen, new Point(offset, 0), new Point(offset, templateSize));
                }

                for(int i = 1; i < this.CellCount; i++)
                {
                    double offset = (double)((int)(scaledCellSize * i));

                    groupContext.DrawLine(cellPen, new Point(0, offset), new Point(templateSize, offset));
                }
            }

            imageBrush.Drawing = drawingGroup;

            context.DrawRectangle(imageBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            */
        }

        #endregion
    }
}