# Changelog / История изменений

All notable changes to this project will be documented in this file.  
Все важные изменения в этом проекте будут задокументированы в этом файле.

## [0.2.3] - 2025-10-19

### Added / Добавлено
- 🔄 **Future-proof compatibility system** / Система совместимости с будущими версиями
  - Floating version support for CounterStrikeSharp.API / Поддержка плавающих версий для CounterStrikeSharp.API
  - Automatic compatibility with new API releases / Автоматическая совместимость с новыми релизами API
  - Version range management for dependencies / Управление диапазонами версий для зависимостей

- 🛡️ **Enhanced defensive coding** / Улучшенное защитное программирование
  - Try-catch blocks around all critical API calls / Блоки try-catch вокруг всех критичных вызовов API
  - Safe wrappers for admin permission checks / Безопасные обертки для проверки админ-прав
  - Graceful degradation on API changes / Плавная деградация при изменениях API
  - API version logging for troubleshooting / Логирование версии API для диагностики
  - Protected event handlers with error recovery / Защищенные обработчики событий с восстановлением после ошибок
  - Safe player identification with multiple fallbacks / Безопасная идентификация игроков с несколькими fallback механизмами
  
- 📖 **Documentation improvements** / Улучшения документации
  - Added comprehensive COMPATIBILITY.md guide / Добавлено подробное руководство COMPATIBILITY.md
  - Installation and building instructions / Инструкции по установке и сборке
  - API usage documentation / Документация использования API
  - Troubleshooting guide / Руководство по решению проблем
  - Testing checklist for new versions / Чеклист тестирования для новых версий

- 🔧 **Project configuration** / Конфигурация проекта
  - Added .gitignore for cleaner repository / Добавлен .gitignore для более чистого репозитория
  - LangVersion set to 'latest' / LangVersion установлен в 'latest'
  - Platform target set to x64 / Целевая платформа установлена в x64

### Changed / Изменено
- 🔄 **Dependency management** / Управление зависимостями
  - Updated to floating version for CounterStrikeSharp.API (Version="*") / Обновлено до плавающей версии для CounterStrikeSharp.API
  - Removed duplicate DLL reference / Удалена дублирующаяся ссылка на DLL
  - Version ranges for Newtonsoft.Json (13.*) / Диапазоны версий для Newtonsoft.Json
  - Version ranges for Microsoft.Extensions.Localization (8.*) / Диапазоны версий для Microsoft.Extensions.Localization
  - Added PrivateAssets="all" to CounterStrikeSharp.API reference / Добавлен PrivateAssets="all" к ссылке на CounterStrikeSharp.API

### Technical Details / Технические детали
- The plugin now automatically adapts to new CounterStrikeSharp.API versions / Плагин теперь автоматически адаптируется к новым версиям CounterStrikeSharp.API
- No manual version updates required / Не требуется ручное обновление версий
- Better package management with NuGet / Улучшенное управление пакетами через NuGet
- Cleaner project structure / Более чистая структура проекта

---

## [0.2.2] - Previous Version / Предыдущая версия

### Features / Функции
- ✅ Strict blocking mode (blocks ALL except whitelist) / Строгий режим блокировки
- ✅ Auto-logging blocked domains / Автоматическое логирование заблокированных доменов
- ✅ Enhanced name checking at multiple events / Улучшенная проверка имён при множественных событиях
- ✅ Optimized performance with caching / Оптимизированная производительность с кэшированием
- ✅ Asynchronous logging system / Асинхронная система логирования
- ✅ Universal chat handling (say + say_team) / Универсальная обработка чата
- ✅ Smart domain detection (naked domains) / Умное определение доменов
- ✅ Admin immunity support / Поддержка иммунитета админов
- ✅ Safe config updates / Безопасное обновление конфигов

### Core Functionality / Основной функционал
- Blocks URLs, IPs, and domains in chat / Блокирует URL, IP и домены в чате
- Checks player names for blocked content / Проверяет имена игроков на заблокированный контент
- Kick or rename action for banned names / Действие кик или переименование для запрещённых имён
- Whitelist system for allowed content / Система белого списка для разрешённого контента
- Comprehensive logging / Полное логирование
- Color support in messages / Поддержка цветов в сообщениях

---

## Version Compatibility / Совместимость версий

| Plugin Version | CounterStrikeSharp API | .NET Version | Notes |
|----------------|------------------------|--------------|-------|
| 0.2.3+ | Any (floating) | .NET 8.0 | Auto-compatible with new versions |
| 0.2.2 | Any | .NET 8.0 | Manual version management |
| 0.2.x | Any | .NET 8.0 | Initial releases |

---

## How to Update / Как обновить

### From 0.2.2 to 0.2.3 / С 0.2.2 до 0.2.3
1. Pull the latest code / Получите последний код
2. Run `dotnet restore` / Запустите `dotnet restore`
3. Run `dotnet build -c Release` / Запустите `dotnet build -c Release`
4. Replace the plugin DLL on your server / Замените DLL плагина на сервере
5. Restart server or reload plugins / Перезапустите сервер или перезагрузите плагины

The plugin will automatically work with your current CounterStrikeSharp version and future versions.  
Плагин автоматически будет работать с текущей версией CounterStrikeSharp и будущими версиями.

---

## Links / Ссылки
- [Compatibility Guide](COMPATIBILITY.md) - Detailed version compatibility information / Подробная информация о совместимости версий
- [README](README.md) - Main documentation / Основная документация
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) - Core framework / Основной фреймворк

