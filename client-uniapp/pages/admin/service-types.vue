<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else>
      <view class="list">
        <view v-for="t in list" :key="t.id" class="card">
          <view v-if="editId === t.id" class="edit-form">
            <!-- #ifdef H5 -->
            <input v-model="editName" type="text" placeholder="类型名称" class="input input-native" />
            <input v-model="editSort" type="number" placeholder="排序" class="input short input-native" />
            <!-- #endif -->
            <!-- #ifndef H5 -->
            <input :value="editName" @input="editName = $event.detail?.value || ''" placeholder="类型名称" class="input" />
            <input :value="editSort" @input="editSort = $event.detail?.value || ''" type="number" placeholder="排序" class="input short" />
            <!-- #endif -->
            <view class="edit-row">
              <view :class="['radio-btn', editEnabled ? 'active' : '']" @click="editEnabled = !editEnabled">{{ editEnabled ? '启用' : '停用' }}</view>
              <button class="btn-sm btn-blue" @click="saveEdit(t.id)">保存</button>
              <button class="btn-sm btn-gray" @click="editId = 0">取消</button>
            </view>
          </view>
          <view v-else class="card-body" @click="goItems(t.id)">
            <view class="card-main">
              <text class="name">{{ t.name }}</text>
              <text class="badge" :style="{ background: t.isEnabled ? '#dcfce7' : '#e5e7eb', color: t.isEnabled ? '#166534' : '#6b7280' }">{{ t.isEnabled ? '启用' : '停用' }}</text>
            </view>
            <text class="sort">排序: {{ t.sortOrder }}</text>
            <view class="card-actions">
              <text class="act edit" @click.stop="startEdit(t)">编辑</text>
              <text class="act del" @click.stop="remove(t.id)">删除</text>
              <text class="act items" @click.stop="goItems(t.id)">管理项目 →</text>
            </view>
          </view>
        </view>
      </view>

      <view class="add-form">
        <text class="section-title">添加服务类型</text>
        <!-- #ifdef H5 -->
        <input v-model="newName" type="text" placeholder="类型名称，如：安装服务" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="newName" @input="newName = $event.detail?.value || ''" placeholder="类型名称，如：安装服务" class="input" />
        <!-- #endif -->
        <view style="display:flex;gap:16rpx;margin-top:16rpx;">
          <!-- #ifdef H5 -->
          <input v-model="newSort" type="number" placeholder="排序" class="input short input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="newSort" @input="newSort = $event.detail?.value || ''" type="number" placeholder="排序" class="input short" />
          <!-- #endif -->
          <button class="btn-sm btn-blue" @click="add">添加</button>
        </view>
      </view>
    </view>
  </view>
</template>
<script>
import { getServiceTypes, createServiceType, updateServiceType, deleteServiceType } from '../../api/admin.js'

export default {
  data() {
    return {
      list: [], loading: true,
      newName: '', newSort: '0',
      editId: 0, editName: '', editSort: '0', editEnabled: true
    }
  },
  onShow() { this.load() },
  methods: {
    async load() {
      this.loading = true
      try {
        const res = await getServiceTypes()
        this.list = (res && res.ok) ? res.list : []
      } catch (e) { this.list = [] }
      this.loading = false
    },
    async add() {
      if (!this.newName.trim()) return uni.showToast({ title: '请输入名称', icon: 'none' })
      try {
        const res = await createServiceType({ name: this.newName.trim(), sortOrder: parseInt(this.newSort) || 0, isEnabled: true })
        if (res && res.ok) { this.newName = ''; this.newSort = '0'; this.load(); uni.showToast({ title: '已添加' }) }
        else uni.showToast({ title: res?.msg || '失败', icon: 'none' })
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    startEdit(t) {
      this.editId = t.id; this.editName = t.name; this.editSort = String(t.sortOrder); this.editEnabled = t.isEnabled
    },
    async saveEdit(id) {
      try {
        const res = await updateServiceType(id, { name: this.editName.trim(), sortOrder: parseInt(this.editSort) || 0, isEnabled: this.editEnabled })
        if (res && res.ok) { this.editId = 0; this.load(); uni.showToast({ title: '已保存' }) }
        else uni.showToast({ title: res?.msg || '失败', icon: 'none' })
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    remove(id) {
      uni.showModal({
        title: '确认', content: '确定删除？',
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await deleteServiceType(id)
            if (res && res.ok) { this.load(); uni.showToast({ title: '已删除' }) }
            else uni.showToast({ title: res?.msg || '删除失败', icon: 'none' })
          } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
        }
      })
    },
    goItems(typeId) {
      uni.navigateTo({ url: '/pages/admin/service-items?typeId=' + typeId })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.list { display: flex; flex-direction: column; gap: 16rpx; margin-bottom: 32rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.card-body {}
.card-main { display: flex; align-items: center; gap: 16rpx; margin-bottom: 8rpx; }
.name { font-weight: 600; font-size: 30rpx; }
.badge { padding: 4rpx 16rpx; border-radius: 8rpx; font-size: 22rpx; }
.sort { font-size: 24rpx; color: #9ca3af; }
.card-actions { display: flex; gap: 24rpx; margin-top: 16rpx; }
.act { font-size: 26rpx; color: #2563eb; }
.act.del { color: #dc2626; }
.act.items { margin-left: auto; }
.edit-form { display: flex; flex-direction: column; gap: 12rpx; }
.edit-row { display: flex; gap: 12rpx; align-items: center; }
.add-form { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.section-title { font-size: 30rpx; font-weight: 600; margin-bottom: 16rpx; }
.input { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.input.short { width: 200rpx; flex-shrink: 0; }
.radio-btn { padding: 12rpx 24rpx; border: 2rpx solid #e5e7eb; border-radius: 8rpx; font-size: 26rpx; }
.radio-btn.active { border-color: #2563eb; background: #eff6ff; color: #2563eb; }
.btn-sm { height: 72rpx; line-height: 72rpx; border-radius: 10rpx; font-size: 28rpx; padding: 0 32rpx; }
.btn-blue { background: #2563eb; color: #fff; }
.btn-gray { background: #6b7280; color: #fff; }
</style>
