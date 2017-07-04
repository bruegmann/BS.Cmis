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
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core
   Partial Class cmisObjectType

#Region "Java-Serialization support"
      ''' <summary>
      ''' Pass-through to extensions of _properties-instance
      ''' </summary>
      ''' <remarks></remarks>
      <Attributes.JavaScriptConverter(GetType(JSON.Core.cmisObjectTypeConverter.PropertiesExtensionsConverter))>
      Public Class PropertiesExtensions
         Inherits Serialization.XmlSerializable

#Region "Constructors"
         Public Sub New()
            _extensions = New CmisObjectModel.Collections.Generic.ArrayContainer(Of Extensions.Extension)("Extensions")
         End Sub
         Public Sub New(owner As cmisObjectType)
            Dim getter As Func(Of CmisObjectModel.Extensions.Extension()) = Function()
                                                                               If owner Is Nothing OrElse owner._properties Is Nothing Then
                                                                                  Return Nothing
                                                                               Else
                                                                                  Return owner._properties.Extensions
                                                                               End If
                                                                            End Function
            Dim setter As Action(Of CmisObjectModel.Extensions.Extension()) = Sub(value)
                                                                                 If owner IsNot Nothing Then
                                                                                    If value Is Nothing Then
                                                                                       If owner._properties IsNot Nothing Then owner._properties.Extensions = Nothing
                                                                                    ElseIf owner._properties Is Nothing Then
                                                                                       owner.Properties = New Collections.cmisPropertiesType With {.Extensions = value}
                                                                                    Else
                                                                                       owner._properties.Extensions = value
                                                                                    End If
                                                                                 End If
                                                                              End Sub
            _extensions = New Common.Generic.DynamicProperty(Of Extensions.Extension())(getter, setter, "Extensions")
         End Sub
         Public Sub New(owner As CmisObjectModel.Core.Collections.cmisPropertiesType)
            Dim getter As Func(Of CmisObjectModel.Extensions.Extension()) = Function()
                                                                               If owner Is Nothing Then
                                                                                  Return Nothing
                                                                               Else
                                                                                  Return owner.Extensions
                                                                               End If
                                                                            End Function
            Dim setter As Action(Of CmisObjectModel.Extensions.Extension()) = Sub(value)
                                                                                 If owner IsNot Nothing Then
                                                                                    If value Is Nothing Then
                                                                                       owner.Extensions = Nothing
                                                                                    Else
                                                                                       owner.Extensions = value
                                                                                    End If
                                                                                 End If
                                                                              End Sub
            _extensions = New Common.Generic.DynamicProperty(Of Extensions.Extension())(getter, setter, "Extensions")
         End Sub
#End Region

#Region "IXmlSerialization"
         Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
         End Sub

         Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
            Extensions = ReadArray(Of CmisObjectModel.Extensions.Extension)(reader, attributeOverrides, Nothing, AddressOf CmisObjectModel.Extensions.Extension.CreateInstance)
         End Sub

         Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
            WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmis, Extensions)
         End Sub
