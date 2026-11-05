using System.ComponentModel.DataAnnotations;
using FeedbackMap.Models;

namespace FeedbackMap.ViewModels;

/// <summary>Skjema for registrering av ny bruker. Feltene som kreves avhenger av valgt rolle.</summary>
public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "E-post må fylles ut."),
     EmailAddress(ErrorMessage = "Ugyldig e-postadresse."),
     Display(Name = "E-post")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passord må fylles ut."),
     StringLength(100, MinimumLength = 10, ErrorMessage = "Passordet må være minst 10 tegn."),
     DataType(DataType.Password), Display(Name = "Passord")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Display(Name = "Bekreft passord"),
     Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Jeg er")]
    public string Role { get; set; } = Roles.ResourceProvider;

    // Offentlig aktør
    [MaxLength(150), Display(Name = "Organisasjon")]
    public string? Organisation { get; set; }

    [MaxLength(100), Display(Name = "Stilling / rolle")]
    public string? RoleTitle { get; set; }

    // Ressursleverandør
    [Display(Name = "Type ressursleverandør")]
    public ProviderType? ProviderType { get; set; }

    [Phone(ErrorMessage = "Ugyldig telefonnummer."), MaxLength(30), Display(Name = "Telefon")]
    public string? ContactPhone { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Role != Roles.PublicActor && Role != Roles.ResourceProvider)
            yield return new ValidationResult("Velg en gyldig rolle.", new[] { nameof(Role) });

        if (Role == Roles.PublicActor && string.IsNullOrWhiteSpace(Organisation))
            yield return new ValidationResult("Organisasjon må fylles ut.", new[] { nameof(Organisation) });

        if (Role == Roles.ResourceProvider && ProviderType is null)
            yield return new ValidationResult("Velg type.", new[] { nameof(ProviderType) });
    }
}

/// <summary>Skjema for innlogging med e-post og passord.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "E-post må fylles ut."), EmailAddress(ErrorMessage = "Ugyldig e-postadresse."),
     Display(Name = "E-post")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passord må fylles ut."), DataType(DataType.Password), Display(Name = "Passord")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Husk meg på denne enheten")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

/// <summary>Informasjon som vises på «Min side».</summary>
public class ManageViewModel
{
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public string? Organisation { get; set; }
}
