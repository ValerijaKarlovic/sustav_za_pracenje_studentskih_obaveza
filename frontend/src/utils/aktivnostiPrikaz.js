export function mapObvezeZaPrikaz(aktivnosti) {
  return (aktivnosti ?? []).map(a => {
    const max = a.maxBodovi
    return {
      id: a.id,
      naziv: a.naziv,
      datum: a.datum,
      datumLabel: a.datum ? new Date(a.datum).toLocaleDateString('hr-HR') : '',
      opis: a.opis?.trim() || null,
      status: a.status === 'odradeno' ? 'Odrađeno' : a.status === 'ceka_se' ? 'Čeka se' : 'Nije odrađeno',
      bodovi: a.bodovi === null || a.bodovi === undefined ? '-' : `${a.bodovi} / ${max}`,
    }
  })
}

/** Zbroj max bodova aktivnosti na kolegiju pokriva ukupno bodova kolegija. */
export function sveObavezeDefiniraneNaKolegiju(aktivnosti, ukupnoBodovaKolegija) {
  const sumaMax = (aktivnosti ?? []).reduce((s, a) => s + Number(a.maxBodovi ?? 0), 0)
  return sumaMax >= Number(ukupnoBodovaKolegija)
}

/** Aktivnost je ocijenjena kad status u evidenciji nije „ceka_se”. */
export function sveAktivnostiOcijenjene(aktivnosti) {
  const lista = aktivnosti ?? []
  if (!lista.length) return false
  return lista.every(a => a.status !== 'ceka_se')
}

/** Sve obaveze unesene u sustav i student ocijenjen na svakoj. */
export function zavrsenoOcjenjivanje(aktivnosti, ukupnoBodovaKolegija) {
  return sveObavezeDefiniraneNaKolegiju(aktivnosti, ukupnoBodovaKolegija)
    && sveAktivnostiOcijenjene(aktivnosti)
}

/** Položeno = prag bodova + sve obaveze na kolegiju + sve ocijenjeno. */
export function kolegijJePolozen(prolazi, aktivnosti, ukupnoBodovaKolegija) {
  return Boolean(prolazi) && zavrsenoOcjenjivanje(aktivnosti, ukupnoBodovaKolegija)
}

/** ECTS ulazi u ukupno samo kad je kolegij položen. */
export function ectsOstvarenoZaKolegij(prolazi, ectsKolegija, aktivnosti, ukupnoBodovaKolegija) {
  return kolegijJePolozen(prolazi, aktivnosti, ukupnoBodovaKolegija) ? Number(ectsKolegija) : 0
}

export function statusLabelStudenta(prolazi, aktivnosti, maxBodoviKolegija) {
  if (kolegijJePolozen(prolazi, aktivnosti, maxBodoviKolegija)) return 'Položeno'
  if (zavrsenoOcjenjivanje(aktivnosti, maxBodoviKolegija)) return 'Nije položeno'
  return 'U tijeku'
}

export function statusLabelNastavnika(bodovi, maxBodovi, pragProlaza, zavrseno = false) {
  const prag = maxBodovi * (pragProlaza / 100)
  if (zavrseno && bodovi >= prag) return 'Položeno'
  if (zavrseno) return 'Nije položeno'
  return 'U tijeku'
}

export function bojaStatusa(label) {
  if (label === 'Položeno') return 'var(--green)'
  if (label === 'U tijeku') return 'var(--yellow)'
  return 'var(--red)'
}
