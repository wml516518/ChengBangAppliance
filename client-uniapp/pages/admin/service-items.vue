<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else>
      <view v-if="typeName" class="type-header">
        <text class="type-name">{{ typeName }}</text>
        <text class="type-sub">下属服务项目</text>
      </view>

      <view class="list">
        <view v-for="item in list" :key="item.id" class="card">
          <view v-if="editId === item.id" class="edit-form">
            <!-- #ifdef H5 -->
            <input v-model="editName" type="text" placeholder="项目名称" class="input input-native" />
            <input v-model="editSort" type="number" placeholder="排序" class="input short input-native" />
            <!-- #endif -->
            <!-- #ifndef H5 -->
            <input :value="editName" @input="editName = $event.detail?.value || ''" placeholder="项目名称" class="input" />
            <input :value="editSort" @input="editSort = $event.detail?.value || ''" type="number" placeholder="排序" class="input short" />
            <!-- #endif -->
            <view class="edit-row">
              <view :class="['radio-btn', editEnabled ? 'active' : '']" @click="editEnabled = !editEnabled">{{ editEnabled ? '启用' : '停用' }}</view>
              <button class="btn-sm btn-blue" @click="saveEdit(item.id)">保存</button>
              <button class="btn-sm btn-gray" @click="editId = 0">取消</button>
            </view>
          </view>
          <view v-else class="card-body">
            <view class="card-main">
              <text class="name">{{ item.name }}</text>
              <text class="badge" :style="{ background: item.isEnabled ? '#dcfce7' : '#e5e7eb', color: item.isEnabled ? '#166534' : '#6b7280' }">{{ item.isEnabled ? '启用' : '停用' }}</text>
            </view>
            <text class="sort">排序: {{ item.sortOrder }} | 所属: {{ item.typeName }}</text>
            <view class="card-actions">
              <text class="act edit" @click="startEdit(item)">编辑</text>
              <text class="act del" @click="remove(item.id)">删除</text>
            </view>
          </view>
        </view>
        <view v-if="list.length === 0" class="tip" style="padding:40rpx;">暂无项目</view>
      </view>

      <view class="add-form">
        <text class="section-title">添加服务项目</text>
        <!-- #ifdef H5 -->
        <input v-model="newName" type="text" placeholder="项目名称，如：空调安装" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="newName" @input="newName = $event.detail?.value || ''" placeholder="项目名称，如：空调安装" class="input" />
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
import { getServiceItems, getServiceTypes, createServiceItem, updateServiceItem, deleteServiceItem } from '../../api/admin.js'

export default {
  data() {
    return {
      typeId: 0, typeName: '',
      list: [], loading: true,
      newName: '', newSort: '0',
      editId: 0, editName: '', editSort: '0', editEnabled: true
    }
  },
  onLoad(op) {
    this.typeId = parseInt(op.typeId) || 0
    this.load()
  },
  methods: {
    async load() {
      this.loading = true
      try {
        if (this.typeId) {
          const typesRes = await getServiceTypes()
          if (typesRes && typesRes.ok) {
            const t = typesRes.list.find(x => x.id === this.typeId)
            this.typeName = t ? t.name : ''
          }
        }
        const res = await getServiceItems(this.typeId || undefined)
        this.list = (res && res.ok) ? res.list : []
      } catch (e) { this.list = [] }
      this.loading = false
    },
    async add() {
      if (!this.newName.trim()) return uni.showToast({ title: '请输入名称', icon: 'none' })
      if (!this.typeId) return uni.showToast({ title: '缺少类型ID', icon: 'none' })
      try {
        const res = await createServiceItem({ name: this.newName.trim(), serviceTypeId: this.typeId, sortOrder: parseInt(this.newSort) || 0, isEnabled: true })
        if (res && res.ok) { this.newName = ''; this.newSort = '0'; this.load(); uni.showToast({ title: '已添加' }) }
        else uni.showToast({ title: res?.msg || '失败', icon: 'none' })
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
    },
    startEdit(item) {
      this.editId = item.id; this.editName = item.name; this.editSort = String(item.sortOrder); this.editEnabled = item.isEnabled
    },
    async saveEdit(id) {
      try {
        const res = await updateServiceItem(id, { name: this.editName.trim(), serviceTypeId: this.typeId, sortOrder: parseInt(this.editSort) || 0, isEnabled: this.editEnabled })
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
            const res = await deleteServiceItem(id)
            if (res && res.ok) { this.load(); uni.showToast({ title: '已删除' }) }
            else uni.showToast({ title: res?.msg || '失败', icon: 'none' })
          } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
        }
      })
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.type-header { background: #2563eb; color: #fff; border-radius: 12rpx; padding: 24rpx; margin-bottom: 20rpx; }
.type-name { font-size: 32rpx; font-weight: 600; display: block; }
.type-sub { font-size: 24rpx; opacity: 0.8; }
.list { display: flex; flex-direction: column; gap: 16rpx; margin-bottom: 32rpx; }
.card { background: #fff; border-radius: 16rpx; padding: 24rpx; }
.card-main { display: flex; align-items: center; gap: 16rpx; margin-bottom: 8rpx; }
.name { font-weight: 600; font-size: 30rpx; }
.badge { padding: 4rpx 16rpx; border-radius: 8rpx; font-size: 22rpx; }
.sort { font-size: 24rpx; color: #9ca3af; }
.card-actions { display: flex; gap: 24rpx; margin-top: 16rpx; }
.act { font-size: 26rpx; color: #2563eb; }
.act.del { color: #dc2626; }
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
