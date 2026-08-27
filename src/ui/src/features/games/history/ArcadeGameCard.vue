<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { type RouteLocationRaw } from "vue-router";
import type { ArcadeGame, GameState } from "../model/game";
import HistoryDisplayWord from "./HistoryDisplayWord.vue";
import UiButton from "../../../shared/ui/UiButton.vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

const props = defineProps<{
  arcadeGame: ArcadeGame;
  to?: RouteLocationRaw;
}>();

const {
  triggerId: statusTriggerId,
  panelId: statusPanelId,
  popoverPt: statusPopoverPt,
  setPopover: setStatusPopover,
  visible: statusPopoverVisible,
  show: showStatus,
  hide: hideStatus,
  onPointerDown: onStatusPointerDown,
  onShow: onStatusPopoverShow,
  onHide: onStatusPopoverHide,
} = useInfoPopover();

const statusConfig: Record<
  GameState,
  { icon: string; label: string; status: GameState }
> = {
  active: {
    icon: "pi pi-spinner-dotted",
    label: "В процессе",
    status: "active",
  },
  guessed: {
    icon: "pi pi-check-circle",
    label: "Отгадано",
    status: "guessed",
  },
  surrendered: {
    icon: "pi pi-times-circle",
    label: "Вы сдались",
    status: "surrendered",
  },
  cancelled: {
    icon: "pi pi-ban",
    label: "Игра отменена",
    status: "cancelled",
  },
};

const config = computed(() => statusConfig[props.arcadeGame.state]);

const startedAt = computed(() =>
  new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(props.arcadeGame.createdAt)),
);

</script>

<template>
  <article class="arcade-game-card" :class="`arcade-game-card--status-${config.status}`">
    <div class="arcade-game-card__layout">
      <div class="arcade-game-card__body">
        <div class="arcade-game-card__status-zone">
          <button class="arcade-game-card__status" :class="[
            `arcade-game-card__status--${config.status}`,
            { 'arcade-game-card__status--open': statusPopoverVisible },
          ]" :id="statusTriggerId" type="button" :aria-label="config.label" :aria-describedby="statusPanelId"
            @mouseenter="showStatus" @mouseleave="hideStatus"
            @pointerdown="onStatusPointerDown" @focus="showStatus" @blur="hideStatus" @click.stop>
            <i :class="config.icon" aria-hidden="true"></i>
          </button>
        </div>

        <div class="arcade-game-card__text">
          <div class="arcade-game-card__title-row">
            <h3 class="arcade-game-card__title">{{ startedAt }}</h3>
          </div>

          <p class="arcade-game-card__meta">
            {{ arcadeGame.difficulty.name }}
          </p>
        </div>

        <div class="arcade-game-card__word">
          <HistoryDisplayWord :display-word="arcadeGame.word" />
        </div>
      </div>

      <div class="arcade-game-card__action-zone">
        <UiButton class="arcade-game-card__action" size="md" variant="soft" :to="to">
          <span>Открыть</span>
          <i class="pi pi-arrow-right" aria-hidden="true"></i>
        </UiButton>
      </div>
    </div>

    <Popover :ref="setStatusPopover" :pt="statusPopoverPt" class="arcade-game-card__popover info-popover"
      @show="onStatusPopoverShow" @hide="onStatusPopoverHide">
      <div class="arcade-game-card__popover-content">
        {{ config.label }}
      </div>
    </Popover>
  </article>
</template>

<style scoped>
.arcade-game-card {
  --arcade-history-info-width: 120px;
  min-width: 0;
  container-type: inline-size;
  padding: 10px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
}

.arcade-game-card--status-active,
.arcade-game-card--status-cancelled {
  border-color: var(--color-gray-300);
  background: white;
}

.arcade-game-card--status-guessed {
  border-color: var(--color-primary-500);
  background: white;
}

.arcade-game-card--status-surrendered {
  border-color: var(--color-red-600);
}

.arcade-game-card__layout {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 10px;
}

.arcade-game-card__body {
  min-width: 0;
  flex: 1 1 auto;
  display: flex;
  align-items: center;
  gap: 0;
}

