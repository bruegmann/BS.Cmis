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
Imports ccg = CmisObjectModel.Common.Generic
Imports ccg1 = CmisObjectModel.Collections.Generic
Imports cs = CmisObjectModel.Serialization
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Serialization
   ''' <summary>
   ''' JavaScriptSerializer to handle instances derived from CmisObjectModelLibrary.Serialization.XmlSerializable-class
   ''' </summary>
   ''' <remarks></remarks>
   Public Class JavaScriptSerializer
      Inherits System.Web.Script.Serialization.JavaScriptSerializer

#Region "Constructors"
      Public Sub New()
         MyBase.New()
         Me.MaxJsonLength = Integer.MaxValue
      End Sub
      ''' <summary>
      ''' Succinct support
      ''' </summary>
      ''' <param name="succinct"></param>
      ''' <remarks></remarks>
      Public Sub New(succinct As Boolean)
         Me.New()
         If succinct Then
            AttributesOverrides.ElementConverter(GetType(CmisObjectModel.Core.cmisObjectType), "properties", "succinctProperties") =
               New JSON.Collections.SuccinctPropertiesConverter()
         End If
      End Sub
#End Region

      Public ReadOnly AttributesOverrides As New JSONAttributeOverrides()
      Private Shared _converters As New Dictionary(Of Type, JavaScriptConverter)

      ''' <summary>
      ''' Creates an instance of type using reflection
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function CreateInstance(Of T)(type As Type, ParamArray args As Object()) As T
         If GetType(T).IsAssignableFrom(type) Then
            Dim argsLength As Integer = If(args Is Nothing, 0, args.Length)

            If argsLength = 0 Then
               'default constructor
               Dim ci As System.Reflection.ConstructorInfo = type.GetConstructor(New Type() {})

               Return If(ci Is Nothing, Nothing, CType(ci.Invoke(New Object() {}), T))
            Else
               'use constructor with parameters
               For Each ci As System.Reflection.ConstructorInfo In type.GetConstructors()
                  Dim parameters As System.Reflection.ParameterInfo() = ci.GetParameters()
                  If argsLength = If(parameters Is Nothing, 0, parameters.Length) Then
                     Dim argIndex As Integer = 0
                     While argIndex < argsLength
                        Dim arg As Object = args(argIndex)
                        'constructor does not match the arguments
                        If Not (arg Is Nothing OrElse parameters(argIndex).ParameterType.IsAssignableFrom(arg.GetType())) Then
                           Continue For
                        Else
                           argIndex += 1
                        End If
                     End While
                     'all arguments accepted
                     Return CType(ci.Invoke(args), T)
                  End If
               Next

               'unable to find suitable constructor then try default constructor
               Return CreateInstance(Of T)(type)
            End If
         End If
      End Function

      ''' <summary>
      ''' Before deserialization of input Deserialize() auto-registers the best fitting javaConverter-instance for type TSerializable
      ''' </summary>
      Public Shadows Function Deserialize(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable)(input As String) As TSerializable
         RegisterConverter(Of TSerializable)()
         Return MyBase.Deserialize(Of TSerializable)(input)
      End Function
      ''' <summary>
      ''' Deserializes an array
      ''' </summary>
      Public Function DeserializeArray(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable)(input As String) As TSerializable()
         RegisterConverter(Of TSerializable)()
         Return MyBase.Deserialize(Of TSerializable())(input)
      End Function
      ''' <summary>
      ''' Deserializes an array represented as a map
      ''' </summary>
      Public Shadows Function DeserializeMap(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable,
                                                TKey)(input As String, keyProperty As Common.Generic.DynamicProperty(Of TSerializable, TKey)) As TSerializable()
         Dim dictionary As IDictionary(Of String, Object) = MyBase.Deserialize(Of Dictionary(Of String, Object))(input)
         If dictionary Is Nothing Then
            Return Nothing
         Else
            Dim arrayProperty As New ccg1.ArrayContainer(Of TSerializable)("Values")
            Dim mapper As New ccg1.ArrayMapper(Of cs.EmptyXmlSerializable, TSerializable, TKey)(cs.EmptyXmlSerializable.Singleton, arrayProperty, keyProperty)
            mapper.JavaImport(dictionary, Me)
            Return arrayProperty.Value
         End If
      End Function

      ''' <summary>
      ''' Returns a JavaScriptConverter-instance designed for an object of type or null, if no JavaScriptConverter could be found
      ''' </summary>
      Public Function GetJavaScriptConverter(type As Type) As JavaScriptConverter
         Dim retVal As JavaScriptConverter = AttributesOverrides.TypeConverter(type)

         If retVal Is Nothing Then
            SyncLock _converters
               If _converters.ContainsKey(type) Then
                  Return _converters(type)
               ElseIf GetType(CmisObjectModel.Serialization.XmlSerializable).IsAssignableFrom(type) Then
                  'first call with given type parameter, so the system has to search for JavaScriptConverterAttribute
                  'to create a JavaScriptConverter-factory for it
                  Try
                     Dim javaScriptConverterAttr As Attributes.JavaScriptConverterAttribute =
                        GetCustomAttribute(Of Attributes.JavaScriptConverterAttribute)(type)
                     'this attribute will always be found, because there is a fallback mechanism defined for XmlSerializable-classes
                     Dim javaScriptObjectResolverAttr As Attributes.JavaScriptObjectResolverAttribute =
                        GetCustomAttribute(Of Attributes.JavaScriptObjectResolverAttribute)(type)
                     If javaScriptConverterAttr IsNot Nothing Then
                        'if type is a mustinherit type then a specific objectResolver MUST be declared for it, otherwise - only using the fallback
                        'mechanism - an exception is thrown while trying to get the ObjectResolverType
                        retVal = CreateInstance(Of JavaScriptConverter)(javaScriptConverterAttr.JavaScriptConverterType(type),
                                                                        CreateInstance(Of ObjectResolver)(javaScriptObjectResolverAttr.ObjectResolverType(type)))
                     End If
                  Catch ex As Exception
                  End Try
               End If
               _converters.Add(type, retVal)
            End SyncLock
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Searches in the custom type attributes for specified attribute type
      ''' </summary>
      Public Shared Function GetCustomAttribute(Of TAttribute As Attribute)(type As Type) As TAttribute
         Dim currentType As Type = type

         While currentType IsNot Nothing
            Dim attrs As Object() = currentType.GetCustomAttributes(GetType(TAttribute), False)

            If attrs IsNot Nothing AndAlso attrs.Length > 0 Then
               Return CType(attrs(0), TAttribute)
            Else
               currentType = currentType.BaseType
            End If
         End While

         Return Nothing
      End Function

      ''' <summary>
      ''' Registers the best fitting javaScriptConverter for type TSerializable
      ''' </summary>
      ''' <typeparam name="TSerializable"></typeparam>
      ''' <remarks></remarks>
      Private Sub RegisterConverter(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable)()
         'auto-select javaScriptConverter to start the serialization
         MyBase.RegisterConverters(New swss.JavaScriptConverter() {GetJavaScriptConverter(GetType(TSerializable))})
      End Sub

      ''' <summary>
      ''' Marks RegisterConverters() as obsolete
      ''' </summary>
      ''' <remarks></remarks>
      <Obsolete("The JavaScriptSerializer automatically selects the best fitting JavaScriptConverter for (de-)serialization.", False)>
      Public Shadows Sub RegisterConverters(converters As IEnumerable(Of swss.JavaScriptConverter))
         MyBase.RegisterConverters(converters)
      End Sub

      ''' <summary>
      ''' Before serialization of obj Serialize() auto-registers the best fitting javaConverter-instance for type TSerializable
      ''' </summary>
      Public Shadows Function Serialize(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable)(obj As TSerializable) As String
         RegisterConverter(Of TSerializable)()
         Return MyBase.Serialize(obj)
      End Function
      ''' <summary>
      ''' Before serialization of objects Serialize() auto-registers the best fitting javaConverter-instance for type TSerializable
      ''' </summary>
      Public Shadows Function SerializeArray(Of TSerializable As CmisObjectModel.Serialization.XmlSerializable)(objects As TSerializable()) As String
         RegisterConverter(Of TSerializable)()
         Return MyBase.Serialize(objects)
      End Function
      ''' <summary>
      ''' Serializes an array as a map
      ''' </summary>
      Public Shadows Function SerializeMap(Of TSerializable As cs.XmlSerializable, TKey)(obj As TSerializable(), keyProperty As Common.Generic.DynamicProperty(Of TSerializable, TKey)) As String
         If obj Is Nothing Then
            Return Nothing
         Else
            Dim arrayProperty As New ccg1.ArrayContainer(Of TSerializable)("Values", obj)
            Dim mapper As New ccg1.ArrayMapper(Of cs.EmptyXmlSerializable, TSerializable, TKey)(cs.EmptyXmlSerializable.Singleton, arrayProperty, keyProperty)
            Return MyBase.Serialize(mapper.JavaExport(Nothing, Me))
         End If
      End Function
   End Class
End Namespace