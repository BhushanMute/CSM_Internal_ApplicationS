using Blazored.LocalStorage;
using CSMTutorial.Auth;
using CSMTutorial.Components;
using CSMTutorial.Data;
using CSMTutorial.Data.Repositories;
using CSMTutorial.Models;
using CSMTutorial.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using MudBlazor.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURATION SETTINGS
// ==========================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// ==========================================
// BLAZORED LOCAL STORAGE
// ==========================================
builder.Services.AddBlazoredLocalStorage();

// ==========================================
// DATABASE
// ==========================================
builder.Services.AddSingleton<DapperContext>();

// ==========================================
// REPOSITORIES
// ==========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IWhatsAppSettingsService, WhatsAppSettingsService>();

// ==========================================
// SERVICES
// ==========================================
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<JsInteropService>();
builder.Services.AddScoped<ITeacherAbsenceService, TeacherAbsenceService>();
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
// ==========================================
// HTTP CLIENT
// ==========================================
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ==========================================
// BACKGROUND SERVICES
// ==========================================
builder.Services.AddHostedService<WhatsAppBackgroundService>();

// ==========================================
// AUTHENTICATION STATE PROVIDER
// ==========================================
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

// ==========================================
// AUTHORIZATION
// ==========================================
builder.Services.AddAuthorizationCore();

// ==========================================
// MUDBLAZOR SERVICES
// ==========================================
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// ==========================================
// JWT AUTHENTICATION
// ==========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Set to true in production
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };

        // Events for debugging (remove in production)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    throw new InvalidOperationException("JWT Settings are not configured properly in appsettings.json");
}

// ==========================================
// BLAZOR COMPONENTS
// ==========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ==========================================
// LOGGING
// ==========================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft", LogLevel.Information);
    builder.Logging.AddFilter("System", LogLevel.Information);
}

// ==========================================
// BUILD APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// MIDDLEWARE PIPELINE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Authentication & Authorization (order matters!)
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// ROUTING
// ==========================================
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ==========================================
// RUN APPLICATION
// ==========================================
app.Run();