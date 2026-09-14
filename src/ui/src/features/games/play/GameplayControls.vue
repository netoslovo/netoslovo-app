<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import Popover from "primevue/popover";
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from "vue";
import { showToast } from "../../../shared/notifications/toastStore";
import { useGameplayScroll } from "./useGameplayScroll";
import type { GuessPresentationEvent } from "./useGameSession";
import UiMenu, { type UiMenuItem } from "../../../shared/ui/UiMenu.vue";
import { useGuessHints } from "./useGuessHints";
import type { GameActionResult } from "../lib/gameActionOutcomes";
import type { HintsInfo } from "../model/game";
import DictionaryChangedDialog from "./DictionaryChangedDialog.vue";
import GameGuide from "../components/GameGuide.vue";
import GuessForm from "./GuessForm.vue";
import GuessHintsPanel from "./GuessHintsPanel.vue";
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
  gameId: string;
  presentationEvent: GuessPresentationEvent | null;
  dialogOpen: boolean;
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
const mobileMedia = window.matchMedia("(width < 1024px)");
const mobile = ref(mobileMedia.matches);
const view = ref<"game" | "hints" | "actions">("game");
const panelKind = ref<"hints" | "actions">("hints");
const scrollport = ref<HTMLElement | null>(null);
const gameContent = ref<HTMLElement | null>(null);
const mobilePanel = ref<HTMLElement | null>(null);
const panelId = `game-panel-${useId()}`;
let panelTrigger: HTMLElement | null = null;
let savedGameScrollTop = 0;
let panelKeyboardFocus = false;
let pendingCenterKey: string | null = null;
let viewSequence = 0;
let hintViewSequence: number | null = null;
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
const dialogOpen = computed(() => props.dialogOpen || howToPlayOpen.value || surrenderDialogOpen.value
  || wordNotFoundInfoOpen.value || dictionaryChangedDialogOpen.value);
const { showScrollTop, scrollbarWidth, registerGuess, updateScrollPosition, centerGuess, cancelCentering, restorePosition, scrollToTop: scrollGameToTop } = useGameplayScroll(
  scrollport, gameContent, computed(() => mobile.value && view.value === "game"), dialogOpen,
);

function updateMobileLayout() {
  mobile.value = mobileMedia.matches;
  pendingCenterKey = null;
  hintsOpen.value = false;
  void closePanel(false);
  closeHintsPopover();
  actionsMenu.value?.close();
  cancelCentering();
}

function blurInput() {
  const active = document.activeElement;
  if (active instanceof HTMLInputElement || active instanceof HTMLTextAreaElement) active.blur();
}

function togglePanel(kind: "hints" | "actions", event: Event) {
  if (view.value === kind) {
    void closePanel(event instanceof MouseEvent && event.detail === 0);
    return;
  }
  const trigger = event.currentTarget;
  if (!(trigger instanceof HTMLElement)) return;
  if (view.value === "game") savedGameScrollTop = scrollport.value?.scrollTop ?? 0;
  panelTrigger = trigger;
  panelKeyboardFocus = event instanceof MouseEvent && event.detail === 0;
  cancelCentering();
  blurInput();
  panelKind.value = kind;
  view.value = kind;
  viewSequence += 1;
}

async function onPanelShow() {
  const sequence = viewSequence;
  await nextTick();
  if (sequence !== viewSequence || view.value === "game" || dialogOpen.value) return;
  mobilePanel.value?.focus({ preventScroll: true });
}

async function closePanel(restoreFocus = panelKeyboardFocus) {
  if (view.value === "game") return;
  view.value = "game";
  const sequence = ++viewSequence;
  await nextTick();
  if (sequence !== viewSequence) return;
  restorePosition(savedGameScrollTop);
  if (pendingCenterKey) {
    centerGuess(pendingCenterKey);
    pendingCenterKey = null;
  }
  if (restoreFocus && !dialogOpen.value) panelTrigger?.focus({ preventScroll: true });
}

function onInputFocus(event: FocusEvent) {
  if (event.target instanceof HTMLInputElement) void closePanel(false);
}

async function scrollToTop() {
  cancelCentering();
  actionsMenu.value?.close();
  closeHintsPopover();
  await closePanel(false);
  if (mobile.value) scrollGameToTop();
}

function onPointerMove(event: PointerEvent) {
  if (event.buttons || event.pointerType === "touch") cancelCentering();
}

function onScrollKey(event: KeyboardEvent) {
  if (["ArrowUp", "ArrowDown", "PageUp", "PageDown", "Home", "End", " "].includes(event.key)) cancelCentering();
}

function onEscape(event: KeyboardEvent) {
  if (event.defaultPrevented || view.value === "game" || dialogOpen.value) return;
  event.preventDefault();
  void closePanel(true);
}

