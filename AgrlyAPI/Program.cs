using AgrlyAPI.Models.User;

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
