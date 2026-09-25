import { formatBodovi } from '@/utils/formatBodovi'

export function formatDatumAktivnosti(datum) {
  return datum ? new Date(datum).toLocaleDateString('hr-HR') : '-'
}

/** Ista meta linija kao na kartici aktivnosti (kolegij, vrsta, datum, max bodovi). */
export function metaLinijaAktivnosti(a) {
  if (!a) return ''
  return `${a.kolegij} - ${a.vrsta} - ${formatDatumAktivnosti(a.datum)} - ${formatBodovi(a.bodovi)}`
}

export function oznakaUnosaBodova(a) {
  return a?.imaUneseneBodove ? 'Pogledaj bodove' : 'Unesi bodove'
}

export function naslovModalaBodova(a) {
  if (!a?.naziv) return 'Bodovi'
  return `${oznakaUnosaBodova(a)} - ${a.naziv}`
}
