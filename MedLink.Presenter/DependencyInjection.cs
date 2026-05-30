using MedLink.Presenter.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.Presenter;

public static class DependencyInjection
{
    public static IServiceCollection AddPresenterLayer(this IServiceCollection services)
    {
        services.AddScoped<DoctorListPresenter>();
        services.AddScoped<AppointmentPresenter>();
        services.AddScoped<AdminDashboardPresenter>();
        services.AddScoped<AdminDoctorPresenter>();
        services.AddScoped<AdminAppointmentPresenter>();
        services.AddScoped<ReviewPresenter>();
        services.AddScoped<IEmailSender, BrevoEmailSender>();
        return services;
    }
}