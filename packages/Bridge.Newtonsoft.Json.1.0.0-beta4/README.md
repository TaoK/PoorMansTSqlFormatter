![bridge-newtonsoft](https://user-images.githubusercontent.com/62210/27410384-649e0abe-56a4-11e7-8af3-91f8dbc9e756.png)

The **Bridge.Newtonsoft.Json** package provides [Bridge.NET](http://bridge.net/) support for a limited subset of the [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) API. See it working on [Deck.NET](https://deck.net/newtonsoft.json).

[![Build status](https://ci.appveyor.com/api/projects/status/rg5v87synus9wnun/branch/master?svg=true)](https://ci.appveyor.com/project/ObjectDotNet/bridge-newtonsoft-json/branch/master)
[![Build Status](https://travis-ci.org/bridgedotnet/Bridge.Newtonsoft.Json.svg?branch=master)](https://travis-ci.org/bridgedotnet/Bridge.Newtonsoft.Json)
[![NuGet Status](https://img.shields.io/nuget/v/Bridge.Newtonsoft.Json.svg)](https://www.nuget.org/packages/Bridge.Newtonsoft.Json)

**IMPORTANT:** Only the features listed below are supported.

## Installation

Install using [NuGet](https://www.nuget.org/):

```
Install-Package Bridge.Newtonsoft.Json
```

## JsonConvert.SerializeObject

Serializes the specified object to a JSON string.

Original **SerializeObject** [documentation](http://www.newtonsoft.com/json/help/html/Overload_Newtonsoft_Json_JsonConvert_SerializeObject.htm) from Newtonsoft.Json.

Supported | Name | Description
:----: | ---- | ----
![supported](https://speed.bridge.net/icons/png/16px/check.png) | SerializeObject(Object) | Serializes the specified object to a JSON string.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | SerializeObject(Object, [Formatting](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#formatting)) | Serializes the specified object to a JSON string using formatting.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | SerializeObject(Object, [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings) | Serializes the specified object to a JSON string using JsonSerializerSettings.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | SerializeObject(Object, [Formatting](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#formatting), [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings)) | Serializes the specified object to a JSON string using formatting and JsonSerializerSettings.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | SerializeObject(Object, JsonConverter[]) | Serializes the specified object to a JSON string using a collection of JsonConverter.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | SerializeObject(Object, [Formatting](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#formatting), JsonConverter[]) | Serializes the specified object to a JSON string using formatting and a collection of JsonConverter.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | SerializeObject(Object, Type, JsonSerializerSettings) | Serializes the specified object to a JSON string using a type, formatting and JsonSerializerSettings.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | SerializeObject(Object, Type, [Formatting](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#formatting), [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings)) | Serializes the specified object to a JSON string using a type, formatting and JsonSerializerSettings.

#### Example

```csharp
Product product = new Product();
 
product.Name = "Apple";
product.ExpiryDate = new DateTime(2008, 12, 28);
product.Price = 3.99M;
product.Sizes = new string[] { "Small", "Medium", "Large" };
 
string output = JsonConvert.SerializeObject(product);

//{
//  "Name": "Apple",
//  "ExpiryDate": "2008-12-28T00:00:00",
//  "Price": 3.99,
//  "Sizes": [
//    "Small",
//    "Medium",
//    "Large"
//  ]
//}
```

## JsonConvert.DeserializeObject

Deserializes the JSON to a .NET object.

Original **DeserializeObject** [documentation](http://www.newtonsoft.com/json/help/html/Overload_Newtonsoft_Json_JsonConvert_DeserializeObject.htm) from Newtonsoft.Json.

Supported | Name | Description
:----: | ---- | ----
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject(String) | Deserializes the JSON to a .NET object.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject<T>(String) | Deserializes the JSON to the specified .NET type.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject(String, [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings)) | Deserializes the JSON to a .NET object using JsonSerializerSettings.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject<T>(String, [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings)) | Deserializes the JSON to the specified .NET type using JsonSerializerSettings.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject(String, Type) | Deserializes the JSON to the specified .NET type.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | DeserializeObject(String, Type, [JsonSerializerSettings](https://github.com/bridgedotnet/Bridge.Newtonsoft.Json/blob/master/README.md#jsonserializersettings)) | Deserializes the JSON to the specified .NET type using JsonSerializerSettings.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | DeserializeObject<T>(String, JsonConverter[]) | Deserializes the JSON to the specified .NET type using a collection of JsonConverter.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | DeserializeObject(String, Type, JsonConverter[]) | Deserializes the JSON to the specified .NET type using a collection of JsonConverter.

#### Example

```csharp
Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
```

## Deck.NET

https://deck.net/newtonsoft.json

```csharp
public class Program
{
    public static void Main()
    {
        Product product = new Product();

        product.Name = "Apple";
        product.ExpiryDate = new DateTime(2008, 12, 28);
        product.Price = 3.99M;
        product.Sizes = new string[] { "Small", "Medium", "Large" };

        string output = JsonConvert.SerializeObject(product, Formatting.Indented);

        // Write the json string
        Console.WriteLine(output);

        // Deserialize the json back into a real Product
        Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);

        // Write the properties
        Console.WriteLine(deserializedProduct.Name);
        Console.WriteLine(deserializedProduct.ExpiryDate);
        Console.WriteLine(deserializedProduct.Price);
        Console.WriteLine(deserializedProduct.Sizes);
    }
}

public class Product
{
    public string Name { get; set; }
    
    public DateTime ExpiryDate { get; set; }

    public decimal Price { get; set; }

    public string[] Sizes { get; set; }
}
```

## Formatting

Specifies formatting options for the JsonTextWriter.

Original **Formatting** [documentation](http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Formatting.htm) from Newtonsoft.Json.

Supported | Name | Value | Description
:----: | ---- | ---- | ----
![supported](https://speed.bridge.net/icons/png/16px/check.png) | None | 0 | No special formatting is applied. This is the default.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | Indented | 1 | Causes child objects to be indented according to the Indentation and IndentChar settings.

## JsonSerializerSettings

Original **JsonSerializerSettings** [documentation](http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm) from Newtonsoft.Json.

### ContractResolver

Original **ContractResolver** [documentation](http://www.newtonsoft.com/json/help/html/ContractResolver.htm#CamelCasePropertyNamesContractResolver) from Newtonsoft.Json.

```csharp
new JsonSerializerSettings 
{ 
    ContractResolver = new CamelCasePropertyNamesContractResolver() 
});
```

### TypeNameHandling

Specifies type name handling options for the JsonSerializer.

Original **TypeNameHandling** [documentation](http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_TypeNameHandling.htm) from Newtonsoft.Json.

Supported | Name | Value | Description
:----: | ---- | ---- | ----
![supported](https://speed.bridge.net/icons/png/16px/check.png) | None | 0 | Do not include the .NET type name when serializing types.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | Objects | 1 | Include the .NET type name when serializing into a JSON object structure.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | Arrays | 2 | Include the .NET type name when serializing into a JSON array structure.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | All | 3 | Always include the .NET type name when serializing.
![not supported](https://speed.bridge.net/icons/png/16px/circle-slash.png) | Auto | 4 | Include the .NET type name when the type of the object being serialized is not the same as its declared type. Note that this doesn't include the root serialized object by default. To include the root object's type name in JSON you must specify a root type object with SerializeObject(Object, Type, JsonSerializerSettings) or Serialize(JsonWriter, Object, Type).

### NullValueHandling

Specifies null value handling options for the JsonSerializer.

Original **NullValueHandling** [documentation](http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_NullValueHandling.htm) from Newtonsoft.Json.

Supported | Name | Value | Description
:----: | ---- | ---- | ----
![supported](https://speed.bridge.net/icons/png/16px/check.png) | Include | 0 | Include null values when serializing and deserializing objects.
![supported](https://speed.bridge.net/icons/png/16px/check.png) | Ignore | 1 | Ignore null values when serializing and deserializing objects.

## JsonConstructor Attribute

Instructs the JsonSerializer to use the specified constructor when deserializing that object.

Original **JsonConstructor** [documentation](http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonConstructorAttribute.htm) from Newtonsoft.Json.

https://deck.net/5adc821b73491a122a51fd229ee1a8d3

```csharp
using System;
using Newtonsoft.Json;

public class Program
{
    public static void Main(string[] args)
    {
        var x = Message.Empty.Add("abc");
        var json = JsonConvert.SerializeObject(x);
        
        Console.WriteLine(json);
        
        var cloneX = JsonConvert.DeserializeObject<Message>(json);
        
        Console.WriteLine(cloneX.Value);
    }
}

public class Message
{
  
  static Message _empty = new Message("");
    
  public static Message Empty 
    { 
        get 
        {
            return _empty; 
        } 
        private set 
        { 
            _empty = value; 
        } 
    }
  
    private Message(bool value) { }
    
    [JsonConstructor]
    private Message(string value)
    {
        Value = value;
    }
  
    public string Value { get; private set; }
  
    public Message Add(string value)
    {
        return new Message(Value + value);
    }
}
```
