import type { DailyGameStreakTier } from "../model/game";

export type DailyStreakTierLegendItem = {
  tier: DailyGameStreakTier;
  minimumStreak: number;
  range: string;
};

export const dailyStreakTierLegend: readonly DailyStreakTierLegendItem[] = [
  { tier: "started", minimumStreak: 1, range: "1+" },
  { tier: "steady", minimumStreak: 3, range: "3+" },
  { tier: "week", minimumStreak: 7, range: "7+" },
  { tier: "twoWeeks", minimumStreak: 14, range: "14+" },
  { tier: "month", minimumStreak: 30, range: "30+" },
  { tier: "season", minimumStreak: 60, range: "60+" },
  { tier: "century", minimumStreak: 100, range: "100+" },
  { tier: "legend", minimumStreak: 365, range: "365+" },
];

export function getDailyStreakTier(streak: number): DailyGameStreakTier {
  if (streak < 1) return "none";

  for (let index = dailyStreakTierLegend.length - 1; index >= 0; index--) {
    const item = dailyStreakTierLegend[index];
    if (item && streak >= item.minimumStreak) return item.tier;
  }

  return "none";
}
