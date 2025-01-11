using Discord2.Data;
using Discord2.Hubs;
using Discord2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.ExpireTimeSpan = TimeSpan.FromMicroseconds(2);
//});

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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
    name: "SendMessage",
    pattern: "Channels/SendMessage/{channelId?}/{message?}",
    defaults: new { controller = "Channels", action = "SendMessage" }
);

app.MapControllerRoute(
    name: "ChangeUserRole",
    pattern: "Groups/AssignRole/{groupId?}/{userId?}/{new_role_id?}",
    defaults: new { controller = "Groups", action = "AssignRole" }
);

app.MapControllerRoute(
    name: "AddUserGroup",
    pattern: "Groups/AddMember/{groupId?}/{userId?}",
    defaults: new { controller = "Groups", action = "AddMember" }
);

app.MapControllerRoute(
    name: "RemoveUserGroup",
    pattern: "Groups/RemoveMember/{groupId?}/{userId?}",
    defaults: new { controller = "Groups", action = "RemoveMember" }
);

app.MapControllerRoute(
    name: "AddChannel",
    pattern: "Channels/New/{groupId?}",
    defaults: new { controller = "Channels", action = "New" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

//app.Urls.Add("http://0.0.0.0:80");
app.Run();
