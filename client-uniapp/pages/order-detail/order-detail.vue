<template>
  <view class="page" v-if="order">
    <view class="card">
      <text class="orderNo">订单号：{{ order.orderNo }}</text>
      <text class="status">状态：{{ statusText(order.status) }}</text>
      <text class="line">联系人：{{ order.contactName }} {{ order.contactPhone }}</text>
      <text v-if="order.address" class="line">地址：{{ order.address }}</text>
      <text v-if="order.remark" class="line">备注：{{ order.remark }}</text>
      <text class="total">合计：¥{{ order.totalAmount.toFixed(2) }}</text>
    </view>
    <view class="card">
      <text class="title">商品明细</text>
      <view v-for="item in items" :key="item.id" class="item">
        <image v-if="item.imagePath" class="thumb" :src="baseUrl + item.imagePath" mode="aspectFill" />
        <view class="info">
          <text class="name">{{ item.productName }}</text>
          <text class="sub">¥{{ item.price.toFixed(2) }} × {{ item.quantity }}</text>
        </view>
        <text class="amount">¥{{ item.amount.toFixed(2) }}</text>
      </view>
    </view>
    <button class="btn secondary" @click="goBack">返回订单列表</button>
  </view>
  <view v-else class="loading">加载中...</view>
</template>
<script>
import config from '../../config.js'
import { getOrderDetail } from '../../api/order.js'

export default {
  data() {
    return {
      baseUrl: config.BASE_URL,
      id: 0,
      order: null,
      items: []
    }
  },
  onLoad(op) {
    this.id = this._parseId(op)
    this.load()
  },
  methods: {
    _parseId(op) {
      let id = op.id ?? op.Id
      if (id === undefined || id === null || id === '') {
        try {
          if (typeof window !== 'undefined' && window.location && window.location.search) {
            const m = window.location.search.match(/[?&]id=(\d+)/i)
            if (m) id = m[1]
          }
        } catch (e) {}
      }
      return parseInt(id, 10) || 0
    },
    async load() {
      let id = this.id
      if (id <= 0 && typeof window !== 'undefined' && window.location && window.location.search) {
        id = this._parseId({})
      }
      if (!id || id <= 0) {
        this.order = null
        uni.showToast({ title: '订单参数错误', icon: 'none' })
        setTimeout(() => uni.navigateBack(), 1500)
        return
      }
      this.id = id
      try {
        const res = await getOrderDetail(id)
        if (res && res.order) {
          this.order = res.order
          this.items = res.items || []
        } else {
          this.order = null
        }
      } catch (e) {
        this.order = null
      }
    },
    statusText(s) {
      const m = { 0: '待支付', 1: '已支付', 2: '已完成', 3: '已取消' }
      return m[s] || ''
    },
    goBack() {
      uni.navigateBack()
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 24rpx; }
.orderNo { font-weight: 600; display: block; margin-bottom: 8rpx; }
.status { color: #2563eb; display: block; margin-bottom: 8rpx; }
.line { display: block; margin-bottom: 8rpx; color: #374151; }
.total { display: block; margin-top: 16rpx; padding-top: 16rpx; border-top: 1rpx solid #e5e7eb; font-weight: 600; }
.title { font-weight: 600; display: block; margin-bottom: 16rpx; }
.item { display: flex; padding: 16rpx 0; border-bottom: 1rpx solid #f3f4f6; align-items: center; }
.thumb { width: 80rpx; height: 80rpx; border-radius: 8rpx; background: #f3f4f6; margin-right: 16rpx; }
.info { flex: 1; min-width: 0; }
.name { display: block; }
.sub { font-size: 24rpx; color: #6b7280; }
.amount { font-size: 28rpx; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; }
.secondary { background: #e5e7eb; color: #374151; }
.loading { text-align: center; padding: 80rpx; color: #6b7280; }
</style>
