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
# 确保不存在 src/ 目录，让 UniApp CLI 使用项目根目录作为源码目录
RUN rm -rf src
RUN npm run build:h5
RUN echo "=== Build output ===" && ls -laR /app/dist/ 2>/dev/null || true
# 统一复制到固定路径（兼容 dist/build/h5、dist/h5 等）
RUN mkdir -p /tmp/h5out && \
  (cp -r /app/dist/build/h5/. /tmp/h5out/ 2>/dev/null) || \
  (cp -r /app/dist/h5/. /tmp/h5out/ 2>/dev/null) || \
  (cp -r /app/build/h5/. /tmp/h5out/ 2>/dev/null) || true
RUN test -f /tmp/h5out/index.html || (echo "H5 build output not found (no index.html)" && ls -laR /app/dist 2>/dev/null; exit 1)

# 阶段2：用 Nginx 提供静态资源
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# 移除默认静态页
RUN rm -rf /usr/share/nginx/html/*

# 从固定路径复制（由上一阶段准备）
COPY --from=build /tmp/h5out ./

# 复制自定义 nginx 配置（由 docker-compose 挂载或下方 default.conf 提供）
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
