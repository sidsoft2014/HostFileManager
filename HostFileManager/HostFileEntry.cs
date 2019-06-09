using System.ComponentModel;
using System.Net;

namespace HostFileManager
{
    public class HostFileEntry : INotifyPropertyChanged
    {
        private IPAddress iPAddress;
        private string domain;
        private bool isActive;

        public HostFileEntry(IPAddress ip, string domain)
        {
            IPAddress = ip;
            Domain = domain;
        }
        public HostFileEntry(IPAddress ip, string domain, bool isActive)
            : this(ip, domain)
        {
            IsActive = isActive;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IPAddress IPAddress
        {
            get => iPAddress;
            private set
            {
                if (iPAddress != value)
                {
                    iPAddress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPAddress)));
                }
            }
        }
        public string Domain
        {
            get => domain;
            set
            {
                if (domain != value)
                {
                    domain = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Domain)));
                }
            }
        }
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
                }
            }
        }

        public string IPString
        {
            get => IPAddress?.ToString();
            set
            {
                if (IPAddress.TryParse(value, out IPAddress ip))
                {
                    IPAddress = ip;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPAddress)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPString)));
                }
            }
        }
    }
}
