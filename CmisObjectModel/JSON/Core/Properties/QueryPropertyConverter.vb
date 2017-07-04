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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Core.Properties
   ''' <summary>
   ''' Converter for cmisProperty-instances in query results
   ''' </summary>
   ''' <remarks></remarks>
   Public Class QueryPropertyConverter
      Inherits PropertyConverter

#Region "Constructors"
      Public Sub New()
         MyBase.New()
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.Properties.cmisProperty))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overrides Function SerializeValue(context As SerializationContext) As Object
         With context
            If .Object.Values Is Nothing Then
               Return Nothing
            ElseIf TypeOf .Object Is CmisObjectModel.Core.Properties.cmisPropertyDateTime Then
               Dim [object] As CmisObjectModel.Core.Properties.cmisPropertyDateTime = CType(.Object, CmisObjectModel.Core.Properties.cmisPropertyDateTime)

               If [object].Values.Length = 1 Then
                  Return Common.ToJSONTime([object].Value.DateTime)
               Else
                  Return (From value As DateTimeOffset In [object].Values
                          Select Common.ToJSONTime(value.DateTime)).ToArray()
               End If
            ElseIf .Object.Values.Length = 1 Then
               Return .Object.Value
            Else
               Return .Object.Values
            End If
         End With
      End Function

   End Class
End Namespace