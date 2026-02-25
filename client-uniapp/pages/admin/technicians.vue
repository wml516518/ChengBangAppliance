<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else>
      <!-- 从已有用户设为师傅 -->
      <view v-if="availableUsers.length > 0" class="card">
        <text class="section-title">从已有用户设为师傅</text>
        <picker :range="availableUserNames" @change="onPickUser">
          <view class="picker-box">{{ pickedUserName || '选择用户' }}</view>
        </picker>
        <button class="btn-sm btn-blue" style="margin-top:16rpx;" @click="setTech">设为师傅</button>
      </view>

      <!-- 添加用户 -->
      <view class="card">
        <text class="section-title">添加用户</text>
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
          <input v-model="newForm.confirmPassword" type="password" placeholder="确认密码" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newForm.confirmPassword" @input="newForm.confirmPassword = $event.detail?.value || ''" type="password" placeholder="确认密码" class="input" />
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
        <view class="item">
          <text class="item-label">角色</text>
          <picker :range="roleOptions" range-key="label" :value="newForm.roleIndex" @change="onRoleChange">
            <view class="picker-box">{{ roleOptions[newForm.roleIndex].label }}</view>
          </picker>
        </view>
        <button class="btn-sm btn-blue" @click="createUser">创建</button>
      </view>

      <!-- 用户列表 -->
      <view class="section-title" style="margin-top:32rpx;">用户列表</view>
      <view v-if="allUsers.length === 0" class="tip" style="padding:40rpx;">暂无用户</view>
      <view v-else class="list">
        <view v-for="u in allUsers" :key="u.id" class="card tech-card">
          <view class="tech-main">
            <text class="tech-name">{{ u.realName || u.userName }}</text>
            <text class="badge" :class="u.roleName === '管理员' ? 'badge-danger' : (u.roleName === '师傅' ? 'badge-success' : 'badge-secondary')">{{ u.roleName }}</text>
          </view>
          <text class="tech-phone">电话: {{ u.phone || '—' }}</text>
          <text class="tech-user">账号: {{ u.userName }}</text>
          <view class="actions">
            <text class="act edit" @click="editUser(u.id)">编辑</text>
            <text v-if="u.isTechnician" class="act del" @click="removeTech(u.id, u.realName || u.userName)">移除师傅</text>
            <text v-else class="act add" @click="setTechById(u.id)">设为师傅</text>
          </view>
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
      allUsers: [],
      technicians: [],
      availableUsers: [],
      pickedUserId: null,
      pickedUserName: '',
      roleOptions: [
        { value: 1, label: '管理员' },
        { value: 2, label: '用户' },
        { value: 3, label: '师傅' }
      ],
      newForm: { userName: '', password: '', confirmPassword: '', realName: '', phone: '', role: 2, roleIndex: 1 }
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
          this.allUsers = res.allUsers || []
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
    onRoleChange(e) {
      const idx = parseInt(e.detail.value, 10)
      this.newForm.roleIndex = idx
      this.newForm.role = this.roleOptions[idx].value
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
    async setTechById(userId) {
      try {
        const res = await setTechnician(userId)
        if (res && res.ok) {
          uni.showToast({ title: '已设为师傅' })
          this.load()
        } else {
          uni.showToast({ title: res?.msg || '失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    async createUser() {
      if (!this.newForm.userName.trim()) return uni.showToast({ title: '请输入用户名', icon: 'none' })
      if (!this.newForm.password) return uni.showToast({ title: '请输入密码', icon: 'none' })
      if (this.newForm.password !== this.newForm.confirmPassword) return uni.showToast({ title: '两次密码不一致', icon: 'none' })
      try {
        const res = await createTechnician({
          userName: this.newForm.userName,
          password: this.newForm.password,
          confirmPassword: this.newForm.confirmPassword,
          realName: this.newForm.realName,
          phone: this.newForm.phone,
          role: this.newForm.role
        })
        if (res && res.ok) {
          uni.showToast({ title: '已创建' })
          this.newForm = { userName: '', password: '', confirmPassword: '', realName: '', phone: '', role: 2, roleIndex: 1 }
          this.load()
        } else {
          uni.showToast({ title: res?.msg || '创建失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    editUser(userId) {
      uni.navigateTo({ url: `/pages/admin/user-edit?id=${userId}` })
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
.tech-phone { color: #6b7280; font-size: 28rpx; display: block; }
.tech-user { font-size: 24rpx; color: #9ca3af; display: block; margin-bottom: 12rpx; }
.badge { font-size: 22rpx; padding: 4rpx 12rpx; border-radius: 8rpx; }
.badge-success { background: #dcfce7; color: #166534; }
.badge-secondary { background: #e5e7eb; color: #374151; }
.badge-danger { background: #fee2e2; color: #b91c1c; }
.item-label { font-size: 28rpx; color: #374151; margin-bottom: 8rpx; display: block; }
.actions { display: flex; gap: 24rpx; align-items: center; }
.act { font-size: 26rpx; }
.act.edit { color: #2563eb; }
.act.del { color: #dc2626; }
.act.add { color: #2563eb; }
.btn-sm { height: 72rpx; line-height: 72rpx; border-radius: 10rpx; font-size: 28rpx; }
.btn-blue { background: #2563eb; color: #fff; }
</style>
