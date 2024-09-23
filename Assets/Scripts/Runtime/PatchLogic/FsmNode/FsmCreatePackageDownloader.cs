﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// 创建文件下载器
/// </summary>
public class FsmCreatePackageDownloader : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("创建补丁下载器！");
        CreateDownloader().Forget();
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    async UniTask CreateDownloader()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: false);

        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        _machine.SetBlackboardValue("Downloader", downloader);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            _machine.ChangeState<FsmUpdaterDone>();
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
        }
    }
}