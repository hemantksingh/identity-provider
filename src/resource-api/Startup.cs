using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace resource_api
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
			const string authenticationScheme = "JwtBearer";

			services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = authenticationScheme;
					options.DefaultChallengeScheme = authenticationScheme;
				})
				.AddJwtBearer(authenticationScheme, jwtBearerOptions =>
					{
						jwtBearerOptions.Authority = "https://localhost:44376/";
						jwtBearerOptions.RequireHttpsMetadata = true;
						jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
						{
							ValidateIssuer = true,
							ValidIssuer = "https://localhost:44376/",
							ValidAudience = "resourceapi",
							ValidateAudience = true,
							ValidateLifetime = true
						};
					}
				);

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseMvc();
		}
	}
}