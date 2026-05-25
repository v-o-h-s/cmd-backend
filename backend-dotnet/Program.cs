using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Controllers;
using ClinicBackend.Api.Infrastructure;
using ClinicBackend.Api.MedicalFiles;
using ClinicBackend.Api.Patients;
using ClinicBackend.Api.Staff;
using ClinicBackend.Api.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddHttpClient();

builder.Services.AddScoped<SupabaseAuthClient>();
builder.Services.AddScoped<SupabaseRestClient>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<ReceptionistService>();
builder.Services.AddScoped<MedicalFileService>();
builder.Services.AddScoped<PatientService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapGet("/", () => "Hello, World!");

app.MapControllers();

app.Run();
