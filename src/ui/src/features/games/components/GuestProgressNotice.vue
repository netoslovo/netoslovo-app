<script setup lang="ts">
import Checkbox from "primevue/checkbox";
import { ref, useId } from "vue";
import UiActionNotice from "../../../shared/ui/UiActionNotice.vue";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  saving: boolean;
}>();

const emit = defineEmits<{
  dismiss: [doNotShowAgain: boolean];
  login: [doNotShowAgain: boolean];
}>();

const preferenceId = `guest-progress-preference-${useId()}`;
const doNotShowAgain = ref(false);
</script>

<template>
  <UiActionNotice class="guest-progress-notice" title="Войдите, чтобы сохранять результаты" icon="pi pi-cloud-upload"
    dismissible :dismiss-disabled="saving" @dismiss="emit('dismiss', doNotShowAgain)">
    <p>Пока вы не авторизованы, игровой прогресс может быть потерян после завершения гостевой сессии.</p>

    <template #preference>
      <Checkbox :input-id="preferenceId" v-model="doNotShowAgain" binary :disabled="saving" />
      <label :for="preferenceId">Больше не показывать</label>
    </template>

    <template #actions>
      <UiButton size="sm" variant="outlined" :disabled="saving" @click="emit('dismiss', doNotShowAgain)">
        Не сейчас
      </UiButton>
      <UiButton size="sm" :loading="saving" @click="emit('login', doNotShowAgain)">
        Войти
      </UiButton>
    </template>
  </UiActionNotice>
</template>

<style scoped>
.guest-progress-notice {
  animation: guest-progress-notice-enter 0.2s ease-out both;
}

:deep(label) {
  cursor: pointer;
}

@media (prefers-reduced-motion: reduce) {
  .guest-progress-notice {
    animation: none;
  }
}

@keyframes guest-progress-notice-enter {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
