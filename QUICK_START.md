# Quick Start / Быстрый старт

## 🚀 5-минутное начало работы / 5-Minute Setup

### Шаг 1: Скачать / Step 1: Download
```bash
# Download the latest IPAndSiteBlocker.dll from releases
# Скачайте последний IPAndSiteBlocker.dll из релизов
```

### Шаг 2: Установить / Step 2: Install
```bash
# Copy to your CS2 server
# Скопируйте на ваш CS2 сервер
cp IPAndSiteBlocker.dll csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/
```

### Шаг 3: Перезагрузить / Step 3: Reload
```bash
# In server console / В консоли сервера
css_plugins reload
```

### Шаг 4: Настроить / Step 4: Configure
```bash
# Edit config file / Отредактируйте конфиг
nano csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json
```

### Шаг 5: Готово! / Step 5: Done!
Плагин работает! / Plugin is working!

---

## ⚡ Основные возможности / Key Features

### Что блокирует / What It Blocks
- ❌ URL с протоколами (http://, https://, ftp://)
- ❌ IP адреса (192.168.1.1)
- ❌ Домены (example.com, site.net)
- ❌ Голые домены (site.io, domain.xyz)

### Где блокирует / Where It Blocks
- 💬 Публичный чат / Public chat
- 💬 Командный чат / Team chat
- 👤 Имена игроков / Player names

### Белый список / Whitelist
```json
{
  "whitelist": [
    "yoursite.com",
    "192.168.1.1"
  ]
}
```

---

## 🔧 Базовая настройка / Basic Configuration

```json
{
  "whitelist": [],                    // Разрешённые сайты/IP
  "block_message": "{darkred}Blocked: Sending IP addresses or websites is not allowed.",
  "name_action": 1,                   // 0=кик, 1=переименование
  "rename_message": "{darkred}Your name contains a blocked IP address or website.",
  "admin_immunity": 1,                // 0=выкл, 1=админы с @css/generic имеют иммунитет
  "log_path": "addons/counterstrikesharp/logs/ip_site_blocker.log",
  "blocked_domains_log": "addons/counterstrikesharp/logs/blocked_domains.log",
  "auto_log_blocked": true,           // Логировать заблокированные попытки
  "ConfigVersion": 2
}
```

---

## 🎯 Совместимость / Compatibility

### ✅ Работает с / Works With
- **Любая версия CounterStrikeSharp** / **Any CounterStrikeSharp version**
- Текущие релизы / Current releases
- Будущие обновления / Future updates
- Старые версии / Legacy versions

### 🔄 Не требует / No Need To
- ❌ Обновлять при обновлении CS2 / Update when CS2 updates
- ❌ Обновлять при обновлении CSS / Update when CSS updates
- ❌ Ручных изменений / Manual modifications
- ❌ Специальной настройки / Special configuration

### ⚙️ Требует / Requirements
- ✅ Counter-Strike 2 Server
- ✅ CounterStrikeSharp установлен / CounterStrikeSharp installed
- ✅ .NET 8.0 runtime

---

## 📊 Проверка работы / Verify It Works

### 1. Проверить загрузку / Check Loading
```bash
# In server console / В консоли сервера
css_plugins list

# Should show / Должно показать:
# IPAndSiteBlocker v0.2.3
```

### 2. Проверить логи / Check Logs
```bash
cat csgo/addons/counterstrikesharp/logs/ip_site_blocker.log

# Should see / Должно быть:
# [2025-10-19 20:45:10] IPAndSiteBlocker v0.2.3 loading...
# [2025-10-19 20:45:10] CounterStrikeSharp API: 1.0.XXX
# [2025-10-19 20:45:10] IPAndSiteBlocker loaded successfully!
```

### 3. Протестировать / Test It
- Отправить сообщение с URL → должно блокироваться
- Send message with URL → should be blocked
- Отправить сообщение с IP → должно блокироваться
- Send message with IP → should be blocked

---

## 🛠️ Типичные настройки / Common Configurations

### Строгий режим (блокировать всё) / Strict Mode (block everything)
```json
{
  "whitelist": [],
  "name_action": 0,        // Kick players
  "admin_immunity": 0      // No immunity
}
```

### Мягкий режим (только переименование) / Soft Mode (rename only)
```json
{
  "whitelist": ["yoursite.com"],
  "name_action": 1,        // Rename players
  "admin_immunity": 1      // Admins immune
}
```

### Только чат / Chat Only
```json
{
  "name_action": 1,        // Rename (mild action for names)
  "admin_immunity": 1
}
```

---

## ❓ Частые вопросы / FAQ

### Q: Нужно ли обновлять плагин?
**A:** Нет, плагин автоматически работает с новыми версиями.

### Q: Do I need to update the plugin?
**A:** No, the plugin automatically works with new versions.

---

### Q: Что если не работает?
**A:** 
1. Проверьте логи / Check logs
2. Убедитесь что CounterStrikeSharp установлен / Ensure CSS is installed
3. Перезагрузите сервер / Restart server

### Q: What if it doesn't work?
**A:**
1. Check logs
2. Ensure CounterStrikeSharp is installed
3. Restart server

---

### Q: Как добавить сайт в белый список?
**A:**
```json
{
  "whitelist": [
    "mysite.com",
    "discord.gg",
    "192.168.1.1"
  ]
}
```

### Q: How to whitelist a site?
**A:**
```json
{
  "whitelist": [
    "mysite.com",
    "discord.gg",
    "192.168.1.1"
  ]
}
```

---

## 📚 Больше информации / More Information

Для детальной информации смотрите / For detailed information see:

1. **[VERSION_COMPATIBILITY.md](VERSION_COMPATIBILITY.md)** - Совместимость версий / Version compatibility
2. **[COMPATIBILITY.md](COMPATIBILITY.md)** - Полное руководство / Full guide
3. **[README.md](README.md)** - Основная документация / Main documentation
4. **[CHANGELOG.md](CHANGELOG.md)** - История изменений / Change history

---

## 🎉 Готово! / You're Done!

Плагин установлен и работает!  
Plugin is installed and working!

**Поддержка / Support:**
- GitHub Issues
- Discord
- Email

**Автор / Author:** PattHs and Luxecs2.ru  
**Версия / Version:** 0.2.3+  
**Лицензия / License:** Open Source

