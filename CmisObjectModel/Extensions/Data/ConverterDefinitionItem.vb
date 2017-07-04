'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
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
Imports CmisObjectModel.Constants
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Extensions.Data
   <sxs.XmlRoot("converterDefinitionItem", Namespace:=Constants.Namespaces.com),
    Attributes.CmisTypeInfo("com:converterDefinitionItem", Nothing, "converterDefinitionItem")>
   Public Class ConverterDefinitionItem
      Inherits Serialization.XmlSerializable

      Public Sub New()
      End Sub
      Public Sub New(key As String, value As String)
         _key = key
         _value = value
      End Sub

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _key = Read(reader, attributeOverrides, "key", Nothing)
         _value = Read(reader, attributeOverrides, "value", Nothing)
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If Not String.IsNullOrEmpty(_key) Then WriteElement(writer, attributeOverrides, "key", Constants.Namespaces.com, _key)
         If Not String.IsNullOrEmpty(_value) Then WriteElement(writer, attributeOverrides, "value", Constants.Namespaces.com, _value)
      End Sub
#End Region

      Private _key As String
      Public Property Key As String
         Get
            Return _key
         End Get
         Set(value As String)
            If _key <> value Then
               Dim oldValue As String = _key
               _key = value
               OnPropertyChanged("Key", value, oldValue)
            End If
         End Set
      End Property 'Key

      Private _value As String
      Public Property Value As String
         Get
            Return _value
         End Get
         Set(value As String)
            If _value <> value Then
               Dim oldValue As String = _value
               _value = value
               OnPropertyChanged("Value", value, oldValue)
            End If
         End Set
      End Property 'Value

   End Class
End Namespace