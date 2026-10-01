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
public class StudentController(AppDbContext db, KolegijRezultatService kolegijRezultat) : ControllerBase
{
    private int StudentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("kolegiji")]
    public async Task<ActionResult<IEnumerable<object>>> Kolegiji()
    {
        var lista = await kolegijRezultat.ListZaStudentaAsync(StudentId);
        return Ok(lista);
    }

    [HttpGet("kolegiji/{id:int}")]
    public async Task<ActionResult<object>> Kolegij(int id)
    {
        var detalj = await kolegijRezultat.DetaljZaStudentaAsync(StudentId, id);
        if (detalj is null) return NotFound(new { poruka = "Kolegij nije pronađen." });

        var (kolegij, nastavnik, aktivnosti, rez) = detalj.Value;
        return Ok(new
        {
            kolegij,
            nastavnik,
            aktivnosti,
            bodovi = rez.Bodovi,
            maxBodovi = rez.MaxBodovi,
            pragBodova = rez.PragBodova,
            prolaziPrag = rez.ProlaziPrag,
            zavrsenoOcjenjivanje = rez.ZavrsenoOcjenjivanje,
            polozen = rez.Polozen,
            ectsOstvareno = rez.EctsOstvareno,
            statusPrikaz = rez.StatusPrikaz,
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
