'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports cs = CmisObjectModel.Serialization
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Serialization
   Public MustInherit Class JavaScriptConverter
      Inherits swss.JavaScriptConverter

      Protected Sub New(objectResolver As ObjectResolver)
         Me.ObjectResolverCore = objectResolver
      End Sub

#Region "Helper-classes"
      Protected Interface IExtendedDeserialization
         Function DeserializeProperties([object] As Object,
                                        dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                        serializer As JSON.Serialization.JavaScriptSerializer) As Boolean
      End Interface
#End Region

      ''' <summary>
      ''' non generic variant
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ObjectResolver As ObjectResolver
         Get
            Return ObjectResolverCore
         End Get
      End Property
      Protected MustOverride Property ObjectResolverCore As ObjectResolver

   End Class

   Namespace Generic
      ''' <summary>
      ''' Baseclass for typesafe JavaScriptConverter with overloaded serialization-methods
      ''' </summary>
      ''' <typeparam name="TSerializable"></typeparam>
      ''' <remarks></remarks>
      Public MustInherit Class JavaScriptConverter(Of TSerializable As cs.XmlSerializable)
         Inherits JavaScriptConverter(Of TSerializable, ObjectResolver(Of TSerializable))
         Implements IExtendedDeserialization

#Region "Constructors"
         Protected Sub New(objectResolver As Generic.ObjectResolver(Of TSerializable))
            MyBase.New(objectResolver)
         End Sub
#End Region

         ''' <summary>
         ''' Converts dictionary to a XmlSerializable-instance of type SupportedType
         ''' </summary>
         ''' <param name="serializer">MUST be a CmisObjectModelLibrary.JSON.JavaScriptSerializer</param>
         ''' <param name="type">ignored, should be nothing</param>
         Public NotOverridable Overrides Function Deserialize(dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                                              type As System.Type, serializer As swss.JavaScriptSerializer) As Object
            If dictionary Is Nothing Then
               Return Nothing
            Else
               Dim retVal As TSerializable = _objectResolver.CreateInstance(dictionary)
               If retVal IsNot Nothing Then
                  Dim context As New SerializationContext(retVal, dictionary, CType(serializer, JSON.Serialization.JavaScriptSerializer))
                  Deserialize(context)
               End If
               Return retVal
            End If
         End Function
         Protected MustOverride Overloads Sub Deserialize(context As SerializationContext)
         Public Function DeserializeProperties([object] As Object,
                                               dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                               serializer As JavaScriptSerializer) As Boolean Implements JavaScriptConverter.IExtendedDeserialization.DeserializeProperties
            If TypeOf [object] Is TSerializable Then
               Dim context As New SerializationContext(DirectCast([object], TSerializable), dictionary, serializer)
               Deserialize(context)
               Return True
            Else
               Return False
            End If
         End Function

         ''' <summary>
         ''' Converts XmlSerializable-instance to IDictionary(Of String, Object)
         ''' </summary>
         ''' <param name="serializer">MUST be a CmisObjectModelLibrary.JSON.JavaScriptSerializer</param>
         Public NotOverridable Overrides Function Serialize(obj As Object, serializer As System.Web.Script.Serialization.JavaScriptSerializer) As System.Collections.Generic.IDictionary(Of String, Object)
            If obj Is Nothing Then
               Return Nothing
            Else
               Dim retVal As New Dictionary(Of String, Object)
               Dim context As New SerializationContext(CType(obj, TSerializable), retVal,
                                                       CType(serializer, JavaScriptSerializer))
               Serialize(context)
               Return retVal
            End If
         End Function
         Protected MustOverride Overloads Sub Serialize(context As SerializationContext)
      End Class
      ''' <summary>
      ''' Baseclass for typesafe JavaScriptConverter
      ''' </summary>
      ''' <typeparam name="TSerializable"></typeparam>
      ''' <remarks></remarks>
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public MustInherit Class JavaScriptConverter(Of TSerializable As cs.XmlSerializable,
                                                      TObjectResolver As ObjectResolver)
         Inherits JavaScriptConverter

#Region "Constructors"
         Protected Sub New(objectResolver As TObjectResolver)
            MyBase.New(objectResolver)
         End Sub
#End Region

#Region "Helper classes"
         ''' <summary>
         ''' Context for complex Read- and all Write-calls
         ''' </summary>
         ''' <remarks></remarks>
         Protected Class SerializationContext
            Public Sub New([object] As TSerializable,
                           dictionary As IDictionary(Of String, Object),
                           serializer As JSON.Serialization.JavaScriptSerializer)
               Me.Object = [object]
               Me.Dictionary = dictionary
               Me.Serializer = serializer
            End Sub

            Public Sub Add(key As String, value As Object)
               Dictionary.Add(key, value)
            End Sub
            Public ReadOnly Dictionary As IDictionary(Of String, Object)
            Public ReadOnly [Object] As TSerializable
            Public ReadOnly Serializer As JSON.Serialization.JavaScriptSerializer
         End Class
#End Region

         ''' <summary>
         ''' Returns the value-property from value or, if not set, returns insteadOfNull
         ''' </summary>
         Protected Function NVL(Of TStructure As Structure)(value As TStructure?, Optional insteadOfNull As TStructure = Nothing) As TStructure
            Return If(value.HasValue, value.Value, insteadOfNull)
         End Function

         Protected _objectResolver As TObjectResolver
         Protected Overrides Property ObjectResolverCore As ObjectResolver
            Get
               Return _objectResolver
            End Get
            Set(value As ObjectResolver)
               If _objectResolver Is Nothing Then _objectResolver = TryCast(value, TObjectResolver)
            End Set
         End Property
         Public Shadows Property ObjectResolver As TObjectResolver
            Get
               Return _objectResolver
            End Get
            Set(value As TObjectResolver)
               If _objectResolver Is Nothing Then _objectResolver = value
            End Set
         End Property

         ''' <summary>
         ''' Reads datetime
         ''' </summary>
         ''' <remarks>JSON representation of DateTimeOffset-instance as long</remarks>
         Protected Overridable Function Read(dictionary As IDictionary(Of String, Object),
                                             propertyName As String, defaultValue As DateTimeOffset) As DateTimeOffset
            If dictionary.ContainsKey(propertyName) Then
               Return Common.TryCastDynamic(Of Long)(dictionary(propertyName), 0).FromJSONTime()
            Else
               Return Nothing
            End If
         End Function

         ''' <summary>
         ''' Reads simple property from dictionary
         ''' </summary>
         Protected Overridable Function Read(Of T)(dictionary As IDictionary(Of String, Object),
                                                   propertyName As String, defaultValue As T) As T
            If dictionary.ContainsKey(propertyName) Then
               Return Common.TryCastDynamic(Of T)(dictionary(propertyName), defaultValue)
            Else
               Return defaultValue
            End If
         End Function

         ''' <summary>
         ''' Reads complex property from dictionary
         ''' </summary>
         Protected Overridable Function Read(Of T As cs.XmlSerializable)(
            context As SerializationContext, propertyName As String,
            defaultValue As T) As T
            Dim innerDictionary As IDictionary(Of String, Object) = Nothing
            Dim javaScriptConverter As swss.JavaScriptConverter = Nothing

            If context.Dictionary.ContainsKey(propertyName) Then
               'default: dictionary contains the expected property
               innerDictionary = TryCast(context.Dictionary(propertyName), IDictionary(Of String, Object))
               If innerDictionary IsNot Nothing Then
                  javaScriptConverter = context.Serializer.GetJavaScriptConverter(GetType(T))
               End If
            Else
               'second chance: perhaps there exists an AttributesOverrides-information for the property
               Dim elementAttribute As JSONAttributeOverrides.JSONElementAttribute =
                  context.Serializer.AttributesOverrides.ElementAttribute(SupportedType, propertyName)
               If elementAttribute IsNot Nothing AndAlso context.Dictionary.ContainsKey(elementAttribute.AliasName) Then
                  innerDictionary = TryCast(context.Dictionary(elementAttribute.AliasName), IDictionary(Of String, Object))
                  If innerDictionary IsNot Nothing Then javaScriptConverter = elementAttribute.ElementConverter
               End If
            End If

            If javaScriptConverter Is Nothing Then
               Return defaultValue
            Else
               Return CType(javaScriptConverter.Deserialize(innerDictionary, GetType(T), context.Serializer), T)
            End If
         End Function

         ''' <summary>
         ''' Reads an array of datetime-objects
         ''' </summary>
         ''' <remarks>JSON representation of DateTimeOffset-instance as long</remarks>
         Protected Overridable Function ReadArray(dictionary As IDictionary(Of String, Object),
                                                  propertyName As String, defaultValues As DateTimeOffset()) As DateTimeOffset()
            If dictionary.ContainsKey(propertyName) Then
               Dim values As Object = dictionary(propertyName)

               If values Is Nothing Then
                  Return Nothing
               ElseIf TypeOf values Is ICollection Then
                  Return (From value As Object In CType(values, ICollection)
                          Select CType(Common.TryCastDynamic(Of Long)(value, 0).FromJSONTime(), DateTimeOffset)).ToArray()
               Else
                  Return New DateTimeOffset() {Common.TryCastDynamic(Of Long)(values, 0).FromJSONTime()}
               End If
            Else
               Return defaultValues
            End If
         End Function

         ''' <summary>
         ''' Reads an array of primitives or strings
         ''' </summary>
         Protected Overridable Function ReadArray(Of T)(dictionary As IDictionary(Of String, Object),
                                                        propertyName As String, defaultValues As T()) As T()
            If dictionary.ContainsKey(propertyName) Then
               Dim values As Object = dictionary(propertyName)

               If values Is Nothing Then
                  Return Nothing
               ElseIf TypeOf values Is ICollection Then
                  Return (From value As Object In CType(values, ICollection)
                          Select Common.TryCastDynamic(Of T)(value, Nothing)).ToArray()
               Else
                  Return New T() {Common.TryCastDynamic(Of T)(values, Nothing)}
               End If
            Else
               Return defaultValues
            End If
         End Function

         ''' <summary>
         ''' Reads an array of complex types
         ''' </summary>
         Protected Overridable Function ReadArray(Of T As cs.XmlSerializable)(context As SerializationContext,
                                                                              propertyName As String, defaultValues As T()) As T()
            Dim javaScriptConverter As swss.JavaScriptConverter = Nothing
            Dim values As ICollection = Nothing
            Dim emptyDictionary As IDictionary(Of String, Object) = New Dictionary(Of String, Object)

            If context.Dictionary.ContainsKey(propertyName) Then
               javaScriptConverter = context.Serializer.GetJavaScriptConverter(GetType(T))
               values = TryCast(context.Dictionary(propertyName), ICollection)
            Else
               'second chance: perhaps there exists an AttributesOverrides-information for the property
               Dim elementAttribute As JSONAttributeOverrides.JSONElementAttribute =
                  context.Serializer.AttributesOverrides.ElementAttribute(SupportedType, propertyName)
               If elementAttribute IsNot Nothing AndAlso context.Dictionary.ContainsKey(elementAttribute.AliasName) Then
                  values = TryCast(context.Dictionary(elementAttribute.AliasName), ICollection)
                  javaScriptConverter = elementAttribute.ElementConverter
               End If
            End If

            If javaScriptConverter Is Nothing Then
               Return defaultValues
            ElseIf values Is Nothing Then
               Return Nothing
            Else
               Return (From value As Object In values
                       Let innerDictionary As IDictionary(Of String, Object) = If(TryCast(value, IDictionary(Of String, Object)), emptyDictionary)
                       Select Common.TryCastDynamic(Of T)(javaScriptConverter.Deserialize(innerDictionary, GetType(T), context.Serializer), Nothing)).ToArray()
            End If
         End Function

         ''' <summary>
         ''' Reads enum-property from dictionary
         ''' </summary>
         Protected Overridable Function ReadEnum(Of TEnum As Structure)(dictionary As IDictionary(Of String, Object),
                                                                        propertyName As String, defaultValue As TEnum) As TEnum
            Dim value As TEnum
            If dictionary.ContainsKey(propertyName) Then
               Return If(TryParse(CStr(dictionary(propertyName)), value, True), value, defaultValue)
            Else
               Return defaultValue
            End If
         End Function

         ''' <summary>
         ''' Reads an array of enums
         ''' </summary>
         Protected Overridable Function ReadEnumArray(Of TEnum As Structure)(dictionary As IDictionary(Of String, Object),
                                                                             propertyName As String, defaultValue As TEnum()) As TEnum()
            If dictionary.ContainsKey(propertyName) Then
               Dim values As IEnumerable = TryCast(dictionary(propertyName), IEnumerable)

               If values Is Nothing Then
                  Return Nothing
               Else
                  Dim enumValue As TEnum

                  Return (From value As Object In values
                          Select If(TryParse(CStr(value), enumValue, True), enumValue, Nothing)).ToArray()

               End If
            Else
               Return defaultValue
            End If
         End Function

         ''' <summary>
         ''' Reads datetime nullable property from dictionary
         ''' </summary>
         ''' <remarks>JSON representation of DateTimeOffset-instance as long</remarks>
         Protected Overridable Function ReadNullable(dictionary As IDictionary(Of String, Object),
                                                     propertyName As String, defaultValue As DateTimeOffset?) As DateTimeOffset?
            If dictionary.ContainsKey(propertyName) Then
               Return Read(dictionary, propertyName, If(defaultValue.HasValue, defaultValue.Value, Nothing))
            Else
               Return defaultValue
            End If
         End Function

         ''' <summary>
         ''' Reads simple nullable property from dictionary
         ''' </summary>
         Protected Overridable Function ReadNullable(Of T As Structure)(dictionary As IDictionary(Of String, Object),
                                                                        propertyName As String, defaultValue As T?) As T?
            If dictionary.ContainsKey(propertyName) Then
               Return Read(Of T)(dictionary, propertyName, If(defaultValue.HasValue, defaultValue.Value, Nothing))
            Else
               Return defaultValue
            End If
         End Function

         ''' <summary>
         ''' Reads optional enum-property from dictionary
         ''' </summary>
         Protected Overridable Function ReadOptionalEnum(Of TEnum As Structure)(dictionary As IDictionary(Of String, Object),
                                                                                propertyName As String, defaultValue As TEnum?) As TEnum?
            Dim value As TEnum
            If dictionary.ContainsKey(propertyName) Then
               If TryParse(CStr(dictionary(propertyName)), value, True) Then
                  Return value
               Else
                  Return defaultValue
               End If
            Else
               Return defaultValue
            End If
         End Function

         Public Shared ReadOnly SupportedType As Type = GetType(TSerializable)
         Public NotOverridable Overrides ReadOnly Property SupportedTypes As System.Collections.Generic.IEnumerable(Of System.Type)
            Get
               Return New Type() {SupportedType}
            End Get
         End Property

         ''' <summary>
         ''' Writes a complex property to dictionary
         ''' </summary>
         Protected Overridable Sub Write(context As SerializationContext,
                                         value As cs.XmlSerializable, propertyName As String)
            If value IsNot Nothing Then
               Dim elementAttribute As JSONAttributeOverrides.JSONElementAttribute = If(context.Serializer Is Nothing, Nothing,
                                                                                        context.Serializer.AttributesOverrides.ElementAttribute(SupportedType, propertyName))

               If elementAttribute Is Nothing Then
                  Dim javaScriptConverter As JavaScriptConverter = context.Serializer.GetJavaScriptConverter(value.GetType())
                  If javaScriptConverter IsNot Nothing Then context.Dictionary.Add(propertyName, javaScriptConverter.Serialize(value, context.Serializer))
               Else
                  context.Dictionary.Add(elementAttribute.AliasName, elementAttribute.ElementConverter.Serialize(value, context.Serializer))
               End If
            End If
         End Sub

         ''' <summary>
         ''' Writes a datetime
         ''' </summary>
         ''' <remarks>JSON representation of DateTimeOffset-instance as long</remarks>
         Protected Overridable Sub Write(context As SerializationContext,
                                         value As DateTimeOffset, propertyName As String)
            If Not DateTime.MinValue.Equals(value.DateTime) Then
               context.Dictionary.Add(propertyName, value.DateTime.ToJSONTime())
            End If
         End Sub

         ''' <summary>
         ''' Writes an array of datetime-objects
         ''' </summary>
         ''' <remarks>JSON representation of DateTimeOffset-instance as long</remarks>
         Protected Overridable Sub WriteArray(context As SerializationContext,
                                              values As DateTimeOffset(), propertyName As String)
            If values IsNot Nothing Then
               WriteArray(Of Long)(context,
                                   (From value As DateTimeOffset In values
                                    Select value.DateTime.ToJSONTime()).ToArray(),
                                   propertyName)
            End If
         End Sub

         ''' <summary>
         ''' Writes an array to dictionary
         ''' </summary>
         Protected Overridable Sub WriteArray(Of TItem)(context As SerializationContext,
                                                        values As TItem(), propertyName As String)
            If values IsNot Nothing Then
               If GetType(TItem).IsEnum Then
                  'enums
                  context.Dictionary.Add(propertyName, (From value As TItem In values
                                                        Select Common.GetName(CType(CObj(value), System.Enum))).ToArray)
               ElseIf GetType(cs.XmlSerializable).IsAssignableFrom(GetType(TItem)) Then
                  'complex types
                  Dim elementAttribute As JSONAttributeOverrides.JSONElementAttribute =
                     If(context.Serializer Is Nothing, Nothing,
                        context.Serializer.AttributesOverrides.ElementAttribute(SupportedType, propertyName))
                  If elementAttribute IsNot Nothing Then propertyName = elementAttribute.AliasName
                  context.Dictionary.Add(propertyName, (From value As TItem In values
                                                        Let valueType As Type = If(value Is Nothing, GetType(TItem), value.GetType())
                                                        Let javaScriptConverter As swss.JavaScriptConverter =
                                                            If(If(elementAttribute Is Nothing, Nothing, elementAttribute.ElementConverter),
                                                               context.Serializer.GetJavaScriptConverter(valueType))
                                                        Where javaScriptConverter IsNot Nothing
                                                        Select javaScriptConverter.Serialize(value, context.Serializer)).ToArray())
               ElseIf GetType(String) Is GetType(TItem) Then
                  context.Add(propertyName, (From rawValue As TItem In values
                                             Let value As String = CType(CObj(rawValue), String)
                                             Select If(value, String.Empty)).ToArray)
               Else
                  'others
                  context.Dictionary.Add(propertyName, values)
               End If
            End If
         End Sub

      End Class

      ''' <summary>
      ''' Fallback mechanism: just create the type, but no deserialization/serialization-support
      ''' </summary>
      ''' <typeparam name="TSerializable"></typeparam>
      ''' <remarks>Unspecific converter using reflection to (de-)serialize a XmlSerializable-instance</remarks>
      Public Class XmlSerializerConverter(Of TSerializable As cs.XmlSerializable)
         Inherits JavaScriptConverter(Of TSerializable)

         Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of TSerializable))
            MyBase.New(objectResolver)
         End Sub

#Region "Helper classes"
         ''' <summary>
         ''' Helper class to allow access to generic methods of the base class
         ''' </summary>
         ''' <remarks></remarks>
         Private MustInherit Class IOHelper
            Public MustOverride Function Read(caller As XmlSerializerConverter(Of TSerializable),
                                              source As Object,
                                              propertyName As String,
                                              defaultValue As Object) As Object
            Public MustOverride Function ReadArray(caller As XmlSerializerConverter(Of TSerializable),
                                                   source As Object,
                                                   propertyName As String,
                                                   defaultValue As Object) As Object
            Public Sub Write(caller As XmlSerializerConverter(Of TSerializable), context As SerializationContext, value As Object, propertyName As String)
               caller.Write(context, CType(value, cs.XmlSerializable), propertyName)
            End Sub
            Public MustOverride Sub WriteArray(caller As XmlSerializerConverter(Of TSerializable), context As SerializationContext, value As Object, propertyName As String)
         End Class

         ''' <summary>
         ''' Extension of IOHelper to handle value types
         ''' </summary>
         ''' <remarks></remarks>
         Private MustInherit Class IOValueTypeHelper
            Inherits IOHelper
            Public MustOverride Function GetValue(value As Object) As Object
            Public MustOverride Function HasValue(value As Object) As Boolean
            Public MustOverride Function ReadNullable(caller As XmlSerializerConverter(Of TSerializable),
                                                      dictionary As IDictionary(Of String, Object),
                                                      propertyName As String,
                                                      defaultValue As Object) As Object
         End Class

         ''' <summary>
         ''' Used for enum-type
         ''' </summary>
         ''' <typeparam name="T"></typeparam>
         ''' <remarks></remarks>
         Private Class IOEnumTypeHelper(Of T As Structure)
            Inherits IOValueTypeHelper(Of T)

