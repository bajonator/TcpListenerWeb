using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TcpListenerWeb.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(11000); 
});

var app = builder.Build();
app.UsePathBase("/TcpListener");
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();