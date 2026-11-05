using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.ViewModels;

/// <summary>Skjema for å sette organisasjonens posisjon.</summary>
public class ActorBaseViewModel
{
    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-90.0, 90.0, ErrorMessage = "Ugyldig breddegrad.")]
    public decimal? Latitude { get; set; }

    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-180.0, 180.0, ErrorMessage = "Ugyldig lengdegrad.")]
    public decimal? Longitude { get; set; }

    [MaxLength(100, ErrorMessage = "Maks 100 tegn."), Display(Name = "Navn på enhet")]
    public string? Label { get; set; }
}
