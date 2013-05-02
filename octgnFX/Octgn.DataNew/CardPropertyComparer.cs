namespace Octgn.DataNew
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Octgn.DataNew.Entities;

    public class CardPropertyComparer : IComparer, IComparer<ICard>
    {
        private readonly bool _isName;
        private readonly string _propertyName;

        public CardPropertyComparer(string propertyName)
        {
            _propertyName = propertyName;
            _isName = propertyName == "Name";
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
            if (_isName)
                return String.CompareOrdinal(x.Name, y.Name);

            object px = x.Properties[x.Alternate].Properties[new PropertyDef(){Name=_propertyName}];
            object py = y.Properties[y.Alternate].Properties[new PropertyDef() { Name = _propertyName }];
            if (px == null) return py == null ? 0 : -1;
            return ((IComparable)px).CompareTo(py);
        }

        #endregion
    }
}