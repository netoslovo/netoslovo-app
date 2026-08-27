<script setup lang="ts">
import Select from "primevue/select";
import { ref } from "vue";
import UiButton from "../../shared/ui/UiButton.vue";
import UiSkeleton from "../../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../../shared/ui/UiSkeletonHandoff.vue";
import { useAdminModeration } from "../../features/admin/moderation/useAdminModeration";

const {
  closestWordsCountOptions, difficulties, difficultyCode, closestWordsCount,
  source, sourceMode, manualReviewStatus,
  searchWord, searchError, searching, loading, neighborsLoading, saving,
  queueCompleted, error, dragOffset, canReview,
  loadSource, searchSource, changeDifficulty, decide: decideSource, reloadClosestWords,
  skipSource, onPointerDown, onPointerMove, onPointerEnd,
} = useAdminModeration();

const pendingDecision = ref<boolean | null>(null);
const sourceOpen = ref(false);

async function decide(approved: boolean) {
  if (saving.value) return;

  pendingDecision.value = approved;
  try {
    await decideSource(approved);
  } finally {
    pendingDecision.value = null;
  }
}
</script>

<template>
  <div class="admin-page">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Слова дня</p>
        <h1>Модерация</h1>
        <p class="admin-page__description">Свайп влево отклоняет, вправо одобряет.</p>
      </div>
    </header>

    <section class="admin-panel moderation-source" aria-labelledby="moderation-source-title">
      <div class="moderation-source__header">
        <h2 id="moderation-source-title">Источник слова</h2>
        <button
          class="moderation-source__toggle"
          type="button"
          :aria-expanded="sourceOpen"
          aria-controls="moderation-source-content"
          aria-label="Показать или скрыть источник слова"
          @click="sourceOpen = !sourceOpen"
        >
          <i class="pi pi-chevron-down" aria-hidden="true"></i>
        </button>
      </div>
      <div
        id="moderation-source-content"
        class="moderation-source__lanes"
        :class="{ 'moderation-source__lanes--closed': !sourceOpen }"
      >
        <section class="moderation-source__lane">
          <div class="moderation-source__heading">
            <div><h3>Очередь</h3><p>Следующее нерассмотренное слово</p></div>
            <small v-if="source && sourceMode === 'queue'">Текущий источник</small>
          </div>
          <label for="moderation-difficulty">Сложность</label>
          <Select
            v-model="difficultyCode"
            input-id="moderation-difficulty"
            class="admin-select"
            overlay-class="admin-select-overlay"
            :options="difficulties"
            option-label="name"
            option-value="code"
            :disabled="saving"
            fluid
            @change="changeDifficulty"
          />
        </section>

        <form class="moderation-source__lane moderation-search" @submit.prevent="searchSource">
          <div class="moderation-source__heading">
            <div><h3>Ручной поиск</h3><p>Точное совпадение</p></div>
            <small v-if="source && sourceMode === 'manual'">Текущий источник</small>
          </div>
          <label for="moderation-word-search">Слово</label>
          <div class="moderation-search__controls">
            <input id="moderation-word-search" v-model="searchWord" class="admin-native-field" type="search" placeholder="Слово целиком"
              :disabled="loading || saving || searching" @input="searchError = null" />
            <UiButton type="submit" variant="outlined" :loading="searching" :disabled="loading || saving">Найти</UiButton>
          </div>
          <small v-if="searchError" class="moderation-search__error">{{ searchError }}</small>
        </form>
      </div>
    </section>

    <section class="moderation-review" aria-labelledby="moderation-review-title">
      <div class="moderation-review__header">
        <h2 id="moderation-review-title">Карточка слова</h2>
        <div class="moderation-neighbors">
          <label for="moderation-neighbors-count">Соседей:</label>
          <Select
            v-model="closestWordsCount"
            input-id="moderation-neighbors-count"
            class="admin-select"
            overlay-class="admin-select-overlay"
            :options="closestWordsCountOptions"
            :disabled="saving || neighborsLoading"
            @change="reloadClosestWords"
          />
        </div>
      </div>

      <div class="moderation-stage">
      <UiSkeletonHandoff :skeleton-visible="loading" :content-visible="!loading">
        <template #skeleton>
          <div class="moderation-state">
            <article class="moderation-card moderation-card--skeleton" aria-hidden="true">
              <UiSkeleton width="96px" height="28px" border-radius="999px" />
              <UiSkeleton width="58%" height="58px" />
              <UiSkeleton width="112px" height="14px" />
              <div class="closest-words closest-words--skeleton">
                <UiSkeleton width="132px" height="14px" />
                <div class="closest-words__skeleton-grid">
                  <UiSkeleton v-for="index in 8" :key="index" height="34px" border-radius="8px" />
                </div>
              </div>
            </article>
          </div>
        </template>
        <div v-if="error" class="moderation-state">
          <p>{{ error }}</p>
          <UiButton variant="outlined" :loading="loading" @click="loadSource()">Повторить</UiButton>
        </div>
        <div v-else-if="queueCompleted" class="moderation-state moderation-state--done"><i class="pi pi-check-circle"
            aria-hidden="true"></i>
          <h2>Очередь закончилась</h2>
          <p>Для выбранной сложности больше нет нерассмотренных слов.</p>
        </div>
        <div v-else-if="source" class="moderation-state"><article class="moderation-card" :class="{ 'moderation-card--saving': saving }"
          :style="{ transform: `translateX(${dragOffset}px) rotate(${dragOffset / 25}deg)` }" @pointerdown="onPointerDown"
          @pointermove="onPointerMove" @pointerup="onPointerEnd" @pointercancel="onPointerEnd">
        <div class="moderation-card__decision moderation-card__decision--reject"
          :style="{ opacity: Math.max(0, -dragOffset / 90) }">Отклонить</div>
        <div class="moderation-card__decision moderation-card__decision--approve"
          :style="{ opacity: Math.max(0, dragOffset / 90) }">Одобрить</div>
        <span class="moderation-card__difficulty">{{ source.gameSource.difficulty.name }}</span>
        <span v-if="sourceMode === 'manual'" class="moderation-card__status"
          :class="{ 'moderation-card__status--approved': manualReviewStatus === true, 'moderation-card__status--rejected': manualReviewStatus === false }">{{
            manualReviewStatus === null ? "Не рассмотрено" : manualReviewStatus ? "Одобрено" : "Отклонено" }}</span>
        <strong>{{ source.gameSource.word }}</strong>
        <small>Источник #{{ source.gameSource.gameSourceId }}</small>
        <div v-if="source.closestWords.length > 0" class="closest-words">
          <span class="closest-words__title">Ближайшие слова</span>
          <ol>
            <li v-for="(word, index) in source.closestWords" :key="`${word}-${index}`">
              <span>{{ index + 1 }}</span>
              <b>{{ word }}</b>
            </li>
          </ol>
        </div>
      </article></div>
      </UiSkeletonHandoff>
      </div>

      <div v-if="source" class="moderation-actions">
        <UiButton class="decision-button decision-button--reject" aria-label="Отклонить" title="Отклонить"
          :loading="pendingDecision === false" :disabled="loading || saving || !canReview" @click="decide(false)">
          <i class="pi pi-times" aria-hidden="true"></i><span>Отклонить</span>
        </UiButton>
        <button class="decision-button decision-button--skip" type="button" aria-label="Пропустить" title="Пропустить"
          :disabled="loading || saving" @click="skipSource"><i class="pi pi-forward"
            aria-hidden="true"></i><span>Пропустить</span></button>
        <UiButton class="decision-button decision-button--approve" aria-label="Одобрить" title="Одобрить"
          :loading="pendingDecision === true" :disabled="loading || saving || !canReview" @click="decide(true)">
          <i class="pi pi-check" aria-hidden="true"></i><span>Одобрить</span>
        </UiButton>
      </div>

    </section>
  </div>
