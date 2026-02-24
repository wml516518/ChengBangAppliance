<template>
  <view class="page">
    <view v-if="loading" class="tip">加载中...</view>
    <view v-else-if="!order" class="tip">工单不存在</view>
    <view v-else>
      <view class="status-bar">
        <text class="order-no">{{ order.orderNo }}</text>
        <text :class="['status', 'status-' + order.status]">{{ statusText(order.status) }}</text>
      </view>

      <!-- 基本信息（只读） -->
      <view class="section">
        <view class="section-title">服务信息</view>
        <view class="info-row"><text class="info-label">订单类型</text><text class="info-val">{{ order.typeName }}</text></view>
        <view class="info-row"><text class="info-label">服务项目</text><text class="info-val">{{ order.itemName }}</text></view>
        <view class="info-row"><text class="info-label">保修类型</text><text class="info-val">{{ order.warrantyType === 0 ? '包内' : '保外' }}</text></view>
      </view>

      <view class="section">
        <view class="section-title">联系人</view>
        <view class="info-row"><text class="info-label">姓名</text><text class="info-val">{{ order.contactName }}</text></view>
        <view class="info-row"><text class="info-label">电话</text><text class="info-val link" @click="callPhone">{{ order.contactPhone }}</text></view>
        <view class="info-row" v-if="order.area"><text class="info-label">区域</text><text class="info-val">{{ order.area }}</text></view>
        <view class="info-row" v-if="order.address"><text class="info-label">地址</text><text class="info-val">{{ order.address }}</text></view>
      </view>

      <!-- 可编辑区域 -->
      <view class="section">
        <view class="section-title">可编辑信息</view>
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
            <picker mode="date" :value="form.startDate" @change="form.startDate = $event.detail.value">
              <view class="picker-box small">{{ form.startDate || '开始日期' }}</view>
            </picker>
            <picker mode="time" :value="form.startTime" @change="form.startTime = $event.detail.value">
              <view class="picker-box small">{{ form.startTime || '时间' }}</view>
            </picker>
            <text class="sep">至</text>
            <picker mode="date" :value="form.endDate" @change="form.endDate = $event.detail.value">
              <view class="picker-box small">{{ form.endDate || '结束日期' }}</view>
            </picker>
            <picker mode="time" :value="form.endTime" @change="form.endTime = $event.detail.value">
              <view class="picker-box small">{{ form.endTime || '时间' }}</view>
            </picker>
          </view>
        </view>
        <view class="item">
          <text class="label">备注</text>
          <!-- #ifdef H5 -->
          <textarea v-model="form.remark" class="textarea textarea-native" placeholder="填写备注" />
          <!-- #endif -->
          <!-- #ifndef H5 -->
          <textarea :value="form.remark" @input="form.remark = $event.detail?.value || ''" class="textarea" placeholder="填写备注" />
          <!-- #endif -->
        </view>
        <button class="btn btn-save" :loading="saving" @click="save">保存修改</button>
      </view>

      <!-- 照片 -->
      <view class="section">
        <view class="section-title">现场照片</view>
        <view class="photo-grid">
          <view v-for="(p, idx) in photos" :key="idx" class="photo-item">
            <image :src="baseUrl + p" class="photo-img" mode="aspectFill" @click="previewPhoto(idx)" />
            <text class="photo-del" @click="delPhoto(p)">×</text>
          </view>
          <view class="photo-add" @click="choosePhoto">
            <text class="photo-add-icon">+</text>
            <text class="photo-add-text">添加照片</text>
          </view>
        </view>
      </view>

      <!-- 状态操作 -->
      <view class="action-row" v-if="order.status < 3">
        <button v-if="order.status === 1" class="btn btn-start" @click="setStatus(2)">开始处理</button>
        <button v-if="order.status === 1 || order.status === 2" class="btn btn-done" @click="setStatus(3)">完成工单</button>
      </view>

      <view class="meta">
        <text>创建时间：{{ formatTime(order.createTime) }}</text>
        <text v-if="order.assignTime">指派时间：{{ formatTime(order.assignTime) }}</text>
        <text v-if="order.completeTime">完成时间：{{ formatTime(order.completeTime) }}</text>
      </view>
    </view>
  </view>
