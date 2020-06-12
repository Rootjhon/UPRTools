using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UPRLuaProfiler
{
    public sealed class LuaHook
    {
        public static bool isHook = true;
        public static byte[] Hookloadbuffer(IntPtr L, byte[] buff, string name)
        {
            if (LuaDeepProfilerSetting.Instance.isCleanMode)
            {
                return buff;
            }
            if (!isHook)
            {
                return buff;
            }
            if (buff.Length < 2)
            {
                return buff;
            }
            if (buff[0] == 0x1b && buff[1] == 0x4c)
            {
                return buff;
            }

            string value = "";
            string hookedValue = "";

            string fileName = name.Replace(".lua", "");
            fileName = fileName.Replace("@", "").Replace('.', '/');
            if (fileName.Contains("protobuf"))
            {
                return buff;
            }
            // utf-8
            if (buff[0] == 239 && buff[1] == 187 && buff[2] == 191)
            {
                value = Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
            }
            else
            {
                value = Encoding.UTF8.GetString(buff);
            }

            hookedValue = Parse.InsertSample(value, fileName);

            buff = Encoding.UTF8.GetBytes(hookedValue);

            return buff;
        }

        public static void HookRef(IntPtr L, int reference, LuaDLL.tolua_getref_fun refFun = null)
        {
            if (isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_add_ref_fun_info", reference, refFun);
            }
        }

        public static void HookUnRef(IntPtr L, int reference, LuaDLL.tolua_getref_fun refFun = null)
        {
            if (isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_remove_ref_fun_info", reference, refFun);
            }
        }

        #region check
        public static int staticHistoryRef = -100;
        public static LuaDiffInfo RecordStatic()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            isHook = false;

            ClearStaticRecord();
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_getglobal(L, "_G");
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_G");
            //recrod
            LuaDLL.lua_newtable(L);
            staticHistoryRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, staticHistoryRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushvalue(L, -1);
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            if (LuaDLL.lua_pcall(L, 6, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_R");
            LuaDLL.lua_getref(L, staticHistoryRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_getref(L, nullObjectRef);

            if (LuaDLL.lua_pcall(L, 6, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);

            isHook = true;
            return ld;
        }

        public static int historyRef = -100;
        public static LuaDiffInfo Record()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            isHook = false;

            ClearRecord();
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_getglobal(L, "_G");
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_G");
            //recrod
            LuaDLL.lua_newtable(L);
            historyRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, historyRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushvalue(L, -1);
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_pcall(L, 7, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_R");
            LuaDLL.lua_getref(L, historyRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_getref(L, nullObjectRef);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_pcall(L, 7, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);

            LuaDLL.lua_unref(L, nullObjectRef);
            isHook = true;
            return ld;
        }

        public static void ClearStaticRecord()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            if (staticHistoryRef != -100)
            {
                LuaDLL.lua_unref(L, staticHistoryRef);
                staticHistoryRef = -100;
            }
        }

        public static void ClearRecord()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            if (historyRef != -100)
            {
                LuaDLL.lua_unref(L, historyRef);
                historyRef = -100;
            }
        }
        private static void SetTable(int refIndex, Dictionary<LuaTypes, HashSet<string>> dict, Dictionary<string, List<string>> detailDict)
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            dict.Clear();
            int oldTop = LuaDLL.lua_gettop(L);

            LuaDLL.lua_getref(L, refIndex);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE)
            {
                LuaDLL.lua_pop(L, 1);
                return;
            }
            int t = oldTop + 1;
            LuaDLL.lua_pushnil(L);  /* 第一个 key */
            while (LuaDLL.lua_next(L, t) != 0)
            {
                /* 用一下 'key' （在索引 -2 处） 和 'value' （在索引 -1 处） */
                int key_t = LuaDLL.lua_gettop(L);
                LuaDLL.lua_pushnil(L);  /* 第一个 key */
                string firstKey = null;
                List<string> detailList = new List<string>();
                while (LuaDLL.lua_next(L, key_t) != 0)
                {
                    string key = LuaHook.GetRefString(L, -1);
                    if (string.IsNullOrEmpty(firstKey))
                    {
                        firstKey = key;
                    }
                    detailList.Add(key);
                    LuaDLL.lua_pop(L, 1);
                }
                LuaDLL.lua_settop(L, key_t);
                if (!string.IsNullOrEmpty(firstKey))
                {
                    HashSet<string> list;
                    LuaTypes luaType = (LuaTypes)LuaDLL.lua_type(L, -2);
                    if (!dict.TryGetValue(luaType, out list))
                    {
                        list = new HashSet<string>();
                        dict.Add(luaType, list);
                    }
                    if (!list.Contains(firstKey))
                    {
                        list.Add(firstKey);
                    }
                    detailDict[firstKey] = detailList;
                }

                /* 移除 'value' ；保留 'key' 做下一次迭代 */
                LuaDLL.lua_pop(L, 1);
            }
            LuaDLL.lua_settop(L, oldTop);
        }

        public static void DiffServer()
        {
            NetWorkClient.SendMessage(Diff());
        }

        public static void MarkRecordServer()
        {
            NetWorkClient.SendMessage(Record());
        }

        public static void MarkStaticServer()
        {
            NetWorkClient.SendMessage(Record());
        }

        public static LuaDiffInfo Diff()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            isHook = false;
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);


            if (staticHistoryRef == -100)
            {
                Debug.LogError("has no history");
                return null;
            }

            if (historyRef == -100)
            {
                Debug.LogError("has no history");
                return null;
            }

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_diff");
            LuaDLL.lua_getref(L, historyRef);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE &&
                LuaDLL.lua_type(L, -2) != LuaTypes.LUA_TTABLE)
            {
                Debug.LogError(LuaDLL.lua_type(L, -1));
                LuaDLL.lua_settop(L, oldTop);
                historyRef = -100;
                staticHistoryRef = -100;
                return null;
            }

            if (LuaDLL.lua_pcall(L, 2, 3, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int rmRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int addRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);
            SetTable(rmRef, ld.rmRef, ld.rmDetail);
            SetTable(addRef, ld.addRef, ld.addDetail);

            LuaDLL.lua_unref(L, nullObjectRef);
            LuaDLL.lua_unref(L, rmRef);
            LuaDLL.lua_unref(L, addRef);
            LuaDLL.lua_settop(L, oldTop);

            isHook = true;

            return ld;
        }
        #endregion

        #region luastring
        public static readonly Dictionary<long, object> stringDict = new Dictionary<long, object>();
        public static bool TryGetLuaString(IntPtr p, out object result)
        {
            return stringDict.TryGetValue(p.ToInt64(), out result);
        }
        public static void RefString(IntPtr strPoint, int index, object s, IntPtr L)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            //把字符串ref了之后就不GC了
            LuaDLL.lua_getglobal(L, "MikuLuaProfilerStrTb");
            int len = LuaDLL.lua_getobjlen(L, -1);
            LuaDLL.lua_pushnumber(L, len + 1);
            LuaDLL.lua_pushvalue(L, index);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_settop(L, oldTop);
            stringDict[(long)strPoint] = s;
        }
        public static string GetRefString(IntPtr L, int index)
        {
            IntPtr len;
            IntPtr intPtr = LuaDLL.lua_tolstring(L, index, out len);
            object text;
            if (!TryGetLuaString(intPtr, out text))
            {
                string tmpText = LuaDLL.lua_tostring(L, index);
                text = string.IsNullOrEmpty(tmpText) ? "nil" : string.Intern(tmpText);
                RefString(intPtr, index, text, L);
            }
            return (string)text;
        }
        #endregion
    }
}