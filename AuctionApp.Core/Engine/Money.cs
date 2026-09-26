using System.Globalization;

namespace AuctionApp.Core.Engine;

/// <summary>Prices and budgets are expressed in millions with one decimal (0.1 steps).</summary>
public static class Money
{
    public const decimal Step = 0.1m;

    /// <summary>The highest amount anywhere: a budget, a bid, a price. Nothing in a draft cup goes past this.</summary>
    public const decimal Max = 30m;

    /// <summary>The smallest budget a captain can have.</summary>
    public const decimal MinBudget = 0.1m;

    /// <summary>Whether an amount typed for a budget is acceptable: 0.1 to 30.0, in steps of 0.1.</summary>
    public static bool IsValidBudget(decimal amount) => amount is >= MinBudget and <= Max && IsWholeStep(amount);

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
