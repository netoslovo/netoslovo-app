<script setup lang="ts">
import Dialog from "primevue/dialog";
import Popover from "primevue/popover";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useRouter, type RouteLocationRaw } from "vue-router";
import type { DailyGame, DailyGameStreak } from "../model/game";
import { dailyStreakTierLegend } from "../lib/dailyStreakPresentation";
import { useInfoPopoverSemantics } from "../../../shared/composables/useInfoPopover";
import { userNoticeCodes } from "../../user-notices/model/userNoticeCodes";
import { useUserNoticeStore } from "../../user-notices/model/userNoticeStore";
import { showToast } from "../../../shared/notifications/toastStore";
import DailyCompactTile from "./DailyCompactTile.vue";
import DailyTodayCard from "./DailyTodayCard.vue";
import NewArcadeGameDialog from "./NewArcadeGameDialog.vue";
import DailyStreakBadge from "../components/DailyStreakBadge.vue";
import GameGuide from "../components/GameGuide.vue";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  authenticated: boolean;
  loginLocked: boolean;
  arcadeLocked: boolean;
  todayFailed: boolean;
  statusMessage: string | null;
  hasGame: boolean;
  dailyGames: DailyGame[];
  dailyStreak: DailyGameStreak | null;
  dailyHistoryFailed: boolean;
  dailyLoading: boolean;
  statusRetryAvailable?: boolean;
  continueGameTo?: RouteLocationRaw;
}>();

const emit = defineEmits<{
  retryStatus: [];
  continueGame: [];
}>();

const router = useRouter();
const userNotices = useUserNoticeStore();

type PopoverRef = {
  show: (event: Event, target: HTMLElement) => void;
  hide: () => void;
};

type InfoPopover = "login" | "daily" | "streak" | "arcade";

const dailyStreakTierClasses: Record<DailyGameStreak["tier"], string> = {
  none: "daily-streak--tier-none",
  started: "daily-streak--tier-started",
  steady: "daily-streak--tier-steady",
  week: "daily-streak--tier-week",
  twoWeeks: "daily-streak--tier-two-weeks",
  month: "daily-streak--tier-month",
  season: "daily-streak--tier-season",
  century: "daily-streak--tier-century",
  legend: "daily-streak--tier-legend",
};

const loginInfoPopover = ref<PopoverRef | null>(null);
const dailyInfoPopover = ref<PopoverRef | null>(null);
const streakInfoPopover = ref<PopoverRef | null>(null);
const arcadeInfoPopover = ref<PopoverRef | null>(null);
const visibleInfoPopover = ref<InfoPopover | null>(null);
const loginInfoSemantics = useInfoPopoverSemantics();
const dailyInfoSemantics = useInfoPopoverSemantics();
const streakInfoSemantics = useInfoPopoverSemantics();
const arcadeInfoSemantics = useInfoPopoverSemantics();
const howToPlayOpen = ref(false);
const newGamePromptOpen = ref(false);
const newGamePromptChecking = ref(false);
const newGamePromptSaving = ref(false);
let mounted = false;

onMounted(() => {
  mounted = true;
});

onUnmounted(() => {
  mounted = false;
});

const dailyStreakTierClass = computed(
  () => props.dailyStreak
    ? dailyStreakTierClasses[props.dailyStreak.tier]
    : dailyStreakTierClasses.none,
);

function showPopover(popover: PopoverRef | null, event: Event) {
  if (!(event.currentTarget instanceof HTMLElement)) {
    return;
  }

  popover?.show(event, event.currentTarget);
}

function hidePopover(popover: PopoverRef | null) {
  popover?.hide();
}

function onPopoverShow(popoverName: InfoPopover) {
  visibleInfoPopover.value = popoverName;
}

function onPopoverHide(popoverName: InfoPopover) {
  if (visibleInfoPopover.value === popoverName) {
    visibleInfoPopover.value = null;
  }
}

