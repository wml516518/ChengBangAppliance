<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else-if="!order" class="tip">工单不存在</view>
    <view v-else>
      <view class="status-bar">
        <text class="order-no">{{ order.orderNo }}</text>
        <text :class="['status', 'status-' + order.status]">{{ statusText(order.status) }}</text>
      </view>

      <view class="form">
        <view class="section-title">服务信息</view>
        <view class="item">
          <text class="label">订单类型</text>
          <picker :range="typeNames" :value="typeIndex" @change="onTypeChange">
            <view class="picker-box">{{ selectedTypeName || '请选择' }}</view>
          </picker>
        </view>
        <view class="item">
          <text class="label">服务项目</text>
          <picker :range="itemNames" :value="itemIndex" @change="onItemChange">
            <view class="picker-box">{{ selectedItemName || '请选择' }}</view>
          </picker>
        </view>
        <view class="item">
          <text class="label">保修类型</text>
          <view class="radio-row">
            <view :class="['radio-btn', form.warrantyType === 0 ? 'active' : '']" @click="form.warrantyType = 0">包内</view>
            <view :class="['radio-btn', form.warrantyType === 1 ? 'active' : '']" @click="form.warrantyType = 1">保外</view>
          </view>
        </view>
        <view class="item">
          <text class="label">金额（元）</text>
          <!-- #ifdef H5 -->
          <input v-model="form.amount" type="digit" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.amount" @input="form.amount = $event.detail?.value || ''" type="digit" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <text class="label">预约时间段</text>
          <view class="time-row">
            <picker mode="date" :value="form.appointmentStartDate" @change="form.appointmentStartDate = $event.detail.value">
              <view class="picker-box small">{{ form.appointmentStartDate || '开始日期' }}</view>
            </picker>
            <picker mode="time" :value="form.appointmentStartTime" @change="form.appointmentStartTime = $event.detail.value">
              <view class="picker-box small">{{ form.appointmentStartTime || '时间' }}</view>
            </picker>
            <text class="sep">至</text>
            <picker mode="date" :value="form.appointmentEndDate" @change="form.appointmentEndDate = $event.detail.value">
              <view class="picker-box small">{{ form.appointmentEndDate || '结束日期' }}</view>
            </picker>
            <picker mode="time" :value="form.appointmentEndTime" @change="form.appointmentEndTime = $event.detail.value">
              <view class="picker-box small">{{ form.appointmentEndTime || '时间' }}</view>
            </picker>
          </view>
        </view>

        <view class="section-title">联系人信息</view>
        <view class="item">
          <text class="label">联系人</text>
          <!-- #ifdef H5 -->
          <input v-model="form.contactName" type="text" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.contactName" @input="form.contactName = $event.detail?.value || ''" type="text" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <text class="label">联系方式</text>
          <!-- #ifdef H5 -->
          <input v-model="form.contactPhone" type="tel" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.contactPhone" @input="form.contactPhone = $event.detail?.value || ''" type="tel" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <text class="label">所在区域</text>
          <!-- #ifdef H5 -->
          <input v-model="form.area" type="text" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.area" @input="form.area = $event.detail?.value || ''" type="text" class="input" />
          <!-- #endif -->
        </view>
        <view class="item">
          <text class="label">详细地址</text>
          <!-- #ifdef H5 -->
          <input v-model="form.address" type="text" class="input input-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <input :value="form.address" @input="form.address = $event.detail?.value || ''" type="text" class="input" />
          <!-- #endif -->
        </view>

        <view class="section-title">指派与备注</view>
        <view class="item">
          <text class="label">指派师傅</text>
          <picker :range="workerNames" :value="workerIndex" @change="onWorkerChange">
            <view class="picker-box">{{ selectedWorkerName || '暂不指派' }}</view>
          </picker>
        </view>
        <view class="item">
          <text class="label">备注</text>
          <!-- #ifdef H5 -->
          <textarea v-model="form.remark" class="textarea textarea-native" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <textarea :value="form.remark" @input="form.remark = $event.detail?.value || ''" class="textarea" />
          <!-- #endif -->
        </view>
      </view>

      <button class="btn primary" :loading="saving" @click="save">保存修改</button>

      <view class="action-row">
        <button v-if="order.status === 1" class="btn-sm btn-blue" @click="setStatus(2)">开始处理</button>
        <button v-if="order.status === 1 || order.status === 2" class="btn-sm btn-green" @click="setStatus(3)">完成</button>
        <button v-if="order.status < 3" class="btn-sm btn-gray" @click="setStatus(4)">取消工单</button>
        <button class="btn-sm btn-red" @click="del">删除</button>
      </view>

      <view class="meta">
        <text>创建人：{{ order.creatorName }}</text>
        <text>创建时间：{{ formatTime(order.createTime) }}</text>
        <text v-if="order.assignTime">指派时间：{{ formatTime(order.assignTime) }}</text>
        <text v-if="order.completeTime">完成时间：{{ formatTime(order.completeTime) }}</text>
      </view>
    </view>
  </view>
