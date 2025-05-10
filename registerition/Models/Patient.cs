namespace registerition.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? MedicalHistory { get; set; }
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