function onInfoPointerDown(
  event: PointerEvent,
  popover: PopoverRef | null,
  popoverName: InfoPopover,
) {
  event.preventDefault();

  if (event.pointerType === "mouse") {
    return;
  }

  event.stopPropagation();

  if (visibleInfoPopover.value === popoverName) {
    hidePopover(popover);
    return;
  }

  showPopover(popover, event);
}

async function requestArcadeCreate() {
  if (props.arcadeLocked || newGamePromptChecking.value) {
    return;
  }

  if (!props.hasGame) {
    await openArcadeCreate();
    return;
  }

  newGamePromptChecking.value = true;
  try {
    const shouldShow = await userNotices.shouldShowUserNotice(
      userNoticeCodes.newArcadeGameWhileActive,
    );
    if (!mounted) {
      return;
    }

    if (shouldShow) {
      newGamePromptOpen.value = true;
      return;
    }

    await openArcadeCreate();
  } finally {
    newGamePromptChecking.value = false;
  }
}

function openGameGuide() {
  howToPlayOpen.value = true;
}

async function dismissNewGamePrompt(doNotShowAgain: boolean) {
  if (await saveNewGamePromptView(doNotShowAgain)) {
    newGamePromptOpen.value = false;
  }
}

async function cancelNewGamePrompt(doNotShowAgain: boolean) {
  await dismissNewGamePrompt(doNotShowAgain);
}

async function confirmArcadeCreate(doNotShowAgain: boolean) {
  if (!await saveNewGamePromptView(doNotShowAgain)) {
    return;
  }

  newGamePromptOpen.value = false;
  await openArcadeCreate();
}

async function saveNewGamePromptView(doNotShowAgain: boolean) {
  const request = userNotices.saveRecurringUserNoticeView(
    userNoticeCodes.newArcadeGameWhileActive,
    doNotShowAgain,
  );
  if (!doNotShowAgain) {
    void request.catch(() => undefined);
    return true;
  }

  newGamePromptSaving.value = true;
  try {
    await request;
    return true;
  } catch {
    showToast({
      status: "error",
      title: "Обработка запроса",
      message: "Произошла ошибка обработки запроса, попробуйте позже",
    });
    return false;
  } finally {
    newGamePromptSaving.value = false;
  }
}

function openArcadeCreate() {
  return router.push({ name: "arcade-create" });
}

const todayDailyGame = computed(() =>
  props.dailyGames.find((dailyGame) => dailyGame.isToday) ?? null,
);

const previousDailyGames = computed(() =>
  props.dailyGames.filter((dailyGame) => !dailyGame.isToday).slice(0, 6).reverse(),
);

const unavailableTodayDailyGame = computed<DailyGame>(() => ({
  day: formatLocalDay(new Date()),
  state: null,
  guessedAtGameDay: null,
  isToday: true,
  gameWord: null,
}));

