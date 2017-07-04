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
   ''' see cmisRepositoryInfoType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisRepositoryInfoType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisRepositoryInfoType, String)) From {
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
         _repositoryId = Read(reader, attributeOverrides, "repositoryId", Constants.Namespaces.cmis, _repositoryId)
         _repositoryName = Read(reader, attributeOverrides, "repositoryName", Constants.Namespaces.cmis, _repositoryName)
         _repositoryDescription = Read(reader, attributeOverrides, "repositoryDescription", Constants.Namespaces.cmis, _repositoryDescription)
         _vendorName = Read(reader, attributeOverrides, "vendorName", Constants.Namespaces.cmis, _vendorName)
         _productName = Read(reader, attributeOverrides, "productName", Constants.Namespaces.cmis, _productName)
         _productVersion = Read(reader, attributeOverrides, "productVersion", Constants.Namespaces.cmis, _productVersion)
         _rootFolderId = Read(reader, attributeOverrides, "rootFolderId", Constants.Namespaces.cmis, _rootFolderId)
         _latestChangeLogToken = Read(reader, attributeOverrides, "latestChangeLogToken", Constants.Namespaces.cmis, _latestChangeLogToken)
         _capabilities = Read(Of Core.cmisRepositoryCapabilitiesType)(reader, attributeOverrides, "capabilities", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisRepositoryCapabilitiesType))
         _aclCapability = Read(Of Core.Security.cmisACLCapabilityType)(reader, attributeOverrides, "aclCapability", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Security.cmisACLCapabilityType))
         _cmisVersionSupported = Read(reader, attributeOverrides, "cmisVersionSupported", Constants.Namespaces.cmis, _cmisVersionSupported)
         _thinClientURI = Read(reader, attributeOverrides, "thinClientURI", Constants.Namespaces.cmis, _thinClientURI)
         _changesIncomplete = Read(reader, attributeOverrides, "changesIncomplete", Constants.Namespaces.cmis, _changesIncomplete)
         _changesOnTypes = ReadEnumArray(Of Core.enumBaseObjectTypeIds)(reader, attributeOverrides, "changesOnType", Constants.Namespaces.cmis)
         _principalAnonymous = Read(reader, attributeOverrides, "principalAnonymous", Constants.Namespaces.cmis, _principalAnonymous)
         _principalAnyone = Read(reader, attributeOverrides, "principalAnyone", Constants.Namespaces.cmis, _principalAnyone)
         _extendedFeatures = ReadArray(Of Core.cmisExtensionFeatureType)(reader, attributeOverrides, "extendedFeatures", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisExtensionFeatureType))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "repositoryId", Constants.Namespaces.cmis, _repositoryId)
         WriteElement(writer, attributeOverrides, "repositoryName", Constants.Namespaces.cmis, _repositoryName)
         WriteElement(writer, attributeOverrides, "repositoryDescription", Constants.Namespaces.cmis, _repositoryDescription)
         WriteElement(writer, attributeOverrides, "vendorName", Constants.Namespaces.cmis, _vendorName)
         WriteElement(writer, attributeOverrides, "productName", Constants.Namespaces.cmis, _productName)
         WriteElement(writer, attributeOverrides, "productVersion", Constants.Namespaces.cmis, _productVersion)
         WriteElement(writer, attributeOverrides, "rootFolderId", Constants.Namespaces.cmis, _rootFolderId)
         If Not String.IsNullOrEmpty(_latestChangeLogToken) Then WriteElement(writer, attributeOverrides, "latestChangeLogToken", Constants.Namespaces.cmis, _latestChangeLogToken)
         WriteElement(writer, attributeOverrides, "capabilities", Constants.Namespaces.cmis, _capabilities)
         WriteElement(writer, attributeOverrides, "aclCapability", Constants.Namespaces.cmis, _aclCapability)
         WriteElement(writer, attributeOverrides, "cmisVersionSupported", Constants.Namespaces.cmis, _cmisVersionSupported)
         If Not String.IsNullOrEmpty(_thinClientURI) Then WriteElement(writer, attributeOverrides, "thinClientURI", Constants.Namespaces.cmis, _thinClientURI)
         If _changesIncomplete.HasValue Then WriteElement(writer, attributeOverrides, "changesIncomplete", Constants.Namespaces.cmis, Convert(_changesIncomplete))
         WriteArray(writer, attributeOverrides, "changesOnType", Constants.Namespaces.cmis, _changesOnTypes)
         If Not String.IsNullOrEmpty(_principalAnonymous) Then WriteElement(writer, attributeOverrides, "principalAnonymous", Constants.Namespaces.cmis, _principalAnonymous)
         If Not String.IsNullOrEmpty(_principalAnyone) Then WriteElement(writer, attributeOverrides, "principalAnyone", Constants.Namespaces.cmis, _principalAnyone)
         WriteArray(writer, attributeOverrides, "extendedFeatures", Constants.Namespaces.cmis, _extendedFeatures)
      End Sub
#End Region

      Protected _aclCapability As Core.Security.cmisACLCapabilityType
      Public Overridable Property AclCapability As Core.Security.cmisACLCapabilityType
         Get
            Return _aclCapability
         End Get
         Set(value As Core.Security.cmisACLCapabilityType)
            If value IsNot _aclCapability Then
               Dim oldValue As Core.Security.cmisACLCapabilityType = _aclCapability
               _aclCapability = value
               OnPropertyChanged("AclCapability", value, oldValue)
            End If
         End Set
      End Property 'AclCapability

      Protected _capabilities As Core.cmisRepositoryCapabilitiesType
      Public Overridable Property Capabilities As Core.cmisRepositoryCapabilitiesType
         Get
            Return _capabilities
         End Get
         Set(value As Core.cmisRepositoryCapabilitiesType)
            If value IsNot _capabilities Then
               Dim oldValue As Core.cmisRepositoryCapabilitiesType = _capabilities
               _capabilities = value
               OnPropertyChanged("Capabilities", value, oldValue)
            End If
         End Set
      End Property 'Capabilities

      Protected _changesIncomplete As Boolean?
      Public Overridable Property ChangesIncomplete As Boolean?
         Get
            Return _changesIncomplete
         End Get
         Set(value As Boolean?)
            If Not _changesIncomplete.Equals(value) Then
               Dim oldValue As Boolean? = _changesIncomplete
               _changesIncomplete = value
               OnPropertyChanged("ChangesIncomplete", value, oldValue)
            End If
         End Set
      End Property 'ChangesIncomplete

      Protected _changesOnTypes As Core.enumBaseObjectTypeIds()
      Public Overridable Property ChangesOnTypes As Core.enumBaseObjectTypeIds()
         Get
            Return _changesOnTypes
         End Get
         Set(value As Core.enumBaseObjectTypeIds())
            If value IsNot _changesOnTypes Then
               Dim oldValue As Core.enumBaseObjectTypeIds() = _changesOnTypes
               _changesOnTypes = value
               OnPropertyChanged("ChangesOnTypes", value, oldValue)
            End If
         End Set
      End Property 'ChangesOnTypes

      Protected _cmisVersionSupported As String
      Public Overridable Property CmisVersionSupported As String
         Get
            Return _cmisVersionSupported
         End Get
         Set(value As String)
            If _cmisVersionSupported <> value Then
               Dim oldValue As String = _cmisVersionSupported
               _cmisVersionSupported = value
               OnPropertyChanged("CmisVersionSupported", value, oldValue)
            End If
         End Set
      End Property 'CmisVersionSupported

      Protected _extendedFeatures As Core.cmisExtensionFeatureType()
      Public Overridable Property ExtendedFeatures As Core.cmisExtensionFeatureType()
         Get
            Return _extendedFeatures
         End Get
         Set(value As Core.cmisExtensionFeatureType())
            If value IsNot _extendedFeatures Then
               Dim oldValue As Core.cmisExtensionFeatureType() = _extendedFeatures
               _extendedFeatures = value
               OnPropertyChanged("ExtendedFeatures", value, oldValue)
            End If
         End Set
      End Property 'ExtendedFeatures

      Protected _latestChangeLogToken As String
      Public Overridable Property LatestChangeLogToken As String
         Get
            Return _latestChangeLogToken
         End Get
         Set(value As String)
            If _latestChangeLogToken <> value Then
               Dim oldValue As String = _latestChangeLogToken
               _latestChangeLogToken = value
               OnPropertyChanged("LatestChangeLogToken", value, oldValue)
            End If
         End Set
      End Property 'LatestChangeLogToken

      Protected _principalAnonymous As String
      Public Overridable Property PrincipalAnonymous As String
         Get
            Return _principalAnonymous
         End Get
         Set(value As String)
            If _principalAnonymous <> value Then
               Dim oldValue As String = _principalAnonymous
               _principalAnonymous = value
               OnPropertyChanged("PrincipalAnonymous", value, oldValue)
            End If
         End Set
      End Property 'PrincipalAnonymous

      Protected _principalAnyone As String
      Public Overridable Property PrincipalAnyone As String
         Get
            Return _principalAnyone
         End Get
         Set(value As String)
            If _principalAnyone <> value Then
               Dim oldValue As String = _principalAnyone
               _principalAnyone = value
               OnPropertyChanged("PrincipalAnyone", value, oldValue)
            End If
         End Set
      End Property 'PrincipalAnyone

      Protected _productName As String
      Public Overridable Property ProductName As String
         Get
            Return _productName
         End Get
         Set(value As String)
            If _productName <> value Then
               Dim oldValue As String = _productName
               _productName = value
               OnPropertyChanged("ProductName", value, oldValue)
            End If
         End Set
      End Property 'ProductName

      Protected _productVersion As String
      Public Overridable Property ProductVersion As String
         Get
            Return _productVersion
         End Get
         Set(value As String)
            If _productVersion <> value Then
               Dim oldValue As String = _productVersion
               _productVersion = value
               OnPropertyChanged("ProductVersion", value, oldValue)
            End If
         End Set
      End Property 'ProductVersion

      Protected _repositoryDescription As String
      Public Overridable Property RepositoryDescription As String
         Get
            Return _repositoryDescription
         End Get
         Set(value As String)
            If _repositoryDescription <> value Then
               Dim oldValue As String = _repositoryDescription
               _repositoryDescription = value
               OnPropertyChanged("RepositoryDescription", value, oldValue)
            End If
         End Set
      End Property 'RepositoryDescription

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

      Protected _repositoryName As String
      Public Overridable Property RepositoryName As String
         Get
            Return _repositoryName
         End Get
         Set(value As String)
            If _repositoryName <> value Then
               Dim oldValue As String = _repositoryName
               _repositoryName = value
               OnPropertyChanged("RepositoryName", value, oldValue)
            End If
         End Set
      End Property 'RepositoryName

      Protected _rootFolderId As String
      Public Overridable Property RootFolderId As String
         Get
            Return _rootFolderId
         End Get
         Set(value As String)
            If _rootFolderId <> value Then
               Dim oldValue As String = _rootFolderId
               _rootFolderId = value
               OnPropertyChanged("RootFolderId", value, oldValue)
            End If
         End Set
      End Property 'RootFolderId

      Protected _thinClientURI As String
      Public Overridable Property ThinClientURI As String
         Get
            Return _thinClientURI
         End Get
         Set(value As String)
            If _thinClientURI <> value Then
               Dim oldValue As String = _thinClientURI
               _thinClientURI = value
               OnPropertyChanged("ThinClientURI", value, oldValue)
            End If
         End Set
      End Property 'ThinClientURI

      Protected _vendorName As String
      Public Overridable Property VendorName As String
         Get
            Return _vendorName
         End Get
         Set(value As String)
            If _vendorName <> value Then
               Dim oldValue As String = _vendorName
               _vendorName = value
               OnPropertyChanged("VendorName", value, oldValue)
            End If
         End Set
      End Property 'VendorName

   End Class
End Namespace