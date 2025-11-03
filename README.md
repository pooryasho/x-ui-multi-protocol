# x-ui-multi-protocol ✨

> Traffic-unifier micro-service for the **x-ui** panel. Written in C#/.NET 8, hardened for systemd, and now fully configurable.

## کلیدی‌‌ترین تغییرات نسبت به ریپوی اصلی (3xui-multi-protocol)

1. **پشتیبانی از .NET 8** – دیگر نیازی به نصب نسخه 7 نیست.
2. **قابل تنظیم بودن بازه همگام‌سازی** با متغیر محیطی `SYNC_INTERVAL_SEC` (پیش‌فرض 30 ثانیه).
3. **هندلینگ خطا و لاگ تایم‌استمپ‌دار** – حلقه کرش نمی‌کند، خطاها در journalctl ثبت می‌شوند.
4. **busy_timeout برای SQLite** – از خطای «database is locked» جلوگیری می‌کند.
5. **سخت‌سازی systemd** (`ProtectSystem`, `NoNewPrivileges`, …).
6. README جدید + تصویر تازه.

---

# نصب / Install

```bash
bash <(curl -Ls https://raw.githubusercontent.com/pooryasho/x-ui-multi-protocol/master/install.sh)
```

اسکریپت به صورت خودکار:

- Runtime یا SDK نسخه 9 را نصب می‌کند (دبیان/اوبونتو 24.04+, سنت‌اواس، فدورا...)
- پروژه را `dotnet publish` کرده و در ‎`/etc/x-ui-multi-protocol`‎ قرار می‌دهد
- فایل ‎`x-ui-multi-protocol.service`‎ را در ‎`/etc/systemd/system`‎ نصب و فعال می‌کند

---
### تنظیم بازه زمانی (اختیاری)

- با دستور زیر فایل سرویس را باز کنید.


```bash
sudo nano /etc/systemd/system/x-ui-multi-protocol.service
```
- در بخش
Environment=SYNC_INTERVAL_SEC=120 
میتوانید تایم دلخواه خود برای اجرای سرویس را به ثانیه وارد کنید.
---

### ❗️توجه: برای کاهش حجم و یا ریست ترافیک کاربر لازم است سرویس را متوقف و پس از ویرایش آن را مجدد اجرا کنید!

### توقف / اجرا / مشاهده وضعیت

```bash
systemctl stop  x-ui-multi-protocol   # توقف
systemctl start x-ui-multi-protocol  # اجرا دوباره
systemctl status x-ui-multi-protocol # مشاهده وضعیت
```
---
## حذف کامل / Uninstall
```bash
bash <(curl -Ls https://raw.githubusercontent.com/pooryasho/x-ui-multi-protocol/master/uninstall.sh)
```

## نحوه کار

سرویس هر `SYNC_INTERVAL_SEC` ثانیه به دیتابیس ‎`/etc/x-ui/x-ui.db`‎ سر می‌زند و ترافیک همه کلاینت‌هایی که **Subscription-ID یکسان** دارند را مساوی با بیشترین مقدار بین آن‌ها می‌کند.

- هر عملیات دیتابیس در صورت خطای «database is locked» تا **۳ بار** با فاصله ۵۰۰ میلی‌ثانیه تکرار می‌شود؛ بنابراین سرویس در شرایط شلوغ کرش نمی‌کند.
- فایل کش `LocalDB.json` در همان پوشه اجرایی ‎`/etc/x-ui-multi-protocol`‎ ذخیره می‌شود.

![subscription](subscription-img.png)

> Only traffic is unified; ipLimit or other fields remain untouched.

Enjoy! 🎉
