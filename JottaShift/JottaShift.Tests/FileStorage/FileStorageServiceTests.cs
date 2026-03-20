using JottaShift.Tests.TestData;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests.FileStorage;

public class FileStorageServiceTests(FileStorageFixture _fixture) : IClassFixture<FileStorageFixture>
{
    #region CopyFile
    [Fact]
    public void CopyFile_ShouldReturnFailure_WhenInvalidSource()
    {
        var invalidSourceFile = Path.Combine(
            _fixture.BaseDirectory,
            $"file-{Path.GetInvalidFileNameChars().First()}");

        var targetDirectory = Path.Combine(_fixture.BaseDirectory, "SomeTarget");

        var fileStorageService = _fixture.CreateFileStorageService();

        var result = fileStorageService.CopyFile(invalidSourceFile, targetDirectory);

        ResultAssert.ValueFailure(result);
    }

    [Fact]
    public void CopyFile_ShouldReturnFailure_WhenSourceFileDoesNotExist()
    {
        var targetDirectory = Path.Combine(_fixture.BaseDirectory, "SomeTarget");
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { targetDirectory, new MockDirectoryData() }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.CopyFile(
            _fixture.SomeValidFileFullPath, targetDirectory);

        ResultAssert.ValueFailure(result);
    }

    [Fact]
    public void CopyFile_ShouldReturnSuccess_WhenValidSourceAndDestination()
    {
        var sourceFileName = Path.GetFileName(_fixture.SomeValidFileFullPath);

        var destinationDirectory = Path.Combine(_fixture.BaseDirectory, "SomeTarget");
        var destinationFileFullPath = Path.Combine(destinationDirectory, sourceFileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.CopyFile(_fixture.SomeValidFileFullPath, destinationDirectory);

        ResultAssert.ValueSuccess(result, destinationFileFullPath);

        fileSystemMock.AssertFileExists(_fixture.SomeValidFileFullPath);
        fileSystemMock.AssertFileExists(destinationFileFullPath);
    }

    [Theory]
    [InlineData(@"noExtension")]
    [InlineData(@"\withInvalidChar.doc")]
    [InlineData(@"folder\fileName.jpg")]
    [InlineData(@"C:\fully-qualified\file.pdf")]
    [InlineData(@"C:\no-file\directory-only")]

    public void CopyFile_ShouldReturnFailure_WhenNewFileFileNameIsInvalid(string newFileName)
    {
        var targetDirectory = Path.Combine(_fixture.BaseDirectory);
        string expectedValue = Path.Combine(_fixture.BaseDirectory, newFileName);

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystem);

        var result = fileStorageService.CopyFile(
            _fixture.SomeValidFileFullPath,
            targetDirectory,
            newFileName);

        ResultAssert.ValueFailure(result);
        Assert.False(fileSystem.FileExists(expectedValue));
    }

    [Theory]
    [InlineData(@"simpleFileName.jpg")]
    [InlineData(@"img_20200615.jpg")]
    [InlineData(@"some-document.pdf")]

    public void CopyFile_ShouldReturnSuccess_WhenNewFileFileNameIsValid(string newFileName)
    {
        var targetDirectory = Path.Combine(_fixture.BaseDirectory);
        string expectedValue = Path.Combine(_fixture.BaseDirectory, newFileName);

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystem);

        var result = fileStorageService.CopyFile(
            _fixture.SomeValidFileFullPath,
            targetDirectory,
            newFileName);

