using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class KolegijPregledService(AppDbContext db)
{
    public async Task<bool> KolegijJeUOpseguAsync(int kolegijId, IReadOnlyCollection<int> opsegKolegijIds)
        => opsegKolegijIds.Contains(kolegijId);

    public async Task<object> GetDashboardAsync(IReadOnlyCollection<int> opsegKolegijIds, int? kolegijId = null, int? dana = null)
    {
        var kolegiji = await db.Kolegiji.Where(x => opsegKolegijIds.Contains(x.Id)).ToListAsync();
        var ids = kolegiji.Where(x => !kolegijId.HasValue || x.Id == kolegijId).Select(x => x.Id).ToHashSet();
        var pocetak = dana is 7 or 30 ? DateOnly.FromDateTime(DateTime.Today.AddDays(-dana.Value)) : (DateOnly?)null;

        var aktivnosti = await (from a in db.Aktivnosti
                                join k in db.Kolegiji on a.KolegijId equals k.Id
                                join v in db.VrsteAktivnosti on a.VrstaId equals v.Id
                                where ids.Contains(a.KolegijId)
                                select new { a.Id, a.KolegijId, a.Naziv, a.Datum, a.MaxBodovi, vrsta = v.Naziv, kolegij = k.Naziv }).ToListAsync();
        var evidencije = await (from e in db.Evidencije
                                join s in db.Studenti on e.StudentId equals s.KorisnikId
                                join u in db.Korisnici on s.KorisnikId equals u.Id
                                where ids.Contains(e.KolegijId)
                                select new { e.StudentId, e.KolegijId, e.AktivnostId, e.Bodovi, e.Status, ime = u.Ime + " " + u.Prezime }).ToListAsync();
        var upisi = await db.Upisi.Where(x => ids.Contains(x.KolegijId)).ToListAsync();
        var studentNames = await (from s in db.Studenti
                                  join u in db.Korisnici on s.KorisnikId equals u.Id
                                  where upisi.Select(x => x.StudentId).Contains(s.KorisnikId)
                                  select new { s.KorisnikId, ime = u.Ime + " " + u.Prezime })
            .ToDictionaryAsync(x => x.KorisnikId, x => x.ime);

        if (pocetak.HasValue)
            aktivnosti = aktivnosti.Where(x => !x.Datum.HasValue || x.Datum >= pocetak).ToList();

        var comparison = upisi.GroupBy(x => x.StudentId).Select(group =>
        {
            var studentEvidencije = evidencije.Where(x => x.StudentId == group.Key).ToList();
            var studentKolegiji = kolegiji.Where(x => group.Any(upis => upis.KolegijId == x.Id));
            var bodovi = studentEvidencije.Sum(x => x.Bodovi ?? 0);
            var ects = studentKolegiji.Where(k => bodovi >= k.UkupnoBodova * k.PragProlaza / 100m).Sum(k => k.Ects);
            var odradeneAktivnosti = studentEvidencije.Count(x => x.Status == "odradeno");
            return new { studentId = group.Key, ime = studentNames.GetValueOrDefault(group.Key, ""), bodovi, ects, aktivnosti = odradeneAktivnosti };
        }).OrderByDescending(x => x.bodovi).ToList();

        var mjeseci = Enumerable.Range(0, 5).Select(offset => DateTime.Today.AddMonths(-4 + offset)).ToList();
        var angažman = mjeseci.Select(mjesec => new
        {
            label = mjesec.ToString("MMM", new System.Globalization.CultureInfo("hr-HR")),
            bodovi = evidencije.Where(e => e.Bodovi.HasValue && aktivnosti.Any(a => a.Id == e.AktivnostId && a.Datum?.Month == mjesec.Month && a.Datum?.Year == mjesec.Year)).Select(e => e.Bodovi!.Value).DefaultIfEmpty().Average()
        });

        var poVrsti = aktivnosti.GroupBy(x => x.vrsta).Select(group => new { naziv = group.Key, broj = group.Count() });
        var bodoviPoKolegiju = kolegiji
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Naziv)
            .Select(k =>
            {
                var brojStudenata = upisi.Where(u => u.KolegijId == k.Id).Select(u => u.StudentId).Distinct().Count();
                var ukupnoBodova = evidencije.Where(e => e.KolegijId == k.Id).Sum(e => e.Bodovi ?? 0);
                return new { k.Id, naziv = k.Naziv, ukupnoBodova, brojStudenata };
            }).ToList();

        return new
        {
            kolegiji = kolegiji.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Naziv }),
            statistika = new { kolegiji = ids.Count, studenata = upisi.Select(x => x.StudentId).Distinct().Count(), aktivnosti = aktivnosti.Count(x => x.Datum?.Month == DateTime.Today.Month) },
            najaktivniji = comparison.Take(5),
            usporedba = comparison,
            angažman,
            poVrsti,
            bodoviPoKolegiju
        };
    }

    public async Task<object?> GetStudentiRosterAsync(int kolegijId, IReadOnlyCollection<int> opsegKolegijIds)
    {
        if (!opsegKolegijIds.Contains(kolegijId)) return null;

        var roster = await (from upis in db.Upisi
                            join student in db.Studenti on upis.StudentId equals student.KorisnikId
                            join korisnik in db.Korisnici on student.KorisnikId equals korisnik.Id
                            join evidencija in db.Evidencije.Where(x => x.KolegijId == kolegijId)
                                on student.KorisnikId equals evidencija.StudentId into evidencije
                            where upis.KolegijId == kolegijId
                            select new
                            {
                                studentId = student.KorisnikId,
                                ime = korisnik.Ime,
                                prezime = korisnik.Prezime,
                                brojIndeksa = student.BrojIndeksa,
                                bodovi = evidencije.Sum(x => x.Bodovi ?? 0)
                            }).ToListAsync();

        var kolegij = await db.Kolegiji.AsNoTracking().SingleAsync(x => x.Id == kolegijId);
        var sumaMaxAktivnosti = await db.Aktivnosti.AsNoTracking()
            .Where(a => a.KolegijId == kolegijId)
            .SumAsync(a => a.MaxBodovi);
        var obavezeDefinirane = KolegijStatusHelper.SveObavezeDefiniraneNaKolegiju(sumaMaxAktivnosti, kolegij.UkupnoBodova);

        var ocijenjenoPoStudentu = await SveAktivnostiOcijenjenePoStudentimaAsync(kolegijId, roster.Select(x => x.studentId).ToList());

        return roster.Select(s => new
        {
            s.studentId,
            s.ime,
            s.prezime,
            s.brojIndeksa,
            s.bodovi,
            zavrsenoOcjenjivanje = obavezeDefinirane && ocijenjenoPoStudentu.GetValueOrDefault(s.studentId, false)
        }).ToList();
    }

    public async Task<Dictionary<int, bool>> SveAktivnostiOcijenjenePoStudentimaAsync(int kolegijId, IReadOnlyCollection<int> studentIds)
    {
        if (studentIds.Count == 0) return new Dictionary<int, bool>();

        var aktivnostIds = await db.Aktivnosti.AsNoTracking()
            .Where(a => a.KolegijId == kolegijId)
            .Select(a => a.Id)
            .ToListAsync();
        var ukupnoAktivnosti = aktivnostIds.Count;

        if (ukupnoAktivnosti == 0)
            return studentIds.ToDictionary(id => id, _ => false);

        var ocijenjeno = await db.Evidencije.AsNoTracking()
            .Where(e => e.KolegijId == kolegijId
                        && aktivnostIds.Contains(e.AktivnostId)
                        && studentIds.Contains(e.StudentId)
                        && e.Status != "ceka_se")
            .GroupBy(e => e.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Select(e => e.AktivnostId).Distinct().Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count);

        return studentIds.ToDictionary(
            id => id,
            id => ocijenjeno.GetValueOrDefault(id) == ukupnoAktivnosti);
    }

    public async Task<object?> GetKartonAsync(int studentId, int kolegijId, IReadOnlyCollection<int> opsegKolegijIds)
    {
        if (!opsegKolegijIds.Contains(kolegijId)) return null;

        var kolegij = await db.Kolegiji.AsNoTracking().SingleOrDefaultAsync(x => x.Id == kolegijId);
        if (kolegij is null) return null;

        var upisan = await db.Upisi.AnyAsync(x => x.KolegijId == kolegijId && x.StudentId == studentId);
        if (!upisan) return null;

        var student = await (from s in db.Studenti.AsNoTracking()
                             join u in db.Korisnici.AsNoTracking() on s.KorisnikId equals u.Id
                             where s.KorisnikId == studentId
                             select new { u.Ime, u.Prezime, s.BrojIndeksa }).SingleOrDefaultAsync();
        if (student is null) return null;

        var aktivnosti = await (from a in db.Aktivnosti.AsNoTracking()
                                join v in db.VrsteAktivnosti.AsNoTracking() on a.VrstaId equals v.Id
                                join e in db.Evidencije.AsNoTracking().Where(x => x.StudentId == studentId)
                                    on new { AktivnostId = a.Id, KolegijId = a.KolegijId }
                                    equals new { e.AktivnostId, e.KolegijId } into evidencije
                                from e in evidencije.DefaultIfEmpty()
                                where a.KolegijId == kolegijId
                                orderby a.Datum
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

        return new
        {
            student = new { student.Ime, student.Prezime, student.BrojIndeksa },
            kolegij = new { kolegij.Id, naziv = kolegij.Naziv, kolegij.Ects, ukupnoBodova = kolegij.UkupnoBodova, pragProlaza = kolegij.PragProlaza },
            bodovi,
            maxBodovi = kolegij.UkupnoBodova,
            prolazi,
            zavrsenoOcjenjivanje,
            aktivnosti
        };
    }

    public async Task<object?> GetSlobodniStudentiAsync(int kolegijId, IReadOnlyCollection<int> opsegKolegijIds)
    {
        if (!opsegKolegijIds.Contains(kolegijId)) return null;

        return await (from s in db.Studenti
                      join u in db.Korisnici on s.KorisnikId equals u.Id
                      where !db.Upisi.Any(upis => upis.StudentId == s.KorisnikId && upis.KolegijId == kolegijId)
                      orderby u.Prezime, u.Ime
                      select new { id = s.KorisnikId, ime = u.Ime + " " + u.Prezime, brojIndeksa = s.BrojIndeksa }).ToListAsync();
    }

    public async Task<bool> UpisiStudentaAsync(int studentId, int kolegijId, IReadOnlyCollection<int> opsegKolegijIds)
    {
        if (!opsegKolegijIds.Contains(kolegijId)) return false;
        if (await db.Upisi.AnyAsync(x => x.StudentId == studentId && x.KolegijId == kolegijId)) return true;
        db.Upisi.Add(new Upis { StudentId = studentId, KolegijId = kolegijId });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UkloniStudentaAsync(int kolegijId, int studentId, IReadOnlyCollection<int> opsegKolegijIds)
    {
        if (!opsegKolegijIds.Contains(kolegijId)) return false;
        var upis = await db.Upisi.SingleOrDefaultAsync(x => x.KolegijId == kolegijId && x.StudentId == studentId);
        if (upis is null) return false;
        db.Upisi.Remove(upis);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<HashSet<int>> SviKolegijIdsAsync()
        => (await db.Kolegiji.Select(x => x.Id).ToListAsync()).ToHashSet();

    public async Task<HashSet<int>> KolegijIdsZaNastavnikaAsync(int nastavnikId)
        => (await db.Kolegiji.Where(x => x.NastavnikId == nastavnikId).Select(x => x.Id).ToListAsync()).ToHashSet();
}
