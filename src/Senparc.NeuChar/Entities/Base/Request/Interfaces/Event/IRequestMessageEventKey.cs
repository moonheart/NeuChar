﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2018 Suzhou Senparc Network Technology Co.,Ltd.

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
    
    文件名：IRequestMessageEventKey.cs
    文件功能描述：具有EventKey属性的RequestMessage接口
    
    
    创建标识：Senparc - 20181006
    
----------------------------------------------------------------*/

namespace Senparc.NeuChar.Entities
{
    /// <summary>
    /// 具有EventKey属性的RequestMessage接口
    /// </summary>
    public interface IRequestMessageEventKey
    {
        /// <summary>
        /// EventKey值
        /// </summary>
        string EventKey { get; set; }
    }
}
