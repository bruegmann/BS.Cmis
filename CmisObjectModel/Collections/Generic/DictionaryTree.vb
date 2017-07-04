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
   Public Class DictionaryTree(Of TKey, TValue)

      Public Sub New()
         Me.New(Nothing)
      End Sub
      Protected Sub New(parent As DictionaryTree(Of TKey, TValue))
         _parent = parent
      End Sub
      Protected Overridable Function CreateSubTree(parent As DictionaryTree(Of TKey, TValue)) As DictionaryTree(Of TKey, TValue)
         Return New DictionaryTree(Of TKey, TValue)(parent)
      End Function

      Private _count As Integer = 0
      Private _parent As DictionaryTree(Of TKey, TValue)
      Private _subTrees As New Dictionary(Of TKey, DictionaryTree(Of TKey, TValue))
      Private _value As Common.Generic.Nullable(Of TValue) = Nothing

      ''' <summary>
      ''' Removes all subtrees and the resets value to unset
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub Clear()
         For Each de As KeyValuePair(Of TKey, DictionaryTree(Of TKey, TValue)) In _subTrees
            de.Value.Clear()
            de.Value._parent = Nothing
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
      ''' Returns True if the specified keys exist in the tree
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <returns></returns>
      ''' <remarks>A null-key is a placeholder for any key</remarks>
      Public Function ContainsKeys(ParamArray keys As TKey()) As Boolean
         Dim tree As DictionaryTree(Of TKey, TValue) = Me
         Dim uBound As Integer = If(keys Is Nothing, -1, keys.Length - 1)

         For index As Integer = 0 To uBound
            Dim key As TKey = keys(index)

            If key Is Nothing Then
               'placeholder
               If index = uBound Then
                  'last level
                  Return tree._subTrees.Count > 0
               Else
                  'search subkeys in all subtrees
                  keys = keys.Copy(index + 1)
                  For Each subTree As DictionaryTree(Of TKey, TValue) In tree._subTrees.Values
                     If subTree.ContainsKeys(keys) Then Return True
                  Next
                  Return False
               End If
            ElseIf tree._subTrees.ContainsKey(key) Then
               tree = tree._subTrees(key)
            Else
               Return False
            End If
         Next

         Return True
      End Function


      ''' <summary>
      ''' Returns the value for specified keys
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <returns></returns>
      ''' <remarks>A null-key is a placeholder for any key</remarks>
      Public Function GetValue(ParamArray keys As TKey()) As Common.Generic.Nullable(Of TValue)
         Dim tree As DictionaryTree(Of TKey, TValue) = Me
         Dim uBound As Integer = If(keys Is Nothing, -1, keys.Length - 1)

         For index As Integer = 0 To uBound
            Dim key As TKey = keys(index)

            If key Is Nothing Then
               'search in all subtrees
               keys = keys.Copy(index + 1)
               For Each subTree As DictionaryTree(Of TKey, TValue) In tree._subTrees.Values
                  Dim retVal As Common.Generic.Nullable(Of TValue) = subTree.GetValue(keys)
                  If retVal.HasValue Then Return retVal
               Next
               Return Nothing
            ElseIf tree._subTrees.ContainsKey(key) Then
               tree = tree._subTrees(key)
            Else
               Return Nothing
            End If
         Next

         Return tree._value
      End Function

      Public Property Item(ParamArray keys As TKey()) As TValue
         Get
            Return Tree(keys)._value
         End Get
         Set(value As TValue)
            Dim tree As DictionaryTree(Of TKey, TValue) = Me.Tree(keys)

            If Not tree._value.HasValue Then tree.Count += 1
            tree._value = value
         End Set
      End Property

      ''' <summary>
      ''' Removes the value for specified keys
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Remove(ParamArray keys As TKey()) As Boolean
         Dim keysLength As Integer = If(keys Is Nothing, 0, keys.Length)
         Dim tree As DictionaryTree(Of TKey, TValue) = Me
         Dim trees As New Stack(Of Tuple(Of TKey, DictionaryTree(Of TKey, TValue)))
         Dim retVal As Boolean

         If keys IsNot Nothing Then
            For Each key As TKey In keys
               If key IsNot Nothing AndAlso tree._subTrees.ContainsKey(key) Then
                  trees.Push(New Tuple(Of TKey, DictionaryTree(Of TKey, TValue))(key, tree))
                  tree = tree._subTrees(key)
               Else
                  Exit For
               End If
            Next
         End If
         retVal = (trees.Count = keysLength)
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
                  tree._subTrees.Remove(.Item1)
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
      Public ReadOnly Property SubTrees As Dictionary(Of TKey, DictionaryTree(Of TKey, TValue))
         Get
            Return _subTrees
         End Get
      End Property

      ''' <summary>
      ''' Returns the tree following the keys-path
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>A null-key is a placeholder for any key; null-key at the end of the keys-array will be ignored</remarks>
      Public ReadOnly Property Tree(ParamArray keys As TKey()) As DictionaryTree(Of TKey, TValue)
         Get
            Dim retVal As DictionaryTree(Of TKey, TValue) = Me
            Dim uBound As Integer = If(keys Is Nothing, -1, keys.Length - 1)

            For index As Integer = 0 To uBound
               Dim key As TKey = keys(index)

               If key Is Nothing Then
                  If index < uBound Then
                     keys = keys.Copy(index + 1)
                     'first chance: the specified subpath is defined in a subtree
                     For Each subTree As DictionaryTree(Of TKey, TValue) In retVal._subTrees.Values
                        If subTree.ContainsKeys(keys) Then Return subTree.Tree(keys)
                     Next

                     'second chance: best fit
                     Dim maxPathDepth As Integer = -1
                     For Each subTree As DictionaryTree(Of TKey, TValue) In retVal._subTrees.Values
                        Dim pathDepth As Integer = subTree.ValidPathDepth(keys)
                        If pathDepth > maxPathDepth Then
                           maxPathDepth = pathDepth
                           retVal = subTree
                        End If
                     Next
                  Else
                     'get the first subtree
                     For Each subTree As DictionaryTree(Of TKey, TValue) In retVal._subTrees.Values
                        retVal = subTree
                        Exit For
                     Next
                  End If
                  Exit For
               Else
                  If Not retVal._subTrees.ContainsKey(key) Then retVal._subTrees.Add(key, CreateSubTree(retVal))
                  retVal = retVal._subTrees(key)
               End If
            Next

            Return retVal
         End Get
      End Property

      ''' <summary>
      ''' Returns the depth of path, that is the number of keys defined in this tree
      ''' </summary>
      ''' <param name="path"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>A null-key is a placeholder for any key</remarks>
      Public ReadOnly Property ValidPathDepth(ParamArray path As TKey()) As Integer
         Get
            Dim retVal As Integer = 0
            Dim tree As DictionaryTree(Of TKey, TValue) = Me
            Dim uBound As Integer = If(path Is Nothing, 0, path.Length - 1)

            For index As Integer = 0 To uBound
               Dim key As TKey = path(index)

               If key Is Nothing Then
                  If index < uBound Then
                     Dim maxValidPathDepth As Integer = -1

                     path = path.Copy(index + 1)
                     For Each subTree As DictionaryTree(Of TKey, TValue) In tree._subTrees.Values
                        maxValidPathDepth = Math.Max(maxValidPathDepth, subTree.ValidPathDepth(path))
                     Next
                     Return retVal + maxValidPathDepth + 1
                  ElseIf tree._subTrees.Count = 0 Then
                     Return retVal
                  Else
                     Return retVal + 1
                  End If
               ElseIf tree._subTrees.ContainsKey(key) Then
                  retVal += 1
                  tree = tree._subTrees(key)
               Else
                  Exit For
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