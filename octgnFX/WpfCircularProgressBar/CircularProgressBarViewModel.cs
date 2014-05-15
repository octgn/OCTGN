using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LinqToVisualTree;

namespace WpfCircularProgressBar
{
    /// <summary>
    /// An attached view model that adapts a ProgressBar control to provide properties
    /// that assist in the creation of a circular template
    /// </summary>
    public class CircularProgressBarViewModel : AttachedViewModelBase
    {
        #region fields

        private double _angle;

        private double _centreX;

        private double _centreY;

        private double _radius;

        private double _innerRadius;

        private double _diameter;

        private double _percent;

        private double _holeSizeFactor = 0.0;

        #endregion

        #region properties

        public double Percent
        {
            get { return _percent; }
            set { _percent = value; OnPropertyChanged("Percent"); }
        }

        public double Diameter
        {
            get { return _diameter; }
            set { _diameter = value; OnPropertyChanged("Diameter"); }
        }

        public double Radius
        {
            get { return _radius; }
            set { _radius = value; OnPropertyChanged("Radius"); }
        }

        public double InnerRadius
        {
            get { return _innerRadius; }
            set { _innerRadius = value; OnPropertyChanged("InnerRadius"); }
        }

        public double CentreX
        {
            get { return _centreX; }
            set { _centreX = value; OnPropertyChanged("CentreX"); }
        }

        public double CentreY
        {
            get { return _centreY; }
            set { _centreY = value; OnPropertyChanged("CentreY"); }
        }

        public double Angle
        {
            get { return _angle; }
            set { _angle = value; OnPropertyChanged("Angle"); }
        }

        public double HoleSizeFactor
        {
            get { return _holeSizeFactor; }
            set { _holeSizeFactor = value; ComputeViewModelProperties(); }
        }

        #endregion


        /// <summary>
        /// Re-computes the various properties that the elements in the template bind to.
        /// </summary>
        public override void ComputeViewModelProperties()
        {
            if (_progressBar == null)
                return;

            Angle = (_progressBar.Value - _progressBar.Minimum) * 360 / (_progressBar.Maximum - _progressBar.Minimum);
            CentreX = _progressBar.ActualWidth / 2;
            CentreY = _progressBar.ActualHeight / 2;
            Radius = Math.Min(CentreX, CentreY);
            Diameter = Radius * 2;
            InnerRadius = Radius * HoleSizeFactor;
            Percent = Angle / 360;
        }

        protected override void AdaptedDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName == "Value")
            {
                // ValueAngle depends on value
                OnPropertyChanged("ValueAngle");
            }
            else
            {
                // otherwise fire a generic property changed
                OnPropertyChanged("");
            }*/
        }
    }
}
