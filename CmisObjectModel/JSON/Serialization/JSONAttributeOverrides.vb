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
Imports cjs = CmisObjectModel.JSON.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Serialization
   ''' <summary>
   ''' Customization of JavaScriptSerialization
   ''' </summary>
   ''' <remarks></remarks>
   Public Class JSONAttributeOverrides

#Region "Helper classes"
      ''' <summary>
      ''' Customization of JavaScriptSerialization for an element of a type
      ''' </summary>
      ''' <remarks></remarks>
      Public Class JSONElementAttribute
         Public Sub New(aliasName As String, elementConverter As cjs.JavaScriptConverter)
            Me.AliasName = If(aliasName, String.Empty)
            Me.ElementConverter = elementConverter
         End Sub

         Public ReadOnly AliasName As String
         Public ReadOnly ElementConverter As cjs.JavaScriptConverter
      End Class

      ''' <summary>
      ''' Customization of JavaScriptSerialization for a type and/or elements of a type
      ''' </summary>
      ''' <remarks></remarks>
      Public Class JSONTypeAttribute
         Public ReadOnly ElementAttributes As New System.Collections.Generic.Dictionary(Of String, JSONElementAttribute)
         Public TypeConverter As cjs.JavaScriptConverter
      End Class
#End Region

      Private ReadOnly _attributes As New System.Collections.Generic.Dictionary(Of Type, JSONTypeAttribute)

      ''' <summary>
      ''' Gets or sets the serialization overrides for an element of a type
      ''' </summary>
      Public Property ElementAttribute(type As Type, elementName As String) As JSONElementAttribute
         Get
            Dim jsonAttribute As JSONTypeAttribute = If(_attributes.ContainsKey(type), _attributes(type), Nothing)
            Return If(jsonAttribute Is Nothing OrElse Not jsonAttribute.ElementAttributes.ContainsKey(elementName),
                      Nothing, jsonAttribute.ElementAttributes(elementName))
         End Get
         Set(value As JSONElementAttribute)
            Dim jsonAttribute As JSONTypeAttribute = GetOrCreateAttribute(type)

            If value Is Nothing Then
               jsonAttribute.ElementAttributes.Remove(elementName)
            ElseIf jsonAttribute.ElementAttributes.ContainsKey(elementName) Then
               jsonAttribute.ElementAttributes(elementName) = value
            Else
               jsonAttribute.ElementAttributes.Add(elementName, value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Creates a new ElementConverter for type, elementName and aliasName
      ''' </summary>
      Public WriteOnly Property ElementConverter(type As Type, elementName As String, aliasName As String) As cjs.JavaScriptConverter
         Set(value As cjs.JavaScriptConverter)
            If Not (type Is Nothing OrElse String.IsNullOrEmpty(elementName) OrElse String.IsNullOrEmpty(aliasName)) Then
               ElementAttribute(type, elementName) = New JSONElementAttribute(aliasName, value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Returns a valid JSONAttribute for given type
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetOrCreateAttribute(type As Type) As JSONTypeAttribute
         If _attributes.ContainsKey(type) Then
            Return _attributes(type)
         Else
            Dim retVal As New JSONTypeAttribute()
            _attributes.Add(type, retVal)
            Return retVal
         End If
      End Function

      ''' <summary>
      ''' Gets or sets the serialization overrides for a type and its elements
      ''' </summary>
      ''' <param name="type"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property TypeAttribute(type As Type) As JSONTypeAttribute
         Get
            Return If(_attributes.ContainsKey(type), _attributes(type), Nothing)
         End Get
         Set(value As JSONTypeAttribute)
            If value Is Nothing Then
               _attributes.Remove(type)
            ElseIf _attributes.ContainsKey(type) Then
               _attributes(type) = value
            Else
               _attributes.Add(type, value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Gets or sets the serialization overrides for a type
      ''' </summary>
      Public Property TypeConverter(type As Type) As cjs.JavaScriptConverter
         Get
            Return If(_attributes.ContainsKey(type), _attributes(type).TypeConverter, Nothing)
         End Get
         Set(value As cjs.JavaScriptConverter)
            GetOrCreateAttribute(type).TypeConverter = value
         End Set
      End Property

   End Class
End Namespace