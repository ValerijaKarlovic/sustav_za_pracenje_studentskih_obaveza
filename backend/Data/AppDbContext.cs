using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Korisnik> Korisnici => Set<Korisnik>();
    public DbSet<Student> Studenti => Set<Student>();
    public DbSet<Nastavnik> Nastavnici => Set<Nastavnik>();
    public DbSet<Kolegij> Kolegiji => Set<Kolegij>();
    public DbSet<Upis> Upisi => Set<Upis>();
    public DbSet<VrstaAktivnosti> VrsteAktivnosti => Set<VrstaAktivnosti>();
    public DbSet<Aktivnost> Aktivnosti => Set<Aktivnost>();
    public DbSet<Evidencija> Evidencije => Set<Evidencija>();
    public DbSet<BodoviStudentKolegij> BodoviStudentKolegij => Set<BodoviStudentKolegij>();
    public DbSet<NapredakStudentKolegij> NapredakStudentKolegij => Set<NapredakStudentKolegij>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Korisnik>().ToTable("korisnik").HasKey(x => x.Id);
        modelBuilder.Entity<Student>().ToTable("student").HasKey(x => x.KorisnikId);
        modelBuilder.Entity<Nastavnik>().ToTable("nastavnik").HasKey(x => x.KorisnikId);
        modelBuilder.Entity<Kolegij>().ToTable("kolegij").HasKey(x => x.Id);
        modelBuilder.Entity<Upis>().ToTable("upis").HasKey(x => new { x.StudentId, x.KolegijId });
        modelBuilder.Entity<VrstaAktivnosti>().ToTable("vrsta_aktivnosti").HasKey(x => x.Id);
        modelBuilder.Entity<Aktivnost>().ToTable("aktivnost").HasKey(x => x.Id);
        modelBuilder.Entity<Evidencija>().ToTable("evidencija").HasKey(x => new { x.AktivnostId, x.StudentId });

        modelBuilder.Entity<BodoviStudentKolegij>().HasNoKey().ToView("v_bodovi_student_kolegij");
        modelBuilder.Entity<NapredakStudentKolegij>().HasNoKey().ToView("v_napredak_student_kolegij");

        modelBuilder.Entity<Korisnik>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Korisnik>().Property(x => x.Ime).HasColumnName("ime");
        modelBuilder.Entity<Korisnik>().Property(x => x.Prezime).HasColumnName("prezime");
        modelBuilder.Entity<Korisnik>().Property(x => x.Email).HasColumnName("email");
        modelBuilder.Entity<Korisnik>().Property(x => x.LozinkaHash).HasColumnName("lozinka_hash");
        modelBuilder.Entity<Korisnik>().Property(x => x.Uloga).HasColumnName("uloga");
        modelBuilder.Entity<Korisnik>().Property(x => x.MoraPromijenitiLozinku).HasColumnName("mora_promijeniti_lozinku");
        modelBuilder.Entity<Korisnik>().Property<DateTimeOffset>("kreirano").HasColumnName("kreirano");

        modelBuilder.Entity<Student>().Property(x => x.KorisnikId).HasColumnName("korisnik_id");
        modelBuilder.Entity<Student>().Property(x => x.BrojIndeksa).HasColumnName("broj_indeksa");
        modelBuilder.Entity<Nastavnik>().Property(x => x.KorisnikId).HasColumnName("korisnik_id");
        modelBuilder.Entity<Nastavnik>().Property(x => x.Zvanje).HasColumnName("zvanje");

        modelBuilder.Entity<Kolegij>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Kolegij>().Property(x => x.Sifra).HasColumnName("sifra");
        modelBuilder.Entity<Kolegij>().Property(x => x.Naziv).HasColumnName("naziv");
        modelBuilder.Entity<Kolegij>().Property(x => x.Ects).HasColumnName("ects");
        modelBuilder.Entity<Kolegij>().Property(x => x.UkupnoBodova).HasColumnName("ukupno_bodova");
        modelBuilder.Entity<Kolegij>().Property(x => x.PragProlaza).HasColumnName("prag_prolaza");
        modelBuilder.Entity<Kolegij>().Property(x => x.NastavnikId).HasColumnName("nastavnik_id");
        modelBuilder.Entity<Upis>().Property(x => x.StudentId).HasColumnName("student_id");
        modelBuilder.Entity<Upis>().Property(x => x.KolegijId).HasColumnName("kolegij_id");
        modelBuilder.Entity<VrstaAktivnosti>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<VrstaAktivnosti>().Property(x => x.Naziv).HasColumnName("naziv");

        modelBuilder.Entity<Aktivnost>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Aktivnost>().Property(x => x.KolegijId).HasColumnName("kolegij_id");
        modelBuilder.Entity<Aktivnost>().Property(x => x.VrstaId).HasColumnName("vrsta_id");
        modelBuilder.Entity<Aktivnost>().Property(x => x.Naziv).HasColumnName("naziv");
        modelBuilder.Entity<Aktivnost>().Property(x => x.Opis).HasColumnName("opis");
        modelBuilder.Entity<Aktivnost>().Property(x => x.Datum).HasColumnName("datum");
        modelBuilder.Entity<Aktivnost>().Property(x => x.MaxBodovi).HasColumnName("max_bodovi");
        modelBuilder.Entity<Aktivnost>().Property<DateTimeOffset>("kreirano").HasColumnName("kreirano");
        modelBuilder.Entity<Evidencija>().Property(x => x.AktivnostId).HasColumnName("aktivnost_id");
        modelBuilder.Entity<Evidencija>().Property(x => x.StudentId).HasColumnName("student_id");
        modelBuilder.Entity<Evidencija>().Property(x => x.KolegijId).HasColumnName("kolegij_id");
        modelBuilder.Entity<Evidencija>().Property(x => x.Status).HasColumnName("status");
        modelBuilder.Entity<Evidencija>().Property(x => x.Bodovi).HasColumnName("bodovi");

        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.StudentId).HasColumnName("student_id");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.KolegijId).HasColumnName("kolegij_id");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.Kolegij).HasColumnName("kolegij");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.Ects).HasColumnName("ects");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.UkupnoBodova).HasColumnName("ukupno_bodova");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.PragProlaza).HasColumnName("prag_prolaza");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.Bodovi).HasColumnName("bodovi");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.EctsOstvareno).HasColumnName("ects_ostvareno");
        modelBuilder.Entity<BodoviStudentKolegij>().Property(x => x.Prolazi).HasColumnName("prolazi");
        modelBuilder.Entity<NapredakStudentKolegij>().Property(x => x.StudentId).HasColumnName("student_id");
        modelBuilder.Entity<NapredakStudentKolegij>().Property(x => x.KolegijId).HasColumnName("kolegij_id");
        modelBuilder.Entity<NapredakStudentKolegij>().Property(x => x.Datum).HasColumnName("datum");
        modelBuilder.Entity<NapredakStudentKolegij>().Property(x => x.KumulativniBodovi).HasColumnName("kumulativni_bodovi");

        modelBuilder.Entity<Upis>().HasOne<Student>().WithMany().HasForeignKey(x => x.StudentId);
        modelBuilder.Entity<Upis>().HasOne<Kolegij>().WithMany().HasForeignKey(x => x.KolegijId);
    }
}