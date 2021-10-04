'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2017, Brügmann Software GmbH, Papenburg, All rights reserved
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
Namespace CmisObjectModel.EventBus
   ''' <summary>
   ''' Class to handle Events only known by names
   ''' </summary>
   ''' <remarks>
   ''' The purpose of the Backbone/WeakListener is to give easy way to consume an event without knowing instances of
   ''' objects that may raise them. The design prevents memoryleaks because every callback method the is connected to
   ''' the backbone is stored in a weakreference. It lies in the responsibility of the calling class to keep the
   ''' garbage collector from the callback method as long as its used. You find a small example how to manage lifetime
   ''' of the callback method in the class EventConsumerSample.
   ''' </remarks>
   Public NotInheritable Class Backbone

      Public Const csWildcard As String = "*"

      Private Sub New()
      End Sub
      Private Sub New(parent As Backbone, childKey As String)
         _parent = parent
         _childKey = childKey
         parent._children(childKey) = Me
      End Sub

#Region "Helper classes"
      ''' <summary>
      ''' Sample to consume a named event
      ''' </summary>
      Private Class EventConsumerSample

         Public Sub New()
            'Only if You want to remove the listener manually (a good idea for classes with the IDispose-interface),
            'You have to store the instance
            CmisObjectModel.EventBus.WeakListener.CreateInstance(_myEventHandler, "MyServiceUri", "MyRespositoryId", "MyEventName")
         End Sub

         Private _myEventHandler As New CmisObjectModel.EventBus.WeakListenerCallback(AddressOf MyEventHandler)
         Private Function MyEventHandler(e As CmisObjectModel.EventBus.EventArgs) As CmisObjectModel.EventBus.enumEventBusListenerResult
            'Your code here

            'if You only want to execute the handler once, return removeListener to remove the listener automatically
            Return CmisObjectModel.EventBus.enumEventBusListenerResult.dontCare
         End Function
      End Class
#End Region

      Public Sub AddListener(listener As WeakListener)
         SyncLock _syncObject
            Dim current As Backbone = Me
            Dim child As Backbone = Nothing

            For Each key As String In GetKeys(listener.ServiceDocUri, listener.RepositoryId, listener.EventName, listener.EventParameters)
               If current._children.TryGetValue(key, child) Then
                  current = child
               Else
                  current = New Backbone(current, key)
               End If
            Next
            current._listeners.Add(listener)
         End SyncLock
      End Sub

      Private ReadOnly _childKey As String
      Private ReadOnly _children As New Dictionary(Of String, Backbone)

      Public Shared Sub DispatchEvent(e As EventArgs)
         Root.DispatchEventCore(e)
      End Sub
      Protected Sub DispatchEventCore(e As EventArgs)
         Dim listeners As WeakListener()
         Dim pendingRemovals As New List(Of WeakListener)

         SyncLock _syncObject
            listeners = (From listener As WeakListener In GetListeners(GetKeys(e.ServiceDocUri, e.RepositoryId, e.EventName, e.EventParameters))
                         Select listener
                         Order By listener.OrderIndex).ToArray()
         End SyncLock
         For index As Integer = 0 To listeners.Length - 1
            Dim listener As WeakListener = listeners(index)
            Try
               If listener.Invoke(e) = CmisObjectModel.EventBus.enumEventBusListenerResult.removeListener Then pendingRemovals.Add(listener)
            Catch
            End Try
         Next
         SyncLock _syncObject
            For index As Integer = 0 To pendingRemovals.Count - 1
               RemoveListener(pendingRemovals(index))
            Next
         End SyncLock
      End Sub

      ''' <summary>
      ''' Returns key or '*'-placeholder if key is not set
      ''' </summary>
      Private Shared Function GetKey(key As String) As String
         Return If(String.IsNullOrEmpty(key), csWildcard, key)
      End Function

      Private Shared Iterator Function GetKeys(serviceDocUri As String, repositoryId As String, eventName As String,
                                               ParamArray eventParameters As String()) As IEnumerable(Of String)
         Yield GetKey(serviceDocUri)
         Yield GetKey(repositoryId)
         Yield GetKey(eventName)

         Dim length As Integer = If(eventParameters Is Nothing, 0, eventParameters.Length)
         For index As Integer = 0 To length - 1
            Yield GetKey(eventParameters(index))
         Next
      End Function

      ''' <summary>
      ''' Returns an IEnumerable-instance which contains all WeakListener-instances suitable for given keys
      ''' </summary>
      Private Iterator Function GetListeners(keys As IEnumerable(Of String)) As IEnumerable(Of WeakListener)
         Dim instances As New Queue(Of Backbone)
         Dim child As Backbone = Nothing

         'every listener stored in root belongs to the result
         For Each listener As WeakListener In _listeners
            Yield listener
         Next

         instances.Enqueue(Me)
         'The algorithm uses the fact, that each listener in each child with a suitable key belongs to the result.
         'For example: if keys-sequence contains 'FirstKey', 'SecondKey', 'ThirdKey' then all listeners belongs
         'to the result if their key-sequence is complete contained in the keys-sequence beginning from the first
         'key.
         'No.  key-sequence sample                                    belongs to keys  remarks
         ' 1.  'FirstKey','SecondKey'                                 yes              keys-sequence starts with key-sequence
         ' 2.  '*', 'SecondKey'                                       yes              wildcard matches every key
         ' 3.  'FirstKey','AnotherSecondKey','ThirdKey'               no               'AnotherSecondKey' does not match
         ' 4.  'FirstKey','SecondKey','ThirdKey','FourthKey'          no               keys-sequence to unspecific
         ' 5.  'FirstKey','SecondKey','ThirdKey','*'                  no               keys-sequence to unspecific; wildcard
         '                                                                             can only match every EXISTING key
         'If the keys-sequence contains 'FirstKey','*','ThirdKey' then 3. matches as well.
         For Each key As String In keys
            Dim count As Integer = instances.Count

            If count = 0 Then
               'no listener registered
               Exit Function
            Else
               For index As Integer = 1 To count
                  With instances.Dequeue()
                     If key.Equals(csWildcard) Then
                        'attend every child
                        For Each de As KeyValuePair(Of String, Backbone) In ._children
                           'every listener in a child belongs to the result
                           For Each listener As WeakListener In de.Value._listeners
                              Yield listener
                           Next
                           instances.Enqueue(de.Value)
                        Next
                     Else
                        'accept only the childs with suitable key or wildcard
                        For suitableKeyIndex As Integer = 0 To 1
                           If ._children.TryGetValue(If(suitableKeyIndex = 0, key, csWildcard), child) Then
                              'every listener in this child belongs to the result
                              For Each listener As WeakListener In child._listeners
                                 Yield listener
                              Next
                              instances.Enqueue(child)
                           End If
                        Next
                     End If
                  End With
               Next
            End If
         Next
      End Function

      Private ReadOnly _listeners As New HashSet(Of WeakListener)
      Private ReadOnly _parent As Backbone

      Public Sub RemoveListener(listener As WeakListener)
         SyncLock _syncObject
            Dim current As Backbone = Me

            For Each key As String In GetKeys(listener.ServiceDocUri, listener.RepositoryId, listener.EventName, listener.EventParameters)
               If Not current._children.TryGetValue(key, current) Then
                  'unknown listener
                  Exit Sub
               End If
            Next
            current._listeners.Remove(listener)
            'remove empty Backbone-instances
            While current IsNot Root AndAlso current._listeners.Count = 0 AndAlso current._children.Count = 0
               Dim parent As Backbone = current._parent

               parent._children.Remove(current._childKey)
               current = parent
            End While
         End SyncLock
      End Sub

      Public Shared ReadOnly Root As New Backbone()
      Private _syncObject As New Object()

   End Class
End Namespace