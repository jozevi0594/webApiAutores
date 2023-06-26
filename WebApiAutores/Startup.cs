using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filters;
using WebApiAutores.Middleware;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

[assembly:ApiConventionType(typeof(DefaultApiConventions))]//Documentar por default a nivel de API
namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration) 
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();//limpia el mapeo de los tipos de claims para poder traer el email con su nombre EMAIL
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) 
        {

            services.AddControllers(option =>
            {
                option.Filters.Add(typeof(ExceptionFilter));//filtro de excepcion
                option.Conventions.Add(new SwaggerAgrupaPorVersion());
            }).AddJsonOptions(x=>
            x.JsonSerializerOptions.ReferenceHandler=ReferenceHandler.IgnoreCycles)//ignora referencias ciclicas muchos a muchos
            .AddNewtonsoftJson();//agrega el nuget newton soft para usar JsonpatchDocument
            
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"))
                );//configurar el dbcontext para la base de datos

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)//uso de JsonWebToken Bearer
                .AddJwtBearer(opciones=> opciones.TokenValidationParameters=new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey=new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew=TimeSpan.Zero
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>     //Configuracion de swagger 
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebApiAutores",
                    Version = "v1",
                    Description = "web api v1"
                    ,
                    Contact = new OpenApiContact
                    {
                        Email="jozevi0594@gmail.com",
                        Name="Jorge Zegarra",
                        Url= new Uri("https://www.linkedin.com/in/jorge-zegarra-villa-21860211a")
                    },
                    License= new OpenApiLicense
                    {
                        Name = "License",
                    }
                });//doc version 1
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebApiAutores", Version = "v2" });//doc version 2
                c.OperationFilter<AgregarParametroHATEOAS>();//filtro URL Version
                c.OperationFilter<AgregarParametroXVersion>(); //Filtro cabecera
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme //para que acepte los JWT
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In=ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{} 
                    }
                });
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";//activar comentarios XML tambien se coloca en el .csproj del proyecto
                var rutaXML=Path.Combine(AppContext.BaseDirectory, archivoXML);//el ejemplo esta en el delete de autor V1
                c.IncludeXmlComments(rutaXML);
            });
            services.AddAutoMapper(typeof(Startup));//configurando el mapper
            services.AddIdentity<IdentityUser,IdentityRole>() // agrega el servicio de Identity para el manejo de autenticacion 
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
                opciones.AddPolicy("EsVendedor", politica => politica.RequireClaim("esVendedor"));
            });

            services.AddDataProtection(); //activar servicio de proteccion de datos "encriptacion"

            services.AddTransient<HashService>();

            services.AddCors(opciones =>  // activar CORS en web api que te permite ejecutar la aplicacion para webs especificas(solo navegadores web)
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader()//permiso para que el front ejecute mi web api
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros" });//mostrar la paginacion
                });
            });
            services.AddTransient<GeneradorEnlaces>();// creacion de una nueva instancia para este servicio debido al AddTransient
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();// creacion de una unica instancia para este servicio debido al AddSingleton 

            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectionString"]);
        }

        public void Configure(IApplicationBuilder app,IWebHostEnvironment env)
        {

            app.UseLoggerResponseHTTP();

                   

            if (env.IsDevelopment())//usamos la interfaz de swagger solo para development
            {
               app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();//sacamos la conf de swagger para poder verlo en produccion
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiAutores v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebApiAutores v2");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(); //Activar Cors en web api

            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
