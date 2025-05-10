namespace registerition.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Specialty { get; set; }
        public string Experience { get; set; }
        public decimal Rating { get; set; }
        public bool IsAvailable { get; set; }
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
