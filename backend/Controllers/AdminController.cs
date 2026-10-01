using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
public class AdminController(AppDbContext db, KolegijPregledService pregled) : ControllerBase
{
    private const string DozvoljenaEmailDomena = "@fakultet.hr";

    private async Task<HashSet<int>> OpsegKolegija() => await pregled.SviKolegijIdsAsync();

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> Dashboard([FromQuery] int? kolegijId, [FromQuery] int? dana)
    {
        if (!kolegijId.HasValue)
        {
            var pocetakMjeseca = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var noveAktivnosti = await db.Aktivnosti.CountAsync(x => EF.Property<DateTimeOffset>(x, "kreirano") >= pocetakMjeseca);
            var noviKorisnici = await db.Korisnici.CountAsync(x => EF.Property<DateTimeOffset>(x, "kreirano") >= pocetakMjeseca);
            return Ok(new
            {
                studenata = await db.Studenti.CountAsync(),
                nastavnika = await db.Nastavnici.CountAsync(),
                kolegija = await db.Kolegiji.CountAsync(),
                aktivnosti = await db.Aktivnosti.CountAsync(),
                evidencija = await db.Evidencije.CountAsync(),
                aktivnostiOvajMjesec = noveAktivnosti,
                novihAktivnosti = noveAktivnosti,
                novihKorisnika = noviKorisnici
            });
        }

        var opseg = await OpsegKolegija();
        if (!opseg.Contains(kolegijId.Value)) return NotFound(new { poruka = "Kolegij nije pronađen." });

        var nastavnik = await (from k in db.Kolegiji
                               join n in db.Korisnici on k.NastavnikId equals n.Id
                               where k.Id == kolegijId.Value
                               select n.Ime + " " + n.Prezime).SingleOrDefaultAsync();

        var dashboard = await pregled.GetDashboardAsync(opseg, kolegijId, dana);
        return Ok(new { nastavnik, dashboard });
    }

    [HttpGet("kolegiji/{id:int}/studenti")]
    public async Task<ActionResult<object>> StudentiKolegija(int id)
    {
        var opseg = await OpsegKolegija();
        var studenti = await pregled.GetStudentiRosterAsync(id, opseg);
        if (studenti is null) return NotFound(new { poruka = "Kolegij nije pronađen." });
        return Ok(studenti);
    }

    [HttpGet("kolegiji/{id:int}/studenti/{studentId:int}")]
    public async Task<ActionResult<object>> KartonStudenta(int id, int studentId)
    {
        var opseg = await OpsegKolegija();
        var karton = await pregled.GetKartonAsync(studentId, id, opseg);
        if (karton is null) return NotFound(new { poruka = "Kolegij ili student nije pronađen." });
        return Ok(karton);
    }

    [HttpGet("kolegiji/{id:int}/slobodni-studenti")]
    public async Task<ActionResult<object>> SlobodniStudentiKolegija(int id)
    {
        var opseg = await OpsegKolegija();
        var studenti = await pregled.GetSlobodniStudentiAsync(id, opseg);
        if (studenti is null) return NotFound(new { poruka = "Kolegij nije pronađen." });
        return Ok(studenti);
    }

    [HttpPost("upisi")]
    public async Task<IActionResult> UpisiStudenta(UpisZahtjev zahtjev)
    {
        var opseg = await OpsegKolegija();
        if (!await pregled.UpisiStudentaAsync(zahtjev.StudentId, zahtjev.KolegijId, opseg))
            return NotFound(new { poruka = "Kolegij nije pronađen." });
        return NoContent();
    }

    [HttpDelete("upisi/{kolegijId:int}/{studentId:int}")]
    public async Task<IActionResult> UkloniUpis(int kolegijId, int studentId)
    {
        var opseg = await OpsegKolegija();
        if (!await pregled.UkloniStudentaAsync(kolegijId, studentId, opseg))
            return NotFound();
        return NoContent();
    }

