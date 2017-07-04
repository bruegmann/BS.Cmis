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

Namespace CmisObjectModel.Core.Definitions.Types
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisTypeDefinitionType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public MustInherit Class cmisTypeDefinitionType
      Inherits Core.Definitions.DefinitionBase

      Protected Sub New()
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisTypeDefinitionType, String)) From {
         } '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _id = Read(reader, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         _localName = Read(reader, attributeOverrides, "localName", Constants.Namespaces.cmis, _localName)
         _localNamespace = Read(reader, attributeOverrides, "localNamespace", Constants.Namespaces.cmis, _localNamespace)
         _displayName = Read(reader, attributeOverrides, "displayName", Constants.Namespaces.cmis, _displayName)
         _queryName = Read(reader, attributeOverrides, "queryName", Constants.Namespaces.cmis, _queryName)
         _description = Read(reader, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         'baseId is readonly
         ReadEnum(reader, attributeOverrides, "baseId", Constants.Namespaces.cmis, _baseId)
         _parentId = Read(reader, attributeOverrides, "parentId", Constants.Namespaces.cmis, _parentId)
         _creatable = Read(reader, attributeOverrides, "creatable", Constants.Namespaces.cmis, _creatable)
         _fileable = Read(reader, attributeOverrides, "fileable", Constants.Namespaces.cmis, _fileable)
         _queryable = Read(reader, attributeOverrides, "queryable", Constants.Namespaces.cmis, _queryable)
         _fulltextIndexed = Read(reader, attributeOverrides, "fulltextIndexed", Constants.Namespaces.cmis, _fulltextIndexed)
         _includedInSupertypeQuery = Read(reader, attributeOverrides, "includedInSupertypeQuery", Constants.Namespaces.cmis, _includedInSupertypeQuery)
         _controllablePolicy = Read(reader, attributeOverrides, "controllablePolicy", Constants.Namespaces.cmis, _controllablePolicy)
         _controllableACL = Read(reader, attributeOverrides, "controllableACL", Constants.Namespaces.cmis, _controllableACL)
         _typeMutability = Read(Of Core.cmisTypeMutabilityCapabilitiesType)(reader, attributeOverrides, "typeMutability", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisTypeMutabilityCapabilitiesType))
         _propertyDefinitions = ReadArray(Of Core.Definitions.Properties.cmisPropertyDefinitionType)(reader, attributeOverrides, Nothing, AddressOf Core.Definitions.Properties.cmisPropertyDefinitionType.CreateInstance)
         _extensions = ReadArray(Of CmisObjectModel.Extensions.Extension)(reader, attributeOverrides, Nothing, AddressOf CmisObjectModel.Extensions.Extension.CreateInstance)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         WriteElement(writer, attributeOverrides, "localName", Constants.Namespaces.cmis, _localName)
         WriteElement(writer, attributeOverrides, "localNamespace", Constants.Namespaces.cmis, _localNamespace)
         If Not String.IsNullOrEmpty(_displayName) Then WriteElement(writer, attributeOverrides, "displayName", Constants.Namespaces.cmis, _displayName)
         If Not String.IsNullOrEmpty(_queryName) Then WriteElement(writer, attributeOverrides, "queryName", Constants.Namespaces.cmis, _queryName)
         If Not String.IsNullOrEmpty(_description) Then WriteElement(writer, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         WriteElement(writer, attributeOverrides, "baseId", Constants.Namespaces.cmis, _baseId.GetName())
         If Not String.IsNullOrEmpty(_parentId) Then WriteElement(writer, attributeOverrides, "parentId", Constants.Namespaces.cmis, _parentId)
         WriteElement(writer, attributeOverrides, "creatable", Constants.Namespaces.cmis, Convert(_creatable))
         WriteElement(writer, attributeOverrides, "fileable", Constants.Namespaces.cmis, Convert(_fileable))
         WriteElement(writer, attributeOverrides, "queryable", Constants.Namespaces.cmis, Convert(_queryable))
         WriteElement(writer, attributeOverrides, "fulltextIndexed", Constants.Namespaces.cmis, Convert(_fulltextIndexed))
         WriteElement(writer, attributeOverrides, "includedInSupertypeQuery", Constants.Namespaces.cmis, Convert(_includedInSupertypeQuery))
         WriteElement(writer, attributeOverrides, "controllablePolicy", Constants.Namespaces.cmis, Convert(_controllablePolicy))
         WriteElement(writer, attributeOverrides, "controllableACL", Constants.Namespaces.cmis, Convert(_controllableACL))
         WriteElement(writer, attributeOverrides, "typeMutability", Constants.Namespaces.cmis, _typeMutability)
         WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmis, _propertyDefinitions)
         WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmis, _extensions)
      End Sub
