<template>
  <view class="page">
    <view class="form">
      <view class="section-title">服务信息</view>
      <view class="item">
        <text class="label">订单类型 *</text>
        <picker :range="typeNames" @change="onTypeChange">
          <view class="picker-box">{{ selectedTypeName || '请选择订单类型' }}</view>
        </picker>
      </view>
      <view class="item">
        <text class="label">服务项目 *</text>
        <picker :range="itemNames" @change="onItemChange">
          <view class="picker-box">{{ selectedItemName || '请选择服务项目' }}</view>
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
        <input v-model="form.amount" type="digit" placeholder="请输入金额" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.amount" @input="form.amount = $event.detail?.value || ''" type="digit" placeholder="请输入金额" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">预约时间段</text>
        <view class="time-row">
          <picker mode="date" @change="form.appointmentStartDate = $event.detail.value">
            <view class="picker-box small">{{ form.appointmentStartDate || '开始日期' }}</view>
          </picker>
          <picker mode="time" @change="form.appointmentStartTime = $event.detail.value">
            <view class="picker-box small">{{ form.appointmentStartTime || '时间' }}</view>
          </picker>
          <text class="sep">至</text>
          <picker mode="date" @change="form.appointmentEndDate = $event.detail.value">
            <view class="picker-box small">{{ form.appointmentEndDate || '结束日期' }}</view>
          </picker>
          <picker mode="time" @change="form.appointmentEndTime = $event.detail.value">
            <view class="picker-box small">{{ form.appointmentEndTime || '时间' }}</view>
          </picker>
        </view>
      </view>

      <view class="section-title">联系人信息</view>
      <view class="item">
        <text class="label">联系人 *</text>
        <!-- #ifdef H5 -->
        <input v-model="form.contactName" type="text" placeholder="请输入联系人" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.contactName" @input="form.contactName = $event.detail?.value || ''" type="text" placeholder="请输入联系人" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">联系方式 *</text>
        <!-- #ifdef H5 -->
        <input v-model="form.contactPhone" type="tel" placeholder="请输入手机号" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.contactPhone" @input="form.contactPhone = $event.detail?.value || ''" type="tel" placeholder="请输入手机号" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">所在区域</text>
        <!-- #ifdef H5 -->
        <input v-model="form.area" type="text" placeholder="如：XX市XX区" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.area" @input="form.area = $event.detail?.value || ''" type="text" placeholder="如：XX市XX区" class="input" />
        <!-- #endif -->
      </view>
      <view class="item">
        <text class="label">详细地址</text>
        <!-- #ifdef H5 -->
        <input v-model="form.address" type="text" placeholder="请输入详细地址" class="input input-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <input :value="form.address" @input="form.address = $event.detail?.value || ''" type="text" placeholder="请输入详细地址" class="input" />
        <!-- #endif -->
      </view>

      <view class="section-title">指派与备注</view>
      <view class="item">
        <text class="label">指派师傅</text>
        <picker :range="workerNames" @change="onWorkerChange">
          <view class="picker-box">{{ selectedWorkerName || '暂不指派' }}</view>
        </picker>
      </view>
      <view class="item">
        <text class="label">备注</text>
        <!-- #ifdef H5 -->
        <textarea v-model="form.remark" placeholder="选填" class="textarea textarea-native" />
        <!-- #endif -->
        <!-- #ifndef H5 -->
        <textarea :value="form.remark" @input="form.remark = $event.detail?.value || ''" placeholder="选填" class="textarea" />
        <!-- #endif -->
      </view>
    </view>
    <button class="btn primary" :loading="submitting" @click="submit">提交工单</button>
  </view>
</template>
<script>
import { getServiceTypes, getServiceItemsByType, getWorkers, createManualOrder } from '../../api/admin.js'

