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
   ''' see cmisBulkUpdateType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisBulkUpdateType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisBulkUpdateType, String)) From {
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
         _objectIdAndChangeTokens = ReadArray(Of Core.cmisObjectIdAndChangeTokenType)(reader, attributeOverrides, "objectIdAndChangeToken", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisObjectIdAndChangeTokenType))
         _properties = Read(Of Core.Collections.cmisPropertiesType)(reader, attributeOverrides, "properties", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Collections.cmisPropertiesType))
         _addSecondaryTypeIds = ReadArray(Of String)(reader, attributeOverrides, "addSecondaryTypeIds", Constants.Namespaces.cmis)
         _removeSecondaryTypeIds = ReadArray(Of String)(reader, attributeOverrides, "removeSecondaryTypeIds", Constants.Namespaces.cmis)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteArray(writer, attributeOverrides, "objectIdAndChangeToken", Constants.Namespaces.cmis, _objectIdAndChangeTokens)
         WriteElement(writer, attributeOverrides, "properties", Constants.Namespaces.cmis, _properties)
         WriteArray(writer, attributeOverrides, "addSecondaryTypeIds", Constants.Namespaces.cmis, _addSecondaryTypeIds)
         WriteArray(writer, attributeOverrides, "removeSecondaryTypeIds", Constants.Namespaces.cmis, _removeSecondaryTypeIds)
      End Sub
#End Region

      Protected _addSecondaryTypeIds As String()
      Public Overridable Property AddSecondaryTypeIds As String()
         Get
            Return _addSecondaryTypeIds
         End Get
         Set(value As String())
            If value IsNot _addSecondaryTypeIds Then
               Dim oldValue As String() = _addSecondaryTypeIds
               _addSecondaryTypeIds = value
               OnPropertyChanged("AddSecondaryTypeIds", value, oldValue)
            End If
         End Set
      End Property 'AddSecondaryTypeIds

      Protected _objectIdAndChangeTokens As Core.cmisObjectIdAndChangeTokenType()
      Public Overridable Property ObjectIdAndChangeTokens As Core.cmisObjectIdAndChangeTokenType()
         Get
            Return _objectIdAndChangeTokens
         End Get
         Set(value As Core.cmisObjectIdAndChangeTokenType())
            If value IsNot _objectIdAndChangeTokens Then
               Dim oldValue As Core.cmisObjectIdAndChangeTokenType() = _objectIdAndChangeTokens
               _objectIdAndChangeTokens = value
               OnPropertyChanged("ObjectIdAndChangeTokens", value, oldValue)
            End If
         End Set
      End Property 'ObjectIdAndChangeTokens

      Protected _properties As Core.Collections.cmisPropertiesType
      Public Overridable Property Properties As Core.Collections.cmisPropertiesType
         Get
            Return _properties
         End Get
         Set(value As Core.Collections.cmisPropertiesType)
            If value IsNot _properties Then
               Dim oldValue As Core.Collections.cmisPropertiesType = _properties
               _properties = value
               OnPropertyChanged("Properties", value, oldValue)
            End If
         End Set
      End Property 'Properties

      Protected _removeSecondaryTypeIds As String()
      Public Overridable Property RemoveSecondaryTypeIds As String()
         Get
            Return _removeSecondaryTypeIds
         End Get
         Set(value As String())
            If value IsNot _removeSecondaryTypeIds Then
               Dim oldValue As String() = _removeSecondaryTypeIds
               _removeSecondaryTypeIds = value
               OnPropertyChanged("RemoveSecondaryTypeIds", value, oldValue)
            End If
         End Set
      End Property 'RemoveSecondaryTypeIds

   End Class
End Namespace