function formatLocalDay(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

</script>

<template>
  <div class="home-screen">
    <div v-if="statusMessage" class="home-screen__status">
      <p>{{ statusMessage }}</p>
      <UiButton v-if="statusRetryAvailable" size="md" variant="outlined" @click="emit('retryStatus')">
        Повторить
      </UiButton>
    </div>

    <div v-else class="home-screen__menu">
      <article v-if="!authenticated" class="entry-card">
        <div class="entry-card__header">
          <div class="entry-card__title-row">
            <h2 class="entry-card__title">Профиль</h2>

            <button class="entry-card__info entry-card__info--sm"
              :class="{ 'entry-card__info--open': visibleInfoPopover === 'login' }" :id="loginInfoSemantics.triggerId"
              type="button" aria-label="Об авторизации" :aria-describedby="loginInfoSemantics.panelId"
              @mouseenter="showPopover(loginInfoPopover, $event)" @mouseleave="hidePopover(loginInfoPopover)"
              @pointerdown="onInfoPointerDown($event, loginInfoPopover, 'login')"
              @focus="showPopover(loginInfoPopover, $event)" @blur="hidePopover(loginInfoPopover)" @click.stop>
              <i class="pi pi-info-circle" aria-hidden="true"></i>
            </button>
          </div>

          <Popover ref="loginInfoPopover" :pt="loginInfoSemantics.popoverPt" class="entry-card__popover info-popover"
            @show="onPopoverShow('login')" @hide="onPopoverHide('login')">
            <div class="entry-card__popover-content">
              Пока вы не авторизованы, игровой прогресс привязан к временной гостевой сессии и может быть потерян
              после её завершения. После входа результаты сохраняются в профиле и доступны на других устройствах.
            </div>
          </Popover>
        </div>

        <div class="entry-card__body">
          <UiButton size="md" :disabled="loginLocked" :to="{ name: 'login' }">
            Войти
          </UiButton>

          <div v-if="loginLocked" class="entry-card__status">
            Не удалось загрузить данные профиля.
          </div>
        </div>
      </article>

      <article class="entry-card entry-card--daily">
        <div class="entry-card__header entry-card__header--with-streak">
          <div class="entry-card__title-row">
            <h2 class="entry-card__title">Слово дня</h2>

            <button class="entry-card__info entry-card__info--sm"
              :class="{ 'entry-card__info--open': visibleInfoPopover === 'daily' }" type="button"
              :id="dailyInfoSemantics.triggerId" aria-label="Об игре дня" :aria-describedby="dailyInfoSemantics.panelId"
              @mouseenter="showPopover(dailyInfoPopover, $event)" @mouseleave="hidePopover(dailyInfoPopover)"
              @pointerdown="onInfoPointerDown($event, dailyInfoPopover, 'daily')"
              @focus="showPopover(dailyInfoPopover, $event)" @blur="hidePopover(dailyInfoPopover)" @click.stop>
              <i class="pi pi-info-circle" aria-hidden="true"></i>
            </button>

            <Popover ref="dailyInfoPopover" :pt="dailyInfoSemantics.popoverPt" class="entry-card__popover info-popover"
              @show="onPopoverShow('daily')" @hide="onPopoverHide('daily')">
              <div class="entry-card__popover-content">
                <p>Ежедневный режим с одной общей загадкой для всех игроков. </p>
                <p>Слово обновляется каждый день в 00:00 по времени Екатеринбурга (UTC+5). </p>
                <p>Прошедшие игры сохраняются в истории: их можно пройти позже или открыть, чтобы посмотреть свои
                  результаты.</p>
              </div>
            </Popover>

          </div>

          <div class="daily-streak" :class="dailyStreakTierClass">
            <span class="daily-streak__label">Серия:</span>
            <strong class="daily-streak__value">{{ dailyStreak?.streak ?? "—" }}</strong>
            <button class="daily-streak__info" :class="{ 'daily-streak__info--open': visibleInfoPopover === 'streak' }"
              :id="streakInfoSemantics.triggerId" type="button" aria-label="О серии"
              :aria-describedby="streakInfoSemantics.panelId" @mouseenter="showPopover(streakInfoPopover, $event)"
              @mouseleave="hidePopover(streakInfoPopover)"
              @pointerdown="onInfoPointerDown($event, streakInfoPopover, 'streak')"
              @focus="showPopover(streakInfoPopover, $event)" @blur="hidePopover(streakInfoPopover)" @click.stop>
              <i class="pi pi-info-circle" aria-hidden="true"></i>
            </button>

            <Popover ref="streakInfoPopover" :pt="streakInfoSemantics.popoverPt"
              class="entry-card__popover info-popover" @show="onPopoverShow('streak')" @hide="onPopoverHide('streak')">
              <div class="entry-card__popover-content">
                <p>Счетчик серии увеличивается, когда вы отгадываете слово дня в тот же день, когда оно появляется. Если
                  пропустить день, активная серия сбросится.</p>
                <p>Есть несколько уровней в зависимости от длины серии:</p>
                <ul class="daily-streak-legend" aria-label="Уровни серии">
                  <li v-for="item in dailyStreakTierLegend" :key="item.tier" class="daily-streak-legend__item">
                    <DailyStreakBadge :value="item.range" :tier="item.tier" :label="`Серия ${item.range}`" />
                  </li>
                </ul>
              </div>
            </Popover>
          </div>
        </div>

        <div class="entry-card__body entry-card__body--daily">
          <div v-if="dailyLoading && dailyGames.length === 0" class="entry-card__status">
            Загружаем слова дня...
          </div>

          <template v-else>
            <div class="daily-chain">
              <div class="daily-chain__today">
                <DailyTodayCard v-if="todayDailyGame && !todayFailed" :daily-game="todayDailyGame"
                  :to="{ name: 'daily', params: { day: todayDailyGame.day } }" />

                <DailyTodayCard v-else :daily-game="unavailableTodayDailyGame" disabled
                  :disabled-label="todayFailed ? 'Не удалось загрузить слово дня' : 'Слово дня пока недоступно'" />
              </div>

              <section class="daily-week-section daily-chain__history">
                <h3 class="daily-week-section__title">
                  <span>Прошлые слова дня</span>
                </h3>

                <div class="daily-week-section__content">
                  <div v-if="dailyHistoryFailed" class="entry-card__status">
                    Не удалось загрузить прошлые слова дня.
                  </div>

                  <div v-else-if="previousDailyGames.length === 0" class="entry-card__status">
                    Прошлых слов дня пока нет.
                  </div>

                  <div v-else class="daily-week" aria-label="Недавние слова дня">
                    <div v-for="dailyGame in previousDailyGames" :key="dailyGame.day" class="daily-week__item">
                      <DailyCompactTile :daily-game="dailyGame"
                        :to="{ name: 'daily', params: { day: dailyGame.day } }" />
                    </div>
                  </div>

                  <UiButton class="daily-week-section__history-button" size="md" variant="outlined"
                    :to="{ name: 'daily-history' }">
                    <i class="pi pi-calendar" aria-hidden="true"></i>
                    <span>Все слова дня</span>
                  </UiButton>

                  <UiButton class="daily-week-section__history-button" size="md" variant="outlined"
                    :to="{ name: 'daily-leaderboard' }">
                    <i class="pi pi-trophy" aria-hidden="true"></i>
                    <span>Лучшие игроки</span>
                  </UiButton>
                </div>
              </section>
            </div>
          </template>
        </div>
      </article>

      <article class="entry-card">
        <div class="entry-card__header">
          <div class="entry-card__title-row">
            <h2 class="entry-card__title">Случайное слово</h2>

            <button class="entry-card__info entry-card__info--sm"
              :class="{ 'entry-card__info--open': visibleInfoPopover === 'arcade' }" type="button"
              :id="arcadeInfoSemantics.triggerId" aria-label="О случайной игре"
              :aria-describedby="arcadeInfoSemantics.panelId" @mouseenter="showPopover(arcadeInfoPopover, $event)"
              @mouseleave="hidePopover(arcadeInfoPopover)"
              @pointerdown="onInfoPointerDown($event, arcadeInfoPopover, 'arcade')"
              @focus="showPopover(arcadeInfoPopover, $event)" @blur="hidePopover(arcadeInfoPopover)" @click.stop>
              <i class="pi pi-info-circle" aria-hidden="true"></i>
            </button>
          </div>

          <Popover ref="arcadeInfoPopover" :pt="arcadeInfoSemantics.popoverPt" class="entry-card__popover info-popover"
            @show="onPopoverShow('arcade')" @hide="onPopoverHide('arcade')">
            <div class="entry-card__popover-content">
              <p>Вам будет загадано случайное слово выбранной сложности.</p>
              <p>Можно играть в любое время и запускать несколько игр одновременно.</p>
            </div>
          </Popover>
        </div>

        <div class="entry-card__body">
          <UiButton v-if="hasGame && continueGameTo" size="md" :to="continueGameTo">
            Продолжить игру
          </UiButton>
          <UiButton v-else-if="hasGame" size="md" @click="emit('continueGame')">
            Продолжить игру
          </UiButton>

          <div v-if="arcadeLocked" class="entry-card__status">
            Не удалось загрузить данные случайной игры.
          </div>

          <UiButton size="md" :loading="newGamePromptChecking" :disabled="arcadeLocked" @click="requestArcadeCreate">
            Новая игра
          </UiButton>

          <UiButton size="md" variant="outlined" :to="{ name: 'arcade-history' }">
            <i class="pi pi-history" aria-hidden="true"></i>
            <span>История игр</span>
          </UiButton>

          <UiButton size="md" variant="outlined" :to="{ name: 'arcade-leaderboard' }">
            <i class="pi pi-trophy" aria-hidden="true"></i>
            <span>Лучшие игроки</span>
          </UiButton>
        </div>
      </article>
    </div>

    <button v-if="!statusMessage" class="home-screen__how-to-play" type="button" @click="openGameGuide">
      <i class="pi pi-info-circle" aria-hidden="true"></i>
      <span>Об игре</span>
    </button>

    <Dialog v-if="!statusMessage" v-model:visible="howToPlayOpen" modal dismissable-mask header="Об игре"
      class="game-dialog">
      <GameGuide :open="howToPlayOpen" @close="howToPlayOpen = false" />
    </Dialog>

    <NewArcadeGameDialog :visible="newGamePromptOpen" :saving="newGamePromptSaving" @dismiss="dismissNewGamePrompt"
      @cancel="cancelNewGamePrompt" @confirm="confirmArcadeCreate" />

  </div>
</template>

<style scoped>
.home-screen {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: center;
  justify-content: center;
  overflow-y: auto;
  padding: 12px 0;
}

.home-screen__status {
  width: min(100%, 300px);
  margin: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  color: var(--p-text-muted-color);
  font-size: 14px;
  line-height: 1.4;
  text-align: center;
}

.home-screen__status p {
  margin: 0;
}

.home-screen__menu {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: min(100%, 438px);
}

.home-screen__how-to-play {
  padding: 4px 2px;
  border: 0;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  background: transparent;
  color: var(--p-text-muted-color);
  font: inherit;
  font-weight: 400;
  transition: color 0.16s ease;
}

@media (hover: hover) and (pointer: fine) {
  .home-screen__how-to-play:hover {
    color: var(--color-primary-600);
  }

  .home-screen__how-to-play:hover span {
    text-decoration: underline;
    text-underline-offset: 3px;
  }
}

.home-screen__how-to-play:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
  border-radius: 4px;
}

