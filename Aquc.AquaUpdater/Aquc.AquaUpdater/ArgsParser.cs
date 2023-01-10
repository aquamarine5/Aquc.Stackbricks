using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Aquc.AquaUpdater.Util
{
    public delegate void ParseValueDelegate(FieldInfo field, object obj, object value);
    public class ArgsParser
    {
        readonly string[] args;
        
        public ArgsParser(string[] args)
        {
            this.args = args;
        }
        public T Parse<T>(bool ignoreCase = true) where T : struct, IArgsParseResult => Parse<T>(args,new ParseValueDelegate(ParseValue),ignoreCase);
        public static T Parse<T>(string[] args,bool ignoreCase=true) where T : struct, IArgsParseResult => Parse<T>(args, LocalParseValue,ignoreCase);

        static T Parse<T>(string[] args, ParseValueDelegate parseValue, bool ignoreCase = true) where T : struct, IArgsParseResult
        {
            T result = Activator.CreateInstance<T>();
            object resultObject = result;
            List<string> argsList = new(args);
            if (ignoreCase) argsList.ForEach(str => {
                if (str.StartsWith("-") || str.StartsWith("/")) str = str.ToLower();
            });
            FieldInfo[] fieldInfos = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
            int argsLength = argsList.Count;
            foreach (var field in fieldInfos)
            {
                string fieldName = ignoreCase ? field.Name.ToLower() : field.Name;
                Type fieldType = field.FieldType;
                bool isContains = true;
                string content = null;
                int keyIndex = -1;
                int index1;
                int index2 = argsList.IndexOf("/" + fieldName);
                if(fieldName.Contains('_'))
                {
                    var splitedFieldName = fieldName.Split('_');
                    var finalFieldName = "--" + splitedFieldName[0];
                    for (int i = 1; i < splitedFieldName.Length; i++)
                    {
                        finalFieldName += "-" + splitedFieldName[i];
                    }
                    index1= argsList.IndexOf(finalFieldName);
                }
                else
                {
                    index1=argsList.IndexOf("-" + fieldName);
                }
                if (index1 == -1 && index2 == -1) isContains = false;
                else if (index1 != -1 && index2 != -1) keyIndex = index2;
                else keyIndex = index1 != -1 ? index1 : index2;
                if (isContains && argsLength > keyIndex + 1)
                    content = argsList[keyIndex + 1];

                UnionCondition unionCondition = (UnionCondition)Attribute.GetCustomAttribute(field, typeof(UnionCondition)); ;
                UniqueCondition uniqueCondition = (UniqueCondition)Attribute.GetCustomAttribute(field, typeof(UniqueCondition)); ;
                NecessaryCondition necessaryCondition = (NecessaryCondition)Attribute.GetCustomAttribute(field, typeof(NecessaryCondition));
                if (fieldType == typeof(ArgsNamedKey))
                {
                    field.SetValue(result, isContains ? ArgsNamedKey.Contains : ArgsNamedKey.NotContains);
                    continue;
                }
                if (isContains)
                {
                    if (fieldType == typeof(ArgsSwitchKey<>))
                    {
                        object keyObject = Activator.CreateInstance(fieldType);
                        FieldInfo switchKeyField = fieldType.GetField("switchKey");
                        FieldInfo contentField = fieldType.GetField("content");
                        string[] message = content.Split(':');
                        switchKeyField.SetValue(keyObject, message.Length != 2 ? null : message[1]);
                        parseValue(contentField, keyObject, content);
                        parseValue(field, resultObject, keyObject);
                    }
                    else parseValue(field, resultObject, content);
                }
                else field.SetValue(resultObject, null);
            }
            return (T)resultObject;
        }
        static void LocalParseValue(FieldInfo field, object obj, object value)
        {
            Type fieldType = field.FieldType;
            if (fieldType == typeof(int))
                field.SetValue(obj, int.Parse((string)value));
            else if (fieldType == typeof(float))
                field.SetValue(obj, float.Parse((string)value));
            else if (fieldType == typeof(bool))
                field.SetValue(obj, bool.Parse(((string)value).ToLower()));
            else if (fieldType == typeof(string))
                field.SetValue(obj, (string)value);
            else throw new ArgumentException("error",nameof(field));
        }
        public virtual void ParseValue(FieldInfo field, object obj, object value) =>
            LocalParseValue(field, obj, value);
    }

    public interface IArgsParseResult { }
    public struct ArgsSwitchKey<T>
    {
        public string switchKey;
        public T content;
    }
    public struct ArgsNamedKey
    {
        public static readonly ArgsNamedKey Contains = new (true);
        public static readonly ArgsNamedKey NotContains = new (false);
        public readonly bool isContains;
        public ArgsNamedKey(bool isContains)
        {
            this.isContains = isContains;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class UnionCondition : Attribute
    {
        public readonly string positionalString;
        public UnionCondition(string positionalString)
        {
            this.positionalString = positionalString;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class UniqueCondition : Attribute
    {
        public readonly string positionalString;
        public UniqueCondition(string positionalString)
        {
            this.positionalString = positionalString;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class NecessaryCondition : Attribute
    {
        public readonly string positionalString;
        public NecessaryCondition(string positionalString)
        {
            this.positionalString = positionalString;
        }
    }
}
