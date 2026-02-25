import { request } from './http.js'

// 自建工单
export const getManualOrders = (status) =>
  request({ url: '/api/AdminApi/manual-orders' + (status !== undefined && status !== null ? '?status=' + status : ''), method: 'GET' })

export const getManualOrder = (id) =>
  request({ url: `/api/AdminApi/manual-orders/${id}`, method: 'GET' })

export const createManualOrder = (data) =>
  request({ url: '/api/AdminApi/manual-orders', method: 'POST', data })

export const updateManualOrder = (id, data) =>
  request({ url: `/api/AdminApi/manual-orders/${id}`, method: 'PUT', data })

export const updateManualOrderStatus = (id, status) =>
  request({ url: `/api/AdminApi/manual-orders/${id}/status`, method: 'POST', data: { status } })

export const deleteManualOrder = (id) =>
  request({ url: `/api/AdminApi/manual-orders/${id}`, method: 'DELETE' })

// 服务类型
export const getServiceTypes = () =>
  request({ url: '/api/AdminApi/service-types', method: 'GET' })

export const createServiceType = (data) =>
  request({ url: '/api/AdminApi/service-types', method: 'POST', data })

export const updateServiceType = (id, data) =>
  request({ url: `/api/AdminApi/service-types/${id}`, method: 'PUT', data })

export const deleteServiceType = (id) =>
  request({ url: `/api/AdminApi/service-types/${id}`, method: 'DELETE' })

// 服务项目
export const getServiceItems = (typeId) =>
  request({ url: '/api/AdminApi/service-items' + (typeId ? '?typeId=' + typeId : ''), method: 'GET' })

export const getServiceItemsByType = (typeId) =>
  request({ url: `/api/AdminApi/service-items-by-type/${typeId}`, method: 'GET' })

export const createServiceItem = (data) =>
  request({ url: '/api/AdminApi/service-items', method: 'POST', data })

export const updateServiceItem = (id, data) =>
  request({ url: `/api/AdminApi/service-items/${id}`, method: 'PUT', data })

export const deleteServiceItem = (id) =>
  request({ url: `/api/AdminApi/service-items/${id}`, method: 'DELETE' })

// 用户管理
export const getTechnicians = () =>
  request({ url: '/api/AdminApi/technicians', method: 'GET' })

export const setTechnician = (userId) =>
  request({ url: `/api/AdminApi/technicians/set/${userId}`, method: 'POST' })

export const removeTechnician = (userId) =>
  request({ url: `/api/AdminApi/technicians/remove/${userId}`, method: 'POST' })

export const createTechnician = (data) =>
  request({ url: '/api/AdminApi/technicians/create', method: 'POST', data })

export const getUserDetail = (userId) =>
  request({ url: `/api/AdminApi/technicians/${userId}`, method: 'GET' })

export const updateUser = (userId, data) =>
  request({ url: `/api/AdminApi/technicians/update/${userId}`, method: 'POST', data })

// 师傅列表（选择用）
export const getWorkers = () =>
  request({ url: '/api/AdminApi/workers', method: 'GET' })
