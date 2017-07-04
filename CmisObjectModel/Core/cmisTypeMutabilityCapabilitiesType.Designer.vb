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
   ''' see cmisTypeMutabilityCapabilitiesType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public Class cmisTypeMutabilityCapabilitiesType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisTypeMutabilityCapabilitiesType, String)) From {
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
         _create = Read(reader, attributeOverrides, "create", Constants.Namespaces.cmis, _create)
         _update = Read(reader, attributeOverrides, "update", Constants.Namespaces.cmis, _update)
         _delete = Read(reader, attributeOverrides, "delete", Constants.Namespaces.cmis, _delete)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "create", Constants.Namespaces.cmis, Convert(_create))
         WriteElement(writer, attributeOverrides, "update", Constants.Namespaces.cmis, Convert(_update))
         WriteElement(writer, attributeOverrides, "delete", Constants.Namespaces.cmis, Convert(_delete))
      End Sub
#End Region

      Protected _create As Boolean
      Public Overridable Property Create As Boolean
         Get
            Return _create
         End Get
         Set(value As Boolean)
            If _create <> value Then
               Dim oldValue As Boolean = _create
               _create = value
               OnPropertyChanged("Create", value, oldValue)
            End If
         End Set
      End Property 'Create

      Protected _delete As Boolean
      Public Overridable Property Delete As Boolean
         Get
            Return _delete
         End Get
         Set(value As Boolean)
            If _delete <> value Then
               Dim oldValue As Boolean = _delete
               _delete = value
               OnPropertyChanged("Delete", value, oldValue)
            End If
         End Set
      End Property 'Delete

      Protected _update As Boolean
      Public Overridable Property Update As Boolean
         Get
            Return _update
         End Get
         Set(value As Boolean)
            If _update <> value Then
               Dim oldValue As Boolean = _update
               _update = value
               OnPropertyChanged("Update", value, oldValue)
            End If
         End Set
      End Property 'Update

   End Class
End Namespace