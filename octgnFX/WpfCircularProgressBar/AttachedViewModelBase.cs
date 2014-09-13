using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using LinqToVisualTree;
using System.Windows.Controls;

namespace WpfCircularProgressBar
{
    public abstract class AttachedViewModelBase : FrameworkElement, INotifyPropertyChanged
    {
        #region Attach attached property

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(object), typeof(AttachedViewModelBase),
                new PropertyMetadata(null, new PropertyChangedCallback(OnAttachChanged)));

        public static AttachedViewModelBase GetAttach(DependencyObject d)
        {
            return (AttachedViewModelBase)d.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject d, AttachedViewModelBase value)
        {
            d.SetValue(AttachProperty, value);
        }

        /// <summary>
        /// Change handler for the Attach property
        /// </summary>
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement targetElement = d as FrameworkElement;
            AttachedViewModelBase viewModel = e.NewValue as AttachedViewModelBase;
            viewModel.AttachedElement = targetElement;

            // handle the loaded event
            targetElement.Loaded += new RoutedEventHandler(Element_Loaded);

        }

        /// <summary>
        /// Handle the Loaded event of the element to which this view model is attached
        /// inorder to enable the attached
        /// view model to bind to properties of the parent element
        /// </summary>
        static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement targetElement = sender as FrameworkElement;
            FrameworkElement parent = targetElement.Parent as FrameworkElement;

            // use the attached view model as the DataContext of the element it is attached to
            AttachedViewModelBase attachedModel = GetAttach(targetElement);
            targetElement.DataContext = attachedModel;

            // find the ProgressBar and associated it with the view model
            var progressBar = targetElement.Ancestors<ProgressBar>().Single() as ProgressBar;
            attachedModel.SetProgressBar(progressBar);

            // bind the DataContext of the view model to the DataContext of the parent.
            attachedModel.SetBinding(AttachedViewModelBase.DataContextProperty,
              new Binding("DataContext")
              {
                  Source = parent
              });

            // bind the piggyback to give DataContext change notification
            attachedModel.SetBinding(AttachedViewModelBase.DataContextPiggyBackProperty,
              new Binding("DataContext")
              {
                  Source = parent
              });
        }

        #endregion

        #region DataContextPiggyBack attached property

        /// <summary>
        /// DataContextPiggyBack Attached Dependency Property, used as a mechanism for exposing
        /// DataContext changed events
        /// </summary>
        public static readonly DependencyProperty DataContextPiggyBackProperty =
            DependencyProperty.RegisterAttached("DataContextPiggyBack", typeof(object), typeof(AttachedViewModelBase),
                new PropertyMetadata(null, new PropertyChangedCallback(OnDataContextPiggyBackChanged)));

        public static object GetDataContextPiggyBack(DependencyObject d)
        {
            return (object)d.GetValue(DataContextPiggyBackProperty);
        }

        public static void SetDataContextPiggyBack(DependencyObject d, object value)
        {
            d.SetValue(DataContextPiggyBackProperty, value);
        }

        /// <summary>
        /// Handles changes to the DataContextPiggyBack property.
        /// </summary>
        private static void OnDataContextPiggyBackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AttachedViewModelBase viewModel = d as AttachedViewModelBase;
            viewModel.OnDataContextChanged();
        }

        /// <summary>
        /// Invoked when the DataContext changes
        /// </summary>
        private void OnDataContextChanged()
        {
            INotifyPropertyChanged adaptedDataContext = this.DataContext as INotifyPropertyChanged;
            if (adaptedDataContext != null)
            {
                adaptedDataContext.PropertyChanged += new PropertyChangedEventHandler(AdaptedDataContextPropertyChanged);
            }

            // when the DataContext changes, all computed properties are anticipated
            // to change as a result. Therefore we fire a generic property changed event
            OnPropertyChanged("");
        }


        /// <summary>
        /// Handles property changes from the adapted DataContext
        /// </summary>
        protected abstract void AdaptedDataContextPropertyChanged(object sender, PropertyChangedEventArgs e);


        #endregion

        public virtual void ComputeViewModelProperties()
        {

        }

        protected ProgressBar _progressBar;

        PropertyChangeNotifier valueNotifier;
        PropertyChangeNotifier maximumNotifier;
        PropertyChangeNotifier minimumNotifier;

        /// <summary>
        /// Add handlers for the updates on various properties of the ProgressBar
        /// </summary>
        private void SetProgressBar(ProgressBar progressBar)
        {
            _progressBar = progressBar;
            _progressBar.SizeChanged += (s, e) => ComputeViewModelProperties();
            
            valueNotifier = new PropertyChangeNotifier(_progressBar, "Value");
            valueNotifier.ValueChanged += valueNotifier_ValueChanged;

            maximumNotifier = new PropertyChangeNotifier(_progressBar, "Maximum");
            maximumNotifier.ValueChanged += maximumNotifier_ValueChanged;

            minimumNotifier = new PropertyChangeNotifier(_progressBar, "Minimum");
            minimumNotifier.ValueChanged += minimumNotifier_ValueChanged;

            ComputeViewModelProperties();
        }

        void minimumNotifier_ValueChanged(object sender, EventArgs e)
        {
 	        ComputeViewModelProperties();
        }

        void maximumNotifier_ValueChanged(object sender, EventArgs e)
        {
 	        ComputeViewModelProperties();
        }

        private void valueNotifier_ValueChanged(object sender, EventArgs e)
        {
 	        ComputeViewModelProperties();
        }

        public AttachedViewModelBase()
        {
        }

        #region INotifPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        private FrameworkElement _element;

        /// <summary>
        /// The element to which this view model is attached
        /// </summary>
        public FrameworkElement AttachedElement
        {
            get { return _element; }
            set
            {
                _element = value;
                _element.SizeChanged += new SizeChangedEventHandler(AttachedElement_SizeChanged);
            }
        }

        /// <summary>
        /// Handle SizeChanged events from the element that this view model is attached to
        /// so that we can inform elements
        /// of changes in the ActualHeight / ActualWidth
        /// </summary>
        protected virtual void AttachedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged("ElementHeight");
            OnPropertyChanged("ElementWidth");
        }

        public double ElementWidth
        {
            get { return _element.ActualWidth; }
        }

        public double ElementHeight
        {
            get { return _element.ActualHeight; }
        }
    }
}
