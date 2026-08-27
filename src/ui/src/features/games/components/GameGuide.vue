<script setup lang="ts">
import { computed, ref, watch } from "vue";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  open: boolean;
  showWelcome?: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();

const sections = [
  "Добро пожаловать в нетослово.рф!",
  "Как играть",
  "Подсказки",
  "Счёт",
  "О режимах",
  "Приятной игры!",
] as const;
const activeSection = ref(0);
const visibleSections = computed(() =>
  props.showWelcome === true ? sections : sections.slice(1),
);
const contentSection = computed(() =>
  activeSection.value + (props.showWelcome === true ? 0 : 1),
);
const transitionDirection = ref<"forward" | "backward">("forward");
let touchStartX: number | null = null;
let touchStartY: number | null = null;

watch(
  () => props.open,
  (open) => {
    if (open) {
      activeSection.value = 0;
      transitionDirection.value = "forward";
    }
  },
);

function showSection(index: number) {
  if (index < 0 || index >= visibleSections.value.length || index === activeSection.value) {
    return;
  }

  transitionDirection.value = index > activeSection.value ? "forward" : "backward";
  activeSection.value = index;
}

function showPreviousSection() {
  showSection(activeSection.value - 1);
}

function showNextSection() {
  if (activeSection.value === visibleSections.value.length - 1) {
    emit("close");
    return;
  }

  showSection(activeSection.value + 1);
}

function onTouchStart(event: TouchEvent) {
  const touch = event.touches[0];
  touchStartX = touch?.clientX ?? null;
  touchStartY = touch?.clientY ?? null;
}

function onTouchEnd(event: TouchEvent) {
  const touch = event.changedTouches[0];

  if (touchStartX === null || touchStartY === null || !touch) {
    resetTouch();
    return;
  }

  const shiftX = touch.clientX - touchStartX;
  const shiftY = touch.clientY - touchStartY;
  resetTouch();

  if (Math.abs(shiftX) < 50 || Math.abs(shiftX) <= Math.abs(shiftY)) {
    return;
  }

  if (shiftX < 0) {
    showSection(activeSection.value + 1);
    return;
  }

  showPreviousSection();
}

function resetTouch() {
  touchStartX = null;
  touchStartY = null;
}
</script>

