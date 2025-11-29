import api from './api'

export async function getAircraft(status) {
  const params = {}
  if (status) params.status = status
  const { data } = await api.get('/aircraft', { params })
  return data
}

export async function getAircraftById(id) {
  const { data } = await api.get(`/aircraft/${id}`)
  return data
}

export async function createAircraft(payload) {
  const { data } = await api.post('/aircraft', payload)
  return data
}

export async function updateAircraft(id, payload) {
  const { data } = await api.put(`/aircraft/${id}`, payload)
  return data
}

export async function deleteAircraft(id) {
  await api.delete(`/aircraft/${id}`)
}
