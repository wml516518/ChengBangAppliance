# 诚邦家电 - 前端 H5 (UniApp + Vite)
# 阶段1：构建
FROM node:20-alpine AS build
WORKDIR /app

# 复制依赖定义
COPY package.json package-lock.json* ./

# 使用国内镜像加速（可选，部署到国内服务器时取消注释下一行）
# RUN npm config set registry https://registry.npmmirror.com

# 先装依赖（无 lock 时用 install，避免 ci 报错）
RUN npm install --legacy-peer-deps

COPY . ./

# 同域部署时 API 使用相对路径；可构建时传入: --build-arg VITE_BASE_URL=https://api.example.com
ARG VITE_BASE_URL=
ENV VITE_BASE_URL=${VITE_BASE_URL}
# 抑制 Vite CJS Node API 弃用警告（仅影响构建日志）
ENV NODE_OPTIONS="--no-deprecation"
# UniApp CLI 期望源码在 src/ 下（manifest.json 等）
RUN mkdir -p src && \
  cp manifest.json pages.json App.vue main.js index.html config.js src/ && \
  cp -r api pages utils src/
RUN npm run build:h5

# 阶段2：用 Nginx 提供静态资源
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# 移除默认静态页
RUN rm -rf /usr/share/nginx/html/*

# UniApp Vite H5 默认输出目录
COPY --from=build /app/dist/build/h5 ./

# 复制自定义 nginx 配置（由 docker-compose 挂载或下方 default.conf 提供）
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
