#!/bin/bash
# 诚邦家电 - 一键拉取代码并 Docker 部署
# 用法: bash deploy.sh  或  chmod +x deploy.sh && ./deploy.sh
set -e

# ========== 配置（部署前请修改） ==========
# GitHub 仓库地址（HTTPS 或 SSH）
GITHUB_REPO="${GITHUB_REPO:-https://github.com/wml516518/ChengBangAppliance.git}"
# 分支
BRANCH="${BRANCH:-main}"
# 服务器上项目存放目录（将在此目录执行 git pull / clone 和 docker compose）
PROJECT_DIR="${PROJECT_DIR:-/opt/chengbang-app}"

# ========== 以下无需修改 ==========
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() { echo -e "${GREEN}[INFO]${NC} $1"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
err() { echo -e "${RED}[ERROR]${NC} $1"; }

# 检查 Docker
if ! command -v docker &>/dev/null; then
  err "未安装 Docker，请先安装: https://docs.docker.com/engine/install/"
  exit 1
fi
if ! docker compose version &>/dev/null && ! docker-compose version &>/dev/null; then
  err "未找到 docker compose，请安装 Docker Compose V2"
  exit 1
fi
COMPOSE_CMD="docker compose"
if ! docker compose version &>/dev/null; then
  COMPOSE_CMD="docker-compose"
fi

# 拉取或克隆代码
if [ -d "$PROJECT_DIR/.git" ]; then
  log "项目目录已存在，执行 git pull..."
  cd "$PROJECT_DIR"
  git fetch origin
  git checkout -q "$BRANCH" 2>/dev/null || true
  git pull origin "$BRANCH"
else
  log "首次部署，克隆仓库到 $PROJECT_DIR ..."
  sudo mkdir -p "$(dirname "$PROJECT_DIR")"
  sudo chown -R "$(whoami):$(whoami)" "$(dirname "$PROJECT_DIR")" 2>/dev/null || true
  if [ -d "$PROJECT_DIR" ] && [ -z "$(ls -A "$PROJECT_DIR" 2>/dev/null)" ]; then
    rmdir "$PROJECT_DIR" 2>/dev/null || true
  fi
  if [ ! -d "$PROJECT_DIR" ]; then
    git clone -b "$BRANCH" "$GITHUB_REPO" "$PROJECT_DIR"
  else
    log "目录已存在且非空，尝试进入并 pull..."
    cd "$PROJECT_DIR"
    git pull origin "$BRANCH" || true
  fi
  cd "$PROJECT_DIR"
fi

cd "$PROJECT_DIR"
if [ ! -f "docker-compose.yml" ]; then
  err "未找到 docker-compose.yml，请确认仓库根目录或 PROJECT_DIR 是否正确"
  exit 1
fi

# 若无 .env 则从示例复制
if [ ! -f ".env" ]; then
  if [ -f "docker-compose.env.example" ]; then
    log "未找到 .env，已从 docker-compose.env.example 复制，请按需修改 .env 中的 JWT_KEY 等"
    cp docker-compose.env.example .env
  fi
fi

# 供下方日志显示端口（compose 会自行读取 .env）
if [ -f ".env" ] && grep -q '^HTTP_PORT=' .env 2>/dev/null; then
  HTTP_PORT=$(grep '^HTTP_PORT=' .env | head -1 | cut -d= -f2- | tr -d '\r')
else
  HTTP_PORT=80
fi

# 构建并启动
log "执行 Docker 构建并启动..."
$COMPOSE_CMD up -d --build

log "部署完成。访问: http://$(hostname -I 2>/dev/null | awk '{print $1}'):${HTTP_PORT}"
log "查看日志: cd $PROJECT_DIR && $COMPOSE_CMD logs -f"