    [HttpGet("aktivnosti")]
    public async Task<ActionResult<object>> Aktivnosti([FromQuery] int? kolegijId)
        => Ok(await (from a in db.Aktivnosti
                     join k in db.Kolegiji on a.KolegijId equals k.Id
                     join v in db.VrsteAktivnosti on a.VrstaId equals v.Id
                     where !kolegijId.HasValue || a.KolegijId == kolegijId
                     orderby a.Datum
                     select new
                     {
                         id = a.Id,
                         naziv = a.Naziv,
                         kolegijId = k.Id,
                         kolegij = k.Naziv,
                         vrstaId = v.Id,
                         vrsta = v.Naziv,
                         datum = a.Datum,
                         opis = a.Opis,
                         bodovi = a.MaxBodovi,
                         imaUneseneBodove = db.Evidencije.Any(e => e.AktivnostId == a.Id)
                     }).ToListAsync());

    [HttpGet("vrste-aktivnosti")]
    public async Task<ActionResult<IEnumerable<VrstaAktivnosti>>> VrsteAktivnosti()
        => Ok(await db.VrsteAktivnosti.OrderBy(x => x.Naziv).ToListAsync());

    [HttpPost("vrste-aktivnosti")]
    public async Task<ActionResult<VrstaAktivnosti>> DodajVrstuAktivnosti(VrstaAktivnostiZahtjev zahtjev)
    {
        var naziv = zahtjev.Naziv.Trim();
        if (string.IsNullOrWhiteSpace(naziv)) return BadRequest(new { poruka = "Naziv vrste aktivnosti je obavezan." });
        var postojeca = await db.VrsteAktivnosti.SingleOrDefaultAsync(x => x.Naziv.ToLower() == naziv.ToLower());
        if (postojeca is not null) return Ok(postojeca);
        var vrsta = new VrstaAktivnosti { Naziv = naziv };
        db.VrsteAktivnosti.Add(vrsta);
        await db.SaveChangesAsync();
        return Created($"/api/admin/vrste-aktivnosti/{vrsta.Id}", vrsta);
    }

    [HttpGet("aktivnosti/{id:int}/studenti")]
    public async Task<ActionResult<object>> StudentiZaAktivnost(int id)
        => Ok(await (from upis in db.Upisi
                     join s in db.Studenti on upis.StudentId equals s.KorisnikId
                     join u in db.Korisnici on s.KorisnikId equals u.Id
                     join e in db.Evidencije.Where(x => x.AktivnostId == id)
                         on s.KorisnikId equals e.StudentId into evidencije
                     where upis.KolegijId == db.Aktivnosti.Where(a => a.Id == id).Select(a => a.KolegijId).FirstOrDefault()
                     select new { studentId = s.KorisnikId, ime = u.Ime + " " + u.Prezime, status = evidencije.Select(x => x.Status).FirstOrDefault() ?? "ceka_se", bodovi = evidencije.Select(x => x.Bodovi).FirstOrDefault() }).ToListAsync());

    [HttpPost("aktivnosti")]
    public async Task<ActionResult<Aktivnost>> DodajAktivnost(AktivnostZahtjev zahtjev)
    {
        if (!await db.Kolegiji.AnyAsync(x => x.Id == zahtjev.KolegijId))
            return NotFound(new { poruka = "Kolegij nije pronađen." });

        var aktivnost = new Aktivnost
        {
            KolegijId = zahtjev.KolegijId,
            VrstaId = zahtjev.VrstaId,
            Naziv = zahtjev.Naziv,
            Opis = string.IsNullOrWhiteSpace(zahtjev.Opis) ? null : zahtjev.Opis.Trim(),
            Datum = zahtjev.Datum,
            MaxBodovi = zahtjev.MaxBodovi
        };
        db.Aktivnosti.Add(aktivnost);
        await db.SaveChangesAsync();
        return Created($"/api/admin/aktivnosti/{aktivnost.Id}", aktivnost);
    }

