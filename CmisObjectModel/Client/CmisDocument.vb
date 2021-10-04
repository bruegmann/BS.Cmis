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
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#If Not xs_HttpRequestAddRange64 Then
#Const HttpRequestAddRangeShortened = True
#End If
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Simplifies requests to cmis document
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisDocument
      Inherits CmisObject

#Region "Constructors"
      Public Sub New(cmisObject As Core.cmisObjectType,
                     client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         MyBase.New(cmisObject, client, repositoryInfo)
      End Sub
#End Region

#Region "Predefined properties"
      Public Overridable Property CheckinComment As ccg.Nullable(Of String)
         Get
            Return _cmisObject.CheckinComment
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.CheckinComment = value
         End Set
      End Property

      Public Overridable Property ContentStreamFileName As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ContentStreamFileName
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ContentStreamFileName = value
         End Set
      End Property

      Public Overridable Property ContentStreamId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ContentStreamId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ContentStreamId = value
         End Set
      End Property

      Public Overridable Property ContentStreamLength As xs_Integer?
         Get
            Return _cmisObject.ContentStreamLength
         End Get
         Set(value As xs_Integer?)
            _cmisObject.ContentStreamLength = value
         End Set
      End Property

      Public Overridable Property ContentStreamMimeType As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ContentStreamMimeType
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ContentStreamMimeType = value
         End Set
      End Property

      Public Overridable Property IsImmutable As Boolean?
         Get
            Return _cmisObject.IsImmutable
         End Get
         Set(value As Boolean?)
            _cmisObject.IsImmutable = value
         End Set
      End Property

      Public Overridable Property IsLatestMajorVersion As Boolean?
         Get
            Return _cmisObject.IsLatestMajorVersion
         End Get
         Set(value As Boolean?)
            _cmisObject.IsLatestMajorVersion = value
         End Set
      End Property

      Public Overridable Property IsLatestVersion As Boolean?
         Get
            Return _cmisObject.IsLatestVersion
         End Get
         Set(value As Boolean?)
            _cmisObject.IsLatestVersion = value
         End Set
      End Property

      Public Overridable Property IsMajorVersion As Boolean?
         Get
            Return _cmisObject.IsMajorVersion
         End Get
         Set(value As Boolean?)
            _cmisObject.IsMajorVersion = value
         End Set
      End Property

      Public Overridable ReadOnly Property IsPrivateWorkingCopy As Boolean?
         Get
            If _repositoryInfo IsNot Nothing AndAlso _repositoryInfo.CmisVersionSupported = "1.0" Then
               'the version 1.0 doesn't support cmis:isPrivateWorkingCopy-property
               With _cmisObject
                  If .ObjectId.HasValue AndAlso .VersionSeriesCheckedOutId.HasValue Then
                     Return _cmisObject.ObjectId.Value = _cmisObject.VersionSeriesCheckedOutId.Value
                  Else
                     Return Nothing
                  End If
               End With
            Else
               Return _cmisObject.IsPrivateWorkingCopy
            End If
         End Get
      End Property

      Public Overridable Property IsVersionSeriesCheckedOut As Boolean?
         Get
            Return _cmisObject.IsVersionSeriesCheckedOut
         End Get
         Set(value As Boolean?)
            _cmisObject.IsVersionSeriesCheckedOut = value
         End Set
      End Property

      Public Overridable Property VersionLabel As ccg.Nullable(Of String)
         Get
            Return _cmisObject.VersionLabel
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.VersionLabel = value
         End Set
      End Property

      Public Overridable Property VersionSeriesCheckedOutBy As ccg.Nullable(Of String)
         Get
            Return _cmisObject.VersionSeriesCheckedOutBy
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.VersionSeriesCheckedOutBy = value
         End Set
      End Property

      Public Overridable Property VersionSeriesCheckedOutId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.VersionSeriesCheckedOutId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.VersionSeriesCheckedOutId = value
         End Set
      End Property

      Public Overridable Property VersionSeriesId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.VersionSeriesId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.VersionSeriesId = value
         End Set
      End Property
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the current document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function AppendContentStream(contentStream As cm.cmisContentStreamType,
                                          Optional isLastChunk As Boolean = False) As Boolean
         With _client.AppendContentStream(New cmr.appendContentStream() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                              .ContentStream = contentStream, .IsLastChunk = isLastChunk,
                                                                              .ChangeToken = _cmisObject.ChangeToken})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Dim objectId As String = .Response.ObjectId
               Dim changeToken As String = .Response.ChangeToken

               If Not String.IsNullOrEmpty(objectId) Then _cmisObject.ObjectId = objectId
               If Not String.IsNullOrEmpty(changeToken) Then _cmisObject.ChangeToken = changeToken
               Return True
            Else
               Return False
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the content stream for the specified document object
      ''' </summary>
      Public Function DeleteContentStream(Optional changeToken As String = Nothing) As cm.Responses.deleteContentStreamResponse
         With _client.DeleteContentStream(New cmr.deleteContentStream() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                              .ObjectId = _cmisObject.ObjectId,
                                                                              .ChangeToken = changeToken})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the current object
      ''' </summary>
      Public Overrides Function DeleteObject(Optional allVersions As Boolean = True) As Boolean
         Try
            _onPWCRemovedPaused = True
            Return MyBase.DeleteObject(allVersions)
         Finally
            _onPWCRemovedPaused = False
         End Try
      End Function

      ''' <summary>
      ''' Gets the content stream for the current document object, or gets a rendition stream for a specified rendition of the current document
      ''' </summary>
      ''' <param name="streamId"></param>
      ''' <param name="offset"></param>
      ''' <param name="length"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
