<template>
  <view class="page">
    <view class="card">
      <text class="success">下单成功</text>
      <text class="orderNo">订单号：{{ orderNo }}</text>
    </view>
    <button class="btn primary" @click="goOrders">去订单列表</button>
    <button class="btn secondary" @click="goHome">返回首页</button>
  </view>
</template>
<script>
export default {
  data() {
    return { orderNo: '' }
  },
  onLoad(op) {
    this.orderNo = op.orderNo ?? op.orderno ?? ''
    if (!this.orderNo && typeof window !== 'undefined' && window.location && window.location.search) {
      try {
        const m = window.location.search.match(/[?&]orderNo=([^&]*)/i)
        if (m) this.orderNo = decodeURIComponent(m[1] || '')
      } catch (e) {}
    }
  },
  methods: {
    goOrders() {
      uni.switchTab({ url: '/pages/my-orders/my-orders' })
    },
    goHome() {
      uni.switchTab({ url: '/pages/index/index' })
    }
  }
}
</script>
<style scoped>
.page { padding: 48rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 48rpx; text-align: center; margin-bottom: 24rpx; }
.success { color: #16a34a; font-size: 36rpx; font-weight: 600; display: block; margin-bottom: 16rpx; }
.orderNo { display: block; margin-bottom: 24rpx; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; margin-bottom: 24rpx; }
.primary { background: #2563eb; color: #fff; }
.secondary { background: #e5e7eb; color: #374151; }
</style>
