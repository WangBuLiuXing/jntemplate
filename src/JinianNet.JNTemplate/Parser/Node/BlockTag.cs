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
using System.Text;
using System.IO;

namespace JinianNet.JNTemplate.Parser.Node
{
    /// <summary>
    /// 标签块
    /// </summary>
    public class BlockTag : BaseTag
    {
        private String text;
        /// <summary>
        /// 模板上下文
        /// </summary>
        public String TemplateContent
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// 解析标签
        /// </summary>
        /// <param name="context">上下文</param>
        public override Object Parse(TemplateContext context)
        {
            using (System.IO.StringWriter writer = new StringWriter())
            {
                Render(context, writer);

                return writer.ToString();
            }
        }
        /// <summary>
        /// 解析标签
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="write">write</param>
        public override void Parse(TemplateContext context, TextWriter write)
        {
            Render(context, write);
        }
        /// <summary>
        /// 呈现标签
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="writer">writer</param>
        protected void Render(TemplateContext context, TextWriter writer)
        {
            if (context == null)
            {
                writer.Write(this.TemplateContent);
                return;
            }

            if (!String.IsNullOrEmpty(this.TemplateContent))
            {
                TemplateLexer lexer = new TemplateLexer(this.TemplateContent);
                TemplateParser parser = new TemplateParser(lexer.Parse());

                while (parser.MoveNext())
                {
                    try
                    {
                        parser.Current.Parse(context, writer);
                    }
                    catch (Exception.TemplateException e)
                    {
                        if (context.ThrowExceptions)
                        {
                            throw e;
                        }
                        else
                        {
                            context.AddError(e);
                            writer.Write(parser.Current.ToString());
                        }
                    }
                    catch (System.Exception e)
                    {
                        System.Exception baseException = e.GetBaseException();

                        Exception.ParseException ex = new Exception.ParseException(baseException.Message, baseException);
                        if (context.ThrowExceptions)
                        {
                            throw ex;
                        }
                        else
                        {
                            context.AddError(ex);
                            writer.Write(parser.Current.ToString());
                        }
                    }
                }

            }
        }
    }
}
