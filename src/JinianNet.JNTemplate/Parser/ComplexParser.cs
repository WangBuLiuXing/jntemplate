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
using JinianNet.JNTemplate.Parser.Node;

namespace JinianNet.JNTemplate.Parser
{
    /// <summary>
    /// Complex标签分析器
    /// </summary>
    public class ComplexParser : ITagParser
    {
        #region ITagParser 成员
        /// <summary>
        /// 分析标签
        /// </summary>
        /// <param name="parser">TemplateParser</param>
        /// <param name="tc">Token集合</param>
        /// <returns></returns>
        public Tag Parse(JinianNet.JNTemplate.Parser.TemplateParser parser, TokenCollection tc)
        {
            if (tc.Count > 2)
            {
                Int32 start, end, pos;
                start = end = pos = 0;

                Boolean isFunc = false;

                List<Token> data = new List<Token>();

                Queue<TokenCollection> queue = new Queue<TokenCollection>();

                for (Int32 i = 0; i < tc.Count; i++)
                {
                    end = i;
                    if (tc[i].TokenKind == TokenKind.LeftParentheses)
                    {
                        if (pos == 0)
                        {
                            if (i > 0 && tc[i - 1].TokenKind == TokenKind.TextData)
                            {
                                isFunc = true;
                            }
                        }
                        pos++;
                    }
                    else if (tc[i].TokenKind == TokenKind.RightParentheses)
                    {
                        if (pos > 0)
                        {
                            pos--;
                        }
                        else
                        {
                            throw new Exception.ParseException(String.Concat("syntax error near ):", tc), data[i].BeginLine, data[i].BeginColumn);
                        }

                        if (pos == 0)
                        {
                            TokenCollection coll = new TokenCollection();
                            if (!isFunc)
                            {
                                coll.Add(tc, start + 1, end - 1);
                            }
                            else
                            {
                                coll.Add(tc, start, end);
                            }
                            queue.Enqueue(coll);
                            data.Add(null);
                            start = i + 1;
                            //tag.AddChild(parser.Read(coll));
                        }
                    }
                    else if (pos == 0 && (tc[i].TokenKind == TokenKind.Dot || tc[i].TokenKind == TokenKind.Operator))
                    {
                        if (end > start)
                        {
                            TokenCollection coll = new TokenCollection();
                            coll.Add(tc, start, end - 1);
                            queue.Enqueue(coll);
                            data.Add(null);
                        }
                        start = i + 1;
                        data.Add(tc[i]);
                    }

                    if (i == tc.Count - 1 && end >= start)
                    {
                        if (start == 0 && end == i)
                        {
                            throw new Exception.ParseException(String.Concat("Unexpected  tag:", tc), tc[0].BeginLine, tc[0].BeginColumn);
                        }
                        TokenCollection coll = new TokenCollection();
                        coll.Add(tc, start, end);
                        queue.Enqueue(coll);
                        data.Add(null);
                        start = i + 1;
                    }
                }

                List<Tag> tags = new List<Tag>();

                for (Int32 i = 0; i < data.Count; i++)
                {
                    if (data[i] == null)
                    {
                        //TokenCollection coll = queue.Dequeue();
                        //if (coll.First.TokenKind == TokenKind.LeftParentheses && (coll.Last.TokenKind == TokenKind.RightParentheses))
                        //{
                        //    coll.Remove(coll.First);
                        //    coll.Remove(coll.Last);
                        //}
                        tags.Add(parser.Read(queue.Dequeue()));
                    }
                    else if (data[i].TokenKind == TokenKind.Dot)
                    {
                        if (tags.Count == 0 || i == data.Count - 1 || data[i + 1] != null)
                        {
                            throw new Exception.ParseException(String.Concat("syntax error near .:", tc), data[i].BeginLine, data[i].BeginColumn);
                        }
                        //TokenCollection coll = queue.Dequeue();
                        //if (coll.First.TokenKind == TokenKind.LeftParentheses && (coll.Last.TokenKind == TokenKind.RightParentheses))
                        //{
                        //    coll.Remove(coll.First);
                        //    coll.Remove(coll.Last);
                        //}
                        if (tags[tags.Count - 1] is ReferenceTag)
                        {
                            tags[tags.Count - 1].AddChild(parser.Read(queue.Dequeue()));
                        }
                        else
                        {
                            ReferenceTag t = new ReferenceTag();
                            t.AddChild(tags[tags.Count - 1]);
                            t.AddChild(parser.Read(queue.Dequeue()));
                            tags[tags.Count - 1] = t;
                        }
                        i++;
                    }
                    else if (data[i].TokenKind == TokenKind.Operator)
                    {
                        tags.Add(new TextTag());
                        tags[tags.Count - 1].FirstToken = data[i];

                    }
                }

                if (tags.Count == 1)
                {
                    return tags[0];
                }
                if (tags.Count > 1)
                {
                    ExpressionTag t = new ExpressionTag();

                    for (Int32 i = 0; i < tags.Count; i++)
                    {
                        t.AddChild(tags[i]);
                    }

                    tags.Clear();
                    return t;
                }
            }
            return null;
        }

        #endregion
    }
}
