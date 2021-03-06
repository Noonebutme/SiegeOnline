﻿//-----------------------------------------------------------------------------------------------------------
// Copyright (C) 2015-2016 SiegeOnline
// 版权所有
//
// 文件名：PlayerCollection.cs
//
// 文件功能描述：
//
// 玩家角色数据集合，存放游戏中的角色对象信息及进行角色信息的基本操作
//
// 创建标识：taixihuase 20150722
//
// 修改标识：
// 修改描述：
// 
//
// 修改标识：
// 修改描述：
//
//-----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using SiegeOnlineServer.Protocol.Common.Character;
using SiegeOnlineServer.Protocol.Common.User;

namespace SiegeOnlineServer.Collection
{
    /// <summary>
    /// 类型：类
    /// 名称：CharacterCollection
    /// 作者：taixihuase
    /// 作用：保存当前正在进行的游戏角色信息
    /// 编写日期：2015/7/22
    /// </summary>
    public class CharacterCollection
    {
        // 正在进行游戏的客户端列表
        public List<ServerPeer> GamingClientsToBroadcast { get; set; }

        // 从编号获得角色初始信息
        protected Dictionary<int, Character> UniqueIdToCharacterOriginal { get; set; }

        // 从编号获得角色加成信息
        protected Dictionary<int, Character>  UniqueIdToCharacterCopy { get; set; }

        // 服务端
        public readonly ServerApplication Server;

        /// <summary>
        /// 类型：方法
        /// 名称：CharacterCollection
        /// 作者：taixihuase
        /// 作用：构造 CharacterCollection 对象
        /// 编写日期：2015/7/22
        /// </summary>
        public CharacterCollection(ServerApplication server)
        {
            GamingClientsToBroadcast = new List<ServerPeer>();
            UniqueIdToCharacterOriginal = new Dictionary<int, Character>();
            UniqueIdToCharacterCopy = new Dictionary<int, Character>();
            Server = server;
        }

        /// <summary>
        /// 类型：类
        /// 名称：CharacterReturn
        /// 作者：taixihuase
        /// 作用：返回用户登录信息
        /// 编写日期：2015/7/22
        /// </summary>
        [Serializable]
        public class CharacterReturn
        {
            // 回传码
            public ReturnCodeType ReturnCode { get; set; }

            // 回传字串
            public StringBuilder DebugMessage { get; set; }

            /// <summary>
            /// 类型：枚举
            /// 名称：ReturnCodeTypes
            /// 作者：taixihuase
            /// 作用：角色查询操作回传码枚举
            /// 编写日期：2015/8/1
            /// </summary>
            [Serializable]
            public enum ReturnCodeType : byte
            {
                Default = 0, // 初始默认值
                Success = 1, // 获取资料成功
                CharacterNotFound = 2, // 无角色信息
            }

            /// <summary>
            /// 类型：方法
            /// 名称：CharacterReturn
            /// 作者：taixihuase
            /// 作用：构造 CharacterReturn 对象
            /// 编写日期：2015/7/22
            /// </summary>
            public CharacterReturn()
            {
                ReturnCode = ReturnCodeType.Default;
                DebugMessage = new StringBuilder();
            }
        }

        /// <summary>
        /// 类型：方法
        /// 名称：AddGamingCharacter
        /// 作者：taixihuase
        /// 作用：添加一个正在游戏的客户端连接
        /// 编写日期：2015/7/22
        /// </summary>
        /// <param name="peer"></param>
        public void AddGamingCharacter(ServerPeer peer)
        {
            GamingClientsToBroadcast.Add(peer);
        }