.entry-card {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
}

.entry-card--daily {
  gap: 14px;
  padding: 18px;
}

.entry-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.entry-card__header--with-streak {
  align-items: center;
}

.entry-card__title-row {
  min-width: 0;
  display: inline-flex;
  align-items: center;
  gap: 3px;
}

.entry-card__title {
  margin: 0;
  color: var(--color-gray-900);
  font-size: 18px;
  font-weight: 500;
  line-height: 1.2;
}

.entry-card__body {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.entry-card__body--daily {
  gap: 14px;
}

.entry-card__body>.ui-button.p-button {
  width: 100%;
}

.daily-streak {
  --daily-streak-border: var(--color-gray-300);
  --daily-streak-bg: var(--color-gray-50);
  --daily-streak-value: var(--color-gray-900);
  width: fit-content;
  max-width: 100%;
  min-height: 34px;
  padding: 6px 9px;
  border: 1px solid var(--daily-streak-border);
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  gap: 7px;
  background: var(--daily-streak-bg);
  color: var(--color-gray-700);
  flex-shrink: 0;
}

.daily-streak--tier-started {
  --daily-streak-border: #d8dee3;
  --daily-streak-bg: #ffffff;
  --daily-streak-value: var(--color-primary-600);
}

.daily-streak--tier-steady {
  --daily-streak-border: #d5b28f;
  --daily-streak-bg: #fbf3eb;
  --daily-streak-value: #855322;
}

.daily-streak--tier-week {
  --daily-streak-border: #b9c4cf;
  --daily-streak-bg: #f4f7fa;
  --daily-streak-value: #55616d;
}

.daily-streak--tier-two-weeks {
  --daily-streak-border: #d8b75a;
  --daily-streak-bg: #fff7df;
  --daily-streak-value: #80610d;
}

.daily-streak--tier-month {
  --daily-streak-border: #76adb8;
  --daily-streak-bg: linear-gradient(135deg, #dff3f5 0%, #f7fbfb 48%, #bfe4e9 100%);
  --daily-streak-value: #32616c;
}

.daily-streak--tier-season {
  --daily-streak-border: #7fa4d8;
  --daily-streak-bg: linear-gradient(135deg, #deedff 0%, #f7faff 48%, #c8d8f7 100%);
  --daily-streak-value: #2d5a91;
}

.daily-streak--tier-century {
  --daily-streak-border: #9f83cb;
  --daily-streak-bg: linear-gradient(135deg, #eee4fb 0%, #fbf8ff 48%, #d8c3f2 100%);
  --daily-streak-value: #593889;
}

.daily-streak--tier-legend {
  --daily-streak-border: #d0ae4b;
  --daily-streak-bg:
    linear-gradient(135deg, rgba(255, 255, 255, 0.58) 0%, transparent 24%),
    linear-gradient(135deg, #fff0b8 0%, #f7fbfc 48%, #f2d37a 100%);
  --daily-streak-value: #70540b;
}

.daily-streak--tier-legend .daily-streak__label {
  color: #806521;
}

.daily-streak__value {
  color: var(--daily-streak-value);
  font-size: 18px;
  font-weight: 500;
  line-height: 1;
  transform: translateY(-1px);
  font-variant-numeric: tabular-nums;
}

.daily-streak__label {
  color: var(--color-gray-600);
  font-size: 13px;
  font-weight: 400;
  line-height: 1;
}

.daily-streak__info {
  width: 23px;
  min-width: 23px;
  height: 23px;
  border: 0;
  border-radius: 50%;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--daily-streak-value);
  cursor: help;
  transition:
    background-color 0.16s ease,
    color 0.16s ease;
}

.daily-streak__info--open {
  background: rgba(255, 255, 255, 0.72);
  color: var(--daily-streak-value);
}

@media (hover: hover) and (pointer: fine) {
  .daily-streak__info:hover {
    background: rgba(255, 255, 255, 0.72);
    color: var(--daily-streak-value);
  }
}

.daily-streak__info:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.daily-streak__info .pi {
  font-size: 13px;
}

.daily-streak-legend {
  margin: 9px 0 0;
  padding: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
  list-style: none;
}

.daily-streak-legend__item {
  min-width: 0;
  display: flex;
}

.daily-chain {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.daily-week-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.daily-week-section__title {
  margin: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: var(--color-gray-700);
  font-size: 16px;
  font-weight: 500;
  line-height: 1.2;
  list-style: none;
}

.daily-week-section__title::-webkit-details-marker {
  display: none;
}

.daily-week-section__content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.daily-week {
  width: 100%;
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 6px;
}

.daily-week__item {
  min-width: 0;
}

.daily-week :deep(.daily-compact-tile) {
  width: 100%;
  min-width: 0;
}

.daily-week-section__history-button.ui-button.p-button {
  width: 100%;
}

.daily-week-section__history-button :deep(.p-button-label) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.daily-week-section__history-button :deep(.pi) {
  font-size: 14px;
}

.entry-card__info {
  width: 34px;
  min-width: 34px;
  height: 34px;
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
    border-color 0.16s ease,
    color 0.16s ease,
    background-color 0.16s ease;
}

.entry-card__info--sm {
  width: 30px;
  min-width: 30px;
  height: 30px;
  border-radius: 50%;
}

.entry-card__info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
}

@media (hover: hover) and (pointer: fine) {
  .entry-card__info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-600);
  }
}

.entry-card__info:focus-visible {
  outline: none;
}

.entry-card__info .pi {
  font-size: 16px;
}

.entry-card__status {
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.35;
}

.entry-card__popover-content {
  margin: 0;
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

.entry-card__popover-content p {
  margin: 0;
}

.entry-card__popover-content p+p {
  margin-top: 6px;
}

:global(.entry-card__popover.p-popover) {
  max-width: min(280px, calc(100vw - 24px));
  color: var(--color-gray-700);
}

@media (min-width: 768px) {
  .home-screen {
    gap: 14px;
  }

  .entry-card {
    padding: 18px;
  }

  .entry-card--daily {
    gap: 16px;
    padding: 20px;
  }

  .entry-card__title {
    font-size: 19px;
  }
}

@media (max-height: 760px) {

  .home-screen__menu,
  .entry-card,
  .entry-card__body--daily {
    gap: 10px;
  }

  .entry-card {
    padding: 12px;
  }

  .entry-card--daily {
    gap: 12px;
    padding: 14px;
  }

  .entry-card__title {
    font-size: 17px;
  }

  .daily-chain {
    gap: 12px;
  }

  .entry-card__body--daily {
    gap: 12px;
  }
}

@media (max-height: 640px) {
  .home-screen {
    padding-block: 8px;
  }

  .entry-card {
    gap: 8px;
  }

  .entry-card--daily {
    gap: 10px;
  }
}

@media (max-width: 480px) {
  .home-screen {
    gap: 10px;
    padding: 2px 0 8px;
  }

  .home-screen__menu {
    gap: 10px;
  }

  .home-screen__how-to-play {
    gap: 5px;
    font-size: 14px;
  }

  .entry-card {
    gap: 10px;
    padding: 12px;
    border-radius: 12px;
  }

  .entry-card--daily {
    gap: 12px;
    padding: 14px;
  }

  .entry-card__title {
    font-size: 17px;
  }

  .entry-card__body {
    gap: 8px;
  }

  .entry-card__body--daily {
    container-type: inline-size;
    --daily-week-gap: 5px;
    --daily-week-card-size: calc((100cqw - (var(--daily-week-gap) * 4)) / 5);
    gap: 12px;
  }

  .daily-week-section__title {
    font-size: 15px;
  }

  .daily-week {
    grid-template-columns: repeat(5, var(--daily-week-card-size));
    gap: var(--daily-week-gap);
  }

  .daily-week__item:first-child:nth-last-child(6) {
    display: none;
  }

  .entry-card__info {
    width: 30px;
    min-width: 30px;
    height: 30px;
    border-radius: 50%;
  }

  .entry-card__info .pi {
    font-size: 15px;
  }

  .daily-streak {
    min-height: 32px;
    padding-block: 5px;
    gap: 7px;
  }

  .daily-streak__value {
    font-size: 17px;
  }

  .daily-streak__label {
    font-size: 13px;
  }

  .entry-card__body :deep(.ui-button--md.p-button),
  .daily-week-section__history-button.ui-button.p-button {
    min-height: 36px;
    padding-inline: 12px;
    font-size: 15px;
  }

  .entry-card__body--daily :deep(.daily-today-card) {
    height: var(--daily-week-card-size);
    min-height: 54px;
    gap: 8px;
    padding: 8px;
  }

  .entry-card__body--daily :deep(.daily-today-card__title) {
    font-size: 16px;
  }

  .entry-card__body--daily :deep(.daily-today-card__meta) {
    font-size: 12px;
  }

  .entry-card__body--daily :deep(.daily-today-card__action) {
    width: 36px;
    min-width: 36px;
    height: 36px;
  }

  .entry-card__body--daily :deep(.daily-today-card__action .pi) {
    font-size: 18px;
  }

}
</style>
