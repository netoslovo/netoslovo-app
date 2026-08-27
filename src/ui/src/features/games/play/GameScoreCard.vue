<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import type { HintType, ScoreDetails } from "../model/game";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

const props = withDefaults(
  defineProps<{
    score: number;
    scoreDetails: ScoreDetails | null;
    embedded?: boolean;
  }>(),
  {
    embedded: false,
  },
);

const hintTypeLabels: Record<HintType, string> = {
  revealHalfwayWord: "Промежуточное слово",
  revealLength: "Показать длину слова",
  revealLetter: "Открыть случайную букву",
};

const {
  triggerId: scoreTriggerId,
  panelId: scorePanelId,
  popoverPt: scorePopoverPt,
  setPopover: setScorePopover,
  visible: scorePopoverVisible,
  show: showScoreInfo,
  hide: hideScoreInfo,
  onPointerDown: onScoreInfoPointerDown,
  onShow: onScorePopoverShow,
  onHide: onScorePopoverHide,
} = useInfoPopover();

const guessesCount = computed(() => props.scoreDetails?.guessesCount ?? 0);

const hintPenaltyTotal = computed(
  () =>
    props.scoreDetails?.usedHints.reduce(
      (total, hint) => total + hint.penalty,
      0,
    ) ?? 0,
);

function getHintNumber(hintType: HintType, hintIndex: number) {
  return (
    props.scoreDetails?.usedHints
      .slice(0, hintIndex + 1)
      .filter((hint) => hint.type === hintType).length ?? 1
  );
}

function getUsedHintLabel(hintType: HintType, hintIndex: number) {
  const label = hintTypeLabels[hintType];

  if (hintType === "revealLength") {
    return label;
  }

  return `${label} #${getHintNumber(hintType, hintIndex)}`;
}

</script>

<template>
  <section class="game-score-card" :class="{ 'game-score-card--embedded': embedded }" aria-label="Счёт игры">
    <div class="game-score-card__score-group">
      <div class="game-score-card__primary" :aria-label="`Счёт: ${score}`">
        <span class="game-score-card__label">Счёт:</span>
        <strong class="game-score-card__score">{{ score }}</strong>
      </div>

      <button class="game-score-card__info" :class="{ 'game-score-card__info--open': scorePopoverVisible }"
        :id="scoreTriggerId" type="button" aria-label="Подробнее о счёте" :aria-describedby="scorePanelId"
        @mouseenter="showScoreInfo" @mouseleave="hideScoreInfo" @pointerdown="onScoreInfoPointerDown"
        @focus="showScoreInfo" @blur="hideScoreInfo" @click.stop>
        <i class="game-score-card__info-icon pi pi-info-circle" aria-hidden="true"></i>
      </button>
    </div>

    <div class="game-score-card__details">
      <span class="game-score-card__detail" :aria-label="`Попытки: ${guessesCount}`">
        <span class="game-score-card__detail-label">Попытки:</span>
        <strong class="game-score-card__detail-value">{{ guessesCount }}</strong>
      </span>

      <span class="game-score-card__detail" :aria-label="`Подсказки: плюс ${hintPenaltyTotal}`">
        <span class="game-score-card__detail-label">Подсказки:</span>
        <strong class="game-score-card__detail-value">+{{ hintPenaltyTotal }}</strong>
      </span>
    </div>

    <Popover :ref="setScorePopover" :pt="scorePopoverPt" class="game-score-card__popover info-popover"
      @show="onScorePopoverShow" @hide="onScorePopoverHide">
      <div class="game-score-card__popover-content">
        <p class="game-score-card__popover-note">
          Каждая попытка увеличивает счёт на 1. За подсказки начисляются дополнительные баллы. Чем меньше
          итоговый счёт, тем лучше результат.
        </p>

        <dl class="game-score-card__popover-details">
          <div class="game-score-card__popover-row game-score-card__popover-row--total">
            <dt>Счёт</dt>
            <dd>{{ score }}</dd>
          </div>

          <div class="game-score-card__popover-row">
            <dt>Попытки</dt>
            <dd>{{ guessesCount }}</dd>
          </div>

          <div class="game-score-card__popover-row">
            <dt>Подсказки</dt>
            <dd>+{{ hintPenaltyTotal }}</dd>
          </div>
        </dl>

        <ul v-if="scoreDetails?.usedHints.length" class="game-score-card__hints">
          <li v-for="(hint, index) in scoreDetails.usedHints" :key="`${hint.type}-${hint.usedAt}`"
            class="game-score-card__hint">
            <span>{{ getUsedHintLabel(hint.type, index) }}</span>
            <strong>+{{ hint.penalty }}</strong>
          </li>
        </ul>
      </div>
    </Popover>
  </section>
</template>

<style scoped>
.game-score-card {
  --game-score-card-min-height: 42px;
  --game-score-card-padding-block: 7px;
  --game-score-card-padding-inline-start: 10px;
  --game-score-card-padding-inline-end: 9px;
  --game-score-card-gap: 12px;
  --game-score-card-primary-gap: 7px;
  --game-score-card-score-info-gap: 2px;
  --game-score-card-label-font-size: 17px;
  --game-score-card-detail-label-font-size: 13px;
  --game-score-card-score-font-size: 18px;
  --game-score-card-detail-font-size: 15px;
  --game-score-card-detail-gap: 5px;
  --game-score-card-info-size: 26px;
  --game-score-card-info-icon-size: 14px;
  width: 100%;
  min-height: var(--game-score-card-min-height);
  padding:
    var(--game-score-card-padding-block) var(--game-score-card-padding-inline-end) var(--game-score-card-padding-block) var(--game-score-card-padding-inline-start);
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;

  display: flex;
  align-items: center;
  gap: var(--game-score-card-gap);
}

