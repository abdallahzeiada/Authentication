namespace registerition.DTOs.Doctor
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
        public string Experience { get; set; }
        public decimal Rating { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
    }
}
