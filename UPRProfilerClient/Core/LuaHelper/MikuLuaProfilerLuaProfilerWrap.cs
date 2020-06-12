using System;
using UnityEngine;

namespace UPRLuaProfiler
{
    public sealed class MikuLuaProfilerLuaProfilerWrap
    {
        public static LuaDeepProfilerSetting setting = LuaDeepProfilerSetting.Instance;
        public static LuaCSFunction beginSample = new LuaCSFunction(BeginSample);
        public static LuaCSFunction beginSampleCustom = new LuaCSFunction(BeginSampleCustom);
        public static LuaCSFunction endSample = new LuaCSFunction(EndSample);
        public static LuaCSFunction unpackReturnValue = new LuaCSFunction(UnpackReturnValue);
        public static LuaCSFunction addRefFunInfo = new LuaCSFunction(AddRefFunInfo);
        public static LuaCSFunction removeRefFunInfo = new LuaCSFunction(RemoveRefFunInfo);
        public static LuaCSFunction checkType = new LuaCSFunction(CheckType);
        public static LuaCSFunction handleError = new LuaCSFunction(HandleError);
        public static void __Register(IntPtr L)
        {
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushstring(L, "LuaProfiler");
            LuaDLL.lua_newtable(L);

            LuaDLL.lua_pushstring(L, "BeginSample");
            LuaDLL.lua_pushstdcallcfunction(L, beginSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "EndSample");
            LuaDLL.lua_pushstdcallcfunction(L, endSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "BeginSampleCustom");
            LuaDLL.lua_pushstdcallcfunction(L, beginSampleCustom);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "EndSampleCustom");
            LuaDLL.lua_pushstdcallcfunction(L, endSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_rawset(L, -3);
            LuaDLL.lua_setglobal(L, "UPRLuaProfiler");

            LuaDLL.lua_pushstdcallcfunction(L, unpackReturnValue);
            LuaDLL.lua_setglobal(L, "miku_unpack_return_value");

            LuaDLL.lua_pushstdcallcfunction(L, addRefFunInfo);
            LuaDLL.lua_setglobal(L, "miku_add_ref_fun_info");

            LuaDLL.lua_pushstdcallcfunction(L, removeRefFunInfo);
            LuaDLL.lua_setglobal(L, "miku_remove_ref_fun_info");

            LuaDLL.lua_pushstdcallcfunction(L, checkType);
            LuaDLL.lua_setglobal(L, "miku_check_type");

            LuaDLL.lua_pushstdcallcfunction(L, handleError);
            LuaDLL.lua_setglobal(L, "miku_handle_error");

            LuaDLL.lua_newtable(L);
            LuaDLL.lua_setglobal(L, "MikuLuaProfilerStrTb");

            LuaLib.DoString(L, get_ref_string);
            LuaLib.DoString(L, null_script);
            LuaLib.DoString(L, diff_script);
        }

        #region script
        const string get_ref_string = @"
local weak_meta_table = {__mode = 'k'}
local infoTb = {}
local funAddrTb = {}
setmetatable(infoTb, weak_meta_table)
setmetatable(funAddrTb, weak_meta_table)

function miku_get_fun_info(fun)
    local result = infoTb[fun]
    local addr = funAddrTb[fun]
    if not result then
        local info = debug.getinfo(fun, 'Sl')
        result = string.format('function:%s&line:%d', info.source, info.linedefined)
        addr = string.sub(tostring(fun), 11)
        infoTb[fun] = result
        funAddrTb[fun] = addr
    end
    return result,addr
end

local function serialize(obj)
    if obj == _G then
        return '_G'
    end
    local lua = ''
    lua = lua .. '{\n'
    local count = 0
    for k, v in pairs(obj) do
        lua = lua .. '[' .. tostring(tostring(k)) .. ']=' .. tostring(tostring(v)) .. ',\n'
        count = count + 1
        if count > 5 then
            break
        end
    end
    lua = lua .. '}'
    if lua == '{\n}' then
        lua = tostring(obj)
    end
    return lua
end

local function get_table_info(tb)
    local result = infoTb[tb]
    local addr = funAddrTb[tb]
    if not result then
        local tostringFun
        local metaTb = getmetatable(tb)
        if metaTb and miku_check_type(metaTb) == 2 and rawget(metaTb, '__tostring') then
            tostringFun = rawget(metaTb, '__tostring')
            rawset(metaTb, '__tostring', nil)
        end
        local addStr = tostring(tb)
        if tostringFun then
            rawset(getmetatable(tb), '__tostring', tostringFun)
        end
        result = rawget(tb, '__name') or rawget(tb, 'name') or rawget(tb, '__cname') or rawget(tb, '.name')
        if not result then
            result = serialize(tb)
        end

