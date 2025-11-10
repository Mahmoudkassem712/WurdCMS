using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AttributeBuilder;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.AspNetCore.Identity.SQLServer;
using Microsoft.AspNetCore.Identity;
using Piranha.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;


var builder = WebApplication.CreateBuilder(args);

var jwtKey = "Vm9K2bTq5Xr1PdN8sL0GhYz4Wc7Aj3Fu6QeR5Mv2Sb8Kp1Zn6Lx0Cr9Ht4Vb3Yq7Pw2Sd5Km8Nj1Ub6Tf0Gh4Rz7Lp3Cx9Vm2Qw5Er8Bn1";
var key = Encoding.ASCII.GetBytes(jwtKey);

// Add JWT Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;  // 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wurd API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});




builder.AddPiranha(options =>
{
    /**
     * This will enable automatic reload of .cshtml
     * without restarting the application. However since
     * this adds a slight overhead it should not be
     * enabled in production.
     */
    options.AddRazorRuntimeCompilation = true;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();

    options.UseEF<Piranha.Data.EF.SQLServer.SQLServerDb>(db =>
        db.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("Wurd")
        ));

    options.UseIdentityWithSeed<IdentitySQLServerDb>(db =>
        db.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseApi();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wurd API v1"));

//using (var scope = app.Services.CreateScope())
//{
//    var api = scope.ServiceProvider.GetRequiredService<IApi>();
//    App.Init(api);
//}


app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/wurd"))
    {
        var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
    }
    await next();
});

app.UseStaticFiles();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


using (var scope = app.Services.CreateScope())
{
    var api = scope.ServiceProvider.GetRequiredService<IApi>();

    // 1. Init Piranha
    App.Init(api);

    // 2. Build Content Types
    new ContentTypeBuilder(api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();
}
EditorConfig.FromFile("editorconfig.json");


app.UseAuthentication();
app.UseAuthorization();

app.UsePiranha(options =>
{

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();

});

app.Run();