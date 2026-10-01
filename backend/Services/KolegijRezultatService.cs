using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class KolegijRezultatService(AppDbContext db)
{
    public KolegijRezultatDto Izracunaj(
        Kolegij kolegij,
        IReadOnlyList<(decimal MaxBodovi, string Status, decimal? Bodovi)> stavke)
    {
        var bodovi = stavke
            .Where(s => s.Status != "ceka_se")
            .Sum(s => s.Bodovi ?? 0);
        var sumaMax = stavke.Sum(s => s.MaxBodovi);
        var statusi = stavke.Select(s => s.Status).ToList();

        var pragBodova = kolegij.UkupnoBodova * kolegij.PragProlaza / 100m;
        var prolaziPrag = KolegijStatusHelper.ProlaziPrag(bodovi, kolegij.UkupnoBodova, kolegij.PragProlaza);
        var sveObveze = KolegijStatusHelper.SveObavezeDefiniraneNaKolegiju(sumaMax, kolegij.UkupnoBodova);
        var sveOcijenjeno = statusi.Count > 0 && KolegijStatusHelper.SveAktivnostiOcijenjene(statusi);
        var zavrseno = KolegijStatusHelper.ZavrsenoOcjenjivanje(sumaMax, kolegij.UkupnoBodova, statusi);
        var polozen = KolegijStatusHelper.KolegijJePolozen(
            bodovi, kolegij.UkupnoBodova, kolegij.PragProlaza, sumaMax, statusi);
        var ectsOstvareno = KolegijStatusHelper.EctsOstvareno(polozen, kolegij.Ects);
        var statusPrikaz = KolegijStatusHelper.StatusPrikaz(polozen, zavrseno);

        return new KolegijRezultatDto(
            bodovi,
            kolegij.UkupnoBodova,
            pragBodova,
            prolaziPrag,
            sveObveze,
            sveOcijenjeno,
            zavrseno,
            polozen,
            ectsOstvareno,
            statusPrikaz);
    }

    public KolegijRezultatDto Izracunaj(Kolegij kolegij, IReadOnlyList<AktivnostOdgovor> aktivnosti)
        => Izracunaj(kolegij, aktivnosti.Select(a => (a.MaxBodovi, a.Status, a.Bodovi)).ToList());

    public async Task<IReadOnlyList<StudentKolegijListOdgovor>> ListZaStudentaAsync(int studentId)
    {
        var kolegijIds = await db.Upisi.AsNoTracking()
            .Where(u => u.StudentId == studentId)
            .Select(u => u.KolegijId)
            .ToListAsync();
        if (kolegijIds.Count == 0) return [];

        var kolegiji = await db.Kolegiji.AsNoTracking()
            .Where(k => kolegijIds.Contains(k.Id))
            .OrderBy(k => k.Naziv)
            .ToListAsync();

        var aktivnosti = await db.Aktivnosti.AsNoTracking()
            .Where(a => kolegijIds.Contains(a.KolegijId))
            .ToListAsync();
        var evidencije = await db.Evidencije.AsNoTracking()
            .Where(e => e.StudentId == studentId && kolegijIds.Contains(e.KolegijId))
            .ToListAsync();

        return kolegiji.Select(k =>
        {
            var stavke = aktivnosti
                .Where(a => a.KolegijId == k.Id)
                .Select(a =>
                {
                    var ev = evidencije.FirstOrDefault(e => e.AktivnostId == a.Id);
                    return (a.MaxBodovi, ev?.Status ?? "ceka_se", ev?.Bodovi);
                })
                .ToList();
            var rez = Izracunaj(k, stavke);
            return new StudentKolegijListOdgovor(
                k.Id,
                k.Naziv,
                k.Ects,
                k.UkupnoBodova,
                k.PragProlaza,
                rez.Bodovi,
                rez.EctsOstvareno,
                rez.ProlaziPrag,
                rez.Polozen,
                rez.StatusPrikaz);
        }).ToList();
    }

    public async Task<(Kolegij Kolegij, string Nastavnik, IReadOnlyList<AktivnostOdgovor> Aktivnosti, KolegijRezultatDto Rezultat)?> DetaljZaStudentaAsync(
        int studentId,
        int kolegijId)
    {
        var upis = await db.Upisi.AsNoTracking()
            .SingleOrDefaultAsync(x => x.StudentId == studentId && x.KolegijId == kolegijId);
        if (upis is null) return null;

        var kolegij = await db.Kolegiji.AsNoTracking().SingleAsync(x => x.Id == kolegijId);
        var nastavnik = await db.Korisnici.AsNoTracking()
            .Where(n => n.Id == kolegij.NastavnikId)
            .Select(n => n.Ime + " " + n.Prezime)
            .SingleOrDefaultAsync() ?? "";

        var aktivnosti = await (from a in db.Aktivnosti.AsNoTracking()
                                join v in db.VrsteAktivnosti.AsNoTracking() on a.VrstaId equals v.Id
                                join e in db.Evidencije.AsNoTracking().Where(x => x.StudentId == studentId)
                                    on new { AktivnostId = a.Id, KolegijId = a.KolegijId }
                                    equals new { e.AktivnostId, e.KolegijId } into evidencije
                                from e in evidencije.DefaultIfEmpty()
                                where a.KolegijId == kolegijId
                                orderby a.Datum
                                select new AktivnostOdgovor(a.Id, a.Naziv, a.Datum, a.MaxBodovi,
                                    e == null ? "ceka_se" : e.Status, e == null ? null : e.Bodovi, v.Naziv, a.Opis))
            .ToListAsync();

        var rezultat = Izracunaj(kolegij, aktivnosti);
        return (kolegij, nastavnik, aktivnosti, rezultat);
    }
}