    [HttpPut("aktivnosti/{id:int}")]
    public async Task<IActionResult> UrediAktivnost(int id, AktivnostZahtjev zahtjev)
    {
        var aktivnost = await db.Aktivnosti.FindAsync(id);
        if (aktivnost is null) return NotFound();
        aktivnost.KolegijId = zahtjev.KolegijId;
        aktivnost.VrstaId = zahtjev.VrstaId;
        aktivnost.Naziv = zahtjev.Naziv;
        aktivnost.Opis = string.IsNullOrWhiteSpace(zahtjev.Opis) ? null : zahtjev.Opis.Trim();
        aktivnost.Datum = zahtjev.Datum;
        aktivnost.MaxBodovi = zahtjev.MaxBodovi;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("aktivnosti/{id:int}")]
    public async Task<IActionResult> ObrisiAktivnost(int id)
    {
        var aktivnost = await db.Aktivnosti.FindAsync(id);
        if (aktivnost is null) return NotFound();
        db.Aktivnosti.Remove(aktivnost);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("korisnici")]
    public async Task<ActionResult<IEnumerable<AdminKorisnikListOdgovor>>> Korisnici()
        => Ok(await (from k in db.Korisnici
                     join s in db.Studenti on k.Id equals s.KorisnikId into studenti
                     from s in studenti.DefaultIfEmpty()
                     orderby k.Prezime, k.Ime
                     select new AdminKorisnikListOdgovor(
                         k.Id,
                         k.Ime,
                         k.Prezime,
                         k.Email,
                         k.Uloga,
                         s != null ? s.BrojIndeksa : null)).ToListAsync());

    [HttpPost("korisnici")]
    public async Task<ActionResult<Korisnik>> DodajKorisnika(AdminKorisnikZahtjev zahtjev)
    {
        var email = zahtjev.Email.Trim().ToLowerInvariant();
        if (!email.EndsWith(DozvoljenaEmailDomena, StringComparison.Ordinal))
            return BadRequest(new { poruka = "Email mora završavati s @fakultet.hr." });

        if (await db.Korisnici.AnyAsync(x => x.Email == email))
            return Conflict(new { poruka = "Korisnik s tim emailom već postoji." });

        if (zahtjev.Uloga == "student" && string.IsNullOrWhiteSpace(zahtjev.BrojIndeksa))
            return BadRequest(new { poruka = "Broj indeksa je obavezan za studenta." });

        var brojIndeksa = zahtjev.BrojIndeksa?.Trim();
        if (zahtjev.Uloga == "student" && await db.Studenti.AnyAsync(s => s.BrojIndeksa == brojIndeksa))
            return Conflict(new { poruka = "Broj indeksa je već zauzet." });

        var korisnik = new Korisnik
        {
            Ime = zahtjev.Ime.Trim(),
            Prezime = zahtjev.Prezime.Trim(),
            Email = email,
            LozinkaHash = BCrypt.Net.BCrypt.HashPassword(zahtjev.Lozinka),
            Uloga = zahtjev.Uloga,
            MoraPromijenitiLozinku = true
        };
        db.Korisnici.Add(korisnik);
        await db.SaveChangesAsync();

        if (korisnik.Uloga == "student")
            db.Studenti.Add(new Student { KorisnikId = korisnik.Id, BrojIndeksa = brojIndeksa! });
        else if (korisnik.Uloga == "nastavnik")
            db.Nastavnici.Add(new Nastavnik { KorisnikId = korisnik.Id });
        await db.SaveChangesAsync();
        return Created($"/api/admin/korisnici/{korisnik.Id}", korisnik);
    }

