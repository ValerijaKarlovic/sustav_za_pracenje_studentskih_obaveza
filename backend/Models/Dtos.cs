namespace backend.Models;

public record LoginZahtjev(string Email, string Lozinka);

public record PromijeniLozinkuZahtjev(string TrenutnaLozinka, string NovaLozinka);

public record KorisnikOdgovor(
    int Id,
    string Ime,
    string Prezime,
    string Email,
    string Uloga,
    bool MoraPromijenitiLozinku,
    string Token);

public record AktivnostOdgovor(
    int Id,
    string Naziv,
    DateOnly? Datum,
    decimal MaxBodovi,
    string Status,
    decimal? Bodovi,
    string Vrsta,
    string? Opis);

public record AktivnostZahtjev(int KolegijId, int VrstaId, string Naziv, DateOnly? Datum, decimal MaxBodovi, string? Opis);
public record EvidencijaZahtjev(string Status, decimal? Bodovi);
public record KolegijZahtjev(string Sifra, string Naziv, short Ects, decimal UkupnoBodova, short PragProlaza, int NastavnikId);
public record UpisZahtjev(int StudentId, int KolegijId);
public record AdminKorisnikListOdgovor(int Id, string Ime, string Prezime, string Email, string Uloga, string? BrojIndeksa);
public record AdminKorisnikZahtjev(string Ime, string Prezime, string Email, string Lozinka, string Uloga, string? BrojIndeksa);
public record AdminKorisnikUrediZahtjev(string Ime, string Prezime, string Email, string? Lozinka, string? BrojIndeksa);
public record AdminEvidencijaZahtjev(string Status, decimal? Bodovi);
public record KolegijPostavkeZahtjev(decimal UkupnoBodova, short PragProlaza);
public record VrstaAktivnostiZahtjev(string Naziv);