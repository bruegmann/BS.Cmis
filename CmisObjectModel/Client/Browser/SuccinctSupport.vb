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

Namespace CmisObjectModel.Client.Browser
   Public Class SuccinctSupport

      ''' <summary>
      ''' Sets the current succinct value for the current thread
      ''' </summary>
      ''' <param name="succinct"></param>
      ''' <remarks></remarks>
      Public Shared Sub BeginSuccinct(succinct As Boolean)
         Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread

         SyncLock _succinctStacks
            Dim stack As Stack(Of Boolean)

            If _succinctStacks.ContainsKey(thread) Then
               stack = _succinctStacks(thread)
            Else
               stack = New Stack(Of Boolean)
               _succinctStacks.Add(thread, stack)
            End If
            stack.Push(succinct)
         End SyncLock
      End Sub

      ''' <summary>
      ''' Returns the current succinct value for the current thread
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property Current As Boolean
         Get
            Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread

            SyncLock _succinctStacks
               Return _succinctStacks.ContainsKey(thread) AndAlso _succinctStacks(thread).Peek()
            End SyncLock
         End Get
      End Property

      ''' <summary>
      ''' Removes the current succinct value valid for the current thread
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function EndSuccinct() As Boolean
         Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread
         Dim retVal As Boolean

         SyncLock _succinctStacks
            If _succinctStacks.ContainsKey(thread) Then
               Dim stack As Stack(Of Boolean) = _succinctStacks(thread)
               Dim count As Integer = stack.Count

               If count > 0 Then
                  retVal = stack.Pop()
                  count -= 1
               Else
                  retVal = False
               End If
               If count = 0 Then _succinctStacks.Remove(thread)
            Else
               retVal = False
            End If
         End SyncLock

         Return retVal
      End Function

      Private Shared _succinctStacks As New Dictionary(Of System.Threading.Thread, Stack(Of Boolean))

   End Class
End Namespace