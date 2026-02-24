<template>
  <view class="page">
    <view class="header">
      <text class="hello">你好，{{ userName }}</text>
      <text class="role">管理员</text>
    </view>
    <view class="menu-grid">
      <view class="menu-item" @click="go('/pages/admin/manual-orders')">
        <text class="menu-icon">📋</text>
        <text class="menu-label">自建工单</text>
      </view>
      <view class="menu-item" @click="go('/pages/admin/manual-order-create')">
        <text class="menu-icon">➕</text>
        <text class="menu-label">创建工单</text>
      </view>
      <view class="menu-item" @click="go('/pages/admin/service-types')">
        <text class="menu-icon">📂</text>
        <text class="menu-label">服务类型</text>
      </view>
      <view class="menu-item" @click="go('/pages/admin/service-items')">
        <text class="menu-icon">🔧</text>
        <text class="menu-label">服务项目</text>
      </view>
      <view class="menu-item" @click="go('/pages/admin/technicians')">
        <text class="menu-icon">👷</text>
        <text class="menu-label">师傅管理</text>
      </view>
    </view>
    <button class="btn logout" @click="doLogout">退出登录</button>
  </view>
</template>
<script>
import { checkLogin, logout } from '../../utils/auth.js'

export default {
  data() {
    return { userName: '' }
  },
  onShow() {
    if (!checkLogin()) return
    this.userName = uni.getStorageSync('userName') || '管理员'
  },
  methods: {
    go(url) {
      uni.navigateTo({ url })
    },
    doLogout() {
      uni.showModal({
        title: '提示', content: '确定退出登录？',
        success: (r) => { if (r.confirm) logout() }
      })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.header { background: #2563eb; color: #fff; border-radius: 16rpx; padding: 40rpx 32rpx; margin-bottom: 32rpx; }
.hello { font-size: 36rpx; font-weight: 600; display: block; }
.role { font-size: 24rpx; opacity: 0.8; margin-top: 8rpx; display: block; }
.menu-grid { display: flex; flex-wrap: wrap; gap: 20rpx; }
.menu-item { width: calc(33.33% - 14rpx); background: #fff; border-radius: 16rpx; padding: 32rpx 16rpx; display: flex; flex-direction: column; align-items: center; gap: 12rpx; }
.menu-icon { font-size: 48rpx; }
.menu-label { font-size: 26rpx; color: #374151; font-weight: 500; }
.btn { width: 100%; height: 80rpx; line-height: 80rpx; border-radius: 12rpx; font-size: 30rpx; margin-top: 40rpx; }
.logout { background: #fff; color: #dc2626; border: 1rpx solid #fecaca; }
</style>
