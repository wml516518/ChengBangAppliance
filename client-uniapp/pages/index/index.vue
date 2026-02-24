<template>
  <view class="page">
    <view class="tabs">
      <view :class="['tab', !currentCategory ? 'active' : '']" @click="currentCategory = ''">全部</view>
      <view v-for="c in categories" :key="c" :class="['tab', currentCategory === c ? 'active' : '']" @click="currentCategory = c">{{ c }}</view>
    </view>
    <view v-if="loading" class="loading">加载中...</view>
    <view v-else-if="list.length === 0" class="empty">{{ currentCategory ? '该分类暂无商品' : '暂无上架商品' }}</view>
    <view v-else class="list">
      <view v-for="p in list" :key="p.id" class="card" @click="goDetail(p.id)">
        <image v-if="p.imagePath" class="thumb" :src="baseUrl + p.imagePath" mode="aspectFill" />
        <view v-else class="thumb empty-img">无图</view>
        <view class="info">
          <text class="name">{{ p.name }}</text>
          <text class="price">¥{{ p.price.toFixed(2) }}</text>
        </view>
      </view>
    </view>

    <view class="bottom-nav">
      <view class="nav-item active">
        <text class="nav-icon">🏠</text>
        <text class="nav-label">首页</text>
      </view>
      <view class="nav-item" @click="go('/pages/my-orders/my-orders')">
        <text class="nav-icon">📦</text>
        <text class="nav-label">我的订单</text>
      </view>
      <view class="nav-item" @click="doLogout">
        <text class="nav-icon">👤</text>
        <text class="nav-label">退出</text>
      </view>
    </view>
  </view>
</template>
<script>
import config from '../../config.js'
import { getCategories, getProducts } from '../../api/client.js'
import { checkLogin, logout } from '../../utils/auth.js'

export default {
  data() {
    return {
      baseUrl: config.BASE_URL,
      categories: [],
      currentCategory: '',
      list: [],
      loading: true
    }
  },
  watch: {
    currentCategory() {
      this.loadProducts()
    }
  },
  onShow() {
    if (!checkLogin()) return
    this.loadCategories()
    this.loadProducts()
  },
  methods: {
    async loadCategories() {
      try {
        this.categories = await getCategories() || []
      } catch (e) {
        this.categories = []
      }
    },
    async loadProducts() {
      this.loading = true
      try {
        this.list = await getProducts(this.currentCategory || undefined) || []
      } catch (e) {
        this.list = []
      }
      this.loading = false
    },
    goDetail(id) {
      uni.navigateTo({ url: '/pages/detail/detail?id=' + id })
    },
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
.page { padding: 24rpx; padding-bottom: 140rpx; }
.tabs { display: flex; flex-wrap: wrap; gap: 16rpx; margin-bottom: 24rpx; }
.tab { padding: 16rpx 28rpx; background: #e5e7eb; border-radius: 12rpx; font-size: 28rpx; }
.tab.active { background: #2563eb; color: #fff; }
.loading, .empty { text-align: center; padding: 80rpx 0; color: #6b7280; }
.list { display: flex; flex-direction: column; gap: 24rpx; }
.card { display: flex; background: #fff; border-radius: 16rpx; overflow: hidden; padding: 24rpx; }
.thumb { width: 160rpx; height: 160rpx; flex-shrink: 0; background: #f3f4f6; border-radius: 12rpx; }
.empty-img { display: flex; align-items: center; justify-content: center; color: #9ca3af; font-size: 24rpx; }
.info { flex: 1; margin-left: 24rpx; min-width: 0; display: flex; flex-direction: column; justify-content: center; }
.name { font-weight: 600; font-size: 30rpx; margin-bottom: 8rpx; }
.price { color: #2563eb; font-size: 32rpx; }
.bottom-nav { position: fixed; bottom: 0; left: 0; right: 0; height: 110rpx; background: #fff; display: flex; border-top: 1rpx solid #e5e7eb; z-index: 999; }
.nav-item { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4rpx; }
.nav-item.active .nav-label { color: #2563eb; }
.nav-icon { font-size: 40rpx; }
.nav-label { font-size: 22rpx; color: #6b7280; }
</style>