.arcade-game-card__text {
  min-width: 0;
  flex: 0 0 var(--arcade-history-info-width);
  padding-right: 8px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.arcade-game-card__title-row {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 6px;
}

.arcade-game-card__title,
.arcade-game-card__meta {
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.arcade-game-card__title {
  min-width: 0;
  color: var(--color-gray-900);
  font-size: var(--history-card-title-font-size);
  font-weight: 400;
  line-height: 1.2;
}

.arcade-game-card__status-zone {
  width: 40px;
  min-width: 40px;
  height: 40px;
  flex: 0 0 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.arcade-game-card__status {
  width: 40px;
  min-width: 40px;
  height: 40px;
  position: relative;
  border: 0;
  border-radius: 8px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--color-primary-600);
  cursor: help;
  transition: color 0.16s ease;
}

.arcade-game-card__status::before {
  content: "";
  position: absolute;
  inset: 4px;
  border-radius: 8px;
  background: transparent;
  transition: background-color 0.16s ease;
}

.arcade-game-card__status--open::before {
  background: var(--color-primary-50);
}

.arcade-game-card__status--active {
  color: var(--p-text-muted-color);
}

.arcade-game-card__status--cancelled {
  color: var(--p-text-muted-color);
}

.arcade-game-card__status--guessed {
  color: var(--color-primary-600);
}

.arcade-game-card__status--surrendered {
  color: var(--color-red-600);
}

.arcade-game-card__status--active.arcade-game-card__status--open::before {
  background: var(--color-gray-100);
}

.arcade-game-card__status--guessed.arcade-game-card__status--open::before,
.arcade-game-card__status--surrendered.arcade-game-card__status--open::before,
.arcade-game-card__status--cancelled.arcade-game-card__status--open::before {
  background: var(--color-gray-100);
}

@media (hover: hover) and (pointer: fine) {
  .arcade-game-card__status:hover::before {
    background: var(--color-primary-50);
  }

  .arcade-game-card__status--active:hover::before,
  .arcade-game-card__status--guessed:hover::before,
  .arcade-game-card__status--surrendered:hover::before {
    background: var(--color-gray-100);
  }
}

.arcade-game-card__status:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.arcade-game-card__status .pi {
  position: relative;
  z-index: 1;
  font-size: var(--history-card-status-icon-size);
  line-height: 1;
}

.arcade-game-card__popover-content {
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.35;
}

.arcade-game-card__meta {
  color: var(--color-gray-600);
  font-size: 13px;
  line-height: 1.3;
}

.arcade-game-card__word {
  position: relative;
  min-width: 0;
  flex: 1 1 148px;
  max-width: 420px;
  height: 100%;
  padding-left: 10px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.arcade-game-card__word :deep(.history-display-word) {
  --history-display-word-max-width: 420px;
}

.arcade-game-card__action-zone {
  min-width: 0;
  flex: 0 0 auto;
  height: 100%;
  margin-left: auto;
  padding-left: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.arcade-game-card__action.ui-button {
  width: max-content;
  min-height: 40px;
  padding-inline: 12px;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-size: 14px;
  line-height: 1;
  white-space: nowrap;
}

.arcade-game-card__action .pi {
  font-size: 14px;
}

@container (max-width: 460px) {
  .arcade-game-card {
    padding-block: 8px;
  }

  .arcade-game-card__layout {
    display: grid;
    grid-template-columns: 40px minmax(0, 1fr) max-content;
    grid-template-areas:
      "status text action"
      "word   word word";
    align-items: center;
    row-gap: 8px;
    column-gap: 0;
  }

  .arcade-game-card__body {
    display: contents;
  }

  .arcade-game-card__status-zone {
    grid-area: status;
  }

  .arcade-game-card__text {
    grid-area: text;
    flex: 0 1 auto;
    width: 100%;
    padding-right: 0;
    align-items: flex-start;
    text-align: left;
  }

  .arcade-game-card__action-zone {
    grid-area: action;
    justify-self: end;
    width: max-content;
    height: auto;
    margin-left: 0;
    padding-left: 0;
    border-left: 0;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .arcade-game-card__action.ui-button {
    min-height: 36px;
    font-size: 13px;
  }

  .arcade-game-card__action .pi {
    font-size: 13px;
  }

  .arcade-game-card__word {
    grid-area: word;
    box-sizing: border-box;
    flex-basis: auto;
    width: 100%;
    max-width: 100%;
    min-height: 22px;
    border-top: 1px solid var(--color-gray-200);
    padding-top: 6px;
    padding-left: 0;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
  }

  .arcade-game-card__word :deep(.history-display-word) {
    width: fit-content;
    max-width: 100%;
    justify-content: center;
  }

  .arcade-game-card__title,
  .arcade-game-card__meta {
    width: 100%;
    text-align: left;
  }

  .arcade-game-card__title-row {
    width: 100%;
    justify-content: flex-start;
  }

  .arcade-game-card__title {
    font-size: 14px;
  }

  .arcade-game-card__meta {
    font-size: 12px;
  }

}

@container (min-width: 720px) {
  .arcade-game-card {
    --arcade-history-info-width: 108px;
  }
}

@media (max-width: 640px) {
  .arcade-game-card {
    width: min(100%, 360px);
    margin-inline: auto;
  }
}
</style>
