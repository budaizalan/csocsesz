FROM node:22-alpine

WORKDIR /src

COPY package.json pnpm-lock.yaml* ./

RUN npm install --omit=dev

COPY . .

EXPOSE 5000

CMD ["node", "src/app.js"]