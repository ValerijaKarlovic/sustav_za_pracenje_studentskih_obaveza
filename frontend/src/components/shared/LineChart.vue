<script setup>
import { computed } from 'vue'

const props = defineProps({
  points: { type: Array, default: () => [5, 10, 15, 20, 28] },
  labels: { type: Array, default: () => ['Ruj', 'Lis', 'Stu', 'Pro', 'Sij'] },
})

const coords = computed(() => {
  const validPoints = props.points.map(Number).filter(Number.isFinite)
  const safePoints = validPoints.length ? validPoints : [0]
  const max = Math.max(...safePoints, 1)
  const min = Math.min(...safePoints, 0)
  const range = max - min || 1
  const leftPadding = 30
  const rightPadding = 30
  const drawableWidth = 400 - leftPadding - rightPadding
  const stepX = safePoints.length > 1 ? drawableWidth / (safePoints.length - 1) : 0
  return safePoints.map((p, i) => {
    const x = safePoints.length > 1
      ? leftPadding + i * stepX
      : 200
    const y = 130 - ((p - min) / range) * 110
    return { x, y, value: p }
  })
})

const polylinePoints = computed(() => coords.value.map(c => `${c.x},${c.y}`).join(' '))

function formatBroj(n) {
  const x = Number(n)
  if (!Number.isFinite(x)) return '0'
  return Number.isInteger(x) ? String(x) : x.toFixed(1)
}
</script>

<template>
  <div>
    <svg class="chart" viewBox="0 0 400 140" preserveAspectRatio="none">
      <polyline fill="none" stroke="#2f5fd1" stroke-width="2.5" :points="polylinePoints" />
      <g v-for="(c, i) in coords" :key="i">
        <circle :cx="c.x" :cy="c.y" r="4" fill="#2f5fd1" />
        <text
          :x="c.x"
          :y="c.y - 8"
          text-anchor="middle"
          font-size="11"
          fill="#1a3d8f"
          font-weight="600"
        >
          {{ formatBroj(c.value) }}
        </text>
        <text
          :x="c.x"
          y="138"
          text-anchor="middle"
          font-size="9"
          fill="#202020"
        >
          {{ labels[i] }}
        </text>
      </g>
    </svg>
  </div>
</template>
