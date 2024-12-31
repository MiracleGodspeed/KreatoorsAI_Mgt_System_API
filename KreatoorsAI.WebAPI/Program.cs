using KreatoorsAI.Data;
using KreatoorsAI.WebAPI.Infrastructures;
using KreatoorsAI.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kreatoors Profile Mgt API's", Version = "v1" });
    c.EnableAnnotations();

    // add Basic Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });

    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "kreatoorsapi.xml"));
});

builder.Services.AddDbContext<KreatoorsDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                                  sqlServerOptionsAction: sqlOptions =>
                                  {
                                      sqlOptions.EnableRetryOnFailure(
                                                  maxRetryCount: 3,
                                                  maxRetryDelay: TimeSpan.FromSeconds(30),
                                                  errorNumbersToAdd: null);
                                  })
                                 , ServiceLifetime.Transient
    );

builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.ConfigureService();
builder.Services.ConfigureEmailService(builder.Configuration);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<KreatoorsDbContext>();
    context.Database.Migrate();

}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
    RequestPath = "/Resources"
});
app.UseRouting();

app.UseCors(
     options => options.SetIsOriginAllowed(x => _ = true)
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()
 );

app.UseMiddleware<JWTMiddleware>();
app.UseMiddleware<DeviceTokenValidationMiddleware>();

app.UseAuthorization();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();
