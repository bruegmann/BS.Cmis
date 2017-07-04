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

Namespace CmisObjectModel.Collections.Generic
   Public Class Tree(Of TValue)

      Public Sub New()
         Me.New(Nothing)
      End Sub
      Protected Sub New(parent As Tree(Of TValue))
         _parent = parent
      End Sub
      Protected Overridable Function CreateSubTree(parent As Tree(Of TValue)) As Tree(Of TValue)
         Return New Tree(Of TValue)(parent)
      End Function

      Private _count As Integer = 0
      Private _parent As Tree(Of TValue)
      Private _subTrees As New List(Of Tree(Of TValue))
      Private _value As Common.Generic.Nullable(Of TValue) = Nothing

      ''' <summary>
      ''' Removes all subtrees and the resets value to unset
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub Clear()
         For Each tree As Tree(Of TValue) In _subTrees
            tree.Clear()
            tree._parent = Nothing
         Next
         _subTrees.Clear()
         UnsetValue()
      End Sub

      ''' <summary>
      ''' Returns the values stored in this tree and all subtrees
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property Count As Integer
         Get
            Return _count
         End Get
         Private Set(value As Integer)
            If _parent IsNot Nothing Then _parent.Count += (value - _count)
            _count = value
         End Set
      End Property


      ''' <summary>
      ''' Returns the value for specified path
      ''' </summary>
      ''' <param name="path"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetValue(ParamArray path As Integer()) As Common.Generic.Nullable(Of TValue)
         Dim tree As Tree(Of TValue) = Me
         Dim uBound As Integer = If(path Is Nothing, -1, path.Length - 1)

         For pathIndex As Integer = 0 To uBound
            Dim index As Integer = Math.Max(0, path(pathIndex))
            If tree._subTrees.Count <= index Then
               Return Nothing
            Else
               tree = tree._subTrees(index)
            End If
         Next

         Return tree._value
      End Function

      Public Property Item(ParamArray path As Integer()) As TValue
         Get
            Return Tree(path)._value
         End Get
         Set(value As TValue)
            Dim tree As Tree(Of TValue) = Me.Tree(path)

            If Not tree._value.HasValue Then tree.Count += 1
            tree._value = value
         End Set
      End Property

      ''' <summary>
      ''' Removes the value for specified path
      ''' </summary>
      ''' <param name="path"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Remove(ParamArray path As Integer()) As Boolean
         Dim pathLength As Integer = If(path Is Nothing, 0, path.Length)
         Dim tree As Tree(Of TValue) = Me
         Dim trees As New Stack(Of Tuple(Of Integer, Tree(Of TValue)))
         Dim retVal As Boolean

         If path IsNot Nothing Then
            For pathIndex As Integer = 0 To pathLength - 1
               Dim index As Integer = Math.Max(0, path(pathIndex))
               If tree._subTrees.Count > index Then
                  trees.Push(New Tuple(Of Integer, Tree(Of TValue))(index, tree))
                  tree = tree._subTrees(index)
               End If
            Next
         End If
         retVal = (trees.Count = pathLength)
         'remove the value
         If retVal AndAlso tree._value.HasValue Then
            tree._value = Nothing
            tree.Count -= 1
         End If
         'remove empty tree-instances (unset value, no subtrees)
         While trees.Count > 0
            With trees.Pop()
               If Not tree._value.HasValue AndAlso tree._subTrees.Count = 0 Then
                  tree._parent = Nothing
                  tree = .Item2
                  tree._subTrees.RemoveAt(.Item1)
               Else
                  Exit While
               End If
            End With
         End While

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the subtrees of the current tree
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property SubTrees As List(Of Tree(Of TValue))
         Get
            Return _subTrees
         End Get
      End Property

      ''' <summary>
      ''' Returns a list of values of this instance and its subtrees
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ToList() As List(Of TValue)
         Dim retVal As New List(Of TValue)
         ToList(retVal)
         Return retVal
      End Function
      Private Sub ToList(list As List(Of TValue))
         If _value.HasValue Then list.Add(_value.Value)
         For Each tree As Tree(Of TValue) In _subTrees
            tree.ToList(list)
         Next
      End Sub

      ''' <summary>
      ''' Returns the tree following the index-path
      ''' </summary>
      ''' <param name="path"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Tree(ParamArray path As Integer()) As Tree(Of TValue)
         Get
            Dim retVal As Tree(Of TValue) = Me
            Dim uBound As Integer = If(path Is Nothing, -1, path.Length - 1)

            For pathIndex As Integer = 0 To uBound
               Dim index As Integer = Math.Max(0, path(pathIndex))
               'ensure instances
               While retVal._subTrees.Count <= index
                  retVal._subTrees.Add(CreateSubTree(retVal))
               End While
               retVal = retVal._subTrees(index)
            Next

            Return retVal
         End Get
      End Property

      ''' <summary>
      ''' Returns the depth of path, that is the number of indexes defined in this tree
      ''' </summary>
      ''' <param name="path"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ValidPathDepth(ParamArray path As Integer()) As Integer
         Get
            Dim retVal As Integer = 0
            Dim tree As Tree(Of TValue) = Me
            Dim uBound As Integer = If(path Is Nothing, 0, path.Length - 1)

            For pathIndex As Integer = 0 To uBound
               Dim index As Integer = Math.Max(0, path(pathIndex))
               If tree._subTrees.Count <= index Then
                  Return retVal
               Else
                  retVal += 1
                  tree = tree._subTrees(index)
               End If
            Next

            Return retVal
         End Get
      End Property

      ''' <summary>
      ''' Resets value to unset
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub UnsetValue()
         If _value.HasValue Then
            _value = Nothing
            Count -= 1
         End If
      End Sub

   End Class
End Namespace