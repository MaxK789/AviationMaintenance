<template>
  <div class="app">
    <header class="app-header">
      <h1>Aviation Maintenance Portal</h1>
      <nav>
        <router-link to="/aircraft">Літаки</router-link>
        <router-link to="/workorders">Заявки</router-link>
        <router-link to="/graphql">GraphQL</router-link>
      </nav>
    </header>

    <main class="app-main">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.app {
  font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}
.app-header {
  padding: 12px 24px;
  background: #111827;
  color: #f9fafb;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.app-header h1 {
  font-size: 20px;
  margin: 0;
}
nav {
  display: flex;
  gap: 16px;
}
a {
  color: #e5e7eb;
  text-decoration: none;
}
a.router-link-active {
  font-weight: 600;
  border-bottom: 2px solid #3b82f6;
}
.app-main {
  padding: 16px 24px;
  flex: 1;
  background: #f3f4f6;
}
</style>
