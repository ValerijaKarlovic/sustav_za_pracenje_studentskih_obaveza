




CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Korisnici
INSERT INTO korisnik (ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku)
SELECT v.ime, v.prezime, v.email, crypt('test123', gen_salt('bf', 11)), v.uloga, false
FROM (VALUES
    ('Ana',    'Marić',         'ana.maric@fakultet.hr',    'student'),
    ('Petra',  'Babić',         'petra.babic@fakultet.hr',  'student'),
    ('Luka',   'Perić',         'luka.peric@fakultet.hr',   'student'),
    ('Ivan',   'Kovač',         'ivan.kovac@fakultet.hr',   'student'),
    ('Ema',    'Novak',         'ema.novak@fakultet.hr',    'student'),
    ('Marko',  'Horvat',        'marko.horvat@fakultet.hr', 'nastavnik'),
    ('Ivana',  'Kralj',         'ivana.kralj@fakultet.hr',  'nastavnik'),
    ('Sustav', 'Administrator', 'admin@fakultet.hr',        'admin')
) AS v(ime, prezime, email, uloga);

INSERT INTO student (korisnik_id, broj_indeksa)
SELECT k.id, v.indeks
FROM (VALUES
    ('ana.maric@fakultet.hr',   '0123456789'),
    ('petra.babic@fakultet.hr', '0123456790'),
    ('luka.peric@fakultet.hr',  '0123456791'),
    ('ivan.kovac@fakultet.hr',  '0123456792'),
    ('ema.novak@fakultet.hr',   '0123456793')
) AS v(email, indeks)
JOIN korisnik k ON k.email = v.email;

INSERT INTO nastavnik (korisnik_id, zvanje)
SELECT k.id, v.zvanje
FROM (VALUES
    ('marko.horvat@fakultet.hr', 'Docent'),
    ('ivana.kralj@fakultet.hr',  'Izvanredni profesor')
) AS v(email, zvanje)
JOIN korisnik k ON k.email = v.email;

-- Kolegiji
INSERT INTO kolegij (sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id)
SELECT v.sifra, v.naziv, v.ects, v.bodovi, v.prag, k.id
FROM (VALUES
    ('PRG2', 'Programiranje 2', 5, 100, 55, 'marko.horvat@fakultet.hr'),
    ('UXD',  'UX Dizajn',       3,  50, 50, 'marko.horvat@fakultet.hr'),
    ('BP',   'Baze podataka',   6,  60, 55, 'ivana.kralj@fakultet.hr')
) AS v(sifra, naziv, ects, bodovi, prag, email)
JOIN korisnik k ON k.email = v.email;

-- Upisi
INSERT INTO upis (student_id, kolegij_id)
SELECT k.id, ko.id
FROM (VALUES
    ('ana.maric@fakultet.hr',   'PRG2'),
    ('ana.maric@fakultet.hr',   'UXD'),
    ('ana.maric@fakultet.hr',   'BP'),
    ('petra.babic@fakultet.hr', 'PRG2'),
    ('luka.peric@fakultet.hr',  'PRG2'),
    ('luka.peric@fakultet.hr',  'BP'),
    ('ivan.kovac@fakultet.hr',  'UXD'),
    ('ema.novak@fakultet.hr',   'UXD')
) AS v(email, sifra)
JOIN korisnik k ON k.email = v.email
JOIN kolegij ko ON ko.sifra = v.sifra;

-- Vrste aktivnosti
INSERT INTO vrsta_aktivnosti (naziv) VALUES
    ('Lab. vježba'), ('Seminar'), ('Zadaća'), ('Kolokvij'), ('Projekt');

-- Aktivnosti
INSERT INTO aktivnost (kolegij_id, vrsta_id, naziv, datum, max_bodovi)
SELECT ko.id, va.id, v.naziv, v.datum::date, v.bodovi
FROM (VALUES
    ('PRG2', 'Zadaća',      'Zadaća 1',                 '2026-09-18', 2),
    ('PRG2', 'Kolokvij',    'Kolokvij 1',               '2026-09-25', 6),
    ('PRG2', 'Lab. vježba', 'Lab vježba 3',             '2026-10-10', 2),
    ('PRG2', 'Zadaća',      'Zadaća 2',                 '2026-10-05', 3),
    ('UXD',  'Seminar',     'Seminar: UX istraživanje', '2026-10-15', 4),
    ('BP',   'Zadaća',      'Zadaća 2',                 '2026-10-20', 3)
) AS v(sifra, vrsta, naziv, datum, bodovi)
JOIN kolegij ko ON ko.sifra = v.sifra
JOIN vrsta_aktivnosti va ON va.naziv = v.vrsta;

-- Evidencije
INSERT INTO evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi)
SELECT a.id, s.id, ko.id, v.status, v.bodovi::numeric
FROM (VALUES
    ('ana.maric@fakultet.hr',   'PRG2', 'Zadaća 1',                 'odradeno',      2),
    ('ana.maric@fakultet.hr',   'PRG2', 'Kolokvij 1',               'odradeno',      6),
    ('ana.maric@fakultet.hr',   'PRG2', 'Lab vježba 3',             'odradeno',      2),
    ('ana.maric@fakultet.hr',   'PRG2', 'Zadaća 2',                 'nije_odradeno', 0),
    ('ana.maric@fakultet.hr',   'UXD',  'Seminar: UX istraživanje', 'ceka_se',       NULL),
    ('ana.maric@fakultet.hr',   'BP',   'Zadaća 2',                 'nije_odradeno', 0),
    ('petra.babic@fakultet.hr', 'PRG2', 'Lab vježba 3',             'odradeno',      2),
    ('petra.babic@fakultet.hr', 'PRG2', 'Kolokvij 1',               'odradeno',      6),
    ('luka.peric@fakultet.hr',  'PRG2', 'Lab vježba 3',             'odradeno',      2),
    ('luka.peric@fakultet.hr',  'PRG2', 'Kolokvij 1',               'nije_odradeno', 0),
    ('luka.peric@fakultet.hr',  'BP',   'Zadaća 2',                 'nije_odradeno', 0),
    ('ivan.kovac@fakultet.hr',  'UXD',  'Seminar: UX istraživanje', 'ceka_se',       NULL)
) AS v(email, sifra, aktivnost, status, bodovi)
JOIN korisnik s  ON s.email = v.email
JOIN kolegij ko  ON ko.sifra = v.sifra
JOIN aktivnost a ON a.kolegij_id = ko.id AND a.naziv = v.aktivnost;