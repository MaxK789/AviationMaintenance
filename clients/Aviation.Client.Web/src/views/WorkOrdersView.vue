<script setup>
import { ref, onMounted, computed } from 'vue'
import { getAircraft } from '../services/aircraftService'
import {
  getWorkOrders,
  createWorkOrder,
  changeWorkOrderStatus,
  deleteWorkOrder
} from '../services/workOrderService'
import {
  startConnection,
  onWorkOrderCreated,
  onWorkOrderUpdated,
  onWorkOrderDeleted
} from '../signalr/maintenanceHub'

const aircraft = ref([])
const workOrders = ref([])

const loading = ref(false)
const error = ref(null)

const filters = ref({
  aircraftId: '',
  status: '',
  priority: ''
})

const form = ref({
  aircraftId: '',
  title: '',
  description: '',
  priority: 'Medium',
  plannedStart: '',
  plannedEnd: ''
})

const hasAircraft = computed(() => aircraft.value.length > 0)

async function loadInitial() {
  loading.value = true
  error.value = null
  try {
    aircraft.value = await getAircraft()
    await loadWorkOrders()
    if (!form.value.aircraftId && aircraft.value[0]) {
      form.value.aircraftId = aircraft.value[0].id.toString()
    }
  } catch (e) {
    console.error(e)
    error.value = 'Помилка завантаження даних'
  } finally {
    loading.value = false
  }
}

async function loadWorkOrders() {
  loading.value = true
  error.value = null
  try {
    const filt = {
      aircraftId: filters.value.aircraftId
        ? Number(filters.value.aircraftId)
        : undefined,
      status: filters.value.status || undefined,
      priority: filters.value.priority || undefined
    }
    workOrders.value = await getWorkOrders(filt)
  } catch (e) {
    console.error(e)
    error.value = 'Не вдалося завантажити заявки'
  } finally {
    loading.value = false
  }
}

async function submitForm() {
  if (!form.value.aircraftId || !form.value.title) return

  try {
    const payload = {
      aircraftId: Number(form.value.aircraftId),
      title: form.value.title,
      description: form.value.description || null,
      priority: form.value.priority,
      plannedStart: form.value.plannedStart || null,
      plannedEnd: form.value.plannedEnd || null
    }
    await createWorkOrder(payload)
    form.value.title = ''
    form.value.description = ''
    form.value.priority = 'Medium'
    form.value.plannedStart = ''
    form.value.plannedEnd = ''
    await loadWorkOrders()
  } catch (e) {
    console.error(e)
    alert('Помилка під час створення заявки')
  }
}

async function setStatus(id, status) {
  try {
    await changeWorkOrderStatus(id, status)
    await loadWorkOrders()
  } catch (e) {
    console.error(e)
    alert('Помилка під час зміни статусу')
  }
}

async function remove(id) {
  if (!confirm('Видалити заявку?')) return
  try {
    await deleteWorkOrder(id)
    await loadWorkOrders()
  } catch (e) {
    console.error(e)
    alert('Помилка під час видалення')
  }
}

onMounted(async () => {
  await loadInitial()

  try {
    await startConnection()

    // На будь-яку подію просто перезавантажуємо список.
    // Це простіше, ніж вручну патчити масив.
    onWorkOrderCreated(async () => {
      await loadWorkOrders()
    })
    onWorkOrderUpdated(async () => {
      await loadWorkOrders()
    })
    onWorkOrderDeleted(async () => {
      await loadWorkOrders()
    })
  } catch (e) {
    console.error('SignalR connection failed', e)
  }
})
</script>

