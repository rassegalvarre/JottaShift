using JottaShift.Core.FileExport.Jobs;

namespace JottaShift.Tests.FileExport;

internal static class FileTransferResultAssert
{
    public static void SuccessfullJob(FileTransferJobResult jobResult)
    {
        ResultAssert.Success(jobResult);
        Assert.NotNull(jobResult.Value);

        if (jobResult.Status == FileTransferJobResultStatus.NoFilesToTransfer)
        {
            Assert.Empty(jobResult.Value);
            return;
        }
        else if (jobResult.Status == FileTransferJobResultStatus.AllFilesTransferredSuccessfully)
        {

            Assert.All(jobResult.Value, transferResult =>
            {
                ResultAssert.Success(transferResult);
                Assert.Equal(FileTransferResultStatus.TransferSucceeded, transferResult.Status);
                Assert.False(string.IsNullOrEmpty(transferResult.SourceFileFullPath));
                Assert.False(string.IsNullOrEmpty(transferResult.NewFileFullPath));
                Assert.True(transferResult.SourceFileDeleted);
            });
        }
        else
        {
            Assert.Fail($"Unexpected status for a successfull result: {jobResult.Status}");
        }
    }

    public static void FailedJob(FileTransferJobResult jobResult)
    {
        ResultAssert.Failure(jobResult);
        Assert.NotNull(jobResult.Value);

        if (jobResult.Status is FileTransferJobResultStatus.InvalidJob)
        {
            Assert.Empty(jobResult.Value);
            return;
        }
        else if (jobResult.Status is FileTransferJobResultStatus.SomeFilesFailedToTransfer)
        {
            Assert.NotEmpty(jobResult.Value);

            bool containsInvalidSource = jobResult.Value.Any(transferResult => transferResult.Status == FileTransferResultStatus.InvalidSourceFile);
            bool containsTransferFailed = jobResult.Value.Any(transferResult => transferResult.Status == FileTransferResultStatus.TransferFailed);
            bool containsNewFileCorrupted = jobResult.Value.Any(transferResult => transferResult.Status == FileTransferResultStatus.NewFileCorrupted);

            Assert.True(containsInvalidSource || containsTransferFailed || containsNewFileCorrupted);
        }
        else
        {
            Assert.Fail($"Unexpected status for a failed result: {jobResult.Status}");
        }
    }

    public static void FailedTransfer(FileTransferResult transferResult)
    {
        ResultAssert.Failure(transferResult);
        Assert.False(string.IsNullOrEmpty(transferResult.SourceFileFullPath));
        Assert.False(transferResult.SourceFileDeleted);

        if (transferResult.Status is FileTransferResultStatus.InvalidSourceFile)
        {
            return;
        }
        else if (transferResult.Status is FileTransferResultStatus.TransferFailed or FileTransferResultStatus.NewFileCorrupted)
        {
            Assert.False(string.IsNullOrEmpty(transferResult.NewFileFullPath));
        }
        else
        {
            Assert.Fail($"Unexpected status for a failed result: {transferResult.Status}");
        }
    }
}
