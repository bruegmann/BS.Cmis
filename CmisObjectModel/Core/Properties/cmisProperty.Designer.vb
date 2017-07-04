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

Namespace CmisObjectModel.Core.Properties
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisProperty
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public MustInherit Class cmisProperty
      Inherits Serialization.XmlSerializable

      Protected Sub New()
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisProperty, String)) From {
         {"displayname", AddressOf SetDisplayName},
         {"localname", AddressOf SetLocalName},
         {"propertydefinitionid", AddressOf SetPropertyDefinitionId},
         {"queryname", AddressOf SetQueryName}} '_setter

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
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If Not String.IsNullOrEmpty(_propertyDefinitionId) Then WriteAttribute(writer, attributeOverrides, "propertyDefinitionId", Nothing, _propertyDefinitionId)
         If Not String.IsNullOrEmpty(_localName) Then WriteAttribute(writer, attributeOverrides, "localName", Nothing, _localName)
         If Not String.IsNullOrEmpty(_displayName) Then WriteAttribute(writer, attributeOverrides, "displayName", Nothing, _displayName)
         If Not String.IsNullOrEmpty(_queryName) Then WriteAttribute(writer, attributeOverrides, "queryName", Nothing, _queryName)
      End Sub
#End Region

      Protected _displayName As String
      Public Overridable Property DisplayName As String
         Get
            Return _displayName
         End Get
         Set(value As String)
            If _displayName <> value Then
               Dim oldValue As String = _displayName
               _displayName = value
               OnPropertyChanged("DisplayName", value, oldValue)
            End If
         End Set
      End Property 'DisplayName
      Private Shared Sub SetDisplayName(instance As cmisProperty, value As String)
         instance.DisplayName = value
      End Sub

      Protected _localName As String
      Public Overridable Property LocalName As String
         Get
            Return _localName
         End Get
         Set(value As String)
            If _localName <> value Then
               Dim oldValue As String = _localName
               _localName = value
               OnPropertyChanged("LocalName", value, oldValue)
            End If
         End Set
      End Property 'LocalName
      Private Shared Sub SetLocalName(instance As cmisProperty, value As String)
         instance.LocalName = value
      End Sub

      Protected _propertyDefinitionId As String
      Public Overridable Property PropertyDefinitionId As String
         Get
            Return _propertyDefinitionId
         End Get
         Set(value As String)
            If _propertyDefinitionId <> value Then
               Dim oldValue As String = _propertyDefinitionId
               _propertyDefinitionId = value
               OnPropertyChanged("PropertyDefinitionId", value, oldValue)
            End If
         End Set
      End Property 'PropertyDefinitionId
      Private Shared Sub SetPropertyDefinitionId(instance As cmisProperty, value As String)
         instance.PropertyDefinitionId = value
      End Sub

      Protected _queryName As String
      Public Overridable Property QueryName As String
         Get
            Return _queryName
         End Get
         Set(value As String)
            If _queryName <> value Then
               Dim oldValue As String = _queryName
               _queryName = value
               OnPropertyChanged("QueryName", value, oldValue)
            End If
         End Set
      End Property 'QueryName
      Private Shared Sub SetQueryName(instance As cmisProperty, value As String)
         instance.QueryName = value
      End Sub

   End Class
End Namespace