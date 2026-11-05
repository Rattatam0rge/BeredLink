using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using FeedbackMap.Models;
using FeedbackMap.Services;

namespace FeedbackMap.ViewModels;

/// <summary>Skjema for å registrere eller endre en ressurs, med valgfritt bilde.</summary>
public class ResourceCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Velg type ressurs."), Display(Name = "Type ressurs")]
    public string Type { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Maks 1000 tegn."), Display(Name = "Beskrivelse")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-90.0, 90.0, ErrorMessage = "Ugyldig breddegrad.")]
    public decimal? Latitude { get; set; }

    [Required(ErrorMessage = "Velg et sted på kartet."), Range(-180.0, 180.0, ErrorMessage = "Ugyldig lengdegrad.")]
    public decimal? Longitude { get; set; }

    [Required(ErrorMessage = "Fra-tidspunkt må fylles ut."), Display(Name = "Tilgjengelig fra")]
    public DateTime? AvailableFrom { get; set; }

    [Display(Name = "Tilgjengelig til (valgfritt)")]
    public DateTime? AvailableTo { get; set; }

    /// <summary>Valgfritt bilde av ressursen (JPEG, PNG eller WebP, maks 5 MB). Valideres i ImageStorage.</summary>
    public IFormFile? Image { get; set; }

    /// <summary>Kun ved endring: true betyr at eksisterende bilde skal fjernes.</summary>
    public bool RemoveImage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Type) && !Catalog.ResourceTypes.Contains(Type))
            yield return new ValidationResult("Ugyldig type.", new[] { nameof(Type) });

        if (AvailableFrom.HasValue && AvailableTo.HasValue && AvailableTo < AvailableFrom)
            yield return new ValidationResult("«Til» må være etter «fra».", new[] { nameof(AvailableTo) });
    }
}

/// <summary>Ressursoversikten: leverandørens egne ressurser eller alle tilgjengelige.</summary>
public class ResourceIndexViewModel
{
    public bool IsProvider { get; set; }
    public IReadOnlyList<ResourceListItem> Items { get; set; } = Array.Empty<ResourceListItem>();
}

/// <summary>Utskriftsvennlig ressursliste for en region (kartutsnitt).</summary>
public class ResourcePrintViewModel
{
    public IReadOnlyList<ResourceListItem> Items { get; set; } = Array.Empty<ResourceListItem>();
    public string? Type { get; set; }
    public bool HasBounds { get; set; }
    public decimal MinLat { get; set; }
    public decimal MaxLat { get; set; }
    public decimal MinLng { get; set; }
    public decimal MaxLng { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
