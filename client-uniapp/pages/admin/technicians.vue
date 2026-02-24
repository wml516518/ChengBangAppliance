<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else>
      <!-- 从已有用户设为师傅 -->
      <view v-if="availableUsers.length > 0" class="card">
        <text class="section-title">从已有用户添加</text>
        <picker :range="availableUserNames" @change="onPickUser">
          <view class="picker-box">{{ pickedUserName || '选择用户' }}</view>
        </picker>
        <button class="btn-sm btn-blue" style="margin-top:16rpx;" @click="setTech">设为师傅</button>
      </view>

      <!-- 新建师傅 -->
      <view class="card">
        <text class="section-title">新建师傅账号</text>
        <view class="item">
          <!-- #ifdef H5 -->
          <input v-model="newForm.userName" type="text" placeholder="用户名（至少2字符）" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newForm.userName" @input="newForm.userName = $event.detail?.value || ''" placeholder="用户名（至少2字符）" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <!-- #ifdef H5 -->
          <input v-model="newForm.password" type="password" placeholder="密码（至少6位）" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newForm.password" @input="newForm.password = $event.detail?.value || ''" type="password" placeholder="密码（至少6位）" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <!-- #ifdef H5 -->
          <input v-model="newForm.realName" type="text" placeholder="姓名" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newForm.realName" @input="newForm.realName = $event.detail?.value || ''" placeholder="姓名" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <!-- #ifdef H5 -->
          <input v-model="newForm.phone" type="tel" placeholder="联系电话" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newForm.phone" @input="newForm.phone = $event.detail?.value || ''" type="tel" placeholder="联系电话" class="input" />
          <!-- #endif -->
        </view>
        <button class="btn-sm btn-blue" @click="createTech">创建</button>
      </view>

      <!-- 师傅列表 -->
      <view class="section-title" style="margin-top:32rpx;">师傅列表</view>
      <view v-if="technicians.length === 0" class="tip" style="padding:40rpx;">暂无师傅</view>
      <view v-else class="list">
        <view v-for="u in technicians" :key="u.id" class="card tech-card">
          <view class="tech-main">
            <text class="tech-name">{{ u.realName || u.userName }}</text>
            <text class="tech-phone">{{ u.phone || '—' }}</text>
          </view>
          <text class="tech-user">账号: {{ u.userName }}</text>
          <text class="act del" @click="removeTech(u.id, u.realName || u.userName)">移除师傅身份</text>
        </view>
      </view>
    </view>
  </view>
</template>
<script>
import { getTechnicians, setTechnician, removeTechnician, createTechnician } from '../../api/admin.js'

export default {
  data() {
    return {
      loading: true,
      technicians: [],
      availableUsers: [],
      pickedUserId: null,
      pickedUserName: '',
      newForm: { userName: '', password: '', realName: '', phone: '' }
    }
  },
  computed: {
    availableUserNames() {
      return this.availableUsers.map(u => (u.realName || u.userName) + (u.phone ? ` (${u.phone})` : ''))
    }
  },
  onShow() { this.load() },
  methods: {
    async load() {
      this.loading = true
      try {
        const res = await getTechnicians()
        if (res && res.ok) {
          this.technicians = res.technicians || []
          this.availableUsers = res.availableUsers || []
        }
      } catch (e) {}
      this.loading = false
    },
    onPickUser(e) {
      const idx = e.detail.value
      const u = this.availableUsers[idx]
      if (u) {
        this.pickedUserId = u.id
        this.pickedUserName = u.realName || u.userName
      }
    },
    async setTech() {
      if (!this.pickedUserId) return uni.showToast({ title: '请选择用户', icon: 'none' })
      try {
        const res = await setTechnician(this.pickedUserId)
        if (res && res.ok) {
          uni.showToast({ title: '已设为师傅' })
          this.pickedUserId = null; this.pickedUserName = ''
          this.load()
        } else {
          uni.showToast({ title: res?.msg || '失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    async createTech() {
      if (!this.newForm.userName.trim()) return uni.showToast({ title: '请输入用户名', icon: 'none' })
      if (!this.newForm.password) return uni.showToast({ title: '请输入密码', icon: 'none' })
      try {
        const res = await createTechnician(this.newForm)
        if (res && res.ok) {
          uni.showToast({ title: '已创建' })
          this.newForm = { userName: '', password: '', realName: '', phone: '' }
          this.load()
        } else {
          uni.showToast({ title: res?.msg || '创建失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    removeTech(userId, name) {
      uni.showModal({
        title: '确认', content: `确定取消「${name}」的师傅身份？`,
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await removeTechnician(userId)
            if (res && res.ok) { uni.showToast({ title: '已移除' }); this.load() }
          } catch (e) { uni.showToast({ title: '操作失败', icon: 'none' }) }
        }
      })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 20rpx; }
.section-title { font-size: 30rpx; font-weight: 600; margin-bottom: 16rpx; display: block; }
.item { margin-bottom: 16rpx; }
.input { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.picker-box { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; color: #374151; }
.list { display: flex; flex-direction: column; gap: 16rpx; }
.tech-card {}
.tech-main { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8rpx; }
.tech-name { font-weight: 600; font-size: 30rpx; }
.tech-phone { color: #6b7280; font-size: 28rpx; }
.tech-user { font-size: 24rpx; color: #9ca3af; display: block; margin-bottom: 12rpx; }
.act { font-size: 26rpx; }
.act.del { color: #dc2626; }
.btn-sm { height: 72rpx; line-height: 72rpx; border-radius: 10rpx; font-size: 28rpx; }
.btn-blue { background: #2563eb; color: #fff; }
</style>
