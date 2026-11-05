# BlazorHtmlEditor — analýza (FABLE)

## Co to je
Blazor komponentová knihovna (.NET 8, RCL) pro editaci Razor šablon: Monaco Editor (code editing s Razor syntax highlightingem) + živý náhled přes RazorLight (runtime kompilace šablon) + panel vlastností modelu pro vkládání `@Model.X` výrazů. Publikováno na NuGet jako `BlazorHtmlEditor` v1.0.1 (`BlazorHtmlEditor/BlazorHtmlEditor.csproj`). Dva projekty: knihovna `BlazorHtmlEditor/` a demo `BlazorHtmlEditor.Demo.Server/`. Pozn.: navzdory názvu to NENÍ WYSIWYG HTML editor — je to code editor (proto Fakvio používá Blazored.TextEditor).

## Stav
- **Git:** klon z `github.com/zcloudcz/BlazorHtmlEditor.git` 10. 2. 2026, jediný lokální commit „dev config ignore" (`.git/logs/HEAD`). Od té doby beze změn — projekt je v režimu údržby / hotovo.
- **Kompletnost:** malá, čistá a konzistentní codebase (4 komponenty, 2 služby, 2 modely), bohatě komentovaná. README je kvalitní a odpovídá kódu. Funkční demo server.
- **Mrtvý/živý kód:** nic mrtvého nenalezeno; `.gitignore` standardní VS šablona, LICENSE (MIT) přítomna.

## Problémy
1. **Žádné testy** — v solution (`BlazorHtmlEditor.sln`) není testovací projekt. Minimálně `ModelMetadataProvider` (reflexe), `TemplateEditor.CreateDemoData()` a `RazorRenderService` jsou čistě testovatelné.
2. **Cache klíč z `GetHashCode()`** — `RazorRenderService.RenderAsync` (`BlazorHtmlEditor/Services/RazorRenderService.cs:37`) používá `$"template_{razorTemplate.GetHashCode()}"` jako klíč kompilační cache. `string.GetHashCode()` je 32bitový s reálnou šancí kolize → při kolizi RazorLight vrátí ZKOMPILOVANOU CIZÍ ŠABLONU (špatný výstup bez chyby). Použít SHA-256 hex.
3. **Neomezený růst paměti při psaní** — náhled se renderuje při každé změně parametrů (`TemplatePreview.OnParametersSetAsync`, `Components/TemplatePreview.razor:125-128`) a každá verze šablony se zkompiluje do nové in-memory assembly, kterou .NET neuvolní (známé omezení RazorLight/Roslyn). Dlouhá editační session na Blazor Serveru = stovky MB. Chybí debounce a/nebo explicitní render tlačítko.
4. **Bezpečnost: runtime kompilace = spuštění libovolného kódu** — RazorLight šablona může volat cokoliv (`@{ System.IO.File.Delete(...) }`). Na Blazor Serveru se šablona kompiluje a SPOUŠTÍ na serveru s právy aplikace. README na to nikde neupozorňuje — knihovna je bezpečná jen pro důvěryhodné autory šablon (admin), ne pro user-generated obsah.
5. **Monaco z CDN** — README (§2) instruuje načítat Monaco 0.45.0 z cdnjs. Funguje, ale aplikace je závislá na třetí straně (výpadek, supply-chain); pro produkci by se hodila self-hosted varianta v `wwwroot` knihovny.
6. **Chybí CI/CD** — žádný GitHub Actions workflow (build/test/pack); NuGet release je tedy ruční a nereprodukovatelný.
7. **Drobnost:** `TemplatePreview` zachytává výjimky, ale `RazorRenderService` chyby už sám převádí na HTML error box (`RazorRenderService.cs:42-50`) — `renderError` větev v `TemplatePreview.razor:13-19` je tak prakticky nedosažitelná (duplicitní error handling, dvě různé podoby chybového UI).

## Návrhy
- **P1** Opravit cache klíč: SHA-256 hash šablony místo `GetHashCode()` (`RazorRenderService.cs:37`) — tichá záměna šablon je datová chyba.
- **P1** Do README přidat výrazné bezpečnostní upozornění: šablony = spustitelný kód na serveru; určeno jen pro důvěryhodné editory.
- **P2** Debounce náhledu (např. 500 ms po posledním stisku) a/nebo render až při přepnutí na Preview tab — omezí únik paměti z kompilací; zvážit limit velikosti memory cache.
- **P2** Přidat testovací projekt: unit testy `ModelMetadataProvider`, `CreateDemoData`, `RazorRenderService` (kolize klíčů, chybová šablona, prázdná šablona).
- **P3** GitHub Actions: build + test + `dotnet pack` s publikací na NuGet při tagu.
- **P3** Sjednotit error handling náhledu (jedno místo, jeden vzhled) — buď service vrací `Result`, nebo vyhazuje a komponenta renderuje chybu.
- **P3** Volitelně bundlovat Monaco do `wwwroot/_content` místo CDN.
