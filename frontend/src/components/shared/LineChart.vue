<script setup>
// Jednostavan linijski graf crtan SVG-om, bez vanjske biblioteke.
// points: niz brojeva (npr. bodovi kroz mjesece), labels: nazivi mjeseci
defineProps({
  points: { type: Array, default: () => [5, 10, 15, 20, 28] },
  labels: { type: Array, default: () => ['Ruj', 'Lis', 'Stu', 'Pro', 'Sij'] },
})

function toCoords(points) {
  const validPoints = points.map(Number).filter(Number.isFinite)
  const safePoints = validPoints.length ? validPoints : [0]
  const max = Math.max(...safePoints, 1)
  const min = Math.min(...safePoints, 0)
  const range = max - min || 1
  const stepX = safePoints.length > 1 ? 380 / (safePoints.length - 1) : 0
  return safePoints.map((p, i) => {
    const x = 10 + i * stepX
    const y = 130 - ((p - min) / range) * 110
    return `${x},${y}`
  })
}
</script>

<template>
  <div>
    <svg class="chart" viewBox="0 0 400 140" preserveAspectRatio="none">
      <polyline fill="none" stroke="#2f5fd1" stroke-width="2.5" :points="toCoords(points).join(' ')" />
      <circle v-for="(c, i) in toCoords(points)" :key="i"
        :cx="c.split(',')[0]" :cy="c.split(',')[1]" r="4" fill="#2f5fd1" />
    </svg>
    <div class="chart-labels">
      <span v-for="(l, i) in labels" :key="i">{{ l }}</span>
    </div>
  </div>
</template>