        addr = string.sub(addStr, 7)
        infoTb[tb] = result
        funAddrTb[tb] = addr
    end
    return result,addr
end

function lua_miku_add_ref_fun_info(data)
    local result = ''
    local addr = ''
    local t = 1
    local typeStr = miku_check_type(data)
    if typeStr == 1 then
        result,addr = miku_get_fun_info(data)
        t = 1
    elseif typeStr == 2 then
        result,addr = get_table_info(data)
        t = 2
    end
    miku_add_ref_fun_info(result, addr, t)
end

function lua_miku_remove_ref_fun_info(data)
    local result = infoTb[data]
    local addr = funAddrTb[data]
    local typeStr = miku_check_type(data)
    local t = 1
    if typeStr == 1 then
        t = 1
    elseif typeStr == 2 then
        t = 2
    end

    miku_remove_ref_fun_info(result, addr, t)
end
";
        const string null_script = @"
function miku_is_null(val)
    local metaTable = getmetatable(val)
    if type(metaTable) == 'table' and metaTable.__index and val.Equals then
        local status,retval = pcall(val.Equals, val, nil)
        if status then
            return retval
        else
            return true
        end
    end
    return false
end
";
        const string diff_script = @"
local weak_meta_key_table = {__mode = 'k'}
local weak_meta_value_table = {__mode = 'v'}
local infoTb = {}
local cache_key = 'miku_record_prefix_cache'

function miku_do_record(val, prefix, key, record, history, null_list, staticRecord)
    if val == staticRecord then
        return
    end
    if val == infoTb then
        return
    end
    if val == miku_do_record then
        return
    end
    if val == miku_diff then
        return
    end
    if val == lua_miku_remove_ref_fun_info then
        return
    end
    if val == lua_miku_add_ref_fun_info then
        return
    end
    if val == history then
        return
    end
    if val == record then
        return
    end
    if val == miku_get_fun_info then
        return
    end
    if val == MikuLuaProfilerStrTb then
        return
	end

    if getmetatable(record) ~= weak_meta_key_table then
        setmetatable(record, weak_meta_key_table)
    end

    local typeStr = type(val)
    if typeStr ~= 'table' and typeStr ~= 'userdata' and typeStr ~= 'function' then
        return
    end

    local tmp_prefix
    local strKey = tostring(key)
    if not strKey then
        strKey = 'empty'
    end
    local prefixTb = infoTb[prefix]
    if not prefixTb then
        prefixTb = {}
        infoTb[prefix] = prefixTb
    end
    tmp_prefix = prefixTb[strKey]
    if not tmp_prefix then
        tmp_prefix = prefix.. (prefix == '' and '' or '.') .. strKey
        prefixTb[strKey] = tmp_prefix
    end

    if null_list then
        if type(val) == 'userdata' then
            local st,ret = pcall(miku_is_null, val)
            if st and ret then
                if null_list[val] == nil then
                    null_list[val] = { }
                end
                table.insert(null_list[val], tmp_prefix)
            end
        end
    end

    if record[val] then
        table.insert(record[val], tmp_prefix)
        return
    end

