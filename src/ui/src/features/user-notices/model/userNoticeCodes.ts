export const userNoticeCodes = {
  gameGuide: "game-guide",
  defaultUserName: "default-user-name",
  newArcadeGameWhileActive: "new-arcade-game-while-active",
  guestLoginAfterFinishedGame: "guest-login-after-finished-game",
  dailyResultShareButton: "daily-result-share-button",
} as const;

export type UserNoticeCode = typeof userNoticeCodes[keyof typeof userNoticeCodes];
