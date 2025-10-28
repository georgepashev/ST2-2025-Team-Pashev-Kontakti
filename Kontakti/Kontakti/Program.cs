using Kontakti.Services;

namespace Kontakti
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1) Опции за LLM
            builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));
            // 2) HttpClient за LlmClient
            builder.Services.AddHttpClient<LlmClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var opt =
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LlmOptions>>().Value;
                http.BaseAddress = new Uri(opt.BaseUrl);
                http.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
