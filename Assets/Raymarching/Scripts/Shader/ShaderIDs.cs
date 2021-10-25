using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class ShaderIDs
{
  private struct PropertyWithID
  {
    public readonly int ID;
    public readonly Func<object, object> Get;

    public PropertyWithID(int id, Func<object, object> getDelegate)
    {
      ID = id;
      Get = getDelegate;
    }
  }

  private Dictionary<ShaderType, List<PropertyWithID>> _shaderIDs = new Dictionary<ShaderType, List<PropertyWithID>>();

  public void Init(object obj, SerializableGuid guid)
  {
    // NOTE(WSWhitehouse): Using expression trees to speed up reflection, more info is available here:
    // http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx
    // https://dejanstojanovic.net/aspnet/2019/february/making-reflection-in-net-work-faster/
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/
    // https://mattwarren.org/2016/12/14/Why-is-Reflection-slow/

    string guidString = guid.ToShaderSafeString();
    ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
    PropertyInfo[] properties = obj.GetType().GetProperties();

    foreach (var property in properties)
    {
      if (property.GetCustomAttribute(typeof(UploadToShaderAttribute)) is not UploadToShaderAttribute) continue;
      if (IsPropertyIndexed(property)) return;
      
      
      Type type = property.PropertyType;
      ShaderType shaderType = type.ToShaderType();
      int id = Shader.PropertyToID($"_{property.Name}{guidString}");


      UnaryExpression instanceCast = !property.DeclaringType.IsValueType
        ? Expression.TypeAs(instance, property.DeclaringType)
        : Expression.Convert(instance, property.DeclaringType);

      Func<object, object> getDelegate = Expression
        .Lambda<Func<object, object>>(
          Expression.TypeAs(Expression.Call(instanceCast, property.GetGetMethod()), typeof(object)), instance)
        .Compile();

      PropertyWithID idWithGetter = new PropertyWithID(id, getDelegate);

      if (!_shaderIDs.ContainsKey(shaderType))
      {
        _shaderIDs.Add(shaderType, new List<PropertyWithID>());
      }

      _shaderIDs[shaderType].Add(idWithGetter);
    }
  }

  public void UploadShaderData(object obj, Material material)
  {
    foreach ((ShaderType shaderType, List<PropertyWithID> propertyList) in _shaderIDs)
    {
      switch (shaderType)
      {
        case ShaderType.Float:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetFloat(property.ID, (float) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Int:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetInteger(property.ID, (int) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Bool:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetInteger(property.ID, (bool) property.Get(obj) ? 1 : 0);
          }

          continue;
        }
        case ShaderType.Vector2:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetVector(property.ID, (Vector2) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Vector3:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetVector(property.ID, (Vector3) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Vector4:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetVector(property.ID, (Vector4) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Colour:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetVector(property.ID, (Color) property.Get(obj));
          }

          continue;
        }
        case ShaderType.Texture2D:
        {
          foreach (PropertyWithID property in propertyList)
          {
            material.SetTexture(property.ID, (Texture2D) property.Get(obj));
          }

          continue;
        }
        // NOTE(WSWhitehouse): Don't upload void type to shader
        case ShaderType.Void:
          continue; 
        default:
          throw new NotSupportedException($"{shaderType.ToString()} is not supported in ShaderIDs::UploadShaderData.");
      }
    }
  }

  private bool IsPropertyIndexed(PropertyInfo propertyInfo)
  {
    return propertyInfo.GetIndexParameters().Length != 0;
  }
}