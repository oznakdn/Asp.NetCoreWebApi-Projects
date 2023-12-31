using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BookStore.WebApi.DBOperations;
using BookStore.WebApi.Middlewares;
using BookStore.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace BookStore.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Authentication servisinin eklenmesi
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt=>{
                opt.TokenValidationParameters=new TokenValidationParameters{
                    ValidateAudience=true,
                    ValidateIssuer=true,
                    ValidateLifetime=true,
                    ValidateIssuerSigningKey=true,
                    ValidIssuer=Configuration["Token:Issuer"],
                    ValidAudience=Configuration["Token:Audience"],
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:SecurityKey"])),
                    ClockSkew=TimeSpan.Zero
                };
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookStore.WebApi", Version = "v1" });
            });

            services.AddDbContext<BookStoreDbContext>(options=>options.UseInMemoryDatabase("BookStoreDB"));
            //services.AddScoped<IBookStoreDbContext>(provider=>provider.GetService<BookStoreDbContext>());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddSingleton<ILoggerService,ConsoleLogger>();
            services.AddSingleton<ILoggerService,DbLogger>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore.WebApi v1"));
            }


            // Authentication, Authorization dan önce tanımlanmalıdır.
            app.UseAuthentication();


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // Our middleware
            app.UseCustomExceptionMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
