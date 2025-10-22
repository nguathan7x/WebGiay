using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using WebGiayOnline.Models.Cloudinary;
using WebGiayOnline.Models.Momo;
using WebGiayOnline.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
var builder = WebApplication.CreateBuilder(args);

// ?? C?u h�nh c�c d?ch v? b�n ngo�i
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddSingleton<PhotoService>();

builder.Services.AddSingleton<CloudinaryService>();


builder.Services.AddTransient<IEmailSender, GmailEmailSender>();

// ?? C?u h�nh DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ?? Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// ? C?u h�nh ng�n ng? h? tr?

//Th�m d?ch v? ?a ng�n ng?
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//Th�m MVC v� h? tr? ViewLocalizer
builder.Services.AddControllersWithViews()
    .AddViewLocalization()  // B? sung h? tr? IViewLocalizer
    .AddDataAnnotationsLocalization();

// C?u h�nh ?a ng�n ng?
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("vi-VN"),  // Ti?ng Vi?t (m?c ??nh)
        new CultureInfo("en-US"),  // Ti?ng Anh
        new CultureInfo("fr-FR")   // Ti?ng Ph�p (n?u c?n)
    };

    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ?? Razor Pages cho Identity
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "Login");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "Register");
});

// ?? ??ng nh?p Google
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
        options.SaveTokens = true;
    });

var app = builder.Build();

// �p d?ng Localization Middleware
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// ?? Pipeline m?c ??nh
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

// ?? Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
