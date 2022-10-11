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
    
    文件名：EntityHelper.cs
    文件功能描述：实体与xml相互转换
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
    
    修改标识：Senparc - 20170810
    修改描述：v14.5.9 提取EntityHelper.FillClassValue()方法，优化FillEntityWithXml()方法

    修改标识：Senparc - 20180901
    修改描述：优化FillEntityWithXml()方法

    修改标识：Senparc - 20190529
    修改描述：FillEntityWithXml()方法添加 "ThirdFasteRegisterInfo" 类型: 开放平台-小程序-快速注册

    修改标识：ccccccmd - 20201013
    修改描述：v1.2.201 MASSSENDJOBFINISH 事件增加 ArticleUrlResult 节点

    修改标识：Billzjh - 20201210
    修改描述：v1.3.200 添加企业微信推广码注册对应转换方法

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.MessageHandlers;
using Senparc.NeuChar.NeuralSystems;

namespace Senparc.NeuChar.Helpers
{
    /// <summary>
    /// 实体帮助类
    /// </summary>
    public static class EntityHelper
    {
        /// <summary>
        /// 根据XML信息填充实实体
        /// </summary>
        /// <typeparam name="T">MessageBase为基类的类型，Response和Request都可以</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="doc">XML</param>
        public static void FillEntityWithXml<T>(this T entity, XDocument doc) where T : /*MessageBase*/ class, new()
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var root = doc.Root;
            if (doc.Root == null)
            {
                throw new ArgumentNullException(nameof(doc.Root));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            //entity = entity ?? new T();

            var props = entity.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite)
                {
                    continue;//如果不可读则跳过
                }

