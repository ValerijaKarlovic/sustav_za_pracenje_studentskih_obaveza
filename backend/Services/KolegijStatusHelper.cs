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
}
