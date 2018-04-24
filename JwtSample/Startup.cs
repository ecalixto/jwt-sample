using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JwtSample.Middlewares;

namespace JwtSample
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _hostingEnv = env;

            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // JWT CONFIGURATION - START
            var jwtConfiguration = Configuration.GetSection(nameof(JwtBearerOptions));

            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .WithExposedHeaders("jwtsession")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"])),
                    ValidIssuer = jwtConfiguration["Issuer"],
                    ValidAudience = jwtConfiguration["Audience"],
                };
            });

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });

            services.AddSingleton<Services.IAuthorizeService, Services.AuthorizeService>();

            services.AddMvc();

            if (_hostingEnv.IsDevelopment() || _hostingEnv.IsStaging())
            {
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // global policy - assign here or on each controller
            app.UseCors("CorsPolicy");
            app.UseDefaultFiles();
            app.UseAuthentication();

            // Usa um middleware para atualizar o token a cada request
            // evitando expiracao de sessao
            // (UNCOMMENT TO USE)
            // app.UseMiddleware<JwtSessionMiddleware>();

            app.UseMvc();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate");
                    ctx.Context.Response.Headers.Add("Pragma", "no-cache");
                    ctx.Context.Response.Headers.Add("Expires", "0");
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
