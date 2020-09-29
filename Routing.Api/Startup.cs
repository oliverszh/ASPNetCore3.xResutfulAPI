using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Routing.Api.Data;
using Routing.Api.Services;

namespace Routing.Api
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
            services.AddControllers(setup => {
                setup.ReturnHttpNotAcceptable = true;
                //setup.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                //setup.OutputFormatters.Insert(0, new XmlDataContractSerializerOutputFormatter());
            }).AddNewtonsoftJson(setup =>
              {
                  setup.SerializerSettings.ContractResolver = 
                  new CamelCasePropertyNamesContractResolver();
              })
              .AddXmlDataContractSerializerFormatters()
              .ConfigureApiBehaviorOptions(setup=> {
                  setup.InvalidModelStateResponseFactory = context =>
                  {
                      var problemDetails = new ValidationProblemDetails(context.ModelState)
                      {
                          Type = "http://www.baidu.com",
                          Title = "有错误!!",
                          Status = StatusCodes.Status422UnprocessableEntity,
                          Detail="请查看详细信息",
                          Instance=context.HttpContext.Request.Path
                      };

                      problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                      return new UnprocessableEntityObjectResult(problemDetails)
                      {
                          ContentTypes = { "application/problem+json" }
                      };
                  };
              });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<ICompanyRepository, CompanyRepository>();//注册服务
            services.AddDbContext<RoutingDbContext>(option =>
            {
                option.UseSqlite("Data Source=routing.db");
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder => {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error!");
                    });
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
