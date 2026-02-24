<template>
  <view class="page">
    <view class="card">
      <text class="title">注册</text>
      <view class="item">
        <text class="label">用户名 *</text>
        <!-- #ifdef H5 -->
        <input v-model="userName" type="text" placeholder="至少2个字符" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="userName" @input="userName = $event.detail?.value || ''" type="text" placeholder="至少2个字符" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">密码 *</text>
        <!-- #ifdef H5 -->
        <input v-model="password" type="password" placeholder="至少6个字符" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="password" @input="password = $event.detail?.value || ''" type="password" placeholder="至少6个字符" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">确认密码 *</text>
        <!-- #ifdef H5 -->
        <input v-model="confirmPassword" type="password" placeholder="再次输入密码" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="confirmPassword" @input="confirmPassword = $event.detail?.value || ''" type="password" placeholder="再次输入密码" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">姓名</text>
        <!-- #ifdef H5 -->
        <input v-model="realName" type="text" placeholder="选填" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="realName" @input="realName = $event.detail?.value || ''" type="text" placeholder="选填" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">手机号</text>
        <!-- #ifdef H5 -->
        <input v-model="phone" type="tel" placeholder="选填" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="phone" @input="phone = $event.detail?.value || ''" type="tel" placeholder="选填" class="input" />
        <!-- #endif -->
      </view>
      <button class="btn primary" :loading="loading" @click="register">注册</button>
      <view class="link" @click="goLogin">已有账号？去登录</view>
    </view>
  </view>
</template>
<script>
import { register as apiRegister } from '../../api/auth.js'

export default {
  data() {
    return {
      userName: '',
      password: '',
      confirmPassword: '',
      realName: '',
      phone: '',
      loading: false
    }
  },
  methods: {
    async register() {
      if (!this.userName.trim() || this.userName.trim().length < 2) {
        uni.showToast({ title: '用户名至少2个字符', icon: 'none' })
        return
      }
      if (!this.password || this.password.length < 6) {
        uni.showToast({ title: '密码至少6个字符', icon: 'none' })
        return
      }
      if (this.password !== this.confirmPassword) {
        uni.showToast({ title: '两次密码不一致', icon: 'none' })
        return
      }
      this.loading = true
      try {
        const res = await apiRegister({
          userName: this.userName.trim(),
          password: this.password,
          realName: this.realName.trim() || undefined,
          phone: this.phone.trim() || undefined
        })
        if (res && res.ok) {
          uni.showToast({ title: res.msg || '注册成功' })
          setTimeout(() => {
            uni.navigateTo({ url: '/pages/login/login' })
          }, 1000)
        } else {
          uni.showToast({ title: res.msg || '注册失败', icon: 'none' })
        }
      } catch (e) {
        uni.showToast({ title: '网络错误', icon: 'none' })
      }
      this.loading = false
    },
    goLogin() {
      uni.navigateBack()
    }
  }
}
</script>
<style scoped>
.page { padding: 48rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 48rpx; }
.title { font-size: 40rpx; font-weight: 600; display: block; margin-bottom: 48rpx; }
.item { margin-bottom: 32rpx; }
.label { display: block; margin-bottom: 12rpx; font-weight: 500; }
.input { border: 1px solid #d1d5db; padding: 24rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; margin-top: 24rpx; }
.primary { background: #2563eb; color: #fff; }
.link { text-align: center; margin-top: 32rpx; color: #2563eb; font-size: 28rpx; }
</style>
