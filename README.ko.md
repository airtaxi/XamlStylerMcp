# 에이전트에서 XAML을 포매팅하세요.

[![NuGet](https://img.shields.io/nuget/v/xaml-styler-mcp.svg)](https://www.nuget.org/packages/xaml-styler-mcp)
[![NuGet downloads](https://img.shields.io/nuget/dt/xaml-styler-mcp.svg)](https://www.nuget.org/packages/xaml-styler-mcp)
[![Pack and Publish](https://github.com/airtaxi/XamlStylerMcp/actions/workflows/pack-and-publish.yml/badge.svg)](https://github.com/airtaxi/XamlStylerMcp/actions/workflows/pack-and-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

🌐 [English](README.md) | 한국어

xaml-styler-mcp는 [XamlStyler](https://github.com/Xavalon/XamlStyler)를 stdio MCP 서버로 노출하는 .NET 글로벌 도구입니다. 로컬 코딩 에이전트가 XAML 문자열을 포매팅하거나, XAML 파일을 포매팅하거나, 파일 변경 여부를 확인할 수 있습니다.

## 주요 기능

- Codex, Claude Code, GitHub Copilot CLI, Gemini CLI 또는 stdio MCP 서버를 지원하는 임의의 provider에서 XAML 포매팅 도구를 사용할 수 있습니다.
- `mcp-install`, `mcp-remove`, `mcp-uninstall` 명령으로 지원 provider에 MCP 서버를 등록하거나 제거합니다.
- provider 설치 명령이 없는 환경을 위해 임의의 `mcp.json` 파일을 직접 편집할 수 있습니다.
- `Settings.XamlStyler` 설정 파일을 재사용하며, 파일 경로 기준으로 상위 디렉터리에서 자동 탐색합니다.
- 파일 포매팅 시 기존 인코딩을 보존합니다.

## 설치

```powershell
dotnet tool install --global xaml-styler-mcp
```

기존 설치를 업데이트하려면 다음을 실행합니다.

```powershell
dotnet tool update --global xaml-styler-mcp
```

## Provider 설정

지원되는 provider CLI를 통해 등록합니다.

```powershell
xaml-styler-mcp mcp-install --provider codex
xaml-styler-mcp mcp-install --provider claude
xaml-styler-mcp mcp-install --provider copilot
xaml-styler-mcp mcp-install --provider gemini
```

등록을 제거하려면 다음을 실행합니다.

```powershell
xaml-styler-mcp mcp-remove --provider codex
xaml-styler-mcp mcp-uninstall --provider claude
```

기본 MCP 서버 이름은 `xaml-styler`입니다. 필요하면 이름을 지정할 수 있습니다.

```powershell
xaml-styler-mcp mcp-install --provider codex --name xaml-styler
```

## 사용자 지정 mcp.json

다른 provider에서는 대상 MCP 설정 파일을 직접 지정합니다.

```powershell
xaml-styler-mcp mcp-install --config "C:\path\to\mcp.json"
```

다음 항목이 추가됩니다.

```json
{
  "mcpServers": {
    "xaml-styler": {
      "command": "xaml-styler-mcp",
      "args": []
    }
  }
}
```

설정 편집기는 읽을 때 JSON 주석과 trailing comma를 허용합니다. 저장할 때는 표준 JSON으로 정규화하며, 기존 설정 파일을 수정하기 전에 `.bak` 백업을 만듭니다.

## MCP 도구

- `format_xaml`: XAML 문자열을 포매팅해 결과 문자열을 반환합니다.
- `format_xaml_file`: `.xaml` 또는 `.axaml` 파일을 포매팅하고 기본적으로 파일에 씁니다.
- `check_xaml_file`: `.xaml` 또는 `.axaml` 파일에 포매팅 변경이 필요한지 확인합니다.

각 도구는 선택적으로 XamlStyler 설정 파일 경로인 `configPath`를 받을 수 있습니다. 파일 기반 도구는 `configPath`가 없으면 상위 디렉터리에서 `Settings.XamlStyler`를 찾습니다.

## 수동 서버 명령

인자 없이 실행하면 stdio MCP 서버가 시작됩니다.

```powershell
xaml-styler-mcp
```

CLI 도움말은 다음으로 확인합니다.

```powershell
xaml-styler-mcp help
xaml-styler-mcp --help
```

## 패키지 개발

```powershell
dotnet restore
dotnet build
dotnet test
dotnet pack
```

## 라이선스

xaml-styler-mcp는 [MIT License](LICENSE.txt)로 배포됩니다.

XAML 포매팅은 Apache-2.0으로 배포되는 [XamlStyler](https://github.com/Xavalon/XamlStyler)를 사용합니다. 자세한 고지는 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)와 [licenses/XamlStyler-Apache-2.0.txt](licenses/XamlStyler-Apache-2.0.txt)를 확인해 주세요.

## 작성자

[Howon Lee (airtaxi)](https://github.com/airtaxi)가 만들었습니다.

OpenAI Codex의 도움을 받아 제작했습니다.
