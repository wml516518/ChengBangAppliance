<template>
  <view class="page">
    <scroll-view scroll-x class="tabs">
      <view :class="['tab', currentStatus === null ? 'active' : '']" @click="currentStatus = null">全部</view>
      <view :class="['tab', currentStatus === 0 ? 'active' : '']" @click="currentStatus = 0">待指派</view>
      <view :class="['tab', currentStatus === 1 ? 'active' : '']" @click="currentStatus = 1">已指派</view>
      <view :class="['tab', currentStatus === 2 ? 'active' : '']" @click="currentStatus = 2">进行中</view>
      <view :class="['tab', currentStatus === 3 ? 'active' : '']" @click="currentStatus = 3">已完成</view>
      <view :class="['tab', currentStatus === 4 ? 'active' : '']" @click="currentStatus = 4">已取消</view>
    </scroll-view>
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else-if="list.length === 0" class="tip">暂无工单</view>
    <view v-else class="list">
      <view v-for="o in list" :key="o.id" class="card" @click="goDetail(o.id)">
        <view class="card-top">
          <text class="order-no">{{ o.orderNo }}</text>
          <text :class="['status', 'status-' + o.status]">{{ statusText(o.status) }}</text>
        </view>
        <view class="card-row">
          <text class="label">类型</text>
          <text>{{ o.typeName }}</text>
        </view>
        <view class="card-row">
          <text class="label">项目</text>
          <text>{{ o.itemName }}</text>
        </view>
        <view class="card-row">
          <text class="label">联系人</text>
          <text>{{ o.contactName }} {{ o.contactPhone }}</text>
        </view>
        <view class="card-row">
          <text class="label">金额</text>
          <text class="amount">¥{{ o.amount.toFixed(2) }}</text>
          <text class="warranty" :style="{ color: o.warrantyType === 1 ? '#dc2626' : '#16a34a' }">{{ o.warrantyType === 1 ? '保外' : '包内' }}</text>
        </view>
        <view v-if="o.workerName" class="card-row">
          <text class="label">师傅</text>
          <text>{{ o.workerName }}</text>
        </view>
        <view class="card-row">
          <text class="label">时间</text>
          <text class="time">{{ formatTime(o.createTime) }}</text>
        </view>
      </view>
    </view>
    <view class="fab" @click="goCreate">+</view>
  </view>
</template>
<script>
import { getManualOrders } from '../../api/admin.js'

export default {
  data() {
    return { list: [], loading: true, currentStatus: null }
  },
  watch: {
    currentStatus() { this.load() }
  },
  onShow() { this.load() },
  methods: {
    async load() {
      this.loading = true
      try {
        const res = await getManualOrders(this.currentStatus)
        this.list = (res && res.ok) ? res.list : []
      } catch (e) { this.list = [] }
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
      uni.navigateTo({ url: '/pages/admin/manual-order-detail?id=' + id })
    },
    goCreate() {
      uni.navigateTo({ url: '/pages/admin/manual-order-create' })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 160rpx; }
.tabs { display: flex; white-space: nowrap; gap: 12rpx; margin-bottom: 24rpx; }
.tab { display: inline-block; padding: 14rpx 28rpx; background: #e5e7eb; border-radius: 12rpx; font-size: 26rpx; flex-shrink: 0; }
.tab.active { background: #2563eb; color: #fff; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.list { display: flex; flex-direction: column; gap: 20rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.card-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16rpx; }
.order-no { font-size: 24rpx; color: #6b7280; font-family: monospace; }
.status { font-size: 24rpx; padding: 4rpx 16rpx; border-radius: 8rpx; }
.status-0 { background: #fef3c7; color: #92400e; }
.status-1 { background: #dbeafe; color: #1e40af; }
.status-2 { background: #bfdbfe; color: #1e40af; }
.status-3 { background: #dcfce7; color: #166534; }
.status-4 { background: #e5e7eb; color: #6b7280; }
.card-row { display: flex; align-items: center; gap: 12rpx; margin-bottom: 8rpx; font-size: 28rpx; }
.label { color: #6b7280; font-size: 24rpx; min-width: 70rpx; }
.amount { color: #dc2626; font-weight: 600; }
.warranty { font-size: 24rpx; margin-left: 12rpx; }
.time { font-size: 24rpx; color: #9ca3af; }
.fab { position: fixed; right: 40rpx; bottom: 140rpx; width: 96rpx; height: 96rpx; border-radius: 50%; background: #2563eb; color: #fff; font-size: 52rpx; display: flex; align-items: center; justify-content: center; box-shadow: 0 4rpx 16rpx rgba(37,99,235,0.4); }
</style>
