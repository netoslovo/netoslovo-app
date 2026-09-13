<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import Popover from "primevue/popover";
import { computed, onBeforeUnmount, onMounted, ref, useId } from "vue";
import { showToast } from "../../../shared/notifications/toastStore";
import { dismissKeyboardAndWaitForViewport } from "../../../shared/lib/dismissKeyboardAndWaitForViewport";
import UiMenu, { type UiMenuItem } from "../../../shared/ui/UiMenu.vue";
import { useGuessHints } from "./useGuessHints";
import type { GameActionResult } from "../lib/gameActionOutcomes";
import type { HintsInfo } from "../model/game";
import DictionaryChangedDialog from "./DictionaryChangedDialog.vue";
import GameGuide from "../components/GameGuide.vue";
import GuessForm from "./GuessForm.vue";
import HintMenuItem from "./HintMenuItem.vue";
import SurrenderDialog from "./SurrenderDialog.vue";
import WordNotFoundInfoDialog from "./WordNotFoundInfoDialog.vue";

type MenuRef = {
  close: () => void;
};
type PopoverRef = {
  toggle: (event: Event) => void;
  hide: () => void;
};

export type GameplayActions = {
  submitGuess: (word: string) => Promise<GameActionResult>;
  surrender: () => Promise<GameActionResult>;
  revealHalfwayWord: () => Promise<GameActionResult>;
  revealWordLength: () => Promise<GameActionResult>;
  revealRandomLetter: () => Promise<GameActionResult>;
};

