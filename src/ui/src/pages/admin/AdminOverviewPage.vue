<script setup lang="ts">
import Dialog from "primevue/dialog";
import { computed, ref } from "vue";
import { formatAdminDate, formatWeekday } from "../../features/admin/lib/date";
import { useAdminOverview } from "../../features/admin/schedule/useAdminOverview";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import AdminAutoAssignDialog from "../../features/admin/schedule/AdminAutoAssignDialog.vue";
import { useHandoffDelayedLoadingState } from "../../shared/composables/useSkeletonHandoff";
import UiButton from "../../shared/ui/UiButton.vue";
import UiSkeleton from "../../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../../shared/ui/UiSkeletonHandoff.vue";
import { authState } from "../../features/auth/model/authSession";

const { today, periodEnd, schedules, loading, loadError, assignedCount, emptySchedules, nextEmptyDay, loadSchedule } =
  useAdminOverview();
const loadingState = useHandoffDelayedLoadingState(loading);
const scheduleOpen = ref(false);
const wordsVisible = ref(false);
const spoilerDialogOpen = ref(false);
const autoAssignOpen = ref(false);

const canAssign = computed(() =>
  hasPermission(authState.value, adminPermissions.assignSource),
);

function requestRevealWords() {
  if (wordsVisible.value) {
    wordsVisible.value = false;
    return;
  }
  spoilerDialogOpen.value = true;
}

function revealWords() {
  wordsVisible.value = true;
  spoilerDialogOpen.value = false;
}

</script>

<template>
  <div class="admin-page">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Слова дня</p>
        <h1>Обзор</h1>
        <p class="admin-page__description">Состояние расписания без случайных спойлеров.</p>
      </div>
      <UiButton
        v-if="canAssign"
        variant="outlined"
        @click="autoAssignOpen = true"
      >
        Автоназначить
      </UiButton>
    </header>

    <UiSkeletonHandoff :skeleton-visible="loadingState.visible" :content-visible="loadingState.ready">
      <template #skeleton>
        <div class="admin-overview-skeleton" aria-hidden="true">
          <section class="overview-stats">
            <article v-for="index in 2" :key="index" class="stat-card">
              <UiSkeleton width="42%" height="15px" />
              <UiSkeleton width="54px" height="38px" />
              <UiSkeleton width="72%" height="13px" />
            </article>
          </section>
          <section class="schedule-preview">
            <div class="schedule-preview__toggle">
              <UiSkeleton width="190px" height="18px" />
              <UiSkeleton width="22px" height="22px" shape="circle" />
            </div>
          </section>
        </div>
      </template>
      <div v-if="loadError" class="admin-error">
        <p>{{ loadError }}</p>
        <UiButton variant="outlined" :loading="loading" @click="loadSchedule">Повторить</UiButton>
      </div>
      <div v-else class="admin-overview-content">
        <section class="overview-stats">
        <article class="stat-card">
          <span>Назначено</span>
          <strong>{{ assignedCount }}</strong>
          <small>из {{ schedules.length }} ближайших дней</small>
        </article>
        <article class="stat-card" :class="{ 'stat-card--warning': emptySchedules.length }">
          <span>Пустых дней</span>
          <strong>{{ emptySchedules.length }}</strong>
          <small v-if="nextEmptyDay">Ближайший: {{ formatAdminDate(nextEmptyDay, true) }}</small>
          <small v-else>Пробелов нет</small>
        </article>
      </section>

      <section v-if="emptySchedules.length" class="overview-warning">
        <i class="pi pi-exclamation-triangle" aria-hidden="true"></i>
        <span>В ближайшие две недели есть незаполненные дни.</span>
      </section>

      <section class="schedule-preview">
        <button
          class="admin-native-action schedule-preview__toggle"
          type="button"
          :aria-expanded="scheduleOpen"
          @click="scheduleOpen = !scheduleOpen"
        >
          <span>Ближайшее расписание</span>
          <i :class="scheduleOpen ? 'pi pi-chevron-up' : 'pi pi-chevron-down'" aria-hidden="true"></i>
        </button>

        <div v-if="scheduleOpen" class="schedule-preview__body">
          <div class="schedule-preview__toolbar">
            <span>Тексты слов скрыты</span>
            <button type="button" class="admin-native-action" @click="requestRevealWords">
              <i :class="wordsVisible ? 'pi pi-eye-slash' : 'pi pi-eye'" aria-hidden="true"></i>
              {{ wordsVisible ? "Скрыть слова" : "Показать слова" }}
            </button>
          </div>
          <ul>
            <li v-for="item in schedules" :key="item.day">
              <span>
                <strong>{{ formatAdminDate(item.day) }}</strong>
                {{ formatWeekday(item.day) }}
              </span>
              <span v-if="item.gameSource" class="schedule-preview__assigned">
                {{ wordsVisible ? item.gameSource.word : "Назначено" }}
              </span>
              <span v-else class="schedule-preview__empty">Не назначено</span>
            </li>
          </ul>
        </div>
      </section>
      </div>
    </UiSkeletonHandoff>

    <Dialog v-model:visible="spoilerDialogOpen" modal header="Показать слова?" class="admin-dialog">
      <p class="spoiler-warning">
        Будут раскрыты слова ближайших игр дня. Это может испортить вашу игру.
      </p>
      <div class="dialog-actions">
        <UiButton @click="revealWords">Показать</UiButton>
        <UiButton variant="outlined" @click="spoilerDialogOpen = false">Отмена</UiButton>
      </div>
    </Dialog>

    <AdminAutoAssignDialog
      v-model:visible="autoAssignOpen"
      :initial-from="today"
      :initial-to="periodEnd"
      @completed="loadSchedule"
    />
  </div>
