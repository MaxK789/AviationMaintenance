import { createRouter, createWebHistory } from 'vue-router'
import AircraftsView from '../views/AircraftsView.vue'
import WorkOrdersView from '../views/WorkOrdersView.vue'

const routes = [
  { path: '/', redirect: '/aircraft' },
  { path: '/aircraft', component: AircraftsView },
  { path: '/workorders', component: WorkOrdersView }
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})
