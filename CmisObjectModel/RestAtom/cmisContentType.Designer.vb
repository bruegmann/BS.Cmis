'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2017, Brügmann Software GmbH, Papenburg, All rights reserved
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
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.RestAtom
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisContentType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-RestAtom.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public Class cmisContentType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisContentType, String)) From {
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
         Dim lastNamespaceURI As String = Nothing
         Dim lastLocalName As String = Nothing

         reader.MoveToContent()
         While reader.IsStartElement()
            Dim localName As String = reader.LocalName
            Dim namespaceURI As String = reader.NamespaceURI

            If String.Equals(lastLocalName, localName, StringComparison.InvariantCultureIgnoreCase) AndAlso
               String.Equals(lastNamespaceURI, namespaceURI, StringComparison.InvariantCultureIgnoreCase) Then
               'unknown node detected
               reader.ReadOuterXml()
            Else
               lastLocalName = localName
               lastNamespaceURI = namespaceURI
               ReadXmlCoreFuzzy(reader, attributeOverrides, True)
            End If
            reader.MoveToContent()
         End While
      End Sub
      Protected Overrides Function ReadXmlCoreFuzzy(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides, callReadXmlCoreFuzzy2 As Boolean) As Boolean
         If MyBase.ReadXmlCoreFuzzy(reader, attributeOverrides, False) Then Return True

         Select Case If(reader.NamespaceURI, String.Empty).ToLowerInvariant()
            Case Constants.NamespacesLowerInvariant.cmisra
               Select Case reader.LocalName.ToLowerInvariant()
                  Case "mediatype"
                     _mediatype = Read(reader, attributeOverrides, "mediatype", Constants.Namespaces.cmisra, _mediatype)
                  Case "base64"
                     _base64 = Read(reader, attributeOverrides, "base64", Constants.Namespaces.cmisra, _base64)
                  Case Else
                     'try to find node in the namespace-independent section
                     Return callReadXmlCoreFuzzy2 AndAlso ReadXmlCoreFuzzy2(reader, attributeOverrides, True)
               End Select
            Case Else
               'try to find node in the namespace-independent section
               Return callReadXmlCoreFuzzy2 AndAlso ReadXmlCoreFuzzy2(reader, attributeOverrides, True)
         End Select

         Return True
      End Function
      Protected Overrides Function ReadXmlCoreFuzzy2(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides, callReadOuterXml As Boolean) As Boolean
         If MyBase.ReadXmlCoreFuzzy2(reader, attributeOverrides, False) Then Return True

         'ignore node
         If callReadOuterXml Then reader.ReadOuterXml()
         Return callReadOuterXml
      End Function

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "mediatype", Constants.Namespaces.cmisra, _mediatype)
         WriteElement(writer, attributeOverrides, "base64", Constants.Namespaces.cmisra, _base64)
      End Sub
#End Region

      Protected _base64 As String
      Public Overridable Property Base64 As String
         Get
            Return _base64
         End Get
         Set(value As String)
            If _base64 <> value Then
               Dim oldValue As String = _base64
               _base64 = value
               OnPropertyChanged("Base64", value, oldValue)
            End If
         End Set
      End Property 'Base64

      Protected _mediatype As String
      Public Overridable Property Mediatype As String
         Get
            Return _mediatype
         End Get
         Set(value As String)
            If _mediatype <> value Then
               Dim oldValue As String = _mediatype
               _mediatype = value
               OnPropertyChanged("Mediatype", value, oldValue)
            End If
         End Set
      End Property 'Mediatype

   End Class
End Namespace