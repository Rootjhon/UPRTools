
using System;
using System.Text;
using UnityEngine;

namespace UPRLuaProfiler
{
    public sealed class LuaLib
    {
        public static long GetLuaMemory(IntPtr luaState)
        {
            long result = 0;
            if (LuaProfiler.m_hasL)
            {
                result = LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNT, 0);
                result = result * 1024 + LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNTB, 0);
            }
            return result;
        }
        public static void DoString(IntPtr L, string script)
        {
            LuaHook.isHook = false;
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");
            if (LuaDLL.luaL_loadbuffer(L, chunk, (IntPtr)chunk.Length, "chunk") == 0)
            {
                if (LuaDLL.lua_pcall(L, 0, -1, oldTop + 1) == 0)
                {
                    LuaDLL.lua_remove(L, oldTop + 1);
                }
            }
            else
            {
                Debug.Log(script);
            }
            LuaHook.isHook = true;
            LuaDLL.lua_settop(L, oldTop);
        }
        public static void DoRefLuaFun(IntPtr L, string funName, int reference, LuaDLL.tolua_getref_fun refFun)
        {
            int moreOldTop = LuaDLL.lua_gettop(L);
            if (refFun == null)
            {
                LuaDLL.lua_getref(L, reference);
            }
            else
            {
                refFun(L, reference);
            }

            if (LuaDLL.lua_isfunction(L, -1) || LuaDLL.lua_istable(L, -1))
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "miku_handle_error");
                do
                {
                    LuaDLL.lua_getglobal(L, funName);
                    if (!LuaDLL.lua_isfunction(L, -1)) break;
                    LuaDLL.lua_pushvalue(L, oldTop);
                    if (LuaDLL.lua_pcall(L, 1, 0, oldTop + 1) == 0)
                    {
                        LuaDLL.lua_remove(L, oldTop + 1);
                    }

                } while (false);
                LuaDLL.lua_settop(L, oldTop);
            }

            LuaDLL.lua_settop(L, moreOldTop);
        }
    }
}