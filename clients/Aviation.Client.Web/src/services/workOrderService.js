import api from './api'

export async function getWorkOrders(filters = {}) {
  const { aircraftId, status, priority } = filters
  const params = {}
  if (aircraftId) params.aircraftId = aircraftId
  if (status) params.status = status
  if (priority) params.priority = priority

  const { data } = await api.get('/workorders', { params })
  return data
}

export async function getWorkOrderById(id) {
  const { data } = await api.get(`/workorders/${id}`)
  return data
}

export async function createWorkOrder(payload) {
  const { data } = await api.post('/workorders', payload)
  return data
}

export async function updateWorkOrder(id, payload) {
  const { data } = await api.put(`/workorders/${id}`, payload)
  return data
}

export async function changeWorkOrderStatus(id, newStatus) {
  const { data } = await api.put(`/workorders/${id}/status`, {
    newStatus
  })
  return data
}

export async function deleteWorkOrder(id) {
  await api.delete(`/workorders/${id}`)
}
