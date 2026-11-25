using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Roost.Converters;

public static class IntConverters
{
    /// <summary>int == 0  ->  true</summary>
    public static readonly IValueConverter IsZero =
        new FuncValueConverter<int, bool>(n => n == 0);

    /// <summary>int &gt; 0  ->  true</summary>
    public static readonly IValueConverter IsPositive =
        new FuncValueConverter<int, bool>(n => n > 0);
}
