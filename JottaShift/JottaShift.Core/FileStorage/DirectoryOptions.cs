using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public record DirectoryOptions(string directoryFullPath, bool createIfNotExists);
