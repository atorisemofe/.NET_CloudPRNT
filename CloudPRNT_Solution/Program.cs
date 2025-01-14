using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using CloudPRNT_Solution.Controllers;
using CloudPRNT_Solution;
using Microsoft.AspNetCore.SignalR;
using CloudPRNT_Solution.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add SignalR services
builder.Services.AddSignalR();

builder.Services.AddDbContext<PrintQueueContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PrintQueueContext") ?? throw new InvalidOperationException("Connection string 'PrintQueueContext' not found.")));

builder.Services.AddDbContext<DeviceTableContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DeviceTableContext") ?? throw new InvalidOperationException("Connection string 'DeviceTableContext' not found.")));

builder.Services.AddDbContext<LocationTableContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LocationTableContext") ?? throw new InvalidOperationException("Connection string 'LocationTableContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MqttMessageRecieved>(); // Add this line
// builder.Services.AddSingleton<IHostedService, PollingScheduler>();

var app = builder.Build();

// Use SignalR
// app.UseSignalR(routes =>
// {
//     routes.MapHub<NotificationHub>("/notificationHub");
// });

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    SeedData.Initialize(services);
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure the Client_Subscribe with the service provider
Client_Subscribe.Configure(app.Services);

// Start the MQTT client connection when the application starts
await Client_Subscribe.Connect_Client();

app.MapHub<NotificationHub>("/notificationHub"); // Use SignalR



app.Run();

