using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostFileManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private HostFileService _fileService = new HostFileService();
        private HostFileEntry selectedHostFileEntry;
        private string _statusMessage;

        public MainWindowViewModel()
        {
            HostFileEntries = new ObservableCollection<HostFileEntry>(_fileService.GetHostFileEntries());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<HostFileEntry> HostFileEntries { get; set; }
        public HostFileEntry SelectedHostFileEntry
        {
            get => selectedHostFileEntry;
            set
            {
                if (value != selectedHostFileEntry)
                {
                    selectedHostFileEntry = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedHostFileEntry)));
                }
            }
        }
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (value != _statusMessage)
                {
                    _statusMessage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
                }
            }
        }

        public void AddEntry()
        {
            HostFileEntries.Add(new HostFileEntry(new System.Net.IPAddress(16777343), "change.me", true));
            SelectedHostFileEntry = HostFileEntries.Last();
            StatusMessage = "New host added";
            QueueStatusReset();
        }

        public void RemoveEntry()
        {
            if (SelectedHostFileEntry is null)
            {
                StatusMessage = "No host selected to remove";
                return;
            }

            if (HostFileEntries.Remove(SelectedHostFileEntry))
            {
                SelectedHostFileEntry = null;
                StatusMessage = "Host removed";
            }
            else
            {
                StatusMessage = "Could not remove host";
            }
            QueueStatusReset();
        }

        public void Write()
        {
            StatusMessage = _fileService.Write(HostFileEntries) ? "File updated" : "Error updating file";
            QueueStatusReset();
        }

        private void QueueStatusReset()
        {
            Task.Run(() =>
            {
                Thread.Sleep(1250);
                StatusMessage = "";
            });
        }
    }
}