#If HttpRequestAddRangeShortened Then
      Public Shadows Function GetContentStream(Optional streamId As String = Nothing,
                                               Optional offset As Integer? = Nothing,
                                               Optional length As Integer? = Nothing) As Messaging.cmisContentStreamType
#Else
      Public Shadows Function GetContentStream(Optional streamId As String = Nothing,
                                               Optional offset As xs_Integer? = Nothing,
                                               Optional length As xs_Integer? = Nothing) As Messaging.cmisContentStreamType
#End If
         Return MyBase.GetContentStream(streamId, offset, length)
      End Function

      ''' <summary>
      ''' Sets the content stream for the current document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function SetContentStream(contentStream As cm.cmisContentStreamType,
                                       Optional overwriteFlag As Boolean = True) As Boolean
         With _client.SetContentStream(New cmr.setContentStream() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                        .ContentStream = contentStream, .OverwriteFlag = overwriteFlag,
                                                                        .ChangeToken = _cmisObject.ChangeToken})
            _lastException = .Exception
            If _lastException Is Nothing Then
               If .Response IsNot Nothing Then
                  Dim objectId As String = .Response.ObjectId
                  Dim changeToken As String = .Response.ChangeToken

                  If Not String.IsNullOrEmpty(objectId) Then _cmisObject.ObjectId = objectId
                  If Not String.IsNullOrEmpty(changeToken) Then _cmisObject.ChangeToken = changeToken
               End If

               'get last modification info from server
               Dim filter As String = String.Join(",", CmisPredefinedPropertyNames.LastModificationDate, CmisPredefinedPropertyNames.LastModifiedBy)
               Dim cmisObject As CmisObject = GetObject(_cmisObject.ObjectId, filter)
               If cmisObject IsNot Nothing Then
                  _cmisObject.LastModificationDate = cmisObject.LastModificationDate
                  _cmisObject.LastModifiedBy = cmisObject.LastModifiedBy
               End If

               Return True
            Else
               Return False
            End If
         End With
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CancelCheckOut(Optional pwcRequired As Boolean? = Nothing) As Boolean
         Dim objectOfLatestVersion As CmisDocument = GetObjectOfLatestVersion(filter:=String.Join(",", CmisPredefinedPropertyNames.Description, CmisPredefinedPropertyNames.ObjectId))
         Dim cancelCheckOutFallbackId As String = Me.CancelCheckOutFallbackId

         Try
            _onPWCRemovedPaused = True
            With _client.CancelCheckOut(New cmr.cancelCheckOut() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                       .ObjectId = _cmisObject.ObjectId,
                                                                       .PWCLinkRequired = If(pwcRequired.HasValue, pwcRequired.Value,
                                                                                             Not (IsPrivateWorkingCopy.HasValue AndAlso IsPrivateWorkingCopy.Value))})
               _lastException = .Exception
               If _lastException Is Nothing Then
                  Dim isAdded As Boolean = String.IsNullOrEmpty(CancelCheckOutFallbackId)
                  Dim cmisObject As CmisObject = If(isAdded, Nothing, GetObject(CancelCheckOutFallbackId))
                  If TypeOf cmisObject Is CmisDocument Then
                     _cmisObject = cmisObject.Object
                     Me.CancelCheckOutFallbackId = Nothing
                     Return True
                  End If
               End If
               Return False
            End With
         Finally
            _onPWCRemovedPaused = False
            'remove complete versionseries if the CancelCheckOut belongs to a document created with versioningState checkedout
            If objectOfLatestVersion IsNot Nothing AndAlso
               If(objectOfLatestVersion.Description.Value, String.Empty).StartsWith(VersioningStateCheckedOutPrefix) Then
               objectOfLatestVersion.DeleteObject(True)
            End If
         End Try
      End Function

      ''' <summary>
      ''' Checks-in the Private Working Copy document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CheckIn(Optional major As Boolean = True,
                              Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                              Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                              Optional checkinComment As String = Nothing,
                              Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                              Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                              Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing,
                              Optional pwcRequired As Boolean? = Nothing) As Boolean
         Try
            _onPWCRemovedPaused = True
            With _client.CheckIn(New cmr.checkIn() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                         .ObjectId = _cmisObject.ObjectId,
                                                         .Major = major, .Properties = properties, .ContentStream = contentStream,
                                                         .CheckinComment = checkinComment, .Policies = policies, .AddACEs = addACEs,
                                                         .RemoveACEs = removeACEs,
                                                         .PWCLinkRequired = If(pwcRequired.HasValue, pwcRequired.Value,
                                                                               Not (IsPrivateWorkingCopy.HasValue AndAlso IsPrivateWorkingCopy.Value))})
               _lastException = .Exception
               If _lastException Is Nothing Then
                  Dim cmisObject As CmisObject = GetObject(.Response.ObjectId)
                  If TypeOf cmisObject Is CmisDocument Then
                     _cmisObject = cmisObject.Object
                     CancelCheckOutFallbackId = Nothing
                     Return True
                  End If
               End If
               Return False
            End With
         Finally
            _onPWCRemovedPaused = False
         End Try
      End Function

      ''' <summary>
      ''' Create a private working copy (PWC) of the document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CheckOut() As Boolean
         Dim retVal As Boolean = False

         Try
            If IsPrivateWorkingCopy.HasValue AndAlso IsPrivateWorkingCopy.Value Then
               If String.IsNullOrEmpty(CancelCheckOutFallbackId) Then
                  Dim cmisObject As CmisObject = GetObjectOfLatestVersion()
                  If TypeOf cmisObject Is CmisDocument Then CancelCheckOutFallbackId = cmisObject.ObjectId
               End If
               'already checked out
               retVal = True
            Else
               With _client.CheckOut(New cmr.checkOut() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId})
                  _lastException = .Exception
                  If _lastException Is Nothing Then
                     Dim cmisObject As CmisObject = GetObject(.Response.ObjectId)
                     If TypeOf cmisObject Is CmisDocument Then
                        CancelCheckOutFallbackId = _cmisObject.ObjectId
                        _cmisObject = cmisObject.Object
                        retVal = True
                     End If
                  End If
               End With
            End If
         Finally
            'this is a pwc; we have to install listeners, because another CmisDocument-instance or a low-level client request within this
            'application might execute a checkIn or cancelCheckout
            If retVal Then AddPWCRemovedListeners()
         End Try

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the list of all document objects in the specified version series, sorted by cmis:creationDate descending
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>If a Private Working Copy exists for the version series and the caller has permissions to access it,
      ''' then it MUST be returned as the first object in the result list</remarks>
      Public Function GetAllVersions(Optional filter As String = Nothing,
                                     Optional includeAllowableActions As Boolean? = Nothing) As CmisDocument()
         With _client.GetAllVersions(New cmr.getAllVersions() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                    .Filter = filter, .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return (From [object] As Core.cmisObjectType In .Response.Objects
                       Let cmisObject As CmisObject = CreateCmisObject([object])
                       Where TypeOf cmisObject Is CmisDocument
                       Select CType(cmisObject, CmisDocument)).ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetObjectOfLatestVersion(Optional major As Boolean = False,
                                                       Optional filter As String = Nothing,
                                                       Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                       Optional includePolicyIds As Boolean? = Nothing,
                                                       Optional renditionFilter As String = Nothing,
                                                       Optional includeACL As Boolean? = Nothing,
                                                       Optional includeAllowableActions As Boolean? = Nothing,
                                                       Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.notCheckedOut) As CmisDocument
         Dim versionSeriesId As ccg.Nullable(Of String) = _cmisObject.VersionSeriesId
         'returns the private working copy if exists and is accepted, otherwise returns ObjectOfLatestVersion
         Return MyBase.GetObjectOfLatestVersion(CType(If(versionSeriesId.HasValue, versionSeriesId, _cmisObject.ObjectId), String),
                                                major, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, acceptPWC)
      End Function

      ''' <summary>
      ''' Get the private working copy if the current document is checked out by current user (acceptPWC=enumCheckedOutState.checkedOutByMe)
      ''' or by any user (acceptPWC=enumCheckedOutState.checkedOut)
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetPrivateWorkingCopy(Optional filter As String = Nothing,
                                                    Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                    Optional includePolicyIds As Boolean? = Nothing,
                                                    Optional renditionFilter As String = Nothing,
                                                    Optional includeACL As Boolean? = Nothing,
                                                    Optional includeAllowableActions As Boolean? = Nothing,
                                                    Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.checkedOutByMe) As CmisDocument
         Return MyBase.GetPrivateWorkingCopy(_cmisObject.ObjectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, acceptPWC)
      End Function

      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetPropertiesOfLatestVersion(Optional major As Boolean = False,
                                                   Optional filter As String = Nothing,
                                                   Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.notCheckedOut) As Core.Collections.cmisPropertiesType
         'check for private working copy
         If acceptPWC <> enumCheckedOutState.notCheckedOut Then
            Dim pwc = MyBase.GetPrivateWorkingCopy(_cmisObject.ObjectId, filter, acceptPWC:=acceptPWC)
            If pwc IsNot Nothing Then Return pwc.Object.Properties
         End If

         'default: PropertiesOfLatestVersion
         Dim versionSeriesId As ccg.Nullable(Of String) = _cmisObject.VersionSeriesId
         With _client.GetPropertiesOfLatestVersion(New cmr.getPropertiesOfLatestVersion() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                                                .ObjectId = CType(If(versionSeriesId.HasValue, versionSeriesId, _cmisObject.ObjectId), String),
                                                                                                .Major = major, .Filter = filter})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response.Properties
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "AllowableActions"
      Public Property CanCheckIn As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanCheckIn
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanCheckIn = value
         End Set
      End Property

      Public Property CanCheckOut As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanCheckOut
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanCheckOut = value
         End Set
      End Property

      Public Property CanCancelCheckOut As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanCancelCheckOut
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanCancelCheckOut = value
         End Set
      End Property

      Public Property CanGetContentStream As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanGetContentStream
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanGetContentStream = value
         End Set
      End Property

      Public Property CanSetContentStream As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanSetContentStream
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanSetContentStream = value
         End Set
      End Property

      Public Property CanGetAllVersions As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanGetAllVersions
                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanGetAllVersions = value
         End Set
      End Property
