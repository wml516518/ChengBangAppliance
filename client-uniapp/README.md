# 诚邦家电 UniApp 客户端

一套代码编译为 **H5（Web）** 与 **微信小程序**，与后端 API 通信（JWT 鉴权）。

## 环境

- Node.js 18+
- 微信开发者工具（仅编译小程序时需要）
- 后端需先启动并开启 CORS（见主项目）

## 配置

1. **接口地址**：编辑 `config.js` 中的 `BASE_URL`。
   - 开发时：`http://localhost:5000`（与后端一致）
   - 发布 H5：填写实际域名，如 `https://yourdomain.com`
   - 小程序：必须为 **https** 且在小程序后台配置合法域名

2. **小程序 appid**：在 `manifest.json` → `mp-weixin.appid` 填写你的微信小程序 appid。

## 使用 HBuilderX（推荐）

1. 用 [HBuilderX](https://www.dcloud.io/hbuilderx.html) 打开本目录。
2. 运行 → 运行到浏览器 → 选择 Chrome（H5）。
3. 运行 → 运行到小程序模拟器 → 微信开发者工具（需先安装并打开服务端口）。

## 使用 CLI

```bash
# 安装依赖
npm install

# H5 开发
npm run dev:h5

# H5 打包
npm run build:h5

# 微信小程序开发（需先安装 uni-app CLI）
npm run dev:mp-weixin

# 微信小程序打包
npm run build:mp-weixin
```

### 若 npm install 报错（如 ETARGET 找不到版本）

先用官方脚手架新建一个 Vue3 + Vite 的 uni-app 项目，再把本项目的页面和配置拷进去：

```bash
# 在上一级目录执行，生成新的 uni-app 空项目
npx create uni-app@latest

# 按提示选择：默认模板 → Vue 3 → Vite
# 假设生成到 client-uniapp-new，进入并安装
cd client-uniapp-new
npm install

# 把当前 client-uniapp 里的这些覆盖到新项目里：
# - pages/  整目录
# - api/    整目录
# - config.js
# - App.vue
# - pages.json
# - manifest.json（可只合并 mp-weixin、h5 等配置）
# - index.html
# - vite.config.js
```

然后在新项目里执行 `npm run dev:h5` 即可。

更推荐直接用 **HBuilderX** 打开本目录，用其自带的运行/发行，可避免本地 @dcloudio 版本问题。

## 页面说明

| 页面     | 路径                    | 说明           |
|----------|-------------------------|----------------|
| 首页     | pages/index/index       | 分类 + 商品列表 |
| 商品详情 | pages/detail/detail     | 数量 + 立即下单 |
| 确认订单 | pages/checkout/checkout | 联系人、支付方式、提交 |
| 下单成功 | pages/order-result/order-result | 订单号、查看订单 |
| 我的订单 | pages/my-orders/my-orders | 列表、删除     |
| 订单详情 | pages/order-detail/order-detail | 明细     |
| 登录     | pages/login/login      | 获取 token     |
| 注册     | pages/register/register | 新用户注册     |

## 后端 API 约定

- 登录：`POST /api/Auth/login` → 返回 `{ ok, token, userName }`
- 所有需登录接口在请求头带 `Authorization: Bearer <token>`
- 分类：`GET /api/Client/categories`
- 商品列表：`GET /api/Client/products?category=xxx`
- 商品详情：`GET /api/Client/products/:id`
- 提交订单：`POST /api/Order/submit`
- 我的订单：`GET /api/Order/list`
- 订单详情：`GET /api/Order/:id`
- 删除订单：`DELETE /api/Order/:id`
