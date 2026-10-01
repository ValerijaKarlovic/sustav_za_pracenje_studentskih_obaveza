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

/** Rezervno za starije odgovore; status s API-ja je statusPrikaz iz KolegijRezultatService. */
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
