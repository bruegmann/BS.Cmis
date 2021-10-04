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
   ''' This class represents the elements which can be hosted by the backbone. The given WeakListenerCallback-instance
   ''' in the constructor is only stored in a weakreference. Therefor the caller is responsible for the lifetime of the
   ''' callback instance.
   ''' </summary>
   Public NotInheritable Class WeakListener

      Public Sub New(callback As WeakListenerCallback,
                     serviceDocUri As String, repositoryId As String, eventName As String,
                     ParamArray eventParameters As String())
         _callback = New WeakReference(callback)
         Me.ServiceDocUri = serviceDocUri
         Me.RepositoryId = repositoryId
         Me.EventName = eventName
         Me.EventParameters = eventParameters
         Backbone.Root.AddListener(Me)
      End Sub
      Public Shared Function CreateInstance(callback As WeakListenerCallback,
                                            serviceDocUri As String, repositoryId As String, eventName As String,
                                            ParamArray eventParameters As String()) As WeakListener
         Return New WeakListener(callback, serviceDocUri, repositoryId, eventName, eventParameters)
      End Function

      Private _callback As WeakReference
      Public ReadOnly EventName As String
      Public ReadOnly EventParameters As String()

      Public Function Invoke(e As EventArgs) As CmisObjectModel.EventBus.enumEventBusListenerResult
         Dim callback As WeakListenerCallback = DirectCast(_callback.Target, WeakListenerCallback)

         Return If(callback Is Nothing, CmisObjectModel.EventBus.enumEventBusListenerResult.removeListener, callback.Invoke(e))
      End Function

      Private Shared _nextOrderIndex As Long = 0L
      Public ReadOnly OrderIndex As Long = System.Threading.Interlocked.Increment(_nextOrderIndex)

      Public Sub RemoveListener()
         Backbone.Root.RemoveListener(Me)
      End Sub

      Public ReadOnly RepositoryId As String
      Public ReadOnly ServiceDocUri As String

   End Class

   Public Delegate Function WeakListenerCallback(e As EventArgs) As CmisObjectModel.EventBus.enumEventBusListenerResult
End Namespace