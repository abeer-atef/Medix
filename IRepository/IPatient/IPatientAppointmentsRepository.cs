using Midix.ViewModel;

namespace Midix.IRepository.IPatient
{
    public interface IPatientAppointmentsRepository
    {
        Task<PatientAppointmentsViewModel> GetAppointmentsAsync(
            string patientId, int upcomingPage = 1, int pastPage = 1, int pageSize = 10);
    }
}
