# multibot 

**multibot** is an incredibly simple libremetaverse bot designed to help QA
test mass region crossings.

## Configuration

**multibot** requires a YAML configuration file, example:

```yaml
---
admins:
  - <ADMIN1_AGENT_ID>
  - <ADMIN2_AGENT_ID>
bots:
  - first: <BOT1_FIRSTNAME>
    last: <BOT1_LASTNAME>
    pass: <BOT1_PASSWORD>
  - first: <BOT2_FIRSTNAME>
    last: <BOT2_LASTNAME>
    pass: <BOT2_PASSWORD>
```

Only admins are allowed to give multibots commands.

## Commands

Commands a prefixed by "!" a'la copybot in days of yore. They may be given in
local chat or IM.

```
!come - move to position of speaker
```

Additionally, there are some other behaviors:

- multibot(s) will accept teleport requests made by admins

## Development

Requirements:

- [dotnet](https://dotnet.microsoft.com/en-us/download)

Install dependencies and run multibot:
```
dotnet restore
dotnet run -- -c my-config.yaml
```

Build
```
dotnet build --configuration Release
```
