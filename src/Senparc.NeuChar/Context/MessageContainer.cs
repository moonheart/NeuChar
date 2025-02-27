﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2021 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2022 Senparc
    
    文件名：MessageContainer.cs
    文件功能描述：微信消息容器
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口

    修改标识：Senparc - 20190914
    修改描述：v0.8.0 提供支持分布式缓存的消息上下文（MessageContext）

----------------------------------------------------------------*/

using Senparc.NeuChar.Entities;
using System.Collections.Generic;

namespace Senparc.NeuChar.Context
{
    /// <summary>
    /// 消息容器（列表）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageContainer<T> : List<T>
        where T : IMessageBase
    {
        /// <summary>
        /// 最大记录条数（保留尾部），如果小于等于0则不限制
        /// </summary>
        public int MaxRecordCount { get; set; }

        public MessageContainer()
        {
        }

        public MessageContainer(int maxRecordCount)
        {
            MaxRecordCount = maxRecordCount;
        }

        public new void Add(T item)
        {
            base.Add(item);
            RemoveExpressItems();
        }

        /// <summary>
        /// 移除超出限制的上下文记录
        /// </summary>
        private void RemoveExpressItems()
        {
            //说明：为了提高效率，这里不加锁
            if (MaxRecordCount > 0 && base.Count > MaxRecordCount)
            {
                base.RemoveRange(0, base.Count - MaxRecordCount);
            }
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            RemoveExpressItems();
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            RemoveExpressItems();
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
            RemoveExpressItems();
        }
    }
}
