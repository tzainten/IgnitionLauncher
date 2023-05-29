using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public enum PacketType : int
{
    None,
    CompareFileHash,
    RequestFullDownload,
    RequestDownloadFolder,
    RequestFileCount,
    RequestDownloadFile,
    FileMismatched,
    DoneComparingFileHashes,
    AckFolder,
    DoneAckingFolders,
    NotifyOfMissingFolders,
    NotifyOfMissingFiles,
    RequestMissingFile,
    RequestMissingFolder
}