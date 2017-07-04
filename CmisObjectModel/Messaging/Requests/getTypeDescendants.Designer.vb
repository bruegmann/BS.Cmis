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

Namespace CmisObjectModel.Messaging.Requests
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see getTypeDescendants
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Messaging.xsd
   ''' </remarks>
   <sxs.XmlRoot("getTypeDescendants", Namespace:=Constants.Namespaces.cmism)>
   Public Class getTypeDescendants
      Inherits Messaging.Requests.RequestBase

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
      Private Shared _setter As New Dictionary(Of String, Action(Of getTypeDescendants, String)) From {
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
         _repositoryId = Read(reader, attributeOverrides, "repositoryId", Constants.Namespaces.cmism, _repositoryId)
         _typeId = Read(reader, attributeOverrides, "typeId", Constants.Namespaces.cmism, _typeId)
         _depth = Read(reader, attributeOverrides, "depth", Constants.Namespaces.cmism, _depth)
         _includePropertyDefinitions = Read(reader, attributeOverrides, "includePropertyDefinitions", Constants.Namespaces.cmism, _includePropertyDefinitions)
         _extension = Read(Of Messaging.cmisExtensionType)(reader, attributeOverrides, "extension", Constants.Namespaces.cmism, AddressOf GenericXmlSerializableFactory(Of Messaging.cmisExtensionType))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "repositoryId", Constants.Namespaces.cmism, _repositoryId)
         If Not String.IsNullOrEmpty(_typeId) Then WriteElement(writer, attributeOverrides, "typeId", Constants.Namespaces.cmism, _typeId)
         If _depth.HasValue Then WriteElement(writer, attributeOverrides, "depth", Constants.Namespaces.cmism, Convert(_depth))
         If _includePropertyDefinitions.HasValue Then WriteElement(writer, attributeOverrides, "includePropertyDefinitions", Constants.Namespaces.cmism, Convert(_includePropertyDefinitions))
         WriteElement(writer, attributeOverrides, "extension", Constants.Namespaces.cmism, _extension)
      End Sub
#End Region

      Protected _depth As xs_Integer?
      Public Overridable Property Depth As xs_Integer?
         Get
            Return _depth
         End Get
         Set(value As xs_Integer?)
            If Not _depth.Equals(value) Then
               Dim oldValue As xs_Integer? = _depth
               _depth = value
               OnPropertyChanged("Depth", value, oldValue)
            End If
         End Set
      End Property 'Depth

      Protected _extension As Messaging.cmisExtensionType
      Public Overridable Property Extension As Messaging.cmisExtensionType
         Get
            Return _extension
         End Get
         Set(value As Messaging.cmisExtensionType)
            If value IsNot _extension Then
               Dim oldValue As Messaging.cmisExtensionType = _extension
               _extension = value
               OnPropertyChanged("Extension", value, oldValue)
            End If
         End Set
      End Property 'Extension

      Protected _includePropertyDefinitions As Boolean?
      Public Overridable Property IncludePropertyDefinitions As Boolean?
         Get
            Return _includePropertyDefinitions
         End Get
         Set(value As Boolean?)
            If Not _includePropertyDefinitions.Equals(value) Then
               Dim oldValue As Boolean? = _includePropertyDefinitions
               _includePropertyDefinitions = value
               OnPropertyChanged("IncludePropertyDefinitions", value, oldValue)
            End If
         End Set
      End Property 'IncludePropertyDefinitions

      Protected _repositoryId As String
      Public Overridable Property RepositoryId As String
         Get
            Return _repositoryId
         End Get
         Set(value As String)
            If _repositoryId <> value Then
               Dim oldValue As String = _repositoryId
               _repositoryId = value
               OnPropertyChanged("RepositoryId", value, oldValue)
            End If
         End Set
      End Property 'RepositoryId

      Protected _typeId As String
      Public Overridable Property TypeId As String
         Get
            Return _typeId
         End Get
         Set(value As String)
            If _typeId <> value Then
               Dim oldValue As String = _typeId
               _typeId = value
               OnPropertyChanged("TypeId", value, oldValue)
            End If
         End Set
      End Property 'TypeId

   End Class
End Namespace