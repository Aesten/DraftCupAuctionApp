using System.Globalization;

namespace AuctionApp.Core.Engine;

/// <summary>Prices and budgets are expressed in millions with one decimal (0.1 steps).</summary>
public static class Money
{
    public const decimal Step = 0.1m;

    public static string Format(decimal amount, IFormatProvider? culture = null) =>
        amount.ToString("0.0", culture ?? CultureInfo.CurrentCulture);

    /// <summary>Parses an amount typed by a user. Both "2.5" and "2,5" are accepted whatever the system language.</summary>
    public static bool TryParse(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount);
    }

    public static bool IsWholeStep(decimal amount) => decimal.Round(amount, 1) == amount;
}
