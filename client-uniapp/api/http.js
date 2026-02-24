import config from '../config.js'

function getToken() {
  return uni.getStorageSync('token') || ''
}

export function request(options) {
  const token = getToken()
  return new Promise((resolve, reject) => {
    uni.request({
      url: (options.url.startsWith('http') ? options.url : config.BASE_URL + options.url),
      method: options.method || 'GET',
      data: options.data,
      header: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: 'Bearer ' + token } : {}),
        ...options.header
      },
      success: (res) => {
        if (res.statusCode === 401) {
          uni.removeStorageSync('token')
          uni.removeStorageSync('role')
          uni.showToast({ title: '请先登录', icon: 'none' })
          setTimeout(() => {
            uni.reLaunch({ url: '/pages/login/login' })
          }, 1500)
          reject(new Error('未登录'))
          return
        }
        resolve(res.data)
      },
      fail: (err) => reject(err)
    })
  })
}
