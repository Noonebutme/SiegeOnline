﻿//-----------------------------------------------------------------------------------------------------------
// Copyright (C) 2015-2016 SiegeOnline
// 版权所有
//
// 文件名：PlayerData.cs
//
// 文件功能描述：
//
// 玩家数据缓存
//
// 创建标识：taixihuase 20150729
//
// 修改标识：
// 修改描述：
// 
//
// 修改标识：
// 修改描述：
//
//----------------------------------------------------------------------------------------------------------

using SiegeOnlineServer.Protocol.Common.Character;
using SiegeOnlineServer.Protocol.Common.User;
// ReSharper disable CheckNamespace
// ReSharper disable EmptyConstructor

namespace SiegeOnlineClient.Data.Player
{
    /// <summary>
    /// 类型：类
    /// 名称：PlayerData
    /// 作者：taixihuase
    /// 作用：玩家数据缓存类
    /// 编写日期：2015/7/29
    /// </summary>
    public class PlayerData
    {
        public UserInfo User { get; protected set; }

        public Character CharacterOriginal { get; protected set; }

        public Character CharacterCopy { get; set; }

        public void SetUserInfo(UserInfo user)
        {
            User = user;
        }

        public void SetCharacterOriginal(Character original)
        {
            CharacterOriginal = new Character(original);
        }

        public void SetCharacterCopy(Character copy)
        {
            CharacterCopy = new Character(copy);
        }

        public void SetCharacter(Character original, Character copy)
        {
            SetUserInfo(new UserInfo(original.Guid, original.Account, original.UniqueId, original.Nickname, original.Status));
            SetCharacterOriginal(original);
            SetCharacterCopy(copy);
        }
    }
}

