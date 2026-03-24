using JottaShift.Core;

namespace JottaShift.Tests;

public static class ResultAssert
{
    public static void Success(Result result)
    {
        Assert.True(result.Succeeded, $"Expected a successfull result");

        Assert.True(
            string.IsNullOrEmpty(result.ErrorMessage),
            $"Result is successfull but contains an error message");
    }

    public static void Failure(Result result)
    {
        Assert.False(result.Succeeded, $"Expected a failed result");

        Assert.False(
            string.IsNullOrEmpty(result.ErrorMessage),
            $"Result failed but does not contain an error message");
    }

    public static void ValueSuccess<T>(Result<T> result, T expectedValue)
    {
        Success(result);

        Assert.NotNull(result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    public static void ValueFailure<T>(Result<T> result)
    {
        Failure(result);

        if (result.Value is not null)
        {
            Assert.Equal(default, result.Value);
        }
        else
        {
            Assert.Null(result.Value);
        }
    }
}
