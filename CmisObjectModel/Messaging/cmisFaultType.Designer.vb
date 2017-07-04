'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'* Author: auto-generated code
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
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Messaging
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisFaultType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Messaging.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public Class cmisFaultType
      Inherits Serialization.XmlSerializable

      Public Sub New()
      End Sub
      ''' <summary>
      ''' this constructor is only used if derived classes from this class needs an InitClass()-call
      ''' </summary>
      ''' <param name="initClassSupported"></param>
      ''' <remarks></remarks>
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisFaultType, String)) From {
         } '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in attributes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
         'at least one property is serialized in an attribute-value
         If _setter.Count > 0 Then
            For attributeIndex As Integer = 0 To reader.AttributeCount - 1
               reader.MoveToAttribute(attributeIndex)
               'attribute name
               Dim key As String = reader.Name.ToLowerInvariant
               If _setter.ContainsKey(key) Then _setter(key).Invoke(Me, reader.GetAttribute(attributeIndex))
            Next
         End If
      End Sub

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _type = ReadEnum(reader, attributeOverrides, "type", Constants.Namespaces.cmism, _type)
         _code = Read(reader, attributeOverrides, "code", Constants.Namespaces.cmism, _code)
         _message = Read(reader, attributeOverrides, "message", Constants.Namespaces.cmism, _message)
         _extensions = ReadArray(Of CmisObjectModel.Extensions.Extension)(reader, attributeOverrides, Nothing, AddressOf CmisObjectModel.Extensions.Extension.CreateInstance)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "type", Constants.Namespaces.cmism, _type.GetName())
         WriteElement(writer, attributeOverrides, "code", Constants.Namespaces.cmism, Convert(_code))
         WriteElement(writer, attributeOverrides, "message", Constants.Namespaces.cmism, _message)
         WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmism, _extensions)
      End Sub
#End Region

      Protected _code As xs_Integer
      Public Overridable Property Code As xs_Integer
         Get
            Return _code
         End Get
         Set(value As xs_Integer)
            If _code <> value Then
               Dim oldValue As xs_Integer = _code
               _code = value
               OnPropertyChanged("Code", value, oldValue)
            End If
         End Set
      End Property 'Code

      Protected _extensions As CmisObjectModel.Extensions.Extension()
      Public Overridable Property Extensions As CmisObjectModel.Extensions.Extension()
         Get
            Return _extensions
         End Get
         Set(value As CmisObjectModel.Extensions.Extension())
            If value IsNot _extensions Then
               Dim oldValue As CmisObjectModel.Extensions.Extension() = _extensions
               _extensions = value
               OnPropertyChanged("Extensions", value, oldValue)
            End If
         End Set
      End Property 'Extensions

      Protected _message As String
      Public Overridable Property Message As String
         Get
            Return _message
         End Get
         Set(value As String)
            If _message <> value Then
               Dim oldValue As String = _message
               _message = value
               OnPropertyChanged("Message", value, oldValue)
            End If
         End Set
      End Property 'Message

      Protected _type As Messaging.enumServiceException
      Public Overridable Property Type As Messaging.enumServiceException
         Get
            Return _type
         End Get
         Set(value As Messaging.enumServiceException)
            If _type <> value Then
               Dim oldValue As Messaging.enumServiceException = _type
               _type = value
               OnPropertyChanged("Type", value, oldValue)
            End If
         End Set
      End Property 'Type

   End Class
End Namespace