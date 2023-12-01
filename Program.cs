using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<UserDataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("UserDataContext") ?? throw new InvalidOperationException("Connection string 'UserDataContext' not found.")));
builder.Services.AddDbContext<ContestContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContestContext") ?? throw new InvalidOperationException("Connection string 'ContestContext' not found.")));
builder.Services.AddDbContext<ContestBookmarkContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContestBookmarkContext") ?? throw new InvalidOperationException("Connection string 'ContestBookmarkContext' not found.")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddScssBundle("/css/danger.css", "/css/danger.scss");
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    // options.Cookie.IsEssential = true;
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseWebOptimizer();


app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