#End Region

         Protected _extensions As Common.Generic.DynamicProperty(Of CmisObjectModel.Extensions.Extension())
         Public Overridable Property Extensions As CmisObjectModel.Extensions.Extension()
            Get
               Return _extensions.Value
            End Get
            Set(value As CmisObjectModel.Extensions.Extension())
               Dim oldValue As CmisObjectModel.Extensions.Extension() = Extensions

               If value IsNot Extensions Then
                  _extensions.Value = value
                  OnPropertyChanged("Extensions", value, oldValue)
               End If
            End Set
         End Property 'Extensions
      End Class

      ''' <summary>
      ''' pass-through for extensions of properties
      ''' </summary>
      Public Property PropertiesExtension As PropertiesExtensions
         Get
            Return New PropertiesExtensions(Me)
         End Get
         Set(value As PropertiesExtensions)
            Dim extensions As CmisObjectModel.Extensions.Extension() = If(value Is Nothing, Nothing, value.Extensions)

            If extensions Is Nothing Then
               If _properties IsNot Nothing Then _properties.Extensions = Nothing
            ElseIf _properties Is Nothing Then
               Me.Properties = New Collections.cmisPropertiesType With {.Extensions = extensions}
            Else
               _properties.Extensions = extensions
            End If
         End Set
      End Property 'PropertiesExtension

#End Region

   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core
   ''' <summary>
   ''' Converter for propertiesExtension of a cmisObjectType-Instance
   ''' </summary>
   ''' <remarks></remarks>
   Public Class cmisObjectTypeConverter

      ''' <summary>
      ''' Converter for PropertiesExtension-property
      ''' </summary>
      ''' <remarks>Serializes the extensions of the cmisObjectType.Properties-property into a IDictionary(Of String, Object)
      ''' with one item. Key: 'extensions', value: array of extensions.</remarks>
      Public Class PropertiesExtensionsConverter
         Inherits JSON.Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Core.cmisObjectType.PropertiesExtensions)

         Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.cmisObjectType.PropertiesExtensions))
            MyBase.New(objectResolver)
         End Sub

         ''' <summary>
         ''' Deserializes the extensions of the cmisPropertiesType-class
         ''' </summary>
         ''' <param name="context"></param>
         ''' <remarks>Supported formats:
         ''' 1. Key: "extensions", Value: array of IDictionary(Of String, Object)
         ''' 2. Keys: names of known ExtensionTypeNames, Values: IDictionary(Of String, Object) or IDictionary(Of String, Object)()</remarks>
         Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
            With context
               If .Dictionary.ContainsKey("extensions") Then
                  .Object.Extensions = ReadArray(context, "extensions", .Object.Extensions)
               Else
                  Dim extension As CmisObjectModel.Extensions.Extension
                  Dim extensions As New List(Of CmisObjectModel.Extensions.Extension)

                  For Each de As KeyValuePair(Of String, Object) In .Dictionary
                     If TypeOf de.Value Is IDictionary(Of String, Object) Then
                        extension = DeserializeExtension(context, de.Key, de.Value)
                        If extension IsNot Nothing Then extensions.Add(extension)
                     ElseIf TypeOf de.Value Is ICollection Then
                        For Each value As Object In CType(de.Value, ICollection)
                           extension = DeserializeExtension(context, de.Key, value)
                           If extension IsNot Nothing Then extensions.Add(extension)
                        Next
                     End If
                  Next
                  .Object.Extensions = extensions.ToArray()
               End If
            End With
         End Sub

         ''' <summary>
         ''' Deserializes a single extension
         ''' </summary>
         Private Function DeserializeExtension(context As SerializationContext,
                                               extensionTypeName As String, value As Object) As CmisObjectModel.Extensions.Extension
            If TypeOf value Is IDictionary(Of String, Object) Then
               Dim extensionType As Type = CmisObjectModel.Extensions.Extension.GetExtensionType(extensionTypeName)

               If extensionType IsNot Nothing Then
                  Dim javaScriptConverter As swss.JavaScriptConverter = context.Serializer.GetJavaScriptConverter(extensionType)
                  If javaScriptConverter IsNot Nothing Then
                     Return CType(javaScriptConverter.Deserialize(CType(value, IDictionary(Of String, Object)), extensionType, context.Serializer), CmisObjectModel.Extensions.Extension)
                  End If
               End If
            End If

            Return Nothing
         End Function

         Protected Overloads Overrides Sub Serialize(context As SerializationContext)
            With context
               WriteArray(context, .Object.Extensions, "extensions")
            End With
         End Sub
      End Class

      ''' <summary>
      ''' Extended version of PropertiesExtensionsConverter (see Serialize())
      ''' </summary>
      ''' <remarks>Serializes the extensions of the cmisObjectType.Properties-property into a IDictionary(Of String, Object).
      ''' Keys: the extensionTypeNames found in cmisObjectType.Properties.Extensions,
      ''' values: if more than one extension-instance of the same type exist cmisObjectType.Properties.Extensions, they are
      '''         collected in an array of extensions, otherwise it is a single extension</remarks>
      Public Class PropertiesExtensionsExConverter
         Inherits PropertiesExtensionsConverter

#Region "Constructors"
         Public Sub New()
            MyBase.New(New Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Core.cmisObjectType.PropertiesExtensions))
         End Sub
         Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.cmisObjectType.PropertiesExtensions))
            MyBase.New(objectResolver)
         End Sub
#End Region

         ''' <summary>
         ''' Collects extensions of the same type and serializes them in single items within the context.Dictionary
         ''' </summary>
         ''' <param name="context"></param>
         ''' <remarks></remarks>
         Protected Overloads Overrides Sub Serialize(context As SerializationContext)
            With context
               Dim extensions As New Dictionary(Of String, List(Of IDictionary(Of String, Object)))

               'collect same types
               For Each extension As CmisObjectModel.Extensions.Extension In .Object.Extensions
                  Dim key As String = If(extension Is Nothing, Nothing, extension.ExtensionTypeName)
                  If Not String.IsNullOrEmpty(key) Then
                     Dim javaScriptConverter As swss.JavaScriptConverter = context.Serializer.GetJavaScriptConverter(extension.GetType())
                     If javaScriptConverter IsNot Nothing Then
                        Dim dictionary As IDictionary(Of String, Object) = javaScriptConverter.Serialize(extension, context.Serializer)

                        If dictionary IsNot Nothing Then
                           If Not extensions.ContainsKey(key) Then
                              extensions.Add(key, New List(Of IDictionary(Of String, Object)))
                           End If
                           extensions(key).Add(dictionary)
                        End If
                     End If
                  End If
               Next

               'transfer to output
               For Each de As KeyValuePair(Of String, List(Of IDictionary(Of String, Object))) In extensions
                  If de.Value.Count = 1 Then
                     .Dictionary.Add(de.Key, de.Value(0))
                  Else
                     .Dictionary.Add(de.Key, de.Value.ToArray())
                  End If
               Next
            End With
         End Sub
      End Class

      ''' <summary>
      ''' Specialized Read() overload to preserve the properties-extensions
      ''' </summary>
      Protected Overloads Function Read(context As SerializationContext, propertyName As String,
                                        defaultValue As CmisObjectModel.Core.Collections.cmisPropertiesType) As CmisObjectModel.Core.Collections.cmisPropertiesType
         Dim retVal As CmisObjectModel.Core.Collections.cmisPropertiesType = MyBase.Read(context, propertyName, defaultValue)

         'preserve extensions (doing so the code is independent from the order of deserialization of the properties)
         If Not (defaultValue Is Nothing OrElse retVal Is Nothing) Then retVal.Extensions = defaultValue.Extensions

         Return retVal
      End Function

   End Class
End Namespace