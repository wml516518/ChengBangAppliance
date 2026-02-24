// 后端 API 根地址。H5 与后端同域可写相对路径；小程序需写完整域名（如 https://yourdomain.com）
const BASE_URL = process.env.NODE_ENV === 'development'
  ? 'http://localhost:5000'
  : 'https://yourdomain.com'  // 发布时改为你的服务器地址

export default { BASE_URL }