    [HttpPut("korisnici/{id:int}")]
    public async Task<IActionResult> UrediKorisnika(int id, AdminKorisnikUrediZahtjev zahtjev)
    {
        var novaUloga = zahtjev.Uloga?.Trim().ToLowerInvariant();
        if (novaUloga is not ("student" or "nastavnik" or "admin"))
            return BadRequest(new { poruka = "Uloga mora biti student, nastavnik ili admin." });

        var email = zahtjev.Email.Trim().ToLowerInvariant();
        if (!email.EndsWith(DozvoljenaEmailDomena, StringComparison.Ordinal))
            return BadRequest(new { poruka = "Email mora završavati s @fakultet.hr." });

        var korisnik = await db.Korisnici.FindAsync(id);
        if (korisnik is null) return NotFound();

        if (await db.Korisnici.AnyAsync(x => x.Email == email && x.Id != id))
            return Conflict(new { poruka = "Korisnik s tim emailom već postoji." });

        if (novaUloga == "student" && string.IsNullOrWhiteSpace(zahtjev.BrojIndeksa))
            return BadRequest(new { poruka = "Broj indeksa je obavezan za studenta." });

        string? brojIndeksa = zahtjev.BrojIndeksa?.Trim();
        if (novaUloga == "student" && await db.Studenti.AnyAsync(s => s.BrojIndeksa == brojIndeksa && s.KorisnikId != id))
            return Conflict(new { poruka = "Broj indeksa je već zauzet." });

        var staraUloga = korisnik.Uloga;
        if (staraUloga != novaUloga)
        {
            if (staraUloga == "admin" && novaUloga != "admin")
            {
                var brojAdmina = await db.Korisnici.CountAsync(x => x.Uloga == "admin");
                if (brojAdmina <= 1)
                    return BadRequest(new { poruka = "Mora postojati barem jedan administrator." });
            }

            if (staraUloga == "nastavnik" && await db.Kolegiji.AnyAsync(x => x.NastavnikId == id))
                return Conflict(new { poruka = "Nastavnik ima dodijeljene kolegije. Prvo promijenite nastavnika na kolegijima." });

            if (staraUloga == "student")
            {
                var student = await db.Studenti.FindAsync(id);
                if (student is not null)
                    db.Studenti.Remove(student);
            }
            else if (staraUloga == "nastavnik")
            {
                var nastavnik = await db.Nastavnici.FindAsync(id);
                if (nastavnik is not null)
                    db.Nastavnici.Remove(nastavnik);
            }

            korisnik.Uloga = novaUloga;

            if (novaUloga == "student")
                db.Studenti.Add(new Student { KorisnikId = id, BrojIndeksa = brojIndeksa! });
            else if (novaUloga == "nastavnik")
                db.Nastavnici.Add(new Nastavnik { KorisnikId = id });
        }
        else if (novaUloga == "student")
        {
            var student = await db.Studenti.FindAsync(id);
            if (student is null)
                return BadRequest(new { poruka = "Podaci studenta nisu pronađeni." });
            student.BrojIndeksa = brojIndeksa!;
        }

        korisnik.Ime = zahtjev.Ime.Trim();
        korisnik.Prezime = zahtjev.Prezime.Trim();
        korisnik.Email = email;
        if (!string.IsNullOrWhiteSpace(zahtjev.Lozinka))
        {
            korisnik.LozinkaHash = BCrypt.Net.BCrypt.HashPassword(zahtjev.Lozinka);
            korisnik.MoraPromijenitiLozinku = true;
        }
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("korisnici/{id:int}")]
    public async Task<IActionResult> ObrisiKorisnika(int id)
    {
        var korisnik = await db.Korisnici.FindAsync(id);
        if (korisnik is null) return NotFound();
        if (korisnik.Uloga == "admin" && await db.Korisnici.CountAsync(x => x.Uloga == "admin") <= 1)
            return BadRequest(new { poruka = "Ne može se obrisati zadnji administrator u sustavu." });
        if (korisnik.Uloga == "nastavnik" && await db.Kolegiji.AnyAsync(x => x.NastavnikId == id))
            return Conflict(new { poruka = "Nastavnik ima kolegije. Prvo promijenite nastavnika kolegijima." });
        db.Korisnici.Remove(korisnik);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("kolegiji")]
    public async Task<ActionResult<object>> Kolegiji()
        => Ok(await (from k in db.Kolegiji
                     join n in db.Korisnici on k.NastavnikId equals n.Id
                     orderby k.Naziv
                     select new
                     {
                         k.Id,
                         k.Sifra,
                         k.Naziv,
                         k.Ects,
                         k.UkupnoBodova,
                         k.PragProlaza,
                         k.NastavnikId,
                         Nastavnik = n.Ime + " " + n.Prezime,
                         Studenata = db.Upisi.Count(u => u.KolegijId == k.Id)
                     }).ToListAsync());

    [HttpGet("statistika")]
    public async Task<ActionResult<object>> Statistika()
    {
        var pocetakMjeseca = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var noveAktivnosti = await db.Aktivnosti.CountAsync(x => EF.Property<DateTimeOffset>(x, "kreirano") >= pocetakMjeseca);
        var noviKorisnici = await db.Korisnici.CountAsync(x => EF.Property<DateTimeOffset>(x, "kreirano") >= pocetakMjeseca);
        return Ok(new
        {
            studenata = await db.Studenti.CountAsync(),
            nastavnika = await db.Nastavnici.CountAsync(),
            kolegija = await db.Kolegiji.CountAsync(),
            aktivnosti = await db.Aktivnosti.CountAsync(),
            evidencija = await db.Evidencije.CountAsync(),
            aktivnostiOvajMjesec = noveAktivnosti,
            novihAktivnosti = noveAktivnosti,
            novihKorisnika = noviKorisnici
        });
    }

