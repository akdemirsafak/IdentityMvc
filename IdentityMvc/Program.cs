using Identity.Core.OptionModels;
using Identity.Core.Permissions;
using Identity.Repository.ClaimProvider;
using Identity.Repository.Models;
using Identity.Repository.Seeds;
using Identity.Service.Services;
using IdentityMvc.Extensions;
using IdentityMvc.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication().AddFacebook(facebookSettings =>
{
    facebookSettings.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    facebookSettings.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
}).AddGoogle(googleSettings =>
{
    googleSettings.ClientId = builder.Configuration["Authentication:Google:ClientID"];
    googleSettings.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

//////////-----------Identity Started//////////////
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        options =>
        {
            options.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext))!.GetName().Name);
        });
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services
    .AddScoped<IEmailService, EmailService>(); //Soyutladık. Request response'a döndüğünde her seferinde belirlensin.
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>(); //Claim uygulaması için 
builder.Services.AddScoped<IAuthorizationHandler, ExchangeExpireRequirementHandler>(); //Policy based için 
builder.Services.AddScoped<IAuthorizationHandler, ViolenceRequirementHandler>(); //Policy based için
builder.Services.AddScoped<IMemberService, MemberService>(); //Repositories / DbContexts single olmaz DbContext zaten Scoped lifetime'a sahiptir transaction bütünlüğünü sağlar.
builder.Services.AddIdentityWithExtention();
builder.Services.Configure<SecurityStampValidatorOptions>(
    options =>
    {
        options.ValidationInterval =
            TimeSpan.FromMinutes(
                30); //SecurityStamp Varsayılan olarak 30 dakikadır,daha az veya fazla olması durumu performansı etkileyecektir.
        //Veritabanında tutulan securitystamp ile cookiedekini 30 dakikada bir karşılaştırır ve değişiklikleri saptar, gerekirse logout yapıp tekrar giriş yapılmasını ister.
    });
builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookieBuilder = new CookieBuilder();
    cookieBuilder.Name = "IdentityServerLogin";
    opt.LoginPath = new PathString("/Home/Login");
    opt.LogoutPath = new PathString("/Member/Logout"); //Efektif olarak isimlendiriğimiz Logout için yazdık.
    opt.AccessDeniedPath = new PathString("/Member/AccessDenied");
    opt.Cookie = cookieBuilder;
    opt.ExpireTimeSpan = TimeSpan.FromDays(60);
    opt.SlidingExpiration = true; //kullanıcı her giriş yaptığında cookie süresini 60 gün uzatır.
});
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Directory
        .GetCurrentDirectory())); // ! Best practice olarak bu şekilde yaptık ayrıca herhangi bir class'ın constructor'unda IFileProvider ile istediğimiz klasöre erişebiliriz.

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AnkaraPolicy", policy =>
    {
        policy.RequireClaim("City", "Ankara");
        //Burada birden fazla kural tanımlayabiliriz.
    });
    opt.AddPolicy("ExchangePolicy", policy =>
    {
        policy.AddRequirements(new ExchangeExpireRequirement()); //Dynamic Policy
    });
    opt.AddPolicy("ViolencePolicy", policy =>
    {
        policy.AddRequirements(new ViolenceRequirement
        { ThresholdAge = 18 }); //Kullanıcının yaşına göre görebileceği sayfalar
    });
    opt.AddPolicy("OrderPermissionForReadOrDelete", policy =>
    {
        policy.RequireClaim("Permission", Permission.Order.Read);
        policy.RequireClaim("Permission", Permission.Order.Delete);
        policy.RequireClaim("Permission", Permission.Stock.Delete);
    });
    opt.AddPolicy("Permission.Order.Read", policy =>
    {
        policy.RequireClaim("Permission", Permission.Order.Read);
    });
    opt.AddPolicy("Permission.Order.Delete", policy =>
    {
        policy.RequireClaim("Permission", Permission.Order.Delete);
    });
    opt.AddPolicy("Permission.Stock.Delete", policy =>
    {
        policy.RequireClaim("Permission", Permission.Stock.Delete);
    });

});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    await PermissionSeed.Seed(roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//SecurityStamp ve ConcurrencyStamp arasındaki fark SecurityStamp'in otomatik çıkış yapmakla ilgili ConcurrencyStamp ise eş zamanlı problemi çözmek için vardır ve identity e özgü değildir.
//SecurityStamp her zaman güncellenmez.Kullanıcının hassas bilgilerinde değişiklik yaptığımızda kendimiz çağırırız. ConcurrencyStamp ise her seferinde güncellenir. 

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //Authentication için. kimliği doğrular


app.UseAuthorization(); //yetkilendirme
//Buradaki sıralama önemli önce kimlik doğrulanmalı sonra yetki.

//// NEW AREA ADDED
// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllerRoute(
//         name : "areas",
//         pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//     );
// });

//üstteki .net6 dan önce kullanılan area route'u güncel hali aşağıdaki gibi.
app.MapControllerRoute(
    "areas",
    "{area:exists}/{controller=Home}/{action=Index}/{id?}");
///// NEW AREA ADDED END
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();