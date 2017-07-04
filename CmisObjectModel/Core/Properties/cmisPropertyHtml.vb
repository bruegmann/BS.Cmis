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
   <sxs.XmlRoot(cmisPropertyHtml.DefaultElementName, Namespace:=Constants.Namespaces.cmis),
    Attributes.CmisTypeInfo(cmisPropertyHtml.CmisTypeName, cmisPropertyHtml.TargetTypeName, cmisPropertyHtml.DefaultElementName)>
   Partial Public Class cmisPropertyHtml

      Public Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String, ParamArray values As String())
         MyBase.New(propertyDefinitionId, localName, displayName, queryName, values)
      End Sub

#Region "Constants"
      Public Const CmisTypeName As String = "cmis:cmisPropertyHtml"
      Public Const TargetTypeName As String = "html"
      Public Const DefaultElementName As String = "propertyHtml"
#End Region

#Region "IComparable"
      Protected Overrides Function CompareTo(ParamArray other As String()) As Integer
         Dim length As Integer = If(_values Is Nothing, 0, _values.Length)
         Dim otherLength As Integer = If(other Is Nothing, 0, other.Length)
         If otherLength = 0 Then
            Return If(length = 0, 0, 1)
         ElseIf length = 0 Then
            Return -1
         Else
            For index As Integer = 0 To Math.Min(length, otherLength) - 1
               Dim result As Integer = String.Compare(_values(index), other(index))
               If result <> 0 Then Return result
            Next
            Return If(length = otherLength, 0, If(length > otherLength, 1, -1))
         End If
      End Function
#End Region

      Public Overrides ReadOnly Property Type As enumPropertyType
         Get
            Return enumPropertyType.html
         End Get
      End Property

   End Class
End Namespace