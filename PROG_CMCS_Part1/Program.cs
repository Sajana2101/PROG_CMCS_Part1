using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Add session support
builder.Services.AddSession(options =>
{
    // Session timeout
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Add custom file encryption service
builder.Services.AddScoped<FileEncryptionService>();
// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configure Identity with custom ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();
// Ensure database is created and roles exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    // Create DB if not exists
    db.Database.EnsureCreated();
    // Create default roles
    await EnsureRoles(services);
    // Seed default HR user
    await CreateHRRole(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Enable Identity auth
app.UseAuthentication();
app.UseAuthorization();
// Enable session
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
// Create default roles
async Task EnsureRoles(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "HR", "Lecturer", "Coordinator", "Manager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
// Seed default HR user
async Task CreateHRRole(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("HR"))
        await roleManager.CreateAsync(new IdentityRole("HR"));

    var hrUser = await userManager.FindByEmailAsync("hr@example.com");

    if (hrUser == null)
    {
        hrUser = new ApplicationUser
        {
            UserName = "hr@example.com",
            Email = "hr@example.com",
            FirstName = "HR",
            LastName = "Admin"
        };
        // Set password
        await userManager.CreateAsync(hrUser, "@Admin1234");
        // Assign HR role
        await userManager.AddToRoleAsync(hrUser, "HR");
    }
}
