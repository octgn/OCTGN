// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;

namespace Octgn.Core
{
    public class GameFeedGame : INotifyPropertyChanged, IEquatable<GameFeedGame>
    {
        public Guid Id { get; }

        public GameFeedGame() {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(GameFeedGame other) {
            if (other == null) return false;
            return Id == other.Id;
        }
    }
}
