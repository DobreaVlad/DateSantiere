using DateSantiere.Data;
using DateSantiere.Models;
using DateSantiere.Web.Models;
using DateSantiere.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.LiteDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Keep SQLite support for local dev connection strings; use MySQL in production.
    if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString, b => b.MigrationsAssembly("DateSantiere.Web"));
    }
    else
    {
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            b => b.MigrationsAssembly("DateSantiere.Web"));
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<SantierHistoryService>();
builder.Services.AddScoped<ScriptExecutionService>();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Hangfire with LiteDB storage
// builder.Services.AddHangfire(config => 
//     config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
//         .UseSimpleAssemblyNameTypeSerializer()
//         .UseRecommendedSerializerSettings()
//         .UseLiteDbStorage());

// builder.Services.AddHangfireServer();

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Email sender service (you'll need to implement this)
// builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Configure Hangfire Dashboard
// app.UseHangfireDashboard("/admin/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await SeedData.Initialize(services, userManager, roleManager);
        
        // Seed santiere
        await DateSantiere.Web.Data.SantiereSeedData.SeedSantiere(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
