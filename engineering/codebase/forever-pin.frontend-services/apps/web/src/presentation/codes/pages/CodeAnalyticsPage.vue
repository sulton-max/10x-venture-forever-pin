<script setup lang="ts">
import { ArrowLeft, ChartColumn, Pencil } from 'lucide-vue-next';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Card, EmptyState, Heading, Sparkline, StatCard, Text } from '@wow-two-beta/fe/presentation/display';
import { DataRegion, SkeletonState } from '@wow-two-beta/fe/presentation/feedback';
import { ToggleGroup, ToggleInput } from '@wow-two-beta/fe/presentation/forms';
import { computed, ref } from 'vue';

import { useCode, useCodeAnalytics } from '@/application/codes';
import { ContentMode, type ScanDayDto } from '@/domain/codes';

import { AnalyticsDisplays, AnalyticsWindows, DefaultAnalyticsDays } from '../analytics/AnalyticsDisplays';
import ScanShareCard from '../analytics/ScanShareCard.vue';

/** Defines the props of {@link CodeAnalyticsPage}. */
export interface CodeAnalyticsPageProps {
  /** The code whose scans are shown. */
  readonly codeId: string;
  /** Returns to the codes list. */
  readonly onBack: () => void;
  /** Opens the code in the builder. */
  readonly onEdit: (id: string) => void;
}

/** Renders one dynamic code's scans per UTC day, device, operating system and country. */
defineOptions({ name: 'CodeAnalyticsPage' });
const props = defineProps<CodeAnalyticsPageProps>();

const days = ref<number>(DefaultAnalyticsDays);
const code = useCode(() => props.codeId);
const isStatic = computed(() => code.data.value?.mode === ContentMode.Static);
// A static code's scans never reach ForeverPin, so only a dynamic code is asked for its window.
const analytics = useCodeAnalytics(
  () => (code.data.value?.mode === ContentMode.Dynamic ? props.codeId : undefined),
  days,
);

const scans = computed(() => analytics.data.value);
/** The window the answer covers: the one asked for, cut to the plan's reach. */
const windowDays = computed(() => scans.value?.daily.length ?? days.value);
/** How far back the owner's plan reaches; every window is open until the first answer says otherwise. */
const maxDays = computed(() => scans.value?.maxDays ?? Number.POSITIVE_INFINITY);
const hasLockedWindows = computed(() => AnalyticsWindows.some((option) => option.days > maxDays.value));
const dailyScans = computed(() => scans.value?.daily.map((day) => day.scans) ?? []);
const peak = computed(() =>
  scans.value?.daily.reduce<ScanDayDto | undefined>(
    (best, day) => (day.scans > (best?.scans ?? 0) ? day : best),
    undefined,
  ),
);
const topCountry = computed(() => scans.value?.countries.find((share) => share.key));
const activeDays = computed(() => scans.value?.daily.filter((day) => day.scans > 0).reverse() ?? []);
const chartLabel = computed(() => {
  if (!scans.value) return 'Scans per day';
  const total = AnalyticsDisplays.count(scans.value.rangeScans);
  return peak.value
    ? `${total} scans over ${windowDays.value} days, peaking at ${AnalyticsDisplays.count(peak.value.scans)} on ${AnalyticsDisplays.day(peak.value.date)}`
    : `No scans over ${windowDays.value} days`;
});
/** The read that failed, if any; its message reaches the toast host, the region settles into neutral copy. */
const failure = computed(() => code.error.value ?? analytics.error.value);

function selectDays(value: unknown): void {
  if (typeof value === 'string') days.value = Number(value);
}

