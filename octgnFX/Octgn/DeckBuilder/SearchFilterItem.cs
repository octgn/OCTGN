namespace Octgn.DeckBuilder
{
    using System;

    [Serializable]
    public class SearchFilterItem
    {
        public bool IsSetProperty { get; set; }
        public string PropertyName { get; set; }
        public string SelectedComparison { get; set; }
        public string CompareValue { get; set; }
    }
}