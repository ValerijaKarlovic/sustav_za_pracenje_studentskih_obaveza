<script setup>
import { computed } from 'vue'
import { formatBodovi } from '@/utils/formatBodovi'

const props = defineProps({
  items: { type: Array, default: () => [] },
})

const STEP = 5
const ROW_HEIGHT = 38
const LABEL_WIDTH = 118
const BAR_HEIGHT = 20
const RADIUS = 4

function xMaxZaPodatke(maxValue) {
  const max = Math.max(maxValue, 0)
  if (max === 0) return 30
  return Math.ceil(max / STEP) * STEP
}

function roundedRightBarPath(x, y, w, h, r) {
  if (w <= 0) return ''
  const rad = Math.min(r, h / 2, w / 2)
  if (w < rad * 2) {
    return `M ${x} ${y} H ${x + w} V ${y + h} H ${x} Z`
  }
  return [
    `M ${x} ${y}`,
    `H ${x + w - rad}`,
    `Q ${x + w} ${y} ${x + w} ${y + rad}`,
    `V ${y + h - rad}`,
    `Q ${x + w} ${y + h} ${x + w - rad} ${y + h}`,
    `H ${x}`,
    'Z',
  ].join(' ')
}

const layout = computed(() => {
  const items = props.items.length
    ? props.items.map(i => ({ label: i.naziv, value: Number(i.vrijednost) || 0 }))
    : []

  const dataMax = items.length ? Math.max(...items.map(i => i.value), 0) : 0
  const xMax = xMaxZaPodatke(dataMax)
  const xTicks = []
  for (let v = 0; v <= xMax; v += STEP) xTicks.push(v)

  const width = 420
  const left = LABEL_WIDTH
  const right = 12
  const bottom = 26
  const top = 10
  const rowCount = Math.max(items.length, 1)
  const height = top + bottom + rowCount * ROW_HEIGHT
  const chartW = width - left - right
  const baselineX = left

  const tickLines = xTicks.map(value => ({
    value,
    x: baselineX + (value / xMax) * chartW,
  }))

  const rows = items.map((item, index) => {
    const rowY = top + index * ROW_HEIGHT
    const centerY = rowY + ROW_HEIGHT / 2
    const barW = xMax > 0 ? (item.value / xMax) * chartW : 0
    const barX = baselineX
    const barY = centerY - BAR_HEIGHT / 2
    const valueLabel = formatBodovi(item.value, item.value.toFixed(1))
    return {
      label: item.label,
      value: item.value,
      valueLabel,
      barPath: roundedRightBarPath(barX, barY, barW, BAR_HEIGHT, RADIUS),
      valueX: barX + barW + 6,
      valueY: centerY,
      labelX: LABEL_WIDTH - 8,
      labelY: centerY,
    }
  })

  return {
    width,
    height,
    xMax,
    tickLines,
    rows,
    left,
    right: width - right,
    axisY: height - bottom + 4,
    chartBottom: height - bottom,
    top,
  }
})
</script>

<template>
  <div class="bar-chart-wrap">
    <svg
      class="bar-chart"
      :viewBox="`0 0 ${layout.width} ${layout.height}`"
      preserveAspectRatio="xMidYMid meet"
    >
      <line
        v-for="tick in layout.tickLines"
        :key="'grid-' + tick.value"
        :x1="tick.x"
        :y1="layout.top"
        :x2="tick.x"
        :y2="layout.chartBottom"
        class="grid-line"
      />
      <text
        v-for="tick in layout.tickLines"
        :key="'x-' + tick.value"
        :x="tick.x"
        :y="layout.axisY"
        text-anchor="middle"
        class="x-tick"
      >
        {{ tick.value }}
      </text>
      <template v-for="row in layout.rows" :key="row.label">
        <text
          :x="row.labelX"
          :y="row.labelY"
          text-anchor="end"
          dominant-baseline="middle"
          class="y-label"
        >
          {{ row.label }}
        </text>
        <path v-if="row.barPath" :d="row.barPath" class="bar" />
        <text
          :x="row.valueX"
          :y="row.valueY"
          dominant-baseline="middle"
          class="value-label"
        >
          {{ row.valueLabel }}
        </text>
      </template>
    </svg>
  </div>
</template>

<style scoped>
.bar-chart-wrap {
  width: 100%;
  background: transparent;
}
.bar-chart {
  width: 100%;
  display: block;
}
.grid-line {
  stroke: #e8eaed;
  stroke-width: 1;
}
.x-tick {
  font-size: 10px;
  fill: #6b7280;
}
.y-label {
  font-size: 11px;
  fill: #22272e;
}
.bar {
  fill: #4a7fe8;
}
.value-label {
  font-size: 10px;
  fill: #4a5568;
}
</style>
