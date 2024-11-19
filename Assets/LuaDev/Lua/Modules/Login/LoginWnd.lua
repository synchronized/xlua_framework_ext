local UIView = require "Framework.Core.UIView"
local LoginWnd = UIView:extend("LoginWnd")

local playerInfo = require "Entity.PlayerInfo"

function LoginWnd:PrefabPath()
    return "Prefabs/Login/LoginWnd"
end

function LoginWnd:Awake()
    self.super.Awake(self)

    local txtUsername = self.transform:Find("UIWindow/panContent/panInput/txtAccount"):GetComponent(typeof(TMPro.TMP_InputField))
    local txtPassword = self.transform:Find("UIWindow/panContent/panInput/txtPassword"):GetComponent(typeof(TMPro.TMP_InputField))

    --设置保存的用户名和密码
    txtUsername.text = PlayerPrefs.GetString("PLAYERINFO.USERNAME")
    txtPassword.text = PlayerPrefs.GetString("PLAYERINFO.PASSWORD")

    self.onLoginBtnClick = function()
        local username = txtUsername.text
        local password = txtPassword.text
        playerInfo.LoginInfo.username = username
        playerInfo.LoginInfo.password = password

        --保存用户名和密码
        PlayerPrefs.SetString("PLAYERINFO.USERNAME", username)
        PlayerPrefs.SetString("PLAYERINFO.PASSWORD", password)
        Log(string.format("username: %s, password: %s", username, password))
        --CommandManager.Execute(CommandID.DoLogin)
        --TODO 临时直接进入主界面
        CommandManager.Execute(CommandID.OpenUI, "LobbyMainMgr")
    end

    self.btnLogin = self.transform:Find("UIWindow/panContent/btnLogin"):GetComponent(typeof(UnityEngine.UI.Button));
    self.btnLogin.onClick:AddListener(self.onLoginBtnClick)
end

function LoginWnd:OnDestroy()
    self.btnLogin.onClick:RemoveListener(self.onLoginBtnClick)
end

return LoginWnd