const props = defineProps<{
  modelValue: string;
  loading: boolean;
  wordLength?: number | null;
  hintsInfo: HintsInfo;
  actions: GameplayActions;
  showStatistics?: () => void;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

const word = computed({
  get: () => props.modelValue,
  set: (value: string) => emit("update:modelValue", value),
});
const showScrollTop = ref(false);

function updateScrollTopVisibility() {
  showScrollTop.value = window.scrollY > 1;
}

async function scrollToTop() {
  actionsMenu.value?.close();
  closeHintsPopover();
  await dismissKeyboardAndWaitForViewport();
  window.scrollTo({
    top: 0,
    behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth",
  });
}

onMounted(() => {
  updateScrollTopVisibility();
  window.addEventListener("scroll", updateScrollTopVisibility, { passive: true });
});

onBeforeUnmount(() => window.removeEventListener("scroll", updateScrollTopVisibility));

const hintsPopover = ref<PopoverRef | null>(null);
const actionsMenu = ref<MenuRef | null>(null);
const hintsOpen = ref(false);
const hintsPopoverId = `guess-hints-${useId()}`;
const hintsTitleId = `${hintsPopoverId}-title`;
const pendingHint = ref<"halfway" | "length" | "letter" | null>(null);
const surrenderSubmitting = ref(false);
const howToPlayOpen = ref(false);
const surrenderDialogOpen = ref(false);
const wordNotFoundInfoOpen = ref(false);
const dictionaryChangedDialogOpen = ref(false);
const dictionaryChangedAction = ref<"guess" | "hint">("guess");
let hintsViewportListenersBound = false;
let hintsToggleRequestId = 0;

const {
  remainingNeighbourHints,
  totalNeighbourHints,
  remainingRevealLetterHints,
  totalRevealLetterHints,
  canRevealHalfwayWord,
  nextHalfwayWordPenalty,
  halfwayWordHintDisabledReason,
  canRevealWordLength,
  nextWordLengthPenalty,
  wordLengthHintDisabledReason,
  canRevealRandomLetter,
  nextRandomLetterPenalty,
  halfwayWordDescription,
  wordLengthDescription,
  randomLetterDescription,
  randomLetterHintDisabledReason,
} = useGuessHints(props);

const actionMenuItems = computed<UiMenuItem[]>(() => {
  const items: UiMenuItem[] = [];

  if (props.showStatistics) {
    items.push({
      label: "Статистика",
      icon: "pi pi-chart-bar",
      description: "Показать статистику слова дня.",
      activate: () => props.showStatistics?.(),
    });
  }

  items.push({
    label: "Как играть",
    icon: "pi pi-question-circle",
    description: "Показать правила игры.",
    activate: (_event, close) => showHowToPlay(close),
  });

  items.push({
    label: "Сдаться",
    icon: "pi pi-flag",
    description: "Откроет слово и завершит текущую игру.",
    tone: "danger",
    activate: (_event, close) => requestSurrender(close),
  });

  return [{ label: "Меню", items }];
});

async function submitGuess(value: string) {
  const result = await props.actions.submitGuess(value);
  presentResult(result);
  return { clear: result.status === "success" || result.status === "failed" };
}

function presentResult(result: GameActionResult) {
  if (result.status === "dictionary-changed") {
    dictionaryChangedAction.value = result.action;
    dictionaryChangedDialogOpen.value = true;
    return;
  }

  if (result.status !== "failed") return;

  showToast({
    status: "error",
    title: result.title,
    message: result.message,
    ...(result.wordNotFound && {
      actionLabel: "Что это значит?",
      onAction: () => { wordNotFoundInfoOpen.value = true; },
    }),
  });
}

function requestSurrender(closeMenu?: () => void) {
  closeMenu?.();
  surrenderDialogOpen.value = true;
}

function showHowToPlay(closeMenu?: () => void) {
  closeMenu?.();
  howToPlayOpen.value = true;
}

async function toggleHintsPopover(event: Event) {
  actionsMenu.value?.close();
  if (hintsOpen.value) {
    closeHintsPopover();
    return;
  }

  const anchor = event.currentTarget;
  if (!(anchor instanceof HTMLElement)) return;

  const requestId = ++hintsToggleRequestId;
  await dismissKeyboardAndWaitForViewport();
  if (requestId !== hintsToggleRequestId || !anchor.isConnected || hintsOpen.value) return;
  hintsPopover.value?.toggle({ currentTarget: anchor, target: anchor } as unknown as Event);
}

function closeHintsPopover() {
  hintsToggleRequestId += 1;
  hintsPopover.value?.hide();
}

function onHintsPopoverShow() {
  hintsOpen.value = true;
  if (hintsViewportListenersBound) return;
  hintsViewportListenersBound = true;
  window.addEventListener("resize", closeHintsPopover);
  window.visualViewport?.addEventListener("resize", closeHintsPopover);
  window.visualViewport?.addEventListener("scroll", closeHintsPopover);
}

function onHintsPopoverHide() {
  hintsOpen.value = false;
  removeHintsViewportListeners();
}

function removeHintsViewportListeners() {
  if (!hintsViewportListenersBound) return;
  hintsViewportListenersBound = false;
  window.removeEventListener("resize", closeHintsPopover);
  window.visualViewport?.removeEventListener("resize", closeHintsPopover);
  window.visualViewport?.removeEventListener("scroll", closeHintsPopover);
}

onBeforeUnmount(() => {
  hintsToggleRequestId += 1;
  removeHintsViewportListeners();
});

async function confirmSurrender() {
  if (props.loading) return;

  surrenderSubmitting.value = true;
  try {
    const result = await props.actions.surrender();
    presentResult(result);
    if (result.status === "success") {
      word.value = "";
      surrenderDialogOpen.value = false;
    }
  } finally {
    surrenderSubmitting.value = false;
  }
}

async function requestHint(
  kind: "halfway" | "length" | "letter",
  request: () => Promise<GameActionResult>,
  closeMenu?: () => void,
) {
  pendingHint.value = kind;
  try {
    const result = await request();
    presentResult(result);
    if (result.status === "success") word.value = "";
  } finally {
    pendingHint.value = null;
    closeMenu?.();
  }
}

function requestHalfwayWordHint(closeMenu?: () => void) {
  if (props.loading) return;
  if (!canRevealHalfwayWord.value) {
    showToast({ status: "info", title: "Промежуточное слово", message: halfwayWordHintDisabledReason.value ?? "Сейчас нельзя использовать эту подсказку" });
    return;
  }
  return requestHint("halfway", props.actions.revealHalfwayWord, closeMenu);
}

function requestWordLengthHint(closeMenu?: () => void) {
  if (props.loading) return;
  if (!canRevealWordLength.value) {
    showToast({ status: "info", title: "Длина слова", message: wordLengthHintDisabledReason.value ?? "Сейчас нельзя использовать эту подсказку" });
    return;
  }
  return requestHint("length", props.actions.revealWordLength, closeMenu);
}

function requestRandomLetterHint(closeMenu?: () => void) {
  if (props.loading) return;
  if (!canRevealRandomLetter.value) {
    showToast({ status: "info", title: "Случайная буква", message: randomLetterHintDisabledReason.value ?? "Сейчас нельзя использовать эту подсказку" });
    return;
  }
  return requestHint("letter", props.actions.revealRandomLetter, closeMenu);
}
</script>

<template>
  <div class="gameplay-controls">
    <div class="gameplay-controls__input-row">
      <GuessForm v-model="word" class="gameplay-controls__form" :loading="loading" :submit="submitGuess">
        <template #before-submit>
          <Button class="gameplay-controls__hints-button gameplay-controls__hints-button--inline" type="button"
            aria-label="Подсказки" aria-haspopup="dialog" :aria-expanded="hintsOpen ? 'true' : 'false'"
            :aria-controls="hintsPopoverId" icon="pi pi-lightbulb" severity="secondary" outlined
            @pointerdown.prevent @click="toggleHintsPopover" />
        </template>
      </GuessForm>

      <div class="gameplay-controls__actions">
        <Button class="gameplay-controls__hints-button" type="button" aria-label="Подсказки" aria-haspopup="dialog"
          :aria-expanded="hintsOpen ? 'true' : 'false'" :aria-controls="hintsPopoverId" icon="pi pi-lightbulb"
          severity="secondary" outlined @pointerdown.prevent @click="toggleHintsPopover" />

        <Popover :id="hintsPopoverId" ref="hintsPopover" class="gameplay-controls__hints-popover"
          :aria-labelledby="hintsTitleId" @show="onHintsPopoverShow" @hide="onHintsPopoverHide">
          <div>
            <div :id="hintsTitleId" class="gameplay-controls__hints-title">Подсказки</div>
            <HintMenuItem title="Промежуточное слово" icon="pi pi-sort-amount-up" :description="halfwayWordDescription"
              :remaining="remainingNeighbourHints" :total="totalNeighbourHints" :score-penalty="nextHalfwayWordPenalty"
              :disabled="loading" :unavailable="!canRevealHalfwayWord" :loading="pendingHint === 'halfway'"
              :disabled-reason="halfwayWordHintDisabledReason" @activate="requestHalfwayWordHint(closeHintsPopover)" />
            <HintMenuItem title="Показать длину слова" icon="pi pi-eye" :description="wordLengthDescription"
              :score-penalty="nextWordLengthPenalty" :disabled="loading" :unavailable="!canRevealWordLength"
              :loading="pendingHint === 'length'" :disabled-reason="wordLengthHintDisabledReason"
              @activate="requestWordLengthHint(closeHintsPopover)" />
            <HintMenuItem title="Открыть случайную букву" icon="pi pi-question" :description="randomLetterDescription"
              :remaining="remainingRevealLetterHints" :total="totalRevealLetterHints"
              :score-penalty="nextRandomLetterPenalty" :disabled="loading" :unavailable="!canRevealRandomLetter"
              :loading="pendingHint === 'letter'" :disabled-reason="randomLetterHintDisabledReason"
              @activate="requestRandomLetterHint(closeHintsPopover)" />
          </div>
        </Popover>

        <div class="gameplay-controls__menu-anchor">
          <Button v-if="showScrollTop" class="gameplay-controls__scroll-top" type="button"
            aria-label="Наверх" icon="pi pi-arrow-up" severity="secondary" outlined
            @pointerdown.prevent @click="scrollToTop" />
          <UiMenu ref="actionsMenu" :items="actionMenuItems" button-label="Меню" icon="dots" tone="neutral" wide
            :before-open="dismissKeyboardAndWaitForViewport" @open="closeHintsPopover" />
        </div>
      </div>
    </div>

    <Dialog v-model:visible="howToPlayOpen" modal dismissable-mask header="Об игре" class="game-dialog">
      <GameGuide :open="howToPlayOpen" @close="howToPlayOpen = false" />
    </Dialog>
    <SurrenderDialog :visible="surrenderDialogOpen" :loading="surrenderSubmitting" @close="surrenderDialogOpen = false"
      @confirm="confirmSurrender" />
    <WordNotFoundInfoDialog v-model:visible="wordNotFoundInfoOpen" />
    <DictionaryChangedDialog :visible="dictionaryChangedDialogOpen" :action="dictionaryChangedAction"
      @close="dictionaryChangedDialogOpen = false" />
  </div>
</template>

<style scoped>
.gameplay-controls {
  --gameplay-glass-background: linear-gradient(120deg, rgb(255 255 255 / 90%), rgb(255 255 255 / 82%) 45%, rgb(255 255 255 / 88%));
  --gameplay-glass-filter: blur(6px) saturate(110%);
  --guess-control-height: 50px;
  --guess-submit-background: var(--p-primary-color);
  --guess-submit-color: var(--p-primary-contrast-color);
  --guess-submit-hover-background: var(--p-primary-hover-color);
  --guess-submit-hover-color: var(--p-primary-contrast-color);
  --guess-submit-active-background: var(--p-primary-active-color);
  --guess-submit-active-color: var(--p-primary-contrast-color);
  width: 100%;
  min-width: 0;
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  align-items: stretch;
  gap: 8px;
}

.gameplay-controls__actions {
  grid-area: actions;
  display: flex;
  align-items: center;
  gap: 8px;
}

.gameplay-controls__menu-anchor {
  position: relative;
  display: flex;
}

.gameplay-controls__scroll-top.p-button {
  --scroll-top-background: var(--gameplay-glass-background);
  --scroll-top-shadow: inset 0 1px 0 rgb(255 255 255 / 60%), inset 0 -1px 0 rgb(25 32 43 / 3%), 0 2px 8px rgb(25 32 43 / 5%);
  position: absolute;
  bottom: calc(100% + 12px);
  left: 0;
  transform: none;
  width: 50px;
  min-width: 0;
  height: 50px;
  padding: 0;
  border: 1px solid color-mix(in srgb, var(--color-gray-200) 65%, transparent);
  border-radius: 12px;
  background: var(--scroll-top-background);
  -webkit-backdrop-filter: var(--gameplay-glass-filter);
  backdrop-filter: var(--gameplay-glass-filter);
  box-shadow: var(--scroll-top-shadow);
  color: var(--color-gray-600);
  opacity: 1;
  transition: color 0.15s ease;
  touch-action: manipulation;
  -webkit-tap-highlight-color: transparent;
}

.gameplay-controls__scroll-top.p-button:not(:disabled):is(:hover, :active, :focus) {
  border-color: color-mix(in srgb, var(--color-gray-200) 65%, transparent);
  background: var(--scroll-top-background);
  box-shadow: var(--scroll-top-shadow);
  transform: none;
}

.gameplay-controls__scroll-top.p-button:focus-visible {
  outline: 2px solid var(--color-gray-300);
  outline-offset: 2px;
}

.gameplay-controls__scroll-top.p-button:not(:disabled):active {
  color: var(--color-gray-700);
}

@media (hover: hover) and (pointer: fine) {
  .gameplay-controls__scroll-top.p-button:not(:disabled):hover {
    color: var(--color-gray-700);
  }
}

.gameplay-controls__scroll-top :deep(.p-button-icon) {
  opacity: 1;
  color: inherit;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  font-size: 18px;
  line-height: 1;
}

@media (width < 1024px) {
  .gameplay-controls__scroll-top.p-button {
    left: -5px;
    width: calc(var(--guess-control-height) + 2px);
    height: calc(var(--guess-control-height) + 2px);
  }
}

.gameplay-controls__hints-button.p-button,
.gameplay-controls :deep(.ui-menu__button.p-button) {
  width: 50px;
  min-width: 50px;
  height: 50px;
}

.gameplay-controls__hints-button.p-button {
  padding: 0;
  border-color: var(--color-gray-300);
  border-radius: 8px;
  background: white;
  color: var(--color-primary-600);
  touch-action: manipulation;
  -webkit-tap-highlight-color: transparent;
  transition: background-color 0.18s ease, border-color 0.18s ease, color 0.18s ease, box-shadow 0.18s ease;
}

.gameplay-controls :deep(.ui-menu__button--neutral.p-button.p-button-outlined) {
  border: 1px solid var(--color-gray-300);
}

.gameplay-controls__hints-button.p-button:not(:disabled):active {
  border-color: var(--color-gray-400);
  background: var(--color-gray-100);
  color: var(--color-primary-600);
}

.gameplay-controls__hints-button :deep(.p-button-icon) {
  font-size: 18px;
}

:global(.gameplay-controls__hints-popover.p-popover) {
  min-width: 292px;
  max-width: calc(100vw - 24px);
  box-shadow: var(--shadow-menu);
}

:global(.gameplay-controls__hints-popover.p-popover .p-popover-content) {
  max-height: min(70dvh, 520px);
  padding: 8px 0;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.gameplay-controls__hints-title {
  min-height: 32px;
  padding: 0 14px;
  display: flex;
  align-items: center;
  color: var(--p-text-muted-color);
  font-size: 14px;
}

@media (hover: hover) and (pointer: fine) {
  .gameplay-controls__hints-button.p-button:not(:disabled):hover {
    border-color: var(--color-gray-400);
    background: var(--color-gray-100);
    color: var(--color-primary-600);
  }
}

@media (hover: none),
(pointer: coarse) {
  .gameplay-controls__hints-button.p-button:not(:disabled):hover {
    border-color: var(--color-gray-300);
    background: white;
    color: var(--color-primary-600);
  }

  .gameplay-controls__hints-button.p-button:not(:disabled):active {
    border-color: var(--color-gray-400);
    background: var(--color-gray-100);
    color: var(--color-primary-600);
  }
}

@media (width < 1024px) {
  .gameplay-controls__menu-anchor {
    order: -1;
  }
}

@media (min-width: 481px) and (max-width: 1024px) {
  .gameplay-controls {
    --guess-control-height: 46px;
  }

  .gameplay-controls__hints-button.p-button,
  .gameplay-controls__hints-button.p-button.p-button-icon-only,
  .gameplay-controls :deep(.ui-menu__button.p-button),
  .gameplay-controls :deep(.ui-menu__button.p-button.p-button-icon-only) {
    width: 46px;
    min-width: 46px;
    height: var(--guess-control-height);
  }
}

@media (max-width: 480px) {
  .gameplay-controls {
    --guess-control-height: 44px;
  }

  .gameplay-controls__hints-button.p-button,
  .gameplay-controls__hints-button.p-button.p-button-icon-only,
  .gameplay-controls :deep(.ui-menu__button.p-button),
  .gameplay-controls :deep(.ui-menu__button.p-button.p-button-icon-only) {
    width: 44px;
    min-width: 44px;
    height: var(--guess-control-height);
  }

  :global(.gameplay-controls__hints-popover.p-popover) {
    width: min(320px, calc(100vw - 24px));
    min-width: 0;
    max-width: calc(100vw - 24px);
  }

  :global(.gameplay-controls__hints-popover.p-popover .p-popover-content) {
    padding-block: 6px;
  }

  .gameplay-controls__hints-title {
    min-height: 30px;
    padding-inline: 12px;
    font-size: clamp(14px, 4vw, 15px);
  }

}

@media (max-width: 359px) {
  .gameplay-controls {
    --guess-control-height: 42px;
  }

  .gameplay-controls__hints-button.p-button,
  .gameplay-controls__hints-button.p-button.p-button-icon-only,
  .gameplay-controls :deep(.ui-menu__button.p-button),
  .gameplay-controls :deep(.ui-menu__button.p-button.p-button-icon-only) {
    width: 42px;
    min-width: 42px;
    height: var(--guess-control-height);
  }
}

.gameplay-controls__input-row {
  min-width: 0;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  grid-template-areas: "form actions";
  align-items: center;
  gap: 8px;
}

.gameplay-controls__form {
  grid-area: form;
}

@media (width < 1024px) {
  .gameplay-controls {
    width: 100%;
    gap: 6px;
  }

  .gameplay-controls__actions,
  .gameplay-controls__input-row {
    gap: 6px;
  }

  .gameplay-controls__input-row {
    grid-template-columns: auto minmax(0, 1fr);
    grid-template-areas: "actions form";
  }

  :global(.gameplay-controls__hints-popover.p-popover) {
    width: min(320px, calc(100vw - 24px));
    min-width: 0;
    max-width: 320px;
  }

}

@media (width < 1024px) {
  .gameplay-controls {
    --guess-submit-background: transparent;
    --guess-submit-color: var(--p-primary-color);
    --guess-submit-hover-background: transparent;
    --guess-submit-hover-color: var(--p-primary-hover-color);
    --guess-submit-active-background: transparent;
    --guess-submit-active-color: var(--p-primary-active-color);
  }

  .gameplay-controls__input-row {
    position: relative;
    isolation: isolate;
    gap: 0;
    padding-inline: 4px;
    border: 1px solid color-mix(in srgb, var(--color-gray-200) 65%, transparent);
    border-radius: 12px;
    background: transparent;
    box-shadow: inset 0 1px 0 rgb(255 255 255 / 60%), inset 0 -1px 0 rgb(25 32 43 / 3%), 0 2px 8px rgb(25 32 43 / 5%);
  }

  .gameplay-controls__input-row::before {
    content: "";
    position: absolute;
    z-index: -1;
    inset: 0;
    border-radius: inherit;
    pointer-events: none;
    background: var(--gameplay-glass-background);
    -webkit-backdrop-filter: var(--gameplay-glass-filter);
    backdrop-filter: var(--gameplay-glass-filter);
  }

  .gameplay-controls__actions {
    gap: 0;
  }

  .gameplay-controls .gameplay-controls__hints-button.p-button,
  .gameplay-controls .gameplay-controls__hints-button.p-button:is(:hover, :active),
  .gameplay-controls :deep(.ui-menu__button--neutral.p-button.p-button-outlined),
  .gameplay-controls :deep(.ui-menu__button--neutral.p-button.p-button-outlined:is(:hover, :active)) {
    border: 0;
    background: transparent;
    box-shadow: none;
  }

  .gameplay-controls .gameplay-controls__hints-button.p-button:is(:hover, :active),
  .gameplay-controls :deep(.ui-menu__button--neutral.p-button.p-button-outlined:is(:hover, :active)) {
    color: var(--p-primary-hover-color);
  }
}

.gameplay-controls__hints-button--inline.p-button {
  display: none;
}

@media (width < 1024px) {
  .gameplay-controls__actions > .gameplay-controls__hints-button.p-button {
    display: none;
  }

  .gameplay-controls__hints-button--inline.p-button {
    display: inline-flex;
  }
}

@media (width < 1024px) {
  .gameplay-controls__hints-button :deep(.p-button-icon),
  .gameplay-controls :deep(.ui-menu__button .p-button-icon) {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
    font-size: 18px;
    line-height: 1;
  }
}

</style>
