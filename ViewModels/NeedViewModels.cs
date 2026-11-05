using System.ComponentModel.DataAnnotations;
using FeedbackMap.Models;

namespace FeedbackMap.ViewModels;

/// <summary>Skjema for å registrere et behov (valideres både i nettleser og på server).</summary>
public class NeedCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Velg type behov."), Display(Name = "Type behov")]
    public string Type { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Maks 1000 tegn."), Display(Name = "Beskrivelse")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-90.0, 90.0, ErrorMessage = "Ugyldig breddegrad.")]
    public decimal? Latitude { get; set; }

    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-180.0, 180.0, ErrorMessage = "Ugyldig lengdegrad.")]
    public decimal? Longitude { get; set; }

    [Required(ErrorMessage = "Frist må fylles ut."), Display(Name = "Frist")]
    public DateTime? Deadline { get; set; }

    [Display(Name = "Prioritet")]
    public NeedPriority Priority { get; set; } = NeedPriority.Acute;

    [Required(ErrorMessage = "Kontaktpunkt må fylles ut."), MaxLength(200, ErrorMessage = "Maks 200 tegn."),
     Display(Name = "Kontaktpunkt (navn og telefon)")]
    public string ContactPoint { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Type) && !Catalog.NeedTypes.Contains(Type))
            yield return new ValidationResult("Ugyldig type.", new[] { nameof(Type) });
    }
}
