using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octgn.DataNew.Entities;
using System.Linq;
using System.Windows;
using Octgn.Utils;

namespace Octgn.Play
{
    public sealed class Phase : INotifyPropertyChanged
    {
        
        internal static Phase Find(byte id)
        {
            return Program.GameEngine.AllPhases.FirstOrDefault(p => p.Id == id);
        }

        private readonly GamePhase _model;
        private readonly byte _id;
        private bool _hold;
        private bool _isActive;

        internal Phase(byte id, GamePhase model)
        {
            _id = id;
            _model = model;
            _hold = false;
        }
        
        
        internal byte Id
        {
            get { return _id; }
        }

        public bool Hold
        {
            get { return _hold; }
            set {
                if (_hold == value) return;
                _hold = value;
                OnPropertyChanged("Hold");
            }
        }
    
        public string Name
        {
            get { return _model.Name; }
        }

        public string Icon
        {
            get { return _model.Icon; }
        }
        
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        #region INotifyPropertyChanged Members
                 
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}