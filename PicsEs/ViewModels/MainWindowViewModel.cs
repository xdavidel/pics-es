using Microsoft.Win32;
using PicsEs.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Drawing;
using System.Text;
using System;
using System.Threading.Tasks;

namespace PicsEs.ViewModels
{
	class MainWindowViewModel : BaseViewModel
	{
		private static readonly string[] IMAGES_EXT = { "png", "jpeg", "jpg" };
        private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			set { _isBusy = value; OnPropertyChanged(); }
		}

		private int _numOfFiles;
		public int NumOfFiles
		{
			get { return _numOfFiles; }
			set { _numOfFiles = value; OnPropertyChanged(); }
		}

		private int _completedFiles;

		public int CompletedFiles
		{
			get { return _completedFiles; }
			set { _completedFiles = value; OnPropertyChanged(); }
		}

        private int _skippedFiles;

        public int SkippedFiles
        {
            get { return _skippedFiles; }
            set { _skippedFiles = value; OnPropertyChanged(); }
        }



        private ObservableCollection<FileInfo> _files;
		public ObservableCollection<FileInfo> Files
		{
			get
			{
				if (_files == null) { _files = new ObservableCollection<FileInfo>(); }
				return _files;
			}
			set
			{
				_files = value;
			}
		}

		private ICommand _selectFilesCommand;
		public ICommand SelectFilesCommand
		{
			get
			{
				return _selectFilesCommand ?? (_selectFilesCommand = new DelegateCommand(p =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Multiselect = true,
                        Filter = @"Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*"
                    };
                    if (dialog.ShowDialog() == true)
					{
						_files.Clear();
						foreach (var fileName in dialog.FileNames)
						{
							Files.Add(new FileInfo(fileName));
						}
					}


				}));
			}
		}

		private ICommand _selectFolderCommand;
		public ICommand SelectFolderCommand
		{
			get
			{
				return _selectFolderCommand ?? (_selectFolderCommand = new DelegateCommand(p =>
				{
					var dialog = new System.Windows.Forms.FolderBrowserDialog();
					dialog.ShowDialog();

					if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
					{
						_files.Clear();
						var dir = new DirectoryInfo(dialog.SelectedPath);
						foreach (var file in dir.GetFiles())
						{
							Files.Add(file);
						}
					}

				}));
			}
		}

		private ICommand _convertCommand;
		public ICommand ConvertCommand
		{
			get
			{
				return _convertCommand ?? (_convertCommand = new DelegateCommand(p =>
				{
					Convert(p as IEnumerable<object>);
				}, p =>
				 {
					 if (p is IEnumerable<object> e && e.Count() > 0)
					 {
						 foreach (FileInfo file in e)
						 {
							 if (!(
								file.Extension.Contains("png") ||
								file.Extension.Contains("jpeg") ||
								file.Extension.Contains("jpg")))
							 {
								 return false;
							 }
						 }
						 return true;
					 }
					 return false;
				 }));
			}
		}

		private ICommand _convertAllCommand;
		public ICommand ConvertAllCommand
		{
			get
			{
				return _convertAllCommand ?? (_convertAllCommand = new DelegateCommand(p =>
				{
					Convert(Files);
				}, p =>
				{
					return Files != null && Files.Count > 0;
				}));
			}
		}

		private async void Convert(IEnumerable<object> files)
		{
			NumOfFiles = files.Count();
			CompletedFiles = 0;
            SkippedFiles = 0;
            IsBusy = true;
			await Task.Run(() =>
			{
				foreach (FileInfo file in files)
				{
					if (IMAGES_EXT.Any(s => file.Extension.Contains(s)))
					{
						var dateStr = GetDate(file.Name);
                        if (dateStr != null)
                        {
                            SaveImage(file, dateStr);
                            CompletedFiles++;
                        }
                        else
                        {
                            SkippedFiles++;
                        }
					}
                    
				}
			});

			IsBusy = false;
		}

		private string GetDate(string fileName)
		{
			var startIndex = GetFirstIndexOfDate(fileName);
			if (startIndex > -1)
			{
				var length = GetLengthOfDate(startIndex, fileName);
				return fileName.Substring(startIndex, length);
			}
			else
			{
				return null;
			}
		}

		private int GetFirstIndexOfDate(string fileName)
		{
			for (int i = 0; i < fileName.Length; i++)
			{
				if (char.IsDigit(fileName[i]))
				{
					return i;
				}
			}

			return -1;
		}

		private int GetLengthOfDate(int start, string fileName)
		{
			var count = 0;
			for (int i = start; i < fileName.Length; i++)
			{
				if (!char.IsDigit(fileName[i]))
				{
					return count;
				}
				count++;
			}

			return count;
		}

		private void SaveImage(FileInfo file, string dateStr)
		{
			if (dateStr != null && dateStr.Length >= 8)
			{
				var id = 36867;
				var year = dateStr.Substring(0, 4);
				var month = dateStr.Substring(4, 2);
				var day = dateStr.Substring(6, 2);
                try
                {
					// Read a file and resize it.
					Image img = Image.FromFile(file.FullName);
					foreach (var p in img.PropertyItems)
					{
						p.Id = id++;
						p.Value = Encoding.ASCII.GetBytes($"{year}:{month}:{day} 00:00:00\0");
						p.Type = 2;
						p.Len = 20;
						img.SetPropertyItem(p);
					}

					img.Save($"{file.DirectoryName}/New_{file.Name}");

				}
				catch (Exception)
                {
					//can't find the Data Picture Taken time property

				}
			}
		}
	}
}
