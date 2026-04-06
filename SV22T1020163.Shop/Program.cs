using SV22T1020163.Shop;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020163.Shop";
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7);
                    option.SlidingExpiration = true;
                    option.Cookie.HttpOnly = true;
                    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

var mediaRoot = MediaPaths.ResolveRoot(app.Environment, app.Configuration);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(mediaRoot, "products")),
    RequestPath = "/anh_chung/products"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(mediaRoot, "employees")),
    RequestPath = "/anh_chung/employees"
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

ApplicationContext.Configure(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");
SV22T1020163.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();
