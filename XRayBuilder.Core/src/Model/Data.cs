using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XRayBuilder.Core.Model
{
    public class Data : INotifyPropertyChanged
    {
        private string _bio;
        private string _author;
        private string _book;
        private string _goodreads;
        private string _xraysource;

        public Data()
        {
            GoodreadsUrl = string.Empty;
            AuthorUrl = string.Empty;
            AuthorBio = string.Empty;
            BookUrl = string.Empty;
            XraySource = string.Empty;
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

        public string BookUrl
        {
            get => _book;
            set
            {
                if (_book == value) return;
                _book = value ?? string.Empty;
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