#End Region

      ''' <summary>
      ''' Bind to PWCRemovedListeners
      ''' </summary>
      Friend Sub AddPWCRemovedListeners()
         _onPWCRemoved.AddPWCRemovedListeners(_onPWCRemovedListeners, _client.ServiceDocUri.AbsoluteUri,
                                              _repositoryInfo.RepositoryId, _cmisObject.ObjectId)
      End Sub

      Friend CancelCheckOutFallbackId As String

      Private Function Combine(propertiesCollections As Dictionary(Of String, Core.Properties.cmisProperty)(), propertyName As String,
                               addIds As String(), removeIds As String()) As Object()
         Dim properties As Core.Properties.cmisProperty() = (From propertyCollection As Dictionary(Of String, Core.Properties.cmisProperty) In propertiesCollections
                                                             Where propertyCollection.ContainsKey(propertyName)
                                                             Let cmisProperty As Core.Properties.cmisProperty = propertyCollection(propertyName)
                                                             Select cmisProperty).ToArray()
         Return Combine(properties, addIds, removeIds)
      End Function
      Private Function Combine(properties As Core.Properties.cmisProperty(), addIds As String(), removeIds As String()) As Object()
         Dim verify As New HashSet(Of Object)
         Dim result As New List(Of Object)

         'mark ids for remove operation
         If removeIds IsNot Nothing Then
            For index As Integer = 0 To removeIds.Length - 1
               verify.Add(removeIds(index))
            Next
         End If
         'combine the values of given properties
         If properties IsNot Nothing Then
            For propertyIndex As Integer = 0 To properties.Length - 1
               Dim cmisProperty As Core.Properties.cmisProperty = properties(propertyIndex)
               Dim values As Object() = If(cmisProperty Is Nothing, Nothing, cmisProperty.Values)

               If values IsNot Nothing Then
                  For index As Integer = 0 To values.Length - 1
                     Dim value As Object = values(index)

                     If value IsNot Nothing AndAlso verify.Add(value) Then result.Add(value)
                  Next
               End If
            Next
         End If
         'add new identifiers
         If addIds IsNot Nothing Then
            For index As Integer = 0 To addIds.Length - 1
               Dim addId As String = addIds(index)
               If Not String.IsNullOrEmpty(addId) AndAlso verify.Add(addId) Then result.Add(addId)
            Next
         End If

         Return result.ToArray()
      End Function

      ''' <summary>
      ''' Returns a tristate logic: notCheckOut, checkedOut and checkedOutByMe
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetCheckedOut() As Common.enumCheckedOutState
         Return MyBase.GetCheckedOut(_cmisObject, _client)
      End Function
      ''' <summary>
      ''' Returns a tristate logic: notCheckOut, checkedOut and checkedOutByMe
      ''' </summary>
      ''' <param name="cmisObject"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetCheckedOut(cmisObject As Core.cmisObjectType) As Common.enumCheckedOutState
         Return MyBase.GetCheckedOut(cmisObject, _client)
      End Function

      Private _onPWCRemoved As EventBus.WeakListenerCallback = AddressOf OnPWCRemoved
      ''' <summary>
      ''' Reverts the checkedout-state after the pwc has been cancelled, checked in or deleted
      ''' </summary>
      Protected Function OnPWCRemoved(e As EventBus.EventArgs) As EventBus.enumEventBusListenerResult
         If Not _onPWCRemovedPaused Then
            If String.Equals(e.EventName, EventBus.EndCheckIn) Then
               Dim cmisObject As CmisObject = GetObject(e.NewObjectId)
               If TypeOf cmisObject Is CmisDocument Then
                  _cmisObject = cmisObject.Object
               End If
            ElseIf String.IsNullOrEmpty(CancelCheckOutFallbackId) Then
               Dim cmisObject As CmisObject = GetObjectOfLatestVersion(acceptPWC:=enumCheckedOutState.checkedOutByMe)
               If TypeOf cmisObject Is CmisDocument Then
                  _cmisObject = cmisObject.Object
               End If
            Else
               Dim cmisObject As CmisObject = GetObject(CancelCheckOutFallbackId)
               If TypeOf cmisObject Is CmisDocument Then
                  _cmisObject = cmisObject.Object
               End If
            End If
         End If

         CancelCheckOutFallbackId = Nothing

         'remove the listeners, because the document stored in this object is not checkedout anymore
         _onPWCRemoved.RemovePWCRemovedListeners(_onPWCRemovedListeners)

         Return EventBus.enumEventBusListenerResult.success
      End Function
      Private _onPWCRemovedListeners As EventBus.WeakListener()
      Private _onPWCRemovedPaused As Boolean = False

      Public Property RM_DestructionRetention As DateTimeOffset?
         Get
            Return RM_RetentionDate(CmisPredefinedPropertyNames.RM_DestructionDate)
         End Get
         Set(value As DateTimeOffset?)
            RM_RetentionDate(Core.Definitions.Types.cmisTypeRM_DestructionRetentionDefinitionType.TargetTypeName,
                             CmisPredefinedPropertyNames.RM_DestructionDate,
                             Function(factory) factory.RM_DestructionDate(True, True)) = value
         End Set
      End Property

      Public Property RM_ExpirationDate As DateTimeOffset?
         Get
            Return RM_RetentionDate(CmisPredefinedPropertyNames.RM_ExpirationDate)
         End Get
         Set(value As DateTimeOffset?)
            RM_RetentionDate(Core.Definitions.Types.cmisTypeRM_ClientMgtRetentionDefinitionType.TargetTypeName,
                             CmisPredefinedPropertyNames.RM_ExpirationDate,
                             Function(factory) factory.RM_ExpirationDate(True)) = value
         End Set
      End Property

      ''' <summary>
      ''' Returns the hold-identifiers the current document is protected by
      ''' </summary>
      Public ReadOnly Property RM_HoldIds As String()
         Get
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = _cmisObject.GetProperties(CmisPredefinedPropertyNames.RM_HoldIds)

            If properties.Count > 0 Then
               Dim cmisProperty As Core.Properties.cmisProperty = properties(CmisPredefinedPropertyNames.RM_HoldIds)
               Dim values As Object() = cmisProperty.Values

               If values IsNot Nothing Then
                  Return (From value As Object In values
                          Let holdId As String = If(value Is Nothing, Nothing, value.ToString())
                          Where Not String.IsNullOrEmpty(holdId)
                          Select holdId).ToArray()
               End If
            End If

            Return Nothing
         End Get
      End Property


      Public ReadOnly Property RM_Preserved As enumRetentionState
         Get
            Dim filter As String = String.Join(",", CmisPredefinedPropertyNames.BaseTypeId,
                                                    CmisPredefinedPropertyNames.ObjectId, CmisPredefinedPropertyNames.ObjectTypeId,
                                                    CmisPredefinedPropertyNames.RM_ExpirationDate, CmisPredefinedPropertyNames.RM_HoldIds,
                                                    CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
            Dim documentOfLatestVersion As CmisDocument = GetObjectOfLatestVersion(filter:=filter, acceptPWC:=enumCheckedOutState.checkedOutByMe)
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) =
               documentOfLatestVersion._cmisObject.GetProperties(CmisPredefinedPropertyNames.RM_DestructionDate, CmisPredefinedPropertyNames.RM_ExpirationDate,
                                                                 CmisPredefinedPropertyNames.RM_HoldIds, CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
            Dim retVal As enumRetentionState = enumRetentionState.none

            'ExpirationDate
            If properties.ContainsKey(CmisPredefinedPropertyNames.RM_ExpirationDate) Then
               Dim dateTimeProperty As Core.Properties.cmisPropertyDateTime = TryCast(properties(CmisPredefinedPropertyNames.RM_ExpirationDate), Core.Properties.cmisPropertyDateTime)
               If dateTimeProperty IsNot Nothing AndAlso dateTimeProperty.Value.DateTime > DateTime.Now Then
                  retVal = retVal Or enumRetentionState.preservedByExpirationDate
               End If
            End If
            'HoldIds
            If properties.ContainsKey(CmisPredefinedPropertyNames.RM_HoldIds) AndAlso
               properties(CmisPredefinedPropertyNames.RM_HoldIds).Value IsNot Nothing Then
               retVal = retVal Or enumRetentionState.preservedByClientHoldIds
            End If
            'preserved by repository
            If properties.ContainsKey(CmisPredefinedPropertyNames.SecondaryObjectTypeIds) Then
               Dim values As Object() = properties(CmisPredefinedPropertyNames.SecondaryObjectTypeIds).Values

               If values IsNot Nothing Then
                  Dim secondaryObjectTypeIds As New HashSet(Of Object)

                  For Each value As Object In values
                     secondaryObjectTypeIds.Add(value)
                  Next
                  If secondaryObjectTypeIds.Add(Core.Definitions.Types.cmisTypeRM_RepMgtRetentionDefinitionType.TargetTypeName) Then
                     'second chance: check for derived types
                     Try
                        Dim cmisTypes As List(Of CmisType) =
                           CmisObjectModel.Client.Generic.ItemContainer(Of CmisType).GetAllItems(GetTypeDescendants(Core.Definitions.Types.cmisTypeRM_RepMgtRetentionDefinitionType.TargetTypeName, -1, False))
                        For index As Integer = 0 To cmisTypes.Count - 1
                           'retention managed by repository detected
                           If Not secondaryObjectTypeIds.Add(cmisTypes(index).Type.Id) Then
                              retVal = retVal Or enumRetentionState.preservedByRepository
                              Exit Try
                           End If
                        Next
                     Catch
                     End Try
                  Else
                     retVal = retVal Or enumRetentionState.preservedByRepository
                  End If
               End If
            End If

            Return retVal
         End Get
      End Property

      Private ReadOnly Property RM_RetentionDate(propertyDefinitionId As String) As DateTimeOffset?
         Get
            If (RetentionCapability And enumRetentionCapability.clientMgt) = enumRetentionCapability.clientMgt Then
               Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = _cmisObject.GetProperties(propertyDefinitionId)
               If properties.Count > 0 Then
                  Dim dateTimeProperty As Core.Properties.cmisPropertyDateTime = TryCast(properties(propertyDefinitionId), Core.Properties.cmisPropertyDateTime)
                  If dateTimeProperty IsNot Nothing Then Return dateTimeProperty.Value
               End If
            End If

            Return Nothing
         End Get
      End Property
      Private WriteOnly Property RM_RetentionDate(secondaryObjectTypeId As String, propertyDefinitionId As String,
                                                  factory As Func(Of PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType)) As DateTimeOffset?
         Set(value As DateTimeOffset?)
            If (RetentionCapability And enumRetentionCapability.clientMgt) = enumRetentionCapability.clientMgt Then
               If value.HasValue Then
                  Dim retentionDate As DateTimeOffset = value.Value
                  Dim filter As String = String.Join(",", propertyDefinitionId, CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
                  Dim documentOfLatestVersion As CmisDocument = GetObjectOfLatestVersion(filter:=filter, acceptPWC:=enumCheckedOutState.checkedOutByMe)
                  Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = _cmisObject.GetProperties(propertyDefinitionId,
                                                                                                                    CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
                  Dim propertiesOfLatestVersion As Dictionary(Of String, Core.Properties.cmisProperty) =
                     If(documentOfLatestVersion, Me)._cmisObject.GetProperties(propertyDefinitionId, CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
                  Dim propertiesCollections As Dictionary(Of String, Core.Properties.cmisProperty)() =
                     New Dictionary(Of String, Core.Properties.cmisProperty)() {properties, propertiesOfLatestVersion}
                  Dim secondaryObjectTypeIdsValues As Object() = Combine(propertiesCollections, CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                                                         New String() {secondaryObjectTypeId}, Nothing)
                  'update retention-date and secondaryObjectTypeIds
                  With New Common.PredefinedPropertyDefinitionFactory(Nothing)
                     If properties.ContainsKey(propertyDefinitionId) Then
                        properties(propertyDefinitionId).Value = retentionDate
                     ElseIf _cmisObject.Properties Is Nothing Then
                        _cmisObject.Properties = New Core.Collections.cmisPropertiesType(factory(.Self).CreateProperty(retentionDate))
                     Else
                        _cmisObject.Properties.Append(factory(.Self).CreateProperty(retentionDate))
                     End If
                     If properties.ContainsKey(CmisPredefinedPropertyNames.SecondaryObjectTypeIds) Then
                        properties(CmisPredefinedPropertyNames.SecondaryObjectTypeIds).Values = secondaryObjectTypeIdsValues
                     Else
                        _cmisObject.Properties.Append(.SecondaryObjectTypeIds.CreateProperty(secondaryObjectTypeIdsValues))
                     End If
                  End With
               ElseIf _cmisObject.Properties IsNot Nothing Then
                  _cmisObject.Properties.RemoveProperty(propertyDefinitionId)
               End If
            End If
         End Set
      End Property

      Public Property RM_StartOfRetention As DateTimeOffset?
         Get
            Return RM_RetentionDate(CmisPredefinedPropertyNames.RM_StartOfRetention)
         End Get
         Set(value As DateTimeOffset?)
            RM_RetentionDate(Core.Definitions.Types.cmisTypeRM_ClientMgtRetentionDefinitionType.TargetTypeName,
                             CmisPredefinedPropertyNames.RM_StartOfRetention,
                             Function(factory) factory.RM_StartOfRetention(True, True)) = value
         End Set
      End Property

      Private Function ToArray(Of T)(ParamArray result As T()) As T()
         Return result
      End Function

      ''' <summary>
      ''' Modifies the current cmis:rm_holdIds property in the following manner:
      ''' 1. step: merges the the rm_holdIds of this instance with the values currently found in the repository
      ''' 2. step: adds the identifiers given in addHoldIds
      ''' 3. step: removes the identifiers given in removeHoldIds
      ''' </summary>
      ''' <param name="addHoldIds"></param>
      ''' <param name="removeHoldIds"></param>
      Public Sub UpdateRM_HoldIds(addHoldIds As String(), removeHoldIds As String())
         If HoldCapability Then
            Dim filter As String = String.Join(",", CmisPredefinedPropertyNames.RM_HoldIds, CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
            Dim documentOfLatestVersion As CmisDocument = GetObjectOfLatestVersion(filter:=filter, acceptPWC:=enumCheckedOutState.checkedOutByMe)
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = _cmisObject.GetProperties(CmisPredefinedPropertyNames.RM_HoldIds,
                                                                                                              CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
            Dim propertiesOfLatestVersion As Dictionary(Of String, Core.Properties.cmisProperty) =
               If(documentOfLatestVersion, Me)._cmisObject.GetProperties(CmisPredefinedPropertyNames.RM_HoldIds, CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
            Dim propertiesCollections As Dictionary(Of String, Core.Properties.cmisProperty)() = ToArray(properties, propertiesOfLatestVersion)
            Dim holdIdsValues As Object() = Combine(propertiesCollections, CmisPredefinedPropertyNames.RM_HoldIds, addHoldIds, removeHoldIds)
            Dim secondaryObjectTypeIdsValues As Object() = Combine(propertiesCollections, CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                                                   ToArray(Core.Definitions.Types.cmisTypeRM_HoldDefinitionType.TargetTypeName), Nothing)
            'update holdIds and secondaryObjectTypeIds
            With New Common.PredefinedPropertyDefinitionFactory(Nothing)
               If properties.ContainsKey(CmisPredefinedPropertyNames.RM_HoldIds) Then
                  properties(CmisPredefinedPropertyNames.RM_HoldIds).Values = holdIdsValues
               ElseIf _cmisObject.Properties Is Nothing Then
                  _cmisObject.Properties = New Core.Collections.cmisPropertiesType(.RM_HoldIds.CreateProperty(holdIdsValues))
               Else
                  _cmisObject.Properties.Append(.RM_HoldIds.CreateProperty(holdIdsValues))
               End If
               If properties.ContainsKey(CmisPredefinedPropertyNames.SecondaryObjectTypeIds) Then
                  properties(CmisPredefinedPropertyNames.SecondaryObjectTypeIds).Values = secondaryObjectTypeIdsValues
               Else
                  _cmisObject.Properties.Append(.SecondaryObjectTypeIds.CreateProperty(secondaryObjectTypeIdsValues))
               End If
            End With
         End If
      End Sub

   End Class
End Namespace