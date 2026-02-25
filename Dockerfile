# 诚邦家电 - 后端 API (ASP.NET Core 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件并还原
COPY ApplianceRepair.csproj ./
RUN dotnet restore

# 复制源码并发布
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 创建数据目录（SQLite 等）
RUN mkdir -p /app/data

COPY --from=build /app/publish ./
EXPOSE 8080

# 使用 8080 便于容器内运行（生产可用环境变量覆盖）
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ApplianceRepair.dll"]
