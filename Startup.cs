using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FirebaseAdmin;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Api.Web.Dynamics365.Models.Kudu;
using Api.Web.Dynamics365.Servicios.Kudu;

namespace Api.Web.Dynamics365
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        FirebaseApp app = FirebaseApp.Create(new AppOptions()
        {
            Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("serviceMegaAccountKey.json"),
        });

        public IConfiguration Configuration { get; }

        public void configureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        //ValidIssuer = Configuration["Jwt:Issuer"],
                        //ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                        ClockSkew = TimeSpan.Zero
                    };
                }); //Configurando JWT

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddMvc();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.Configure<KuduOptions>(Configuration.GetSection("Kudu"));
            services.AddMemoryCache(); // Borrar si no se usa
            services.AddHttpClient();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                }); //Agregando jwt a swagger

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                         new OpenApiSecurityScheme
                        {
                             Reference =   new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                        },
                         new string[]{ }
                    }
                }); //Agregando jwt a swagger
            });
            services.AddHttpContextAccessor();

            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 1024; //default 1024
                options.ValueLengthLimit = int.MaxValue; //not recommended value
                options.MultipartBodyLengthLimit = long.MaxValue; //not recommended value
                options.MemoryBufferThreshold = Int32.MaxValue;
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders(); // Utilizar Identity de net core para el manejo nativo de usuarios contra la bd

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            }); //Agregar politica de "roles" en los claims

            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            }); // Configurar CORS para aplicaciones web que quieran consumir la api. Mas adelante podemos usar la opcion
                // WithOrigins para setearles todas las webs(Ej: Portales) que pueden consumir la api y asi tenes mas seguridad para que
                // ningun script de otra web pueda ser ejecutado.

            services.AddTransient<HashService>();//Registro del servicio para hashear

            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddSingleton(provider =>
            {
                var firestoreBuilder = new FirestoreDbBuilder
                {
                    ProjectId = "soporte-megatlon-6829a", // ⚠️ cambiar por tu projectId de Firebase
                    Credential = GoogleCredential.FromFile("serviceMegaAccountKey.json")
                };
                return firestoreBuilder.Build();
            });
            services.AddSingleton<IKuduHttpClientFactory, KuduHttpClientFactory>();
            services.AddSingleton<IWebJobCurrentExecutionService, WebJobCurrentExecutionService>();

            //BLOQUEAR PETICIONES HTTP EN EL SERVIDOR DE AZURE!!! SOLO VA A PERMITIR HTTPS

            services.AddScoped<IErrorLogService, ErrorLogService>();
        }

        public async void Configure( IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Middlewares
            //MyStaticHelperClass.SetHttpContextAccessor(accessor);

            //if (env.IsDevelopment())
            //{
                
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
