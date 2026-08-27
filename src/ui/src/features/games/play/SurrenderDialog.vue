<script setup lang="ts">
import Dialog from "primevue/dialog";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  visible: boolean;
  loading: boolean;
}>();

const emit = defineEmits<{
  close: [];
  confirm: [];
}>();

function onVisibleUpdate(value: boolean) {
  if (!value && !props.loading) {
    emit("close");
  }
}
</script>

<template>
  <Dialog :visible="visible" modal :dismissable-mask="!loading" :closable="!loading"
    :close-on-escape="!loading" :content-props="{ 'aria-busy': loading ? 'true' : undefined }"
    header="Сдаться?" class="game-dialog"
    @update:visible="onVisibleUpdate">
    <div class="game-dialog__body">
      <p class="game-dialog__text"> Вы уверены, что хотите сдаться? </p>

      <p class="game-dialog__text">
        Игра завершится, и вы увидите* загаданное слово.
      </p>

      <p class="game-dialog__footnote"> * Если это сегодняшняя игра дня, загаданное слово будет показано завтра. </p>

      <div class="game-dialog__actions">
        <UiButton size="md" class="surrender-confirm" :loading="loading" @click="emit('confirm')">
          Сдаться
        </UiButton>
        <UiButton size="md" variant="outlined" :disabled="loading" @click="emit('close')">
          Отмена
        </UiButton>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.game-dialog__body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.game-dialog__text {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.game-dialog__footnote {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: calc(var(--game-dialog-body-font-size) * 0.85);
  line-height: 1.35;
}

.game-dialog__actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
}

.game-dialog__actions>.ui-button {
  flex: 1;
}

:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button) {
  background: var(--color-red-600);
  border-color: var(--color-red-600);
  color: white;
}

:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button:not(:disabled):hover),
:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button:not(:disabled):focus),
:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button:not(:disabled):active) {
  background: var(--color-red-700);
  border-color: var(--color-red-700);
  color: white;
}

:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button .p-button-label),
:global(.game-dialog.p-dialog .surrender-confirm.ui-button.p-button .p-button-icon) {
  color: white;
}
</style>
