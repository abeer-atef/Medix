namespace Midix.DTO.DoctorDashboardDTOs
{
    public class DoctorPatientRowDto
    {
        public string PatientId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int Age { get; set; }
        public string LastVisit { get; set; } = string.Empty;
        public string LastDiagnosis { get; set; } = string.Empty;
        public int ActivePrescriptions { get; set; }
        public int LastAppointmentId { get; set; }
    }

}
