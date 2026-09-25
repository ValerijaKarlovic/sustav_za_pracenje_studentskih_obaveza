export function vrijemeDatumaAktivnosti(a) {
  const raw = a?.datumRaw ?? a?.datum ?? null
  if (!raw) return null
  const t = new Date(raw).getTime()
  return Number.isNaN(t) ? null : t
}

export function usporediPoDatumu(a, b, silazno) {
  const ta = vrijemeDatumaAktivnosti(a)
  const tb = vrijemeDatumaAktivnosti(b)
  if (ta === null && tb === null) return 0
  if (ta === null) return 1
  if (tb === null) return -1
  return silazno ? tb - ta : ta - tb
}

export function sortirajAktivnostiPoDatumu(lista, poredajPo = 'datum-asc') {
  const silazno = poredajPo === 'datum-desc'
  return [...lista].sort((a, b) => usporediPoDatumu(a, b, silazno))
}
