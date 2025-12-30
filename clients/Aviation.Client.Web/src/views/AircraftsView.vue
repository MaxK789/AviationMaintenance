<script setup>
import { ref, onMounted } from 'vue'
import {
  getAircraft,
  createAircraft,
  deleteAircraft
} from '../services/aircraftService'

const aircraft = ref([])
const loading = ref(false)
const error = ref(null)

const form = ref({
  tailNumber: '',
  model: '',
  status: 'InService'
})

async function loadAircraft() {
  loading.value = true
  error.value = null
  try {
    aircraft.value = await getAircraft()
  } catch (e) {
    console.error(e)
    error.value = 'Не вдалося завантажити літаки'
  } finally {
    loading.value = false
  }
}

async function submitForm() {
  if (!form.value.tailNumber || !form.value.model) return

  try {
    await createAircraft({ ...form.value })
    form.value.tailNumber = ''
    form.value.model = ''
    form.value.status = 'InService'
    await loadAircraft()
  } catch (e) {
    console.error(e)
    alert('Помилка під час створення літака')
  }
}

async function remove(id) {
  if (!confirm('Видалити літак?')) return
  try {
    await deleteAircraft(id)
    await loadAircraft()
  } catch (e) {
    console.error(e)
    alert('Помилка під час видалення')
  }
}

onMounted(loadAircraft)
</script>

<template>
  <section>
    <h2>Літаки</h2>

    <div v-if="error" class="error">{{ error }}</div>

    <form class="card form" @submit.prevent="submitForm">
      <h3>Додати літак</h3>
      <label>
        Бортовий номер
        <input v-model="form.tailNumber" placeholder="UR-XXX" />
      </label>
      <label>
        Модель
        <input v-model="form.model" placeholder="Boeing 737-800" />
      </label>
      <label>
        Статус
        <select v-model="form.status">
          <option value="InService">В експлуатації</option>
          <option value="InMaintenance">На ТО</option>
          <option value="OutOfService">Виведений</option>
        </select>
      </label>
      <button type="submit">Зберегти</button>
    </form>

    <div class="card table-card">
      <h3>Список літаків</h3>
      <div v-if="loading">Завантаження...</div>
      <table v-else>
        <thead>
          <tr>
            <th>ID</th>
            <th>Борт</th>
            <th>Модель</th>
            <th>Статус</th>
            <th />
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in aircraft" :key="a.id">
            <td>{{ a.id }}</td>
            <td>{{ a.tailNumber }}</td>
            <td>{{ a.model }}</td>
            <td>{{ a.status }}</td>
            <td>
              <button @click="remove(a.id)">Видалити</button>
            </td>
          </tr>
          <tr v-if="aircraft.length === 0">
            <td colspan="5">Поки немає літаків</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style scoped>
h2 {
  margin-bottom: 12px;
}
.card {
  background: white;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.1);
}
.form label {
  display: block;
  margin-bottom: 8px;
}
.form input,
.form select {
  display: block;
  margin-top: 4px;
  padding: 6px 8px;
  width: 100%;
  max-width: 320px;
}
button {
  margin-top: 8px;
  padding: 6px 12px;
  cursor: pointer;
}
.table-card table {
  width: 100%;
  border-collapse: collapse;
}
.table-card th,
.table-card td {
  padding: 6px 8px;
  border-bottom: 1px solid #e5e7eb;
}
.error {
  color: #b91c1c;
  margin-bottom: 8px;
}
</style>
