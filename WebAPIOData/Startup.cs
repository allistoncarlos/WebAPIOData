using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Serialization;

namespace WebAPIOData
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
            services.AddOData();
            services.AddSingleton(sp => new ODataUriResolver { EnableCaseInsensitive = true });

            services.AddMvc().AddJsonOptions(opt =>
            {
                if (opt.SerializerSettings.ContractResolver != null)
                {
                    var resolver = opt.SerializerSettings.ContractResolver as DefaultContractResolver;
                    resolver.NamingStrategy = null;
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc(ConfigureODataRoutes);
        }

        private static void ConfigureODataRoutes(IRouteBuilder routes)
        {
            var model = GetEdmModel();
            routes.MapODataServiceRoute("ODataRoute", "odata", model);
            routes.Filter(QueryOptionSetting.Allowed);
            routes.OrderBy();
            routes.Count();
            routes.Select();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var games = builder.EntitySet<Game>("Games").EntityType;
            games.ComplexProperty(y => y.Platform);
            
            return builder.GetEdmModel();
        }
    }
}
