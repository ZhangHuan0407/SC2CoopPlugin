# SC2CoopPlugin

## 仓库同步

### 资源仓库结构
- README
- Commit(is origin ? empty file : local commit version)
- CommanderPipeline-BuildIn
  - ${ComanderName}
    - XXX.json(CommanderPipeline file)
- CommanderPipeline-PlayerProvided
  - ${UploadPersonName}
    - XXX.json(CommanderPipeline file)
- Localization
  - ${System.SystemLanguage}.json
- Tables
  - XXX.json(table file)
- Models
  - XXX.json(model file)

```
enum System.SystemLanguage
{
    English = 10,
    French = 14,
    Korean = 23,
    ChineseSimplified = 40,
    ChineseTraditional = 41,
    German = 0xF,
}
```

### 运行时文件夹结构
- LocalResourceRepository
- SC2Plugin.exe

### 编辑器更新流程
- 修改资源文件，git上传到[资源仓库](https://gitcode.net/qq_34919016/sc2coopplugin-resource)
- Tools/GitRepository/DownloadUpdate 拉取对应分支的更新，内容会同步到 Assets/StreamingAssets
- Assets/StreamingAssets 下的资源，将跟随包作为默认资源

### 运行时更新流程
- 获取commit页的最后一次提交的commit值，命名为origin commit 1
- 比对本地commit版本和origin commit 1是否一致
  - 如果一致，中断更新
- 如果不一致，直接下载最新版本的仓库zip
- 下载成功后立即获取commit页的最后一次提交的commit值，命名为origin commit 2
  - 如果origin commit 1与origin commit 2相等，更新完成
- 丢弃版本不确定的仓库zip
  - 如果更新尝试到达3次，中断更新
- 重新尝试更新

### 上传指挥官流程
- Pull Request
- 修改非当前用户文件的Pull Request不被通过