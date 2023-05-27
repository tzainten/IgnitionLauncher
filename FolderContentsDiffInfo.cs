using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgnitionLauncher;

public class FolderContentsDiffInfo
{
    public List<string> ChangedFiles = new();

    public List<string> AddedFiles = new();
    public List<string> RemovedFiles = new();

    public List<string> AddedDirectories = new();
    public List<string> RemovedDirectories = new();
}