</template>

<style scoped>
.admin-page {
  min-height: 560px;
}

.moderation-source h2,
.moderation-review h2,
.moderation-source h3,
.moderation-source p {
  margin: 0;
}

.moderation-source h2,
.moderation-review h2 {
  font-size: 18px;
}

.moderation-source__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.moderation-source__toggle {
  width: 28px;
  min-width: 28px;
  height: 28px;
  padding: 0;
  border: 0;
  border-radius: 8px;
  display: none;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-surface-500);
  cursor: pointer;
  transition: color .16s ease, background-color .16s ease;
}

.moderation-source__toggle i {
  font-size: 14px;
  line-height: 1;
  transition: transform .16s ease;
}

.moderation-source__toggle[aria-expanded="true"] i {
  transform: rotate(180deg);
}

@media (hover: hover) and (pointer: fine) {
  .moderation-source__toggle:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }
}

.moderation-source__toggle:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.moderation-source__lanes {
  margin-top: 14px;
  display: grid;
  grid-template-columns: minmax(180px, .8fr) minmax(300px, 1.2fr);
}

.moderation-source__lane {
  min-width: 0;
  padding: 0 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.moderation-source__lane:first-child {
  padding-left: 0;
}

.moderation-source__lane + .moderation-source__lane {
  border-left: 1px solid var(--color-gray-200);
  padding-right: 0;
}

.moderation-source__heading {
  min-height: 44px;
  margin-bottom: 4px;
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
}

.moderation-source__heading h3 {
  color: var(--color-gray-800);
  font-size: 15px;
}

.moderation-source__heading p,
.moderation-source__heading small {
  color: var(--p-text-muted-color);
  font-size: 12px;
}

.moderation-source__heading small {
  flex: none;
  color: var(--color-primary-700);
  font-weight: 500;
}

.moderation-source__lane > label,
.moderation-neighbors label {
  color: var(--color-gray-600);
  font-size: 13px;
}

.moderation-source input {
  min-width: 0;
  border: 1px solid var(--color-gray-300);
  padding: 0 10px;
  background: white;
}

.moderation-source__lane > .admin-select,
.moderation-search__controls,
.moderation-search__controls input {
  width: 100%;
}

.moderation-search__controls {
  display: flex;
  gap: 6px;
}

.moderation-search__error {
  color: var(--color-red-600);
}

@container admin-content (min-width: 601px) {
  .moderation-source__lanes {
    grid-template-rows: auto auto auto auto;
    row-gap: 6px;
  }

  .moderation-source__lane {
    grid-row: span 4;
    display: grid;
    grid-template-rows: subgrid;
  }
}

.moderation-review {
  width: min(520px, 100%);
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.moderation-review__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.moderation-neighbors {
  display: flex;
  align-items: center;
  gap: 7px;
}

.moderation-neighbors .admin-select {
  width: 96px;
  flex: none;
}

.moderation-neighbors :deep(.p-select-label) {
  overflow: visible;
  text-overflow: clip;
}

.moderation-stage {
  min-height: 430px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  overflow: clip;
  padding: 8px 0;
}

.moderation-card {
  width: 100%;
  min-height: 340px;
  position: relative;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: white;
  touch-action: pan-y;
  user-select: none;
  transition: transform .15s ease;
}

.moderation-card--saving {
  opacity: .65;
  pointer-events: none;
}

.moderation-card--skeleton {
  pointer-events: none;
}

.moderation-card strong {
  font-size: clamp(32px, 8vw, 52px);
  overflow-wrap: anywhere;
  text-align: center;
}

.moderation-card small {
  color: var(--p-text-muted-color);
}

.moderation-card__difficulty {
  border-radius: 999px;
  padding: 5px 10px;
  background: var(--color-primary-100);
  color: var(--color-primary-700);
  font-size: 13px;
  font-weight: 500;
}

.moderation-card__status {
  border-radius: 999px;
  padding: 4px 9px;
  background: var(--color-gray-100);
  color: var(--color-gray-600);
  font-size: 12px;
  font-weight: 500;
}

.moderation-card__status--approved {
  background: var(--color-primary-100);
  color: var(--color-primary-700);
}

.moderation-card__status--rejected {
  background: var(--color-red-100);
  color: var(--color-red-700);
}

.moderation-card__decision {
  position: absolute;
  top: 18px;
  border: 2px solid currentColor;
  border-radius: 8px;
  padding: 5px 8px;
  font-size: 16px;
  font-weight: 500;
}

.moderation-card__decision--reject {
  left: 22px;
  color: var(--color-red-600);
  transform: rotate(-8deg);
}

.moderation-card__decision--approve {
  right: 22px;
  color: var(--color-primary-600);
  transform: rotate(8deg);
}

.closest-words {
  width: 100%;
  margin-top: 12px;
  padding-top: 14px;
  border-top: 1px solid var(--color-gray-100);
}

.closest-words__title {
  display: block;
  margin-bottom: 10px;
  color: var(--p-text-muted-color);
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0;
  text-align: center;
  text-transform: uppercase;
}

.closest-words--skeleton {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.closest-words__skeleton-grid {
  width: 100%;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(118px, 1fr));
  gap: 8px;
}

.closest-words ol {
  max-height: 174px;
  margin: 0;
  padding: 0 2px;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(118px, 1fr));
  gap: 8px;
  overflow: auto;
  list-style: none;
}

.closest-words li {
  min-width: 0;
  min-height: 34px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 5px 8px;
  display: flex;
  align-items: center;
  gap: 7px;
  background: var(--color-gray-50);
}

.closest-words li span {
  min-width: 24px;
  height: 22px;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: white;
  color: var(--p-text-muted-color);
  font-size: 12px;
  font-weight: 500;
}

.closest-words li b {
  min-width: 0;
  overflow: hidden;
  color: var(--color-gray-800);
  font-size: 14px;
  font-weight: 500;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.moderation-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.decision-button {
  min-width: 132px;
  height: 48px;
  border: 1px solid;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background: white;
  cursor: pointer;
  font-weight: 500;
}

.decision-button i {
  font-size: 25px;
}

.decision-button--reject {
  border-color: var(--color-red-200);
  color: var(--color-red-700);
}

.decision-button--skip {
  border-color: var(--color-gray-300);
  color: var(--color-gray-600);
}

.decision-button--approve {
  border-color: var(--color-primary-300);
  color: var(--color-primary-700);
}

.decision-button:disabled {
  opacity: .45;
  cursor: default;
}

.moderation-state {
  min-height: 260px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: var(--color-gray-600);
}

.moderation-state--done i {
  color: var(--color-primary-600);
  font-size: 48px;
}

.moderation-state--done h2 {
  margin: 12px 0 0;
  color: var(--color-gray-800);
}

@container admin-content (max-width: 600px) {
  .moderation-source__lanes {
    grid-template-columns: 1fr;
  }

  .moderation-source__lane {
    padding: 14px 0 0;
  }

  .moderation-source__lane + .moderation-source__lane {
    border-top: 1px solid var(--color-gray-200);
    border-left: 0;
    margin-top: 14px;
    padding-right: 0;
  }

  .moderation-review__header {
    align-items: flex-start;
    flex-wrap: wrap;
  }

  .moderation-stage {
    min-height: 430px;
    padding: 8px 0;
  }

  .moderation-card {
    min-height: 340px;
    padding: 20px;
  }

  .moderation-actions .decision-button {
    min-width: 56px;
    width: 56px;
  }

  .moderation-actions .decision-button span {
    display: none;
  }
}

@media (max-width: 899px) {
  .moderation-source__toggle {
    display: inline-flex;
  }

  .moderation-source__lanes.moderation-source__lanes--closed {
    display: none;
  }
}
</style>
