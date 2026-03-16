# Palette Studio

A WinUI 3 desktop app that extracts dominant color palettes from images and exports them in formats ready for designers and developers.

<video src="docs/demo.mp4" controls width="100%"></video>

---

## Features

- **Drag & drop or browse** — drop any PNG, JPG, or WEBP onto the app and get results instantly
- **Median-cut color extraction** — implemented from scratch using `BitmapDecoder` and raw BGRA pixel data; no third-party image library
- **Animated swatch grid** — colors appear with a staggered slide-in animation via `ImplicitAnimationCollection`
- **Color detail view** — tap any swatch to edit Hex, RGB, and HSL values with live preview; transitions use `ConnectedAnimation`
- **Three export formats** — CSS Custom Properties, XAML `ResourceDictionary`, and JSON; copy to clipboard or save to file
- **Theme switching** — dark, light, or system default; applied live without restart and persisted across sessions

---

## Stack

| Layer | Technology |
|-------|-----------|
| UI framework | WinUI 3 + Windows App SDK 1.7 |
| Language | C# 13 / .NET 9 |
| MVVM | CommunityToolkit.Mvvm 8.4 (`[ObservableProperty]`, `[RelayCommand]`) |
| DI | Microsoft.Extensions.DependencyInjection |
| Background | Mica material (`MicaBackdrop`) |
| Packaging | Unpackaged (no MSIX required) |

---

## Architecture

```
PaletteStudio/
├── Contracts/          # INavigationService, IThemeService, IColorExtractionService
├── Models/             # ColorModel, PaletteModel (immutable records)
├── Services/           # NavigationService, ThemeService, ColorExtractionService
├── ViewModels/         # One ViewModel per page (CommunityToolkit.Mvvm)
├── Views/              # XAML pages + LabeledSlider UserControl
└── Themes/             # ResourceDictionary with spacing, shape, and color tokens
```

**Rules enforced throughout:**
- ViewModels never reference Views or call `Frame.Navigate()` directly — all navigation goes through `INavigationService`
- All colors and spacing values come from `Themes/Resources.xaml` tokens — no hardcoded hex or magic numbers
- `x:Bind` everywhere; classic `Binding` is not used
- `ConfigureAwait(false)` on all async service calls
- Every interactive control has `AutomationProperties.Name`

---

## Color extraction — how it works

The `ColorExtractionService` reads raw pixel data without any external library:

1. **Decode** — `BitmapDecoder.GetPixelDataAsync()` returns a flat `byte[]` in BGRA8 format
2. **Subsample** — every 4th pixel in both axes is sampled (1/16th of the image) for performance
3. **Median-cut** — pixels are treated as points in RGB space; the algorithm recursively splits the bounding box along the channel with the largest range until 8 buckets remain
4. **Average** — each bucket's pixels are averaged to produce one representative color
5. **Sort** — results are ordered dark → light by luminance (`0.299R + 0.587G + 0.114B`)

---

## Running locally

### Prerequisites

- Visual Studio 2022 17.9+ with the **Windows application development** workload
- Windows 10 1809 (build 17763) or later

### Steps

1. Clone the repo
2. Open `PaletteStudio.csproj` in Visual Studio
3. Set the platform to **x64** (WinUI 3 does not support Any CPU)
4. Press **F5**

NuGet packages restore automatically on first build.

---

## License

MIT
