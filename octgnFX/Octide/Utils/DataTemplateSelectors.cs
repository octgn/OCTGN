// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octide.ItemModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Octide
{
    public class PlayerGroupPanelTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is PileItemModel pile)
            {
                PropertyChangedEventHandler lambda = null;
                lambda = (o, args) =>
                {
                    if (args.PropertyName == "ViewState")
                    {
                        pile.PropertyChanged -= lambda;
                        var cp = (ContentPresenter)container;
                        cp.ContentTemplateSelector = null;
                        cp.ContentTemplateSelector = this;
                    }
                };
                pile.PropertyChanged += lambda;

                switch (pile.ViewState)
                {
                    case Octgn.DataNew.Entities.GroupViewState.Pile:
                        return element.FindResource("GroupPanel") as DataTemplate;
                    case Octgn.DataNew.Entities.GroupViewState.Collapsed:
                        return element.FindResource("CollapsedGroupPanel") as DataTemplate;
                    case Octgn.DataNew.Entities.GroupViewState.Expanded:
                        return element.FindResource("ExpandedGroupPanel") as DataTemplate;
                }
            }
            return null;
        }
    }
}
