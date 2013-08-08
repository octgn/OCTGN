namespace Octgn.DeckBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows;
    using System.Windows.Media;

    using Microsoft.Win32;

    using Octgn.Controls;

    using log4net;

    [Serializable]
    public class SearchSave
    {
        [NonSerialized]
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string FileName { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public List<SearchFilterItem> Filters { get; set; }

        public SearchSave()
        {
            this.Filters = new List<SearchFilterItem>();
        }

        public static SearchSave Create(SearchControl control)
        {
            var ss = new SearchSave();
            ss.GameId = control.Game.Id;
            ss.Name = control.SearchName.Clone() as string;
            ss.FileName = control.FileName.Clone() as string;

            // Load filters
            var generator = control.filterList.ItemContainerGenerator;
            for (int i = 0; i < control.filterList.Items.Count; i++)
            {
                var container = generator.ContainerFromIndex(i);
                var filterCtrl = (FilterControl)VisualTreeHelper.GetChild(container, 0);
                ss.AddFilter(filterCtrl);
            }
            return ss;
        }

        public static SearchSave Load()
        {
            var sf = new OpenFileDialog();
            sf.Filter = "OCTGN Search Save (*.o8ds)|*.o8ds";
            if ((bool)sf.ShowDialog())
            {
                return Load(sf.FileName);
            }
            return null;
        }

        public static SearchSave Load(string filename)
        {
            try
            {
                var ret = new SearchSave();
                var bf = new BinaryFormatter();
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ret = (SearchSave)bf.Deserialize(fs);
                }
                ret.FileName = filename;
                ret.Name = new FileInfo(ret.FileName).Name;
                return ret;
            }
            catch (Exception e)
            {
                Log.Warn("Load Error " + filename, e);
                TopMostMessageBox.Show(
                    "There was an error loading your search. Please consult the log files.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            return null;
        }

        public void AddFilter(FilterControl control)
        {
            var sfi = new SearchFilterItem();
            sfi.PropertyName = control.Property.Name.Clone() as string;
            if (control.Property is SetPropertyDef)
            {
                sfi.SelectedComparison = ((DataNew.Entities.Set)control.comparisonList.SelectedItem).Id.ToString();
                sfi.IsSetProperty = true;
            }
            else
            {
                sfi.SelectedComparison = ((SqlComparison)control.comparisonList.SelectedItem).Name.Clone() as string;
            }
            sfi.CompareValue = control.comparisonText.Text.Clone() as string;
            this.Filters.Add(sfi);
        }

        public bool Save()
        {
            if (string.IsNullOrWhiteSpace(this.FileName))
            {
                return this.SaveAs();
            }
            return this.SaveToFile(this.FileName);
        }

        public bool SaveAs()
        {
            var sf = new SaveFileDialog();
            sf.Filter = "OCTGN Search Save (*.o8ds)|*.o8ds";
            if (!String.IsNullOrWhiteSpace(this.FileName))
                sf.FileName = this.FileName;
            if ((bool)sf.ShowDialog())
            {
                if (this.SaveToFile(sf.FileName))
                    return true;
            }
            return false;
        }

        private bool SaveToFile(string filename)
        {
            try
            {
                var bf = new BinaryFormatter();
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    bf.Serialize(fs, this);
                    fs.Flush(true);
                }
                this.FileName = filename;
                this.Name = new FileInfo(this.FileName).Name;
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("SaveToFile Error " + filename, e);
                TopMostMessageBox.Show(
                    "There was an error saving your search. Please consult the log files.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            return false;
        }
    }
}