</template>

<style scoped>
.admin-overview-skeleton,
.admin-overview-content { display: flex; flex-direction: column; gap: 14px; }
.admin-error { min-height: 220px; display: flex; align-items: center; justify-content: center; flex-direction: column; gap: 12px; }
.admin-error { color: var(--color-red-700); }
.overview-stats { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; }
.stat-card { min-height: 104px; border-top: 1px solid var(--color-gray-200); border-bottom: 1px solid var(--color-gray-200); padding: 14px 4px; display: flex; flex-direction: column; }
.stat-card span { color: var(--color-gray-600); }
.stat-card strong { margin: 6px 0 2px; color: var(--color-gray-900); font-size: 32px; }
.stat-card small { color: var(--p-text-muted-color); }
.stat-card--warning strong { color: var(--color-red-700); }
.overview-warning { border: 1px solid var(--color-orange-200); border-radius: 8px; padding: 11px 12px; display: flex; gap: 10px; background: var(--color-gray-50); color: var(--color-gray-700); }
.schedule-preview { border: 1px solid var(--color-gray-200); border-radius: 8px; overflow: hidden; background: white; }
.schedule-preview__toggle { width: 100%; min-height: 50px; border: 0; padding: 0 14px; display: flex; align-items: center; justify-content: space-between; background: white; color: var(--color-gray-800); font-weight: 500; cursor: pointer; }
.schedule-preview__body { border-top: 1px solid var(--color-gray-200); }
.schedule-preview__toolbar { min-height: 46px; padding: 0 14px; display: flex; align-items: center; justify-content: space-between; gap: 10px; background: var(--color-gray-50); color: var(--p-text-muted-color); font-size: 14px; }
.schedule-preview__toolbar button { border: 0; display: inline-flex; align-items: center; gap: 7px; background: transparent; color: var(--color-primary-700); cursor: pointer; }
.schedule-preview ul { margin: 0; padding: 0; list-style: none; }
.schedule-preview li { min-height: 48px; padding: 8px 14px; display: flex; align-items: center; justify-content: space-between; gap: 12px; border-top: 1px solid var(--color-gray-100); }
.schedule-preview li > span:first-child { display: flex; gap: 8px; color: var(--p-text-muted-color); }
.schedule-preview__assigned { color: var(--color-primary-700); font-weight: 500; }
.schedule-preview__empty { color: var(--color-red-700); }
.spoiler-warning { max-width: 430px; color: var(--color-gray-700); line-height: 1.5; }
@container admin-content (max-width: 600px) { .overview-stats { grid-template-columns: 1fr; } .schedule-preview li { align-items: flex-start; flex-direction: column; } }
</style>
