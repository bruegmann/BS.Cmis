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
   ''' see getObjectByPath
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Messaging.xsd
   ''' </remarks>
   <sxs.XmlRoot("getObjectByPath", Namespace:=Constants.Namespaces.cmism)>
   Public Class getObjectByPath
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
      Private Shared _setter As New Dictionary(Of String, Action(Of getObjectByPath, String)) From {
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
         _path = Read(reader, attributeOverrides, "path", Constants.Namespaces.cmism, _path)
         _filter = Read(reader, attributeOverrides, "filter", Constants.Namespaces.cmism, _filter)
         _includeAllowableActions = Read(reader, attributeOverrides, "includeAllowableActions", Constants.Namespaces.cmism, _includeAllowableActions)
         _includeRelationships = ReadOptionalEnum(reader, attributeOverrides, "includeRelationships", Constants.Namespaces.cmism, _includeRelationships)
         _renditionFilter = Read(reader, attributeOverrides, "renditionFilter", Constants.Namespaces.cmism, _renditionFilter)
         _includePolicyIds = Read(reader, attributeOverrides, "includePolicyIds", Constants.Namespaces.cmism, _includePolicyIds)
         _includeACL = Read(reader, attributeOverrides, "includeACL", Constants.Namespaces.cmism, _includeACL)
         _extension = Read(Of Messaging.cmisExtensionType)(reader, attributeOverrides, "extension", Constants.Namespaces.cmism, AddressOf GenericXmlSerializableFactory(Of Messaging.cmisExtensionType))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "repositoryId", Constants.Namespaces.cmism, _repositoryId)
         WriteElement(writer, attributeOverrides, "path", Constants.Namespaces.cmism, _path)
         If Not String.IsNullOrEmpty(_filter) Then WriteElement(writer, attributeOverrides, "filter", Constants.Namespaces.cmism, _filter)
         If _includeAllowableActions.HasValue Then WriteElement(writer, attributeOverrides, "includeAllowableActions", Constants.Namespaces.cmism, Convert(_includeAllowableActions))
         If _includeRelationships.HasValue Then WriteElement(writer, attributeOverrides, "includeRelationships", Constants.Namespaces.cmism, _includeRelationships.Value.GetName())
         If Not String.IsNullOrEmpty(_renditionFilter) Then WriteElement(writer, attributeOverrides, "renditionFilter", Constants.Namespaces.cmism, _renditionFilter)
         If _includePolicyIds.HasValue Then WriteElement(writer, attributeOverrides, "includePolicyIds", Constants.Namespaces.cmism, Convert(_includePolicyIds))
         If _includeACL.HasValue Then WriteElement(writer, attributeOverrides, "includeACL", Constants.Namespaces.cmism, Convert(_includeACL))
         WriteElement(writer, attributeOverrides, "extension", Constants.Namespaces.cmism, _extension)
      End Sub
#End Region

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

      Protected _filter As String
      Public Overridable Property Filter As String
         Get
            Return _filter
         End Get
         Set(value As String)
            If _filter <> value Then
               Dim oldValue As String = _filter
               _filter = value
               OnPropertyChanged("Filter", value, oldValue)
            End If
         End Set
      End Property 'Filter

      Protected _includeACL As Boolean?
      Public Overridable Property IncludeACL As Boolean?
         Get
            Return _includeACL
         End Get
         Set(value As Boolean?)
            If Not _includeACL.Equals(value) Then
               Dim oldValue As Boolean? = _includeACL
               _includeACL = value
               OnPropertyChanged("IncludeACL", value, oldValue)
            End If
         End Set
      End Property 'IncludeACL

      Protected _includeAllowableActions As Boolean?
      Public Overridable Property IncludeAllowableActions As Boolean?
         Get
            Return _includeAllowableActions
         End Get
         Set(value As Boolean?)
            If Not _includeAllowableActions.Equals(value) Then
               Dim oldValue As Boolean? = _includeAllowableActions
               _includeAllowableActions = value
               OnPropertyChanged("IncludeAllowableActions", value, oldValue)
            End If
         End Set
      End Property 'IncludeAllowableActions

      Protected _includePolicyIds As Boolean?
      Public Overridable Property IncludePolicyIds As Boolean?
         Get
            Return _includePolicyIds
         End Get
         Set(value As Boolean?)
            If Not _includePolicyIds.Equals(value) Then
               Dim oldValue As Boolean? = _includePolicyIds
               _includePolicyIds = value
               OnPropertyChanged("IncludePolicyIds", value, oldValue)
            End If
         End Set
      End Property 'IncludePolicyIds

      Protected _includeRelationships As Core.enumIncludeRelationships?
      Public Overridable Property IncludeRelationships As Core.enumIncludeRelationships?
         Get
            Return _includeRelationships
         End Get
         Set(value As Core.enumIncludeRelationships?)
            If Not _includeRelationships.Equals(value) Then
               Dim oldValue As Core.enumIncludeRelationships? = _includeRelationships
               _includeRelationships = value
               OnPropertyChanged("IncludeRelationships", value, oldValue)
            End If
         End Set
      End Property 'IncludeRelationships

      Protected _path As String
      Public Overridable Property Path As String
         Get
            Return _path
         End Get
         Set(value As String)
            If _path <> value Then
               Dim oldValue As String = _path
               _path = value
               OnPropertyChanged("Path", value, oldValue)
            End If
         End Set
      End Property 'Path

      Protected _renditionFilter As String
      Public Overridable Property RenditionFilter As String
         Get
            Return _renditionFilter
         End Get
         Set(value As String)
            If _renditionFilter <> value Then
               Dim oldValue As String = _renditionFilter
               _renditionFilter = value
               OnPropertyChanged("RenditionFilter", value, oldValue)
            End If
         End Set
      End Property 'RenditionFilter

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

   End Class
End Namespace