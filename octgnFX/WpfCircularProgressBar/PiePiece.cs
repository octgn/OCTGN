using ScottLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfCircularProgressBar
{
    /// <summary>
    /// A pie piece shape
    /// </summary>
    public class PiePiece : Canvas
    {
        #region events

        public delegate void PiePieceClicked(PiePiece sender);

        public event PiePieceClicked PiePieceClickedEvent;

        #endregion

        /// <summary>
        /// The radius of this pie piece
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
           DependencyProperty.Register("Radius", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The fill color of this pie piece
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
           DependencyProperty.Register("Fill", typeof(Brush), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The fill color of this pie piece
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
           DependencyProperty.Register("Stroke", typeof(Brush), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The inner radius of this pie piece
        /// </summary>
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
           DependencyProperty.Register("InnerRadius", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The wedge angle of this pie piece in degrees
        /// </summary>
        public double WedgeAngle
        {
            get { return (double)GetValue(WedgeAngleProperty); }
            set { SetValue(WedgeAngleProperty, value); }
        }

        public static readonly DependencyProperty WedgeAngleProperty =
           DependencyProperty.Register("WedgeAngle", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));


        /// <summary>
        /// The rotation, in degrees, from the Y axis vector of this pie piece.
        /// </summary>
        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty RotationAngleProperty =
           DependencyProperty.Register("RotationAngle", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The Y coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        public static readonly DependencyProperty CentreYProperty =
           DependencyProperty.Register("CentreY", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));

        /// <summary>
        /// The Y coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        public static readonly DependencyProperty CentreXProperty =
           DependencyProperty.Register("CentreX", typeof(double), typeof(PiePiece), new PropertyMetadata(OnDependencyPropertyChanged));


        /// <summary>
        /// The value that this pie piece represents.
        /// </summary>
        public double PieceValue { get; set; }

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PiePiece source = d as PiePiece;
            source.AddPathToCanvas();
        }


        public PiePiece()
        {
            AddPathToCanvas();
            this.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(PiePiece_MouseLeftButtonUp);
        }

        /// <summary>
        /// Handle left mouse button click event, hit testing to see if the pie piece was hit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PiePiece_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // was IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), this);
            HitTestResult hits = VisualTreeHelper.HitTest(this, e.GetPosition(null));

            if (hits != null)
            {
                // This does not seem to be working...
                if (hits.VisualHit is Path)
                {
                    Path path = hits.VisualHit as Path;
                    if (path != null)
                    {
                        if (PiePieceClickedEvent != null)
                        {
                            PiePieceClickedEvent(this);
                        }
                    }
                }
            }
        }

        private void AddPathToCanvas()
        {
            this.Children.Clear();

            String tooltip = (WedgeAngle / 360.00).ToString("#0.##%");

            Path path = ConstructPath();
            ToolTipService.SetToolTip(path, tooltip);
            this.Children.Add(path);
        }

        /// <summary>
        /// Constructs a path that represents this pie segment
        /// </summary>
        /// <returns></returns>
        private Path ConstructPath()
        {
            if (WedgeAngle >= 360)
            {
                Path path = new Path()
                {
                    Fill = this.Fill,
                    Stroke = this.Stroke,
                    StrokeThickness = 1,
                    Data = new GeometryGroup()
                    {
                        FillRule = System.Windows.Media.FillRule.EvenOdd,
                        Children = new GeometryCollection()
            {
              new EllipseGeometry()
              {
                Center = new Point(CentreX, CentreY),
                RadiusX = Radius,
                RadiusY = Radius
              },
              new EllipseGeometry()
              {
                Center = new Point(CentreX, CentreY),
                RadiusX = InnerRadius,
                RadiusY = InnerRadius
              }
            },

                    }
                };
                return path;
            }



            Point startPoint = new Point(CentreX, CentreY);
       
            Point innerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, InnerRadius).OffsetEx(CentreX, CentreY);
            Point innerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius).OffsetEx(CentreX, CentreY);
            Point outerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, Radius).OffsetEx(CentreX, CentreY);
            Point outerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius).OffsetEx(CentreX, CentreY);

            bool largeArc = WedgeAngle > 180.0;
            Size outerArcSize = new Size(Radius, Radius);
            Size innerArcSize = new Size(InnerRadius, InnerRadius);

            PathFigure figure = new PathFigure()
            {
                StartPoint = innerArcStartPoint,
                Segments = new PathSegmentCollection()
              {
                  new LineSegment()
                  {
                      Point = outerArcStartPoint
                  },
                  new ArcSegment()
                  {
                      Point = outerArcEndPoint,
                      Size = outerArcSize,
                      IsLargeArc = largeArc,
                      SweepDirection = SweepDirection.Clockwise,
                      RotationAngle = 0
                  },
                  new LineSegment()
                  {
                      Point = innerArcEndPoint
                  },
                  new ArcSegment()
                  {
                      Point = innerArcStartPoint,
                      Size = innerArcSize,
                      IsLargeArc = largeArc,
                      SweepDirection = SweepDirection.Counterclockwise,
                      RotationAngle = 0
                  }
              }
            };

            return new Path()
            {
                Fill = this.Fill,
                Stroke = this.Stroke,
                StrokeThickness = 1,
                Data = new PathGeometry()
                {
                    Figures = new PathFigureCollection()
                    {
                        figure
                    }
                }
            };
        }


    }

}
