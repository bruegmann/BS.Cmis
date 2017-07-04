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

Namespace CmisObjectModel.Extensions.Alfresco
   ''' <summary>
   ''' Support for Alfresco MandatoryAspects extensions (in type-definitions)
   ''' </summary>
   ''' <remarks></remarks>
   <sxs.XmlRoot("mandatoryAspects", Namespace:=Constants.Namespaces.alf),
    Attributes.CmisTypeInfo("alf:mandatoryAspects", Nothing, "mandatoryAspects")>
   Public Class MandatoryAspects
      Inherits Extension

      Public Sub New()
      End Sub

      Public Sub New(ParamArray aspects As String())
         _aspects = aspects
      End Sub

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _aspects = ReadArray(Of String)(reader, attributeOverrides, "mandatoryAspect", Namespaces.alf)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteArray(writer, attributeOverrides, "mandatoryAspect", Namespaces.alf, _aspects)
      End Sub
#End Region

      Private _aspects As String()
      Public Overridable Property Aspects As String()
         Get
            Return _aspects
         End Get
         Set(value As String())
            If _aspects IsNot value Then
               Dim oldValue As String() = _aspects
               _aspects = value
               OnPropertyChanged("Aspects", value, oldValue)
            End If
         End Set
      End Property 'Aspects

   End Class
End Namespace