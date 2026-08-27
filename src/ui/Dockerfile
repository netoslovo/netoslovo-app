FROM node:24-alpine AS build

WORKDIR /app

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

RUN corepack enable \
    && pnpm install --frozen-lockfile

COPY . .

ARG VITE_API_URL
RUN pnpm build


FROM nginx:1.30-alpine

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]