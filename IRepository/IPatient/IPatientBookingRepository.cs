using Midix.Models;
using Midix.ViewModel;
using Midix.DTO;
using Midix.Enums;

namespace Midix.IRepository.IPatient
{
    public interface IPatientBookingRepository
    {
        Task<BookAppointmentViewModel> GetBookingDataAsync(string patientId);
        Task<SlotResultDto> GetAvailableSlotsAsync(string doctorId, DateTime selectedDate); 
        Task<(BookingResult Result, Appointment? Appointment)> BookAppointmentAsync(
        string patientId, string doctorId, DateTime appointmentDate,
        PaymentMethod paymentMethod, AppointmentType appointmentType);
    }
}