export default {
  data() {
    return {
      types: [],
      items: [],
      workers: [],
      form: {
        serviceTypeId: 0,
        serviceItemId: 0,
        warrantyType: 0,
        amount: '',
        appointmentStartDate: '',
        appointmentStartTime: '',
        appointmentEndDate: '',
        appointmentEndTime: '',
        contactName: '',
        contactPhone: '',
        area: '',
        address: '',
        remark: '',
        assignedUserId: null
      },
      submitting: false
    }
  },
  computed: {
    typeNames() { return this.types.map(t => t.name) },
    itemNames() { return this.items.map(i => i.name) },
    workerNames() { return ['暂不指派', ...this.workers.map(w => w.name + (w.phone ? ` (${w.phone})` : ''))] },
    selectedTypeName() {
      const t = this.types.find(x => x.id === this.form.serviceTypeId)
      return t ? t.name : ''
    },
    selectedItemName() {
      const i = this.items.find(x => x.id === this.form.serviceItemId)
      return i ? i.name : ''
    },
    selectedWorkerName() {
      if (!this.form.assignedUserId) return ''
      const w = this.workers.find(x => x.id === this.form.assignedUserId)
      return w ? w.name : ''
    }
  },
  onLoad() {
    this.loadTypes()
    this.loadWorkers()
  },
  methods: {
    async loadTypes() {
      try {
        const res = await getServiceTypes()
        if (res && res.ok) {
          this.types = (res.list || []).filter(t => t.isEnabled)
        } else {
          this.types = []
          uni.showToast({ title: res?.msg || '加载订单类型失败', icon: 'none' })
        }
      } catch (e) {
        this.types = []
        uni.showToast({ title: '加载订单类型失败', icon: 'none' })
      }
    },
    async loadItems(typeId) {
      try {
        const res = await getServiceItemsByType(typeId)
        this.items = Array.isArray(res) ? res : (res?.list || [])
      } catch (e) {
        this.items = []
        uni.showToast({ title: '加载服务项目失败', icon: 'none' })
      }
    },
    async loadWorkers() {
      try {
        const res = await getWorkers()
        this.workers = Array.isArray(res) ? res : (res?.list || [])
      } catch (e) {
        this.workers = []
        uni.showToast({ title: '加载师傅列表失败', icon: 'none' })
      }
    },
    onTypeChange(e) {
      const idx = e.detail.value
      const t = this.types[idx]
      if (t) {
        this.form.serviceTypeId = t.id
        this.form.serviceItemId = 0
        this.loadItems(t.id)
      }
    },
    onItemChange(e) {
      const idx = e.detail.value
      const i = this.items[idx]
      if (i) this.form.serviceItemId = i.id
    },
    onWorkerChange(e) {
      const idx = parseInt(e.detail.value)
      if (idx === 0) {
        this.form.assignedUserId = null
      } else {
        this.form.assignedUserId = this.workers[idx - 1]?.id || null
      }
    },
    buildDateTime(date, time) {
      if (!date) return null
      return date + 'T' + (time || '00:00') + ':00'
    },
    async submit() {
      if (!this.form.serviceTypeId) return uni.showToast({ title: '请选择订单类型', icon: 'none' })
      if (!this.form.serviceItemId) return uni.showToast({ title: '请选择服务项目', icon: 'none' })
      if (!this.form.contactName.trim()) return uni.showToast({ title: '请填写联系人', icon: 'none' })
      if (!this.form.contactPhone.trim()) return uni.showToast({ title: '请填写联系方式', icon: 'none' })

      this.submitting = true
      try {
        const data = {
          serviceTypeId: this.form.serviceTypeId,
          serviceItemId: this.form.serviceItemId,
          warrantyType: this.form.warrantyType,
          amount: parseFloat(this.form.amount) || 0,
          appointmentStart: this.buildDateTime(this.form.appointmentStartDate, this.form.appointmentStartTime),
          appointmentEnd: this.buildDateTime(this.form.appointmentEndDate, this.form.appointmentEndTime),
          contactName: this.form.contactName.trim(),
          contactPhone: this.form.contactPhone.trim(),
          area: this.form.area.trim(),
          address: this.form.address.trim(),
          remark: this.form.remark.trim(),
          assignedUserId: this.form.assignedUserId
        }
        const res = await createManualOrder(data)
        if (res && res.ok) {
          uni.showToast({ title: '工单已创建' })
          setTimeout(() => uni.navigateBack(), 500)
        } else {
          uni.showToast({ title: res?.msg || '创建失败', icon: 'none' })
        }
      } catch (e) {
        uni.showToast({ title: '网络错误', icon: 'none' })
      }
      this.submitting = false
    }
  }
}
</script>
<style scoped>
.page { padding: 24rpx; padding-bottom: 120rpx; }
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
.picker-box.small { padding: 16rpx 12rpx; font-size: 24rpx; flex: 1; text-align: center; }
.radio-row { display: flex; gap: 24rpx; }
.radio-btn { padding: 16rpx 40rpx; border: 2rpx solid #e5e7eb; border-radius: 12rpx; font-size: 28rpx; }
.radio-btn.active { border-color: #2563eb; background: #eff6ff; color: #2563eb; }
.time-row { display: flex; align-items: center; gap: 8rpx; }
.sep { color: #6b7280; font-size: 26rpx; }
.btn { width: 100%; height: 88rpx; line-height: 88rpx; border-radius: 12rpx; font-size: 32rpx; }
.primary { background: #2563eb; color: #fff; }
</style>
