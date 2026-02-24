import { request } from './http.js'

export function getCategories() {
  return request({ url: '/api/ClientApi/categories', method: 'GET' })
}

export function getProducts(category) {
  const url = category ? `/api/ClientApi/products?category=${encodeURIComponent(category)}` : '/api/ClientApi/products'
  return request({ url, method: 'GET' })
}

export function getProduct(id) {
  return request({ url: `/api/ClientApi/products/${id}`, method: 'GET' })
}
