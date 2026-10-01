using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Authorize(Roles = "nastavnik")]
[Route("api/nastavnik")]
public class NastavnikController(AppDbContext db, KolegijPregledService pregled) : ControllerBase
{
    private int NastavnikId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<HashSet<int>> OpsegKolegija() => await pregled.KolegijIdsZaNastavnikaAsync(NastavnikId);

    [HttpGet("kolegiji")]
    public async Task<ActionResult<IEnumerable<Kolegij>>> Kolegiji()
        => Ok(await db.Kolegiji.Where(x => x.NastavnikId == NastavnikId).ToListAsync());

    [HttpPut("kolegiji/{id:int}/postavke")]
    public async Task<IActionResult> SpremiPostavkeKolegija(int id, KolegijPostavkeZahtjev zahtjev)
    {
        var kolegij = await db.Kolegiji.SingleOrDefaultAsync(x => x.Id == id && x.NastavnikId == NastavnikId);
        if (kolegij is null) return NotFound(new { poruka = "Kolegij nije pronađen među kolegijima koje predajete." });
        kolegij.UkupnoBodova = zahtjev.UkupnoBodova;
        kolegij.PragProlaza = zahtjev.PragProlaza;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> Dashboard([FromQuery] int? kolegijId, [FromQuery] int? dana)
    {
        var opseg = await OpsegKolegija();
        return Ok(await pregled.GetDashboardAsync(opseg, kolegijId, dana));
    }

    [HttpGet("studenti")]
    public async Task<ActionResult<object>> Studenti([FromQuery] int kolegijId)
    {
        var opseg = await OpsegKolegija();
        if (!opseg.Contains(kolegijId)) return NotFound(new { poruka = "Kolegij nije pronađen." });
        var studenti = await pregled.GetStudentiRosterAsync(kolegijId, opseg);
        return Ok(studenti);
    }

    [HttpGet("aktivnosti")]
    public async Task<ActionResult<object>> Aktivnosti()
        => Ok(await (from a in db.Aktivnosti
                     join k in db.Kolegiji on a.KolegijId equals k.Id
                     join v in db.VrsteAktivnosti on a.VrstaId equals v.Id
                     where k.NastavnikId == NastavnikId
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
        return Created($"/api/nastavnik/vrste-aktivnosti/{vrsta.Id}", vrsta);
    }

    [HttpGet("aktivnosti/{id:int}/studenti")]
    public async Task<ActionResult<object>> StudentiZaAktivnost(int id)
    {
        var aktivnost = await db.Aktivnosti.Include(x => x.Kolegij).AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);
        if (aktivnost is null || aktivnost.Kolegij.NastavnikId != NastavnikId)
            return NotFound(new { poruka = "Aktivnost nije pronađena." });

        var kolegijId = aktivnost.KolegijId;
        var rezultat = await (from upis in db.Upisi
                              join s in db.Studenti on upis.StudentId equals s.KorisnikId
                              join u in db.Korisnici on s.KorisnikId equals u.Id
                              join e in db.Evidencije.Where(x => x.AktivnostId == id)
                                  on s.KorisnikId equals e.StudentId into evidencije
                              where upis.KolegijId == kolegijId
                              select new
                              {
                                  studentId = s.KorisnikId,
                                  ime = u.Ime + " " + u.Prezime,
                                  status = evidencije.Select(x => x.Status).FirstOrDefault() ?? "ceka_se",
                                  bodovi = evidencije.Select(x => x.Bodovi).FirstOrDefault()
                              }).ToListAsync();
        return Ok(rezultat);
    }

    [HttpPost("aktivnosti")]
    public async Task<ActionResult<Aktivnost>> DodajAktivnost(AktivnostZahtjev zahtjev)
    {
        var pripada = await db.Kolegiji.AnyAsync(x => x.Id == zahtjev.KolegijId && x.NastavnikId == NastavnikId);
        if (!pripada) return NotFound(new { poruka = "Kolegij nije pronađen." });

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
        return Created($"/api/nastavnik/aktivnosti/{aktivnost.Id}", aktivnost);
    }

    [HttpPut("aktivnosti/{id:int}")]
    public async Task<IActionResult> UrediAktivnost(int id, AktivnostZahtjev zahtjev)
    {
        var aktivnost = await db.Aktivnosti.Include(x => x.Kolegij).SingleOrDefaultAsync(x => x.Id == id);
        if (aktivnost is null || aktivnost.Kolegij.NastavnikId != NastavnikId) return NotFound();
        var noviKolegijOk = await db.Kolegiji.AnyAsync(x => x.Id == zahtjev.KolegijId && x.NastavnikId == NastavnikId);
        if (!noviKolegijOk) return NotFound(new { poruka = "Kolegij nije pronađen." });
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
        var aktivnost = await db.Aktivnosti.Include(x => x.Kolegij).SingleOrDefaultAsync(x => x.Id == id);
        if (aktivnost is null || aktivnost.Kolegij.NastavnikId != NastavnikId) return NotFound();
        db.Aktivnosti.Remove(aktivnost);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("evidencije/{aktivnostId:int}/{studentId:int}")]
    public async Task<IActionResult> SpremiEvidenciju(int aktivnostId, int studentId, EvidencijaZahtjev zahtjev)
    {
        var aktivnost = await db.Aktivnosti.Include(x => x.Kolegij)
            .SingleOrDefaultAsync(x => x.Id == aktivnostId);
        if (aktivnost is null || aktivnost.Kolegij.NastavnikId != NastavnikId)
            return NotFound(new { poruka = "Aktivnost nije pronađena." });

        var upisan = await db.Upisi.AnyAsync(x =>
            x.KolegijId == aktivnost.KolegijId && x.StudentId == studentId);
        if (!upisan)
            return BadRequest(new { poruka = "Student nije upisan na ovaj kolegij." });

        var greska = EvidencijaValidator.Validiraj(zahtjev, aktivnost);
        if (greska is not null) return BadRequest(new { poruka = greska });

        var evidencija = await db.Evidencije.SingleOrDefaultAsync(x =>
            x.AktivnostId == aktivnostId && x.StudentId == studentId);
        if (evidencija is null)
        {
            evidencija = new Evidencija { AktivnostId = aktivnostId, StudentId = studentId, KolegijId = aktivnost.KolegijId };
            db.Evidencije.Add(evidencija);
        }
        evidencija.Status = zahtjev.Status;
        evidencija.Bodovi = zahtjev.Bodovi;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("upisi")]
    public async Task<IActionResult> UpišiStudenta(UpisZahtjev zahtjev)
    {
        var opseg = await OpsegKolegija();
        if (!await pregled.UpisiStudentaAsync(zahtjev.StudentId, zahtjev.KolegijId, opseg))
            return NotFound();
        return NoContent();
    }

    [HttpDelete("upisi/{kolegijId:int}/{studentId:int}")]
    public async Task<IActionResult> UkloniStudenta(int kolegijId, int studentId)
    {
        var opseg = await OpsegKolegija();
        if (!await pregled.UkloniStudentaAsync(kolegijId, studentId, opseg))
            return NotFound();
        return NoContent();
    }

    [HttpGet("studenti/{studentId:int}")]
    public async Task<ActionResult<object>> Karton(int studentId, [FromQuery] int kolegijId)
    {
        var opseg = await OpsegKolegija();
        var karton = await pregled.GetKartonAsync(studentId, kolegijId, opseg);
        if (karton is null) return NotFound(new { poruka = "Kolegij ili student nije pronađen." });
        return Ok(karton);
    }

    [HttpGet("slobodni-studenti")]
    public async Task<ActionResult<object>> SlobodniStudenti([FromQuery] int kolegijId)
    {
        var opseg = await OpsegKolegija();
        if (!opseg.Contains(kolegijId)) return NotFound(new { poruka = "Kolegij nije pronađen." });
        var studenti = await pregled.GetSlobodniStudentiAsync(kolegijId, opseg);
        return Ok(studenti);
    }
}
