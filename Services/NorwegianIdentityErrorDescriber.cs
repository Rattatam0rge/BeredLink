using Microsoft.AspNetCore.Identity;

namespace FeedbackMap.Services;

/// <summary>Norske feilmeldinger fra ASP.NET Core Identity.</summary>
public class NorwegianIdentityErrorDescriber : IdentityErrorDescriber
{
    private static IdentityError E(string code, string description) =>
        new() { Code = code, Description = description };

    public override IdentityError DefaultError() => E(nameof(DefaultError), "Det oppstod en ukjent feil.");
    public override IdentityError ConcurrencyFailure() => E(nameof(ConcurrencyFailure), "Dataene ble endret av noen andre. Prøv igjen.");
    public override IdentityError PasswordMismatch() => E(nameof(PasswordMismatch), "Feil passord.");
    public override IdentityError InvalidToken() => E(nameof(InvalidToken), "Ugyldig kode.");
    public override IdentityError LoginAlreadyAssociated() => E(nameof(LoginAlreadyAssociated), "Denne innloggingen er allerede knyttet til en bruker.");
    public override IdentityError InvalidUserName(string? userName) => E(nameof(InvalidUserName), "Ugyldig brukernavn.");
    public override IdentityError InvalidEmail(string? email) => E(nameof(InvalidEmail), "Ugyldig e-postadresse.");
    public override IdentityError DuplicateUserName(string userName) => E(nameof(DuplicateUserName), "Det finnes allerede en bruker med denne e-postadressen.");
    public override IdentityError DuplicateEmail(string email) => E(nameof(DuplicateEmail), "Det finnes allerede en bruker med denne e-postadressen.");
    public override IdentityError InvalidRoleName(string? role) => E(nameof(InvalidRoleName), "Ugyldig rolle.");
    public override IdentityError DuplicateRoleName(string role) => E(nameof(DuplicateRoleName), "Rollen finnes allerede.");
    public override IdentityError UserAlreadyHasPassword() => E(nameof(UserAlreadyHasPassword), "Brukeren har allerede et passord.");
    public override IdentityError UserLockoutNotEnabled() => E(nameof(UserLockoutNotEnabled), "Utestenging er ikke aktivert for denne brukeren.");
    public override IdentityError UserAlreadyInRole(string role) => E(nameof(UserAlreadyInRole), "Brukeren har allerede denne rollen.");
    public override IdentityError UserNotInRole(string role) => E(nameof(UserNotInRole), "Brukeren har ikke denne rollen.");
    public override IdentityError PasswordTooShort(int length) => E(nameof(PasswordTooShort), $"Passordet må være minst {length} tegn.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => E(nameof(PasswordRequiresUniqueChars), $"Passordet må inneholde minst {uniqueChars} ulike tegn.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => E(nameof(PasswordRequiresNonAlphanumeric), "Passordet må inneholde minst ett spesialtegn.");
    public override IdentityError PasswordRequiresDigit() => E(nameof(PasswordRequiresDigit), "Passordet må inneholde minst ett tall (0–9).");
    public override IdentityError PasswordRequiresLower() => E(nameof(PasswordRequiresLower), "Passordet må inneholde minst én liten bokstav (a–z).");
    public override IdentityError PasswordRequiresUpper() => E(nameof(PasswordRequiresUpper), "Passordet må inneholde minst én stor bokstav (A–Z).");
}
