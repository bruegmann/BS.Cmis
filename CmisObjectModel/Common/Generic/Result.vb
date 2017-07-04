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
   ''' Generic class to return a valid result or an exception
   ''' </summary>
   ''' <typeparam name="T"></typeparam>
   ''' <remarks></remarks>
   Public Class Result(Of T)

      Public Sub New(failure As Exception)
         Me.Failure = failure
      End Sub
      Public Sub New(success As T)
         Me._success = success
      End Sub

      Public ReadOnly Failure As Exception
      Private ReadOnly _success As T
      Public ReadOnly Property Success As T
         Get
            If Failure Is Nothing Then
               Return _success
            Else
               Throw Failure
            End If
         End Get
      End Property

      Public Shared Widening Operator CType(value As T) As Result(Of T)
         Return New Result(Of T)(value)
      End Operator
      Public Shared Widening Operator CType(value As Exception) As Result(Of T)
         Return New Result(Of T)(value)
      End Operator
   End Class
End Namespace