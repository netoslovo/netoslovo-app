<script setup lang="ts">
import Popover from "primevue/popover";
import Select from "primevue/select";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import type { Difficulty } from "../model/game";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  difficulties: Difficulty[];
  modelValue: string;
  creating: boolean;
  canStartArcadeGame: boolean;
  gameplayLocked: boolean;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
  createGame: [];
}>();

const {
  triggerId: difficultyInfoTriggerId,
  panelId: difficultyInfoPanelId,
  popoverPt: difficultyInfoPopoverPt,
  setPopover: setDifficultyInfoPopover,
  visible: difficultyInfoOpen,
  show: showDifficultyInfo,
  hide: hideDifficultyInfo,
  onPointerDown: onDifficultyInfoPointerDown,
  onShow: onDifficultyInfoPopoverShow,
  onHide: onDifficultyInfoPopoverHide,
} = useInfoPopover();

function requestCreateGame() {
  emit("createGame");
}
</script>

<template>
  <div class="arcade-create">
    <section class="arcade-create__panel">
      <div class="arcade-create__header">
        <h1 class="arcade-create__title">Новая игра</h1>
      </div>

      <form class="arcade-create__form" @submit.prevent="requestCreateGame">
        <div class="arcade-create__difficulty">
          <div class="arcade-create__label-row">
            <span class="arcade-create__label">Сложность</span>

            <button class="arcade-create__info" :class="{ 'arcade-create__info--open': difficultyInfoOpen }"
              :id="difficultyInfoTriggerId" type="button" aria-label="О сложности"
              :aria-describedby="difficultyInfoPanelId" @mouseenter="showDifficultyInfo"
              @mouseleave="hideDifficultyInfo" @pointerdown="onDifficultyInfoPointerDown"
              @focus="showDifficultyInfo" @blur="hideDifficultyInfo" @click.stop>
              <i class="pi pi-info-circle" aria-hidden="true"></i>
            </button>
          </div>

          <Select class="arcade-create__select" :model-value="modelValue" :options="difficulties" option-label="name"
            option-value="code" placeholder="Выберите сложность"
            :disabled="creating || !canStartArcadeGame || gameplayLocked" fluid
            @update:model-value="emit('update:modelValue', $event)" />
        </div>

        <UiButton type="submit" size="md" :loading="creating" :disabled="!canStartArcadeGame || gameplayLocked">
          Начать игру
        </UiButton>
      </form>

      <div v-if="!creating && (gameplayLocked || !canStartArcadeGame)" class="arcade-create__status">
        {{ gameplayLocked ? "Попробуйте позже" : "Сложности недоступны" }}
      </div>
    </section>

    <Popover :ref="setDifficultyInfoPopover" :pt="difficultyInfoPopoverPt" class="arcade-create__popover info-popover"
      @show="onDifficultyInfoPopoverShow" @hide="onDifficultyInfoPopoverHide">
      <div class="arcade-create__popover-content">
        Сложность рассчитывается приблизительно: учитываются частотность слова,
        его длина и другие параметры. Для человека реальная сложность может отличаться.
      </div>
    </Popover>
  </div>
</template>

<style scoped>
.arcade-create {
  flex: 1;
  min-height: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow-y: auto;
  padding: 12px 0;
}

.arcade-create__panel {
  width: min(100%, 380px);
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
}

.arcade-create__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.arcade-create__title {
  margin: 0;
  color: var(--color-gray-900);
  font-size: 18px;
  font-weight: 500;
  line-height: 1.2;
}

.arcade-create__form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.arcade-create__difficulty {
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: var(--color-gray-700);
}

.arcade-create__label-row {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.arcade-create__label {
  font-size: 15px;
}

.arcade-create__info {
  width: 30px;
  min-width: 30px;
  height: 30px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.94);
  color: var(--p-surface-500);
  cursor: help;
  transition:
    color 0.16s ease,
    background-color 0.16s ease;
}

.arcade-create__info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
}

@media (hover: hover) and (pointer: fine) {
  .arcade-create__info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-600);
  }
}

.arcade-create__info:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.arcade-create__info .pi {
  font-size: 16px;
}

.arcade-create__select.p-select {
  min-height: 40px;
  border-radius: 8px;
}

.arcade-create__form>.ui-button.p-button {
  width: 100%;
}

.arcade-create__status {
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.35;
  text-align: center;
}

.arcade-create__popover-content {
  margin: 0;
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

:global(.arcade-create__popover.p-popover) {
  max-width: min(300px, calc(100vw - 24px));
  color: var(--color-gray-700);
}

@media (min-width: 768px) {
  .arcade-create__panel {
    padding: 18px;
  }

  .arcade-create__title {
    font-size: 19px;
  }
}

@media (max-height: 760px) {
  .arcade-create__panel {
    gap: 10px;
    padding: 12px;
  }

  .arcade-create__title {
    font-size: 17px;
  }
}

@media (max-width: 480px) {
  .arcade-create {
    padding: 2px 0 8px;
  }

  .arcade-create__panel {
    gap: 10px;
    padding: 12px;
    border-radius: 12px;
  }

  .arcade-create__title {
    font-size: 17px;
  }

  .arcade-create__form {
    gap: 8px;
  }

  .arcade-create__form>.ui-button.p-button {
    min-height: 36px;
    padding-inline: 12px;
    font-size: 15px;
  }
}
</style>
