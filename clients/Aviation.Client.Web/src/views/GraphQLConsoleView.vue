<script setup>
import { ref } from 'vue'

const apiKey = import.meta.env.VITE_API_KEY

const query = ref(`query {
  aircrafts {
    id
    tailNumber
    model
    status
  }
}`)

const variablesText = ref('{}')
const result = ref('')
const error = ref(null)
const loading = ref(false)

async function execute() {
  loading.value = true
  error.value = null
  result.value = ''

  try {
    let variables = {}
    if (variablesText.value && variablesText.value.trim() !== '') {
      variables = JSON.parse(variablesText.value)
    }

    const response = await fetch('/api/graphql', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-API-KEY': apiKey
      },
      body: JSON.stringify({
        query: query.value,
        variables
      })
    })

    const json = await response.json()
    result.value = JSON.stringify(json, null, 2)

    if (!response.ok) {
      error.value = `HTTP ${response.status}`
    } else if (json.errors) {
      error.value = 'GraphQL errors, див. результат'
    }
  } catch (e) {
    console.error(e)
    error.value = String(e)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <section class="graphql-console">
    <h2>GraphQL Console</h2>
    <p class="hint">
      Тут можна вручну надсилати GraphQL-запити на <code>/api/graphql</code>.
    </p>

    <div class="panes">
      <div class="pane">
        <h3>Запит</h3>
        <textarea v-model="query" spellcheck="false" />
      </div>
      <div class="pane">
        <h3>Variables (JSON)</h3>
        <textarea v-model="variablesText" spellcheck="false" />
      </div>
    </div>

    <button @click="execute" :disabled="loading">
      {{ loading ? 'Виконую...' : 'Виконати' }}
    </button>

    <div v-if="error" class="error">Помилка: {{ error }}</div>

    <div class="result card">
      <h3>Результат</h3>
      <pre>{{ result }}</pre>
    </div>
  </section>
</template>

<style scoped>
.graphql-console h2 {
  margin-bottom: 4px;
}
.hint {
  margin-bottom: 12px;
  color: #6b7280;
}
.panes {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}
.pane {
  flex: 1 1 320px;
  display: flex;
  flex-direction: column;
}
textarea {
  min-height: 180px;
  width: 100%;
  resize: vertical;
  padding: 8px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    'Liberation Mono', 'Courier New', monospace;
}
button {
  padding: 6px 12px;
  margin-bottom: 12px;
  cursor: pointer;
}
.result {
  background: #0f172a;
  color: #e5e7eb;
  border-radius: 8px;
  padding: 12px;
  max-height: 400px;
  overflow: auto;
}
.result pre {
  white-space: pre-wrap;
  word-wrap: break-word;
}
.error {
  color: #b91c1c;
  margin-bottom: 8px;
}
.card {
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.3);
}
</style>
