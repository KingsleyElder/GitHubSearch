using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using GitHubTopRepos.Configuration;
using GitHubTopRepos.Repositories;
using GitHubTopRepos.Resilience;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GitHubTopRepos.Data.Entities;
using System;
using Microsoft.AspNetCore.Builder;

namespace GitHubTopRepos
{
    [ExcludeFromCodeCoverage]
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
            var mySqlConnectionStr = Configuration.GetConnectionString("RequestTracking");
            services.AddDbContext<IRequestTrackingContext, RequestTrackingContext>(builder =>
            {
                builder.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr));
            });
            services.AddScoped<IRequestTrackingRepository, RequestTrackingRepository>();
            services.AddScoped<IGitHubRepository, GitHubRepository>();
            
            var executionPolicyOptionConfiguration = Configuration.GetSection("ExecutionPolicyOptions");
            var executionPolicyOptions = new ExecutionPolicyOptions();
            executionPolicyOptionConfiguration.Bind(executionPolicyOptions);
            var executionPolicies = new ExecutionPolicies(new OptionsWrapper<ExecutionPolicyOptions>(executionPolicyOptions));

            services.Configure<ExecutionPolicyOptions>(executionPolicyOptionConfiguration);
            services.AddSingleton<IExecutionPolicies>(executionPolicies);

            services.Configure<GitHubOptions>(option =>
            {
                option.SetConnectionString(Configuration.GetConnectionString("GitHub"));
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddHttpClient("GitHub").AddPolicyHandler(executionPolicies.ApiPolicy);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            logger.LogInformation("GitHub Search up and running");
        }
    }
}
