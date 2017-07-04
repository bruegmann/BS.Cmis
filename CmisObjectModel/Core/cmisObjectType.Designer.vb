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

Namespace CmisObjectModel.Core
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisObjectType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisObjectType
      Inherits Serialization.XmlSerializable

      Public Sub New()
         MyBase.New(CType(True, Boolean?))
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisObjectType, String)) From {
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
         _properties = Read(Of Core.Collections.cmisPropertiesType)(reader, attributeOverrides, "properties", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Collections.cmisPropertiesType))
         _allowableActions = Read(Of Core.cmisAllowableActionsType)(reader, attributeOverrides, "allowableActions", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisAllowableActionsType))
         _relationships = ReadArray(Of Core.cmisObjectType)(reader, attributeOverrides, "relationship", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisObjectType))
         _changeEventInfo = Read(Of Core.cmisChangeEventType)(reader, attributeOverrides, "changeEventInfo", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisChangeEventType))
         _acl = Read(Of Core.Security.cmisAccessControlListType)(reader, attributeOverrides, "acl", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Security.cmisAccessControlListType))
         _exactACL = Read(reader, attributeOverrides, "exactACL", Constants.Namespaces.cmis, _exactACL)
         _policyIds = Read(Of Core.Collections.cmisListOfIdsType)(reader, attributeOverrides, "policyIds", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Collections.cmisListOfIdsType))
         _renditions = ReadArray(Of Core.cmisRenditionType)(reader, attributeOverrides, "rendition", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisRenditionType))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "properties", Constants.Namespaces.cmis, _properties)
         WriteElement(writer, attributeOverrides, "allowableActions", Constants.Namespaces.cmis, _allowableActions)
         WriteArray(writer, attributeOverrides, "relationship", Constants.Namespaces.cmis, _relationships)
         WriteElement(writer, attributeOverrides, "changeEventInfo", Constants.Namespaces.cmis, _changeEventInfo)
         WriteElement(writer, attributeOverrides, "acl", Constants.Namespaces.cmis, _acl)
         If _exactACL.HasValue Then WriteElement(writer, attributeOverrides, "exactACL", Constants.Namespaces.cmis, Convert(_exactACL))
         WriteElement(writer, attributeOverrides, "policyIds", Constants.Namespaces.cmis, _policyIds)
         WriteArray(writer, attributeOverrides, "rendition", Constants.Namespaces.cmis, _renditions)
      End Sub
#End Region

      Protected _acl As Core.Security.cmisAccessControlListType
      Public Overridable Property Acl As Core.Security.cmisAccessControlListType
         Get
            Return _acl
         End Get
         Set(value As Core.Security.cmisAccessControlListType)
            If value IsNot _acl Then
               Dim oldValue As Core.Security.cmisAccessControlListType = _acl
               _acl = value
               OnPropertyChanged("Acl", value, oldValue)
            End If
         End Set
      End Property 'Acl

      Protected _allowableActions As Core.cmisAllowableActionsType
      Public Overridable Property AllowableActions As Core.cmisAllowableActionsType
         Get
            Return _allowableActions
         End Get
         Set(value As Core.cmisAllowableActionsType)
            If value IsNot _allowableActions Then
               Dim oldValue As Core.cmisAllowableActionsType = _allowableActions
               _allowableActions = value
               OnPropertyChanged("AllowableActions", value, oldValue)
            End If
         End Set
      End Property 'AllowableActions

      Protected _changeEventInfo As Core.cmisChangeEventType
      Public Overridable Property ChangeEventInfo As Core.cmisChangeEventType
         Get
            Return _changeEventInfo
         End Get
         Set(value As Core.cmisChangeEventType)
            If value IsNot _changeEventInfo Then
               Dim oldValue As Core.cmisChangeEventType = _changeEventInfo
               _changeEventInfo = value
               OnPropertyChanged("ChangeEventInfo", value, oldValue)
            End If
         End Set
      End Property 'ChangeEventInfo

      Protected _exactACL As Boolean?
      Public Overridable Property ExactACL As Boolean?
         Get
            Return _exactACL
         End Get
         Set(value As Boolean?)
            If Not _exactACL.Equals(value) Then
               Dim oldValue As Boolean? = _exactACL
               _exactACL = value
               OnPropertyChanged("ExactACL", value, oldValue)
            End If
         End Set
      End Property 'ExactACL

      Protected _policyIds As Core.Collections.cmisListOfIdsType
      Public Overridable Property PolicyIds As Core.Collections.cmisListOfIdsType
         Get
            Return _policyIds
         End Get
         Set(value As Core.Collections.cmisListOfIdsType)
            If value IsNot _policyIds Then
               Dim oldValue As Core.Collections.cmisListOfIdsType = _policyIds
               _policyIds = value
               OnPropertyChanged("PolicyIds", value, oldValue)
            End If
         End Set
      End Property 'PolicyIds

      Protected _properties As Core.Collections.cmisPropertiesType
      Public Overridable Property Properties As Core.Collections.cmisPropertiesType
         Get
            Return _properties
         End Get
         Set(value As Core.Collections.cmisPropertiesType)
            If value IsNot _properties Then
               Dim oldValue As Core.Collections.cmisPropertiesType = _properties
               _properties = value
               OnPropertyChanged("Properties", value, oldValue)
            End If
         End Set
      End Property 'Properties

      Protected _relationships As Core.cmisObjectType()
      Public Overridable Property Relationships As Core.cmisObjectType()
         Get
            Return _relationships
         End Get
         Set(value As Core.cmisObjectType())
            If value IsNot _relationships Then
               Dim oldValue As Core.cmisObjectType() = _relationships
               _relationships = value
               OnPropertyChanged("Relationships", value, oldValue)
            End If
         End Set
      End Property 'Relationships

      Protected _renditions As Core.cmisRenditionType()
      Public Overridable Property Renditions As Core.cmisRenditionType()
         Get
            Return _renditions
         End Get
         Set(value As Core.cmisRenditionType())
            If value IsNot _renditions Then
               Dim oldValue As Core.cmisRenditionType() = _renditions
               _renditions = value
               OnPropertyChanged("Renditions", value, oldValue)
            End If
         End Set
      End Property 'Renditions

   End Class
End Namespace