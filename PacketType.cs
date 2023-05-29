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
    RequestDownloadFile,
    FileMismatched
}