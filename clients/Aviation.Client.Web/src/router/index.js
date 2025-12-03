import { createRouter, createWebHistory } from 'vue-router'
import AircraftsView from '../views/AircraftsView.vue'
import WorkOrdersView from '../views/WorkOrdersView.vue'
import GraphQLConsoleView from '../views/GraphQLConsoleView.vue'

const routes = [
  { path: '/', redirect: '/aircraft' },
  { path: '/aircraft', component: AircraftsView },
  { path: '/workorders', component: WorkOrdersView },
  { path: '/graphql', component: GraphQLConsoleView }
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})
