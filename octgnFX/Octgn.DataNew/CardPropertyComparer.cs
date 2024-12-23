namespace Octgn.DataNew
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Octgn.DataNew.Entities;

    public class CardPropertyComparer : IComparer, IComparer<ICard>
    {
        private readonly PropertyDef _property;

        public PropertyDef Property { get { return _property; } }

        public CardPropertyComparer(PropertyDef property)
        {
            _property = property;
        }

        #region IComparer Members

        int IComparer.Compare(object x, object y)
        {
            return Compare(x as ICard, y as ICard);
        }

        #endregion

        #region IComparer<CardModel> Members

        public int Compare(ICard x, ICard y)
        {
            if (_property is NamePropertyDef || _property == null)
            {
                return x.PropertySets[x.Alternate].Name.CompareTo(y.PropertySets[y.Alternate].Name);
            }
            x.PropertySets[x.Alternate].Properties.TryGetValue(_property, out object px);
            y.PropertySets[y.Alternate].Properties.TryGetValue(_property, out object py);
            if (px == null) return py == null ? 0 : -1;
            if (int.TryParse(px.ToString(), out int px2) && int.TryParse(py.ToString(), out int py2))
            {
                return px2.CompareTo(py2);
            }
            return px.ToString().CompareTo(py.ToString());
        }

        #endregion
    }
    public class CompoundComparer : IComparer
    {
        private readonly IComparer first;
        private readonly IComparer second;

        public CompoundComparer(IComparer first, IComparer second)
        {
            this.first = first;
            this.second = second;
        }

        public int Compare(object x, object y)
        {
            int result = this.first.Compare(x, y);
            if (result != 0)
            {
                return result;
            }
            return second.Compare(x, y);
        }
    }

}