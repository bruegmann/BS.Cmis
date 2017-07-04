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
   ''' see cmisTypeDocumentDefinitionType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisTypeDocumentDefinitionType
      Inherits Core.Definitions.Types.cmisTypeDefinitionType

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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisTypeDocumentDefinitionType, String)) From {
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
         MyBase.ReadXmlCore(reader, attributeOverrides)
         _versionable = Read(reader, attributeOverrides, "versionable", Constants.Namespaces.cmis, _versionable)
         _contentStreamAllowed = ReadEnum(reader, attributeOverrides, "contentStreamAllowed", Constants.Namespaces.cmis, _contentStreamAllowed)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         MyBase.WriteXmlCore(writer, attributeOverrides)
         WriteElement(writer, attributeOverrides, "versionable", Constants.Namespaces.cmis, Convert(_versionable))
         WriteElement(writer, attributeOverrides, "contentStreamAllowed", Constants.Namespaces.cmis, _contentStreamAllowed.GetName())
      End Sub
#End Region

      Protected _contentStreamAllowed As Core.enumContentStreamAllowed
      Public Overridable Property ContentStreamAllowed As Core.enumContentStreamAllowed
         Get
            Return _contentStreamAllowed
         End Get
         Set(value As Core.enumContentStreamAllowed)
            If _contentStreamAllowed <> value Then
               Dim oldValue As Core.enumContentStreamAllowed = _contentStreamAllowed
               _contentStreamAllowed = value
               OnPropertyChanged("ContentStreamAllowed", value, oldValue)
            End If
         End Set
      End Property 'ContentStreamAllowed

      Protected _versionable As Boolean
      Public Overridable Property Versionable As Boolean
         Get
            Return _versionable
         End Get
         Set(value As Boolean)
            If _versionable <> value Then
               Dim oldValue As Boolean = _versionable
               _versionable = value
               OnPropertyChanged("Versionable", value, oldValue)
            End If
         End Set
      End Property 'Versionable

   End Class
End Namespace