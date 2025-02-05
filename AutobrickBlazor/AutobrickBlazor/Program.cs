using AutobrickBlazor.Components;
using AutobrickBlazor.Components.Account;
using AutobrickBlazor.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using DataAccess;
using Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AutobrickBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {

            DatabaseDefaultDataset.WipeTables();
            DatabaseDefaultDataset.SeedTables();

            ////////////////////////////////////////////////////////////////
            // Testar pieceDAO - funfa!
            PieceDAO pieceDAO = PieceDAO.GetInstance();
            var pieces = (List<Piece>)pieceDAO.GetAll();
            Console.WriteLine(pieces[1]);

            // Testar userDAO - funfa!
            UserDAO userDAO = UserDAO.GetInstance();
            var users = (List<User>)userDAO.GetAll();
            Console.WriteLine(users[1]);

            // Testar setDAO - funfa!
            SetDAO setDAO = SetDAO.GetInstance();
            var sets = (List<Set>)setDAO.GetAll();
            Console.WriteLine(sets[1]);

            // Testar orderDAO - funfa!
            OrderDAO orderDAO = OrderDAO.GetInstance();
            var orders = (List<Order>)orderDAO.GetAll();
            Console.WriteLine(orders[1]);
            ////////////////////////////////////////////////////////////////

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();


            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "auth_token";
                    options.LoginPath = "/Account/Login";
                    options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
                app.UseMigrationsEndPoint();
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            //app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
