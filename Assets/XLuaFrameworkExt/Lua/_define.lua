CS = CS

--Unity对象在lua内部优化需要先加载
Mathf      = require "UnityEngine.Mathf"
Vector2    = require "UnityEngine.Vector2"
Vector3    = require "UnityEngine.Vector3"
Vector4    = require "UnityEngine.Vector4"
Quaternion = require "UnityEngine.Quaternion"
Color      = require "UnityEngine.Color"
Ray        = require "UnityEngine.Ray"
Bounds     = require "UnityEngine.Bounds"
RaycastHit = require "UnityEngine.RaycastHit"
Touch      = require "UnityEngine.Touch"
LayerMask  = require "UnityEngine.LayerMask"
Plane      = require "UnityEngine.Plane"
Time       = require "UnityEngine.Time"
Object     = require "UnityEngine.Object"

--Unity对象
UnityEngine = CS.UnityEngine
Application = UnityEngine.Application
GameObject = UnityEngine.GameObject
Transform = UnityEngine.Transform
Input = UnityEngine.Input
Slider = UnityEngine.UI.Slider
PlayerPrefs = UnityEngine.PlayerPrefs
EventTrigger = UnityEngine.EventSystems.EventTrigger
HorizontalLayoutGroup = UnityEngine.UI.HorizontalLayoutGroup
VerticalLayoutGroup = UnityEngine.UI.VerticalLayoutGroup
LayoutRebuilder = UnityEngine.UI.LayoutRebuilder
Random = UnityEngine.Random
Time = UnityEngine.Time
Camera = UnityEngine.Camera

TMPro = CS.TMPro

DG = CS.DG

GameFramework = CS.GameFramework

--C#对象
XLuaFrameworkExt = CS.XLuaFrameworkExt
GlobalManager = XLuaFrameworkExt.GlobalManager
EventManager = XLuaFrameworkExt.EventManager
LuaManager = XLuaFrameworkExt.LuaManager
ResManager = XLuaFrameworkExt.ResManager
SoundManager = XLuaFrameworkExt.SoundManager
HttpManager = XLuaFrameworkExt.HttpManager
NetManager = XLuaFrameworkExt.NetManager

--C#工具或方法
MD5 = {
    StirngToMD5 = XLuaFrameworkExt.LMD5.StirngToMD5,
    BytesToMD5 = XLuaFrameworkExt.LMD5.BytesToMD5,
    FileToMD5 = XLuaFrameworkExt.LMD5.FileToMD5
}
AES = {
    Encrypt = XLuaFrameworkExt.LAES.Encrypt,
    Decrypt = XLuaFrameworkExt.LAES.Decrypt
}
LUtils = XLuaFrameworkExt.LUtils

--第三方json插件(用法：JSON.decode(),JSON.encode())
JSON = require "cjson"

--Lua工具
LButton = XLuaFrameworkExt.LButton
LButtonEffect = XLuaFrameworkExt.LButtonEffect

GameClient = CS.GameClient

typeof = typeof
