<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else class="card">
      <text class="section-title">编辑用户 —— {{ form.userName }}</text>
      <view class="item">
        <text class="item-label">用户名</text>
        <view class="input" style="background:#f3f4f6;color:#9ca3af;">{{ form.userName }}</view>
      </view>
      <view class="item">
        <text class="item-label">姓名</text>
        <!-- #ifdef H5 -->
        <input v-model="form.realName" type="text" placeholder="真实姓名" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.realName" @input="form.realName = $event.detail?.value || ''" placeholder="真实姓名" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="item-label">联系电话</text>
        <!-- #ifdef H5 -->
        <input v-model="form.phone" type="tel" placeholder="手机号" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.phone" @input="form.phone = $event.detail?.value || ''" type="tel" placeholder="手机号" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="item-label">角色</text>
        <picker :range="roleOptions" range-key="label" :value="form.roleIndex" @change="onRoleChange">
          <view class="picker-box">{{ roleOptions[form.roleIndex].label }}</view>
        </picker>
      </view>
      <view style="margin-top:20rpx; padding-top:20rpx; border-top:1rpx solid #e5e7eb;">
        <text class="item-label" style="color:#9ca3af;font-size:24rpx;">如不需要修改密码，请留空</text>
        <view class="item">
          <text class="item-label">新密码</text>
          <!-- #ifdef H5 -->
          <input v-model="form.newPassword" type="password" placeholder="留空则不修改" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.newPassword" @input="form.newPassword = $event.detail?.value || ''" type="password" placeholder="留空则不修改" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <text class="item-label">确认新密码</text>
          <!-- #ifdef H5 -->
          <input v-model="form.confirmNewPassword" type="password" placeholder="再次输入新密码" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.confirmNewPassword" @input="form.confirmNewPassword = $event.detail?.value || ''" type="password" placeholder="再次输入新密码" class="input" />
          <!-- #endif -->
        </view>
      </view>
      <button class="btn-sm btn-blue" style="margin-top:24rpx;" @click="save">保存</button>
    </view>
  </view>
</template>
<script>
import { getUserDetail, updateUser } from '../../api/admin.js'

export default {
  data() {
    return {
      loading: true,
      userId: 0,
      roleOptions: [
        { value: 1, label: '管理员' },
        { value: 2, label: '用户' },
        { value: 3, label: '师傅' }
      ],
      form: { userName: '', realName: '', phone: '', role: 2, roleIndex: 1, newPassword: '', confirmNewPassword: '' }
    }
  },
  onLoad(opts) {
    this.userId = parseInt(opts.id, 10)
    this.loadUser()
  },
  methods: {
    async loadUser() {
      this.loading = true
      try {
        const res = await getUserDetail(this.userId)
        if (res && res.ok) {
          const u = res.user
          const ri = this.roleOptions.findIndex(r => r.value === u.role)
          this.form = {
            userName: u.userName,
            realName: u.realName || '',
            phone: u.phone || '',
            role: u.role,
            roleIndex: ri >= 0 ? ri : 1,
            newPassword: '',
            confirmNewPassword: ''
          }
        }
      } catch (e) {}
      this.loading = false
    },
    onRoleChange(e) {
      const idx = parseInt(e.detail.value, 10)
      this.form.roleIndex = idx
      this.form.role = this.roleOptions[idx].value
    },
    async save() {
      if (this.form.newPassword && this.form.newPassword !== this.form.confirmNewPassword) {
        return uni.showToast({ title: '两次密码不一致', icon: 'none' })
      }
      try {
        const res = await updateUser(this.userId, {
          realName: this.form.realName,
          phone: this.form.phone,
          role: this.form.role,
          newPassword: this.form.newPassword || null,
          confirmNewPassword: this.form.confirmNewPassword || null
        })
        if (res && res.ok) {
          uni.showToast({ title: '已保存' })
          setTimeout(() => uni.navigateBack(), 800)
        } else {
          uni.showToast({ title: res?.msg || '保存失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
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
.item-label { font-size: 28rpx; color: #374151; margin-bottom: 8rpx; display: block; }
.input { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.picker-box { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; color: #374151; }
.btn-sm { height: 72rpx; line-height: 72rpx; border-radius: 10rpx; font-size: 28rpx; }
.btn-blue { background: #2563eb; color: #fff; }
</style>
