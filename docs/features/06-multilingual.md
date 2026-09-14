# Feature 06 — Multilingual UI (tk / ru / en) + per-app language: it's a *choice*, not a lock-in

> **Why it beats other CRMs:** global tools ship UI in English and call it done;
> local incumbent CRMs hardcode one regional language. HasapNama is built for
> the Turkmen market, so the UI must be native in **türkmen, русский and
> English** — and each of the several apps sharing the server gets to pick its
> own default language.

## Shared

```csharp
public static class UILanguage
{
    public const string Turkmen = "tk";
    public const string Russian = "ru";
    public const string English = "en";
}

public record LanguageInfo(string Code, string NativeName, string Flag) { }
```

Built-in set: `tk 🇹🇲`, `ru 🇷🇺`, `en 🇬🇧` (flag chars are the emoji-free-ish
fallback; real flags come from the platform later).

## Server (tiny)

```
GET /api/i18n?lang=tk
```
returns a flat `Dictionary<string,string>` of the **active language's** strings
only (no dead translations in the APK). Fallback chain: requested → en → key
name, so a missing string never shows `{login.title}` raw.

## App

* `I18nService` — `T(key)`, `Lazy` cache per `lang`, raises `Changed` for live
  re-render, persisted via `SecureStorage`.
* Language switcher in the top bar (globe icon, next-cycled alphabetically,
  starts at `tk`).
* `_Imports.razor` gets `@using static HasapNama.App.Services.I18nHelper` so
  every page writes `@T("customers.title")`.

## Acceptance

- [ ] Switching tk → ru → en re-renders the whole page without a reload
- [ ] Missing string → graceful fallback to en, never the raw key or a crash on Android
- [ ] Login + customer CRUD fully translated in 3 languages, strands tested
