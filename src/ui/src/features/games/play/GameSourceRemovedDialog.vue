<script setup lang="ts">
import Dialog from "primevue/dialog";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  visible: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();

function onVisibleUpdate(value: boolean) {
  if (!value) {
    emit("close");
  }
}
</script>

<template>
  <Dialog :visible="visible" modal dismissable-mask header="Игра отменена" class="game-dialog"
    @update:visible="onVisibleUpdate">
    <div class="game-dialog__body">
      <p class="game-dialog__text">
        Словарь игры был обновлён, и, к сожалению, в новой версии нет загаданного слова.
      </p>
      <p class="game-dialog__text">
        Текущая игра отменена автоматически. Результат не засчитывается.
      </p>

      <div class="game-dialog__actions">
        <UiButton size="md" @click="emit('close')">
          Понятно
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

.game-dialog__actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
}

.game-dialog__actions>.ui-button {
  flex: 1;
}
</style>
