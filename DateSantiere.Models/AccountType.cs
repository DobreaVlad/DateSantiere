namespace DateSantiere.Models;

public static class AccountType
{
    public const string Free = "Free";
    public const string Basic = "Basic";
    public const string Premium = "Premium";
    public const string Enterprise = "Enterprise";
    
    public static readonly Dictionary<string, AccountLimits> Limits = new()
    {
        { Free, new AccountLimits { SearchLimit = 10, ExportLimit = 0, CanExportData = false, CanSaveSearches = false, MaxSavedSearches = 0 } },
        { Basic, new AccountLimits { SearchLimit = 100, ExportLimit = 10, CanExportData = true, CanSaveSearches = true, MaxSavedSearches = 5 } },
        { Premium, new AccountLimits { SearchLimit = 500, ExportLimit = 50, CanExportData = true, CanSaveSearches = true, MaxSavedSearches = 20 } },
        { Enterprise, new AccountLimits { SearchLimit = -1, ExportLimit = -1, CanExportData = true, CanSaveSearches = true, MaxSavedSearches = -1 } } // -1 = Unlimited
    };
    
    public static AccountLimits GetLimits(string accountType)
    {
        return Limits.TryGetValue(accountType, out var limits) ? limits : Limits[Free];
    }
}

public class AccountLimits
{
    public int SearchLimit { get; set; }
    public int ExportLimit { get; set; }
    public bool CanExportData { get; set; }
    public bool CanSaveSearches { get; set; }
    public int MaxSavedSearches { get; set; }
    
    public bool IsUnlimitedSearches => SearchLimit == -1;
    public bool IsUnlimitedExports => ExportLimit == -1;
    public bool IsUnlimitedSavedSearches => MaxSavedSearches == -1;
}
