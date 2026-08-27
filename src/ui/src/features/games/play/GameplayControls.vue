<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import Popover from "primevue/popover";
import { computed, ref, useId } from "vue";
import { showToast } from "../../../shared/notifications/toastStore";
import UiMenu, { type UiMenuItem } from "../../../shared/ui/UiMenu.vue";
import { useGuessHints } from "./useGuessHints";
import { useScreenKeyboard } from "./useScreenKeyboard";
import type { GameActionResult } from "../lib/gameActionOutcomes";
import type { HintsInfo } from "../model/game";
import CompactScreenKeyboard from "./CompactScreenKeyboard.vue";
import DictionaryChangedDialog from "./DictionaryChangedDialog.vue";
import GameGuide from "../components/GameGuide.vue";
import GuessForm from "./GuessForm.vue";
import HintMenuItem from "./HintMenuItem.vue";
import SurrenderDialog from "./SurrenderDialog.vue";
import WordNotFoundInfoDialog from "./WordNotFoundInfoDialog.vue";

type MenuRef = { close: () => void };
type PopoverRef = { toggle: (event: Event) => void; hide: () => void };
type GuessFormRef = { submit: () => Promise<void> };

export type GameplayActions = {
  submitGuess: (word: string) => Promise<GameActionResult>;
  surrender: () => Promise<GameActionResult>;
  revealHalfwayWord: () => Promise<GameActionResult>;
  revealWordLength: () => Promise<GameActionResult>;
  revealRandomLetter: () => Promise<GameActionResult>;
};

const props = defineProps<{
  loading: boolean;
  wordLength?: number | null;
  hintsInfo: HintsInfo;
  actions: GameplayActions;
  showStatistics?: () => void;
}>();

const form = ref<GuessFormRef | null>(null);
const word = ref("");
const guessSubmitting = ref(false);
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

const {
  preferred: compactKeyboardPreferred,
  visible: compactKeyboardVisible,
  infoOpen: compactKeyboardInfoOpen,
  toggleDescription: compactKeyboardToggleDescription,
  append: appendKeyboardCharacter,
  removeLast: removeKeyboardCharacter,
  show: showCompactKeyboard,
  hide: hideCompactKeyboard,
  toggle: toggleCompactKeyboard,
} = useScreenKeyboard(word, () => props.loading);

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

  items.push(
    {
      label: "Экранная клавиатура",
      icon: "pi pi-th-large",
      description: compactKeyboardToggleDescription.value,
      checkable: true,
      checked: compactKeyboardPreferred.value,
      activate: (_event, close) => toggleCompactKeyboard(close),
    },
    {
      label: "Как играть",
      icon: "pi pi-question-circle",
      description: "Показать правила игры.",
      activate: (_event, close) => showHowToPlay(close),
    },
    {
      label: "Сдаться",
      icon: "pi pi-flag",
      description: "Откроет слово и завершит текущую игру.",
      tone: "danger",
      activate: (_event, close) => requestSurrender(close),
    },
  );

  return [{ label: "Меню", items }];
});

async function submitGuess(value: string) {
  guessSubmitting.value = true;
  try {
    const result = await props.actions.submitGuess(value);
    presentResult(result);
    return { clear: result.status === "success" || result.status === "failed" };
  } finally {
    guessSubmitting.value = false;
  }
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
  actionsMenu.value?.close();
  hintsPopover.value?.toggle(event);
}

