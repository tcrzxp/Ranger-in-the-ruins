﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Liangddyy.Util
{
    /// <summary>
    /// 反射字段
    /// </summary>
    public class Column
    {
        //Column Value
        private readonly MyProperty property;
        public string columnName;
        public Type columnType;
        public FieldInfo fieldInfo;

        public Column(Type ColumnType, string ColumnName, object value, FieldInfo f)
        {
            this.columnType = ColumnType;
            this.fieldInfo = f;
            this.columnName = ColumnName;

            if (ColumnType == typeof(string))
            {
                string v = Convert.ToString(value);

                property = new MyProperty<string>();
                ((MyProperty<string>)property).SetValue(v);
            }
            else if (ColumnType == typeof(double))
            {
                double v = Convert.ToDouble(value);

                property = new MyProperty<double>();
                ((MyProperty<double>)property).SetValue(v);
            }
            else if (ColumnType == typeof(int))
            {
                int v = Convert.ToInt32(value);

                property = new MyProperty<int>();
                ((MyProperty<int>)property).SetValue(v);
            }
        }

        public string TableType
        {
            get
            {
                if (IsPrimaryKey)
                {
                    if(columnType == typeof(string)) return "TEXT PRIMARY KEY NOT NULL";
                    else return "INTEGER PRIMARY KEY";
                }
                    
                return To_TableType(columnType);
            }
        }

        public bool IsPrimaryKey
        {

            // 转换小写
            get
            {
                object[] nameObjs = fieldInfo.GetCustomAttributes(typeof(IsDataBaseKeyAttribute), false);
                //Debug.Log(columnName + "    " + columnType + "     " +nameObjs.Length);
                for (int i = 0; i < nameObjs.Length; i++)
                {
                    IsDataBaseKeyAttribute nameAtt = (IsDataBaseKeyAttribute)nameObjs[i];
                    //Debug.LogError("这是一个key    " + columnName + "   type:   " + columnType +  "  value:  " + ColumnValue);
                    return true;
                }


                //return (columnName.ToLower().StartsWith("id") || columnName.ToLower().StartsWith("_id")) &&
                //       (columnType == typeof(int));
                return false;
            }
        }

        public object ColumnValue
        {
            get
            {
                if (property == null) return null;

                if (columnType == typeof(double))
                    return ((MyProperty<double>)property).GetValue();
                if (columnType == typeof(string))
                    return ((MyProperty<string>)property).GetValue();
                if (columnType == typeof(int))
                    return ((MyProperty<int>)property).GetValue();
                if (columnType == typeof(Guid))
                {
                    Debug.LogError("值为  :" + ((MyProperty<Guid>)property).GetValue());
                    return ((MyProperty<Guid>)property).GetValue();
                }
                   
        
                return null;
            }
        }

        public string To_TableType(Type type)
        {
            if (type == typeof(double)) return ColType.REAL.ToString();

            if (type == typeof(int)) return ColType.INTEGER.ToString();

            if (type == typeof(string)) return ColType.TEXT.ToString();

            if (type == typeof(Guid))
            {
                Debug.Log("这是一个GUID数据  " + type);
                return ColType.BLOB.ToString();
            } 

            throw new Exception("Wrong Type");
        }
    }

    /// <summary>
    /// 枚举数据库Sqlite支持的几种类型
    /// </summary>
    public enum ColType
    {
        INTEGER,
        TEXT,
        REAL,
        BLOB
    }

    public class MyProperty
    {
    }

    public class MyProperty<T> : MyProperty
    {
        private readonly bool _isValue;
        private bool _changing;
        private T _value;

        public MyProperty()
        {
#if UNITY_FLASH
            _isValue = false;
#else
            _isValue = typeof(T).IsValueType;
#endif
        }

        public MyProperty(T value)
            : this()
        {
            _value = value;
        }

        public bool IsOfType(Type t)
        {
            return t == typeof(T);
        }

        public T GetValue()
        {
            return _value;
        }

        protected virtual bool IsValueDifferent(T value)
        {
            return !_value.Equals(value);
        }

        private bool IsClassDifferent(T value)
        {
            return !_value.Equals(value);
        }

        public virtual void SetValue(T value)
        {
            if (_changing)
                return;
            _changing = true;

            bool changed;

            if (_isValue)
                changed = IsValueDifferent(value);
            else
                changed = ((value == null) && (_value != null)) ||
                          ((value != null) && (_value == null)) ||
                          ((_value != null) && IsClassDifferent(value));
            if (changed)
                _value = value;
            _changing = false;
        }
    }
}