using AgrlyAPI.Models.User;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<Supabase.Client>( _ =>
	new Supabase.Client( builder.Configuration["SupabaseUrl"],
	builder.Configuration["SupabaseKey"], new Supabase.SupabaseOptions
	{
		AutoRefreshToken = true,
		AutoConnectRealtime = true,
	} ) );

builder.Services.AddAuthentication( options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
} ).AddJwtBearer( options =>
{
	options.RequireHttpsMetadata = true;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
		ValidAudience = builder.Configuration["JwtConfig:Audiance"],
		IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( builder.Configuration["JwtConfig:Key"]! ) ),
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true
	};
} );

builder.Services.AddAuthorization();

builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();

app.Run();
