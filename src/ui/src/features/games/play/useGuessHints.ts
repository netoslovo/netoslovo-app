import { computed } from "vue";
import type { HintsInfo } from "../model/game";

type GuessHintsOptions = {
  wordLength?: number | null;
  hintsInfo: HintsInfo;
};

export function useGuessHints(options: Readonly<GuessHintsOptions>) {
  const remainingNeighbourHints = computed(() => Math.max(0, options.hintsInfo.neighbourHintsLeft));
  const totalNeighbourHints = computed(() => Math.max(0, options.hintsInfo.neighbourHintsTotal));
  const remainingRevealLetterHints = computed(() =>
    options.hintsInfo.revealLetterHintsLeft == null
      ? null
      : Math.max(0, options.hintsInfo.revealLetterHintsLeft),
  );
  const totalRevealLetterHints = computed(() =>
    options.hintsInfo.revealLetterHintsTotal == null
      ? null
      : Math.max(0, options.hintsInfo.revealLetterHintsTotal),
  );
  const isWordLengthRevealed = computed(() => options.wordLength != null);
  const canRevealHalfwayWord = computed(() => remainingNeighbourHints.value > 0);
  const nextHalfwayWordPenalty = computed(() => {
    if (!canRevealHalfwayWord.value) return null;
    const usedHints = totalNeighbourHints.value - remainingNeighbourHints.value;
    return options.hintsInfo.revealHalfwayWordHintPenalties[usedHints] ?? null;
  });
  const halfwayWordHintDisabledReason = computed(() => {
    if (remainingNeighbourHints.value <= 0) return "Все подсказки этого типа уже использованы";
    return undefined;
  });
  const canRevealWordLength = computed(() =>
    !options.hintsInfo.revealLengthHintUsed,
  );
  const nextWordLengthPenalty = computed(() =>
    canRevealWordLength.value ? options.hintsInfo.revealLengthHintPenalty : null,
  );
  const wordLengthHintDisabledReason = computed(() => {
    if (options.hintsInfo.revealLengthHintUsed) return "Длина слова уже открыта";
    return undefined;
  });
  const canRevealRandomLetter = computed(() => {
    const hintsLeft = remainingRevealLetterHints.value;
    return isWordLengthRevealed.value && hintsLeft != null && hintsLeft > 0;
  });
  const nextRandomLetterPenalty = computed(() => {
    if (!canRevealRandomLetter.value) return null;
    const total = totalRevealLetterHints.value;
    const remaining = remainingRevealLetterHints.value;
    const penalties = options.hintsInfo.revealLetterHintPenalties;
    if (total == null || remaining == null || penalties == null) return null;
    return penalties[total - remaining] ?? null;
  });
  const halfwayWordDescription = computed(() => [
    "Откроет одно слово ближе к ответу.",
    "Нельзя использовать, если вы уже находитесь на расстоянии 1 от загаданного слова.",
    ...formatPenaltyDescription(options.hintsInfo.revealHalfwayWordHintPenalties),
  ]);
  const wordLengthDescription = computed(() => [
    "Покажет, сколько букв в загаданном слове.",
    `Стоимость: +${options.hintsInfo.revealLengthHintPenalty} к счёту.`,
  ]);
  const randomLetterDescription = computed(() => {
    const penalties = options.hintsInfo.revealLetterHintPenalties;
    return [
      "Откроет одну случайную ещё не открытую букву в загаданном слове.",
      "Доступно только после открытия длины слова.",
      "Количество зависит от длины слова.",
      "Общая стоимость раскрытия всех доступных букв: +80 к счёту.",
      ...(penalties
        ? formatPenaltyDescription(penalties)
        : ["Стоимость раскрытия каждой буквы будет доступна после раскрытия длины слова."]),
    ];
  });
  const randomLetterHintDisabledReason = computed(() => {
    if (!isWordLengthRevealed.value) return "Сначала откройте длину слова";
    const hintsLeft = remainingRevealLetterHints.value;
    if (hintsLeft == null) return "Количество подсказок пока неизвестно";
    if (hintsLeft <= 0) return "Все подсказки этого типа уже использованы";
    return undefined;
  });

  return {
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
  };
}

function formatPenaltyDescription(penalties: readonly number[]) {
  return penalties.map((penalty, index) => `${index + 1}-я подсказка: +${penalty} к счёту`);
}
