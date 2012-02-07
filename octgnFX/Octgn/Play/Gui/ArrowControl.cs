﻿using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace Octgn.Play.Gui
{
    class ArrowControl : DrawingVisual
    {
        private const double ArrowHalfWidth = 7;
        private const double HeadHalfWidthFactor = 2;
        private const double HeadLengthFactor = 3;
        private const double CurvatureAlong = 0.3;
        private const double CurvatureOrtho = 0.2;

        public static readonly DependencyProperty ToPointProperty = DependencyProperty.Register("ToPoint", typeof(Point), typeof(ArrowControl), new UIPropertyMetadata(PointChanged));
        public Point ToPoint
        {
            get { return (Point)GetValue(ToPointProperty); }
            set { SetValue(ToPointProperty, value); }
        }

        public static readonly DependencyProperty FromPointProperty = DependencyProperty.Register("FromPoint", typeof(Point), typeof(ArrowControl), new UIPropertyMetadata(PointChanged));
        public Point FromPoint
        {
            get { return (Point)GetValue(FromPointProperty); }
            set { SetValue(FromPointProperty, value); }
        }

        public Path Shape { get; private set; }

        private PathFigure startPt1;
        private BezierSegment startPt2, endPt1;
        private LineSegment arrowPt1, arrowPt2, headPt, endPt2;

        private static readonly SolidColorBrush fillBrush;

        static ArrowControl()
        {
            fillBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
            fillBrush.Freeze();
        }

        public ArrowControl()
        {
            endPt1 = new BezierSegment();
            endPt2 = new LineSegment();
            arrowPt1 = new LineSegment();
            arrowPt2 = new LineSegment();
            headPt = new LineSegment();
            startPt2 = new BezierSegment();
            startPt1 = new PathFigure()
            {
                Segments = new PathSegmentCollection() { endPt1, arrowPt1, headPt, arrowPt2, endPt2, startPt2 },
                IsClosed = true
            };
            Shape = new Path() { Data = new PathGeometry() { Figures = new PathFigureCollection() { startPt1 } } };
            Shape.Stroke = Brushes.Red;
            Shape.Fill = fillBrush;
        }

        private static void PointChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        { (sender as ArrowControl).ComputeShape(); }

        private void ComputeShape()
        {
            double ArrowHalfWidth = Program.Game.Definition.TableDefinition.Height * 0.017;    // 1/60th
            double HeadHalfWidth = ArrowHalfWidth * HeadHalfWidthFactor;
            double HeadLength = ArrowHalfWidth * HeadLengthFactor;

            // Compute the arrow base point so that the arrow tip is approximately at the ToPoint
            var dir = ToPoint - FromPoint;
            dir.Normalize();
            var ortho = new Vector(-dir.Y, dir.X);
            var arrowBasePoint = ToPoint + (-dir * CurvatureAlong + ortho * CurvatureOrtho) * HeadLength * 2;

            // Compute the base direction and othogonal vectors
            dir = arrowBasePoint - FromPoint;
            var length = dir.Length;
            dir /= length;
            ortho = new Vector(-dir.Y, dir.X);
            var widthVec = ortho * ArrowHalfWidth;
            // Compute the base bezier control points (actually compute vectors)
            var bezierBase = (dir * CurvatureAlong + ortho * CurvatureOrtho) * length;
            var bezierTip = (-dir * CurvatureAlong + ortho * CurvatureOrtho) * length;

            // Set the four base points
            startPt1.StartPoint = FromPoint - widthVec;

            endPt1.Point3 = arrowBasePoint - widthVec;
            endPt1.Point2 = endPt1.Point3 + bezierTip;
            endPt1.Point1 = startPt1.StartPoint + bezierBase;

            endPt2.Point = arrowBasePoint + widthVec;

            startPt2.Point3 = FromPoint + widthVec;
            startPt2.Point2 = startPt2.Point3 + bezierBase;
            startPt2.Point1 = endPt2.Point + bezierTip;

            //  Compute the arrow tip direction and orthogonal vectors
            dir = -bezierTip;
            dir.Normalize();
            ortho = new Vector(-dir.Y, dir.X);
            // Compute the three tip points
            var headVec = ortho * HeadHalfWidth;
            arrowPt1.Point = arrowBasePoint - headVec;
            arrowPt2.Point = arrowBasePoint + headVec;
            headPt.Point = arrowBasePoint + dir * HeadLength;

            // Adjust the arrow/base junction so that it's nicer
            endPt2.Point = arrowPt2.Point - headVec / 2;
            endPt1.Point3 = arrowPt1.Point + headVec / 2;
        }
    }
}
