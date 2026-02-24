import { request } from './http.js'

export function submitOrder(data) {
  return request({
    url: '/api/OrderApi/submit',
    method: 'POST',
    data
  })
}

export function getMyOrders() {
  return request({ url: '/api/OrderApi/list', method: 'GET' })
}

export function getOrderDetail(id) {
  return request({ url: `/api/OrderApi/${id}`, method: 'GET' })
}

export function deleteOrder(id) {
  return request({ url: `/api/OrderApi/${id}`, method: 'DELETE' })
}
