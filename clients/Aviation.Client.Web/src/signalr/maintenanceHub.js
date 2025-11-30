import * as signalR from '@microsoft/signalr'

let connection = null
let started = false

const listeners = {
  workOrderCreated: [],
  workOrderUpdated: [],
  workOrderDeleted: []
}

function ensureConnection() {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/maintenance')
      .withAutomaticReconnect()
      .build()

    connection.on('WorkOrderCreated', payload => {
      listeners.workOrderCreated.forEach(fn => fn(payload))
    })

    connection.on('WorkOrderUpdated', payload => {
      listeners.workOrderUpdated.forEach(fn => fn(payload))
    })

    connection.on('WorkOrderDeleted', id => {
      listeners.workOrderDeleted.forEach(fn => fn(id))
    })
  }
}

export async function startConnection() {
  ensureConnection()
  if (started) return

  await connection.start()
  started = true

  // присоединяемся как диспетчер
  await connection.invoke('JoinDispatchers')
}

export function onWorkOrderCreated(handler) {
  listeners.workOrderCreated.push(handler)
}

export function onWorkOrderUpdated(handler) {
  listeners.workOrderUpdated.push(handler)
}

export function onWorkOrderDeleted(handler) {
  listeners.workOrderDeleted.push(handler)
}
