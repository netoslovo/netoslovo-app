export function formatDuration(value: string) {
  const seconds = parseTimeSpanSeconds(value);
  if (seconds < 1) return "<\u00a01\u00a0сек";
  if (seconds >= 1000 * 60) return ">\u00a01000\u00a0мин";

  const totalSeconds = Math.round(seconds);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const restSeconds = totalSeconds % 60;

  if (hours > 0) return `${hours} ч ${minutes} мин`;
  if (minutes > 0) return `${minutes} мин ${restSeconds} сек`;

  return `${restSeconds} сек`;
}

export function formatDurationInMinutes(value: string, maximumMinutes?: number) {
  const seconds = parseTimeSpanSeconds(value);
  if (seconds < 1) return "<\u00a01\u00a0сек";
  if (maximumMinutes !== undefined && seconds >= maximumMinutes * 60) {
    return `${maximumMinutes}+\u00a0мин`;
  }

  const totalSeconds = Math.round(seconds);
  const minutes = Math.floor(totalSeconds / 60);
  const restSeconds = totalSeconds % 60;
  const secondsText = `${restSeconds}\u00a0сек`;

  return minutes > 0 ? `${minutes}\u00a0мин ${secondsText}` : secondsText;
}

function parseTimeSpanSeconds(value: string) {
  const normalized = value.trim();
  const [dayOrHourPart, minutesPart, secondsPart] = normalized.split(":");
  if (minutesPart === undefined || secondsPart === undefined) return 0;

  const [daysPart, hoursPart] = dayOrHourPart.includes(".")
    ? dayOrHourPart.split(".")
    : ["0", dayOrHourPart];

  const days = Number(daysPart);
  const hours = Number(hoursPart);
  const minutes = Number(minutesPart);
  const seconds = Number(secondsPart);

  if (![days, hours, minutes, seconds].every(Number.isFinite)) return 0;

  return days * 86400 + hours * 3600 + minutes * 60 + seconds;
}
