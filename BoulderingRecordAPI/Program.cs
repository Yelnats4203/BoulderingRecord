using System.Text.Json.Serialization;
using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBoulderingRecordDatabase(builder.Configuration);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRecordRepository, RecordRepository>();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.Configure<VideoStorageOptions>(builder.Configuration.GetSection(VideoStorageOptions.SectionName));
builder.Services.AddScoped<IVideoStorageService, LocalVideoStorageService>();

const string FrontendCorsPolicy = "FrontendCorsPolicy";
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
