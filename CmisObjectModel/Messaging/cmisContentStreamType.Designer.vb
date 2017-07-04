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

Namespace CmisObjectModel.Messaging
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisContentStreamType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Messaging.xsd
   ''' </remarks>
   Public Class cmisContentStreamType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisContentStreamType, String)) From {
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
         _length = Read(reader, attributeOverrides, "length", Constants.Namespaces.cmism, _length)
         _mimeType = Read(reader, attributeOverrides, "mimeType", Constants.Namespaces.cmism, _mimeType)
         _filename = Read(reader, attributeOverrides, "filename", Constants.Namespaces.cmism, _filename)
         _stream = Read(reader, attributeOverrides, "stream", Constants.Namespaces.cmism, _stream)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If _length.HasValue Then WriteElement(writer, attributeOverrides, "length", Constants.Namespaces.cmism, Convert(_length))
         If Not String.IsNullOrEmpty(_mimeType) Then WriteElement(writer, attributeOverrides, "mimeType", Constants.Namespaces.cmism, _mimeType)
         If Not String.IsNullOrEmpty(_filename) Then WriteElement(writer, attributeOverrides, "filename", Constants.Namespaces.cmism, _filename)
         WriteElement(writer, attributeOverrides, "stream", Constants.Namespaces.cmism, _stream)
      End Sub
#End Region

      Protected _filename As String
      Public Overridable Property Filename As String
         Get
            Return _filename
         End Get
         Set(value As String)
            If _filename <> value Then
               Dim oldValue As String = _filename
               _filename = value
               OnPropertyChanged("Filename", value, oldValue)
            End If
         End Set
      End Property 'Filename

      Protected _length As xs_Integer?
      Public Overridable Property Length As xs_Integer?
         Get
            Return _length
         End Get
         Set(value As xs_Integer?)
            If Not _length.Equals(value) Then
               Dim oldValue As xs_Integer? = _length
               _length = value
               OnPropertyChanged("Length", value, oldValue)
            End If
         End Set
      End Property 'Length

      Protected _mimeType As String
      Public Overridable Property MimeType As String
         Get
            Return _mimeType
         End Get
         Set(value As String)
            If _mimeType <> value Then
               Dim oldValue As String = _mimeType
               _mimeType = value
               OnPropertyChanged("MimeType", value, oldValue)
            End If
         End Set
      End Property 'MimeType

      Protected _stream As String
      Public Overridable Property Stream As String
         Get
            Return _stream
         End Get
         Set(value As String)
            If _stream <> value Then
               Dim oldValue As String = _stream
               _stream = value
               OnPropertyChanged("Stream", value, oldValue)
            End If
         End Set
      End Property 'Stream

   End Class
End Namespace