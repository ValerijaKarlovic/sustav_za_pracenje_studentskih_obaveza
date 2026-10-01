using backend.Models;

namespace backend.Services;

public static class EvidencijaValidator
{
    private static readonly HashSet<string> DopusteniStatusi = new(StringComparer.Ordinal)
    {
        "ceka_se", "odradeno", "nije_odradeno"
    };

    public static string? Validiraj(EvidencijaZahtjev zahtjev, Aktivnost aktivnost)
    {
        if (!DopusteniStatusi.Contains(zahtjev.Status))
            return "Status evidencije nije dopušten.";

        if (zahtjev.Status == "ceka_se")
        {
            if (zahtjev.Bodovi is not null && zahtjev.Bodovi < 0)
                return "Bodovi ne mogu biti negativni.";
            return null;
        }

        if (zahtjev.Bodovi is null)
            return "Bodovi su obavezni kada je aktivnost ocijenjena.";

        if (zahtjev.Bodovi < 0)
            return "Bodovi ne mogu biti negativni.";

        if (zahtjev.Bodovi > aktivnost.MaxBodovi)
            return $"Bodovi ne mogu biti veći od maksimuma aktivnosti ({aktivnost.MaxBodovi}).";

        return null;
    }
}
