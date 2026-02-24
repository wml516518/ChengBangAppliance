<template>
  <view class="page" v-if="product">
    <view class="card">
      <text class="title">商品信息</text>
      <view class="row">
        <image v-if="product.imagePath" class="thumb" :src="baseUrl + product.imagePath" mode="aspectFill" />
        <view class="right">
          <text class="name">{{ product.name }}</text>
          <text class="amount">¥{{ product.price.toFixed(2) }} × {{ quantity }} = ¥{{ total.toFixed(2) }}</text>
        </view>
      </view>
    </view>
    <view class="form">
      <view class="item">
        <text class="label">联系人 *</text>
        <!-- #ifdef H5 -->
        <input v-model="contactName" type="text" placeholder="请输入姓名" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="contactName" @input="contactName = $event.detail?.value || ''" type="text" placeholder="请输入姓名" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">联系电话 *</text>
        <!-- #ifdef H5 -->
        <input v-model="contactPhone" type="tel" placeholder="请输入手机号" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="contactPhone" @input="contactPhone = $event.detail?.value || ''" type="tel" placeholder="请输入手机号" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">支付方式 *</text>
        <view class="pay-row">
          <view :class="['pay-btn', paymentMethod === 1 ? 'active' : '']" @click="paymentMethod = 1">微信支付</view>
          <view :class="['pay-btn', paymentMethod === 2 ? 'active' : '']" @click="paymentMethod = 2">支付宝</view>
        </view>
      </view>
      <view class="item">
        <text class="label">服务地址</text>
        <!-- #ifdef H5 -->
        <input v-model="address" type="text" placeholder="选填" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="address" @input="address = $event.detail?.value || ''" type="text" placeholder="选填" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">备注</text>
        <textarea v-model="remark" placeholder="选填" class="textarea" />
      </view>
    </view>
    <button class="btn primary" :loading="submitting" @click="submit">提交订单</button>
  </view>
  <view v-else class="loading">加载中...</view>
</template>
<script>
import config from '../../config.js'
import { getProduct } from '../../api/client.js'
import { submitOrder } from '../../api/order.js'
import { getUserInfo } from '../../api/auth.js'

export default {
  data() {
    return {
      baseUrl: config.BASE_URL,
      productId: 0,
      quantity: 1,
      product: null,
      contactName: '',
      contactPhone: '',
      paymentMethod: 1,
      address: '',
      remark: '',
      submitting: false
    }
  },
  computed: {
    total() {
      return this.product ? this.product.price * this.quantity : 0
    }
  },
  onLoad(op) {
    this.productId = parseInt(op.productId, 10)
    this.quantity = parseInt(op.quantity, 10) || 1
    if (this.productId < 1) {
      uni.showToast({ title: '参数错误', icon: 'none' })
      return
    }
    this.loadProduct()
    const token = uni.getStorageSync('token')
    if (token) this.loadUserInfoFromApi()
  },
  methods: {
    async loadProduct() {
      try {
        this.product = await getProduct(this.productId)
      } catch (e) {
        this.product = null
      }
    },
    async loadUserInfoFromApi() {
      try {
        const res = await getUserInfo()
        if (res && res.ok) {
          if (res.realName) this.contactName = res.realName
          else if (res.userName) this.contactName = res.userName
          if (res.phone) this.contactPhone = res.phone
        }
      } catch (e) {}
    },
    async submit() {
      if (!this.contactName.trim()) {
        uni.showToast({ title: '请填写联系人', icon: 'none' })
        return
      }
      if (!this.contactPhone.trim()) {
        uni.showToast({ title: '请填写联系电话', icon: 'none' })
        return
      }
      if (this.paymentMethod !== 1 && this.paymentMethod !== 2) {
        uni.showToast({ title: '请选择支付方式', icon: 'none' })
        return
      }
      this.submitting = true
      try {
        const res = await submitOrder({
          productId: this.productId,
          quantity: this.quantity,
          paymentMethod: this.paymentMethod,
          contactName: this.contactName.trim(),
          contactPhone: this.contactPhone.trim(),
          address: this.address.trim(),
          remark: this.remark.trim()
        })
        if (res && res.ok) {
          uni.redirectTo({ url: `/pages/order-result/order-result?orderId=${res.orderId}&orderNo=${res.orderNo || ''}` })
        } else {
          uni.showToast({ title: res.msg || '提交失败', icon: 'none' })
        }
      } catch (e) {
        if (e.message === '未登录') return
        uni.showToast({ title: '网络错误', icon: 'none' })
      }
      this.submitting = false
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 24rpx; }
.title { font-weight: 600; display: block; margin-bottom: 16rpx; }
.row { display: flex; gap: 24rpx; }
.thumb { width: 140rpx; height: 140rpx; border-radius: 12rpx; background: #f3f4f6; }
.right { flex: 1; min-width: 0; }
.name { font-weight: 600; display: block; }
.amount { color: #2563eb; font-size: 28rpx; }
.form { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 24rpx; }
.item { margin-bottom: 24rpx; }
.label { display: block; margin-bottom: 8rpx; font-weight: 500; }
.input, .textarea { border: 1px solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
/* H5 下原生 input 确保可点击、可输入 */
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.textarea { min-height: 120rpx; }
.pay-row { display: flex; gap: 24rpx; }
.pay-btn { padding: 20rpx 32rpx; border: 2rpx solid #e5e7eb; border-radius: 12rpx; }
.pay-btn.active { border-color: #2563eb; background: #eff6ff; color: #2563eb; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; }
.primary { background: #2563eb; color: #fff; }
.loading { text-align: center; padding: 80rpx; color: #6b7280; }
</style>