    [HttpGet("evidencije")]
    public async Task<ActionResult<object>> Evidencije()
        => Ok(await (from e in db.Evidencije
                     join a in db.Aktivnosti on e.AktivnostId equals a.Id
                     join k in db.Kolegiji on e.KolegijId equals k.Id
                     join s in db.Studenti on e.StudentId equals s.KorisnikId
                     join u in db.Korisnici on s.KorisnikId equals u.Id
                     select new
                     {
                         aktivnostId = e.AktivnostId,
                         studentId = e.StudentId,
                         aktivnost = a.Naziv,
                         kolegij = k.Naziv,
                         student = u.Ime + " " + u.Prezime,
                         status = e.Status,
                         bodovi = e.Bodovi
                     }).ToListAsync());

    [HttpPost("kolegiji")]
    public async Task<ActionResult<Kolegij>> DodajKolegij(KolegijZahtjev zahtjev)
    {
        var kolegij = new Kolegij
        {
            Sifra = zahtjev.Sifra,
            Naziv = zahtjev.Naziv,
            Ects = zahtjev.Ects,
            UkupnoBodova = zahtjev.UkupnoBodova,
            PragProlaza = zahtjev.PragProlaza,
            NastavnikId = zahtjev.NastavnikId
        };
        db.Kolegiji.Add(kolegij);
        await db.SaveChangesAsync();
        return Created($"/api/admin/kolegiji/{kolegij.Id}", kolegij);
    }

    [HttpPut("kolegiji/{id:int}")]
    public async Task<IActionResult> UrediKolegij(int id, KolegijZahtjev zahtjev)
    {
        var kolegij = await db.Kolegiji.FindAsync(id);
        if (kolegij is null) return NotFound();
        kolegij.Sifra = zahtjev.Sifra;
        kolegij.Naziv = zahtjev.Naziv;
        kolegij.Ects = zahtjev.Ects;
        kolegij.UkupnoBodova = zahtjev.UkupnoBodova;
        kolegij.PragProlaza = zahtjev.PragProlaza;
        kolegij.NastavnikId = zahtjev.NastavnikId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("kolegiji/{id:int}")]
    public async Task<IActionResult> ObrisiKolegij(int id)
    {
        var kolegij = await db.Kolegiji.FindAsync(id);
        if (kolegij is null) return NotFound();
        db.Kolegiji.Remove(kolegij);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("evidencije/{aktivnostId:int}/{studentId:int}")]
    public async Task<IActionResult> UrediEvidenciju(int aktivnostId, int studentId, AdminEvidencijaZahtjev zahtjev)
    {
        var aktivnost = await db.Aktivnosti.AsNoTracking().SingleOrDefaultAsync(x => x.Id == aktivnostId);
        if (aktivnost is null) return NotFound(new { poruka = "Aktivnost nije pronađena." });

        if (!await db.Upisi.AnyAsync(x => x.KolegijId == aktivnost.KolegijId && x.StudentId == studentId))
            return BadRequest(new { poruka = "Student nije upisan na ovaj kolegij." });

        var greska = EvidencijaValidator.Validiraj(
            new EvidencijaZahtjev(zahtjev.Status, zahtjev.Bodovi),
            aktivnost);
        if (greska is not null) return BadRequest(new { poruka = greska });

        var evidencija = await db.Evidencije.SingleOrDefaultAsync(x => x.AktivnostId == aktivnostId && x.StudentId == studentId);
        if (evidencija is null)
        {
            evidencija = new Evidencija
            {
                AktivnostId = aktivnostId,
                StudentId = studentId,
                KolegijId = aktivnost.KolegijId,
            };
            db.Evidencije.Add(evidencija);
        }
        evidencija.Status = zahtjev.Status;
        evidencija.Bodovi = zahtjev.Bodovi;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("evidencije/{aktivnostId:int}/{studentId:int}")]
    public async Task<IActionResult> ObrisiEvidenciju(int aktivnostId, int studentId)
    {
        var evidencija = await db.Evidencije.SingleOrDefaultAsync(x => x.AktivnostId == aktivnostId && x.StudentId == studentId);
        if (evidencija is null) return NotFound();
        db.Evidencije.Remove(evidencija);
        await db.SaveChangesAsync();
        return NoContent();
    }
}