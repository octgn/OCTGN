// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;

namespace Octide.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using GalaSoft.MvvmLight.Command;
    using Microsoft.Win32;
    using System.Windows;
    using System.Xml;
    using System.Collections.Generic;
	using Octide.ItemModel;
	using Octide.SetTab.ItemModel;

	public class ImportCardsViewModel : ViewModelBase
	{
		public SetModel Parent { get; set; }
		
		public string FilePath { get; set; }
		public RelayCommand ImportCSVCommand { get; set; }
		public RelayCommand ImportClipboardCommand { get; set; }
		public RelayCommand SaveCommand { get; set; }

		public ObservableCollection<PropertyMappingItem> Mappings { get; set; }
		
		public ObservableCollection<HeaderData> Headers { get; set; }
		public ObservableCollection<CardImportData> Cards { get; set; }

		public PropertyMappingItem NameHeader { get; set; }
		public PropertyMappingItem SizeHeader { get; set; }
		public PropertyMappingItem IdHeader { get; set; }
		public PropertyMappingItem AlternateHeader { get; set; }
		

		
		public string StatusMessage { get; set; }

		public ImportCardsViewModel()
		{
			Headers = new ObservableCollection<HeaderData>();
			Cards = new ObservableCollection<CardImportData>();
			Mappings = new ObservableCollection<PropertyMappingItem>();
			ImportCSVCommand = new RelayCommand(ImportCSV);
			ImportClipboardCommand = new RelayCommand(ImportClipboard);
			SaveCommand = new RelayCommand(SaveSet);
		}

		public void ImportCSV()
		{
			var fo = new OpenFileDialog();
			fo.Filter = "Excel Files(*.XLS;*.XLSX)|*.XLS;*.XLSX|CSV Files(*.CSV)|*.CSV";
			if ((bool)fo.ShowDialog())
			{
				if (File.Exists(fo.FileName))
				{
					FilePath = fo.FileName;
					using (var reader = new StreamReader(fo.FileName))
					{
						{
						}
					}
				}
			}
		}

		public void ImportClipboard()
		{
			Headers.Clear();
			Cards.Clear();
			Mappings.Clear();
			NameHeader = null;
			SizeHeader = null;
			IdHeader = null;
			AlternateHeader = null;

			var clipboard = Clipboard.GetDataObject();
			if (clipboard.GetDataPresent("Xml Spreadsheet"))
			{
				StreamReader stream = new StreamReader((MemoryStream)clipboard.GetData("Xml Spreadsheet"));
				stream.BaseStream.SetLength(stream.BaseStream.Length - 1);
				XmlDocument xml = new XmlDocument();
				xml.LoadXml(stream.ReadToEnd());
				ParseXml(xml);
				StatusMessage = string.Format("Successfully imported {0} headers.", Headers.Count());

				NameHeader = new PropertyMappingItem()
				{
					Property = ViewModelLocator.PropertyTabViewModel.NameProperty,
					Header = Headers.FirstOrDefault(x => x.PropertyName.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
					
				};
				RaisePropertyChanged("NameHeader");
				SizeHeader = new PropertyMappingItem()
				{
					Property = ViewModelLocator.PropertyTabViewModel.SizeProperty,
					Header = Headers.FirstOrDefault(x => x.PropertyName.Equals("Size", StringComparison.InvariantCultureIgnoreCase))
				};
				RaisePropertyChanged("SizeHeader");
				IdHeader = new PropertyMappingItem()
				{
					Property = ViewModelLocator.PropertyTabViewModel.IdProperty,
					Header = Headers.FirstOrDefault(x => x.PropertyName.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
				};
				RaisePropertyChanged("IdHeader");
				AlternateHeader = new PropertyMappingItem()
				{
					Property = ViewModelLocator.PropertyTabViewModel.AlternateProperty,
					Header = Headers.FirstOrDefault(x => x.PropertyName.Equals("Alternate", StringComparison.InvariantCultureIgnoreCase))
				};
				RaisePropertyChanged("AlternateHeader");
				foreach (PropertyItemModel customprop in ViewModelLocator.PropertyTabViewModel.Items)
				{
					Mappings.Add(new PropertyMappingItem()
					{
						Property = customprop,
						Header = Headers.FirstOrDefault(x => x.PropertyName.Equals(customprop.Name, StringComparison.InvariantCultureIgnoreCase))
					});

				}
			}
			else
			{
				StatusMessage = "Invalid Clipboard Data";
			}
			RaisePropertyChanged("StatusMessage");
		}
		
		public void ParseXml(XmlDocument doc)
		{
			XmlNodeList rows = doc.GetElementsByTagName("Row");
			foreach (XmlNode row in rows)
			{
				int count = 1;
				if (Headers.Count == 0) //initiate headers
				{
					foreach (XmlNode cell in row)
					{
						if (cell.Attributes["ss:Index"] != null)
						{
							count = int.Parse(cell.Attributes["ss:Index"].Value);
						}
						if (cell["Data"] != null)
						{
							var HeaderItem = new HeaderData();
							HeaderItem.index = count;
							HeaderItem.PropertyName = cell["Data"].FirstChild.Value;
							Headers.Add(HeaderItem);
						}
						count++;
					}
				}
				else
				{
					var CardItem = new CardImportData();
					foreach (XmlNode cell in row)
					{
						if (cell.Attributes["ss:Index"] != null)
						{
							count = int.Parse(cell.Attributes["ss:Index"].Value);
						}
						if (cell["Data"] != null)
						{
							HeaderData MatchedHeader = Headers.FirstOrDefault(x => x.index == count);
							if (MatchedHeader != null)
							{
								CardItem.Properties.Add(MatchedHeader, cell["Data"].FirstChild.Value);
							}
						}
						count++;
					}
					Cards.Add(CardItem);
				}
			}
		}

		public void SaveSet()
		{
			foreach (var cardData in Cards)
			{
				var guid = (IdHeader?.Header == null) ? Guid.NewGuid() : Guid.Parse(cardData.GetProperty(IdHeader.Header));
				var alternate = (AlternateHeader?.Header == null) ? "" : cardData.GetProperty(AlternateHeader.Header);
				var name = (NameHeader?.Header == null) ? "Card" : cardData.GetProperty(NameHeader.Header);
				SizeItemModel size = (SizeHeader?.Header == null)
					? (SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.DefaultItem
					: GetSize(cardData.GetProperty(SizeHeader.Header));


				CardModel setCard = (CardModel)Parent.CardItems.FirstOrDefault(x => ((CardModel)x).Id == guid);
				if (setCard == null) //no card matches
				{
					setCard = new CardModel(Parent.CardItems);
					setCard._card.Id = guid;
					Parent.CardItems.Add(setCard);
				}
				AlternateModel cardAlternate = (string.IsNullOrEmpty(alternate)) ? (AlternateModel)setCard.Items.DefaultItem : (AlternateModel)setCard.Items.FirstOrDefault(x => ((AlternateModel)x).Type == alternate);

				if (cardAlternate == null) // no alternate matches
				{
					cardAlternate = new AlternateModel(Parent.CardItems)
					{
						Type = alternate,
						SizeProperty = size,
						Name = name
					};
					if (string.IsNullOrEmpty(alternate))
					{
						setCard.Items.DefaultItem = cardAlternate;
					}
					else
					{
						setCard.Items.Add(cardAlternate);
					}
				}
				foreach (var mapping in Mappings)
				{
					if (mapping.IsEnabled)
					{
						var propertyValue = cardData.GetProperty(mapping.Header);
						if (propertyValue == null)
						{
							continue;
						}						
						var altProperty = cardAlternate.Items.FirstOrDefault(x => x.Property == mapping.Property);
						if (altProperty == null)
						{
							altProperty = new CardPropertyModel
							{
								Property = mapping.Property,
								Parent = cardAlternate,
								_isDefined = true
							};
						}
						altProperty.Value = propertyValue;
					}
				}
				setCard.UpdateCardName();
			}
			
		}

		public SizeItemModel GetSize(string size)
		{
			return (SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.FirstOrDefault(x => ((SizeItemModel)x).Name == size) ?? (SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.DefaultItem;
		}
		
	}

	public class CardImportData
	{
		public Dictionary<HeaderData, string> Properties { get; set; }

		public CardImportData()
		{
			Properties = new Dictionary<HeaderData, string>();
		}
		
		public string GetProperty(HeaderData header)
		{
			return Properties.ContainsKey(header) ? Properties[header] : null;
		}
	}

	public class HeaderData
	{
		public int index { get; set; }
		public string PropertyName { get; set; }

	}

	public class PropertyMappingItem : ViewModelBase
	{
		public PropertyItemModel Property { get; set; }
		public bool _isEnabled;
		public HeaderData _header;

		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				if (_isEnabled == value) return;
				_isEnabled = value;
				if (value == false)
				{
					Header = null;
				}
				RaisePropertyChanged("Header");
				RaisePropertyChanged("IsEnabled");
			}
		}
		public HeaderData Header
		{
			get
			{
				return _header;
			}
			set
			{
				if (_header == value) return;
				_header = value;
				_isEnabled = value != null;

				RaisePropertyChanged("Header");
			}
		}
	}
}