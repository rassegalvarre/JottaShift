using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests;

internal static class TestData
{
    private static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    public static readonly string Duck = Path.Combine(TestDataPath, "duck.jpg");
    public static readonly string DuckCopy = Path.Combine(TestDataPath, "duck_copy.jpg");
    public static readonly string Waterfall = Path.Combine(TestDataPath, "waterfall.jpg");
}
