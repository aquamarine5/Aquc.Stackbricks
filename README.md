# Aquc.Stackbricks
[![](https://img.shields.io/badge/.NET-%206-blue)]()
[![GitHub repo size](https://img.shields.io/github/repo-size/aquamarine5/Aquc.Stackbricks)](https://github.com/aquamarine5/Aquc.Stackbricks)
[![Commit Activity](https://img.shields.io/github/commit-activity/m/aquamarine5/Aquc.Stackbricks)]()
[![Last commit](https://img.shields.io/github/last-commit/aquamarine5/Aquc.Stackbricks)]()
[![Release date](https://img.shields.io/github/release-date-pre/aquamarine5/Aquc.Stackbricks)]()
[![](https://img.shields.io/github/actions/workflow/status/aquamarine5/Aquc.Stackbricks/codeql.yml)]()
[![Download count](https://img.shields.io/github/downloads/aquamarine5/Aquc.Stackbricks/total)]()
# How to start?
- First, download the single executable file `Aquc.Stackbricks.exe` from [release] (https://github.com/aquamarine5/Aquc.Stackbricks/releases/latest).
- Use `Aquc.Stackbricks config create` to create an empty config.
- Open the `Aquc.Stackbricks.config.json` config file and write the `ProgramManifest` values.
## About ProgramManifest
- `Version`: current version of the program
- `ProgramDir`: directory of the program file, use `\\` to replace `\`.
- `Id`: Name to identify the program
- `LastCheckTime` and `LastUpdateTime`: remain `null`.
### MsgPvderId and MsgPvderData

| Id |Description |Data
| --- | --- | --- |
|stbks.msgpvder.weibocmts|Request message from commits of Weibo post|Identity of Weibo post, use string type|
|stbks.msgpvder.bilicmts|Request message from commits of Bilibili post|Identity of Bilibili post, use string type|

### About commits standard about `%v1`
- Simple: `stbks.msgpvder.weibocmts%1;;3.0.2.725;;stbks.pkgpvder.ghproxy;;aquamarine5]]Aquc.Stackbricks]]3.0.2.725]]Aquc.Stackbricks.exe​`
- Use `;;` as splited char
- (MsgPvderId) ;; (VersionCode) ;; (PkgPvderId) ;; (PkgPvderData)

### About PkgPvderId and PkgPvderData
- `stbks.pkgpvder.ghproxy`: Use `]]` as splited char
- (Username) ]] (RepoName) ]] (ReleaseTagName) ]] (ReleaseAssetFileName)

### UpdateActions
- UpdateActions is a list to specify what action to perform.
- Items in the list should have three values: `Id`, `Args` and `Flags`.

| Id | Description | Args | Flags
| ----------- | ----------- |-------|-------|
| stbks.action.replaceall | Copy or replace all downloaded files in ProgramDir.| | | |
| *stbks.action.applyselfupdate* | **Should not use this action in ProgramManifest**, apply Stackbricks self update| | | |

## How to interoperation?
- `update`: check for update, download package and run update action if program has latest version
- `self update`: check for Stackbricks update, download package and apply update if Stackbricks has latest version

![Alt](https://repobeats.axiom.co/api/embed/65438e651c9b1b2fb5ac54201fc8ec26cba0b0a9.svg "Repobeats analytics image")
