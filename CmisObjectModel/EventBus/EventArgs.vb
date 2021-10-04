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
   Public Class EventArgs
      Inherits System.EventArgs

#Region "Constructors"
      Protected Sub New(sender As Object, properties As Dictionary(Of String, Object),
                        serviceDocUri As String, repositoryId As String,
                        eventIdentifier As String, eventName As String, eventParameters As String())
         Me.Sender = sender
         _properties = properties
         Me.ServiceDocUri = serviceDocUri
         Me.RepositoryId = repositoryId
         Me.EventIdentifier = eventIdentifier
         Me.EventName = eventName
         Me.EventParameters = eventParameters
      End Sub
#End Region

#Region "Helper classes"
      Public MustInherit Class PredefinedPropertyNames
         Private Sub New()
         End Sub

         Public Const Failure As String = "failure"
         Public Shared ReadOnly FailureType As Type = GetType(System.ServiceModel.FaultException)

         Public Const NewObjectId As String = "newObjectId"
         Public Shared ReadOnly NewObjectIdType As Type = GetType(String)

         Public Const Succeeded As String = "succeeded"
         Public Shared ReadOnly SucceededType As Type = GetType(Boolean)
      End Class
#End Region

#Region "DispatchEvent"
      Public Shared Function DispatchBeginEvent(sender As Object, properties As Dictionary(Of String, Object),
                                                serviceDocUri As String, repositoryId As String, builtInEvent As enumBuiltInEvents,
                                                ParamArray eventParameters As String()) As EventArgs
         Dim retVal As New EventArgs(sender, properties, serviceDocUri, repositoryId, System.Guid.NewGuid.ToString("N"),
                                     GetBeginEventName(builtInEvent), eventParameters) With {._endEventName = GetEndEventName(builtInEvent)}
         Backbone.DispatchEvent(retVal)
         Return retVal
      End Function
      Public Sub DispatchEndEvent(properties As Dictionary(Of String, Object))
         Dim e As New EventArgs(Sender, properties, ServiceDocUri, RepositoryId, EventIdentifier,
                                "End" & If(EventName.StartsWith("Begin", StringComparison.InvariantCultureIgnoreCase),
                                           EventName.Substring("Begin".Length), EventName),
                                EventParameters)
         Backbone.DispatchEvent(e)
      End Sub
      ''' <summary>
      ''' Dispatches a non built-in event
      ''' </summary>
      Public Shared Sub DispatchEvent(sender As Object, properties As Dictionary(Of String, Object),
                                      serviceDocUri As String, repositoryId As String, eventName As String,
                                      ParamArray eventParameters As String())
         Backbone.DispatchEvent(New EventArgs(sender, properties, serviceDocUri, repositoryId, System.Guid.NewGuid.ToString("N"), eventName, eventParameters))
      End Sub
#End Region

      Private _endEventName As String
      Public ReadOnly EventIdentifier As String
      Public ReadOnly EventName As String
      Public ReadOnly EventParameters As String()

      ''' <summary>
      ''' Predefined Property Failure (hosted in _properties)
      ''' </summary>
      Public ReadOnly Property Failure As System.ServiceModel.FaultException
         Get
            Return TryCast([Property](PredefinedPropertyNames.Failure), System.ServiceModel.FaultException)
         End Get
      End Property

      ''' <summary>
      ''' Predefined Property NewObjectId (hosted in _properties)
      ''' </summary>
      Public ReadOnly Property NewObjectId As String
         Get
            Return TryCast([Property](PredefinedPropertyNames.NewObjectId), String)
         End Get
      End Property

      Private _properties As Dictionary(Of String, Object)
      Public ReadOnly Property Properties As KeyValuePair(Of String, Object)()
         Get
            Return _properties.ToArray()
         End Get
      End Property

      Public ReadOnly Property [Property](propertyName As String) As Object
         Get
            Dim retVal As Object = Nothing
            Try
               If _properties IsNot Nothing Then _properties.TryGetValue(propertyName, retVal)
            Catch
            End Try
            Return retVal
         End Get
      End Property

      Public ReadOnly RepositoryId As String
      Public ReadOnly Sender As Object
      Public ReadOnly ServiceDocUri As String

      ''' <summary>
      ''' Predefined Property Succeeded (hosted in _properties)
      ''' </summary>
      Public ReadOnly Property Succeeded As Boolean
         Get
            Dim retVal As Object = Nothing
            If _properties Is Nothing OrElse Not _properties.TryGetValue(PredefinedPropertyNames.Succeeded, retVal) Then Return True
            Try
               Return CBool(retVal)
            Catch
               Return True
            End Try
         End Get
      End Property

   End Class
End Namespace