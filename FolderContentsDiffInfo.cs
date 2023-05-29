using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class FolderContentsDiffInfo
{
    public ConcurrentDictionary<string, bool> ChangedFiles = new();

    public ConcurrentDictionary<string, bool> AddedFiles = new();
    public ConcurrentDictionary<string, bool> RemovedFiles = new();

    public ConcurrentDictionary<string, bool> AddedDirectories = new();
    public ConcurrentDictionary<string, bool> RemovedDirectories = new();
}