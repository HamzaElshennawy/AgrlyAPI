using AgrlyAPI.Models.Users;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddOpenApi();
builder.Services.AddRateLimiter( options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.AddPolicy( "fixed", httpContext =>
	RateLimitPartition.GetFixedWindowLimiter(
		partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
		factory: _ => new FixedWindowRateLimiterOptions
		{
			PermitLimit = 50,
			Window = TimeSpan.FromMinutes( 1 ),
		}));

});


builder.Services.AddScoped<Supabase.Client>( _ =>
	new Supabase.Client( builder.Configuration["SupabaseUrl"]!,
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
		ValidAudience = builder.Configuration["JwtConfig:Audience"],
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
	// redirect to swagger in development mode only.
	// TODO: implement the base url while in production mode to handle the request of the base url.
	app.MapGet( "/", () => Results.Redirect( "http://localhost:5258/scalar" ) );
	app.MapOpenApi();
	//app.UseSwagger();
	//app.UseSwaggerUI();
	app.MapScalarApiReference( options =>
	{
		options.WithTheme( ScalarTheme.BluePlanet )
			.WithDefaultHttpClient( ScalarTarget.CSharp, ScalarClient.Http );
	} );
}




app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
