<script setup>
import { computed } from 'vue'
import Modal from './Modal.vue'
import StatCard from './StatCard.vue'
import Badge from './Badge.vue'
import { donutGradient, segmentiDonutaIzListeAktivnosti } from '@/utils/bodoviPoVrsti'
import { bojaStatusa, mapObvezeZaPrikaz } from '@/utils/aktivnostiPrikaz'
import { sortirajAktivnostiPoDatumu } from '@/utils/sortiranjeAktivnosti'
import { formatBodovi } from '@/utils/formatBodovi'

const props = defineProps({
  courseName: { type: String, required: true },
  subtitle: { type: String, default: 'Pregled obaveza i napretka' },
  bodovi: { type: [Number, String], required: true },
  maxBodovi: { type: [Number, String], required: true },
  statusLabel: { type: String, required: true },
  aktivnostiKolegija: { type: Array, default: () => [] },
})

defineEmits(['close'])

const strukturaBodovaKolegija = computed(() =>
  segmentiDonutaIzListeAktivnosti(props.aktivnostiKolegija),
)

const obvezeSortirane = computed(() =>
  mapObvezeZaPrikaz(sortirajAktivnostiPoDatumu(props.aktivnostiKolegija, 'datum-asc')),
)

</script>

<template>
  <Modal :title="courseName" :subtitle="subtitle" @close="$emit('close')">
    <div class="stats-row">
      <StatCard label="Ukupno bodova" :value="`${bodovi} / ${maxBodovi}`" />
      <StatCard label="Status">
        <span :style="{ color: bojaStatusa(statusLabel), fontSize: '15px' }">
          {{ statusLabel }}
        </span>
      </StatCard>
    </div>

    <div class="section-title">Obaveze na kolegiju</div>
    <div class="card">
      <div class="rank-row obveza-row" v-for="o in obvezeSortirane" :key="o.id ?? o.naziv">
        <div class="rank-name obveza-main">
          <div class="obveza-title-line">
            <span class="obveza-naziv">{{ o.naziv }}</span>
            <span v-if="o.datumLabel" class="obveza-datum">{{ o.datumLabel }}</span>
          </div>
          <p v-if="o.opis" class="activity-opis obveza-opis-text">{{ o.opis }}</p>
        </div>
        <Badge :status="o.status" />
        <div class="rank-points">{{ o.bodovi }}</div>
      </div>
    </div>

    <div class="section-title">Struktura bodova na kolegiju</div>
    <div class="card">
      <div class="pie-wrap">
        <div class="pie-chart donut" :style="{ background: donutGradient(strukturaBodovaKolegija) }"></div>
        <div class="pie-legend">
          <div class="legend-item" v-for="v in strukturaBodovaKolegija" :key="v.vrsta">
            <span class="legend-dot" :style="{ background: v.boja }"></span>{{ v.naziv }}
            <span class="legend-pct">{{ formatBodovi(v.bodovi) }} - {{ v.postotak }}%</span>
          </div>
        </div>
      </div>
    </div>
  </Modal>
</template>

<style scoped>
.obveza-row {
  align-items: flex-start;
}
.obveza-main {
  flex: 1;
  min-width: 0;
}
.obveza-title-line {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}
.obveza-naziv {
  font-weight: 500;
}
.obveza-datum {
  font-size: 12px;
  color: var(--muted);
}
.obveza-opis-text {
  margin: 6px 0 0;
  font-size: 12.5px;
  color: var(--muted);
  line-height: 1.45;
  white-space: pre-wrap;
}
</style>
