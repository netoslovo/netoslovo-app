<template>
  <span class="leaderboard-gap-mark" aria-hidden="true"></span>
</template>

<style scoped>
.leaderboard-gap-mark {
  width: 1.75em;
  height: 1.75em;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  vertical-align: middle;
}

.leaderboard-gap-mark::before {
  width: 0.12em;
  height: 0.12em;
  border-radius: 50%;
  background: currentColor;
  box-shadow:
    -0.4em 0 currentColor,
    0.4em 0 currentColor;
  content: "";
}
</style>
