import axios from 'axios'

const api = axios.create({
  baseURL: '/api'
})

const apiKey = import.meta.env.VITE_API_KEY
if (apiKey) {
  api.defaults.headers.common['X-API-KEY'] = apiKey
}

export default api
