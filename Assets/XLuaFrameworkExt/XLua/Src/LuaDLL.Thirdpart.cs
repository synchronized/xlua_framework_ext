/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

namespace XLua.LuaDLL
{

    //using System;
    using System.Runtime.InteropServices;
    using XLua;

    public partial class Lua
    { 

        /* add library cjson */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson_safe(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadCjson(System.IntPtr L)
        {
            return luaopen_cjson(L);
        }

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadCjsonSafe(System.IntPtr L)
        {
            return luaopen_cjson_safe(L);
        }


        /* add library lpeg */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_lpeg(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadLpeg(System.IntPtr L)
        {
            return luaopen_lpeg(L);
        }

        /* add library lua-protobuf */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_io(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_conv(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_buffer(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_slice(System.IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_unsafe(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPb(System.IntPtr L)
        {
            return luaopen_pb(L);
        }
        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPbIo(System.IntPtr L)
        {
            return luaopen_pb_io(L);
        }
        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPbConv(System.IntPtr L)
        {
            return luaopen_pb_conv(L);
        }
        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPbBuffer(System.IntPtr L)
        {
            return luaopen_pb_buffer(L);
        }
        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPbSlice(System.IntPtr L)
        {
            return luaopen_pb_slice(L);
        }
        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPbUnsafe(System.IntPtr L)
        {
            return luaopen_pb_unsafe(L);
        }

        /* add library lua-rapidjson */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_rapidjson(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadRapidJson(System.IntPtr L)
        {
            return luaopen_rapidjson(L);
        }

        /* add library skynet.crypt */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_crypt(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadCrypt(System.IntPtr L)
        {
            return luaopen_crypt(L);
        }

        /* add library sproto */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_sproto_core(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadSprotoCore(System.IntPtr L)
        {
            return luaopen_sproto_core(L);
        }

    }
}
