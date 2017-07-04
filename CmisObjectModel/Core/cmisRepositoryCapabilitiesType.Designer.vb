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
   ''' see cmisRepositoryCapabilitiesType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisRepositoryCapabilitiesType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisRepositoryCapabilitiesType, String)) From {
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
         _capabilityACL = ReadEnum(reader, attributeOverrides, "capabilityACL", Constants.Namespaces.cmis, _capabilityACL)
         _capabilityAllVersionsSearchable = Read(reader, attributeOverrides, "capabilityAllVersionsSearchable", Constants.Namespaces.cmis, _capabilityAllVersionsSearchable)
         _capabilityChanges = ReadEnum(reader, attributeOverrides, "capabilityChanges", Constants.Namespaces.cmis, _capabilityChanges)
         _capabilityContentStreamUpdatability = ReadEnum(reader, attributeOverrides, "capabilityContentStreamUpdatability", Constants.Namespaces.cmis, _capabilityContentStreamUpdatability)
         _capabilityGetDescendants = Read(reader, attributeOverrides, "capabilityGetDescendants", Constants.Namespaces.cmis, _capabilityGetDescendants)
         _capabilityGetFolderTree = Read(reader, attributeOverrides, "capabilityGetFolderTree", Constants.Namespaces.cmis, _capabilityGetFolderTree)
         _capabilityOrderBy = ReadEnum(reader, attributeOverrides, "capabilityOrderBy", Constants.Namespaces.cmis, _capabilityOrderBy)
         _capabilityMultifiling = Read(reader, attributeOverrides, "capabilityMultifiling", Constants.Namespaces.cmis, _capabilityMultifiling)
         _capabilityPWCSearchable = Read(reader, attributeOverrides, "capabilityPWCSearchable", Constants.Namespaces.cmis, _capabilityPWCSearchable)
         _capabilityPWCUpdatable = Read(reader, attributeOverrides, "capabilityPWCUpdatable", Constants.Namespaces.cmis, _capabilityPWCUpdatable)
         _capabilityQuery = ReadEnum(reader, attributeOverrides, "capabilityQuery", Constants.Namespaces.cmis, _capabilityQuery)
         _capabilityRenditions = ReadEnum(reader, attributeOverrides, "capabilityRenditions", Constants.Namespaces.cmis, _capabilityRenditions)
         _capabilityUnfiling = Read(reader, attributeOverrides, "capabilityUnfiling", Constants.Namespaces.cmis, _capabilityUnfiling)
         _capabilityVersionSpecificFiling = Read(reader, attributeOverrides, "capabilityVersionSpecificFiling", Constants.Namespaces.cmis, _capabilityVersionSpecificFiling)
         _capabilityJoin = ReadEnum(reader, attributeOverrides, "capabilityJoin", Constants.Namespaces.cmis, _capabilityJoin)
         _capabilityCreatablePropertyTypes = Read(Of Core.cmisCreatablePropertyTypesType)(reader, attributeOverrides, "capabilityCreatablePropertyTypes", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisCreatablePropertyTypesType))
         _capabilityNewTypeSettableAttributes = Read(Of Core.cmisNewTypeSettableAttributes)(reader, attributeOverrides, "capabilityNewTypeSettableAttributes", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisNewTypeSettableAttributes))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "capabilityACL", Constants.Namespaces.cmis, _capabilityACL.GetName())
         WriteElement(writer, attributeOverrides, "capabilityAllVersionsSearchable", Constants.Namespaces.cmis, Convert(_capabilityAllVersionsSearchable))
         WriteElement(writer, attributeOverrides, "capabilityChanges", Constants.Namespaces.cmis, _capabilityChanges.GetName())
         WriteElement(writer, attributeOverrides, "capabilityContentStreamUpdatability", Constants.Namespaces.cmis, _capabilityContentStreamUpdatability.GetName())
         WriteElement(writer, attributeOverrides, "capabilityGetDescendants", Constants.Namespaces.cmis, Convert(_capabilityGetDescendants))
         WriteElement(writer, attributeOverrides, "capabilityGetFolderTree", Constants.Namespaces.cmis, Convert(_capabilityGetFolderTree))
         WriteElement(writer, attributeOverrides, "capabilityOrderBy", Constants.Namespaces.cmis, _capabilityOrderBy.GetName())
         WriteElement(writer, attributeOverrides, "capabilityMultifiling", Constants.Namespaces.cmis, Convert(_capabilityMultifiling))
         WriteElement(writer, attributeOverrides, "capabilityPWCSearchable", Constants.Namespaces.cmis, Convert(_capabilityPWCSearchable))
         WriteElement(writer, attributeOverrides, "capabilityPWCUpdatable", Constants.Namespaces.cmis, Convert(_capabilityPWCUpdatable))
         WriteElement(writer, attributeOverrides, "capabilityQuery", Constants.Namespaces.cmis, _capabilityQuery.GetName())
         WriteElement(writer, attributeOverrides, "capabilityRenditions", Constants.Namespaces.cmis, _capabilityRenditions.GetName())
         WriteElement(writer, attributeOverrides, "capabilityUnfiling", Constants.Namespaces.cmis, Convert(_capabilityUnfiling))
         WriteElement(writer, attributeOverrides, "capabilityVersionSpecificFiling", Constants.Namespaces.cmis, Convert(_capabilityVersionSpecificFiling))
         WriteElement(writer, attributeOverrides, "capabilityJoin", Constants.Namespaces.cmis, _capabilityJoin.GetName())
         WriteElement(writer, attributeOverrides, "capabilityCreatablePropertyTypes", Constants.Namespaces.cmis, _capabilityCreatablePropertyTypes)
         WriteElement(writer, attributeOverrides, "capabilityNewTypeSettableAttributes", Constants.Namespaces.cmis, _capabilityNewTypeSettableAttributes)
      End Sub
#End Region

      Protected _capabilityACL As Core.enumCapabilityACL
      Public Overridable Property CapabilityACL As Core.enumCapabilityACL
         Get
            Return _capabilityACL
         End Get
         Set(value As Core.enumCapabilityACL)
            If _capabilityACL <> value Then
               Dim oldValue As Core.enumCapabilityACL = _capabilityACL
               _capabilityACL = value
               OnPropertyChanged("CapabilityACL", value, oldValue)
            End If
         End Set
      End Property 'CapabilityACL

      Protected _capabilityAllVersionsSearchable As Boolean
      Public Overridable Property CapabilityAllVersionsSearchable As Boolean
         Get
            Return _capabilityAllVersionsSearchable
         End Get
         Set(value As Boolean)
            If _capabilityAllVersionsSearchable <> value Then
               Dim oldValue As Boolean = _capabilityAllVersionsSearchable
               _capabilityAllVersionsSearchable = value
               OnPropertyChanged("CapabilityAllVersionsSearchable", value, oldValue)
            End If
         End Set
      End Property 'CapabilityAllVersionsSearchable

      Protected _capabilityChanges As Core.enumCapabilityChanges
      Public Overridable Property CapabilityChanges As Core.enumCapabilityChanges
         Get
            Return _capabilityChanges
         End Get
         Set(value As Core.enumCapabilityChanges)
            If _capabilityChanges <> value Then
               Dim oldValue As Core.enumCapabilityChanges = _capabilityChanges
               _capabilityChanges = value
               OnPropertyChanged("CapabilityChanges", value, oldValue)
            End If
         End Set
      End Property 'CapabilityChanges

      Protected _capabilityContentStreamUpdatability As Core.enumCapabilityContentStreamUpdates
      Public Overridable Property CapabilityContentStreamUpdatability As Core.enumCapabilityContentStreamUpdates
         Get
            Return _capabilityContentStreamUpdatability
         End Get
         Set(value As Core.enumCapabilityContentStreamUpdates)
            If _capabilityContentStreamUpdatability <> value Then
               Dim oldValue As Core.enumCapabilityContentStreamUpdates = _capabilityContentStreamUpdatability
               _capabilityContentStreamUpdatability = value
               OnPropertyChanged("CapabilityContentStreamUpdatability", value, oldValue)
            End If
         End Set
      End Property 'CapabilityContentStreamUpdatability

      Protected _capabilityCreatablePropertyTypes As Core.cmisCreatablePropertyTypesType
      Public Overridable Property CapabilityCreatablePropertyTypes As Core.cmisCreatablePropertyTypesType
         Get
            Return _capabilityCreatablePropertyTypes
         End Get
         Set(value As Core.cmisCreatablePropertyTypesType)
            If value IsNot _capabilityCreatablePropertyTypes Then
               Dim oldValue As Core.cmisCreatablePropertyTypesType = _capabilityCreatablePropertyTypes
               _capabilityCreatablePropertyTypes = value
               OnPropertyChanged("CapabilityCreatablePropertyTypes", value, oldValue)
            End If
         End Set
      End Property 'CapabilityCreatablePropertyTypes

      Protected _capabilityGetDescendants As Boolean
      Public Overridable Property CapabilityGetDescendants As Boolean
         Get
            Return _capabilityGetDescendants
         End Get
         Set(value As Boolean)
            If _capabilityGetDescendants <> value Then
               Dim oldValue As Boolean = _capabilityGetDescendants
               _capabilityGetDescendants = value
               OnPropertyChanged("CapabilityGetDescendants", value, oldValue)
            End If
         End Set
      End Property 'CapabilityGetDescendants

      Protected _capabilityGetFolderTree As Boolean
      Public Overridable Property CapabilityGetFolderTree As Boolean
         Get
            Return _capabilityGetFolderTree
         End Get
         Set(value As Boolean)
            If _capabilityGetFolderTree <> value Then
               Dim oldValue As Boolean = _capabilityGetFolderTree
               _capabilityGetFolderTree = value
               OnPropertyChanged("CapabilityGetFolderTree", value, oldValue)
            End If
         End Set
      End Property 'CapabilityGetFolderTree

      Protected _capabilityJoin As Core.enumCapabilityJoin
      Public Overridable Property CapabilityJoin As Core.enumCapabilityJoin
         Get
            Return _capabilityJoin
         End Get
         Set(value As Core.enumCapabilityJoin)
            If _capabilityJoin <> value Then
               Dim oldValue As Core.enumCapabilityJoin = _capabilityJoin
               _capabilityJoin = value
               OnPropertyChanged("CapabilityJoin", value, oldValue)
            End If
         End Set
      End Property 'CapabilityJoin

      Protected _capabilityMultifiling As Boolean
      Public Overridable Property CapabilityMultifiling As Boolean
         Get
            Return _capabilityMultifiling
         End Get
         Set(value As Boolean)
            If _capabilityMultifiling <> value Then
               Dim oldValue As Boolean = _capabilityMultifiling
               _capabilityMultifiling = value
               OnPropertyChanged("CapabilityMultifiling", value, oldValue)
            End If
         End Set
      End Property 'CapabilityMultifiling

      Protected _capabilityNewTypeSettableAttributes As Core.cmisNewTypeSettableAttributes
      Public Overridable Property CapabilityNewTypeSettableAttributes As Core.cmisNewTypeSettableAttributes
         Get
            Return _capabilityNewTypeSettableAttributes
         End Get
         Set(value As Core.cmisNewTypeSettableAttributes)
            If value IsNot _capabilityNewTypeSettableAttributes Then
               Dim oldValue As Core.cmisNewTypeSettableAttributes = _capabilityNewTypeSettableAttributes
               _capabilityNewTypeSettableAttributes = value
               OnPropertyChanged("CapabilityNewTypeSettableAttributes", value, oldValue)
            End If
         End Set
      End Property 'CapabilityNewTypeSettableAttributes

      Protected _capabilityOrderBy As Core.enumCapabilityOrderBy
      Public Overridable Property CapabilityOrderBy As Core.enumCapabilityOrderBy
         Get
            Return _capabilityOrderBy
         End Get
         Set(value As Core.enumCapabilityOrderBy)
            If _capabilityOrderBy <> value Then
               Dim oldValue As Core.enumCapabilityOrderBy = _capabilityOrderBy
               _capabilityOrderBy = value
               OnPropertyChanged("CapabilityOrderBy", value, oldValue)
            End If
         End Set
      End Property 'CapabilityOrderBy

      Protected _capabilityPWCSearchable As Boolean
      Public Overridable Property CapabilityPWCSearchable As Boolean
         Get
            Return _capabilityPWCSearchable
         End Get
         Set(value As Boolean)
            If _capabilityPWCSearchable <> value Then
               Dim oldValue As Boolean = _capabilityPWCSearchable
               _capabilityPWCSearchable = value
               OnPropertyChanged("CapabilityPWCSearchable", value, oldValue)
            End If
         End Set
      End Property 'CapabilityPWCSearchable

      Protected _capabilityPWCUpdatable As Boolean
      Public Overridable Property CapabilityPWCUpdatable As Boolean
         Get
            Return _capabilityPWCUpdatable
         End Get
         Set(value As Boolean)
            If _capabilityPWCUpdatable <> value Then
               Dim oldValue As Boolean = _capabilityPWCUpdatable
               _capabilityPWCUpdatable = value
               OnPropertyChanged("CapabilityPWCUpdatable", value, oldValue)
            End If
         End Set
      End Property 'CapabilityPWCUpdatable

      Protected _capabilityQuery As Core.enumCapabilityQuery
      Public Overridable Property CapabilityQuery As Core.enumCapabilityQuery
         Get
            Return _capabilityQuery
         End Get
         Set(value As Core.enumCapabilityQuery)
            If _capabilityQuery <> value Then
               Dim oldValue As Core.enumCapabilityQuery = _capabilityQuery
               _capabilityQuery = value
               OnPropertyChanged("CapabilityQuery", value, oldValue)
            End If
         End Set
      End Property 'CapabilityQuery

      Protected _capabilityRenditions As Core.enumCapabilityRendition
      Public Overridable Property CapabilityRenditions As Core.enumCapabilityRendition
         Get
            Return _capabilityRenditions
         End Get
         Set(value As Core.enumCapabilityRendition)
            If _capabilityRenditions <> value Then
               Dim oldValue As Core.enumCapabilityRendition = _capabilityRenditions
               _capabilityRenditions = value
               OnPropertyChanged("CapabilityRenditions", value, oldValue)
            End If
         End Set
      End Property 'CapabilityRenditions

      Protected _capabilityUnfiling As Boolean
      Public Overridable Property CapabilityUnfiling As Boolean
         Get
            Return _capabilityUnfiling
         End Get
         Set(value As Boolean)
            If _capabilityUnfiling <> value Then
               Dim oldValue As Boolean = _capabilityUnfiling
               _capabilityUnfiling = value
               OnPropertyChanged("CapabilityUnfiling", value, oldValue)
            End If
         End Set
      End Property 'CapabilityUnfiling

      Protected _capabilityVersionSpecificFiling As Boolean
      Public Overridable Property CapabilityVersionSpecificFiling As Boolean
         Get
            Return _capabilityVersionSpecificFiling
         End Get
         Set(value As Boolean)
            If _capabilityVersionSpecificFiling <> value Then
               Dim oldValue As Boolean = _capabilityVersionSpecificFiling
               _capabilityVersionSpecificFiling = value
               OnPropertyChanged("CapabilityVersionSpecificFiling", value, oldValue)
            End If
         End Set
      End Property 'CapabilityVersionSpecificFiling

   End Class
End Namespace