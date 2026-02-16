using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public record FolderOptions(string folderFullPath, bool createIfNotExists);
