using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;
using UserManagement.Data.Configuration;
using UserManagement.Data.Repositories;
using UserManagement.Services;
using UserManagement.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Validators
builder.Services.AddScoped<IValidator<CreateUserDto>, NameValidator>();
builder.Services.AddScoped<IValidator<CreateUserDto>, AgeValidator>();
builder.Services.AddScoped<IValidator<CreateUserDto>, PasswordValidator>();
builder.Services.AddScoped<IValidator<CreateUserDto>, UniquenessValidator>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
