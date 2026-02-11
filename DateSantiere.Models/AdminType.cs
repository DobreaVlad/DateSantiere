namespace DateSantiere.Models;

public static class AdminType
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";
    public const string Support = "Support";
    
    public static readonly Dictionary<string, AdminPermissions> Permissions = new()
    {
        { SuperAdmin, new AdminPermissions 
        { 
            CanManageUsers = true,
            CanManageAdmins = true,
            CanManageSantiere = true,
            CanDeleteSantiere = true,
            CanManagePayments = true,
            CanViewReports = true,
            CanExportData = true,
            CanManageSettings = true,
            CanViewLogs = true
        }},
        { Admin, new AdminPermissions 
        { 
            CanManageUsers = true,
            CanManageAdmins = false,
            CanManageSantiere = true,
            CanDeleteSantiere = true,
            CanManagePayments = true,
            CanViewReports = true,
            CanExportData = true,
            CanManageSettings = false,
            CanViewLogs = true
        }},
        { Moderator, new AdminPermissions 
        { 
            CanManageUsers = false,
            CanManageAdmins = false,
            CanManageSantiere = true,
            CanDeleteSantiere = false,
            CanManagePayments = false,
            CanViewReports = true,
            CanExportData = true,
            CanManageSettings = false,
            CanViewLogs = false
        }},
        { Support, new AdminPermissions 
        { 
            CanManageUsers = false,
            CanManageAdmins = false,
            CanManageSantiere = false,
            CanDeleteSantiere = false,
            CanManagePayments = false,
            CanViewReports = false,
            CanExportData = false,
            CanManageSettings = false,
            CanViewLogs = false
        }}
    };
    
    public static AdminPermissions GetPermissions(string? adminType)
    {
        if (string.IsNullOrEmpty(adminType))
            return new AdminPermissions(); // No admin permissions
            
        return Permissions.TryGetValue(adminType, out var permissions) 
            ? permissions 
            : new AdminPermissions();
    }
    
    public static bool IsAdmin(string? adminType)
    {
        return !string.IsNullOrEmpty(adminType);
    }
}

public class AdminPermissions
{
    public bool CanManageUsers { get; set; }
    public bool CanManageAdmins { get; set; }
    public bool CanManageSantiere { get; set; }
    public bool CanDeleteSantiere { get; set; }
    public bool CanManagePayments { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanExportData { get; set; }
    public bool CanManageSettings { get; set; }
    public bool CanViewLogs { get; set; }
}
