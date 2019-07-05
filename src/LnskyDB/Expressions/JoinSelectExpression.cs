﻿using Dapper;
using LnskyDB.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LnskyDB.Expressions
{
    internal class JoinSelectExpression : BaseExpressionVisitor
    {

        public List<string> QueryColumns = new List<string>();

        Dictionary<string, string> _map = new Dictionary<string, string>();

        public JoinSelectExpression(LambdaExpression expression, Dictionary<string, string> map, DynamicParameters para) : base(para)
        {
            foreach (var v in map)
            {
                var key = string.IsNullOrEmpty(v.Key) ? expression.Parameters[0].Name : (expression.Parameters[0].Name + "." + v.Key);
                _map.Add(key, v.Value);
            }
            _tempFieldName = "PJS_" + GetHashCode() + "_";
            var exp = TrimExpression.Trim(expression);
            Visit(exp);
            if (_sqlCmd.Length > 0)
            {
                QueryColumns.Add(_sqlCmd.ToString());
                _sqlCmd.Clear();
            }
        }


        #region 访问成员表达式


        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            for (int i = 0; i < node.Bindings.Count; i++)
            {
                var m = node.Bindings[i] as MemberAssignment;
                Visit(m.Expression); 
                QueryColumns.Add(_sqlCmd.ToString() + " " + node.Bindings[i].Member.Name);
                _sqlCmd.Clear();
            }
            return node;
        }

        /// <inheritdoc />
        /// <summary>
        /// 访问成员表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitMember(MemberExpression node)
        {
            var name = node.ToString();
            if (!_map.TryGetValue(name, out var val))
            {
                name = name.Remove(name.LastIndexOf("."));
                _map.TryGetValue(name, out val);
                val += "." + _openQuote + node.Member.GetColumnAttributeName() + _closeQuote;
            }
            _sqlCmd.Append(val);
            return node;
        }

        #endregion

      

    }
}