#Region "Read"
            Public Overrides Function Read(caller As XmlSerializerConverter(Of TSerializable),
                                           source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.ReadEnum(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                            propertyName, CType(defaultValue, T))
            End Function
            Public Overrides Function ReadArray(caller As XmlSerializerConverter(Of TSerializable),
                                                source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.ReadEnumArray(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                                 propertyName, CType(defaultValue, T()))
            End Function
            Public Overrides Function ReadNullable(caller As XmlSerializerConverter(Of TSerializable),
                                                   dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                                   propertyName As String, defaultValue As Object) As Object
               Return caller.ReadOptionalEnum(Of T)(dictionary, propertyName, CType(defaultValue, T?))
            End Function
#End Region
         End Class

         ''' <summary>
         ''' Used for value-type types that are not enum-type
         ''' </summary>
         ''' <typeparam name="T"></typeparam>
         ''' <remarks></remarks>
         Private Class IOValueTypeHelper(Of T As Structure)
            Inherits IOValueTypeHelper
            Public Overrides Function GetValue(value As Object) As Object
               Return CType(value, T?).Value
            End Function
            Public Overrides Function HasValue(value As Object) As Boolean
               Return CType(value, T?).HasValue
            End Function

#Region "Read"
            Public Overrides Function Read(caller As XmlSerializerConverter(Of TSerializable),
                                           source As Object,
                                           propertyName As String, defaultValue As Object) As Object
               Return caller.Read(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                        propertyName, CType(defaultValue, T))
            End Function
            Public Overloads Overrides Function ReadArray(caller As XmlSerializerConverter(Of TSerializable),
                                                          source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.ReadArray(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                             propertyName, CType(defaultValue, T()))
            End Function
            Public Overrides Function ReadNullable(caller As XmlSerializerConverter(Of TSerializable),
                                                   dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                                   propertyName As String, defaultValue As Object) As Object
               Return caller.ReadNullable(Of T)(dictionary, propertyName, CType(defaultValue, T?))
            End Function
#End Region

#Region "Write"
            Public Overrides Sub WriteArray(caller As XmlSerializerConverter(Of TSerializable), context As SerializationContext, value As Object, propertyName As String)
               caller.WriteArray(Of T)(context, CType(value, T()), propertyName)
            End Sub
#End Region
         End Class

         ''' <summary>
         ''' Used for non value-type types that are not derived from XmlSerializable
         ''' </summary>
         ''' <typeparam name="T"></typeparam>
         ''' <remarks></remarks>
         Private Class IOUnspecificTypeHelper(Of T)
            Inherits IOHelper
#Region "Read"
            Public Overrides Function Read(caller As XmlSerializerConverter(Of TSerializable),
                                           source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.Read(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                        propertyName, CType(defaultValue, T))
            End Function
            Public Overloads Overrides Function ReadArray(caller As XmlSerializerConverter(Of TSerializable),
                                                          source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.ReadArray(Of T)(CType(source, System.Collections.Generic.IDictionary(Of String, Object)),
                                             propertyName, CType(defaultValue, T()))
            End Function
#End Region
#Region "Write"
            Public Overrides Sub WriteArray(caller As XmlSerializerConverter(Of TSerializable),
                                            context As SerializationContext, value As Object, propertyName As String)
               caller.WriteArray(Of T)(context, CType(value, T()), propertyName)
            End Sub
#End Region
         End Class

         ''' <summary>
         ''' Used for XmlSerializable-types
         ''' </summary>
         ''' <typeparam name="T"></typeparam>
         ''' <remarks></remarks>
         Private Class IOXmlSerializableTypeHelper(Of T As cs.XmlSerializable)
            Inherits IOHelper
#Region "Read"
            Public Overloads Overrides Function Read(caller As XmlSerializerConverter(Of TSerializable),
                                                     source As Object,
                                                     propertyName As String,
                                                     defaultValue As Object) As Object
               Return caller.Read(Of T)(CType(source, SerializationContext), propertyName, CType(defaultValue, T))
            End Function
            Public Overloads Overrides Function ReadArray(caller As XmlSerializerConverter(Of TSerializable),
                                                          source As Object, propertyName As String, defaultValue As Object) As Object
               Return caller.ReadArray(Of T)(CType(source, SerializationContext), propertyName, CType(defaultValue, T()))
            End Function
#End Region
#Region "Write"
            Public Overrides Sub WriteArray(caller As XmlSerializerConverter(Of TSerializable), context As SerializationContext, value As Object, propertyName As String)
               caller.WriteArray(Of T)(context, CType(value, T()), propertyName)
            End Sub
#End Region
         End Class
#End Region

         Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
            Dim objectType As Type = context.Object.GetType()

            If objectType IsNot GetType(TSerializable) Then
               Dim moreSuitableConverter As IExtendedDeserialization =
                  TryCast(context.Serializer.GetJavaScriptConverter(objectType), IExtendedDeserialization)
               'a better deserialization method is found
               If moreSuitableConverter IsNot Nothing AndAlso Not TypeOf moreSuitableConverter Is XmlSerializerConverter(Of TSerializable) AndAlso
                  moreSuitableConverter.DeserializeProperties(context.Object, context.Dictionary, context.Serializer) Then Exit Sub
            End If

            Dim props As Dictionary(Of String, System.Reflection.PropertyInfo) = GetPropertyInfos(objectType, Function(pi) pi.CanWrite)

            With context
               For Each de As KeyValuePair(Of String, Object) In .Dictionary
                  Dim key As String = de.Key.ToLowerInvariant()

                  If props.ContainsKey(key) Then
                     Dim pi As System.Reflection.PropertyInfo = props(key)
                     Dim propertyType As Type = pi.PropertyType
                     Dim elementType As Type = If(propertyType.IsArray, propertyType.GetElementType(), propertyType)
                     Dim ioHelper As IOHelper = GetOrCreateIOHelper(elementType)
                     Dim defaultValue As Object = pi.GetValue(.Object, Nothing)
                     Dim value As Object = de.Value

                     If value IsNot Nothing Then
                        If propertyType.IsArray Then
                           If elementType Is GetType(DateTimeOffset) Then
                              value = Me.ReadArray(.Dictionary, de.Key, Nothing)
                           ElseIf IsXmlSerializableType(elementType) Then
                              value = ioHelper.ReadArray(Me, context, de.Key, defaultValue)
                           Else
                              value = ioHelper.ReadArray(Me, .Dictionary, de.Key, defaultValue)
                           End If
                        ElseIf propertyType Is GetType(DateTimeOffset) Then
                           value = Me.Read(.Dictionary, de.Key, Nothing)
                        ElseIf IsNullableType(propertyType) Then
                           If propertyType Is GetType(DateTimeOffset?) Then
                              value = Me.ReadNullable(.Dictionary, de.Key, CType(defaultValue, DateTimeOffset?))
                           Else
                              value = CType(ioHelper, IOValueTypeHelper).ReadNullable(Me, .Dictionary, de.Key, defaultValue)
                           End If
                        ElseIf IsXmlSerializableType(propertyType) Then
                           value = ioHelper.Read(Me, context, de.Key, defaultValue)
                        Else
                           value = ioHelper.Read(Me, .Dictionary, de.Key, defaultValue)
                        End If
                     End If
                     pi.SetValue(.Object, value, Nothing)
                  End If
               Next
            End With
         End Sub

         ''' <summary>
         ''' Returns an ioHelper-instance suitable for type
         ''' </summary>
         Private Function GetOrCreateIOHelper(type As Type) As IOHelper
            If IsNullableType(type) Then
               type = type.GetGenericArguments()(0)
            End If
            'create an ioHelper for specified type
            If Not _ioHelpers.ContainsKey(type) Then
               If type.IsEnum Then
                  _ioHelpers.Add(type, CType(GetType(IOEnumTypeHelper(Of AcceptRejectRule)).GetGenericTypeDefinition.MakeGenericType(GetType(TSerializable), type).GetConstructor(New Type() {}).Invoke(New Object() {}), IOHelper))
               ElseIf type.IsValueType Then
                  _ioHelpers.Add(type, CType(GetType(IOValueTypeHelper(Of AcceptRejectRule)).GetGenericTypeDefinition.MakeGenericType(GetType(TSerializable), type).GetConstructor(New Type() {}).Invoke(New Object() {}), IOHelper))
               ElseIf IsXmlSerializableType(type) Then
                  _ioHelpers.Add(type, CType(GetType(IOXmlSerializableTypeHelper(Of cs.XmlSerializable)).GetGenericTypeDefinition.MakeGenericType(GetType(TSerializable), type).GetConstructor(New Type() {}).Invoke(New Object() {}), IOHelper))
               Else
                  _ioHelpers.Add(type, CType(GetType(IOUnspecificTypeHelper(Of String)).GetGenericTypeDefinition.MakeGenericType(GetType(TSerializable), type).GetConstructor(New Type() {}).Invoke(New Object() {}), IOHelper))
               End If
            End If

            Return _ioHelpers(type)
         End Function

         ''' <summary>
         ''' Returns a collection of propertyInfos declared in XmlSerializable-classes
         ''' </summary>
         Private Function GetPropertyInfos(type As Type, selector As Func(Of System.Reflection.PropertyInfo, Boolean)) As Dictionary(Of String, System.Reflection.PropertyInfo)
            Dim retVal As New Dictionary(Of String, System.Reflection.PropertyInfo)

            While GetType(cs.XmlSerializable).IsAssignableFrom(type)
               For Each pi As System.Reflection.PropertyInfo In type.GetProperties()
                  Dim parameters As System.Reflection.ParameterInfo() = pi.GetIndexParameters()
                  Dim key As String = pi.Name.ToLowerInvariant()

                  If Not retVal.ContainsKey(key) AndAlso (parameters Is Nothing OrElse parameters.Length = 0) AndAlso
                     selector(pi) Then
                     retVal.Add(key, pi)
                  End If
               Next
               type = type.BaseType
            End While

            Return retVal
         End Function

         ''' <summary>
         ''' Returns True if the property should be serialized
         ''' </summary>
         Private Function GetPropertyInfosReadSelector(pi As System.Reflection.PropertyInfo) As Boolean
            If pi.CanRead Then
               Dim attrs As Object() = pi.GetCustomAttributes(GetType(swss.ScriptIgnoreAttribute), True)
               Return attrs Is Nothing OrElse attrs.Length = 0
            Else
               Return False
            End If
         End Function

         Private _ioHelpers As New Dictionary(Of Type, IOHelper)

         ''' <summary>
         ''' Returns True if type is derived from XmlSerializable
         ''' </summary>
         Private Function IsXmlSerializableType(type As Type) As Boolean
            Return GetType(cs.XmlSerializable).IsAssignableFrom(type)
         End Function

         Protected Overloads Overrides Sub Serialize(context As SerializationContext)
            Dim props As System.Reflection.PropertyInfo() = GetPropertyInfos(context.Object.GetType(), AddressOf GetPropertyInfosReadSelector).Values.ToArray()
            Dim emptyIndexParameters As Object() = New Object() {}

            With context
               For Each pi As System.Reflection.PropertyInfo In props
                  Dim propertyType As Type = pi.PropertyType
                  Dim elementType As Type = If(propertyType.IsArray, propertyType.GetElementType(), propertyType)
                  Dim ioHelper As IOHelper = GetOrCreateIOHelper(elementType)
                  Dim value As Object = pi.GetValue(.Object, emptyIndexParameters)

                  If propertyType.IsEnum Then
                     If IsNullableType(propertyType) Then
                        If CType(ioHelper, IOValueTypeHelper).HasValue(value) Then
                           value = CType(ioHelper, IOValueTypeHelper).GetValue(value)
                        Else
                           Continue For
                        End If
                     End If
                     value = Common.GetName(CType(value, System.Enum))
                  ElseIf IsNullableType(propertyType) Then
                     If CType(ioHelper, IOValueTypeHelper).HasValue(value) Then
                        value = CType(ioHelper, IOValueTypeHelper).GetValue(value)
                        If propertyType Is GetType(DateTimeOffset?) Then
                           Me.Write(context, CType(value, DateTimeOffset), pi.Name)
                           Continue For
                        End If
                     Else
                        Continue For
                     End If
                  ElseIf IsXmlSerializableType(propertyType) Then
                     ioHelper.Write(Me, context, value, pi.Name)
                     Continue For
                  ElseIf propertyType.IsArray Then
                     If elementType Is GetType(DateTimeOffset) Then
                        Me.WriteArray(context, CType(value, DateTimeOffset()), pi.Name)
                     Else
                        ioHelper.WriteArray(Me, context, value, pi.Name)
                     End If
                     Continue For
                  ElseIf propertyType Is GetType(DateTimeOffset) Then
                     Me.Write(context, CType(value, DateTimeOffset), pi.Name)
                     Continue For
                  End If

                  'write
                  .Dictionary.Add(pi.Name, value)
               Next
            End With
         End Sub
      End Class
   End Namespace
End Namespace