# SC2CoopPlugin
- 软件的设计目的是，分享指挥官操作流程
- 修改游戏内存/使用作弊码是不被欢迎的
- 当前早期开发，招收下列打工人，想来搭把手的欢迎 QQ群530820766
  - Unity程序，游戏内程序显示、编辑器功能等
  - 多语言本地化，非简体中文都需要帮忙搞下翻译
  - OCR/神经网络
    - 目前使用的WindowsOCR，识别准确率低，识别速度慢
    - 写了个神经网络的类库，用于快速识别地图内的时间，但没写完
  - 星际数据录入/复核/测试
- 即将大幅重构代码 ~~猫猫要累死了~~
- ~~有没有人来救救我，算上最开始Winform的测试代码，我已经撸了2W行代码了~~
- ~~Dead Game努力个什么~~
- ~~我没引用SC2API，风暴之门出来后直接Fork仓库你怕不怕~~
- 此仓库不使用星际2原图，请使用AI重绘后再上传

# 仓库同步

## 数据仓库结构
- 数据仓库指的是这个[仓库](https://gitcode.net/qq_34919016/sc2coopplugin-resource)
---
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

## 运行时文件夹结构
- LocalResourceRepository
- SC2Plugin.exe
- Temp(存放临时文件)
- (其它)

## 编辑器更新流程
- 修改资源文件，git上传到[数据仓库](https://gitcode.net/qq_34919016/sc2coopplugin-resource)
- Tools/GitRepository/DownloadUpdate 拉取对应分支的更新，内容会同步到 Unity/Assets/StreamingAssets 和 Unity/LocalResourceRepository
- Unity/Assets/StreamingAssets/* 的资源，将跟随包作为默认资源
- Unity/LocalResourceRepository/* 不跟随打包，git忽略此路径

## 运行时更新流程
- 获取commit页的最后一次提交的commit值，命名为origin commit 1
- 比对本地commit版本和origin commit 1是否一致
  - 如果一致，中断更新
- 如果不一致，直接下载最新版本的仓库zip
- 下载成功后立即获取commit页的最后一次提交的commit值，命名为origin commit 2
  - 如果origin commit 1与origin commit 2相等，更新完成
- 丢弃版本不确定的仓库zip
  - 如果更新尝试到达3次，中断更新
- 重新尝试更新