onMounted(() => {
  mobileMedia.addEventListener("change", updateMobileLayout);
  document.addEventListener("keydown", onEscape);
});
onBeforeUnmount(() => {
  viewSequence += 1;
  mobileMedia.removeEventListener("change", updateMobileLayout);
  document.removeEventListener("keydown", onEscape);
});

watch(dialogOpen, (open) => {
  if (open) {
    cancelCentering();
    blurInput();
    void closePanel(false);
  }
});
watch(() => props.presentationEvent, async (event) => {
  if (!event || !mobile.value || dialogOpen.value) return;
  const key = `${props.gameId}:${event.word}`;
  // Keep a late result for the next return without closing a different panel.
  if (view.value !== "game") {
    pendingCenterKey = key;
    if (view.value !== "hints" || pendingHint.value !== "halfway" || hintViewSequence !== viewSequence) return;
    await closePanel();
    return;
  }
  const sequence = viewSequence;
  await nextTick();
  if (sequence === viewSequence && event === props.presentationEvent && !dialogOpen.value) centerGuess(key);
}, { flush: "post" });

const {
  canRevealHalfwayWord, halfwayWordHintDisabledReason,
  canRevealWordLength, wordLengthHintDisabledReason,
  canRevealRandomLetter, randomLetterHintDisabledReason,
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

function toggleHintsPopover(event: Event) {
  if (mobile.value) {
    void togglePanel("hints", event);
    return;
  }
  actionsMenu.value?.close();
  hintsPopover.value?.toggle(event);
}

function closeHintsPopover() {
  hintsPopover.value?.hide();
}

function closeHintMenu() {
  closeHintsPopover();
  void closePanel();
}

function requestSelectedHint(kind: "halfway" | "length" | "letter") {
  if (kind === "halfway") return requestHalfwayWordHint(closeHintMenu);
  if (kind === "length") return requestWordLengthHint(closeHintMenu);
  return requestRandomLetterHint(closeHintMenu);
}

function activateMobileAction(item: UiMenuItem, event: Event) {
  if (item.disabled || item.loading) return;
  item.activate?.(event, () => { void closePanel(false); });
  if (!event.defaultPrevented && item.closeOnActivate !== false) void closePanel(false);
}

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
  const requestViewSequence = viewSequence;
  hintViewSequence = requestViewSequence;
  try {
    const result = await request();
    presentResult(result);
    if (result.status === "success") {
      word.value = "";
      if (mobile.value && kind !== "halfway" && viewSequence === requestViewSequence && !dialogOpen.value) {
        await closePanel();
        if (viewSequence === requestViewSequence + 1 && view.value === "game" && !dialogOpen.value) scrollGameToTop();
      }
    }
  } finally {
    pendingHint.value = null;
    hintViewSequence = null;
    if (viewSequence === requestViewSequence) closeMenu?.();
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
  <div class="gameplay-layout" :style="{ '--gameplay-scrollbar-width': `${scrollbarWidth}px` }">
    <div ref="scrollport" class="gameplay-scrollport" :tabindex="mobile ? 0 : undefined" aria-label="Игра"
      :inert="mobile && view !== 'game'"
      @scroll.passive="updateScrollPosition" @pointermove.passive="onPointerMove"
      @touchmove.passive="cancelCentering" @wheel.passive="cancelCentering" @keydown="onScrollKey">
      <div ref="gameContent" class="gameplay-content">
        <slot name="word" />
        <div v-if="mobile" class="gameplay-summary">
          <slot name="summary" :active="view === 'game'" />
        </div>
        <slot name="guesses" :active="view === 'game'" :register-guess="registerGuess" />
      </div>
    </div>
    <div class="gameplay-composer" :inert="mobile && view !== 'game'">
      <slot v-if="!mobile" name="summary" :active="true" />
      <div class="gameplay-controls">
        <Transition name="scroll-top">
          <div v-if="mobile && showScrollTop" class="gameplay-controls__scroll-top-slot">
            <Button class="gameplay-controls__scroll-top" type="button"
              aria-label="Наверх" icon="pi pi-arrow-up" severity="secondary" outlined
              @pointerdown.prevent @click="scrollToTop" />
          </div>
        </Transition>
        <div class="gameplay-controls__input-row">
          <GuessForm v-model="word" class="gameplay-controls__form" :loading="loading" :submit="submitGuess" @focusin="onInputFocus">
            <template #before-submit>
              <Button class="gameplay-controls__hints-button gameplay-controls__hints-button--inline" type="button"
                aria-label="Подсказки" aria-haspopup="dialog" :aria-expanded="(mobile ? view === 'hints' : hintsOpen) ? 'true' : 'false'"
                :aria-controls="mobile ? panelId : hintsPopoverId" icon="pi pi-lightbulb" severity="secondary" outlined
                @pointerdown.prevent @click="toggleHintsPopover" />
            </template>
          </GuessForm>

          <div class="gameplay-controls__actions">
            <Button class="gameplay-controls__hints-button" type="button" aria-label="Подсказки" aria-haspopup="dialog"
              :aria-expanded="hintsOpen ? 'true' : 'false'" :aria-controls="mobile ? panelId : hintsPopoverId" icon="pi pi-lightbulb"
              severity="secondary" outlined @pointerdown.prevent @click="toggleHintsPopover" />

            <Popover v-if="!mobile" :id="hintsPopoverId" ref="hintsPopover" class="gameplay-controls__hints-popover"
              :aria-labelledby="hintsTitleId" @show="hintsOpen = true" @hide="hintsOpen = false">
              <div>
                <div :id="hintsTitleId" class="gameplay-controls__hints-title">Подсказки</div>
                <GuessHintsPanel :hints-info="hintsInfo" :word-length="wordLength" :loading="loading"
                  :pending-hint="pendingHint" @request="requestSelectedHint" />
              </div>
            </Popover>

            <div class="gameplay-controls__menu-anchor">
              <Button v-if="mobile" type="button" class="gameplay-controls__hints-button gameplay-controls__actions-button"
                aria-label="Меню" aria-haspopup="dialog" :aria-expanded="view === 'actions'" :aria-controls="panelId"
                icon="pi pi-ellipsis-h" severity="secondary" outlined @pointerdown.prevent
                @click="togglePanel('actions', $event)" />
              <UiMenu v-else ref="actionsMenu" :items="actionMenuItems" button-label="Меню" icon="dots" tone="neutral" wide
                @open="closeHintsPopover" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <Dialog :visible="mobile && view !== 'game'" :id="panelId" modal dismissable-mask
      position="bottom" :draggable="false" :closable="false" :close-on-escape="false"
      class="gameplay-sheet" :aria-labelledby="`${panelId}-title`"
      :pt="{ mask: { class: 'gameplay-sheet-mask', style: { top: 'var(--app-visual-viewport-top)', height: 'var(--app-visual-viewport-height, 100dvh)' } } }"
      @show="onPanelShow" @update:visible="!$event && closePanel()">
      <template #container>
        <section ref="mobilePanel" class="gameplay-panel" tabindex="-1" :aria-labelledby="`${panelId}-title`">
          <header class="gameplay-panel__header">
            <h2 :id="`${panelId}-title`">{{ panelKind === 'hints' ? 'Подсказки' : 'Действия' }}</h2>
            <Button type="button" icon="pi pi-times" aria-label="Закрыть меню" severity="secondary" text
              @click="closePanel($event.detail === 0)" />
          </header>
          <div class="gameplay-panel__body">
            <GuessHintsPanel v-if="panelKind === 'hints'" :hints-info="hintsInfo" :word-length="wordLength"
              :loading="loading" :pending-hint="pendingHint" @request="requestSelectedHint" />
            <div v-else class="gameplay-panel__actions">
              <button v-for="item in actionMenuItems[0]?.items" :key="item.label" type="button"
                class="gameplay-panel__action" :class="{ 'gameplay-panel__action--danger': item.tone === 'danger' }"
                :disabled="item.disabled || item.loading" @click="activateMobileAction(item, $event)">
                <i :class="item.icon" aria-hidden="true"></i>
                <span><strong>{{ item.label }}</strong><span>{{ item.description }}</span></span>
              </button>
            </div>
          </div>
        </section>
      </template>
    </Dialog>

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
.gameplay-layout,
.gameplay-scrollport,
.gameplay-content {
  display: contents;
}

.gameplay-composer,
.gameplay-summary {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px;
  border: 1px solid var(--color-gray-200);
  border-radius: 10px;
  background: white;
  box-shadow: 0 4px 14px rgba(25, 32, 43, 0.05);
}

.gameplay-composer {
  order: 2;
}

@media (min-width: 768px) {
  .gameplay-composer,
  .gameplay-summary {
    gap: 10px;
    padding: 10px;
  }
}

.gameplay-panel:focus,
.gameplay-scrollport:focus:not(:focus-visible) {
  outline: none;
}

:global(.gameplay-sheet.p-dialog) {
  width: min(calc(100% - 16px), var(--container-sm));
  max-height: calc(var(--app-visual-viewport-height, 100dvh) - 24px);
  margin: 0 8px max(8px, var(--app-visual-viewport-safe-bottom));
  border: 0;
  border-radius: 8px;
  background: white;
  box-shadow: 0 -8px 32px rgb(25 32 43 / 18%);
}

:global(.gameplay-sheet-mask) {
  background: rgb(0 0 0 / 40%);
  overscroll-behavior: contain;
}

.gameplay-panel {
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  overflow: clip;
  padding-top: 8px;
  border-radius: inherit;
}

.gameplay-panel__body {
  min-height: 0;
  overflow-y: auto;
  overscroll-behavior: contain;
  padding: 4px max(16px, env(safe-area-inset-right)) 16px max(16px, env(safe-area-inset-left));
}

.gameplay-panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  flex-shrink: 0;
  padding: 4px max(16px, env(safe-area-inset-right)) 8px max(16px, env(safe-area-inset-left));
}

.gameplay-panel__header h2 {
  margin: 0;
  font-size: 18px;
}

.gameplay-panel__actions {
  display: grid;
  gap: 8px;
}

.gameplay-panel__action {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border: 1px solid var(--color-gray-200);
  border-radius: 10px;
  background: white;
  color: var(--color-gray-800);
  text-align: left;
  cursor: pointer;
}

.gameplay-panel__action > span {
  display: grid;
  gap: 4px;
}

.gameplay-panel__action span span {
  font-size: 14px;
  color: var(--color-gray-600);
}

.gameplay-panel__action--danger {
  color: var(--color-red-600);
}

@media (width < 1024px) {
  .gameplay-layout {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-rows: minmax(0, 1fr) auto;
    overflow: clip;
  }

  .gameplay-scrollport {
    display: block;
    min-height: 0;
    overflow-y: auto;
    overflow-x: hidden;
    overscroll-behavior: none auto;
    overflow-anchor: none;
    padding-block: 8px;
  }

  .gameplay-layout,
  .gameplay-scrollport {
    min-width: 0;
  }

  .gameplay-content,
  .gameplay-composer {
    width: 100%;
    max-width: var(--container-sm);
    min-width: 0;
    margin-inline: auto;
    padding-inline: max(16px, env(safe-area-inset-left)) max(16px, env(safe-area-inset-right));
  }

  .gameplay-content {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .gameplay-composer {
    min-width: 0;
    /* Extend only the left edge to match content centered beside the scrollbar. */
    padding-inline-start: calc(max(16px, env(safe-area-inset-left)) - max(0px, min(var(--gameplay-scrollbar-width) / 2, (100vw - var(--container-sm)) / 2)));
    padding-block: 8px max(8px, var(--app-visual-viewport-safe-bottom));
    border: 0;
    border-radius: 0;
    box-shadow: none;
    background: var(--color-gray-50);
  }
}

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
  gap: 4px;
  display: flex;
}

.gameplay-controls__scroll-top.p-button {
  --scroll-top-background: var(--gameplay-glass-background);
  --scroll-top-shadow: inset 0 1px 0 rgb(255 255 255 / 60%), inset 0 -1px 0 rgb(25 32 43 / 3%), 0 2px 8px rgb(25 32 43 / 5%);
  position: static;
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

.gameplay-controls .gameplay-controls__actions-button.p-button,
.gameplay-controls .gameplay-controls__actions-button.p-button:is(:hover, :active) {
  color: var(--color-gray-600);
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

@media (width < 1024px) {
  .gameplay-controls {
    display: flex;
    gap: 0;
    align-items: center;
  }

  .gameplay-controls__input-row {
    flex: 1;
  }

  .gameplay-controls__scroll-top-slot {
    flex: 0 0 auto;
    width: calc(var(--guess-control-height) + 2px + 6px);
  }

  .gameplay-controls__scroll-top.p-button {
    width: calc(var(--guess-control-height) + 2px);
    height: calc(var(--guess-control-height) + 2px);
  }

  .scroll-top-enter-active,
  .scroll-top-leave-active {
    transition: width 0.18s ease, opacity 0.18s ease;
  }

  .scroll-top-enter-from,
  .scroll-top-leave-to {
    width: 0;
    opacity: 0;
    pointer-events: none;
  }
}

:global(.gameplay-sheet.p-dialog-enter-active),
:global(.gameplay-sheet.p-dialog-leave-active) {
  animation: none;
  transition: transform 0.2s ease, opacity 0.2s ease;
}

:global(.gameplay-sheet.p-dialog-enter-from),
:global(.gameplay-sheet.p-dialog-leave-to) {
  transform: translateY(calc(100% + max(8px, var(--app-visual-viewport-safe-bottom))));
  opacity: 0;
}

@media (prefers-reduced-motion: reduce) {
  :global(.gameplay-sheet.p-dialog-enter-active),
  :global(.gameplay-sheet.p-dialog-leave-active) {
    transition: none;
  }

  :global(.gameplay-sheet-mask) {
    animation: none;
  }

  .scroll-top-enter-active,
  .scroll-top-leave-active {
    transition: none;
  }
}
</style>
