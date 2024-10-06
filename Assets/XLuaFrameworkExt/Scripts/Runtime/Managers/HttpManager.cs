﻿using System;
using System.IO;
using System.Net;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using XLua;

namespace XLuaFrameworkExt
{
    public static class HttpManager
    {
        /// <summary>
        /// 同步
        /// </summary>
        public static string Get(string url)
        {
            if (url.Contains("?"))
            {
                url += "&rand=" + DateTime.Now.Ticks;
            }
            else
            {
                url += "?rand=" + DateTime.Now.Ticks;
            }
            string responseData = null;
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Timeout = 5000;
            StreamReader responseReader = null;
            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch
            {
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
                webRequest = null;
            }
            return responseData;
        }

        /// <summary>
        /// 异步
        /// </summary>
        public static void Get(string url, LuaFunction callback)
        {
            GetDo(url, callback).Forget();
        }

        static async UniTask GetDo(string url, LuaFunction callback)
        {
            if (url.Contains("?"))
            {
                url += "&rand=" + DateTime.Now.Ticks;
            }
            else
            {
                url += "?rand=" + DateTime.Now.Ticks;
            }
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
            if (request.error != null)
            {
                Debug.LogError("Http Get Fail: " + request.error);
                callback.Call("");
                return;
            }
            callback.Call(request.downloadHandler.text);
        }

        /// <summary>
        /// 同步
        /// </summary>
        public static string Post(string url, LuaTable data, string dataType = null)
        {
            if (url.Contains("?"))
            {
                url += "&rand=" + DateTime.Now.Ticks;
            }
            else
            {
                url += "?rand=" + DateTime.Now.Ticks;
            }
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "POST";
            string args = "";
            if (!string.IsNullOrEmpty(dataType) && dataType.ToLower().Contains("json"))
            {
                webRequest.ContentType = "application/json;charset=utf-8";
                args = "{";
                data.ForEach<string, string>((k, v) => {
                    if (!args.Equals("{")) args += ",";
                    args += $"\"{k}:{v}\"";
                });
                args += "}";
            }
            else
            {
                webRequest.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                data.ForEach<string, string>((k, v) => {
                    if (args != "") args += "&";
                    args += $"{k}={v}";
                });
            }
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Timeout = 5000;
            StreamWriter requestWriter = null;
            StreamReader responseReader = null;
            string responseData = null;
            try
            {
                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(args);
                requestWriter.Close();
                requestWriter = null;
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                if (requestWriter != null)
                {
                    requestWriter.Close();
                }
                if (responseReader != null)
                {
                    responseReader.Close();
                }
                webRequest.GetResponse().GetResponseStream().Close();
            }
            return responseData;
        }

        /// <summary>
        /// 异步
        /// </summary>
        public static void Post(string url, LuaTable data, LuaFunction callback, string dataType = null)
        {
            PostDo(url, data, callback, dataType).Forget();
        }

        static async UniTask PostDo(string url, LuaTable data, LuaFunction callback, string dataType)
        {
            if (url.Contains("?"))
            {
                url += "&rand=" + DateTime.Now.Ticks;
            }
            else
            {
                url += "?rand=" + DateTime.Now.Ticks;
            }
            string contentType = null;
            string args = "";
            if (!string.IsNullOrEmpty(dataType) && dataType.ToLower().Contains("json"))
            {
                contentType = "application/json;charset=utf-8";
                args = "{";
                data.ForEach<string, string>((k, v) => {
                    if (!args.Equals("{")) args += ",";
                    args += $"\"{k}:{v}\"";
                });
                args += "}";
            }
            else
            {
                contentType = "application/x-www-form-urlencoded;charset=utf-8";
                data.ForEach<string, string>((k, v) => {
                    if (args != "") args += "&";
                    args += $"{k}={v}";
                });
            }
            UnityWebRequest request = UnityWebRequest.Post(url, args);
            request.SetRequestHeader("Content-Type", contentType);
            await request.SendWebRequest();
            if (request.error != null)
            {
                Debug.LogError("Http POST Fail: " + request.error);
                callback.Call("");
                return;
            }
            callback.Call(request.downloadHandler.text);
        }

        public static string UrlEncode(string str)
        {
            return UnityWebRequest.EscapeURL(str);
        }

        public static string UrlDecode(string str)
        {
            return UnityWebRequest.UnEscapeURL(str);
        }
    }
}
