using JottaShift.Core.FileStorage;
using JottaShift.Tests;
using JottaShift.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests.FileStorage;

public class FileStorageServiceTests(FileStorageFixture _fixture) : IClassFixture<FileStorageFixture>
{
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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = fileStorageService.DeleteDirectoryContent(targetDirectoryPath);

        ResultAssert.Failure(result);
        Assert.True(fileSystemMock.Directory.Exists(_fixture.BaseDirectory));
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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = fileStorageService.DeleteDirectoryContent(_fixture.BaseDirectory);

        ResultAssert.Success(result);

        Assert.Empty(fileSystemMock.AllFiles);

        Assert.True(fileSystemMock.Directory.Exists(_fixture.BaseDirectory));
        Assert.False(fileSystemMock.Directory.Exists(firstSubDirectory));
        Assert.False(fileSystemMock.Directory.Exists(secondSubDirectory));
        Assert.False(fileSystemMock.Directory.Exists(thirdSubDirectory));
        Assert.False(fileSystemMock.Directory.Exists(fourthSubDirectory));
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
        Assert.False(fileSystem.File.Exists(_fixture.SomeValidFileFullPath));
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

    #region GetFileContent
    [Fact]
    public async Task GetFileContent_ShouldReturnFailedResult_WhenFileNotFound()
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
    public async Task GetFileContent_ShouldReturnFailedResult_WhenFileIsEmpty()
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
    public async Task GetFileContent_ShouldReturnFailedResult_WhenExceptionIsThrown()
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
    public async Task GetFileContent_ShouldReturnSuccessfullResult_WhenContentIsValid()
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

        var fileStorageService = new FileStorageService(
           fileSystemMock,
           new Mock<ILogger<FileStorageService>>().Object);

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

    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderExists()
    {
        var directory = AppContext.BaseDirectory;
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(directory, false);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderIsCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            true);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldNotBeValidated_WhenFolderDoesNotExist()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            false);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldNotBeValidated_WhenFolderCannotBeCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(string.Empty, true);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.False(result);
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateDirectories(baseDirectory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateFiles(baseDirectory);

        Assert.Equal(10, collection.Count());
    }

    #region GetImageDate
    [Fact]
    public void GetImageDate_ReturnsFailedResult_WhenFileNotFound()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

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
        var fileSystem = new FileSystem();

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var imageDateResult = fileStorageService.GetImageDate(filename);

        ResultAssert.Success(imageDateResult);

        Assert.Equal(expectedYear, imageDateResult.Value.Year);
        Assert.Equal(expectedMonth, imageDateResult.Value.Month);
        Assert.Equal(expectedDay, imageDateResult.Value.Day);
    }
    #endregion

    public static IEnumerable<object[]> GetImageFilenameTestData()
    {
        return TestDataHelper.ImageFilenamesWithDates.Select(data => new object[] { data.Filename, data.Year, data.Month, data.Day }).ToList();
    }

    [Fact]
    public async Task CopyFile_ShouldFail_WhenSourceDoesNotExist()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destinationFileName);

        var copied = fileSystemMock.File.Exists(destination);

        Assert.False(result.Success);        
        Assert.False(copied);        
    }

    [Fact]
    public async Task CopyFile_ShouldCopy_WhenValidSourceAndDestination()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
            { destination, new MockDirectoryData() },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination);
        
        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.True(sourceExists);
        Assert.True(copied);
    }

    [Fact]
    public async Task CopyFile_ShouldCopySource_WhenValidSourceAndDestination()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
            { destination, new MockDirectoryData() },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.True(sourceExists);
        Assert.True(copied);
    }

    [Fact]
    public async Task CopyFile_ShouldCreateTargetDirectory_WhenNotExists()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.True(sourceExists);
        Assert.True(copied);
    }

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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.False(filesAreBitPerfectMatch);
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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.False(filesAreBitPerfectMatch);
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

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.True(filesAreBitPerfectMatch);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void FilesAreBitPerfectMatch_DoesMatch_WhenCopyOfImage()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(TestDataHelper.Duck, TestDataHelper.DuckCopy);

        Assert.True(filesAreBitPerfectMatch);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void DoesFileMetadataMatch_DoesNotMatch_WhenDifferentImages()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(TestDataHelper.Duck, TestDataHelper.Waterfall);

        Assert.False(metadataMatches);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void DoesFileMetadataMatch_DoesMatch_WhenCopyOfSameImage()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(TestDataHelper.Duck, TestDataHelper.DuckCopy);

        Assert.True(metadataMatches);
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public void GetImageResolution_GetResolutionForTestFile()
    {
        var fileStorageService = new FileStorageService(
           new FileSystem(),
           new Mock<ILogger<FileStorageService>>().Object);

        string resolution = fileStorageService.GetImageResolution(TestDataHelper.Duck);

        Assert.Equal("4096x3072", resolution);
    }
}
