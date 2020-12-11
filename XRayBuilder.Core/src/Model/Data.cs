using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XRayBuilder.Core.Model
{
    public class Data : INotifyPropertyChanged
    {
        private static string _author;
        private static string _bio;
        private static string _bookPath;
        private static string _bookUrl;
        private static string _goodreads;
        private static string _xraysource;

        public Data()
        {
            AuthorBio = string.Empty;
            AuthorUrl = string.Empty;
            BookPath = string.Empty;
            BookUrl = string.Empty;
            GoodreadsUrl = string.Empty;
            XraySource = string.Empty;
        }

        public string AuthorBio
        {
            get => _bio;
            set
            {
                if (_bio == value) return;
                _bio = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }

        public string AuthorUrl
        {
            get => _author;
            set
            {
                if (_author == value) return;
                _author = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }

        public string BookPath
        {
            get => _bookPath;
            set
            {
                if (_bookPath == value) return;
                _bookPath = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }

        public string BookUrl
        {
            get => _bookUrl;
            set
            {
                if (_bookUrl == value) return;
                _bookUrl = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }

        public string GoodreadsUrl
        {
            get => _goodreads;
            set
            {
                if (_goodreads == value) return;
                _goodreads = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }
        
        public string XraySource
        {
            get => _xraysource;
            set
            {
                if (_xraysource == value) return;
                _xraysource = value ?? string.Empty;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}