using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DocumentScanner
{
    public class ImportItem : INotifyPropertyChanged
    {
        private string _destinationFileName;
        private string _filePath;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public string DestinationFileName
        {
            get { return _destinationFileName; }
            set
            {
                _destinationFileName = value;
                OnPropertyChanged();
            }
        }

        public bool FileExists
        {
            get
            {
                return File.Exists(FilePath);
            }
        }

        public bool IsValid
        {
            get
            {
                return FileExists && !string.IsNullOrEmpty(DestinationFileName);
            }
        }
    }
}