</template>
<script>
import { getManualOrder, updateManualOrder, updateManualOrderStatus, deleteManualOrder, getServiceTypes, getServiceItemsByType, getWorkers } from '../../api/admin.js'

export default {
  data() {
    return {
      orderId: 0,
      order: null,
      loading: true,
      saving: false,
      types: [],
      items: [],
      workers: [],
      form: {
        serviceTypeId: 0, serviceItemId: 0, warrantyType: 0,
        amount: '', appointmentStartDate: '', appointmentStartTime: '',
        appointmentEndDate: '', appointmentEndTime: '',
        contactName: '', contactPhone: '', area: '', address: '', remark: '',
        assignedUserId: null
      }
    }
  },
  computed: {
    typeNames() { return this.types.map(t => t.name) },
    itemNames() { return this.items.map(i => i.name) },
    workerNames() { return ['暂不指派', ...this.workers.map(w => w.name + (w.phone ? ` (${w.phone})` : ''))] },
    typeIndex() { return Math.max(0, this.types.findIndex(t => t.id === this.form.serviceTypeId)) },
    itemIndex() { return Math.max(0, this.items.findIndex(i => i.id === this.form.serviceItemId)) },
    workerIndex() {
      if (!this.form.assignedUserId) return 0
      const idx = this.workers.findIndex(w => w.id === this.form.assignedUserId)
      return idx >= 0 ? idx + 1 : 0
    },
    selectedTypeName() { const t = this.types.find(x => x.id === this.form.serviceTypeId); return t?.name || '' },
    selectedItemName() { const i = this.items.find(x => x.id === this.form.serviceItemId); return i?.name || '' },
    selectedWorkerName() {
      if (!this.form.assignedUserId) return ''
      const w = this.workers.find(x => x.id === this.form.assignedUserId)
      return w?.name || ''
    }
  },
  onLoad(op) {
    this.orderId = parseInt(op.id)
    this.loadAll()
  },
  methods: {
    async loadAll() {
      this.loading = true
      await Promise.all([this.loadOrder(), this.loadTypes(), this.loadWorkers()])
      this.loading = false
    },
    async loadOrder() {
      try {
        const res = await getManualOrder(this.orderId)
        if (res && res.ok) {
          this.order = res.order
          this.form.serviceTypeId = res.order.serviceTypeId
          this.form.serviceItemId = res.order.serviceItemId
          this.form.warrantyType = res.order.warrantyType
          this.form.amount = String(res.order.amount || '')
          this.form.contactName = res.order.contactName || ''
          this.form.contactPhone = res.order.contactPhone || ''
          this.form.area = res.order.area || ''
          this.form.address = res.order.address || ''
          this.form.remark = res.order.remark || ''
          this.form.assignedUserId = res.order.assignedUserId || null
          this.parseDatetime(res.order.appointmentStart, 'Start')
          this.parseDatetime(res.order.appointmentEnd, 'End')
          if (res.order.serviceTypeId) this.loadItems(res.order.serviceTypeId)
        }
      } catch (e) { this.order = null }
    },
    parseDatetime(val, suffix) {
      if (!val) return
      const d = new Date(val)
      if (isNaN(d.getTime())) return
      this.form['appointmentDate' !== undefined ? 'appointment' + suffix + 'Date' : ''] =
        d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0')
      this.form['appointment' + suffix + 'Time'] =
        String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
      this.form['appointment' + suffix + 'Date'] =
        d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0')
    },
    async loadTypes() {
      try {
        const res = await getServiceTypes()
        this.types = (res && res.ok) ? (res.list || []) : []
      } catch (e) { this.types = [] }
    },
    async loadItems(typeId) {
      try {
        const res = await getServiceItemsByType(typeId)
        this.items = Array.isArray(res) ? res : (res?.list || [])
      } catch (e) { this.items = [] }
    },
    async loadWorkers() {
      try {
        const res = await getWorkers()
        this.workers = Array.isArray(res) ? res : (res?.list || [])
      } catch (e) { this.workers = [] }
    },
    onTypeChange(e) {
      const t = this.types[e.detail.value]
      if (t) { this.form.serviceTypeId = t.id; this.form.serviceItemId = 0; this.loadItems(t.id) }
    },
    onItemChange(e) {
      const i = this.items[e.detail.value]
      if (i) this.form.serviceItemId = i.id
    },
    onWorkerChange(e) {
      const idx = parseInt(e.detail.value)
      this.form.assignedUserId = idx === 0 ? null : (this.workers[idx - 1]?.id || null)
    },
    buildDateTime(date, time) {
      if (!date) return null
      return date + 'T' + (time || '00:00') + ':00'
    },
    async save() {
      this.saving = true
      try {
        const data = {
          serviceTypeId: this.form.serviceTypeId,
          serviceItemId: this.form.serviceItemId,
          warrantyType: this.form.warrantyType,
          amount: parseFloat(this.form.amount) || 0,
          appointmentStart: this.buildDateTime(this.form.appointmentStartDate, this.form.appointmentStartTime),
          appointmentEnd: this.buildDateTime(this.form.appointmentEndDate, this.form.appointmentEndTime),
          contactName: this.form.contactName, contactPhone: this.form.contactPhone,
          area: this.form.area, address: this.form.address, remark: this.form.remark,
          assignedUserId: this.form.assignedUserId
        }
        const res = await updateManualOrder(this.orderId, data)
        if (res && res.ok) {
          uni.showToast({ title: '已保存' })
          this.loadOrder()
        } else {
          uni.showToast({ title: res?.msg || '保存失败', icon: 'none' })
        }
      } catch (e) { uni.showToast({ title: '网络错误', icon: 'none' }) }
      this.saving = false
    },
    async setStatus(status) {
      const names = { 2: '开始处理', 3: '完成', 4: '取消' }
      uni.showModal({
        title: '确认', content: `确定将工单标记为「${names[status]}」？`,
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await updateManualOrderStatus(this.orderId, status)
            if (res && res.ok) { uni.showToast({ title: '已更新' }); this.loadOrder() }
          } catch (e) { uni.showToast({ title: '操作失败', icon: 'none' }) }
        }
      })
    },
    del() {
      uni.showModal({
        title: '确认删除', content: '确定删除该工单？不可恢复！',
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await deleteManualOrder(this.orderId)
            if (res && res.ok) { uni.showToast({ title: '已删除' }); setTimeout(() => uni.navigateBack(), 500) }
          } catch (e) { uni.showToast({ title: '删除失败', icon: 'none' }) }
        }
      })
    },
    statusText(s) { return { 0: '待指派', 1: '已指派', 2: '进行中', 3: '已完成', 4: '已取消' }[s] || '' },
    formatTime(t) {
      if (!t) return ''
      const d = new Date(t)
      return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0') + ' ' + String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
.tip { text-align: center; padding: 80rpx; color: #6b7280; }
.status-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20rpx; }
.order-no { font-size: 24rpx; color: #6b7280; font-family: monospace; }
.status { font-size: 24rpx; padding: 6rpx 20rpx; border-radius: 8rpx; }
.status-0 { background: #fef3c7; color: #92400e; }
.status-1 { background: #dbeafe; color: #1e40af; }
.status-2 { background: #bfdbfe; color: #1e40af; }
.status-3 { background: #dcfce7; color: #166534; }
.status-4 { background: #e5e7eb; color: #6b7280; }
.form { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 24rpx; }
.section-title { font-size: 30rpx; font-weight: 600; color: #374151; margin: 24rpx 0 12rpx; padding-bottom: 8rpx; border-bottom: 1rpx solid #e5e7eb; }
.section-title:first-child { margin-top: 0; }
.item { margin-bottom: 24rpx; }
.label { display: block; margin-bottom: 8rpx; font-weight: 500; font-size: 28rpx; }
.input { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.textarea { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; min-height: 120rpx; box-sizing: border-box; }
.textarea-native { display: block; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.picker-box { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; color: #374151; }
.picker-box.small { padding: 16rpx 8rpx; font-size: 24rpx; flex: 1; text-align: center; }
.radio-row { display: flex; gap: 24rpx; }
.radio-btn { padding: 16rpx 40rpx; border: 2rpx solid #e5e7eb; border-radius: 12rpx; font-size: 28rpx; }
.radio-btn.active { border-color: #2563eb; background: #eff6ff; color: #2563eb; }
.time-row { display: flex; align-items: center; gap: 8rpx; }
.sep { color: #6b7280; font-size: 26rpx; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; }
.primary { background: #2563eb; color: #fff; }
.action-row { display: flex; flex-wrap: wrap; gap: 16rpx; margin-top: 24rpx; }
.btn-sm { flex: 1; min-width: 30%; height: 72rpx; line-height: 72rpx; border-radius: 10rpx; font-size: 28rpx; }
.btn-blue { background: #2563eb; color: #fff; }
.btn-green { background: #16a34a; color: #fff; }
.btn-gray { background: #6b7280; color: #fff; }
.btn-red { background: #dc2626; color: #fff; }
.meta { margin-top: 32rpx; background: #f9fafb; border-radius: 12rpx; padding: 20rpx; }
.meta text { display: block; font-size: 24rpx; color: #9ca3af; line-height: 1.8; }
</style>
