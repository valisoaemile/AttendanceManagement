using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Models
{
    public class Equipement
    {
        public int Id { get; set; }  // clé primaire interne, générée par la base — ne jamais l'assigner à la main

        public string? CodeExterne { get; set; }  // ID physique du lecteur BioEntry (ex: 541652289)

        [Required]
        public string Nom { get; set; } = string.Empty; // Ohatra: BioEntry P2 CENTRE

        [Required]
        public string Type { get; set; } = string.Empty; // "Entree" na "Sortie"
    }
}