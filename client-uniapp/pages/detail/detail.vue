<template>
  <view class="page" v-if="product">
    <image v-if="product.imagePath" class="pic" :src="baseUrl + product.imagePath" mode="widthFix" />
    <view v-else class="pic empty-pic">暂无图片</view>
    <view class="info">
      <text class="name">{{ product.name }}</text>
      <text class="price">¥{{ product.price.toFixed(2) }}</text>
      <text v-if="product.description" class="desc">{{ product.description }}</text>
    </view>
    <view class="row">
      <text>数量</text>
      <!-- #ifdef H5 -->
      <input v-model.number="quantity" type="number" min="1" max="99" class="input input-native" />
      <!-- #endif -->
      <!-- #ifndef H5 -->
      <input :value="quantity" @input="quantity = parseInt($event.detail?.value || $event.target?.value || '1', 10) || 1" type="number" min="1" max="99" class="input" />
      <!-- #endif -->
    </view>
    <button class="btn primary" @click="goCheckout">立即下单</button>
  </view>
  <view v-else class="loading">加载中...</view>
</template>
<script>
import config from '../../config.js'
import { getProduct } from '../../api/client.js'

export default {
  data() {
    return {
      baseUrl: config.BASE_URL,
      id: 0,
      product: null,
      quantity: 1
    }
  },
  onLoad(op) {
    this.id = parseInt(op.id, 10)
    this.load()
  },
  methods: {
    async load() {
      try {
        this.product = await getProduct(this.id)
        if (!this.product) this.product = null
      } catch (e) {
        this.product = null
      }
    },
    goCheckout() {
      let q = this.quantity
      if (q < 1) q = 1
      uni.navigateTo({ url: `/pages/checkout/checkout?productId=${this.id}&quantity=${q}` })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.pic { width: 100%; max-height: 500rpx; background: #f3f4f6; }
.empty-pic { display: flex; align-items: center; justify-content: center; height: 300rpx; color: #9ca3af; }
.info { padding: 24rpx 0; }
.name { font-size: 36rpx; font-weight: 600; display: block; margin-bottom: 16rpx; }
.price { color: #2563eb; font-size: 44rpx; font-weight: 600; }
.desc { display: block; margin-top: 16rpx; color: #6b7280; font-size: 28rpx; }
.row { margin: 24rpx 0; display: flex; align-items: center; gap: 16rpx; }
.input { border: 1px solid #d1d5db; padding: 16rpx; border-radius: 8rpx; width: 160rpx; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.btn { margin-top: 32rpx; width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; }
.primary { background: #2563eb; color: #fff; }
</style>
