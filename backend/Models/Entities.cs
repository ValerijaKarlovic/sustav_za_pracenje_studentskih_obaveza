namespace backend.Models;

public class Korisnik
{
    public int Id { get; set; }
    public string Ime { get; set; } = "";
    public string Prezime { get; set; } = "";
    public string Email { get; set; } = "";
    public string LozinkaHash { get; set; } = "";
    public string Uloga { get; set; } = "";
    public bool MoraPromijenitiLozinku { get; set; }
}

public class Student
{
    public int KorisnikId { get; set; }
    public string BrojIndeksa { get; set; } = "";
}

public class Nastavnik
{
    public int KorisnikId { get; set; }
    public string? Zvanje { get; set; }
}

public class Kolegij
{
    public int Id { get; set; }
    public string Sifra { get; set; } = "";
    public string Naziv { get; set; } = "";
    public short Ects { get; set; }
    public decimal UkupnoBodova { get; set; }
    public short PragProlaza { get; set; }
    public int NastavnikId { get; set; }
}

public class Upis
{
    public int StudentId { get; set; }
    public int KolegijId { get; set; }
}

public class VrstaAktivnosti
{
    public int Id { get; set; }
    public string Naziv { get; set; } = "";
}

public class Aktivnost
{
    public int Id { get; set; }
    public int KolegijId { get; set; }
    public int VrstaId { get; set; }
    public string Naziv { get; set; } = "";
    public string? Opis { get; set; }
    public DateOnly? Datum { get; set; }
    public decimal MaxBodovi { get; set; }
    public Kolegij Kolegij { get; set; } = null!;
}

public class Evidencija
{
    public int AktivnostId { get; set; }
    public int StudentId { get; set; }
    public int KolegijId { get; set; }
    public string Status { get; set; } = "ceka_se";
    public decimal? Bodovi { get; set; }
}

public class BodoviStudentKolegij
{
    public int StudentId { get; set; }
    public int KolegijId { get; set; }
    public string Kolegij { get; set; } = "";
    public short Ects { get; set; }
    public decimal UkupnoBodova { get; set; }
    public short PragProlaza { get; set; }
    public decimal Bodovi { get; set; }
    public decimal EctsOstvareno { get; set; }
    public bool Prolazi { get; set; }
}

public class NapredakStudentKolegij
{
    public int StudentId { get; set; }
    public int KolegijId { get; set; }
    public DateOnly? Datum { get; set; }
    public decimal KumulativniBodovi { get; set; }
}