function retry(): void {
  if (code.error.value) void code.refetch();
  else void analytics.refetch();
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex flex-col gap-2">
      <Button variant="ghost" tone="neutral" size="sm" class="-ml-2 self-start" @click="props.onBack">
        <template #leading><ArrowLeft :size="16" aria-hidden="true" /></template>
        Back to codes
      </Button>
      <div class="flex flex-wrap items-end justify-between gap-4">
        <div class="min-w-0">
          <Heading :level="1" size="xl" weight="bold" class="truncate">
            {{ code.data.value ? `${code.data.value.name} · Analytics` : 'Analytics' }}
          </Heading>
          <Text color="muted">Scans per day, device, system and country — pull it when you want it.</Text>
        </div>
        <div v-if="code.data.value && !isStatic" class="flex flex-wrap items-center gap-2">
          <ToggleGroup
            variant="segmented"
            type="single"
            :model-value="String(windowDays)"
            aria-label="Analytics window"
            @update:model-value="selectDays"
          >
            <ToggleInput
              v-for="option in AnalyticsWindows"
              :key="option.days"
              variant="soft"
              size="sm"
              :value="String(option.days)"
              :is-disabled="option.days > maxDays"
            >
              {{ option.label }}
            </ToggleInput>
          </ToggleGroup>
          <Text v-if="hasLockedWindows" size="xs" color="muted">Longer windows come with Basic.</Text>
          <Button size="sm" variant="ghost" tone="neutral" @click="props.onEdit(props.codeId)">
            <template #leading><Pencil :size="15" aria-hidden="true" /></template>
            Edit
          </Button>
        </div>
      </div>
    </div>

    <Card v-if="isStatic" class="surface-glass p-6">
      <EmptyState
        title="Static codes aren't counted"
        description="A static code carries its content in the symbol, so a scan never passes through ForeverPin. Copy it as a dynamic code to count its scans."
      >
        <template #icon><ChartColumn :size="32" /></template>
      </EmptyState>
    </Card>

    <DataRegion
      v-else
      class="flex flex-col gap-6"
      :is-first-load="!scans && !failure"
      :is-refreshing="analytics.isRefreshing.value"
      :error="failure"
      :has-data="Boolean(scans)"
      unavailable-title="Analytics aren't available right now"
      loading-label="Loading analytics"
      @retry="retry"
    >
      <template #skeleton>
        <div class="grid grid-cols-2 gap-3 lg:grid-cols-4">
          <SkeletonState v-for="index in 4" :key="index" class="h-24 w-full rounded-xl" />
        </div>
        <SkeletonState class="h-48 w-full rounded-xl" />
      </template>
      <template v-if="scans">
        <div class="grid grid-cols-2 gap-3 lg:grid-cols-4" :aria-busy="analytics.isRefreshing.value">
          <StatCard :label="`Scans · last ${windowDays} days`" :value="AnalyticsDisplays.count(scans.rangeScans)" />
          <StatCard label="All-time scans" :value="AnalyticsDisplays.count(scans.totalScans)" />
          <StatCard
            label="Busiest day"
            :value="peak ? AnalyticsDisplays.count(peak.scans) : '—'"
            :helper="peak ? AnalyticsDisplays.day(peak.date) : 'No scans yet'"
          />
          <StatCard
            label="Top country"
            :value="topCountry ? AnalyticsDisplays.country(topCountry.key) : '—'"
            :helper="topCountry ? `${AnalyticsDisplays.count(topCountry.scans)} scans` : 'Not located yet'"
          />
        </div>

        <Card class="surface-glass flex flex-col gap-3 p-5">
          <div class="flex items-baseline justify-between gap-3">
            <Heading :level="2" size="sm" weight="semibold">Scans per day</Heading>
            <Text size="xs" color="muted">UTC</Text>
          </div>
          <Sparkline
            variant="bar"
            :data="dailyScans"
            :width="720"
            :height="160"
            :min="0"
            :max="Math.max(1, peak?.scans ?? 0)"
            :aria-label="chartLabel"
            class="h-40 w-full"
          />
          <div class="flex justify-between text-xs text-muted-foreground tabular-nums">
            <span>{{ AnalyticsDisplays.day(scans.from) }}</span>
            <span>{{ AnalyticsDisplays.day(scans.to) }}</span>
          </div>
          <Text v-if="activeDays.length === 0" size="sm" color="muted">
            No scans in this window yet — a scan shows up here within seconds.
          </Text>
          <details v-else class="text-sm">
            <summary class="cursor-pointer text-muted-foreground hover:text-foreground">Daily counts</summary>
            <ul class="mt-2 grid grid-cols-2 gap-x-6 gap-y-1 sm:grid-cols-3">
              <li v-for="day in activeDays" :key="day.date.toString()" class="flex justify-between gap-3 tabular-nums">
                <span>{{ AnalyticsDisplays.day(day.date) }}</span>
                <span class="text-muted-foreground">{{ AnalyticsDisplays.count(day.scans) }}</span>
              </li>
            </ul>
          </details>
        </Card>

        <div class="grid grid-cols-1 gap-3 md:grid-cols-3">
          <ScanShareCard title="Devices" :shares="scans.devices" :describe="AnalyticsDisplays.device" />
          <ScanShareCard
            title="Operating systems"
            :shares="scans.operatingSystems"
            :describe="AnalyticsDisplays.system"
          />
          <ScanShareCard title="Countries" :shares="scans.countries" :describe="AnalyticsDisplays.country">
            <template #footer>
              <!-- DB-IP Lite data is licensed CC BY 4.0; showing located countries requires this attribution. -->
              <Text size="xs" color="muted">
                <a href="https://db-ip.com" class="underline hover:text-foreground">IP Geolocation by DB-IP</a>
              </Text>
            </template>
          </ScanShareCard>
        </div>

        <Text size="xs" color="muted">
          Days are UTC. Every resolved scan counts, including repeat scans from the same phone; link previews and bots
          are listed as their own device class.
        </Text>
      </template>
    </DataRegion>
  </div>
</template>
