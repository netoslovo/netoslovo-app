<script setup lang="ts">
import Dialog from "primevue/dialog";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  visible: boolean;
  action: "guess" | "hint";
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
  <Dialog :visible="visible" modal dismissable-mask header="Словарь обновился" class="game-dialog"
    @update:visible="onVisibleUpdate">
    <div class="game-dialog__body">
      <p class="game-dialog__text">
        Словарь игры был обновлён. Из-за этого некоторые расстояния ваших попыток могли быть пересчитаны.
      </p>
      <p class="game-dialog__text">
        {{ action === "guess"
          ? "Последняя попытка не была применена. Отправьте слово повторно."
          : "Запрос подсказки не был выполнен. Запросите подсказку повторно."
        }}
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
