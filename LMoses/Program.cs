using Microsoft.EntityFrameworkCore;
using LMoses.Data;


namespace LMoses
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVercelAndLocal", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5173", // local dev
                            "https://lmoses-git-master-holycrusad3rs-projects.vercel.app" // deployed frontend
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
                options.AddPolicy("AllowVercel", policy =>
                {
                    policy.WithOrigins(
                            "https://lmoses-git-master-holycrusad3rs-projects.vercel.app" // deployed frontend
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "clicks.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));


            var app = builder.Build();

            app.UseCors("AllowVercel");
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate(); // Applies any pending migrations
            }
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
        }
    }
}
