<script setup lang="ts">
import Dialog from "primevue/dialog";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  visible: boolean;
}>();

const emit = defineEmits<{
  continue: [];
  login: [];
}>();

function onVisibleUpdate(visible: boolean) {
  if (!visible) {
    emit("continue");
  }
}
</script>

<template>
  <Dialog :visible="visible" modal dismissable-mask header="Войти в профиль?" class="game-dialog guest-login-prompt"
    @update:visible="onVisibleUpdate">
    <div class="guest-login-prompt__body">
      <p class="guest-login-prompt__text">
        Войдите, чтобы не потерять результаты игр и прогресс в слове дня.
      </p>

      <div class="guest-login-prompt__actions">
        <UiButton size="md" @click="emit('login')">Войти</UiButton>
        <UiButton size="md" variant="outlined" @click="emit('continue')">
          Позже
        </UiButton>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.guest-login-prompt__body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.guest-login-prompt__text {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.guest-login-prompt__actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
}

.guest-login-prompt__actions>.ui-button {
  flex: 1;
}
</style>
