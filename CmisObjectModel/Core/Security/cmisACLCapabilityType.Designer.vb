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

Namespace CmisObjectModel.Core.Security
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisACLCapabilityType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisACLCapabilityType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisACLCapabilityType, String)) From {
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
         _supportedPermissions = ReadEnum(reader, attributeOverrides, "supportedPermissions", Constants.Namespaces.cmis, _supportedPermissions)
         _propagation = ReadEnum(reader, attributeOverrides, "propagation", Constants.Namespaces.cmis, _propagation)
         _permissions = ReadArray(Of Core.Security.cmisPermissionDefinition)(reader, attributeOverrides, "permissions", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Security.cmisPermissionDefinition))
         _mappings = ReadArray(Of Core.Security.cmisPermissionMapping)(reader, attributeOverrides, "mapping", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Security.cmisPermissionMapping))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "supportedPermissions", Constants.Namespaces.cmis, _supportedPermissions.GetName())
         WriteElement(writer, attributeOverrides, "propagation", Constants.Namespaces.cmis, _propagation.GetName())
         WriteArray(writer, attributeOverrides, "permissions", Constants.Namespaces.cmis, _permissions)
         WriteArray(writer, attributeOverrides, "mapping", Constants.Namespaces.cmis, _mappings)
      End Sub
#End Region

      Protected _mappings As Core.Security.cmisPermissionMapping()
      Public Overridable Property Mappings As Core.Security.cmisPermissionMapping()
         Get
            Return _mappings
         End Get
         Set(value As Core.Security.cmisPermissionMapping())
            If value IsNot _mappings Then
               Dim oldValue As Core.Security.cmisPermissionMapping() = _mappings
               _mappings = value
               OnPropertyChanged("Mappings", value, oldValue)
            End If
         End Set
      End Property 'Mappings

      Protected _permissions As Core.Security.cmisPermissionDefinition()
      Public Overridable Property Permissions As Core.Security.cmisPermissionDefinition()
         Get
            Return _permissions
         End Get
         Set(value As Core.Security.cmisPermissionDefinition())
            If value IsNot _permissions Then
               Dim oldValue As Core.Security.cmisPermissionDefinition() = _permissions
               _permissions = value
               OnPropertyChanged("Permissions", value, oldValue)
            End If
         End Set
      End Property 'Permissions

      Protected _propagation As Core.enumACLPropagation
      Public Overridable Property Propagation As Core.enumACLPropagation
         Get
            Return _propagation
         End Get
         Set(value As Core.enumACLPropagation)
            If _propagation <> value Then
               Dim oldValue As Core.enumACLPropagation = _propagation
               _propagation = value
               OnPropertyChanged("Propagation", value, oldValue)
            End If
         End Set
      End Property 'Propagation

      Protected _supportedPermissions As Core.enumSupportedPermissions
      Public Overridable Property SupportedPermissions As Core.enumSupportedPermissions
         Get
            Return _supportedPermissions
         End Get
         Set(value As Core.enumSupportedPermissions)
            If _supportedPermissions <> value Then
               Dim oldValue As Core.enumSupportedPermissions = _supportedPermissions
               _supportedPermissions = value
               OnPropertyChanged("SupportedPermissions", value, oldValue)
            End If
         End Set
      End Property 'SupportedPermissions

   End Class
End Namespace