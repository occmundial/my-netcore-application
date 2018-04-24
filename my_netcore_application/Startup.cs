using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;
using AutoMapper;
using my_netcore_application.Interfaces;
using my_netcore_application.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

// https://stackoverflow.com/questions/43053495/how-to-set-credentials-on-aws-sdk-on-net-core
/*  appsettings.Development.json:
        "AWS": {
            "Profile": "local-test-profile",  
            "Region": "us-west-2"
        },  
    ---------------------------------------------------------------------- 
        Credentials file should be in C:\Users\{USERNAME}\.aws\credentials

        [local-test-profile]
        aws_access_key_id = your_access_key_id
        aws_secret_access_key = your_secret_access_key        
*/

// http://dotnetliberty.com/index.php/2016/09/12/aws-dynamodb-on-net-core-local-dynamodb-server-for-development-part-1/
// http://dotnetliberty.com/index.php/2016/09/19/aws-dynamodb-on-net-core-getting-started/
namespace my_netcore_application
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
            services.AddMvc();

            // Toma la configuración de AWS del archivo "appsettings.{env.EnvironmentName}.json"
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();        // Include="AWSSDK.S3" Version="3.3.17.2"
            // registra el contrato "IAmazonDynamoDB" que luego es usado en el repositorio "FavoriteRepository"
            services.AddAWSService<IAmazonDynamoDB>();           
            
            // esta línea registra el AutoMapper para que pueda ser asignado como default
            services.AddAutoMapper();

            // registramos el contrato "IFavoriteRepository" y su implementación "FavoriteRepository"
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();   

            // registramos el contrato "IUrlHelper" para poder generar urls absolutas y comenzar a utilizar el HATEOAS
            // https://offering.solutions/blog/articles/2017/11/29/crud-operations-angular-with-aspnetcore-hateoas/
            // https://github.com/FabianGosebrink/ASPNETCore-Angular-Material-HATEOAS-Paging
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });                     
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