    local prefix_cache
    if history == nil then
        if record[cache_key] == nil then
            record[cache_key] = {}
        end
        prefix_cache = record[cache_key]
        prefix_cache[tmp_prefix] = tmp_prefix
        local record_val = record[val]
        if record_val == nil then
            record_val = {}
            if typeStr == 'function' then
                local funInfo = miku_get_fun_info(val)
                table.insert(record_val, funInfo)
            end
            record[val] = record_val
        end
        table.insert(record_val, tmp_prefix)
    else
        prefix_cache = history[cache_key]
        if prefix_cache[tmp_prefix] == nil or history[val] then
            local record_val = record[val]
            if record_val == nil then
                record_val = {}
                if typeStr == 'function' then
                    local funInfo = miku_get_fun_info(val)
                    table.insert(record_val, funInfo)
                end
                record[val] = record_val
            end
            table.insert(record_val, tmp_prefix)
        end
    end

    if typeStr == 'table' then
        for k,v in pairs(val) do
            local typeKStr = type(k)
            local typeVStr = type(v)
            local key = k
            if typeKStr == 'table' or typeKStr == 'userdata' or typeKStr == 'function' then
                key = 'table:'
                miku_do_record(k, tmp_prefix, 'table:', record, history, null_list, staticRecord)
            end
            miku_do_record(v, tmp_prefix, key, record, history, null_list, staticRecord)
        end

    elseif typeStr == 'function' then
        if val ~= lua_miku_add_ref_fun_info and val ~= lua_miku_remove_ref_fun_info then
            local i = 1
            while true do
                local k, v = debug.getupvalue(val, i)
                if not k then
                    break
                end
                if v then
                    local funPrefix = miku_get_fun_info(val)
                    miku_do_record(v, funPrefix, k, record, history, null_list, staticRecord)
                end
                i = i + 1
            end
        end
    end

    local metaTable = getmetatable(val)
    if metaTable then
        miku_do_record(metaTable, tmp_prefix, 'metaTable', record, history, null_list, staticRecord)
    end
end

-- staticRecord为打开UI前的快照， record为打开UI后的快照，add为关闭并释放UI后的快照
function miku_diff(record, staticRecord)
    local add = { }
    setmetatable(add, weak_meta_key_table)
    local null_list = { }
    setmetatable(null_list, weak_meta_key_table)
    miku_do_record(_G, '', '_G', add, record, null_list, staticRecord)
    miku_do_record(debug.getregistry(), '', '_R', add, record, null_list, staticRecord)
    local remain = { }

    for key, val in pairs(record) do
        if not add[key] and key ~= cache_key then
        else
            -- 如果打开UI前的快照没有这个数据
            -- 但是打开UI后及关闭并释放UI后的快照都拥有这个数据，视为泄漏
            if not staticRecord[key] and key ~= staticRecord and key ~= cache_key  then
                remain[key] = val
            end
            add[key] = nil
        end
    end

    return add,remain,null_list
end";
        #endregion

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaHook.GetRefString(L, 1));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSampleCustom(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaHook.GetRefString(L, 1), true);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UnpackReturnValue(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return LuaDLL.lua_gettop(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int CheckType(IntPtr L)
        {
            if (LuaDLL.lua_isfunction(L, 1))
            {
                LuaDLL.lua_pushnumber(L, 1);
            }
            else if (LuaDLL.lua_istable(L, 1))
            {
                LuaDLL.lua_pushnumber(L, 2);
            }
            else
            {
                LuaDLL.lua_pushnumber(L, 0);
            }
            return 1;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int AddRefFunInfo(IntPtr L)
        {
            string funName = LuaHook.GetRefString(L, 1);
            string funAddr = LuaHook.GetRefString(L, 2);
            byte type = (byte)LuaDLL.lua_tonumber(L, 3);
            LuaProfiler.AddRef(funName, funAddr, type);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int RemoveRefFunInfo(IntPtr L)
        {
            string funName = LuaHook.GetRefString(L, 1);
            string funAddr = LuaHook.GetRefString(L, 2);
            byte type = (byte)LuaDLL.lua_tonumber(L, 3);
            LuaProfiler.RemoveRef(funName, funAddr, type);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int HandleError(IntPtr L)
        {
            string error = LuaHook.GetRefString(L, 1);
            Debug.LogError(error);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return 0;
        }
    }
}