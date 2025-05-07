FROM node:23.11.0-alpine

WORKDIR /app

EXPOSE 3010
ENV CHOKIDAR_USEPOLLING=true

CMD sh -c "npm install && npm run dev -- --host"
