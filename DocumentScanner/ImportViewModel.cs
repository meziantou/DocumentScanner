using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DocumentScanner
{
    internal class ImportViewModel : INotifyPropertyChanged
    {
        protected readonly SynchronizationContext SyncContext = SynchronizationContext.Current;

        private string _destinationPath;

        public ImportViewModel()
        {
            DestinationPath = Environment.CurrentDirectory;
            Files = new ObservableCollection<ImportItem>();
            Files.CollectionChanged += Files_CollectionChanged;
        }

        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _importCommand?.OnCanExecuteChanged();
        }

        public string DestinationPath
        {
            get { return _destinationPath; }
            set
            {
                _destinationPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DestinationPathExists));
            }
        }

        public bool DestinationPathExists => !string.IsNullOrEmpty(DestinationPath) && Directory.Exists(DestinationPath);

        public ObservableCollection<ImportItem> Files { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OpenDestinationFolderInExplorer()
        {
            if (DestinationPathExists)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = DestinationPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private ICommand _openDestinationFolderInExplorerCommand;
        public ICommand OpenDestinationFolderInExplorerCommand
        {
            get
            {
                return _openDestinationFolderInExplorerCommand ?? (_openDestinationFolderInExplorerCommand = new RelayCommand(
                    p => OpenDestinationFolderInExplorer(),
                    p => DestinationPathExists));
            }
        }

        private RelayCommand _importCommand;
        public ICommand ImportCommand
        {
            get
            {
                return _importCommand ?? (_importCommand = new RelayCommand(
                    p => Import(),
                    p => Files.Any() && DestinationPathExists));
            }
        }

        private ICommand _scanCommand;
        public ICommand ScanCommand
        {
            get
            {
                return _scanCommand ?? (_scanCommand = new RelayCommand(async p => await ScanAsync()));
            }
        }

        private ICommand _browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                return _browseCommand ?? (_browseCommand = new RelayCommand(p =>
                {
                    var dialog = new Meziantou.Framework.Win32.Dialogs.OpenFolderDialog();
                    if (dialog.ShowDialog() != Meziantou.Framework.Win32.Dialogs.DialogResult.OK)
                        return;

                    if (Directory.Exists(dialog.SelectedPath))
                    {
                        DestinationPath = dialog.SelectedPath;
                    }
                }));
            }
        }

        private ICommand _removeItemCommand;
        public ICommand RemoveItemCommand
        {
            get
            {
                return _removeItemCommand ?? (_removeItemCommand = new RelayCommand(
                    p =>
                    {
                        var item = p as ImportItem;
                        if (item != null)
                        {
                            Files.Remove(item);
                        }
                    }));
            }
        }

        private ICommand _previewItemCommand;
        public ICommand PreviewItemCommand
        {
            get
            {
                return _previewItemCommand ?? (_previewItemCommand = new RelayCommand(
                    p =>
                    {
                        var item = p as ImportItem;
                        if (item != null)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = item.FilePath,
                                UseShellExecute = true,
                                Verb = "open"
                            });
                        }
                    }));
            }
        }

        private ICommand _openItemInExplorerCommand;
        public ICommand OpenItemInExplorerCommand
        {
            get
            {
                return _openItemInExplorerCommand ?? (_openItemInExplorerCommand = new RelayCommand(
                    p =>
                    {
                        var item = p as ImportItem;
                        if (item != null)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = "/select, \"" + item.FilePath + "\"",
                                UseShellExecute = true,
                                Verb = "open"
                            });
                        }
                    }));
            }
        }

        public void Import()
        {
            if (!DestinationPathExists)
                return;

            if (!Directory.Exists(DestinationPath))
            {
                Directory.CreateDirectory(DestinationPath);
            }

            var importItems = Files.Where(item => item.IsValid).ToList();
            foreach (var importItem in importItems)
            {
                File.Move(importItem.FilePath, Path.Combine(DestinationPath, importItem.DestinationFileName));
                Files.Remove(importItem);
            }
        }

        public async Task ScanAsync()
        {
            // Use a task so the popup is not modal
            // await so the exception is handled correctly
            string fileName = $"{DateTime.UtcNow.ToString("yyyyMMdd_hhmmss")}.jpg";
            string path = Path.Combine(Path.GetTempPath(), $"documentpro_{fileName}");

            ImportItem item = new ImportItem();
            item.DestinationFileName = fileName;
            item.FilePath = path;
            Files.Add(item);

            await Task.Run(() =>
            {
                try
                {
                    if (!Scanner.Scan(path))
                    {
                        InvokeIfNeeded(() => Files.Remove(item));
                        return;
                    }
                }
                catch
                {
                    InvokeIfNeeded(() => Files.Remove(item));
                    throw;
                }

                if (!item.FileExists)
                {
                    InvokeIfNeeded(() => Files.Remove(item));
                }
            });
        }

        protected void InvokeIfNeeded(Action action)
        {
            if (action == null)
                return;

            if (SyncContext != SynchronizationContext.Current)
            {
                SyncContext.Post(state => action(), null);
            }
        }
    }
}
