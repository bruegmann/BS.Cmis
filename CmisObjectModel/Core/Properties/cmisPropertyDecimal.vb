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
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Properties
   <sxs.XmlRoot(cmisPropertyDecimal.DefaultElementName, Namespace:=Constants.Namespaces.cmis),
    Attributes.CmisTypeInfo(cmisPropertyDecimal.CmisTypeName, cmisPropertyDecimal.TargetTypeName, cmisPropertyDecimal.DefaultElementName)>
   Partial Public Class cmisPropertyDecimal

      Public Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String, ParamArray values As Decimal())
         MyBase.New(propertyDefinitionId, localName, displayName, queryName, values)
      End Sub

#Region "Constants"
      Public Const CmisTypeName As String = "cmis:cmisPropertyDecimal"
      Public Const TargetTypeName As String = "decimal"
      Public Const DefaultElementName As String = "propertyDecimal"
#End Region

#Region "IComparable"
      Protected Overrides Function CompareTo(ParamArray other As Decimal()) As Integer
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

      Public Overrides ReadOnly Property Type As enumPropertyType
         Get
            Return enumPropertyType.decimal
         End Get
      End Property

   End Class
End Namespace