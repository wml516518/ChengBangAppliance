import { request } from './http.js'

export function login(userName, password) {
  return request({
    url: '/api/AuthApi/login',
    method: 'POST',
    data: { userName, password }
  })
}

export function register(data) {
  return request({
    url: '/api/AuthApi/register',
    method: 'POST',
    data
  })
}

export function getUserInfo() {
  return request({ url: '/api/AuthApi/info', method: 'GET' })
}
