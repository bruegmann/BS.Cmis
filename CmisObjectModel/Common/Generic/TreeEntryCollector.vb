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
   ''' A simple collector of entries in a tree
   ''' </summary>
   ''' <typeparam name="TList"></typeparam>
   ''' <typeparam name="TEntry"></typeparam>
   ''' <remarks></remarks>
   Public Class TreeEntryCollector(Of TList, TEntry)

#Region "Constructors"
      Protected Sub New()
      End Sub

      Public Sub New(getChildren As Func(Of TEntry, TList), getEntries As Func(Of TList, IEnumerable(Of TEntry)))
         _getChildren = getChildren
         _getEntries = getEntries
      End Sub
#End Region

      ''' <summary>
      ''' Starts collecting objects from a given entry
      ''' </summary>
      ''' <param name="startEntry"></param>
      ''' <param name="includeStartEntry"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Collect(startEntry As TEntry, Optional includeStartEntry As Boolean = True) As List(Of TEntry)
         Dim retVal As New List(Of TEntry)

         If startEntry IsNot Nothing Then
            If includeStartEntry Then retVal.Add(startEntry)
            Append(retVal, GetChildren(startEntry))
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Starts collecting objects from given entries
      ''' </summary>
      ''' <param name="startEntries"></param>
      ''' <param name="includeStartEntries"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Collect(startEntries As TEntry(), Optional includeStartEntries As Boolean = True) As List(Of TEntry)
         Dim retVal As New List(Of TEntry)

         If startEntries IsNot Nothing Then
            For Each entr As TEntry In startEntries
               If includeStartEntries Then retVal.Add(entr)
               Append(retVal, GetChildren(entr))
            Next
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Starts collecting object from a given list
      ''' </summary>
      ''' <param name="startLists"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Collect(ParamArray startLists As TList()) As List(Of TEntry)
         Dim retVal As New List(Of TEntry)

         If startLists IsNot Nothing Then
            For Each startList As TList In startLists
               Append(retVal, startList)
            Next
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Recursive walk through the tree
      ''' </summary>
      ''' <param name="result"></param>
      ''' <param name="list"></param>
      ''' <remarks></remarks>
      Protected Sub Append(result As List(Of TEntry), list As TList)
         Dim lists As New Queue(Of TList)

         If list IsNot Nothing Then lists.Enqueue(list)
         While lists.Count > 0
            Dim entries As IEnumerable(Of TEntry) = GetEntries(lists.Dequeue())

            If entries IsNot Nothing Then
               For Each entry As TEntry In entries
                  If entry IsNot Nothing Then
                     result.Add(entry)
                     list = GetChildren(entry)
                     If list IsNot Nothing Then lists.Enqueue(list)
                  End If
               Next
            End If
         End While
      End Sub

      Protected _getChildren As Func(Of TEntry, TList)
      Protected Overridable Function GetChildren(entry As TEntry) As TList
         Return If(entry Is Nothing OrElse _getChildren Is Nothing, Nothing, _getChildren(entry))
      End Function

      Protected _getEntries As Func(Of TList, IEnumerable(Of TEntry))
      Protected Overridable Function GetEntries(list As TList) As IEnumerable(Of TEntry)
         Return If(list Is Nothing OrElse _getEntries Is Nothing, Nothing, GetEntries(list))
      End Function

   End Class
End Namespace