#End Region

      Protected _controllableACL As Boolean
      Public Overridable Property ControllableACL As Boolean
         Get
            Return _controllableACL
         End Get
         Set(value As Boolean)
            If _controllableACL <> value Then
               Dim oldValue As Boolean = _controllableACL
               _controllableACL = value
               OnPropertyChanged("ControllableACL", value, oldValue)
            End If
         End Set
      End Property 'ControllableACL

      Protected _controllablePolicy As Boolean
      Public Overridable Property ControllablePolicy As Boolean
         Get
            Return _controllablePolicy
         End Get
         Set(value As Boolean)
            If _controllablePolicy <> value Then
               Dim oldValue As Boolean = _controllablePolicy
               _controllablePolicy = value
               OnPropertyChanged("ControllablePolicy", value, oldValue)
            End If
         End Set
      End Property 'ControllablePolicy

      Protected _creatable As Boolean
      Public Overridable Property Creatable As Boolean
         Get
            Return _creatable
         End Get
         Set(value As Boolean)
            If _creatable <> value Then
               Dim oldValue As Boolean = _creatable
               _creatable = value
               OnPropertyChanged("Creatable", value, oldValue)
            End If
         End Set
      End Property 'Creatable

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

      Protected _fileable As Boolean
      Public Overridable Property Fileable As Boolean
         Get
            Return _fileable
         End Get
         Set(value As Boolean)
            If _fileable <> value Then
               Dim oldValue As Boolean = _fileable
               _fileable = value
               OnPropertyChanged("Fileable", value, oldValue)
            End If
         End Set
      End Property 'Fileable

      Protected _fulltextIndexed As Boolean
      Public Overridable Property FulltextIndexed As Boolean
         Get
            Return _fulltextIndexed
         End Get
         Set(value As Boolean)
            If _fulltextIndexed <> value Then
               Dim oldValue As Boolean = _fulltextIndexed
               _fulltextIndexed = value
               OnPropertyChanged("FulltextIndexed", value, oldValue)
            End If
         End Set
      End Property 'FulltextIndexed

      Protected _includedInSupertypeQuery As Boolean
      Public Overridable Property IncludedInSupertypeQuery As Boolean
         Get
            Return _includedInSupertypeQuery
         End Get
         Set(value As Boolean)
            If _includedInSupertypeQuery <> value Then
               Dim oldValue As Boolean = _includedInSupertypeQuery
               _includedInSupertypeQuery = value
               OnPropertyChanged("IncludedInSupertypeQuery", value, oldValue)
            End If
         End Set
      End Property 'IncludedInSupertypeQuery

      Protected _parentId As String
      Public Overridable Property ParentId As String
         Get
            Return _parentId
         End Get
         Set(value As String)
            If _parentId <> value Then
               Dim oldValue As String = _parentId
               _parentId = value
               OnPropertyChanged("ParentId", value, oldValue)
            End If
         End Set
      End Property 'ParentId

      Protected _propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType()
      Public Overridable Property PropertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType()
         Get
            Return _propertyDefinitions
         End Get
         Set(value As Core.Definitions.Properties.cmisPropertyDefinitionType())
            If value IsNot _propertyDefinitions Then
               Dim oldValue As Core.Definitions.Properties.cmisPropertyDefinitionType() = _propertyDefinitions
               _propertyDefinitions = value
               OnPropertyChanged("PropertyDefinitions", value, oldValue)
            End If
         End Set
      End Property 'PropertyDefinitions

      Protected _typeMutability As Core.cmisTypeMutabilityCapabilitiesType
      Public Overridable Property TypeMutability As Core.cmisTypeMutabilityCapabilitiesType
         Get
            Return _typeMutability
         End Get
         Set(value As Core.cmisTypeMutabilityCapabilitiesType)
            If value IsNot _typeMutability Then
               Dim oldValue As Core.cmisTypeMutabilityCapabilitiesType = _typeMutability
               _typeMutability = value
               OnPropertyChanged("TypeMutability", value, oldValue)
            End If
         End Set
      End Property 'TypeMutability

   End Class
End Namespace