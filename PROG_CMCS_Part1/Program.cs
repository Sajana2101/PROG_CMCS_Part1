using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<FileEncryptionService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await CreateHRRole(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
async Task CreateHRRole(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("HR"))
        await roleManager.CreateAsync(new IdentityRole("HR"));

    var hrUser = await userManager.FindByEmailAsync("hr@system.com");

    if (hrUser == null)
    {
        hrUser = new ApplicationUser
        {
            UserName = "hr@example.com",
            Email = "hr@example.com",
            FirstName = "HR",
            LastName = "Admin"
        };

        await userManager.CreateAsync(hrUser, "@Admin1234");
        await userManager.AddToRoleAsync(hrUser, "HR");
    }
}