        /// <summary>
        /// 类型：方法
        /// 名称：RemoveCharacter
        /// 作者：taixihuase
        /// 作用：删除一个正在游戏中的角色连接
        /// 编写日期：2015/7/22
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveCharacter(Guid guid)
        {
            ServerPeer peer = Server.Users.TryGetPeer(guid);
            if (peer != null)
            {
                if (GamingClientsToBroadcast.Remove(peer))
                {
                    int id = Server.Users.GetUniqueIdFromGuid(guid);
                    if (id >= 0)
                    {
                        UniqueIdToCharacterOriginal.Remove(id);
                        UniqueIdToCharacterCopy.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// 类型：方法
        /// 名称：SearchCharacter
        /// 作者：taixihuase
        /// 作用：尝试搜索一个角色，同时返回搜索情况
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public CharacterReturn SearchCharacter(Character character)
        {
            CharacterReturn characterReturn = Server.Data.CharacterData.GetCharacter(character);

            return characterReturn;
        }

        public bool CharacterLoad(Character character)
        {
            lock (this)
            {
                if (!UniqueIdToCharacterOriginal.ContainsKey(character.UniqueId))
                {
                    UniqueIdToCharacterOriginal.Add(character.UniqueId, character);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 类型：方法
        /// 名称：CharacterEnter
        /// 作者：taixihuase
        /// 作用：添加一个角色连接
        /// 编写日期：2015/7/22
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="copy"></param>
        /// <returns></returns>
        public bool CharacterEnter(int uniqueId, out Character copy)
        {
            lock (this)
            {
                Character original;
                if (GetCharacterOriginal(uniqueId, out original))
                {
                    original.WorldEnterTime = DateTime.Now;
                    original.Status = UserInfo.StatusType.Gaming;
                    copy = new Character(original);
                    copy.ApplyEquipments();
                    UniqueIdToCharacterCopy.Add(copy.UniqueId, copy);
                    AddGamingCharacter(Server.Users.TryGetPeer(copy.Guid));
                    return true;
                }
                copy = null;
                return false;
            }
        }

        /// <summary>
        /// 类型：方法
        /// 名称：CharacterExit
        /// 作者：taixihuase
        /// 作用：登出一个角色并移除连接
        /// 编写日期：2015/7/23
        /// </summary>
        /// <param name="guid"></param>
        public void CharacterExit(Guid guid)
        {
            lock (this)
            {
                RemoveCharacter(guid);
            }
        }

        /// <summary>
        /// 类型：方法
        /// 名称：GetCharacterOriginal
        /// 作者：taixihuase
        /// 作用：通过编号获取角色初始信息
        /// 编写日期：2015/7/22
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool GetCharacterOriginal(int uniqueId, out Character character)
        {
            return UniqueIdToCharacterOriginal.TryGetValue(uniqueId, out character);
        }

        /// <summary>
        /// 类型：方法
        /// 名称：GetCharacterCopy
        /// 作者：taixihuase
        /// 作用：通过编号获取角色加成信息
        /// 编写日期：2015/8/20
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool GetCharacterCopy(int uniqueId, out Character character)
        {
            return UniqueIdToCharacterCopy.TryGetValue(uniqueId, out character);
        }

        /// <summary>
        /// 类型：方法
        /// 名称：GetCharacterOriginalFromNickName
        /// 作者：taixihuase
        /// 作用：通过昵称获取用户初始信息
        /// 编写日期：2015/7/22
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool GetCharacterOriginalFromNickname(string nickName, out Character character)
        {
            int id = Server.Users.GetUniqueIdFromNickname(nickName);
            if (id >= 0 && UniqueIdToCharacterOriginal.ContainsKey(id))
            {
                return GetCharacterOriginal(id, out character);
            }
            character = null;
            return false;
        }

        /// <summary>
        /// 类型：方法
        /// 名称：GetCharacterCopyFromNickName
        /// 作者：taixihuase
        /// 作用：通过昵称获取用户加成信息
        /// 编写日期：2015/8/20
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool GetCharacterCopyFromNickname(string nickName, out Character character)
        {
            int id = Server.Users.GetUniqueIdFromNickname(nickName);
            if (id >= 0 && UniqueIdToCharacterCopy.ContainsKey(id))
            {
                return GetCharacterCopy(id, out character);
            }
            character = null;
            return false;
        }
    }
}
