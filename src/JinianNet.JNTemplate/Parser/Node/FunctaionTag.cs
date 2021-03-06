﻿/*****************************************************
   Copyright (c) 2013-2015 jiniannet (http://www.jiniannet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

   Redistributions of source code must retain the above copyright notice
 *****************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;


namespace JinianNet.JNTemplate.Parser.Node
{
    /// <summary>
    /// 函数（方法）标签
    /// </summary>
    public class FunctaionTag : SimpleTag
    {
        private String name;
        /// <summary>
        /// 方法名
        /// </summary>
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private Object Excute(Object value, TemplateContext context,Object[] args,Type[] types)
        {
            if (value != null)
            {

                //MethodInfo method = null;// ParserAccessor.GetMethod(value.GetType(), list[list.Length - 1], types);F:\Work\工作区\我的项目\JinianNet.JNTemplate\JinianNet.JNTemplate1.2\src\JinianNet.JNTemplate\Parser\Node\FunctaionTag.cs

                FuncHandler handler = value as FuncHandler;
                //判断是否委托
                if (handler!=null)
                {
                    return handler.Invoke(args);
                }



                //不是委托，则在取方法
                MethodInfo method = Common.ReflectionHelpers.GetMethod( value.GetType(),this.Name, types);
                if (method != null)
                {
                    return method.Invoke(value, args);
                }
            }
            return null;
        }
        /// <summary>
        /// 解析标签
        /// </summary>
        /// <param name="context">上下文</param>
        public override Object Parse(TemplateContext context)
        {
            Object[] args = new Object[this.Children.Count];
            Type[] argsType = new Type[this.Children.Count];
            for (Int32 i = 0; i < this.Children.Count; i++)
            {
                args[i] = this.Children[i].Parse(context);
                argsType[i] = args[i].GetType();
            }

            return Excute(context.TempData[this.Name], context, args, argsType);
        }
        /// <summary>
        /// 解析标签
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="baseValue">baseValue</param>
        public override Object Parse(Object baseValue, TemplateContext context)
        {
            if (baseValue != null)
            {
                Object[] args = new Object[this.Children.Count];
                Type[] argsType = new Type[this.Children.Count];
                for (Int32 i = 0; i < this.Children.Count; i++)
                {
                    args[i] = this.Children[i].Parse(context);
                    if (args[i] != null)
                    {
                        argsType[i] = args[i].GetType();
                    }
                    else
                    {

                    }
                }


                Object result = Excute(baseValue, context, args, argsType);
                if (result != null)
                {
                    return result;
                }
                result = Common.ReflectionHelpers.GetPropertyValue(baseValue, this.Name);
                if (result != null && result is FuncHandler)
                {
                    return (result as FuncHandler).Invoke(args);
                }
            }

            return null;
        }

    }
}