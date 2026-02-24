import { request } from './http.js'

export const getMyOrders = (status) =>
  request({ url: '/api/WorkerApi/my-orders' + (status !== undefined && status !== null ? '?status=' + status : ''), method: 'GET' })

export const getMyOrder = (id) =>
  request({ url: `/api/WorkerApi/my-orders/${id}`, method: 'GET' })

export const updateMyOrder = (id, data) =>
  request({ url: `/api/WorkerApi/my-orders/${id}`, method: 'PUT', data })

export const updateOrderStatus = (id, status) =>
  request({ url: `/api/WorkerApi/my-orders/${id}/status`, method: 'POST', data: { status } })

export const addPhoto = (id, url) =>
  request({ url: `/api/WorkerApi/my-orders/${id}/photos`, method: 'POST', data: { url } })

export const removePhoto = (id, url) =>
  request({ url: `/api/WorkerApi/my-orders/${id}/photos/remove`, method: 'POST', data: { url } })
