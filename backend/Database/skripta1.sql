

--  KORISNICI

CREATE TABLE korisnik (
    id                        integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ime                       varchar(100)  NOT NULL,
    prezime                   varchar(100)  NOT NULL,
    email                     varchar(255)  NOT NULL UNIQUE,
    lozinka_hash              varchar(255)  NOT NULL,            
    uloga                     varchar(20)   NOT NULL,
    mora_promijeniti_lozinku  boolean       NOT NULL DEFAULT true, 
    kreirano                  timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT korisnik_uloga_chk CHECK (uloga IN ('student', 'nastavnik', 'admin')),
    CONSTRAINT korisnik_email_lower_chk CHECK (email = lower(email))  
);


CREATE TABLE student (
    korisnik_id   integer      PRIMARY KEY REFERENCES korisnik(id) ON DELETE CASCADE,
    broj_indeksa  varchar(20)  NOT NULL UNIQUE
);


CREATE TABLE nastavnik (
    korisnik_id  integer      PRIMARY KEY REFERENCES korisnik(id) ON DELETE CASCADE,
    zvanje       varchar(60)
);


--  KOLEGIJI
CREATE TABLE kolegij (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sifra          varchar(20)   NOT NULL UNIQUE,
    naziv          varchar(150)  NOT NULL,
    ects           smallint      NOT NULL,
    ukupno_bodova  numeric(6,2)  NOT NULL,   -- postavlja nastavnik, NE zbroj aktivnosti
    prag_prolaza   smallint      NOT NULL DEFAULT 55,  
    nastavnik_id   integer       NOT NULL REFERENCES nastavnik(korisnik_id) ON DELETE RESTRICT,
    kreirano       timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT kolegij_ects_chk   CHECK (ects > 0),
    CONSTRAINT kolegij_bodovi_chk CHECK (ukupno_bodova > 0),
    CONSTRAINT kolegij_prag_chk   CHECK (prag_prolaza BETWEEN 0 AND 100)
);
CREATE INDEX ix_kolegij_nastavnik ON kolegij(nastavnik_id);

-- Upis studenta na kolegij 
CREATE TABLE upis (
    student_id  integer      NOT NULL REFERENCES student(korisnik_id) ON DELETE CASCADE,
    kolegij_id  integer      NOT NULL REFERENCES kolegij(id)          ON DELETE CASCADE,
    upisano     timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (student_id, kolegij_id)
);
CREATE INDEX ix_upis_kolegij ON upis(kolegij_id);


--  AKTIVNOSTI

-- vrste: lab. vježba, seminar, zadaca, kolokvij, projekt + proizvoljne 
CREATE TABLE vrsta_aktivnosti (
    id     integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    naziv  varchar(60) NOT NULL UNIQUE
);

CREATE TABLE aktivnost (
    id           integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    kolegij_id   integer       NOT NULL REFERENCES kolegij(id) ON DELETE CASCADE,
    vrsta_id     integer       NOT NULL REFERENCES vrsta_aktivnosti(id),
    naziv        varchar(200)  NOT NULL,
    opis         text,
    datum        date,                                   
    max_bodovi   numeric(5,2)  NOT NULL DEFAULT 0,       
    kreirano     timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT aktivnost_bodovi_chk CHECK (max_bodovi >= 0),
    CONSTRAINT aktivnost_id_kolegij_uq UNIQUE (id, kolegij_id) 
);
CREATE INDEX ix_aktivnost_kolegij ON aktivnost(kolegij_id);
CREATE INDEX ix_aktivnost_datum   ON aktivnost(datum);


--  EVIDENCIJA (status + bodovi jednog studenta na jednoj aktivnosti)


CREATE TABLE evidencija (
    aktivnost_id  integer       NOT NULL,
    student_id    integer       NOT NULL,
    kolegij_id    integer       NOT NULL,
    status        varchar(20)   NOT NULL DEFAULT 'ceka_se',
    bodovi        numeric(5,2),                          
    kreirano      timestamptz   NOT NULL DEFAULT now(),
    azurirano     timestamptz   NOT NULL DEFAULT now(),
    PRIMARY KEY (aktivnost_id, student_id),
    CONSTRAINT evidencija_status_chk CHECK (status IN ('odradeno', 'ceka_se', 'nije_odradeno')),
    CONSTRAINT evidencija_bodovi_chk CHECK (bodovi IS NULL OR bodovi >= 0),
    CONSTRAINT evidencija_aktivnost_fk FOREIGN KEY (aktivnost_id, kolegij_id)
        REFERENCES aktivnost(id, kolegij_id) ON DELETE CASCADE,
    CONSTRAINT evidencija_upis_fk FOREIGN KEY (student_id, kolegij_id)
        REFERENCES upis(student_id, kolegij_id) ON DELETE CASCADE
);
CREATE INDEX ix_evidencija_student_kolegij ON evidencija(student_id, kolegij_id);
CREATE INDEX ix_evidencija_kolegij         ON evidencija(kolegij_id);

-- Bodovi studenta ne smiju premašiti max bodove aktivnosti
CREATE FUNCTION provjeri_bodove_evidencije() RETURNS trigger AS $$
DECLARE
    v_max numeric(5,2);
BEGIN
    IF NEW.bodovi IS NOT NULL THEN
        SELECT max_bodovi INTO v_max FROM aktivnost WHERE id = NEW.aktivnost_id;
        IF NEW.bodovi > v_max THEN
            RAISE EXCEPTION 'Bodovi (%) premašuju maksimum aktivnosti (%)', NEW.bodovi, v_max;
        END IF;
    END IF;
    NEW.azurirano := now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_evidencija_bodovi
    BEFORE INSERT OR UPDATE ON evidencija
    FOR EACH ROW EXECUTE FUNCTION provjeri_bodove_evidencije();


--  VIEWOVI za dashboarde


-- Zbroj bodova po studentu/kolegiju (ECTS i status računa aplikacija u KolegijRezultatService)
CREATE VIEW v_bodovi_student_kolegij AS
SELECT
    u.student_id,
    k.id                       AS kolegij_id,
    k.naziv                    AS kolegij,
    k.ects,
    k.ukupno_bodova,
    k.prag_prolaza,
    COALESCE(ev_sum.bodovi, 0) AS bodovi
FROM upis u
JOIN kolegij k ON k.id = u.kolegij_id
LEFT JOIN LATERAL (
    SELECT SUM(e.bodovi) AS bodovi
    FROM evidencija e
    WHERE e.student_id = u.student_id
      AND e.kolegij_id = u.kolegij_id
      AND e.status <> 'ceka_se'
) ev_sum ON true;


CREATE VIEW v_napredak_student_kolegij AS
SELECT
    e.student_id,
    e.kolegij_id,
    a.datum,
    SUM(e.bodovi) OVER (
        PARTITION BY e.student_id, e.kolegij_id
        ORDER BY a.datum, a.id
    ) AS kumulativni_bodovi
FROM evidencija e
JOIN aktivnost a ON a.id = e.aktivnost_id
WHERE e.bodovi IS NOT NULL
    AND e.status <> 'ceka_se';




