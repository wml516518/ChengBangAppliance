<template>
  <view class="page">
    <view v-if="loading" class="loading">加载中...</view>
    <view v-else-if="list.length === 0" class="empty">暂无订单</view>
    <view v-else class="list">
      <view v-for="o in list" :key="o.id" class="card" @click="goDetail(o.id)">
        <view class="row">
          <text class="orderNo">订单 {{ o.orderNo }}</text>
          <text class="time">{{ formatTime(o.createTime) }}</text>
        </view>
        <view class="row bottom">
          <text class="amount">¥{{ o.totalAmount.toFixed(2) }} {{ payText(o.paymentMethod) }}</text>
          <view class="right">
            <text class="status">{{ statusText(o.status) }}</text>
            <text class="del" @click.stop="del(o.id)">删除</text>
          </view>
        </view>
      </view>
    </view>
  </view>
</template>
<script>
import { getMyOrders, deleteOrder } from '../../api/order.js'
import { checkLogin } from '../../utils/auth.js'

export default {
  data() {
    return { list: [], loading: true }
  },
  onShow() {
    if (!checkLogin()) return
    this.load()
  },
  methods: {
    async load() {
      this.loading = true
      try {
        this.list = await getMyOrders() || []
      } catch (e) {
        this.list = []
      }
      this.loading = false
    },
    formatTime(t) {
      if (!t) return ''
      const d = new Date(t)
      return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0') + ' ' + String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
    },
    statusText(s) {
      const m = { 0: '待支付', 1: '已支付', 2: '已完成', 3: '已取消' }
      return m[s] || ''
    },
    payText(m) {
      if (m === 1) return '(微信)'
      if (m === 2) return '(支付宝)'
      return ''
    },
    goDetail(id) {
      uni.navigateTo({ url: '/pages/order-detail/order-detail?id=' + id })
    },
    async del(id) {
      uni.showModal({
        title: '提示',
        content: '确定删除该订单？',
        success: async (res) => {
          if (!res.confirm) return
          try {
            const r = await deleteOrder(id)
            if (r && r.ok) {
              uni.showToast({ title: '已删除' })
              this.load()
            } else {
              uni.showToast({ title: r.msg || '删除失败', icon: 'none' })
            }
          } catch (e) {
            uni.showToast({ title: '删除失败', icon: 'none' })
          }
        }
      })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.loading, .empty { text-align: center; padding: 80rpx; color: #6b7280; }
.list { display: flex; flex-direction: column; gap: 24rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.row { display: flex; justify-content: space-between; margin-bottom: 12rpx; }
.bottom { margin-bottom: 0; }
.orderNo { font-weight: 600; }
.time { font-size: 24rpx; color: #6b7280; }
.amount { font-size: 28rpx; }
.right { display: flex; align-items: center; gap: 24rpx; }
.status { color: #2563eb; font-size: 28rpx; }
.del { font-size: 26rpx; color: #991b1b; }
</style>
