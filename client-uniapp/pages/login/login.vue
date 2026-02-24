<template>
  <view class="page">
    <view class="card">
      <text class="title">诚邦家电</text>
      <text class="subtitle">请登录您的账号</text>
      <view class="item">
        <text class="label">用户名</text>
        <!-- #ifdef H5 -->
        <input v-model="userName" type="text" placeholder="请输入用户名" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="userName" @input="userName = $event.detail?.value || ''" type="text" placeholder="请输入用户名" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">密码</text>
        <!-- #ifdef H5 -->
        <input v-model="password" type="password" placeholder="请输入密码" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="password" @input="password = $event.detail?.value || ''" type="password" placeholder="请输入密码" class="input" />
        <!-- #endif -->
      </view>
      <button class="btn primary" :loading="loading" @click="login">登录</button>
      <view class="link" @click="goRegister">没有账号？去注册</view>
    </view>
  </view>
</template>
<script>
import { login as apiLogin } from '../../api/auth.js'

export default {
  data() {
    return { userName: '', password: '', loading: false }
  },
  onShow() {
    const token = uni.getStorageSync('token')
    if (token) {
      this.redirectByRole()
    }
  },
  methods: {
    redirectByRole() {
      const role = uni.getStorageSync('role')
      if (role === 'admin') {
        uni.reLaunch({ url: '/pages/admin/index' })
      } else if (role === 'worker') {
        uni.reLaunch({ url: '/pages/worker/index' })
      } else {
        uni.reLaunch({ url: '/pages/index/index' })
      }
    },
    async login() {
      if (!this.userName.trim()) {
        uni.showToast({ title: '请输入用户名', icon: 'none' })
        return
      }
      if (!this.password) {
        uni.showToast({ title: '请输入密码', icon: 'none' })
        return
      }
      this.loading = true
      try {
        const res = await apiLogin(this.userName.trim(), this.password)
        if (res && res.ok && res.token) {
          uni.setStorageSync('token', res.token)
          uni.setStorageSync('userName', res.userName || '')

          let role = 'user'
          if (res.isAdmin) role = 'admin'
          else if (res.isTechnician) role = 'worker'
          uni.setStorageSync('role', role)

          uni.showToast({ title: '登录成功' })
          setTimeout(() => this.redirectByRole(), 500)
        } else {
          uni.showToast({ title: res.msg || '登录失败', icon: 'none' })
        }
      } catch (e) {
        uni.showToast({ title: '网络错误', icon: 'none' })
      }
      this.loading = false
    },
    goRegister() {
      uni.navigateTo({ url: '/pages/register/register' })
    }
  }
}
</script>
<style scoped>
.page { padding: 48rpx; min-height: 100vh; display: flex; align-items: center; justify-content: center; box-sizing: border-box; }
.card { background: #fff; border-radius: 16rpx; padding: 48rpx; width: 100%; }
.title { font-size: 44rpx; font-weight: 700; display: block; color: #2563eb; }
.subtitle { font-size: 28rpx; color: #6b7280; display: block; margin-bottom: 48rpx; margin-top: 8rpx; }
.item { margin-bottom: 32rpx; }
.label { display: block; margin-bottom: 12rpx; font-weight: 500; }
.input { border: 1px solid #d1d5db; padding: 24rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; margin-top: 24rpx; }
.primary { background: #2563eb; color: #fff; }
.link { text-align: center; margin-top: 32rpx; color: #2563eb; font-size: 28rpx; }
</style>