        ResultAssert.ValueSuccess(result, expectedValue);
        Assert.True(fileSystem.FileExists(expectedValue));
    }
    #endregion

    #region DeleteDirectoryContent
    [Theory]
    [InlineData("")]
    [InlineData("some-invalid-path-string")]
    [InlineData(@"C:\valid\path")]
    public void DeleteDirectoryContent_ReturnFailure_WhenDirectoryDoesNotExists(string targetDirectoryPath)
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.BaseDirectory, new MockDirectoryData() },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.DeleteDirectoryContent(targetDirectoryPath);

        ResultAssert.Failure(result);
        fileSystemMock.AssertDirectoryExists(_fixture.BaseDirectory);
    }

    [Fact]
    public void DeleteDirectoryContent_ShouldDeleteContent_KeepRoot()
    {
        var firstSubDirectory = Path.Combine(_fixture.BaseDirectory, "lorem");
        var secondSubDirectory = Path.Combine(_fixture.BaseDirectory, "ipsum");
        var thirdSubDirectory = Path.Combine(_fixture.BaseDirectory, "dolor");
        var fourthSubDirectory = Path.Combine(_fixture.BaseDirectory, "lamet");


        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.BaseDirectory, new MockDirectoryData() },
            { firstSubDirectory, new MockDirectoryData() },
            { secondSubDirectory, new MockDirectoryData() },
            { Path.Combine(thirdSubDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(fourthSubDirectory, Path.GetRandomFileName()), new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.DeleteDirectoryContent(_fixture.BaseDirectory);

        ResultAssert.Success(result);

        Assert.Empty(fileSystemMock.AllFiles);

        fileSystemMock.AssertDirectoryExists(_fixture.BaseDirectory);
        fileSystemMock.AssertDirectoryDoesNotExist(firstSubDirectory);
        fileSystemMock.AssertDirectoryDoesNotExist(secondSubDirectory);
        fileSystemMock.AssertDirectoryDoesNotExist(thirdSubDirectory);
        fileSystemMock.AssertDirectoryDoesNotExist(fourthSubDirectory);
    }
    #endregion

    #region DeleteFile
    [Fact]
    public void DeleteFile_ShouldReturnSuccess_WhenFileDoesNotExist()
    {
        var fileStorageService = _fixture.CreateFileStorageService();

        var result = fileStorageService.DeleteFile(_fixture.SomeValidFileFullPath);

        ResultAssert.Success(result);
    }

    [Fact]
    public void DeleteFile_ShouldReturnFailure_WhenFilePathIsInvalidPath()
    {
        var fileStorageService = _fixture.CreateFileStorageService();

        var result = fileStorageService.DeleteFile("some-string-that-is-not-a-valid-path");

        ResultAssert.Failure(result);
    }

    [Fact]
    public void DeleteFile_ShouldReturnSuccess_WhenFileWasDeleted()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystem);

        var result = fileStorageService.DeleteFile(_fixture.SomeValidFileFullPath);

        ResultAssert.Success(result);
        fileSystem.AssertFileDoesNotExist(_fixture.SomeValidFileFullPath);
    }
    #endregion

    #region DoesFileMetadataMatch
    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void DoesFileMetadataMatch_DoesNotMatch_WhenDifferentImages()
    {
        var fileStorageService = _fixture.CreateFileStorageService(new FileSystem());

        var metadataMatchResult = fileStorageService.DoesFileMetadataMatch(
            TestDataHelper.Duck,
            TestDataHelper.Waterfall);

        ResultAssert.Failure(metadataMatchResult);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void DoesFileMetadataMatch_DoesMatch_WhenCopyOfSameImage()
    {
        var fileStorageService = _fixture.CreateFileStorageService(new FileSystem());

        var metadataMatchResult = fileStorageService.DoesFileMetadataMatch(
           TestDataHelper.Duck,
           TestDataHelper.DuckCopy);

        ResultAssert.Success(metadataMatchResult);
    }
    #endregion

    #region FilesAreBitPerfectMatch
    [Fact]
    public void FilesAreBitPerfectMatch_DoesNotMatch_WhenDifferentContentLength()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 8)
            .Select(x => (byte)x)
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 16)
          .Select(x => (byte)x)
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        ResultAssert.Failure(filesAreBitPerfectMatch);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesNotMatch_WhenDifferentContent()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 8)
            .Select(x => (byte)x)
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 8)
          .Select(x => (byte)(x * 2))
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        ResultAssert.Failure(filesAreBitPerfectMatch);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesMatch_WhenEqualContent()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 64)
            .Select(x => (byte)(x * 2))
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 64)
          .Select(x => (byte)(x * 2))
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        ResultAssert.Success(filesAreBitPerfectMatch);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void FilesAreBitPerfectMatch_DoesMatch_WhenCopyOfImage()
    {
        var fileStorageService = _fixture.CreateFileStorageService(new FileSystem());

        var filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(TestDataHelper.Duck, TestDataHelper.DuckCopy);

        ResultAssert.Success(filesAreBitPerfectMatch);
    }
    #endregion

    #region IsValidFileName
    [Theory]
    [InlineData("")]
    [InlineData("cleanString")]
    [InlineData("nameWithoutExtension.")]
    [InlineData("?name*With/Invalid:Chars>")]
    [InlineData(@"C:\fullyRooted\fileFullPath.tiff")]
    [InlineData(@"containsDirectory\file.cs")]
    public async Task IsValidFileName_ShouldDetectInvalidNames(string fileName)
    {
        var fileStorage = _fixture.CreateFileStorageService();

        var isValidFileNameResult = fileStorage.IsValidFileName(fileName);

        ResultAssert.Failure(isValidFileNameResult);
    }


    [Theory]
    [InlineData("simpleFile.png")]
    [InlineData("doubleExtension.jpg.png")]
    [InlineData("alphaNum31c.pdf")]
    public async Task IsValidFileName_ShouldDetectValidNames(string fileName)
    {
        var fileStorage = _fixture.CreateFileStorageService();

        var isValidFileNameResult = fileStorage.IsValidFileName(fileName);

        ResultAssert.Success(isValidFileNameResult);
    }
    #endregion

    #region ValidateDirectory
    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderExists()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.BaseDirectory, new MockDirectoryData() }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.ValidateDirectory(_fixture.BaseDirectory);

        ResultAssert.Success(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderIsCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var result = fileStorageService.ValidateDirectory(_fixture.BaseDirectory);

        ResultAssert.Success(result);
        fileSystemMock.AssertDirectoryExists(_fixture.BaseDirectory);
    }

    [Fact]
    public void ValidateDirectory_ShouldNotBeValidated_WhenFolderCannotBeCreated()
    {
        string invalidDirectoryName = $"some-invalid-{Path.GetInvalidPathChars().First()}-directory-name";
        
        var fileStorageService = _fixture.CreateFileStorageService();

        var result = fileStorageService.ValidateDirectory(invalidDirectoryName);
        
        ResultAssert.Failure(result);
    }
    #endregion

    #region GetFileName
    [Theory]
    [InlineData("")]
    [InlineData("cleanString")]
    [InlineData("nameWithoutExtension.")]
    [InlineData("?name*With/Invalid:Chars>")]
    [InlineData(@"notFullyRooted\file.cs")]
    [InlineData(@"C:\fullyRooted\invalidFile*Name.")]

    public async Task GetFileName_ShouldDetectInvalidFilePaths(string fileFullPath)
    {
        var fileStorage = _fixture.CreateFileStorageService();

        var fileNameResult = fileStorage.GetFileName(fileFullPath);

        ResultAssert.Failure(fileNameResult);
    }


    [Theory]
    [InlineData(@"C:\fullyRooted\fileFullPath.tiff")]
    [InlineData(@"C:\fullyRooted\doubleExtension.tiff.jpg")]
    public async Task GetFileName_ShouldDetectValidFilePaths(string fileFullPath)
    {
        var fileStorage = _fixture.CreateFileStorageService();

        var fileNameResult = fileStorage.GetFileName(fileFullPath);

        ResultAssert.Success(fileNameResult);
    }
    #endregion

    #region GetDirectoryName    
    [Theory]
    [InlineData("")]
    [InlineData("someString")]
    [InlineData(@"someFileName.pdf")]
    [InlineData(@"some\incomplete-path")]
    [InlineData(@"invalid\chars<>")]
    [InlineData(@"c:\")]
    public void GetDirectoryName_ShouldDetectInvalidFilePaths(string directoryFullPath)
    {
        var fileStorage = _fixture.CreateFileStorageService();
        var directoryNameResult = fileStorage.GetDirectoryName(directoryFullPath);
        ResultAssert.Failure(directoryNameResult);
    }

    [Theory]
    [InlineData(@"c:\root", "root")]
    [InlineData(@"c:\root\someDirectoryWithContent\someFileName.pdf", "someDirectoryWithContent")]
    [InlineData(@"c:\root\someCompletePath\someDirectoryWithoutContent", "someDirectoryWithoutContent")]

    // TODO. Should be able to handle this scenario. Will currently detect is as a file wil ".23" as extension.
    //[InlineData(
    //    @"c:\root\someConflictedJottaCloudDirectory (Conflict 2026-02-21 14.23.23)",
    //    "someConflictedJottaCloudDirectory (Conflict 2026-02-21 14.23.23)")]
    public void GetDirectoryName_ShouldDetectValidDirectoryPaths(string directoryFullPath, string expectedDirectoryName)
    {
        var fileStorage = _fixture.CreateFileStorageService();
        var directoryNameResult = fileStorage.GetDirectoryName(directoryFullPath);
        ResultAssert.ValueSuccess(directoryNameResult, expectedDirectoryName);
    }
    #endregion

    #region GetFileBytesAsync
    [Fact]
    public async Task GetFileBytesAsync_ShouldReturnFailedResult_WhenFileNotFound()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.BaseDirectory, new MockDirectoryData() }
        });
        var fileStorage = _fixture.CreateFileStorageService(fileSystem);

        var contentResult = await fileStorage.GetFileBytesAsync(_fixture.SomeValidFileFullPath);

        ResultAssert.ValueFailure(contentResult);
    }

    [Fact]
    public async Task GetFileBytesAsync_ShouldReturnFailedResult_WhenFileIsEmpty()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData([]) }
        });
        var fileStorage = _fixture.CreateFileStorageService(fileSystem);

        var contentResult = await fileStorage.GetFileBytesAsync(_fixture.SomeValidFileFullPath);

        ResultAssert.ValueFailure(contentResult);
    }

    [Fact]
    public async Task GetFileBytesAsync_ShouldReturnFailedResult_WhenExceptionIsThrown()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>()))
            .Returns(true);

        mockFileSystem.Setup(fs => fs.File.ReadAllBytesAsync(It.IsAny<string>()))
            .Throws(new IOException("Simulated IO exception"));

        var fileStorage = _fixture.CreateFileStorageService(mockFileSystem.Object);

        var contentResult = await fileStorage.GetFileBytesAsync(_fixture.SomeValidFileFullPath);

        ResultAssert.ValueFailure(contentResult);
    }


    [Fact]
    public async Task GetFileBytesAsync_ShouldReturnSuccessfullResult_WhenContentIsValid()
    {
        byte[] fileContent = [1, 2, 3, 4, 5];
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileFullPath, new MockFileData(fileContent) }
        });
        var fileStorage = _fixture.CreateFileStorageService(fileSystem);

        var contentResult = await fileStorage.GetFileBytesAsync(_fixture.SomeValidFileFullPath);

        ResultAssert.ValueSuccess(contentResult, fileContent);
    }
    #endregion

    #region GetImageDate
    [Fact]
    public void GetImageDate_ReturnsFailedResult_WhenFileNotFound()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var imageDateResult = fileStorageService.GetImageDate(string.Empty);

        ResultAssert.ValueFailure(imageDateResult);
    }

    [Fact]
    public void GetImageDate_ReturnsFailedResult_WhenFileHasNoValidDate()
    {
        string fileNameWithCurrentDate = Path.GetRandomFileName();

        string fileNameWithDefaultDate = Path.GetRandomFileName();
        var fileDataWithDefaultDate = new MockFileData(fileNameWithDefaultDate)
        {
            LastWriteTime = default
        };

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { fileNameWithCurrentDate, new MockFileData([]) },
            { fileNameWithDefaultDate, fileDataWithDefaultDate },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        // Test <LastWriteTime = DateTime.Now>
        var imageDateResult = fileStorageService.GetImageDate(fileNameWithCurrentDate);
        ResultAssert.ValueFailure(imageDateResult);

        // Test <LastWriteTime = default>
        imageDateResult = fileStorageService.GetImageDate(fileNameWithDefaultDate);
        ResultAssert.ValueFailure(imageDateResult);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void GetImageDate_ReturnsDateTakenExiffTag_WhenFileFound()
    {
        var fileStorageService = _fixture.CreateFileStorageService(new FileSystem());

        var imageDateResult = fileStorageService.GetImageDate(TestDataHelper.Duck);

        ResultAssert.Success(imageDateResult);

        Assert.Equal(2025, imageDateResult.Value.Year);
        Assert.Equal(5, imageDateResult.Value.Month);
        Assert.Equal(17, imageDateResult.Value.Day);
        Assert.Equal(13, imageDateResult.Value.Hour);
        Assert.Equal(42, imageDateResult.Value.Minute);
    }

    [Theory]
    [MemberData(nameof(GetImageFilenameTestData))]
    public void GetImageDate_ReturnsDateFromFileName_WhenNoMetadataFound(string filename, int expectedYear, int expectedMonth, int expectedDay)
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filename, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var imageDateResult = fileStorageService.GetImageDate(filename);

        ResultAssert.Success(imageDateResult);

        Assert.Equal(expectedYear, imageDateResult.Value.Year);
        Assert.Equal(expectedMonth, imageDateResult.Value.Month);
        Assert.Equal(expectedDay, imageDateResult.Value.Day);
    }
    #endregion

    #region GetImageResolution
    [Fact]
    public void GetImageResolution_ShouldReturnFailure_WhenFileNotFound()
    {
        var fileStorageService = _fixture.CreateFileStorageService();

        var resolutionResult = fileStorageService.GetImageResolution(_fixture.SomeValidFileName);

        ResultAssert.ValueFailure(resolutionResult);
    }

    [Fact]
    public void GetImageResolution_ShouldReturnFailure_WhenFileDoesNotContainMetadata()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.SomeValidFileName, new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystem);

        var resolutionResult = fileStorageService.GetImageResolution(_fixture.SomeValidFileName);

        ResultAssert.ValueFailure(resolutionResult);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void GetImageResolution_GetResolutionForTestFile()
    {
        var fileStorageService = _fixture.CreateFileStorageService(new FileSystem());

        var resolutionResult = fileStorageService.GetImageResolution(TestDataHelper.Duck);

        ResultAssert.ValueSuccess(resolutionResult, "4096x3072");
    }
    #endregion

    #region SearchFileByExactName
    [Fact]
    public void SearchFileByExactName_ReturnsFirstMatchingFilePath_WhenFileNameFound()
    {
        string fileName = Path.GetRandomFileName();        

        string directoryWithFile = Path.Combine(_fixture.BaseDirectory, "documents", "reports");
        string expectedFullPath = Path.Combine(directoryWithFile, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _fixture.BaseDirectory, new MockDirectoryData() },
            { Path.Combine(_fixture.BaseDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(directoryWithFile, Path.GetRandomFileName()), new MockFileData([]) },
            { expectedFullPath, new MockFileData([]) },
            
            // Duplicate filename in deeper sub-folder
            { Path.Combine(directoryWithFile, "2010", fileName), new MockFileData([]) },
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        // Find file when searched recursivley
        var recursiveResult = fileStorageService.SearchFileByExactName(
            _fixture.BaseDirectory,
            fileName,
            searchRecursively: true);

        ResultAssert.ValueSuccess(recursiveResult, expectedFullPath);

        // Return null when only top directory is searched
        var topDirectoryresult = fileStorageService.SearchFileByExactName(
            _fixture.BaseDirectory,
            fileName,
            searchRecursively: false);

        ResultAssert.ValueFailure(topDirectoryresult);
    }
    #endregion

    #region EnumerateDirectories
    [Fact]
    public void EnumerateDirectories_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateFiles(string.Empty);

        Assert.Empty(collection);
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerate_FlatStructure()
    {
        var baseDirectory = AppContext.BaseDirectory;

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockDirectoryData() }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateDirectories(baseDirectory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerateTopLevel_WhenMultiLayered()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var firstSubLevelDirectoryOne = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var firstSubLevelDirectoryTwo = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var secondSubLevelDirectory = Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName());
        var thirdSubLevelDirectory = Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName());

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { firstSubLevelDirectoryOne, new MockDirectoryData() },
            { Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName()), new MockFileData([]) },
            { firstSubLevelDirectoryTwo, new MockDirectoryData() },

            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockDirectoryData() },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockDirectoryData() }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateDirectories(baseDirectory);

        Assert.Equal(2, collection.Count());
    }
    #endregion

    #region EnumerateFiles
    [Fact]
    public void EnumerateFiles_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateFiles(string.Empty);

        Assert.Empty(collection);
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_FlatStructure()
    {
        var directory = AppContext.BaseDirectory;
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() },
            { Path.Combine(directory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(directory, Path.GetRandomFileName()), new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateFiles(directory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_Recursive()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var firstSubLevelDirectory = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var secondSubLevelDirectory = Path.Combine(firstSubLevelDirectory, Path.GetRandomFileName());
        var thirdSubLevelDirectory = Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName());

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { secondSubLevelDirectory, new MockDirectoryData() },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { thirdSubLevelDirectory, new MockDirectoryData() },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) }
        });

        var fileStorageService = _fixture.CreateFileStorageService(fileSystemMock);

        var collection = fileStorageService.EnumerateFiles(baseDirectory);

        Assert.Equal(10, collection.Count());
    }
    #endregion

    #region SanitizeStringToValidDirectoryName
    [Theory]
    [InlineData("Senua’s Saga: Hellblade II")]
    [InlineData("Folder<Name>")]
    [InlineData("Image*.jpg")]
    [InlineData("railingSpace ")]
    [InlineData("TrailingDot.")]
    [InlineData("Multiple.. ")]
    [InlineData("")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void SanitizeStringToValidDirectoryName_ShouldSanitizeNameWithInvalidChars(string name)
    {
        var fileStorage = _fixture.CreateFileStorageService();

        string sanitizedName = fileStorage.SanitizeStringToValidDirectoryName(name);

        var asCharArray = sanitizedName.ToCharArray();
        var invalidChars = Path.GetInvalidFileNameChars();

        var matching = asCharArray.Intersect(invalidChars);

        Assert.Empty(matching);
    }
    #endregion

    public static IEnumerable<object[]> GetImageFilenameTestData()
    {
        return TestDataHelper.ImageFilenamesWithDates.Select(data => new object[] { data.Filename, data.Year, data.Month, data.Day }).ToList();
    }  
}