<template>
  <div class="game-guide">
    <div class="game-guide__viewport" aria-live="polite" @touchstart.passive="onTouchStart"
      @touchend.passive="onTouchEnd" @touchcancel="resetTouch">
      <Transition :name="`game-guide-${transitionDirection}`" mode="out-in">
        <section :key="activeSection" class="game-guide__section">
          <h2 class="game-guide__title">{{ visibleSections[activeSection] }}</h2>

          <div v-if="contentSection === 0" class="game-guide__content game-guide__content--welcome">
            <p>
              Перед первой игрой предлагаем ознакомиться с короткой справкой о правилах, подсказках и режимах.
            </p>
            <p>
              Она поможет быстрее освоиться и уверенно начать играть.
            </p>
          </div>

          <div v-else-if="contentSection === 1" class="game-guide__content">
            <p>
              Попробуйте отгадать загаданное слово. Вводите свои догадки по одной: после каждой попытки игра покажет,
              насколько ваше слово близко к ответу по смыслу. Чем меньше число, тем ближе слово к загаданному.
            </p>
            <p>
              Например, если вы ввели «самолёт» и получили большое число, а потом ввели «собака» и число стало меньше,
              значит «собака» ближе к ответу. Попробуйте проверить другие близкие слова из этой области, например
              «кошка» или «конура».
            </p>
            <p>
              Количество попыток не ограничено, но каждая из них добавляет один балл в итоговый счёт.
            </p>

          </div>

          <div v-else-if="contentSection === 2" class="game-guide__content">
            <p>
              Подсказки находятся в меню рядом с полем ввода. Они могут показать промежуточное слово,
              длину загаданного слова или одну из его букв.
            </p>
            <p>
              Подсказки помогают продвинуться в игре, но увеличивают ваш счёт.
            </p>
          </div>

          <div v-else-if="contentSection === 3" class="game-guide__content">
            <p>
              Каждая попытка увеличивает счёт на 1. За подсказки начисляются дополнительные баллы. Чем меньше
              итоговый счёт, тем лучше результат.
            </p>
            <p>
              Подробный расчёт можно открыть по значку рядом со счётом.
            </p>
          </div>

          <div v-else-if="contentSection === 4" class="game-guide__content">
            <p>
              <strong>Слово дня.</strong> Ежедневный режим с одной общей загадкой для всех игроков. Слово меняется
              каждый день.
              Прошедшие игры сохраняются в истории: их можно пройти позже или открыть, чтобы посмотреть свои результаты.
            </p>
            <p>
              <strong>Случайное слово.</strong> Вам будет загадано случайное слово выбранной сложности. Можно играть в
              любое время и запускать несколько игр одновременно.
            </p>
          </div>

          <div v-else class="game-guide__content game-guide__content--final">
            <p>
              Играйте, приглашайте друзей, сравнивайте ваши результаты. Возвращайтесь за новым словом дня.
            </p>
            <p>
              Желаем удачи, и пусть интуиция вас не подведёт!
            </p>
          </div>
        </section>
      </Transition>
    </div>

    <div class="game-guide__pagination" aria-label="Разделы справки">
      <button v-for="(section, index) in visibleSections" :key="section" class="game-guide__page"
        :class="{ 'game-guide__page--active': activeSection === index }" type="button"
        :aria-label="`Открыть раздел «${section}»`" :aria-current="activeSection === index ? 'step' : undefined"
        @click="showSection(index)"></button>
    </div>

    <div class="game-guide__actions">
      <UiButton v-if="activeSection > 0" size="md" variant="outlined" @click="showPreviousSection">
        <span class="game-guide__button-label">
          <i class="pi pi-arrow-left" aria-hidden="true"></i>
          Назад
        </span>
      </UiButton>
      <UiButton class="game-guide__next" size="md" @click="showNextSection">
        <span class="game-guide__button-label">
          {{ activeSection === visibleSections.length - 1 ? "Спасибо!" : "Далее" }}
          <i v-if="activeSection < visibleSections.length - 1" class="pi pi-arrow-right" aria-hidden="true"></i>
        </span>
      </UiButton>
    </div>
  </div>
</template>

<style scoped>
.game-guide {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.game-guide__viewport {
  min-height: 224px;
  overflow: hidden;
  touch-action: pan-y;
}

.game-guide__section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.game-guide__title {
  margin: 0;
  color: var(--color-gray-900);
  font-size: clamp(17px, 4.2vw, 18px);
  font-weight: 500;
  line-height: 1.3;
}

.game-guide__content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.game-guide__content p {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.5;
}

.game-guide__content strong {
  color: var(--color-gray-800);
}

.game-guide__content--final {
  max-width: 360px;
}

.game-guide__pagination {
  display: flex;
  justify-content: center;
  gap: 2px;
}

.game-guide__page {
  width: 24px;
  height: 24px;
  padding: 0;
  border: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: transparent;
}

.game-guide__page::before {
  width: 9px;
  height: 9px;
  border-radius: 999px;
  background: var(--color-gray-300);
  content: "";
  transition:
    width 0.16s ease,
    background-color 0.16s ease;
}

.game-guide__page--active::before {
  width: 24px;
  background: var(--color-primary-600);
}

.game-guide__page:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 3px;
}

.game-guide__actions {
  display: flex;
  gap: 10px;
}

.game-guide__actions>.ui-button {
  min-width: 112px;
}

.game-guide__button-label {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.game-guide__next {
  margin-left: auto;
}

.game-guide-forward-enter-active,
.game-guide-forward-leave-active,
.game-guide-backward-enter-active,
.game-guide-backward-leave-active {
  transition:
    opacity 0.14s ease,
    transform 0.14s ease;
}

.game-guide-forward-enter-from,
.game-guide-backward-leave-to {
  opacity: 0;
  transform: translateX(14px);
}

.game-guide-forward-leave-to,
.game-guide-backward-enter-from {
  opacity: 0;
  transform: translateX(-14px);
}

@media (prefers-reduced-motion: reduce) {

  .game-guide-forward-enter-active,
  .game-guide-forward-leave-active,
  .game-guide-backward-enter-active,
  .game-guide-backward-leave-active {
    transition: none;
  }
}
</style>
