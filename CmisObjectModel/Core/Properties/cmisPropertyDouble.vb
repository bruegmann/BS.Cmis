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
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Properties
   ''' <summary>
   ''' Implementation of properties containing double-values
   ''' </summary>
   ''' <remarks>In CMIS specification there is no cmisPropertyDouble defined, but some CMIS servers
   ''' (i.e. alfresco) send double values instead of decimal values</remarks>
   <sxs.XmlRoot(cmisPropertyDecimal.DefaultElementName, Namespace:=Constants.Namespaces.cmis),
    Attributes.CmisTypeInfo(cmisPropertyDouble.CmisTypeName, cmisPropertyDouble.TargetTypeName, cmisPropertyDouble.DefaultElementName)>
   Public Class cmisPropertyDouble
      Inherits Core.Properties.Generic.cmisProperty(Of Double)

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
      Public Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String, ParamArray values As Double())
         MyBase.New(propertyDefinitionId, localName, displayName, queryName, values)
      End Sub

#Region "Constants"
      Public Const CmisTypeName As String = "cmis:cmisPropertyDouble"
      Public Const TargetTypeName As String = "double"
      Public Const DefaultElementName As String = "propertyDouble"
#End Region

#Region "IComparable"
      Protected Overrides Function CompareTo(ParamArray other As Double()) As Integer
         Dim length As Integer = If(_values Is Nothing, 0, _values.Length)
         Dim otherLength As Integer = If(other Is Nothing, 0, other.Length)
         If otherLength = 0 Then
            Return If(length = 0, 0, 1)
         ElseIf length = 0 Then
            Return -1
         Else
            For index As Integer = 0 To Math.Min(length, otherLength) - 1
               Dim first = _values(index)
               Dim second = other(index)
               If first < second Then
                  Return -1
               ElseIf first > second Then
                  Return 1
               End If
            Next
            Return If(length = otherLength, 0, If(length > otherLength, 1, -1))
         End If
      End Function
#End Region

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisPropertyDecimal, String)) From {
         } '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         MyBase.ReadXmlCore(reader, attributeOverrides)
         _values = ReadArray(Of Double)(reader, attributeOverrides, "value", Constants.Namespaces.cmis)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         MyBase.WriteXmlCore(writer, attributeOverrides)
         WriteArray(writer, attributeOverrides, "value", Constants.Namespaces.cmis, _values)
      End Sub
#End Region

      Public Overrides ReadOnly Property Type As enumPropertyType
         Get
            Return enumPropertyType.decimal
         End Get
      End Property

   End Class
End Namespace