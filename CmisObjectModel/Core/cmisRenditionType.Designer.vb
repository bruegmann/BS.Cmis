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
   ''' see cmisRenditionType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisRenditionType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisRenditionType, String)) From {
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
         _streamId = Read(reader, attributeOverrides, "streamId", Constants.Namespaces.cmis, _streamId)
         _mimetype = Read(reader, attributeOverrides, "mimetype", Constants.Namespaces.cmis, _mimetype)
         _length = Read(reader, attributeOverrides, "length", Constants.Namespaces.cmis, _length)
         _kind = Read(reader, attributeOverrides, "kind", Constants.Namespaces.cmis, _kind)
         _title = Read(reader, attributeOverrides, "title", Constants.Namespaces.cmis, _title)
         _height = Read(reader, attributeOverrides, "height", Constants.Namespaces.cmis, _height)
         _width = Read(reader, attributeOverrides, "width", Constants.Namespaces.cmis, _width)
         _renditionDocumentId = Read(reader, attributeOverrides, "renditionDocumentId", Constants.Namespaces.cmis, _renditionDocumentId)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "streamId", Constants.Namespaces.cmis, _streamId)
         WriteElement(writer, attributeOverrides, "mimetype", Constants.Namespaces.cmis, _mimetype)
         WriteElement(writer, attributeOverrides, "length", Constants.Namespaces.cmis, Convert(_length))
         WriteElement(writer, attributeOverrides, "kind", Constants.Namespaces.cmis, _kind)
         If Not String.IsNullOrEmpty(_title) Then WriteElement(writer, attributeOverrides, "title", Constants.Namespaces.cmis, _title)
         If _height.HasValue Then WriteElement(writer, attributeOverrides, "height", Constants.Namespaces.cmis, Convert(_height))
         If _width.HasValue Then WriteElement(writer, attributeOverrides, "width", Constants.Namespaces.cmis, Convert(_width))
         If Not String.IsNullOrEmpty(_renditionDocumentId) Then WriteElement(writer, attributeOverrides, "renditionDocumentId", Constants.Namespaces.cmis, _renditionDocumentId)
      End Sub
#End Region

      Protected _height As xs_Integer?
      Public Overridable Property Height As xs_Integer?
         Get
            Return _height
         End Get
         Set(value As xs_Integer?)
            If Not _height.Equals(value) Then
               Dim oldValue As xs_Integer? = _height
               _height = value
               OnPropertyChanged("Height", value, oldValue)
            End If
         End Set
      End Property 'Height

      Protected _kind As String
      Public Overridable Property Kind As String
         Get
            Return _kind
         End Get
         Set(value As String)
            If _kind <> value Then
               Dim oldValue As String = _kind
               _kind = value
               OnPropertyChanged("Kind", value, oldValue)
            End If
         End Set
      End Property 'Kind

      Protected _length As xs_Integer
      Public Overridable Property Length As xs_Integer
         Get
            Return _length
         End Get
         Set(value As xs_Integer)
            If _length <> value Then
               Dim oldValue As xs_Integer = _length
               _length = value
               OnPropertyChanged("Length", value, oldValue)
            End If
         End Set
      End Property 'Length

      Protected _mimetype As String
      Public Overridable Property Mimetype As String
         Get
            Return _mimetype
         End Get
         Set(value As String)
            If _mimetype <> value Then
               Dim oldValue As String = _mimetype
               _mimetype = value
               OnPropertyChanged("Mimetype", value, oldValue)
            End If
         End Set
      End Property 'Mimetype

      Protected _renditionDocumentId As String
      Public Overridable Property RenditionDocumentId As String
         Get
            Return _renditionDocumentId
         End Get
         Set(value As String)
            If _renditionDocumentId <> value Then
               Dim oldValue As String = _renditionDocumentId
               _renditionDocumentId = value
               OnPropertyChanged("RenditionDocumentId", value, oldValue)
            End If
         End Set
      End Property 'RenditionDocumentId

      Protected _streamId As String
      Public Overridable Property StreamId As String
         Get
            Return _streamId
         End Get
         Set(value As String)
            If _streamId <> value Then
               Dim oldValue As String = _streamId
               _streamId = value
               OnPropertyChanged("StreamId", value, oldValue)
            End If
         End Set
      End Property 'StreamId

      Protected _title As String
      Public Overridable Property Title As String
         Get
            Return _title
         End Get
         Set(value As String)
            If _title <> value Then
               Dim oldValue As String = _title
               _title = value
               OnPropertyChanged("Title", value, oldValue)
            End If
         End Set
      End Property 'Title

      Protected _width As xs_Integer?
      Public Overridable Property Width As xs_Integer?
         Get
            Return _width
         End Get
         Set(value As xs_Integer?)
            If Not _width.Equals(value) Then
               Dim oldValue As xs_Integer? = _width
               _width = value
               OnPropertyChanged("Width", value, oldValue)
            End If
         End Set
      End Property 'Width

   End Class
End Namespace