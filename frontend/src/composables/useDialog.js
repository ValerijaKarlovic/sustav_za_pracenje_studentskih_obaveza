import { ref } from 'vue'

export function useDialog() {
  const dialog = ref({ open: false, type: 'alert', message: '', resolve: null })

  function zatvori(result) {
    const resolve = dialog.value.resolve
    dialog.value = { open: false, type: 'alert', message: '', resolve: null }
    resolve?.(result)
  }

  function obavijest(message) {
    return new Promise(resolve => {
      dialog.value = { open: true, type: 'alert', message, resolve }
    })
  }

  function potvrdi(message) {
    return new Promise(resolve => {
      dialog.value = { open: true, type: 'confirm', message, resolve }
    })
  }

  return {
    dialog,
    obavijest,
    potvrdi,
    potvrdiDialog: () => zatvori(true),
    odustaniDialog: () => zatvori(false),
  }
}
