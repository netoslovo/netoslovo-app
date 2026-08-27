<script setup lang="ts">
import Dialog from "primevue/dialog";
import UiButton from "../../../shared/ui/UiButton.vue";

defineProps<{
  visible: boolean;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
}>();
</script>

<template>
  <Dialog :visible="visible" modal dismissable-mask header="Почему слово не найдено?" class="game-dialog"
    @update:visible="emit('update:visible', $event)">
    <div class="game-dialog__body">
      <p class="game-dialog__text game-dialog__text--word-not-found">
        Сообщение «Слово не найдено» не обязательно означает, что такого слова не существует.
      </p>
      <p class="game-dialog__text game-dialog__text--word-not-found">
        Возможно, оно не встретилось в текстах, на основе которых составлялся словарь игры, оказалось слишком
        редким или не вполне подходящим для игры по смыслу.
      </p>

      <div class="game-dialog__actions">
        <UiButton size="md" @click="emit('update:visible', false)">
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
