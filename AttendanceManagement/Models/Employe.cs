namespace AttendanceManagement.Models
{
    public class Employe
    {
        public int Id { get; set; }
        public string Matricule { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;

        public ICollection<Pointage> Pointages { get; set; } = new List<Pointage>();
    }
}