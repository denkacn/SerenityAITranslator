# SerenityAITranslator
AI Translator for Unity

## Settings layout

Package defaults live inside the package:

- `Assets/SerenityAITranslator/Settings`

Project/user settings are created outside the package:

- `Assets/SerenityAIData/Editor/SerenityAi`
- `Assets/SerenityAIData/VoicesLibrary`

On first launch, existing legacy settings from `Assets/Editor/SerenityAi` are copied into the new project settings folder. Provider configuration assets can contain tokens, so keep them local or use token files.

## Provider transport

AI providers should use `AiRequestService` from `Editor/Services/Common/Ai` for HTTP calls. The service centralizes JSON/binary POST requests, timeout handling, safe error capture, and the shared request result contract.
