using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyQuanCaPhe.Utils;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
// Services
// ----------------------------------------------------------------
builder.Services.AddControllersWithViews();

// Session (dùng cho POS cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Login/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOrManager", policy =>
        policy.RequireRole("Employee", "Manager"));

    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("Manager"));
});

var app = builder.Build();

// ----------------------------------------------------------------
// Cấu hình EmailUtil — đọc từ appsettings.json một lần duy nhất
// ----------------------------------------------------------------
EmailUtil.Configure(app.Configuration);

// ----------------------------------------------------------------
// HTTP Pipeline
// ----------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();              // ← Session trước Authentication

app.UseAuthentication();       // ← phải trước UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
