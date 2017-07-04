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
   ''' see cmisNewTypeSettableAttributes
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisNewTypeSettableAttributes
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisNewTypeSettableAttributes, String)) From {
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
         _id = Read(reader, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         _localName = Read(reader, attributeOverrides, "localName", Constants.Namespaces.cmis, _localName)
         _localNamespace = Read(reader, attributeOverrides, "localNamespace", Constants.Namespaces.cmis, _localNamespace)
         _displayName = Read(reader, attributeOverrides, "displayName", Constants.Namespaces.cmis, _displayName)
         _queryName = Read(reader, attributeOverrides, "queryName", Constants.Namespaces.cmis, _queryName)
         _description = Read(reader, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         _creatable = Read(reader, attributeOverrides, "creatable", Constants.Namespaces.cmis, _creatable)
         _fileable = Read(reader, attributeOverrides, "fileable", Constants.Namespaces.cmis, _fileable)
         _queryable = Read(reader, attributeOverrides, "queryable", Constants.Namespaces.cmis, _queryable)
         _fulltextIndexed = Read(reader, attributeOverrides, "fulltextIndexed", Constants.Namespaces.cmis, _fulltextIndexed)
         _includedInSupertypeQuery = Read(reader, attributeOverrides, "includedInSupertypeQuery", Constants.Namespaces.cmis, _includedInSupertypeQuery)
         _controllablePolicy = Read(reader, attributeOverrides, "controllablePolicy", Constants.Namespaces.cmis, _controllablePolicy)
         _controllableACL = Read(reader, attributeOverrides, "controllableACL", Constants.Namespaces.cmis, _controllableACL)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "id", Constants.Namespaces.cmis, Convert(_id))
         WriteElement(writer, attributeOverrides, "localName", Constants.Namespaces.cmis, Convert(_localName))
         WriteElement(writer, attributeOverrides, "localNamespace", Constants.Namespaces.cmis, Convert(_localNamespace))
         WriteElement(writer, attributeOverrides, "displayName", Constants.Namespaces.cmis, Convert(_displayName))
         WriteElement(writer, attributeOverrides, "queryName", Constants.Namespaces.cmis, Convert(_queryName))
         WriteElement(writer, attributeOverrides, "description", Constants.Namespaces.cmis, Convert(_description))
         WriteElement(writer, attributeOverrides, "creatable", Constants.Namespaces.cmis, Convert(_creatable))
         WriteElement(writer, attributeOverrides, "fileable", Constants.Namespaces.cmis, Convert(_fileable))
         WriteElement(writer, attributeOverrides, "queryable", Constants.Namespaces.cmis, Convert(_queryable))
         WriteElement(writer, attributeOverrides, "fulltextIndexed", Constants.Namespaces.cmis, Convert(_fulltextIndexed))
         WriteElement(writer, attributeOverrides, "includedInSupertypeQuery", Constants.Namespaces.cmis, Convert(_includedInSupertypeQuery))
         WriteElement(writer, attributeOverrides, "controllablePolicy", Constants.Namespaces.cmis, Convert(_controllablePolicy))
         WriteElement(writer, attributeOverrides, "controllableACL", Constants.Namespaces.cmis, Convert(_controllableACL))
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

      Protected _description As Boolean
      Public Overridable Property Description As Boolean
         Get
            Return _description
         End Get
         Set(value As Boolean)
            If _description <> value Then
               Dim oldValue As Boolean = _description
               _description = value
               OnPropertyChanged("Description", value, oldValue)
            End If
         End Set
      End Property 'Description

      Protected _displayName As Boolean
      Public Overridable Property DisplayName As Boolean
         Get
            Return _displayName
         End Get
         Set(value As Boolean)
            If _displayName <> value Then
               Dim oldValue As Boolean = _displayName
               _displayName = value
               OnPropertyChanged("DisplayName", value, oldValue)
            End If
         End Set
      End Property 'DisplayName

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

      Protected _id As Boolean
      Public Overridable Property Id As Boolean
         Get
            Return _id
         End Get
         Set(value As Boolean)
            If _id <> value Then
               Dim oldValue As Boolean = _id
               _id = value
               OnPropertyChanged("Id", value, oldValue)
            End If
         End Set
      End Property 'Id

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

      Protected _localName As Boolean
      Public Overridable Property LocalName As Boolean
         Get
            Return _localName
         End Get
         Set(value As Boolean)
            If _localName <> value Then
               Dim oldValue As Boolean = _localName
               _localName = value
               OnPropertyChanged("LocalName", value, oldValue)
            End If
         End Set
      End Property 'LocalName

      Protected _localNamespace As Boolean
      Public Overridable Property LocalNamespace As Boolean
         Get
            Return _localNamespace
         End Get
         Set(value As Boolean)
            If _localNamespace <> value Then
               Dim oldValue As Boolean = _localNamespace
               _localNamespace = value
               OnPropertyChanged("LocalNamespace", value, oldValue)
            End If
         End Set
      End Property 'LocalNamespace

      Protected _queryable As Boolean
      Public Overridable Property Queryable As Boolean
         Get
            Return _queryable
         End Get
         Set(value As Boolean)
            If _queryable <> value Then
               Dim oldValue As Boolean = _queryable
               _queryable = value
               OnPropertyChanged("Queryable", value, oldValue)
            End If
         End Set
      End Property 'Queryable

      Protected _queryName As Boolean
      Public Overridable Property QueryName As Boolean
         Get
            Return _queryName
         End Get
         Set(value As Boolean)
            If _queryName <> value Then
               Dim oldValue As Boolean = _queryName
               _queryName = value
               OnPropertyChanged("QueryName", value, oldValue)
            End If
         End Set
      End Property 'QueryName

   End Class
End Namespace