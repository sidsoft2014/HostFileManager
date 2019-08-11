using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HostFileManager
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private HostFileService _fileService = new HostFileService();
        private HostFileEntry selectedHostFileEntry;
        private string _statusMessage;
        private ICommand _delete;

        public MainWindowViewModel()
        {
            HostFileEntries = new ObservableCollection<HostFileEntry>(_fileService.GetHostFileEntries());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Delete => _delete ?? (_delete = new DeleteCommand(this));

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
            try
            {
                HostFileEntries.Add(new HostFileEntry(new System.Net.IPAddress(16777343), "change.me", true));
                SelectedHostFileEntry = HostFileEntries.Last();
                StatusMessage = "New host added";
            }
            catch(Exception ex)
            {
                StatusMessage = ex.Message;
            }
            QueueStatusReset();
        }

        public void RemoveEntry()
        {
            try
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
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
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

        internal void DeleteEntry(string v)
        {
            var entry = HostFileEntries.FirstOrDefault(p => p.Domain == v);
            if (entry is null)
                return;

            SelectedHostFileEntry = entry;
            RemoveEntry();
        }
    }

    public class DeleteCommand : ICommand
    {
        public DeleteCommand(MainWindowViewModel vm)
        {
            this._vm = vm;
        }

        private MainWindowViewModel _vm;

        bool ICommand.CanExecute(object parameter)
        {
            return !string.IsNullOrWhiteSpace(parameter as string);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            _vm.DeleteEntry(parameter as string);
        }
    }
}
