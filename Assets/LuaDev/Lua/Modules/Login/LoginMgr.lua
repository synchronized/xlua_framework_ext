local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local LoginMgr = UIModuleMgr:extend("LoginMgr")

require "Modules.Login.LoginPart"

function LoginMgr:init()
    self.super.init(self)

    self:AddUI("Login", require "Modules.Login.LoginWnd")

    --TODO:注册服务器广播事件：onReceveServerData()
    CommandManager.Add(CommandID.DoLogin, LoginPart.DoLogin)
end

function LoginMgr:EnterLogin()
    coroutine.start(function ()

        --加载进度条界面
        local progress = Modules.Common:OpenUI("Progress")
        progress:SetSmoothSpeed(1)
        progress:SetTips("加载资源中...")
        progress:OnComplete(function ()
            LuaManager.StartComplete()
                --coroutine.wait(1) --测试用

            Modules.Login:OpenUI("Login")
            progress:CloseUI()
        end)

        local preloadPaths = {
            "Prefabs/Login/LoginWnd",
        }

        local loader = ResManager.LoadAssetList(preloadPaths)
        local progressPairs = coroutine.waitprogressfor(loader)
        for _, progressValue in progressPairs() do
            progress:SetProgress(progressValue)
        end
        loader:Dispose()
        loader = nil

        progress:SetProgress(1)
    end)
end

return LoginMgr
