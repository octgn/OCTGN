using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octide.Messages;
using System.Windows;

namespace Octide.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// You can also use Blend to data bind with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		public RelayCommand LoadCommand { get; private set; }
		public RelayCommand SaveCommand { get; private set; }

		public MainViewModel()
		{
			SaveCommand = new RelayCommand(SaveGame);
			LoadCommand = new RelayCommand(LoadLoaderWindow);
		}

		private void SaveGame()
		{
			ViewModelLocator.GameLoader.SaveGame();
		}

		private void LoadLoaderWindow()
		{
			//TODO: Make this work -- app doesn't like showing previously-closed windows.
			if (CleanupCurrentGame())
			{
				Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Create));
				Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.Show));
				Messenger.Default.Send(new WindowActionMessage<LoaderViewModel>(WindowActionType.SetMain));
				Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Close));
				ViewModelLocator.Cleanup();
			}
		}
		public bool CleanupCurrentGame()
		{
			if (ViewModelLocator.GameLoader.NeedsSave && ViewModelLocator.GameLoader.DidManualSave)
			{
				var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (res == MessageBoxResult.Yes)
				{
					ViewModelLocator.GameLoader.SaveGame();
				}
				else if (res == MessageBoxResult.Cancel)
				{
					return false;
				}
			}
			if (ViewModelLocator.GameLoader.DidManualSave == false)
			{
				ViewModelLocator.GameLoader.DeleteGame();
			}
			return true;
		}
	}
}