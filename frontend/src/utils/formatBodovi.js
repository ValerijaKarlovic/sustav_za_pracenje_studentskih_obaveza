/** Hrvatski padež: 1 bod, 3 boda, 6 bodova (uključujući 11, 12–14, 21, 22…). */
export function oblikBoda(broj) {
  const n = Math.abs(Math.trunc(Number(broj)))
  if (!Number.isFinite(Number(broj))) return 'bodova'

  const mod10 = n % 10
  const mod100 = n % 100

  if (mod10 === 1 && mod100 !== 11) return 'bod'
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return 'boda'
  return 'bodova'
}

/** npr. formatBodovi(6) → "6 bodova"; drugi argument = prikaz broja (npr. toFixed). */
export function formatBodovi(broj, prikazBroja) {
  if (broj === null || broj === undefined || broj === '') return '-'
  const num = Number(broj)
  if (Number.isNaN(num)) return '-'
  const tekst = prikazBroja !== undefined ? String(prikazBroja) : String(broj)
  return `${tekst} ${oblikBoda(num)}`
}
