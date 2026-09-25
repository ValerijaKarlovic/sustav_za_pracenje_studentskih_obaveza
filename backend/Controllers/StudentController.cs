using System.Security.Claims;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize(Roles = "student")]
[Route("api/student")]
public class StudentController(AppDbContext db) : ControllerBase
{
    private int StudentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("kolegiji")]
    public async Task<ActionResult<IEnumerable<BodoviStudentKolegij>>> Kolegiji()
        => Ok(await db.BodoviStudentKolegij.Where(x => x.StudentId == StudentId).ToListAsync());

    [HttpGet("kolegiji/{id:int}")]
    public async Task<ActionResult<object>> Kolegij(int id)
    {
        var upis = await db.Upisi.AsNoTracking().SingleOrDefaultAsync(x => x.StudentId == StudentId && x.KolegijId == id);
        if (upis is null) return NotFound(new { poruka = "Kolegij nije pronađen." });

        var kolegij = await db.Kolegiji.AsNoTracking().SingleAsync(x => x.Id == id);
        var nastavnik = await db.Korisnici.AsNoTracking()
            .Where(n => n.Id == kolegij.NastavnikId)
            .Select(n => n.Ime + " " + n.Prezime)
            .SingleOrDefaultAsync() ?? "";
        var aktivnosti = await (from a in db.Aktivnosti.AsNoTracking()
                                join v in db.VrsteAktivnosti.AsNoTracking() on a.VrstaId equals v.Id
                                join e in db.Evidencije.AsNoTracking().Where(x => x.StudentId == StudentId)
                                    on new { AktivnostId = a.Id, KolegijId = a.KolegijId }
                                    equals new { e.AktivnostId, e.KolegijId } into evidencije
                                from e in evidencije.DefaultIfEmpty()
                                where a.KolegijId == id
                                select new AktivnostOdgovor(a.Id, a.Naziv, a.Datum, a.MaxBodovi,
                                    e == null ? "ceka_se" : e.Status, e == null ? null : e.Bodovi, v.Naziv, a.Opis)).ToListAsync();

        var bodovi = aktivnosti.Sum(a => a.Bodovi ?? 0);
        var prag = kolegij.UkupnoBodova * kolegij.PragProlaza / 100m;
        var prolazi = bodovi >= prag;
        var sumaMaxAktivnosti = aktivnosti.Sum(a => a.MaxBodovi);
        var zavrsenoOcjenjivanje = KolegijStatusHelper.ZavrsenoOcjenjivanje(
            sumaMaxAktivnosti,
            kolegij.UkupnoBodova,
            aktivnosti.Select(a => a.Status).ToList());

        return Ok(new
        {
            kolegij,
            nastavnik,
            aktivnosti,
            bodovi,
            maxBodovi = kolegij.UkupnoBodova,
            prolazi,
            zavrsenoOcjenjivanje,
        });
    }

    [HttpGet("statistika")]
    public async Task<ActionResult<IEnumerable<NapredakStudentKolegij>>> Statistika()
        => Ok(await db.NapredakStudentKolegij.Where(x => x.StudentId == StudentId)
            .OrderBy(x => x.Datum).ToListAsync());

    [HttpGet("aktivnosti")]
    public async Task<ActionResult<object>> Aktivnosti()
        => Ok(await (from upis in db.Upisi
                     join a in db.Aktivnosti on upis.KolegijId equals a.KolegijId
                     join k in db.Kolegiji on upis.KolegijId equals k.Id
                     join v in db.VrsteAktivnosti on a.VrstaId equals v.Id
                     join e in db.Evidencije.Where(x => x.StudentId == StudentId)
                         on new { AktivnostId = a.Id, KolegijId = a.KolegijId }
                         equals new { e.AktivnostId, e.KolegijId } into evidencije
                     from e in evidencije.DefaultIfEmpty()
                     where upis.StudentId == StudentId
                     orderby a.Datum
                     select new
                     {
                         naziv = a.Naziv,
                         opis = a.Opis,
                         kolegij = k.Naziv,
                         vrsta = v.Naziv,
                         datum = a.Datum,
                         status = e == null ? "ceka_se" : e.Status,
                         bodovi = e == null ? null : e.Bodovi,
                         maxBodovi = a.MaxBodovi
                     }).ToListAsync());

    [HttpGet("profil")]
    public async Task<ActionResult<object>> Profil()
    {
        var profil = await (from s in db.Studenti
                            join k in db.Korisnici on s.KorisnikId equals k.Id
                            where s.KorisnikId == StudentId
                            select new { s.BrojIndeksa, k.Ime, k.Prezime, k.Email }).SingleOrDefaultAsync();
        return profil is null ? NotFound() : Ok(profil);
    }
}