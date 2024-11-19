local protobuf = require "pb"
local cjsonutil = require "cjson.util"
local crypt = require "crypt"
local errcode = require "Error.errcode"

local playerInfo = require "Entity.PlayerInfo"
local loginInfo = playerInfo.LoginInfo

LoginPart = LoginPart or {}

function LoginPart.Lock()
    LoginPart.timerId = LLoadingMgr:Lock(0.5)
end

function LoginPart.Unlock()
    if LoginPart.timerId then
        LLoadingMgr:Unlock(LoginPart.timerId)
        LoginPart.timerId = nil
    end
end

function LoginPart.DoLogin()
    LNetMgr:CloseConnect()
    LNetMgr:CheckConnect()

    -- 打开loading界面
    LoginPart.Lock()
end

function LoginPart.on_acknowledgment(args)

	loginInfo.acknumber = crypt.base64decode(args.acknumber)
	loginInfo.clientkey = crypt.randomkey()
	LNetMgr:SendMessage ("login.handshake", {
		client_pub = crypt.base64encode(crypt.dhexchange(loginInfo.clientkey)),
	})
end

local cb_handshake = function(req, opflag, error_code)
	if not opflag then
		LogError(string.format("<error> RESPONSE.handshake errcode:%d(%s)", error_code, errcode.error_msg(error_code)))
		return
	end
	LNetMgr:SendMessage ("login.auth", {
		username = crypt.base64encode(crypt.desencode(loginInfo.secret, loginInfo.username)),
		password = crypt.base64encode(crypt.desencode(loginInfo.secret, loginInfo.password)),
	})
end

function LoginPart.on_handshake(resp)
	if not resp then
		print(string.format("<error> RESPONSE.handshake resp is nil:"))
		return
	end
	loginInfo.secret = crypt.dhsecret(crypt.base64decode(resp.secret), loginInfo.clientkey)
	print("sceret is ", crypt.hexencode(loginInfo.secret))

	local hmac = crypt.base64encode(crypt.hmac64(loginInfo.acknumber, loginInfo.secret))
	LNetMgr:SendMessage ("login.challenge", { hmac = hmac }, cb_handshake)
end

function LoginPart.on_auth(resp)
	if not resp then
		print(string.format("<error> RESPONSE.auth resp is nil:"))
		return
	end

	loginInfo.login_session = resp.login_session
	loginInfo.login_session_expire = resp.expire
	loginInfo.token = crypt.base64decode(resp.token)

	-- 跳转到游戏服务器
	--LNetMgr:SendMessage ("req_switchgame", nil)

	loginInfo.authed = true
    LNetMgr:StartPing()

	-- 请求角色列表
    CharacterPart.list()
end

return LoginPart