                var propName = prop.Name;
                if (root.Element(propName) != null)
                {
                    switch (prop.PropertyType.Name)
                    {
                        //case "String":
                        //    goto default;
                        case "DateTime":
                        case "DateTimeOffset":
                        case "Int32":
                        case "Int64":
                        case "Double":
                        case "Nullable`1": //可为空对象
                            EntityUtility.FillSystemType(entity, prop, root.Element(propName).Value);
                            break;
                        case "Boolean":
                            if (propName == "FuncFlag")
                            {
                                EntityUtility.FillSystemType(entity, prop, root.Element(propName).Value == "1");
                            }
                            else
                            {
                                goto default;
                            }
                            break;

                        //以下为枚举类型
                        case "RequestMsgType":
                            //已设为只读
                            //prop.SetValue(entity, MsgTypeHelper.GetRequestMsgType(root.Element(propName).Value), null);
                            break;
                        case "ResponseMsgType": //Response适用
                            //已设为只读
                            //prop.SetValue(entity, MsgTypeHelper.GetResponseMsgType(root.Element(propName).Value), null);
                            break;
                        case "Event":
                            //已设为只读
                            //prop.SetValue(entity, EventHelper.GetEventType(root.Element(propName).Value), null);
                            break;
                        //以下为实体类型
                        case "List`1": //List<T>类型，ResponseMessageNews适用
                            {
                                var genericArguments = prop.PropertyType.GetGenericArguments();
                                var genericArgumentTypeName = genericArguments[0].Name;
                                if (genericArgumentTypeName == "Article")
                                {
                                    //文章下属节点item
                                    List<Article> articles = new List<Article>();
                                    foreach (var item in root.Element(propName).Elements("item"))
                                    {
                                        var article = new Article();
                                        FillEntityWithXml(article, new XDocument(item));
                                        articles.Add(article);
                                    }
                                    prop.SetValue(entity, articles, null);
                                }
                                else if (genericArgumentTypeName == "Account")
                                {
                                    List<CustomerServiceAccount> accounts = new List<CustomerServiceAccount>();
                                    foreach (var item in root.Elements(propName))
                                    {
                                        var account = new CustomerServiceAccount();
                                        FillEntityWithXml(account, new XDocument(item));
                                        accounts.Add(account);
                                    }
                                    prop.SetValue(entity, accounts, null);
                                }
                                else if (genericArgumentTypeName == "PicItem")
                                {
                                    List<PicItem> picItems = new List<PicItem>();
                                    foreach (var item in root.Elements(propName).Elements("item"))
                                    {
                                        var picItem = new PicItem();
                                        var picMd5Sum = item.Element("PicMd5Sum").Value;
                                        Md5Sum md5Sum = new Md5Sum() { PicMd5Sum = picMd5Sum };
                                        picItem.item = md5Sum;
                                        picItems.Add(picItem);
                                    }
                                    prop.SetValue(entity, picItems, null);
                                }
                                else if (genericArgumentTypeName == "AroundBeacon")
                                {
                                    List<AroundBeacon> aroundBeacons = new List<AroundBeacon>();
                                    foreach (var item in root.Elements(propName).Elements("AroundBeacon"))
                                    {
                                        var aroundBeaconItem = new AroundBeacon();
                                        FillEntityWithXml(aroundBeaconItem, new XDocument(item));
                                        aroundBeacons.Add(aroundBeaconItem);
                                    }
                                    prop.SetValue(entity, aroundBeacons, null);
                                }
                                else if (genericArgumentTypeName == "CopyrightCheckResult_ResultList")
                                {
                                    List<CopyrightCheckResult_ResultList> resultList = new List<CopyrightCheckResult_ResultList>();
                                    foreach (var item in root.Elements("ResultList").Elements("item"))
                                    {
                                        CopyrightCheckResult_ResultList resultItem = new CopyrightCheckResult_ResultList();
                                        FillEntityWithXml(resultItem.item, new XDocument(item));
                                        resultList.Add(resultItem);
                                    }
                                    prop.SetValue(entity, resultList, null);
                                }
                                else if (genericArgumentTypeName == "ArticleUrlResult_ResultList")
                                {
                                    List<ArticleUrlResult_ResultList> resultList = new List<ArticleUrlResult_ResultList>();
                                    foreach (var item in root.Elements("ResultList").Elements("item"))
                                    {
                                        ArticleUrlResult_ResultList resultItem = new ArticleUrlResult_ResultList();
                                        FillEntityWithXml(resultItem.item, new XDocument(item));
                                        resultList.Add(resultItem);
                                    }
                                    prop.SetValue(entity, resultList, null);
                                }
                                else if (genericArgumentTypeName == nameof(SubscribeMsgChangeEvent))
                                {
                                    List<SubscribeMsgChangeEvent> list = new List<SubscribeMsgChangeEvent>();
                                    foreach (var item in root.Element(propName).Elements("List"))
                                    {
                                        SubscribeMsgChangeEvent resultItem = new SubscribeMsgChangeEvent();
                                        FillEntityWithXml(resultItem, new XDocument(item));
                                        list.Add(resultItem);
                                    }
                                    prop.SetValue(entity, list, null);
                                }

                                else if (genericArgumentTypeName == nameof(SubscribeMsgPopupEvent))
                                {
                                    List<SubscribeMsgPopupEvent> list = new List<SubscribeMsgPopupEvent>();
                                    foreach (var item in root.Element(propName).Elements("List"))
                                    {

                                        SubscribeMsgPopupEvent resultItem = new SubscribeMsgPopupEvent();
                                        FillEntityWithXml(resultItem, new XDocument(item));
                                        list.Add(resultItem);
                                    }
                                    prop.SetValue(entity, list, null);
                                }

                                else if (genericArgumentTypeName == nameof(SubscribeMsgSentEvent))
                                {
                                    List<SubscribeMsgSentEvent> list = new List<SubscribeMsgSentEvent>();
                                    foreach (var item in root.Element(propName).Elements("List"))
                                    {
                                        SubscribeMsgSentEvent resultItem = new SubscribeMsgSentEvent();
                                        FillEntityWithXml(resultItem, new XDocument(item));
                                        list.Add(resultItem);
                                    }
                                    prop.SetValue(entity, list, null);
                                }

                                else if (genericArgumentTypeName == nameof(String))
                                {
                                    List<string> list = new List<string>();
                                    foreach (var item in root.Elements(propName))
                                    {
                                        list.Add(item.Value);
                                    }
                                    prop.SetValue(entity, list, null);
                                }



                                //企业微信
                                else if (genericArguments[0].Name == "MpNewsArticle")
                                {
                                    List<MpNewsArticle> mpNewsArticles = new List<MpNewsArticle>();
                                    foreach (var item in root.Elements(propName))
                                    {
                                        var mpNewsArticle = new MpNewsArticle();
                                        FillEntityWithXml(mpNewsArticle, new XDocument(item));
                                        mpNewsArticles.Add(mpNewsArticle);
                                    }
                                    prop.SetValue(entity, mpNewsArticles, null);
                                }
                                else if (genericArguments[0].Name == "SelectedItem")
                                {
                                    List<SelectedItem> selectedItems = new List<SelectedItem>();
                                    foreach (var item in root.Elements(propName))
                                    {
                                        var selectedItem = new SelectedItem();
                                        FillEntityWithXml(selectedItem, new XDocument(item.Element("SelectedItem")));
                                        selectedItems.Add(selectedItem);
                                    }
                                    prop.SetValue(entity, selectedItems, null);
                                }
                                break;
                            }
                        case "Music"://ResponseMessageMusic适用
                            FillClassValue<Music>(entity, root, propName, prop);
                            break;
                        case "Image"://ResponseMessageImage适用
                            FillClassValue<Image>(entity, root, propName, prop);
                            break;
                        case "Voice"://ResponseMessageVoice适用
                            FillClassValue<Voice>(entity, root, propName, prop);
                            break;
                        case "Video"://ResponseMessageVideo适用
                            FillClassValue<Video>(entity, root, propName, prop);
                            break;
                        case "ScanCodeInfo"://扫码事件中的ScanCodeInfo适用
                            FillClassValue<ScanCodeInfo>(entity, root, propName, prop);
                            break;
                        case "SendLocationInfo"://弹出地理位置选择器的事件推送中的SendLocationInfo适用
                            FillClassValue<SendLocationInfo>(entity, root, propName, prop);
                            break;
                        case "SendPicsInfo"://系统拍照发图中的SendPicsInfo适用
                            FillClassValue<SendPicsInfo>(entity, root, propName, prop);
                            break;
                        case "ChosenBeacon"://摇一摇事件通知
                            FillClassValue<ChosenBeacon>(entity, root, propName, prop);
                            break;
                        case "AroundBeacon"://摇一摇事件通知
                            FillClassValue<AroundBeacon>(entity, root, propName, prop);
                            break;

                        #region 企业微信推广码注册
                        case "ContactSyncToken":
                            FillClassValue<ContactSyncToken>(entity, root, propName, prop);
                            break;
                        case "AuthUserInfoModel":
                            FillClassValue<AuthUserInfoModel>(entity, root, propName, prop);
                            break;
                        #endregion

                        #region 开放平台-小程序
                        case "ThirdFasteRegisterInfo": //开放平台-小程序-快速注册
                            FillClassValue<ThirdFasteRegisterInfo>(entity, root, propName, prop);
                            break;
                        #endregion

                        #region RequestMessageEvent_MassSendJobFinish
                        case "CopyrightCheckResult":
                            FillClassValue<CopyrightCheckResult>(entity, root, propName, prop);
                            break;
                        case "CopyrightCheckResult_ResultList_Item":
                            FillClassValue<CopyrightCheckResult_ResultList_Item>(entity, root, "item", prop);
                            break;
                        case "ArticleUrlResult":
                            FillClassValue<ArticleUrlResult>(entity, root, propName, prop);
                            break;
                        case "ArticleUrlResult_ResultList_Item":
                            FillClassValue<ArticleUrlResult_ResultList_Item>(entity, root, "item", prop);
                            break;


                        #region 企业号
                        /* 暂时放在Work.dll中
                                                case "AgentType":
                                                    {
                                                        AgentType tp;
                        #if NET35
                                                        try
                                                        {
                                                            tp = (AgentType)Enum.Parse(typeof(AgentType), root.Element(propName).Value, true);
                                                            prop.SetValue(entity, tp, null);
                                                        }
                                                        catch
                                                        {

                                                        }
                        #else
                                                        if (Enum.TryParse(root.Element(propName).Value, out tp))
                                                        {
                                                            prop.SetValue(entity, tp, null);
                                                        }
                        #endif
                                                        break;
                                                    }
                                                case "Receiver":
                                                    {
                                                        Receiver receiver = new Receiver();
                                                        FillEntityWithXml(receiver, new XDocument(root.Element(propName)));
                                                        prop.SetValue(entity, receiver, null);
                                                        break;
                                                    }
                                                    */
                        #endregion

                        #endregion

                        default:
                            var enumSuccess = false;
                            if (prop.PropertyType.IsEnum)
                            {
                                //未知的枚举类型
                                try
                                {
                                    prop.SetValue(entity, Enum.Parse(prop.PropertyType, root.Element(propName).Value, true), null);
                                    enumSuccess = true;
                                }
                                catch
                                {
                                }
                            }

                            if (!enumSuccess)
                            {
                                try
                                {
                                    //尝试以字符串形式赋值
                                    prop.SetValue(entity, root.Element(propName).Value, null);
                                }
                                catch
                                {
                                    try
                                    {
                                        //尝试以节点名称对应类型赋值（包括节点在内的完整内容）
                                        prop.SetValue(entity, XmlUtility.Deserialize(prop.PropertyType, root.Element(propName).ToString(), propName), null);
                                    }
                                    catch
                                    {
                                        throw;
                                    }

                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 填充复杂类型的参数
        /// </summary>
        /// <typeparam name="T">复杂类型</typeparam>
        /// <param name="entity">被填充实体</param>
        /// <param name="root">XML节点</param>
        /// <param name="childElementName">XML下一级节点的名称</param>
        /// <param name="prop">属性对象</param>
        public static void FillClassValue<T>(object entity, XElement root, string childElementName, PropertyInfo prop)
            where T : /*MessageBase*/ class, new()
        {
            T subType = new T();
            FillEntityWithXml(subType, new XDocument(root.Element(childElementName)));
            prop.SetValue(entity, subType, null);
        }

        /// <summary>
        /// 将实体转为XML
        /// </summary>
        /// <typeparam name="T">RequestMessage或ResponseMessage</typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static XDocument ConvertEntityToXml<T>(this T entity) where T : class
        {
            //entity = entity ?? new T();
            if (entity == null)
            {
                throw new Senparc.CO2NET.Exceptions.BaseException("entity 参数不能为 null");
            }

            //XmlSerializer xmldes = new XmlSerializer(typeof(T));


            var doc = new XDocument();

            doc.Add(new XElement("xml"));
            var root = doc.Root;

            /* 注意！
             * 经过测试，微信对字段排序有严格要求，这里对排序进行强制约束    —— 目前已经不敏感 20180904
            */
            //var propNameOrder = new List<string>() { "ToUserName", "FromUserName", "CreateTime", "MsgType" };
            //不同返回类型需要对应不同特殊格式的排序
            //if (entity is ResponseMessageNews)
            //{
            //    propNameOrder.AddRange(new[] { "ArticleCount", "Articles", "FuncFlag",/*以下是Atricle属性*/ "Title ", "Description ", "PicUrl", "Url" });
            //}
            //else if (entity is ResponseMessageTransfer_Customer_Service)
            //{
            //    propNameOrder.AddRange(new[] { "TransInfo", "KfAccount", "FuncFlag" });
            //}
            //else if (entity is ResponseMessageMusic)
            //{
            //    propNameOrder.AddRange(new[] { "Music", "FuncFlag", "ThumbMediaId",/*以下是Music属性*/ "Title ", "Description ", "MusicUrl", "HQMusicUrl" });
            //}
            //else if (entity is ResponseMessageImage)
            //{
            //    propNameOrder.AddRange(new[] { "Image",/*以下是Image属性*/ "MediaId " });
            //}
            //else if (entity is ResponseMessageVoice)
            //{
            //    propNameOrder.AddRange(new[] { "Voice",/*以下是Voice属性*/ "MediaId " });
            //}
            //else if (entity is ResponseMessageVideo)
            //{
            //    propNameOrder.AddRange(new[] { "Video",/*以下是Video属性*/ "MediaId ", "Title", "Description" });
            //}
            //else
            //{
            //    //如Text类型
            //    propNameOrder.AddRange(new[] { "Content", "FuncFlag" });
            //}

            //Func<string, int> orderByPropName = propNameOrder.IndexOf;

            var props = entity.GetType().GetProperties();//.OrderBy(p => orderByPropName(p.Name)).ToList();
            foreach (var prop in props)
            {
                var propName = prop.Name;
                if (propName == "Articles")
                {
                    //文章列表
                    var atriclesElement = new XElement("Articles");
                    var articales = prop.GetValue(entity, null) as List<Article>;
                    foreach (var articale in articales)
                    {
                        var subNodes = ConvertEntityToXml(articale).Root.Elements();
                        atriclesElement.Add(new XElement("item", subNodes));
                    }
                    root.Add(atriclesElement);
                }
                else if (propName == "TransInfo")
                {
                    var transInfoElement = new XElement("TransInfo");
                    var transInfo = prop.GetValue(entity, null) as List<CustomerServiceAccount>;
                    foreach (var account in transInfo)
                    {
                        var trans = ConvertEntityToXml(account).Root.Elements();
                        transInfoElement.Add(trans);
                    }

                    root.Add(transInfoElement);
                }
                else if (propName == "Music" || propName == "Image" || propName == "Video" || propName == "Voice")
                {
                    //音乐、图片、视频、语音格式
                    var musicElement = new XElement(propName);
                    var media = prop.GetValue(entity, null);// as Music;
                    var subNodes = ConvertEntityToXml(media).Root.Elements();
                    musicElement.Add(subNodes);
                    root.Add(musicElement);
                }
                else if (propName == "PicList")
                {
                    var picListElement = new XElement("PicList");
                    var picItems = prop.GetValue(entity, null) as List<PicItem>;
                    foreach (var picItem in picItems)
                    {
                        var item = ConvertEntityToXml(picItem).Root.Elements();
                        picListElement.Add(item);
                    }
                    root.Add(picListElement);
                }
                else if (propName == "KfAccount")
                {
                    //TODO:可以交给string处理
                    root.Add(new XElement(propName, prop.GetValue(entity, null).ToString().ToLower()));
                }
                else
                {
                    //其他非特殊类型
                    switch (prop.PropertyType.Name)
                    {
                        case "String":
                            root.Add(new XElement(propName, new XCData(prop.GetValue(entity, null) as string ?? "")));
                            break;
                        case "DateTime":
                            root.Add(new XElement(propName, DateTimeHelper.GetUnixDateTime(((DateTime)prop.GetValue(entity, null)))));
                            break;
                        case "DateTimeOffset":
                            root.Add(new XElement(propName, DateTimeHelper.GetUnixDateTime((DateTimeOffset)prop.GetValue(entity, null))));
                            break;
                        case "Boolean":
                            if (propName == "FuncFlag")
                            {
                                root.Add(new XElement(propName, (bool)prop.GetValue(entity, null) ? "1" : "0"));
                            }
                            else
                            {
                                goto default;
                            }
                            break;
                        case "ResponseMsgType":
                            root.Add(new XElement(propName, new XCData(prop.GetValue(entity, null).ToString().ToLower())));
                            break;
                        case "Article":
                            root.Add(new XElement(propName, prop.GetValue(entity, null).ToString().ToLower()));
                            break;
                        case "TransInfo":
                            root.Add(new XElement(propName, prop.GetValue(entity, null).ToString().ToLower()));
                            break;
                        default:
#if NET462
                            if (prop.PropertyType.IsClass && prop.PropertyType.IsPublic)
#else
                            if (prop.PropertyType.GetTypeInfo().IsClass && prop.PropertyType.GetTypeInfo().IsPublic)
#endif
                            {
                                //自动处理其他实体属性
                                var subEntity = prop.GetValue(entity, null);
                                var subNodes = ConvertEntityToXml(subEntity).Root.Elements();
                                root.Add(new XElement(propName, subNodes));
                            }
                            else
                            {
                                root.Add(new XElement(propName, prop.GetValue(entity, null)));

                            }
                            break;
                    }
                }
            }
            return doc;
        }

        /// <summary>
        /// 将实体转为XML字符串
        /// </summary>
        /// <typeparam name="T">RequestMessage或ResponseMessage</typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static string ConvertEntityToXmlString<T>(this T entity) where T : class
        {
            return entity.ConvertEntityToXml().ToString();
        }

        /// <summary>
        /// ResponseMessageBase.CreateFromRequestMessage&lt;T&gt;(requestMessage)的扩展方法
        /// </summary>
        /// <typeparam name="T">需要生成的ResponseMessage类型</typeparam>
        /// <param name="requestMessage">IRequestMessageBase接口下的接收信息类型</param>
        /// <param name="enlighten">MessageEntityEnlighten，当 T 为接口时必须提供</param>
        /// <returns></returns>
        public static T CreateResponseMessage<T>(this IRequestMessageBase requestMessage, MessageEntityEnlightener enlighten)
            where T : IResponseMessageBase
        {
            return ResponseMessageBase.CreateFromRequestMessage<T>(requestMessage, enlighten);
        }

        /// <summary>
        /// ResponseMessageBase.CreateFromRequestMessage&lt;T&gt;(requestMessage)的扩展方法
        /// </summary>
        /// <typeparam name="T">需要生成的ResponseMessage类型</typeparam>
        /// <param name="requestMessage">IRequestMessageBase接口下的接收信息类型</param>
        /// <returns></returns>
        public static T CreateResponseMessage<T>(this IRequestMessageBase requestMessage)
            where T : class, IResponseMessageBase //只有class才可以enlighten = null
        {
            return ResponseMessageBase.CreateFromRequestMessage<T>(requestMessage);
        }

        /// <summary>
        /// ResponseMessageBase.CreateFromRequestMessage&lt;T&gt;(requestMessage)的扩展方法
        /// </summary>
        /// <typeparam name="T">需要生成的ResponseMessage类型</typeparam>
        /// <param name="enlighten">MessageEntityEnlighten</param>
        /// <param name="requestMessage">IRequestMessageBase接口下的接收信息类型</param>
        /// <returns></returns>
        public static T CreateResponseMessage<T>(this MessageEntityEnlightener enlighten, IRequestMessageBase requestMessage)
            where T : IResponseMessageBase
        {
            return ResponseMessageBase.CreateFromRequestMessage<T>(requestMessage, enlighten);
        }

        /// <summary>
        /// ResponseMessageBase.CreateFromResponseXml(xml)的扩展方法
        /// </summary>
        /// <param name="xml">返回给服务器的Response Xml</param>
        /// <returns></returns>
        public static IResponseMessageBase CreateResponseMessage(this string xml, MessageEntityEnlightener enlighten)
        {
            return ResponseMessageBase.CreateFromResponseXml(xml, enlighten);
        }

        ///// <summary>
        ///// 检查是否是通过场景二维码扫入
        ///// </summary>
        ///// <param name="requestMessage"></param>
        ///// <returns></returns>
        //public static bool IsFromScene(this RequestMessageEvent_Subscribe requestMessage)
        //{
        //    return !string.IsNullOrEmpty(requestMessage.EventKey);
        //}
    }
}
