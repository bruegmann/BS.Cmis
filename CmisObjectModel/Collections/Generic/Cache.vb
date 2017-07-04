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
   ''' <summary>
   ''' Cache implementation for a defined maximal amount of key-value-pairs
   ''' </summary>
   ''' <typeparam name="TKey"></typeparam>
   ''' <typeparam name="TValue"></typeparam>
   ''' <remarks></remarks>
   Public Class Cache(Of TKey, TValue)

#Region "Constructors"
      ''' <summary>
      ''' Create a new cache
      ''' </summary>
      ''' <param name="capacity"></param>
      ''' <param name="leaseTime">Time in seconds</param>
      ''' <param name="autoRenewExpiration">If True every reading access to an entry will update the expiration date of the entry</param>
      ''' <remarks></remarks>
      Public Sub New(capacity As Integer, leaseTime As Double,
                     autoRenewExpiration As Boolean)
         _capacity = Math.Max(1, capacity)
         _fifoCapacity = _capacity
         'not necessary to renew leasetime if the leasetime itself is set to Date.MaxValue
         _autoRenewExpiration = autoRenewExpiration AndAlso Not Double.IsPositiveInfinity(leaseTime)
         _leaseTime = leaseTime
      End Sub
#End Region

#Region "Helper-classes"
      ''' <summary>
      ''' Entry of cache handling expiration and refcounter
      ''' </summary>
      ''' <remarks></remarks>
      Private Class CacheEntry

#Region "Constructors"
         Public Sub New(owner As Cache(Of TKey, TValue), keys As TKey(), value As TValue)
            _owner = owner
            Me.Keys = keys
            Me._value = value
            RenewExpiration()
         End Sub
#End Region

#Region "RefCounter"
         Public Function AddRef() As Integer
            If _refCounter > 0 Then
               _refCounter += 1
               _owner._fifoCapacity += 1
            End If

            Return _refCounter
         End Function
         Public Function Release() As Integer
            If _refCounter > 0 Then
               _refCounter -= 1
               _owner._fifoCapacity -= 1
            End If

            Return _refCounter
         End Function

         Private _refCounter As Integer = 1
         Public ReadOnly Property RefCount As Integer
            Get
               Return _refCounter
            End Get
         End Property
#End Region

         ''' <summary>
         ''' Expiration date
         ''' </summary>
         ''' <remarks></remarks>
         Private _absoluteExpiration As DateTime

         ''' <summary>
         ''' Returns True if the expiration date lies in the past
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public ReadOnly Property IsExpired As Boolean
            Get
               Return DateTime.UtcNow > _absoluteExpiration
            End Get
         End Property

         ''' <summary>
         ''' Returns True if the instance is expired or has more than one reference in the fifo
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public ReadOnly Property IsMultiReferencedOrExpired As Boolean
            Get
               Return _refCounter > 1 OrElse IsExpired
            End Get
         End Property

         ''' <summary>
         ''' Key of current instance in the cache
         ''' </summary>
         ''' <remarks></remarks>
         Public ReadOnly Keys As TKey()

         Private _owner As Cache(Of TKey, TValue)

         Private _value As TValue
         Public Property Value As TValue
            Get
               Return _value
            End Get
            Set(value As TValue)
               _value = value
            End Set
         End Property

         ''' <summary>
         ''' Recalculates the expiration date
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub RenewExpiration()
            If Double.IsPositiveInfinity(_owner._leaseTime) Then
               _absoluteExpiration = DateTime.MaxValue
            Else
               _absoluteExpiration = DateTime.UtcNow.AddSeconds(_owner._leaseTime)
            End If
         End Sub

         ''' <summary>
         ''' Marks the current CacheEntry as expired
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub SetIsExpired()
            _absoluteExpiration = DateTime.MinValue
         End Sub
      End Class
#End Region

      Private _autoRenewExpiration As Boolean
      Private _capacity As Integer
      Private _cache As New DictionaryTree(Of TKey, CacheEntry)
      Private _fifoCapacity As Integer
      Private _fifo As New Queue(Of CacheEntry)
      Private _lastEntry As CacheEntry
      Private _leaseTime As Double

      ''' <summary>
      ''' Capacity of the cache
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property Capacity As Integer
         Get
            Return _capacity
         End Get
         Set(value As Integer)
            If value > 0 AndAlso value <> _capacity Then
               SyncLock _cache
                  Dim fifoCapacityOffset As Integer = _fifoCapacity - _capacity

                  _capacity = value
                  _fifoCapacity = value + fifoCapacityOffset
                  Purge()
               End SyncLock
            End If
         End Set
      End Property

      ''' <summary>
      ''' Removes cacheentries from tree and all of its subtrees.
      ''' The function returns TRUE if any entry is removed
      ''' </summary>
      ''' <param name="tree"></param>
      ''' <remarks></remarks>
      Private Function Clear(tree As DictionaryTree(Of TKey, CacheEntry)) As Boolean
         Dim nullableEntry As Common.Generic.Nullable(Of CacheEntry) = tree.GetValue()
         Dim retVal As Boolean = False

         For Each de As KeyValuePair(Of TKey, DictionaryTree(Of TKey, CacheEntry)) In tree.SubTrees
            retVal = Clear(de.Value) OrElse retVal
         Next
         If nullableEntry.HasValue Then
            nullableEntry.Value.SetIsExpired()
            retVal = True
         End If
         tree.Clear()

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the CacheEntry for given key if exists, otherwise null
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private ReadOnly Property Entry(ParamArray keys As TKey()) As CacheEntry
         Get
            Dim nullableEntry As Common.Generic.Nullable(Of CacheEntry) = _cache.GetValue(keys)

            If nullableEntry.HasValue Then
               Dim retVal As CacheEntry = nullableEntry.Value

               If retVal.IsExpired Then
                  _cache.Remove(keys)
                  Return Nothing
               ElseIf _lastEntry IsNot retVal Then
                  'fifo has to be updated
                  _lastEntry = retVal
                  If _fifo.Peek() Is retVal Then
                     _fifo.Dequeue()
                  Else
                     retVal.AddRef()
                  End If
                  _fifo.Enqueue(retVal)
               End If
               If _autoRenewExpiration Then retVal.RenewExpiration()

               Return retVal
            Else
               Return Nothing
            End If
         End Get
      End Property

      ''' <summary>
      ''' Gets or sets the cached value
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>Use setter for adding or modifying values</remarks>
      Public Property Item(ParamArray keys As TKey()) As TValue
         Get
            SyncLock _cache
               Try
                  Dim entry As CacheEntry = Me.Entry(keys)
                  Return If(entry Is Nothing, Nothing, entry.Value)
               Finally
                  Purge()
               End Try
            End SyncLock
         End Get
         Set(value As TValue)
            SyncLock _cache
               Try
                  Dim entry As CacheEntry = Me.Entry(keys)

                  If entry Is Nothing Then
                     entry = New CacheEntry(Me, keys, value)
                     _cache.Item(keys) = entry
                     _fifo.Enqueue(entry)
                  Else
                     'change value of entry and recalculate expiration date
                     entry.Value = value
                     entry.RenewExpiration()
                  End If
               Finally
                  Purge()
               End Try
            End SyncLock
         End Set
      End Property

      ''' <summary>
      ''' Shrinks a bellied fifo-buffer and removes expired entries or entries that go beyond the scope of capacity
      ''' </summary>
      ''' <remarks>The fifo grows if an item is accessed that is not the current (_fifo.Peek()) in the fifo</remarks>
      Private Sub Purge(Optional enforce As Boolean = False)
         'check capacity and remove the oldest entries if necessary
         While _cache.Count > _capacity OrElse (_fifo.Count > 0 AndAlso _fifo.Peek().IsMultiReferencedOrExpired)
            With _fifo.Dequeue()
               If .RefCount = 1 Then
                  _cache.Remove(.Keys)
               Else
                  .Release()
               End If
            End With
         End While

         'the fifo-buffer has to be cleaned up
         If enforce OrElse ((_fifoCapacity >> 1) > _capacity) Then
            Dim fifo As New Queue(Of CacheEntry)

            While _fifo.Count > 0
               Dim entry As CacheEntry = _fifo.Dequeue

               If entry.RefCount = 1 Then
                  If entry.IsExpired Then
                     _cache.Remove(entry.Keys)
                  Else
                     fifo.Enqueue(entry)
                  End If
               Else
                  entry.Release()
               End If
            End While
            _fifo = fifo
         End If
      End Sub

      ''' <summary>
      ''' Removes CacheEntry-instances for all paths starting with keys
      ''' </summary>
      ''' <param name="keys"></param>
      ''' <remarks></remarks>
      Public Sub RemoveAll(ParamArray keys As TKey())
         If Clear(_cache.Tree(keys)) Then Purge(True)
      End Sub

      ''' <summary>
      ''' Returns the number of keys in the given path defined in this cache-instance
      ''' </summary>
      ''' <param name="path"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ValidPathDepth(ParamArray path As TKey()) As Integer
         Get
            Return _cache.ValidPathDepth(path)
         End Get
      End Property

   End Class
End Namespace