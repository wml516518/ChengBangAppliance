// 后端 API 根地址。H5 与后端同域可写相对路径；小程序需写完整域名（如 https://yourdomain.com）
// Docker 构建时通过 VITE_BASE_URL 注入，同域部署传空字符串
const BASE_URL = typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.VITE_BASE_URL !== undefined
  ? import.meta.env.VITE_BASE_URL
  : (process.env.NODE_ENV === 'development' ? 'http://localhost:5000' : 'https://yourdomain.com')

export default { BASE_URL }
