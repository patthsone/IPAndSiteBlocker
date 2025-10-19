# Upgrade Guide / Руководство по обновлению

## Upgrading to v0.2.3 (Future-Proof Version) / Обновление до v0.2.3 (версия с поддержкой будущих релизов)

Version 0.2.3 introduces automatic compatibility with new CounterStrikeSharp.API versions.  
Версия 0.2.3 вводит автоматическую совместимость с новыми версиями CounterStrikeSharp.API.

### What Changed / Что изменилось

#### ✅ New Features / Новые функции
- **Automatic API Compatibility** / Автоматическая совместимость API
  - Plugin now uses floating version for CounterStrikeSharp.API / Плагин теперь использует плавающую версию для CounterStrikeSharp.API
  - No manual updates needed for new API versions / Не требуется ручное обновление для новых версий API
  - Works with current and future API releases / Работает с текущими и будущими релизами API

- **Improved Documentation** / Улучшенная документация
  - Added COMPATIBILITY.md with detailed version info / Добавлен COMPATIBILITY.md с подробной информацией о версиях
  - Added CHANGELOG.md for tracking changes / Добавлен CHANGELOG.md для отслеживания изменений
  - Updated README with installation instructions / Обновлён README с инструкциями по установке

- **Better Project Structure** / Лучшая структура проекта
  - Added .gitignore for version control / Добавлен .gitignore для контроля версий
  - Cleaner dependency management / Более чистое управление зависимостями
  - Optimized build configuration / Оптимизированная конфигурация сборки

#### 🔄 Configuration Changes / Изменения в конфигурации
**No configuration changes required!** Your existing config will continue to work.  
**Изменений в конфигурации не требуется!** Ваш существующий конфиг продолжит работать.

#### 💾 Project File Changes / Изменения в файле проекта
- Removed duplicate DLL reference / Удалена дублирующаяся ссылка на DLL
- Added version ranges for dependencies / Добавлены диапазоны версий для зависимостей
- Set platform to x64 / Установлена платформа x64
- Set language version to latest / Установлена последняя версия языка

### Upgrade Steps / Шаги обновления

#### Option 1: Quick Update (Recommended) / Вариант 1: Быстрое обновление (Рекомендуется)

1. **Download the latest release** / Скачайте последний релиз
   - Download `IPAndSiteBlocker.dll` from the latest release
   - Скачайте `IPAndSiteBlocker.dll` из последнего релиза

2. **Backup current plugin** / Сделайте резервную копию текущего плагина
   ```bash
   # Backup your current plugin
   # Сделайте резервную копию текущего плагина
   cp csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/IPAndSiteBlocker.dll IPAndSiteBlocker.dll.backup
   ```

3. **Replace the DLL** / Замените DLL
   ```bash
   # Copy new version
   # Скопируйте новую версию
   cp IPAndSiteBlocker.dll csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/
   ```

4. **Reload the plugin** / Перезагрузите плагин
   ```
   css_plugins reload
   ```
   Or restart your server / Или перезапустите сервер

#### Option 2: Build from Source / Вариант 2: Сборка из исходников

1. **Pull latest changes** / Получите последние изменения
   ```bash
   git pull origin main
   ```

2. **Restore dependencies** / Восстановите зависимости
   ```bash
   dotnet restore
   ```

3. **Build the plugin** / Соберите плагин
   ```bash
   dotnet build -c Release
   ```

4. **Copy to server** / Скопируйте на сервер
   ```bash
   cp bin/Release/net8.0/IPAndSiteBlocker.dll /path/to/csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/
   ```

5. **Reload the plugin** / Перезагрузите плагин
   ```
   css_plugins reload
   ```

### Verification / Проверка

After updating, verify the plugin loaded correctly:  
После обновления проверьте что плагин загрузился корректно:

1. **Check plugin version** / Проверьте версию плагина
   ```
   css_plugins list
   ```
   Should show: `IPAndSiteBlocker v0.2.3`  
   Должно показать: `IPAndSiteBlocker v0.2.3`

2. **Test functionality** / Протестируйте функциональность
   - Send a message with a URL (should be blocked) / Отправьте сообщение с URL (должно быть заблокировано)
   - Check logs are being created / Проверьте что логи создаются
   - Verify config is loaded / Убедитесь что конфиг загружен

3. **Check for errors** / Проверьте на ошибки
   ```bash
   # Check CS2 console for any errors
   # Проверьте консоль CS2 на наличие ошибок
   
   # Check plugin logs
   # Проверьте логи плагина
   cat csgo/addons/counterstrikesharp/logs/ip_site_blocker.log
   ```

### Rollback / Откат

If you encounter any issues, you can rollback:  
Если вы столкнулись с проблемами, вы можете откатиться:

```bash
# Restore backup
# Восстановите резервную копию
cp IPAndSiteBlocker.dll.backup csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/IPAndSiteBlocker.dll

# Reload plugin
# Перезагрузите плагин
css_plugins reload
```

### Common Issues / Частые проблемы

#### Plugin doesn't load / Плагин не загружается
- **Solution**: Ensure CounterStrikeSharp is up to date  
- **Решение**: Убедитесь что CounterStrikeSharp обновлён
- Check that .NET 8.0 runtime is installed / Проверьте что .NET 8.0 runtime установлен

#### Build errors / Ошибки сборки
- **Solution**: Run `dotnet restore` and `dotnet clean` before building  
- **Решение**: Запустите `dotnet restore` и `dotnet clean` перед сборкой

#### Config not loading / Конфиг не загружается
- **Solution**: Your old config should work. If not, delete it and let the plugin regenerate it.  
- **Решение**: Ваш старый конфиг должен работать. Если нет, удалите его и позвольте плагину пересоздать его.

### Benefits of v0.2.3 / Преимущества v0.2.3

✅ **Future-proof** - Works with new CounterStrikeSharp versions automatically  
✅ **Защищён от будущего** - Работает с новыми версиями CounterStrikeSharp автоматически

✅ **No maintenance** - No need to update for API changes  
✅ **Без обслуживания** - Не нужно обновлять при изменениях API

✅ **Better stability** - Cleaner dependency management  
✅ **Лучшая стабильность** - Более чистое управление зависимостями

✅ **Improved docs** - Comprehensive guides and troubleshooting  
✅ **Улучшенная документация** - Подробные руководства и решение проблем

### Need Help? / Нужна помощь?

- Read [COMPATIBILITY.md](COMPATIBILITY.md) for version compatibility info  
- Прочитайте [COMPATIBILITY.md](COMPATIBILITY.md) для информации о совместимости версий

- Check [CHANGELOG.md](CHANGELOG.md) for detailed changes  
- Проверьте [CHANGELOG.md](CHANGELOG.md) для подробных изменений

- Review [README.md](README.md) for general documentation  
- Просмотрите [README.md](README.md) для общей документации

---

## Version Comparison / Сравнение версий

| Feature | v0.2.2 | v0.2.3 |
|---------|--------|--------|
| API Compatibility | Manual updates needed | Automatic |
| Dependency Management | Hardcoded DLL reference | NuGet package management |
| Future Updates | Manual rebuild required | Automatic compatibility |
| Documentation | Basic | Comprehensive |
| Version Control | No .gitignore | Clean with .gitignore |
| Build Config | Basic | Optimized |

---

**Recommendation**: All users should upgrade to v0.2.3 for automatic compatibility with future CounterStrikeSharp releases.  
**Рекомендация**: Все пользователи должны обновиться до v0.2.3 для автоматической совместимости с будущими релизами CounterStrikeSharp.

