using PrintSettings.GraphQL;
using PrintSettings.Data.Services;
using PrintSettings.Data.Settings;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PrintSettings.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PrintSettingsDatabaseSettings>(
    builder.Configuration.GetSection("PrintSettingsDatabase")
);

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? ""))
        };
        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            },
            OnTokenValidated = async context => {
                var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                var userClaim = context?.Principal?.Claims.FirstOrDefault(c => c.Type == "UserId");
                User? user = await userService.GetAsync(userClaim?.Value ?? "", UserService.UserSearchType.Id);
                if (user == null) {
                    context?.Fail("Unauthorized");
                }
                context?.HttpContext.Items.Add("User", user);
            }
        };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("JWT", policy => {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<UserService>();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowSpecificOrigins", 
        builder => builder
            .WithOrigins("http://localhost:5106", "https://localhost:44440")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

builder.Services.AddPrintSettingsServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseGraphQL<ISchema>();

app.MapControllers();
app.MapGraphQL();

app.Run();
