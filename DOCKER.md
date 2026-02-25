# Docker 部署说明

## 结构说明

- **后端**：ASP.NET Core 8，镜像内监听 8080，不直接对外暴露。
- **前端**：UniApp H5 构建后由 Nginx 提供静态资源，对外 80 端口；`/api` 由 Nginx 反向代理到后端。

访问 `http://服务器IP:80` 即可使用整站（前端 + API 同域）。

## 首次部署

1. **（可选）配置环境变量**

   ```bash
   cp docker-compose.env.example .env
   # 编辑 .env，至少修改 JWT_KEY 为随机长字符串
   ```

2. **构建并启动**

   ```bash
   docker compose up -d --build
   ```

3. **查看日志**

   ```bash
   docker compose logs -f
   ```

## 常用命令

```bash
# 停止
docker compose down

# 仅重新构建并启动
docker compose up -d --build

# 只重建前端
docker compose build frontend --no-cache && docker compose up -d frontend

# 只重建后端
docker compose build backend --no-cache && docker compose up -d backend
```

## 数据持久化

- 后端 SQLite 数据库：通过卷 `backend-data` 持久化，位置在容器内 `/app/data/appliance.db`。
- 如需备份：`docker compose run --rm backend cat /app/data/appliance.db > backup.db`（或使用 volume 备份方式）。

## 前端 API 地址

- 当前为**同域部署**：前端构建时 `VITE_BASE_URL` 为空，请求走相对路径 `/api/...`，由 Nginx 转发到后端。
- 若改为前后端不同域名部署，需在构建前端时传入 API 根地址，例如：
  `docker compose build --build-arg VITE_BASE_URL=https://api.yourdomain.com frontend`

## 注意事项

- 生产环境务必在 `.env` 中设置强随机 `JWT_KEY`（建议 32 位以上）。
- 若 UniApp 构建输出目录不是 `dist/build/h5`，需修改 `client-uniapp/Dockerfile` 中 `COPY --from=build` 的路径。
