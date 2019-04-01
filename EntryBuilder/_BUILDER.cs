using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Serialize;
using System.Reflection;
using EntryEngine;

namespace EntryBuilder
{
    static class _BUILDER
    {
        public static void AppendSummary(this StringBuilder builder, MemberInfo member)
        {
            ASummary summary = member.GetAttribute<ASummary>();
            if (summary == null)
                return;
            AppendSummary(builder, summary);
        }
        public static void AppendSummary(this StringBuilder builder, ASummary summary)
        {
            AppendSummary(builder, summary.Note);
        }
        public static void AppendSummary(this StringBuilder builder, string summary)
        {
            string[] lines = summary.Split('\n');
            if (lines.Length > 1)
            {
                builder.AppendLine("/// <summary>");
                foreach (var line in lines)
                    builder.AppendLine("/// <para>{0}</para>", line);
                builder.AppendLine("/// </summary>");
            }
            else
                builder.AppendLine("/// <summary>{0}</summary>", summary);
        }
        public static string GetSummary(this ASummary note)
        {
            StringBuilder builder = new StringBuilder();
            AppendSummary(builder, note);
            return builder.ToString();
        }
        public static string GetSummary(this MemberInfo member)
        {
            ASummary note = member.GetAttribute<ASummary>();
            if (note == null)
                return null;
            else
                return GetSummary(note);
        }
        public static void AppendSharpIfElseCompile(this StringBuilder builder, string pre, Action _true, Action _false)
        {
            if (_true == null && _false == null)
                throw new ArgumentNullException("append");
            if (string.IsNullOrEmpty(pre))
                throw new ArgumentNullException("pre");
            builder.AppendLine("#if {0}", pre);
            if (_true != null)
                _true();
            builder.AppendLine("#else");
            if (_false != null)
                _false();
            builder.AppendLine("#endif");
        }
        public static void AppendSharpIfCompile(this StringBuilder builder, string pre, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            if (!string.IsNullOrEmpty(pre))
            {
                builder.AppendLine("#if {0}", pre);
                builder.AppendLine();
                append();
                builder.AppendLine();
                builder.AppendLine("#endif");
            }
            else
                append();
        }
        public static void AppendGenericDefine(this StringBuilder builder, MethodBase method)
        {
            if (method.IsGenericMethod)
            {
                Type[] generic = method.GetGenericArguments();
                builder.Append("<");
                for (int i = 0, len = generic.Length - 1; i <= len; i++)
                {
                    builder.Append(generic[i].CodeName(true));
                    if (i != len)
                        builder.Append(", ");
                }
                builder.Append(">");
            }
        }
        public static void AppendMethodDefine(this StringBuilder builder, MethodInfo method)
        {
            AppendMethodDefine(builder, method, null);
        }
        public static void AppendMethodDefine(this StringBuilder builder, MethodInfo method, string modifier)
        {
            builder.Append("{0} ", method.GetModifier());
            if (!string.IsNullOrEmpty(modifier))
                builder.Append("{0} ", modifier);
            builder.Append("{0} ", method.ReturnType.CodeName());
            builder.Append(method.Name);
            builder.AppendGenericDefine(method);
            builder.AppendMethodParametersWithBracket(method);
        }
        public static void AppendMethodParametersWithBracket(this StringBuilder builder, MethodBase method)
        {
            builder.Append("(");
            AppendMethodParametersOnly(builder, method);
            builder.AppendLine(")");
        }
        public static void AppendMethodParametersOnly(this StringBuilder builder, MethodBase method)
        {
            // BUG: ParamArrayAttribute找不到，这一批扩展方法考虑移出EntryEngine到EntryBuilder
#if DEBUG
            var parameters = method.GetParameters();
            for (int j = 0, n = parameters.Length - 1; j <= n; j++)
            {
                ParameterInfo parameter = parameters[j];
                if (parameter.HasAttribute<ParamArrayAttribute>())
                    builder.Append("params ");
                else if (parameter.IsOut)
                    builder.Append("out ");
                else if (parameter.ParameterType.IsByRef)
                    builder.Append("ref ");
                builder.AppendFormat("{0} {1}", parameter.ParameterType.CodeName(true), parameter.Name);
                if (j != n)
                    builder.Append(", ");
            }
#endif
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method)
        {
            AppendMethodInvoke(builder, method, null, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance)
        {
            AppendMethodInvoke(builder, method, instance, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, null, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, instance, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance, string methodName, string startParam, string endParam)
        {
            if (!string.IsNullOrEmpty(instance))
                builder.Append("{0}.", instance);
            if (string.IsNullOrEmpty(methodName))
                if (!method.IsConstructor)
                    methodName = method.Name;
            builder.Append(methodName);
            builder.AppendGenericDefine(method);
            builder.Append("(");
            var parameters = method.GetParameters();
            if (!string.IsNullOrEmpty(startParam))
            {
                builder.Append(startParam);
                if (parameters.Length > 0)
                    builder.Append(", ");
            }
            int last = parameters.Length - 1;
            bool hasEndParam = !string.IsNullOrEmpty(endParam);
            for (int j = 0; j <= last; j++)
            {
                ParameterInfo parameter = parameters[j];
                if (parameter.IsOut)
                    builder.Append("out ");
                else if (parameter.ParameterType.IsByRef)
                    builder.Append("ref ");
                builder.AppendFormat("{0}", parameter.Name);
                if (j != last || hasEndParam)
                    builder.Append(", ");
            }
            if (hasEndParam)
                builder.Append(endParam);
            builder.AppendLine(");");
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method)
        {
            AppendMethodInvoke(builder, method, null, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance)
        {
            AppendMethodInvoke(builder, method, instance, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, null, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, instance, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance, string methodName, string startParam, string endParam)
        {
            if (method.HasReturnType())
                builder.Append("return ");
            AppendMethodInvoke(builder, (MethodBase)method, instance, methodName, startParam, endParam);
        }
    }
}
