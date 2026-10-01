SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET search_path = public, pg_catalog;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

BEGIN;

TRUNCATE TABLE public.evidencija, public.upis, public.student, public.nastavnik, public.kolegij, public.vrsta_aktivnosti, public.aktivnost, public.korisnik RESTART IDENTITY CASCADE;


INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (1, 'Ana', 'Marić', 'ana.maric@fakultet.hr', '$2a$11$S2McQmZaZnonIJTeW9DfUe6EfsoeAyRa1mnKwxUIGWpeZXwDqu83e', 'student', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (2, 'Petra', 'Babić', 'petra.babic@fakultet.hr', '$2a$11$KtUUF6ExPHH.7AaXQMTbfueu0VlBHrMU5KCKerwILU2RUmOuKScqW', 'student', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (3, 'Luka', 'Perić', 'luka.peric@fakultet.hr', '$2a$11$1EaKmfFgNI7PrbBuUdFQ.e8TXybW88bJl0NXahO1YBLHaK580bQte', 'student', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (4, 'Ivan', 'Kovač', 'ivan.kovac@fakultet.hr', '$2a$11$6GH0JiUc5eA7pqHquW1fIOpC/6l8tJFIflT0ut5Nce0wPPwXWca5m', 'student', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (7, 'Ivana', 'Kralj', 'ivana.kralj@fakultet.hr', '$2a$11$RZYi4B4s/BWO.4kUbLmQ3OuEPZB/5nGR/kODkd7xSx..x6ZhlQVGa', 'nastavnik', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (8, 'Sustav', 'Administrator', 'admin@fakultet.hr', '$2a$11$My0AAI1szunFOH8pg2Y0u.kGZ5PxgdwOfV3OgZTopqZtC8aE5N2u.', 'admin', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (6, 'Marko', 'Horvat', 'marko.horvat@fakultet.hr', '$2a$11$2L24GAVJAPL3s2JM7q5rPOR59eEsJEu2fv9HMGF.Ufz/0YF2.cBde', 'nastavnik', true, true, '2026-09-24 14:35:15.769724+02');
INSERT INTO public.korisnik (id, ime, prezime, email, lozinka_hash, uloga, mora_promijeniti_lozinku, aktivan, kreirano) OVERRIDING SYSTEM VALUE VALUES (12, 'Petra', 'Maric', 'petra.maric@fakultet.hr', '$2a$11$1E.dmY/N1ABMfvs2XUOtk.xQSpiwz3rT/v2nk1KUwqMloQ7wrmbyS', 'student', true, true, '-infinity');



INSERT INTO public.nastavnik (korisnik_id, zvanje) VALUES (6, 'Docent');
INSERT INTO public.nastavnik (korisnik_id, zvanje) VALUES (7, 'Izvanredni profesor');



INSERT INTO public.kolegij (id, sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id, kreirano) OVERRIDING SYSTEM VALUE VALUES (1, 'UXD', 'UX Dizajn', 3, 50.00, 50, 6, '2026-09-24 14:35:16.554916+02');
INSERT INTO public.kolegij (id, sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id, kreirano) OVERRIDING SYSTEM VALUE VALUES (3, 'BP', 'Baze podataka', 6, 60.00, 55, 7, '2026-09-24 14:35:16.554916+02');
INSERT INTO public.kolegij (id, sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id, kreirano) OVERRIDING SYSTEM VALUE VALUES (4, 'RI', 'Racunalno inzenjerstvo', 5, 100.00, 55, 6, '2026-09-24 16:14:10.362078+02');
INSERT INTO public.kolegij (id, sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id, kreirano) OVERRIDING SYSTEM VALUE VALUES (2, 'PRG2', 'Programiranje 2', 5, 13.00, 55, 6, '2026-09-24 14:35:16.554916+02');
INSERT INTO public.kolegij (id, sifra, naziv, ects, ukupno_bodova, prag_prolaza, nastavnik_id, kreirano) OVERRIDING SYSTEM VALUE VALUES (5, 'Sis', 'Signali i sustavi', 6, 70.00, 55, 6, '2026-09-24 19:41:16.559003+02');



INSERT INTO public.vrsta_aktivnosti (id, naziv) OVERRIDING SYSTEM VALUE VALUES (1, 'Lab. vježba');
INSERT INTO public.vrsta_aktivnosti (id, naziv) OVERRIDING SYSTEM VALUE VALUES (2, 'Seminar');
INSERT INTO public.vrsta_aktivnosti (id, naziv) OVERRIDING SYSTEM VALUE VALUES (3, 'Zadaća');
INSERT INTO public.vrsta_aktivnosti (id, naziv) OVERRIDING SYSTEM VALUE VALUES (4, 'Kolokvij');
INSERT INTO public.vrsta_aktivnosti (id, naziv) OVERRIDING SYSTEM VALUE VALUES (5, 'Projekt');



INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (8, 1, 4, 'Kolokvij 1', '2026-06-08', 20.00, '-infinity', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (2, 1, 2, 'Seminar: UX istraživanje', '2026-04-16', 4.00, '2026-09-24 14:35:16.575606+02', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (9, 5, 4, 'Kolokvij 1', '2026-03-30', 30.00, '-infinity', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (3, 3, 3, 'Zadaća 2', '2026-05-05', 3.00, '2026-09-24 14:35:16.575606+02', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (7, 5, 5, 'kontinuirani signali projekt', '2026-06-06', 10.00, '-infinity', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (6, 2, 4, 'Kolokvij 1', '2026-03-16', 6.00, '2026-09-24 14:35:16.575606+02', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (5, 2, 3, 'Zadaća 2', '2026-04-05', 3.00, '2026-09-24 14:35:16.575606+02', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (1, 2, 1, 'Lab vježba 3', '2026-04-13', 2.00, '2026-09-24 14:35:16.575606+02', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (10, 5, 4, 'kolokvij 2', '2026-05-20', 30.00, '-infinity', NULL);
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (4, 2, 3, 'Zadaća 1', '2026-03-15', 2.00, '2026-09-24 14:35:16.575606+02', 'Pokazivači: Rad s pokazivačima i memorijom');
INSERT INTO public.aktivnost (id, kolegij_id, vrsta_id, naziv, datum, max_bodovi, kreirano, opis) OVERRIDING SYSTEM VALUE VALUES (11, 4, 4, 'kolokvij 1', '2026-05-20', 20.00, '-infinity', NULL);



INSERT INTO public.student (korisnik_id, broj_indeksa) VALUES (1, '0123456789');
INSERT INTO public.student (korisnik_id, broj_indeksa) VALUES (2, '0123456790');
INSERT INTO public.student (korisnik_id, broj_indeksa) VALUES (3, '0123456791');
INSERT INTO public.student (korisnik_id, broj_indeksa) VALUES (4, '0123456792');
INSERT INTO public.student (korisnik_id, broj_indeksa) VALUES (12, '7237326');



INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (4, 1, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (1, 1, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (3, 2, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (2, 2, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (1, 2, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (1, 3, '2026-09-24 14:35:16.56245+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (2, 4, '2026-09-24 19:31:01.106505+02');
INSERT INTO public.upis (student_id, kolegij_id, upisano) VALUES (1, 5, '2026-09-24 19:41:52.408204+02');



INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (6, 3, 2, 'nije_odradeno', 0.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (1, 3, 2, 'odradeno', 2.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (6, 2, 2, 'odradeno', 6.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (1, 2, 2, 'odradeno', 2.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (5, 1, 2, 'nije_odradeno', 0.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (1, 1, 2, 'odradeno', 2.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (6, 1, 2, 'odradeno', 6.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (4, 1, 2, 'odradeno', 2.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 14:35:16.586824+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (4, 2, 2, 'odradeno', 2.00, '2026-09-24 16:25:54.155194+02', '2026-09-24 16:26:28.049154+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (2, 1, 1, 'odradeno', 4.00, '2026-09-24 14:35:16.586824+02', '2026-09-24 16:29:14.557851+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (8, 1, 1, 'odradeno', 15.00, '2026-09-25 09:50:44.667355+02', '2026-09-25 09:50:44.667355+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (8, 4, 1, 'nije_odradeno', 0.00, '2026-09-25 09:50:44.667355+02', '2026-09-25 09:50:44.667355+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (3, 1, 3, 'odradeno', 3.00, '2026-09-24 14:35:16.586824+02', '2026-09-25 16:58:11.996074+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (4, 3, 2, 'nije_odradeno', 0.00, '2026-09-24 16:25:54.155199+02', '2026-09-25 17:08:39.444222+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (5, 3, 2, 'odradeno', 1.00, '2026-09-25 17:09:02.716788+02', '2026-09-25 17:09:40.920292+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (5, 2, 2, 'odradeno', 2.00, '2026-09-25 17:09:02.716792+02', '2026-09-25 17:09:40.92031+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (7, 1, 5, 'odradeno', 5.00, '2026-09-25 17:10:20.721543+02', '2026-09-25 17:10:20.721543+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (2, 4, 1, 'nije_odradeno', 0.00, '2026-09-24 14:35:16.586824+02', '2026-09-25 17:10:31.948021+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (10, 1, 5, 'odradeno', 16.00, '2026-09-25 21:25:10.91424+02', '2026-09-25 21:25:10.91424+02');
INSERT INTO public.evidencija (aktivnost_id, student_id, kolegij_id, status, bodovi, kreirano, azurirano) VALUES (9, 1, 5, 'odradeno', 20.00, '2026-09-25 17:05:12.361479+02', '2026-09-25 21:26:44.717523+02');



SELECT pg_catalog.setval('public.aktivnost_id_seq', 11, true);



SELECT pg_catalog.setval('public.kolegij_id_seq', 5, true);



SELECT pg_catalog.setval('public.korisnik_id_seq', 12, true);



SELECT pg_catalog.setval('public.vrsta_aktivnosti_id_seq', 5, true);

COMMIT;