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

#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Extensions.Common
   ''' <summary>
   ''' Properties extension to return the newId in a bulkUpdateProperties-call
   ''' </summary>
   ''' <remarks></remarks>
   <sxs.XmlRoot("newId", Namespace:=Constants.Namespaces.com),
    Attributes.CmisTypeInfo("com:newId", Nothing, "newId")>
   Public Class NewIdExtension
      Inherits Extension

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _newId = Read(reader, attributeOverrides, "newId", Constants.Namespaces.com, _newId)
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If Not String.IsNullOrEmpty(_newId) Then WriteElement(writer, attributeOverrides, "newId", Constants.Namespaces.com, _newId)
      End Sub
#End Region

      Private _newId As String
      ''' <summary>
      ''' The RowState of this instance
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property NewId As String
         Get
            Return _newId
         End Get
         Set(value As String)
            If _newId <> value Then
               Dim oldValue As String = _newId
               _newId = value
               OnPropertyChanged("NewId", value, oldValue)
            End If
         End Set
      End Property 'NewId

   End Class
End Namespace