<template>
  <section>
    <h2>Заявки на ТО</h2>

    <div v-if="error" class="error">{{ error }}</div>

    <!-- Фільтри -->
    <div class="card filters">
      <h3>Фільтр</h3>
      <div class="filters-row">
        <label>
          Літак
          <select v-model="filters.aircraftId">
            <option value="">Усі</option>
            <option v-for="a in aircraft" :key="a.id" :value="a.id">
              {{ a.tailNumber }} ({{ a.model }})
            </option>
          </select>
        </label>

        <label>
          Статус
          <select v-model="filters.status">
            <option value="">Усі</option>
            <option value="New">New</option>
            <option value="InProgress">InProgress</option>
            <option value="Done">Done</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </label>

        <label>
          Пріоритет
          <select v-model="filters.priority">
            <option value="">Усі</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </label>

        <button @click="loadWorkOrders">Застосувати</button>
      </div>
    </div>

    <!-- Форма створення -->
    <form class="card form" @submit.prevent="submitForm">
      <h3>Створити заявку</h3>

      <template v-if="hasAircraft">
        <label>
          Літак
          <select v-model="form.aircraftId">
            <option v-for="a in aircraft" :key="a.id" :value="a.id">
              {{ a.tailNumber }} ({{ a.model }})
            </option>
          </select>
        </label>
      </template>
      <p v-else class="hint">
        Спочатку додайте хоча б один літак на вкладці "Літаки".
      </p>

      <label>
        Назва
        <input v-model="form.title" placeholder="Наприклад: Заміна колеса" />
      </label>

      <label>
        Опис
        <textarea
          v-model="form.description"
          placeholder="Коротко, що потрібно зробити"
        />
      </label>

      <label>
        Пріоритет
        <select v-model="form.priority">
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
      </label>

      <div class="dates">
        <label>
          Плановий початок
          <input type="datetime-local" v-model="form.plannedStart" />
        </label>
        <label>
          Планове завершення
          <input type="datetime-local" v-model="form.plannedEnd" />
        </label>
      </div>

      <button type="submit" :disabled="!hasAircraft">Створити</button>
    </form>

    <!-- Таблиця заявок -->
    <div class="card table-card">
      <h3>Список заявок</h3>
      <div v-if="loading">Завантаження...</div>
      <table v-else>
        <thead>
          <tr>
            <th>ID</th>
            <th>Літак</th>
            <th>Назва</th>
            <th>Пріоритет</th>
            <th>План</th>
            <th>Статус</th>
            <th />
          </tr>
        </thead>
        <tbody>
          <tr v-for="w in workOrders" :key="w.id">
            <td>{{ w.id }}</td>
            <td>
              {{ w.aircraftTailNumber }}
              <div class="sub">{{ w.aircraftModel }}</div>
            </td>
            <td>
              {{ w.title }}
              <div v-if="w.description" class="sub">
                {{ w.description }}
              </div>
            </td>
            <td>{{ w.priority }}</td>
            <td>
              <div class="sub">
                <span v-if="w.plannedStart">from {{ w.plannedStart }}</span>
                <span v-if="w.plannedEnd"><br />to {{ w.plannedEnd }}</span>
              </div>
            </td>
            <td>{{ w.status }}</td>
            <td class="actions">
              <button @click="setStatus(w.id, 'InProgress')" v-if="w.status === 'New'">
                У роботу
              </button>
              <button @click="setStatus(w.id, 'Done')" v-if="w.status === 'InProgress'">
                Завершити
              </button>
              <button @click="setStatus(w.id, 'Cancelled')" v-if="w.status !== 'Done'">
                Скасувати
              </button>
              <button @click="remove(w.id)">Видалити</button>
            </td>
          </tr>
          <tr v-if="workOrders.length === 0">
            <td colspan="7">Наразі заявок немає</td>
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
.filters-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: flex-end;
}
label {
  display: block;
}
select,
input,
textarea {
  display: block;
  margin-top: 4px;
  padding: 6px 8px;
  min-width: 160px;
}
textarea {
  min-height: 60px;
}
button {
  padding: 6px 10px;
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
  vertical-align: top;
}
.sub {
  font-size: 12px;
  color: #6b7280;
}
.actions {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.error {
  color: #b91c1c;
  margin-bottom: 8px;
}
.hint {
  color: #6b7280;
}
.dates {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}
</style>
