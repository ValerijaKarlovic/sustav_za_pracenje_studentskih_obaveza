namespace backend.Services;

public static class KolegijStatusHelper
{
    /// <summary>Sve planirane obaveze unesene: zbroj max bodova aktivnosti ≥ ukupno bodova kolegija.</summary>
    public static bool SveObavezeDefiniraneNaKolegiju(decimal sumaMaxBodovaAktivnosti, decimal ukupnoBodovaKolegija)
        => sumaMaxBodovaAktivnosti >= ukupnoBodovaKolegija;

    /// <summary>Ocijenjeno = za svaku aktivnost status nije „ceka_se”. Bez aktivnosti → nije završeno.</summary>
    public static bool SveAktivnostiOcijenjene(IReadOnlyCollection<string> statusiPoAktivnosti)
    {
        if (statusiPoAktivnosti.Count == 0) return false;
        return statusiPoAktivnosti.All(s => s != "ceka_se");
    }

    public static bool ZavrsenoOcjenjivanje(
        decimal sumaMaxBodovaAktivnosti,
        decimal ukupnoBodovaKolegija,
        IReadOnlyCollection<string> statusiPoAktivnosti)
        => SveObavezeDefiniraneNaKolegiju(sumaMaxBodovaAktivnosti, ukupnoBodovaKolegija)
           && SveAktivnostiOcijenjene(statusiPoAktivnosti);

    public static bool ProlaziPrag(decimal bodovi, decimal ukupnoBodovaKolegija, short pragProlaza)
        => bodovi >= ukupnoBodovaKolegija * pragProlaza / 100m;

    public static bool KolegijJePolozen(
        decimal bodovi,
        decimal ukupnoBodovaKolegija,
        short pragProlaza,
        decimal sumaMaxBodovaAktivnosti,
        IReadOnlyCollection<string> statusiPoAktivnosti)
        => ProlaziPrag(bodovi, ukupnoBodovaKolegija, pragProlaza)
           && ZavrsenoOcjenjivanje(sumaMaxBodovaAktivnosti, ukupnoBodovaKolegija, statusiPoAktivnosti);

    public static decimal EctsOstvareno(bool kolegijPolozen, short ectsKolegija)
        => kolegijPolozen ? ectsKolegija : 0;

    public static string StatusPrikaz(bool kolegijPolozen, bool zavrsenoOcjenjivanje)
    {
        if (kolegijPolozen) return "Položeno";
        if (zavrsenoOcjenjivanje) return "Nije položeno";
        return "U tijeku";
    }
}
