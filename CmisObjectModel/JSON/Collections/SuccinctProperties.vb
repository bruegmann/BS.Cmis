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
Imports ccc = CmisObjectModel.Core.Collections
Imports ccg = CmisObjectModel.Collections.Generic
Imports ccg1 = CmisObjectModel.Common.Generic
Imports ccp = CmisObjectModel.Core.Properties
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Collections
   ''' <summary>
   ''' Representation of cmisPropertiesType as a string-to-object-map
   ''' </summary>
   ''' <remarks>SuccinctProperties definition in a cmisObjectType</remarks>
   Public Class SuccinctProperties
      Inherits CmisObjectModel.Collections.Generic.ArrayMapper(Of CmisObjectModel.Core.Collections.cmisPropertiesType, 
                                                                  CmisObjectModel.Core.Properties.cmisProperty,
                                                                  String, Object, JSON.Serialization.CmisPropertyResolver)

      Public Sub New(owner As CmisObjectModel.Core.Collections.cmisPropertiesType)
         MyBase.New(owner, owner.DefaultArrayProperty, ccp.cmisProperty.DefaultKeyProperty,
                    New ccg1.DynamicProperty(Of ccp.cmisProperty, Object)(Function(cmisProperty)
                                                                             Dim values As Object() = cmisProperty.Values
                                                                             Dim length As Integer = If(values Is Nothing, 0, values.Length)

                                                                             Select Case length
                                                                                Case 0
                                                                                   Return Nothing
                                                                                Case 1
                                                                                   Return values(0)
                                                                                Case Else
                                                                                   Return cmisProperty.PropertyType.CreateValuesArray(values)
                                                                             End Select
                                                                          End Function,
                                                                          Sub(cmisProperty, values)
                                                                             If values Is Nothing Then
                                                                                cmisProperty.Values = Nothing
                                                                                cmisProperty.Cardinality = CmisObjectModel.Core.enumCardinality.multi
                                                                             ElseIf values.GetType().IsArray Then
                                                                                cmisProperty.Values = (From value As Object In CType(values, IEnumerable)
                                                                                                       Select value).ToArray()
                                                                                cmisProperty.Cardinality = CmisObjectModel.Core.enumCardinality.multi
                                                                             Else
                                                                                cmisProperty.Value = values
                                                                                cmisProperty.Cardinality = CmisObjectModel.Core.enumCardinality.single
                                                                             End If
                                                                          End Sub,
                                                                          "Value"))
      End Sub

#Region "IJavaSerializationProvider"
      ''' <summary>
      ''' More comfortable access to JavaExport
      ''' </summary>
      Public Shadows Function JavaExport() As IDictionary(Of String, Object)
         Return TryCast(MyBase.JavaExport(Me, Nothing), IDictionary(Of String, Object))
      End Function

      ''' <summary>
      ''' More comfortable access to JavaImport
      ''' </summary>
      Public Shadows Function JavaImport(source As IDictionary(Of String, Object)) As CmisObjectModel.Core.Collections.cmisPropertiesType
         MyBase.JavaImport(source, Nothing)
         Return _owner
      End Function
#End Region

   End Class
End Namespace