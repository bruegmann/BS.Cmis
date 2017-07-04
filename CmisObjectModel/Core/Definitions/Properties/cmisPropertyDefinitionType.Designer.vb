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

Namespace CmisObjectModel.Core.Definitions.Properties
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisPropertyDefinitionType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public MustInherit Class cmisPropertyDefinitionType
      Inherits Core.Definitions.DefinitionBase

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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisPropertyDefinitionType, String)) From {
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
         'propertyType is readonly
         ReadEnum(reader, attributeOverrides, "propertyType", Constants.Namespaces.cmis, _propertyType)
         _cardinality = ReadEnum(reader, attributeOverrides, "cardinality", Constants.Namespaces.cmis, _cardinality)
         _updatability = ReadEnum(reader, attributeOverrides, "updatability", Constants.Namespaces.cmis, _updatability)
         _inherited = Read(reader, attributeOverrides, "inherited", Constants.Namespaces.cmis, _inherited)
         _required = Read(reader, attributeOverrides, "required", Constants.Namespaces.cmis, _required)
         _queryable = Read(reader, attributeOverrides, "queryable", Constants.Namespaces.cmis, _queryable)
         _orderable = Read(reader, attributeOverrides, "orderable", Constants.Namespaces.cmis, _orderable)
         _openChoice = Read(reader, attributeOverrides, "openChoice", Constants.Namespaces.cmis, _openChoice)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         WriteElement(writer, attributeOverrides, "localName", Constants.Namespaces.cmis, _localName)
         If Not String.IsNullOrEmpty(_localNamespace) Then WriteElement(writer, attributeOverrides, "localNamespace", Constants.Namespaces.cmis, _localNamespace)
         If Not String.IsNullOrEmpty(_displayName) Then WriteElement(writer, attributeOverrides, "displayName", Constants.Namespaces.cmis, _displayName)
         If Not String.IsNullOrEmpty(_queryName) Then WriteElement(writer, attributeOverrides, "queryName", Constants.Namespaces.cmis, _queryName)
         If Not String.IsNullOrEmpty(_description) Then WriteElement(writer, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         WriteElement(writer, attributeOverrides, "propertyType", Constants.Namespaces.cmis, _propertyType.GetName())
         WriteElement(writer, attributeOverrides, "cardinality", Constants.Namespaces.cmis, _cardinality.GetName())
         WriteElement(writer, attributeOverrides, "updatability", Constants.Namespaces.cmis, _updatability.GetName())
         If _inherited.HasValue Then WriteElement(writer, attributeOverrides, "inherited", Constants.Namespaces.cmis, Convert(_inherited))
         WriteElement(writer, attributeOverrides, "required", Constants.Namespaces.cmis, Convert(_required))
         WriteElement(writer, attributeOverrides, "queryable", Constants.Namespaces.cmis, Convert(_queryable))
         WriteElement(writer, attributeOverrides, "orderable", Constants.Namespaces.cmis, Convert(_orderable))
         If _openChoice.HasValue Then WriteElement(writer, attributeOverrides, "openChoice", Constants.Namespaces.cmis, Convert(_openChoice))
      End Sub
#End Region

      Protected _cardinality As Core.enumCardinality
      Public Overridable Property Cardinality As Core.enumCardinality
         Get
            Return _cardinality
         End Get
         Set(value As Core.enumCardinality)
            If _cardinality <> value Then
               Dim oldValue As Core.enumCardinality = _cardinality
               _cardinality = value
               OnPropertyChanged("Cardinality", value, oldValue)
            End If
         End Set
      End Property 'Cardinality

      Protected _inherited As Boolean?
      Public Overridable Property Inherited As Boolean?
         Get
            Return _inherited
         End Get
         Set(value As Boolean?)
            If Not _inherited.Equals(value) Then
               Dim oldValue As Boolean? = _inherited
               _inherited = value
               OnPropertyChanged("Inherited", value, oldValue)
            End If
         End Set
      End Property 'Inherited

      Protected _openChoice As Boolean?
      Public Overridable Property OpenChoice As Boolean?
         Get
            Return _openChoice
         End Get
         Set(value As Boolean?)
            If Not _openChoice.Equals(value) Then
               Dim oldValue As Boolean? = _openChoice
               _openChoice = value
               OnPropertyChanged("OpenChoice", value, oldValue)
            End If
         End Set
      End Property 'OpenChoice

      Protected _orderable As Boolean
      Public Overridable Property Orderable As Boolean
         Get
            Return _orderable
         End Get
         Set(value As Boolean)
            If _orderable <> value Then
               Dim oldValue As Boolean = _orderable
               _orderable = value
               OnPropertyChanged("Orderable", value, oldValue)
            End If
         End Set
      End Property 'Orderable

      Protected _required As Boolean
      Public Overridable Property Required As Boolean
         Get
            Return _required
         End Get
         Set(value As Boolean)
            If _required <> value Then
               Dim oldValue As Boolean = _required
               _required = value
               OnPropertyChanged("Required", value, oldValue)
            End If
         End Set
      End Property 'Required

      Protected _updatability As Core.enumUpdatability
      Public Overridable Property Updatability As Core.enumUpdatability
         Get
            Return _updatability
         End Get
         Set(value As Core.enumUpdatability)
            If _updatability <> value Then
               Dim oldValue As Core.enumUpdatability = _updatability
               _updatability = value
               OnPropertyChanged("Updatability", value, oldValue)
            End If
         End Set
      End Property 'Updatability

   End Class
End Namespace