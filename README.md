# Serenity AI Translator

Editor package for translating Unity localization text with AI providers and generating voice clips with TTS providers.

## Installation

Install the package through Unity Package Manager:

1. Open `Window > Package Manager`.
2. Press `+`.
3. Select `Add package from git URL...`.
4. Use:

```text
https://github.com/denkacn/SerenityAITranslator.git
```

In this development project the package repository is this folder:

```text
Assets/SerenityAITranslator
```

When installed through Unity Package Manager, Unity exposes the package under:

```text
Packages/com.puzikgames.serenityai
```

## Dependencies

### Required

The core package depends on:

- `com.unity.nuget.newtonsoft-json` for provider payload serialization and local JSON data.

This dependency is declared in `package.json`.

### Bundled

The package includes:

- `Plugins/CSCore.dll`

The DLL is imported for Editor only. Keep it inside the package unless audio conversion/playback code is replaced with a Unity-only implementation.

### Optional source providers

The core package works through the `ISourceAssetProvider` contract. Concrete integrations are distributed separately as extension packages:

- `Extension/I2SourceAssetProviderExtension.unitypackage`
- `Extension/UnityLocalizationSourceAssetProviderExtension.unitypackage`

Import only the extension that matches the localization system in the target project.

## Settings Layout

Package defaults live inside the package:

```text
Settings
```

At runtime in the Unity Editor this resolves to either `Assets/SerenityAITranslator/Settings` in the development host project or `Packages/com.puzikgames.serenityai/Settings` after UPM installation.

Project/user settings are created outside the package:

```text
Assets/SerenityAIData/Editor/SerenityAi
Assets/SerenityAIData/VoicesLibrary
```

On first launch, existing legacy settings from `Assets/Editor/SerenityAi` are copied into the new project settings folder.

Provider configuration assets can contain tokens. Keep them local, store secrets in token files, or exclude project data from version control.

## Architecture Notes

- AI and HTTP calls go through `AiRequestService` in `Editor/Services/Common/Ai`.
- Translation and TTS providers are created by provider factories.
- Long-running translation/TTS operations expose status through `SerenityJob`.
- UI views should call manager methods and render job state, but avoid owning provider or HTTP logic.

## Package Boundary

Only files inside this package repository are intended to be distributed through git. In the development host project that means `Assets/SerenityAITranslator`.

Do not commit generated Unity host-project folders such as:

- `Library`
- `Temp`
- `Obj`
- `Logs`
- `UserSettings`
- `Assets/SerenityAIData`

## Provider Development

New translation providers should implement the existing translation provider contract and be registered in `TranslateProviderFactory`.

New TTS providers should implement the existing TTS provider contract and be registered in `TtsProviderFactory`.

Provider HTTP requests should use `AiRequestService` instead of creating local `HttpClient` or `UnityWebRequest` calls.
