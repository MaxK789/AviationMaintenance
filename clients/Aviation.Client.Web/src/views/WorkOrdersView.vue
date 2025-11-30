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
    error.value = 'Ошибка загрузки данных'
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
    error.value = 'Не удалось загрузить заявки'
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
    alert('Ошибка при создании заявки')
  }
}

async function setStatus(id, status) {
  try {
    await changeWorkOrderStatus(id, status)
    await loadWorkOrders()
  } catch (e) {
    console.error(e)
    alert('Ошибка при изменении статуса')
  }
}

async function remove(id) {
  if (!confirm('Удалить заявку?')) return
  try {
    await deleteWorkOrder(id)
    await loadWorkOrders()
  } catch (e) {
    console.error(e)
    alert('Ошибка при удалении')
  }
}

onMounted(async () => {
  await loadInitial()

  try {
    await startConnection()

    // На любое событие просто перезагружаем список.
    // Это проще, чем вручную патчить массив.
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

    <!-- Фильтры -->
    <div class="card filters">
      <h3>Фильтр</h3>
      <div class="filters-row">
        <label>
          Самолёт
          <select v-model="filters.aircraftId">
            <option value="">Все</option>
            <option v-for="a in aircraft" :key="a.id" :value="a.id">
              {{ a.tailNumber }} ({{ a.model }})
            </option>
          </select>
        </label>

        <label>
          Статус
          <select v-model="filters.status">
            <option value="">Все</option>
            <option value="New">New</option>
            <option value="InProgress">InProgress</option>
            <option value="Done">Done</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </label>

        <label>
          Приоритет
          <select v-model="filters.priority">
            <option value="">Все</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </label>

        <button @click="loadWorkOrders">Применить</button>
      </div>
    </div>

    <!-- Форма создания -->
    <form class="card form" @submit.prevent="submitForm">
      <h3>Создать заявку</h3>

      <template v-if="hasAircraft">
        <label>
          Самолёт
          <select v-model="form.aircraftId">
            <option v-for="a in aircraft" :key="a.id" :value="a.id">
              {{ a.tailNumber }} ({{ a.model }})
            </option>
          </select>
        </label>
      </template>
      <p v-else class="hint">
        Сначала добавьте хотя бы один самолёт на вкладке "Самолёты".
      </p>

      <label>
        Заголовок
        <input v-model="form.title" placeholder="Например: Замена колеса" />
      </label>

      <label>
        Описание
        <textarea
          v-model="form.description"
          placeholder="Кратко, что нужно сделать"
        />
      </label>

      <label>
        Приоритет
        <select v-model="form.priority">
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
      </label>

      <div class="dates">
        <label>
          Плановое начало
          <input type="datetime-local" v-model="form.plannedStart" />
        </label>
        <label>
          Плановое окончание
          <input type="datetime-local" v-model="form.plannedEnd" />
        </label>
      </div>

      <button type="submit" :disabled="!hasAircraft">Создать</button>
    </form>

    <!-- Таблица заявок -->
    <div class="card table-card">
      <h3>Список заявок</h3>
      <div v-if="loading">Загрузка...</div>
      <table v-else>
        <thead>
          <tr>
            <th>ID</th>
            <th>Самолёт</th>
            <th>Заголовок</th>
            <th>Приоритет</th>
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
                В работу
              </button>
              <button @click="setStatus(w.id, 'Done')" v-if="w.status === 'InProgress'">
                Завершить
              </button>
              <button @click="setStatus(w.id, 'Cancelled')" v-if="w.status !== 'Done'">
                Отменить
              </button>
              <button @click="remove(w.id)">Удалить</button>
            </td>
          </tr>
          <tr v-if="workOrders.length === 0">
            <td colspan="7">Заявок пока нет</td>
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
