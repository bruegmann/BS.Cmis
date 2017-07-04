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

Namespace CmisObjectModel.Common.Generic
   ''' <summary>
   ''' allows nothing as a valid value
   ''' </summary>
   ''' <typeparam name="T"></typeparam>
   ''' <remarks></remarks>
   Public Structure Nullable(Of T)
      
      Public Sub New(value As T)
         Me.Value = value
         HasValue = True
      End Sub

      Public HasValue As Boolean
      Public Value As T

      Public Overrides Function Equals(obj As Object) As Boolean
         If TypeOf obj Is T OrElse obj Is Nothing Then
            Return Equals(CType(obj, T))
         ElseIf TypeOf obj Is Nullable(Of T) Then
            Return Equals(CType(obj, Nullable(Of T)))
         Else
            Return False
         End If
      End Function

      Public Overloads Function Equals(obj As T) As Boolean
         If Not HasValue Then
            Return False
         ElseIf Value Is Nothing Then
            Return obj Is Nothing OrElse obj.Equals(Value)
         Else
            Return Value.Equals(obj)
         End If
      End Function

      Public Overloads Function Equals(obj As Nullable(Of T)) As Boolean
         If HasValue <> obj.HasValue Then
            Return False
         Else
            Return Not HasValue OrElse Equals(obj.Value)
         End If
      End Function

      Public Overrides Function ToString() As String
         If Not HasValue Then
            Return Me.GetType.Name & ": value not set"
         Else
            Return If(Value Is Nothing, Nothing, Value.ToString())
         End If
      End Function

      Public Shared Widening Operator CType(value As T) As Nullable(Of T)
         Return New Nullable(Of T)(value)
      End Operator
      Public Shared Widening Operator CType(value As Nullable(Of T)) As T
         Return value.Value
      End Operator
   End Structure
End Namespace