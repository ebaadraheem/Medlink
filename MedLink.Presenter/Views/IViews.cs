using MedLink.Presenter.ViewModels;

namespace MedLink.Presenter.Views;

public interface IDoctorListView
{
    List<DoctorCardViewModel> Doctors { get; set; }
    List<SpecialtyViewModel> Specialties { get; set; }
    string? SearchTerm { get; set; }
    int? SelectedSpecialtyId { get; set; }
    string? SortBy { get; set; }
}

public interface IDoctorDetailView
{
    DoctorDetailViewModel? Doctor { get; set; }
    bool CanBookAppointment { get; set; }
}

public interface IAppointmentView
{
    List<AppointmentViewModel> Appointments { get; set; }
    string? StatusFilter { get; set; }
}

public interface IAdminView
{
    AdminDashboardViewModel Dashboard { get; set; }
}