function closeHintsPopover() {
  hintsPopover.value?.hide();
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
    <GuessForm ref="form" v-model="word" :loading="loading" :submit="submitGuess">
      <template #controls>
        <Button class="gameplay-controls__hints-button" type="button" aria-label="Подсказки"
          aria-haspopup="dialog" :aria-expanded="hintsOpen ? 'true' : 'false'" :aria-controls="hintsPopoverId"
          icon="pi pi-lightbulb" severity="secondary" outlined @click="toggleHintsPopover" />

        <Popover :id="hintsPopoverId" ref="hintsPopover" class="gameplay-controls__hints-popover"
          :aria-labelledby="hintsTitleId" @show="hintsOpen = true" @hide="hintsOpen = false">
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
        </Popover>

        <UiMenu ref="actionsMenu" :items="actionMenuItems" button-label="Меню" icon="dots" tone="neutral" wide
          @open="closeHintsPopover" />
      </template>
    </GuessForm>

    <CompactScreenKeyboard v-if="compactKeyboardPreferred" v-model:info-open="compactKeyboardInfoOpen"
      :visible="compactKeyboardVisible" :disabled="loading" :submitting="guessSubmitting"
      @append="appendKeyboardCharacter" @remove-last="removeKeyboardCharacter" @submit="form?.submit()"
      @show="showCompactKeyboard" @hide="hideCompactKeyboard" />

    <Dialog v-model:visible="howToPlayOpen" modal dismissable-mask header="Об игре" class="game-dialog">
      <GameGuide :open="howToPlayOpen" @close="howToPlayOpen = false" />
    </Dialog>
    <SurrenderDialog :visible="surrenderDialogOpen" :loading="surrenderSubmitting"
      @close="surrenderDialogOpen = false" @confirm="confirmSurrender" />
    <WordNotFoundInfoDialog v-model:visible="wordNotFoundInfoOpen" />
    <DictionaryChangedDialog :visible="dictionaryChangedDialogOpen" :action="dictionaryChangedAction"
      @close="dictionaryChangedDialogOpen = false" />
  </div>
</template>

<style scoped>
.gameplay-controls { --guess-control-height: 50px; width: 100%; min-width: 0; }
.gameplay-controls__hints-button.p-button,
.gameplay-controls :deep(.ui-menu__button.p-button) { width: 50px; min-width: 50px; height: 50px; }
.gameplay-controls__hints-button.p-button { padding: 0; border-color: var(--color-gray-300); border-radius: 8px; background: white; color: var(--color-primary-600); touch-action: manipulation; -webkit-tap-highlight-color: transparent; transition: background-color 0.18s ease, border-color 0.18s ease, color 0.18s ease, box-shadow 0.18s ease; }
.gameplay-controls__hints-button.p-button:not(:disabled):active { border-color: var(--color-gray-400); background: var(--color-gray-100); color: var(--color-primary-600); }
.gameplay-controls__hints-button :deep(.p-button-icon) { font-size: 18px; }
:global(.gameplay-controls__hints-popover.p-popover) { min-width: 292px; max-width: calc(100vw - 24px); box-shadow: var(--shadow-menu); }
:global(.gameplay-controls__hints-popover.p-popover .p-popover-content) { max-height: min(70dvh, 520px); padding: 8px 0; overflow-y: auto; overscroll-behavior: contain; }
.gameplay-controls__hints-title { min-height: 32px; padding: 0 14px; display: flex; align-items: center; color: var(--p-text-muted-color); font-size: 14px; }

@media (hover: hover) and (pointer: fine) {
  .gameplay-controls__hints-button.p-button:not(:disabled):hover { border-color: var(--color-gray-400); background: var(--color-gray-100); color: var(--color-primary-600); }
}

@media (hover: none), (pointer: coarse) {
  .gameplay-controls__hints-button.p-button:not(:disabled):hover { border-color: var(--color-gray-300); background: white; color: var(--color-primary-600); }
  .gameplay-controls__hints-button.p-button:not(:disabled):active { border-color: var(--color-gray-400); background: var(--color-gray-100); color: var(--color-primary-600); }
}

@media (max-width: 480px) {
  .gameplay-controls { --guess-control-height: 48px; display: flex; flex-direction: column; gap: 7px; }
  .gameplay-controls__hints-button.p-button,
  .gameplay-controls__hints-button.p-button.p-button-icon-only,
  .gameplay-controls :deep(.ui-menu__button.p-button),
  .gameplay-controls :deep(.ui-menu__button.p-button.p-button-icon-only) { width: 48px; min-width: 48px; height: var(--guess-control-height); }
  :global(.gameplay-controls__hints-popover.p-popover) { width: min(320px, calc(100vw - 24px)); min-width: 0; max-width: calc(100vw - 24px); }
  :global(.gameplay-controls__hints-popover.p-popover .p-popover-content) { padding-block: 6px; }
  .gameplay-controls__hints-title { min-height: 30px; padding-inline: 12px; font-size: clamp(14px, 4vw, 15px); }
}

@media (max-width: 359px) {
  .gameplay-controls { --guess-control-height: 44px; }
  .gameplay-controls__hints-button.p-button,
  .gameplay-controls__hints-button.p-button.p-button-icon-only,
  .gameplay-controls :deep(.ui-menu__button.p-button),
  .gameplay-controls :deep(.ui-menu__button.p-button.p-button-icon-only) { width: 44px; min-width: 44px; height: var(--guess-control-height); }
}
</style>
