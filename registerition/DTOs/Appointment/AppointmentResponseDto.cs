namespace registerition.DTOs.Appointment
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialty { get; set; }
        public string PatientName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}
