using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Octgn.Play.Gui
{
    internal class ArrowControl : DrawingVisual
    {
        //private const double ArrowHalfWidth = 7;
        private const double HeadHalfWidthFactor = 2;
        private const double HeadLengthFactor = 3;
        private const double CurvatureAlong = 0.3;
        private const double CurvatureOrtho = 0.2;

        public static readonly DependencyProperty ToPointProperty = DependencyProperty.Register("ToPoint",
                                                                                                typeof (Point),
                                                                                                typeof (ArrowControl),
                                                                                                new UIPropertyMetadata(
                                                                                                    PointChanged));

        public static readonly DependencyProperty FromPointProperty = DependencyProperty.Register("FromPoint",
                                                                                                  typeof (Point),
                                                                                                  typeof (ArrowControl),
                                                                                                  new UIPropertyMetadata
                                                                                                      (PointChanged));

        private static readonly SolidColorBrush FillBrush;

        private readonly LineSegment _arrowPt1;
        private readonly LineSegment _arrowPt2;
        private readonly BezierSegment _endPt1;
        private readonly LineSegment _endPt2;
        private readonly LineSegment _headPt;
        private readonly PathFigure _startPt1;
        private readonly BezierSegment _startPt2;

        static ArrowControl()
        {
            FillBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
            FillBrush.Freeze();
        }

        public ArrowControl()
        {
            _endPt1 = new BezierSegment();
            _endPt2 = new LineSegment();
            _arrowPt1 = new LineSegment();
            _arrowPt2 = new LineSegment();
            _headPt = new LineSegment();
            _startPt2 = new BezierSegment();
            _startPt1 = new PathFigure
                            {
                                Segments =
                                    new PathSegmentCollection
                                        {_endPt1, _arrowPt1, _headPt, _arrowPt2, _endPt2, _startPt2},
                                IsClosed = true
                            };
            Shape = new Path
                        {
                            Data = new PathGeometry {Figures = new PathFigureCollection {_startPt1}},
                            Stroke = Brushes.Red,
                            Fill = FillBrush
                        };
        }

        public Point ToPoint
        {
            get { return (Point) GetValue(ToPointProperty); }
            set { SetValue(ToPointProperty, value); }
        }

        public Point FromPoint
        {
            get { return (Point) GetValue(FromPointProperty); }
            set { SetValue(FromPointProperty, value); }
        }

        public Path Shape { get; private set; }

        private static void PointChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var arrowControl = sender as ArrowControl;
            if (arrowControl != null) arrowControl.ComputeShape();
        }

        private void ComputeShape()
        {
            double arrowHalfWidth = Program.GameEngine.Definition.Table.Height*0.017; // 1/60th
            double headHalfWidth = arrowHalfWidth*HeadHalfWidthFactor;
            double headLength = arrowHalfWidth*HeadLengthFactor;

            // Compute the arrow base point so that the arrow tip is approximately at the ToPoint
            Vector dir = ToPoint - FromPoint;
            dir.Normalize();
            var ortho = new Vector(-dir.Y, dir.X);
            Point arrowBasePoint = ToPoint + (-dir*CurvatureAlong + ortho*CurvatureOrtho)*headLength*2;

            // Compute the base direction and othogonal vectors
            dir = arrowBasePoint - FromPoint;
            double length = dir.Length;
            dir /= length;
            ortho = new Vector(-dir.Y, dir.X);
            Vector widthVec = ortho*arrowHalfWidth;
            // Compute the base bezier control points (actually compute vectors)
            Vector bezierBase = (dir*CurvatureAlong + ortho*CurvatureOrtho)*length;
            Vector bezierTip = (-dir*CurvatureAlong + ortho*CurvatureOrtho)*length;

            // Set the four base points
            _startPt1.StartPoint = FromPoint - widthVec;

            _endPt1.Point3 = arrowBasePoint - widthVec;
            _endPt1.Point2 = _endPt1.Point3 + bezierTip;
            _endPt1.Point1 = _startPt1.StartPoint + bezierBase;

            _endPt2.Point = arrowBasePoint + widthVec;

            _startPt2.Point3 = FromPoint + widthVec;
            _startPt2.Point2 = _startPt2.Point3 + bezierBase;
            _startPt2.Point1 = _endPt2.Point + bezierTip;

            //  Compute the arrow tip direction and orthogonal vectors
            dir = -bezierTip;
            dir.Normalize();
            ortho = new Vector(-dir.Y, dir.X);
            // Compute the three tip points
            Vector headVec = ortho*headHalfWidth;
            _arrowPt1.Point = arrowBasePoint - headVec;
            _arrowPt2.Point = arrowBasePoint + headVec;
            _headPt.Point = arrowBasePoint + dir*headLength;

            // Adjust the arrow/base junction so that it's nicer
            _endPt2.Point = _arrowPt2.Point - headVec/2;
            _endPt1.Point3 = _arrowPt1.Point + headVec/2;
        }
    }
}