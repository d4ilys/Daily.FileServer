using FileServer.Controllers;
using FileServer.Models;
using FileServer;
using FreeSql;
using Masuit.Tools.Core.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args).Inject();

builder.Services.AddHeatConfig();

//是否开启页面访问，用于测试
if (FileStaticConfiguration.IsShowViews)
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllers();
}

builder.Services.AddHttpContextAccessor();

builder.Services.AddResumeFileResult();

//是否开启鉴权
if (FileStaticConfiguration.IsValid)
{
    builder.Services.AddJwt<JwtHandler>(enableGlobalAuthorize: true);
}

builder.Services.AddCors(delegate (CorsOptions options)
{
    options.AddPolicy("CorsPolicy", delegate (CorsPolicyBuilder builder)
    {
        builder.SetIsOriginAllowed((string x) => true).AllowAnyOrigin().AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FileServer",
        Version = "v1"
    });
});
var str = ConfigHelper.GetAppSetting("ConnectionStrings");

IFreeSql db = new FreeSqlBuilder()
    .UseConnectionString(DataType.Sqlite, str)
    .Build();

db.CodeFirst.SyncStructure<ResourcesInfo>();

builder.Services.AddSingleton<IFreeSql>(db);

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.UseInject(string.Empty);

app.MapControllers();

app.Run();