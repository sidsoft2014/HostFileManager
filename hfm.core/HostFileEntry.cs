using System;
using System.ComponentModel;
using System.Net;

namespace hfm.core
{
    public class HostFileEntry : INotifyPropertyChanged, IEquatable<HostFileEntry>
    {
        private IPAddress _iPAddress;
        private string _domain;
        private bool _isActive;

        public HostFileEntry()
        {
            IPAddress = new IPAddress(16777343);
        }
        public HostFileEntry(string domain)
            : this()
        {
            Domain = domain;
        }
        public HostFileEntry(IPAddress ip, string domain)
            : this(domain)
        {
            IPAddress = ip;
        }
        public HostFileEntry(IPAddress ip, string domain, bool isActive)
            : this(ip, domain)
        {
            IsActive = isActive;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IPAddress IPAddress
        {
            get => _iPAddress;
            set
            {
                if (_iPAddress != value)
                {
                    _iPAddress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPAddress)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPString)));
                }
            }
        }
        public string Domain
        {
            get => _domain;
            set
            {
                if (_domain != value)
                {
                    _domain = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Domain)));
                }
            }
        }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
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
                    _iPAddress = ip; // Change background field to avoid double triggers
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPAddress)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPString)));
                }
            }
        }

        public bool Equals(HostFileEntry other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is HostFileEntry other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            try
            {
                return IPString.GetHashCode() ^ Domain.GetHashCode();
            }
            catch
            {
                try
                {
                    return Domain.GetHashCode();
                }
                catch
                {
                    return IPString.GetHashCode();
                }
            }
        }
    }
}
