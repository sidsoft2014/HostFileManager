using System.Collections.Generic;

namespace hfm.core
{
    public interface IHostFileService
    {
        ResultMessage AddSingleAndWrite(HostFileEntry entry);
        IEnumerable<HostFileEntry> GetHostFileEntries();
        ResultMessage RemoveSingleEntry(HostFileEntry entry);
        ResultMessage ToggleSingleEntry(HostFileEntry entry);
        bool Write(IEnumerable<HostFileEntry> hostFileEntries);
    }
}