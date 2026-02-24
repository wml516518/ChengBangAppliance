export function checkLogin() {
  const token = uni.getStorageSync('token')
  if (!token) {
    uni.reLaunch({ url: '/pages/login/login' })
    return false
  }
  return true
}

export function getRole() {
  return uni.getStorageSync('role') || 'user'
}

export function logout() {
  uni.removeStorageSync('token')
  uni.removeStorageSync('userName')
  uni.removeStorageSync('role')
  uni.reLaunch({ url: '/pages/login/login' })
}