</template>
<script>
import config from '../../config.js'
import { getMyOrder, updateMyOrder, updateOrderStatus, addPhoto, removePhoto } from '../../api/worker.js'
import { checkLogin } from '../../utils/auth.js'

export default {
  data() {
    return {
      baseUrl: config.BASE_URL,
      orderId: 0,
      order: null,
      photos: [],
      loading: true,
      saving: false,
      form: {
        amount: '',
        remark: '',
        startDate: '', startTime: '',
        endDate: '', endTime: ''
      }
    }
  },
  onLoad(op) {
    if (!checkLogin()) return
    this.orderId = parseInt(op.id)
    this.loadOrder()
  },
  methods: {
    async loadOrder() {
      this.loading = true
      try {
        const res = await getMyOrder(this.orderId)
        if (res && res.ok) {
          this.order = res.order
          this.photos = res.order.photos || []
          this.form.amount = String(res.order.amount || '')
          this.form.remark = res.order.remark || ''
          this.parseDatetime(res.order.appointmentStart, 'start')
          this.parseDatetime(res.order.appointmentEnd, 'end')
        }
      } catch (e) { this.order = null }
      this.loading = false
    },
    parseDatetime(val, prefix) {
      if (!val) return
      const d = new Date(val)
      if (isNaN(d.getTime())) return
      const dateStr = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0')
      const timeStr = String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
      if (prefix === 'start') { this.form.startDate = dateStr; this.form.startTime = timeStr }
      else { this.form.endDate = dateStr; this.form.endTime = timeStr }
    },
    buildDateTime(date, time) {
      if (!date) return null
      return date + 'T' + (time || '00:00') + ':00'
    },
    async save() {
      this.saving = true
      try {
        const data = {
          amount: parseFloat(this.form.amount) || 0,
          remark: this.form.remark,
          appointmentStart: this.buildDateTime(this.form.startDate, this.form.startTime),
          appointmentEnd: this.buildDateTime(this.form.endDate, this.form.endTime)
        }
        const res = await updateMyOrder(this.orderId, data)
        if (res && res.ok) {
          uni.showToast({ title: '已保存' })
          this.loadOrder()
        } else {
          uni.showToast({ title: res?.msg || '保存失败', icon: 'none' })
        }
      } catch (e) {
        uni.showToast({ title: '网络错误', icon: 'none' })
      }
      this.saving = false
    },
    async setStatus(status) {
      const names = { 2: '开始处理', 3: '完成' }
      uni.showModal({
        title: '确认', content: `确定将工单标记为「${names[status]}」？`,
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await updateOrderStatus(this.orderId, status)
            if (res && res.ok) { uni.showToast({ title: '已更新' }); this.loadOrder() }
            else uni.showToast({ title: res?.msg || '操作失败', icon: 'none' })
          } catch (e) { uni.showToast({ title: '操作失败', icon: 'none' }) }
        }
      })
    },
    choosePhoto() {
      uni.chooseImage({
        count: 3,
        sizeType: ['compressed'],
        success: (res) => {
          res.tempFilePaths.forEach(path => this.uploadFile(path))
        }
      })
    },
    uploadFile(filePath) {
      const token = uni.getStorageSync('token')
      uni.uploadFile({
        url: this.baseUrl + '/api/WorkerApi/upload-photo',
        filePath,
        name: 'file',
        header: { Authorization: 'Bearer ' + token },
        success: (uploadRes) => {
          try {
            const data = JSON.parse(uploadRes.data)
            if (data.ok) {
              this.addPhotoToOrder(data.url)
            } else {
              uni.showToast({ title: data.msg || '上传失败', icon: 'none' })
            }
          } catch (e) {
            uni.showToast({ title: '上传失败', icon: 'none' })
          }
        },
        fail: () => {
          uni.showToast({ title: '上传失败', icon: 'none' })
        }
      })
    },
    async addPhotoToOrder(url) {
      try {
        const res = await addPhoto(this.orderId, url)
        if (res && res.ok) {
          this.photos = res.photos || []
        }
      } catch (e) {}
    },
    delPhoto(url) {
      uni.showModal({
        title: '确认', content: '删除这张照片？',
        success: async (r) => {
          if (!r.confirm) return
          try {
            const res = await removePhoto(this.orderId, url)
            if (res && res.ok) this.photos = res.photos || []
          } catch (e) {}
        }
      })
    },
    previewPhoto(idx) {
      uni.previewImage({
        current: idx,
        urls: this.photos.map(p => this.baseUrl + p)
      })
    },
    callPhone() {
      if (this.order?.contactPhone) {
        uni.makePhoneCall({ phoneNumber: this.order.contactPhone })
      }
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
.status-1 { background: #dbeafe; color: #1e40af; }
.status-2 { background: #fef3c7; color: #92400e; }
.status-3 { background: #dcfce7; color: #166534; }
.status-4 { background: #e5e7eb; color: #6b7280; }
.section { background: #fff; border-radius: 16rpx; padding: 24rpx; margin-bottom: 20rpx; }
.section-title { font-size: 30rpx; font-weight: 600; color: #374151; margin-bottom: 16rpx; padding-bottom: 8rpx; border-bottom: 1rpx solid #e5e7eb; }
.info-row { display: flex; justify-content: space-between; padding: 12rpx 0; }
.info-label { color: #6b7280; font-size: 28rpx; }
.info-val { color: #111827; font-size: 28rpx; font-weight: 500; }
.link { color: #2563eb; text-decoration: underline; }
.item { margin-bottom: 20rpx; }
.label { display: block; margin-bottom: 8rpx; font-weight: 500; font-size: 28rpx; }
.input { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; box-sizing: border-box; }
.input-native { display: block; min-height: 44px; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.textarea { border: 1rpx solid #d1d5db; padding: 20rpx; border-radius: 8rpx; width: 100%; min-height: 120rpx; box-sizing: border-box; }
.textarea-native { display: block; cursor: text; pointer-events: auto; -webkit-user-select: text; user-select: text; position: relative; z-index: 1; }
.time-row { display: flex; align-items: center; gap: 8rpx; }
.picker-box { border: 1rpx solid #d1d5db; padding: 16rpx 12rpx; border-radius: 8rpx; color: #374151; }
.picker-box.small { font-size: 24rpx; flex: 1; text-align: center; }
.sep { color: #6b7280; font-size: 26rpx; }
.photo-grid { display: flex; flex-wrap: wrap; gap: 16rpx; }
.photo-item { position: relative; width: 200rpx; height: 200rpx; }
.photo-img { width: 200rpx; height: 200rpx; border-radius: 12rpx; }
.photo-del { position: absolute; top: -10rpx; right: -10rpx; width: 40rpx; height: 40rpx; background: #dc2626; color: #fff; border-radius: 50%; text-align: center; line-height: 40rpx; font-size: 28rpx; }
.photo-add { width: 200rpx; height: 200rpx; border: 2rpx dashed #d1d5db; border-radius: 12rpx; display: flex; flex-direction: column; align-items: center; justify-content: center; }
.photo-add-icon { font-size: 56rpx; color: #9ca3af; }
.photo-add-text { font-size: 22rpx; color: #9ca3af; }
.btn { width: 100%; height: 80rpx; line-height: 80rpx; border-radius: 12rpx; font-size: 30rpx; }
.btn-save { background: #16a34a; color: #fff; margin-top: 16rpx; }
.action-row { display: flex; gap: 20rpx; margin-bottom: 20rpx; }
.action-row .btn { flex: 1; }
.btn-start { background: #2563eb; color: #fff; }
.btn-done { background: #16a34a; color: #fff; }
.meta { background: #f9fafb; border-radius: 12rpx; padding: 20rpx; }
.meta text { display: block; font-size: 24rpx; color: #9ca3af; line-height: 1.8; }
</style>