.game-score-card--embedded {
  min-height: auto;
  padding: 0;
  border: 0;
  border-radius: 0;
  background: transparent;
}

.game-score-card__score-group {
  min-width: 0;
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: var(--game-score-card-score-info-gap);
}

.game-score-card__primary {
  min-width: 0;
  display: flex;
  align-items: baseline;
  gap: var(--game-score-card-primary-gap);
}

.game-score-card__label {
  color: var(--p-text-muted-color);
  font-size: var(--game-score-card-label-font-size);
  font-weight: 400;
  line-height: 1.1;
}

.game-score-card__detail-label {
  color: var(--p-text-muted-color);
  font-size: var(--game-score-card-detail-label-font-size);
  font-weight: 400;
  line-height: 1.1;
}

.game-score-card__score,
.game-score-card__detail-value {
  color: var(--color-gray-900);
  font-weight: 500;
  line-height: 1;
  font-variant-numeric: tabular-nums;
}

.game-score-card__score {
  font-size: var(--game-score-card-score-font-size);
}

.game-score-card__details {
  min-width: 0;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: var(--game-score-card-gap);
}

.game-score-card__detail {
  min-width: 0;
  display: inline-flex;
  align-items: baseline;
  gap: var(--game-score-card-detail-gap);
}

.game-score-card__detail-value {
  font-size: var(--game-score-card-detail-font-size);
}

.game-score-card__info {
  width: var(--game-score-card-info-size);
  min-width: var(--game-score-card-info-size);
  height: var(--game-score-card-info-size);
  border: 0;
  padding: 0;

  display: inline-flex;
  align-items: center;
  justify-content: center;

  border-radius: 50%;
  background: transparent;
  color: var(--p-surface-500);
  cursor: help;

  transition:
    color 0.16s ease,
    background-color 0.16s ease;
}

.game-score-card__info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-700);
}

@media (hover: hover) and (pointer: fine) {
  .game-score-card__info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }
}

.game-score-card__info:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.game-score-card__info-icon {
  font-size: var(--game-score-card-info-icon-size);
  line-height: 1;
}

.game-score-card__popover-content {
  min-width: 220px;
  max-width: min(310px, calc(100vw - 24px));
  color: var(--color-gray-700);
}

.game-score-card__popover-note {
  margin: 0 0 8px;
  font-size: var(--info-popover-font-size);
  line-height: 1.4;
}

.game-score-card__popover-details {
  margin: 0;
  display: grid;
  gap: 5px;
}

.game-score-card__popover-row,
.game-score-card__hint {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-size: var(--info-popover-font-size);
  line-height: 1.35;
}

.game-score-card__popover-row dt,
.game-score-card__popover-row dd {
  margin: 0;
}

.game-score-card__popover-row dd,
.game-score-card__hint strong {
  color: var(--color-gray-900);
  font-weight: 500;
}

.game-score-card__popover-row--total {
  border-bottom: 1px solid var(--color-gray-200);
  margin-bottom: 2px;
  padding-bottom: 7px;
  color: var(--color-gray-900);
}

.game-score-card__popover-row--total dt,
.game-score-card__popover-row--total dd {
  font-weight: 500;
}

.game-score-card__hints {
  border-left: 2px solid var(--color-gray-200);
  margin: 2px 0 0 8px;
  padding: 2px 0 0 11px;
  display: grid;
  gap: 5px;
  list-style: none;
}

.game-score-card__hint span {
  min-width: 0;
}

:global(.game-score-card__popover.p-popover) {
  max-width: min(330px, calc(100vw - 24px));
}

@media (max-width: 480px) {
  .game-score-card {
    --game-score-card-gap: 9px;
    --game-score-card-score-info-gap: 0;
  }

  .game-score-card__details {
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 4px 8px;
  }
}

@media (min-width: 768px) {
  .game-score-card {
    --game-score-card-min-height: 44px;
    --game-score-card-padding-block: 8px;
    --game-score-card-padding-inline-start: 12px;
    --game-score-card-padding-inline-end: 10px;
    --game-score-card-gap: 14px;
    --game-score-card-primary-gap: 8px;
    --game-score-card-score-info-gap: 2px;
    --game-score-card-label-font-size: 18px;
    --game-score-card-detail-label-font-size: 14px;
    --game-score-card-score-font-size: 19px;
    --game-score-card-detail-font-size: 16px;
    --game-score-card-detail-gap: 6px;
    --game-score-card-info-size: 28px;
    --game-score-card-info-icon-size: 15px;
  }
}

@media (min-width: 1024px) {
  .game-score-card {
    --game-score-card-min-height: 46px;
    --game-score-card-padding-block: 9px;
    --game-score-card-padding-inline-start: 12px;
    --game-score-card-padding-inline-end: 10px;
    --game-score-card-label-font-size: 19px;
    --game-score-card-score-font-size: 20px;
    --game-score-card-detail-label-font-size: 15px;
    --game-score-card-detail-font-size: 17px;
    --game-score-card-info-size: 30px;
    --game-score-card-info-icon-size: 16px;
  }
}
</style>
