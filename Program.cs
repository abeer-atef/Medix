using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.Hubs;
using Midix.IRepository;
using Midix.IRepository.IAdmin;
using Midix.IRepository.IDoctor;
using Midix.IRepository.IPatient;
using Midix.IRepository.IRating;
using Midix.Models;
using Midix.Repository;
using Midix.Repository.AdminRepositories;
using Midix.Repository.DoctorRepositories;
using Midix.Repository.PatientRepositories;
using Midix.Repository.RatingRepositories;
using Midix.Services;
using Stripe;
using System;

namespace Midix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
                    ));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.AddSignalR();

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();

            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();


            builder.Services.AddScoped<IPatientOverviewRepository, PatientOverviewRepository>();
            builder.Services.AddScoped<IPatientBookingRepository, PatientBookingRepository>();
            builder.Services.AddScoped<IPatientAppointmentsRepository, PatientAppointmentsRepository>();
            builder.Services.AddScoped<IPatientPrescriptionsRepository, PatientPrescriptionsRepository>();
            builder.Services.AddScoped<IPatientProfileRepository, PatientProfileRepository>();
            builder.Services.AddScoped<IRatingRepository, RatingRepository>();

            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorOverviewRepository, DoctorOverviewRepository>();
            builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
            builder.Services.AddScoped<IDoctorPatientsRepository, DoctorPatientsRepository>();
            builder.Services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
            builder.Services.AddScoped<IDoctorPrescriptionRepository, DoctorPrescriptionRepository>();

            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAdminOverviewRepository, AdminOverviewRepository>();
            builder.Services.AddScoped<IAdminStaffRepository, AdminStaffRepository>();
            builder.Services.AddScoped<IAdminAppointmentsRepository, AdminAppointmentsRepository>();
            builder.Services.AddScoped<IAdminFinancialRepository, AdminFinancialRepository>();
            builder.Services.AddScoped<IAdminDoctorManagementRepository, AdminDoctorManagementRepository>();
            builder.Services.AddScoped<IAdminUsersRepository, AdminUsersRepository>();

            // ── AI Prescription Service ──────────────────────────────────────────
            // Named HttpClient for GroqVisionService; IHttpClientFactory is registered
            // automatically by AddHttpClient and is safe for long-lived consumption.
            builder.Services.AddHttpClient(nameof(GroqVisionService));
            builder.Services.AddScoped<IPrescriptionAiService, GroqVisionService>();


            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithRedirects("/Home/BadUrl");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<AppointmentHub>("/appointmentHub");

            // Seed Admin & Specializations
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                string[] roles = { "Admin", "Doctor", "Patient" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                        roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }


                var adminEmail = "admin@medix.com";
                if (userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Super",
                        LastName = "Admin",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(admin, "Admin@123").GetAwaiter().GetResult();
                    if (result.Succeeded)
                        userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                }

                var specializations = new[] { "Cardiology", "Neurology", "Dentistry", "Orthopedics", "Pediatrics", "Dermatology", "General Practice" };
                foreach (var specName in specializations)
                {
                    if (!dbContext.Specializations.Any(s => s.Name == specName))
                        dbContext.Specializations.Add(new Specialization { Name = specName });
                }
                dbContext.SaveChanges();
            }

            app.Run();
        }
    }
}