<template>
  <view class="page">
    <view class="header">
      <text class="hello">你好，{{ userName }}</text>
      <text class="role">师傅</text>
    </view>

    <view class="filter-row">
      <view :class="['filter', currentStatus === null ? 'active' : '']" @click="currentStatus = null">全部</view>
      <view :class="['filter', currentStatus === 1 ? 'active' : '']" @click="currentStatus = 1">待处理</view>
      <view :class="['filter', currentStatus === 2 ? 'active' : '']" @click="currentStatus = 2">进行中</view>
      <view :class="['filter', currentStatus === 3 ? 'active' : '']" @click="currentStatus = 3">已完成</view>
    </view>

    <view v-if="loading" class="tip">加载中...</view>
    <view v-else-if="list.length === 0" class="tip">暂无工单</view>
    <view v-else class="list">
      <view v-for="o in list" :key="o.id" class="card" @click="goDetail(o.id)">
        <view class="card-top">
          <text class="order-no">{{ o.orderNo }}</text>
          <text :class="['status', 'status-' + o.status]">{{ statusText(o.status) }}</text>
        </view>
        <view class="card-info">
          <text class="type">{{ o.typeName }} · {{ o.itemName }}</text>
          <text class="warranty">{{ o.warrantyType === 0 ? '包内' : '保外' }}</text>
        </view>
        <view class="card-contact">
          <text>{{ o.contactName }} {{ o.contactPhone }}</text>
        </view>
        <view v-if="o.area || o.address" class="card-addr">
          <text>{{ o.area }} {{ o.address }}</text>
        </view>
        <view class="card-bottom">
          <text v-if="o.amount" class="amount">¥{{ o.amount }}</text>
          <text class="time">{{ formatTime(o.createTime) }}</text>
        </view>
      </view>
    </view>

    <view class="bottom-nav">
      <view class="nav-item active">
        <text class="nav-icon">📋</text>
        <text class="nav-label">我的工单</text>
      </view>
      <view class="nav-item" @click="doLogout">
        <text class="nav-icon">👤</text>
        <text class="nav-label">退出</text>
      </view>
    </view>
  </view>
</template>
<script>
import { getMyOrders } from '../../api/worker.js'
import { checkLogin, logout } from '../../utils/auth.js'

export default {
  data() {
    return {
      userName: '',
      list: [],
      loading: true,
      currentStatus: null
    }
  },
  watch: {
    currentStatus() { this.load() }
  },
  onShow() {
    if (!checkLogin()) return
    this.userName = uni.getStorageSync('userName') || '师傅'
    this.load()
  },
  methods: {
    async load() {
      this.loading = true
      try {
        const res = await getMyOrders(this.currentStatus)
        this.list = (res && res.ok) ? (res.list || []) : []
      } catch (e) {
        this.list = []
      }
      this.loading = false
    },
    statusText(s) {
      return { 0: '待指派', 1: '已指派', 2: '进行中', 3: '已完成', 4: '已取消' }[s] || ''
    },
    formatTime(t) {
      if (!t) return ''
      const d = new Date(t)
      return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0') + ' ' + String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
    },
    goDetail(id) {
      uni.navigateTo({ url: '/pages/worker/order-detail?id=' + id })
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
.page { padding: 24rpx; padding-bottom: 140rpx; }
.header { background: #16a34a; color: #fff; border-radius: 16rpx; padding: 32rpx; margin-bottom: 24rpx; }
.hello { font-size: 34rpx; font-weight: 600; display: block; }
.role { font-size: 24rpx; opacity: 0.8; margin-top: 4rpx; display: block; }
.filter-row { display: flex; gap: 16rpx; margin-bottom: 24rpx; }
.filter { padding: 14rpx 28rpx; background: #e5e7eb; border-radius: 12rpx; font-size: 26rpx; }
.filter.active { background: #16a34a; color: #fff; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.list { display: flex; flex-direction: column; gap: 20rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.card-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12rpx; }
.order-no { font-size: 24rpx; color: #6b7280; font-family: monospace; }
.status { font-size: 24rpx; padding: 4rpx 16rpx; border-radius: 8rpx; }
.status-1 { background: #dbeafe; color: #1e40af; }
.status-2 { background: #fef3c7; color: #92400e; }
.status-3 { background: #dcfce7; color: #166534; }
.status-4 { background: #e5e7eb; color: #6b7280; }
.card-info { display: flex; justify-content: space-between; margin-bottom: 8rpx; }
.type { font-weight: 600; font-size: 30rpx; }
.warranty { font-size: 24rpx; color: #6b7280; }
.card-contact { font-size: 28rpx; color: #374151; margin-bottom: 4rpx; }
.card-addr { font-size: 24rpx; color: #9ca3af; margin-bottom: 8rpx; }
.card-bottom { display: flex; justify-content: space-between; align-items: center; margin-top: 8rpx; }
.amount { font-size: 30rpx; color: #dc2626; font-weight: 600; }
.time { font-size: 24rpx; color: #9ca3af; }
.bottom-nav { position: fixed; bottom: 0; left: 0; right: 0; height: 110rpx; background: #fff; display: flex; border-top: 1rpx solid #e5e7eb; z-index: 999; }
.nav-item { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4rpx; }
.nav-item.active .nav-label { color: #16a34a; }
.nav-icon { font-size: 40rpx; }
.nav-label { font-size: 22rpx; color: #6b7280; }
</style>
