const KATEGORIJE = [
  { id: 'kolokvij', label: 'Kolokviji', boja: '#c62828', match: (vrsta) => /kolokvij/i.test(vrsta) },
  { id: 'zadaca', label: 'Zadaće', boja: '#d99a1f', match: (vrsta) => /zadać|zadac/i.test(vrsta) },
  { id: 'lab', label: 'Lab. vježbe', boja: '#2f5fd1', match: (vrsta) => /laboratorij|lab/i.test(vrsta) },
  { id: 'seminar', label: 'Seminari', boja: '#1a7f37', match: (vrsta) => /seminar/i.test(vrsta) },
]

const REZERVNE_BOJE = ['#7c3aed', '#0891b2', '#be185d', '#4d7c0f']

function redoslijedVrste(vrsta) {
  const idx = KATEGORIJE.findIndex(k => k.match(vrsta || ''))
  return idx === -1 ? KATEGORIJE.length : idx
}

export function prikazNazivaVrste(vrsta) {
  return KATEGORIJE.find(k => k.match(vrsta || ''))?.label ?? vrsta
}

function kategorijaZaTekst(vrsta, naziv = '') {
  return KATEGORIJE.find(k => k.match(vrsta || '') || k.match(naziv || ''))
}

/** Grupni ključ za aktivnost (tip: lab, zadaca, … ili sirovi naziv vrste). */
export function grupniKljucAktivnosti(a) {
  const vrsta = a.vrsta ?? a.Vrsta ?? ''
  const kategorija = kategorijaZaTekst(vrsta, a.naziv)
  if (kategorija) return kategorija.id
  return vrsta || a.naziv || 'ostalo'
}

function redoslijedGrupe(kljuc) {
  const idx = KATEGORIJE.findIndex(k => k.id === kljuc)
  return idx === -1 ? KATEGORIJE.length : idx
}

/**
 * Donut segmenti iz iste liste aktivnosti kao u modalu kolegija.
 * Grupira po tipu (Lab. vježbe, Zadaće, …); tip s 0 bodova ostaje u legendi.
 */
export function segmentiDonutaIzListeAktivnosti(aktivnosti) {
  if (!aktivnosti?.length) return []

  const kljucevi = [...new Set(aktivnosti.map(grupniKljucAktivnosti))].sort((a, b) => {
    const razlika = redoslijedGrupe(a) - redoslijedGrupe(b)
    if (razlika !== 0) return razlika
    return String(a).localeCompare(String(b), 'hr')
  })

  const bodoviPoGrupi = Object.fromEntries(kljucevi.map(k => [k, 0]))
  aktivnosti.forEach(a => {
    const kljuc = grupniKljucAktivnosti(a)
    if (!(kljuc in bodoviPoGrupi)) return
    const bodovi = a.bodovi ?? a.Bodovi
    if (bodovi === null || bodovi === undefined) return
    bodoviPoGrupi[kljuc] += Number(bodovi)
  })

  const ukupnoOstvarenih = Object.values(bodoviPoGrupi).reduce((sum, b) => sum + b, 0)

  return kljucevi.map((kljuc, index) => {
    const kategorija = KATEGORIJE.find(k => k.id === kljuc)
    const naziv = kategorija?.label ?? prikazNazivaVrste(kljuc)
    const boja = kategorija?.boja ?? bojaZaVrstu(kljuc, index)
    const bodovi = bodoviPoGrupi[kljuc]
    return {
      vrsta: kljuc,
      naziv,
      boja,
      bodovi,
      postotak: ukupnoOstvarenih ? Math.round(bodovi / ukupnoOstvarenih * 100) : 0,
    }
  })
}

export function bojaZaVrstu(vrsta, index = 0) {
  const kategorija = KATEGORIJE.find(k => k.match(vrsta || ''))
  if (kategorija) return kategorija.boja
  return REZERVNE_BOJE[index % REZERVNE_BOJE.length]
}

/** Jedinstvene vrste obaveza koje profesor/sustav ima na kolegiju (iz popisa aktivnosti). */
export function vrsteObavezaIzAktivnosti(aktivnosti) {
  const vrste = aktivnosti.map(a => a.vrsta).filter(Boolean)
  return sortirajVrste(vrste)
}

export function sortirajVrste(vrste) {
  return [...new Set(vrste)].sort((a, b) => {
    const razlika = redoslijedVrste(a) - redoslijedVrste(b)
    if (razlika !== 0) return razlika
    return a.localeCompare(b, 'hr')
  })
}

/**
 * Segmenti za donut: samo proslijeđene vrste (obaveze na kolegiju / upisanim kolegijima).
 * Vrsta s 0 bodova ostaje u listi s postotkom 0.
 */
export function segmentiDonutaPoVrstama(aktivnosti, dozvoljeneVrste) {
  const vrste = sortirajVrste(dozvoljeneVrste)
  if (!vrste.length) return []

  const bodoviPoVrsti = Object.fromEntries(vrste.map(v => [v, 0]))
  aktivnosti.forEach(a => {
    if (!a.vrsta || !(a.vrsta in bodoviPoVrsti)) return
    if (a.bodovi === null || a.bodovi === undefined) return
    bodoviPoVrsti[a.vrsta] += Number(a.bodovi)
  })

  const ukupnoOstvarenih = Object.values(bodoviPoVrsti).reduce((sum, bodovi) => sum + bodovi, 0)

  return vrste.map((vrsta, index) => ({
    vrsta,
    naziv: prikazNazivaVrste(vrsta),
    boja: bojaZaVrstu(vrsta, index),
    bodovi: bodoviPoVrsti[vrsta],
    postotak: ukupnoOstvarenih ? Math.round(bodoviPoVrsti[vrsta] / ukupnoOstvarenih * 100) : 0,
  }))
}

export function donutGradient(segmenti) {
  const aktivni = segmenti.filter(v => v.postotak > 0)
  if (!aktivni.length) return 'conic-gradient(#e2e4e8 0% 100%)'
  let acc = 0
  const stops = aktivni.map(v => {
    const start = acc
    acc += v.postotak
    return `${v.boja} ${start}% ${acc}%`
  })
  return `conic-gradient(${stops.join(', ')})`
}
