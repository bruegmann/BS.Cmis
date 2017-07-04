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
   ''' see cmisQueryType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisQueryType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisQueryType, String)) From {
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
         _statement = Read(reader, attributeOverrides, "statement", Constants.Namespaces.cmis, _statement)
         _searchAllVersions = Read(reader, attributeOverrides, "searchAllVersions", Constants.Namespaces.cmis, _searchAllVersions)
         _includeAllowableActions = Read(reader, attributeOverrides, "includeAllowableActions", Constants.Namespaces.cmis, _includeAllowableActions)
         _includeRelationships = ReadOptionalEnum(reader, attributeOverrides, "includeRelationships", Constants.Namespaces.cmis, _includeRelationships)
         _renditionFilter = Read(reader, attributeOverrides, "renditionFilter", Constants.Namespaces.cmis, _renditionFilter)
         _maxItems = Read(reader, attributeOverrides, "maxItems", Constants.Namespaces.cmis, _maxItems)
         _skipCount = Read(reader, attributeOverrides, "skipCount", Constants.Namespaces.cmis, _skipCount)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "statement", Constants.Namespaces.cmis, _statement)
         If _searchAllVersions.HasValue Then WriteElement(writer, attributeOverrides, "searchAllVersions", Constants.Namespaces.cmis, Convert(_searchAllVersions))
         If _includeAllowableActions.HasValue Then WriteElement(writer, attributeOverrides, "includeAllowableActions", Constants.Namespaces.cmis, Convert(_includeAllowableActions))
         If _includeRelationships.HasValue Then WriteElement(writer, attributeOverrides, "includeRelationships", Constants.Namespaces.cmis, _includeRelationships.Value.GetName())
         If Not String.IsNullOrEmpty(_renditionFilter) Then WriteElement(writer, attributeOverrides, "renditionFilter", Constants.Namespaces.cmis, _renditionFilter)
         If _maxItems.HasValue Then WriteElement(writer, attributeOverrides, "maxItems", Constants.Namespaces.cmis, Convert(_maxItems))
         If _skipCount.HasValue Then WriteElement(writer, attributeOverrides, "skipCount", Constants.Namespaces.cmis, Convert(_skipCount))
      End Sub
#End Region

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

      Protected _maxItems As xs_Integer?
      Public Overridable Property MaxItems As xs_Integer?
         Get
            Return _maxItems
         End Get
         Set(value As xs_Integer?)
            If Not _maxItems.Equals(value) Then
               Dim oldValue As xs_Integer? = _maxItems
               _maxItems = value
               OnPropertyChanged("MaxItems", value, oldValue)
            End If
         End Set
      End Property 'MaxItems

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

      Protected _searchAllVersions As Boolean?
      Public Overridable Property SearchAllVersions As Boolean?
         Get
            Return _searchAllVersions
         End Get
         Set(value As Boolean?)
            If Not _searchAllVersions.Equals(value) Then
               Dim oldValue As Boolean? = _searchAllVersions
               _searchAllVersions = value
               OnPropertyChanged("SearchAllVersions", value, oldValue)
            End If
         End Set
      End Property 'SearchAllVersions

      Protected _skipCount As xs_Integer?
      Public Overridable Property SkipCount As xs_Integer?
         Get
            Return _skipCount
         End Get
         Set(value As xs_Integer?)
            If Not _skipCount.Equals(value) Then
               Dim oldValue As xs_Integer? = _skipCount
               _skipCount = value
               OnPropertyChanged("SkipCount", value, oldValue)
            End If
         End Set
      End Property 'SkipCount

      Protected _statement As String
      Public Overridable Property Statement As String
         Get
            Return _statement
         End Get
         Set(value As String)
            If _statement <> value Then
               Dim oldValue As String = _statement
               _statement = value
               OnPropertyChanged("Statement", value, oldValue)
            End If
         End Set
      End Property 'Statement

   End Class
End Namespace