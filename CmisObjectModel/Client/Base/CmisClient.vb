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
Imports ca = CmisObjectModel.AtomPub
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cmr = CmisObjectModel.Messaging.Responses
Imports sn = System.Net
Imports sri = System.Runtime.InteropServices
Imports ssw = System.ServiceModel.Web
Imports sss = System.ServiceModel.Syndication
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

Namespace CmisObjectModel.Client.Base
   Public MustInherit Class CmisClient
      Implements Contracts.ICmisClient

#Region "Constructors"
      Protected Sub New(serviceDocUri As Uri, vendor As enumVendor,
                        authentication As AuthenticationProvider,
                        Optional connectTimeout As Integer? = Nothing,
                        Optional readWriteTimeout As Integer? = Nothing)
         _serviceDocUri = serviceDocUri
         Select Case vendor
            Case enumVendor.Alfresco
               _vendor = New Vendors.Alfresco(Me)
            Case enumVendor.Agorum
               _vendor = New Vendors.Agorum(Me)
            Case Else
               _vendor = New Vendors.Vendor(Me)
         End Select
         _authentication = authentication
         _connectTimeout = If(connectTimeout.HasValue AndAlso connectTimeout.Value >= -1, connectTimeout, Nothing)
         _readWriteTimeout = If(readWriteTimeout.HasValue AndAlso readWriteTimeout.Value >= -1, readWriteTimeout, Nothing)
      End Sub
#End Region

#Region "IValueMappingSupport"
      Protected _valueMapper As New Data.MapperDictionary
      ''' <summary>
      ''' Adds mapping information to the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="mapper"></param>
      ''' <remarks></remarks>
      Public Sub AddMapper(repositoryId As String, mapper As Data.Mapper) Implements Contracts.ICmisClient.AddMapper
         _valueMapper.AddMapper(repositoryId, mapper)
      End Sub

      ''' <summary>
      ''' Removes mapping information from the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Sub RemoveMapper(repositoryId As String) Implements Contracts.ICmisClient.RemoveMapper
         _valueMapper.RemoveMapper(repositoryId)
      End Sub
#End Region

#Region "Repository"
      Public MustOverride Function CreateType(request As Messaging.Requests.createType) As Client.Generic.Response(Of Messaging.Responses.createTypeResponse) Implements Contracts.ICmisClient.CreateType
      Public MustOverride Function DeleteType(request As Messaging.Requests.deleteType) As Client.Generic.Response(Of Messaging.Responses.deleteTypeResponse) Implements Contracts.ICmisClient.DeleteType
      Public MustOverride Function GetRepositories(request As Messaging.Requests.getRepositories) As Client.Generic.Response(Of Messaging.Responses.getRepositoriesResponse) Implements Contracts.ICmisClient.GetRepositories
      Public MustOverride Function GetRepositoryInfo(request As Messaging.Requests.getRepositoryInfo, Optional ignoreCache As Boolean = False) As Client.Generic.Response(Of Messaging.Responses.getRepositoryInfoResponse) Implements Contracts.ICmisClient.GetRepositoryInfo
      Public MustOverride Function GetTypeChildren(request As Messaging.Requests.getTypeChildren) As Client.Generic.Response(Of Messaging.Responses.getTypeChildrenResponse) Implements Contracts.ICmisClient.GetTypeChildren
      Public MustOverride Function GetTypeDefinition(request As Messaging.Requests.getTypeDefinition) As Client.Generic.Response(Of Messaging.Responses.getTypeDefinitionResponse) Implements Contracts.ICmisClient.GetTypeDefinition
      Public MustOverride Function GetTypeDescendants(request As Messaging.Requests.getTypeDescendants) As Client.Generic.Response(Of Messaging.Responses.getTypeDescendantsResponse) Implements Contracts.ICmisClient.GetTypeDescendants
      Public MustOverride Function UpdateType(request As Messaging.Requests.updateType) As Client.Generic.Response(Of Messaging.Responses.updateTypeResponse) Implements Contracts.ICmisClient.UpdateType
#End Region

#Region "Navigation"
      Public MustOverride Function GetCheckedOutDocs(request As Messaging.Requests.getCheckedOutDocs) As Client.Generic.Response(Of Messaging.Responses.getCheckedOutDocsResponse) Implements Contracts.ICmisClient.GetCheckedOutDocs
      Public MustOverride Function GetChildren(request As Messaging.Requests.getChildren) As Client.Generic.Response(Of Messaging.Responses.getChildrenResponse) Implements Contracts.ICmisClient.GetChildren
      Public MustOverride Function GetDescendants(request As Messaging.Requests.getDescendants) As Client.Generic.Response(Of Messaging.Responses.getDescendantsResponse) Implements Contracts.ICmisClient.GetDescendants
      Public MustOverride Function GetFolderParent(request As Messaging.Requests.getFolderParent) As Client.Generic.Response(Of Messaging.Responses.getFolderParentResponse) Implements Contracts.ICmisClient.GetFolderParent
      Public MustOverride Function GetFolderTree(request As Messaging.Requests.getFolderTree) As Client.Generic.Response(Of Messaging.Responses.getFolderTreeResponse) Implements Contracts.ICmisClient.GetFolderTree
      Public MustOverride Function GetObjectParents(request As Messaging.Requests.getObjectParents) As Client.Generic.Response(Of Messaging.Responses.getObjectParentsResponse) Implements Contracts.ICmisClient.GetObjectParents
#End Region

#Region "Object"
      Public MustOverride Function AppendContentStream(request As Messaging.Requests.appendContentStream) As Client.Generic.Response(Of Messaging.Responses.appendContentStreamResponse) Implements Contracts.ICmisClient.AppendContentStream
      Public MustOverride Function BulkUpdateProperties(request As Messaging.Requests.bulkUpdateProperties) As Client.Generic.Response(Of Messaging.Responses.bulkUpdatePropertiesResponse) Implements Contracts.ICmisClient.BulkUpdateProperties
      Public MustOverride Function CreateDocument(request As Messaging.Requests.createDocument) As Client.Generic.Response(Of Messaging.Responses.createDocumentResponse) Implements Contracts.ICmisClient.CreateDocument
      Public MustOverride Function CreateDocumentFromSource(request As Messaging.Requests.createDocumentFromSource) As Client.Generic.Response(Of Messaging.Responses.createDocumentFromSourceResponse) Implements Contracts.ICmisClient.CreateDocumentFromSource
      Public MustOverride Function CreateFolder(request As Messaging.Requests.createFolder) As Client.Generic.Response(Of Messaging.Responses.createFolderResponse) Implements Contracts.ICmisClient.CreateFolder
      Public MustOverride Function CreateItem(request As Messaging.Requests.createItem) As Client.Generic.Response(Of Messaging.Responses.createItemResponse) Implements Contracts.ICmisClient.CreateItem
      Public MustOverride Function CreatePolicy(request As Messaging.Requests.createPolicy) As Client.Generic.Response(Of Messaging.Responses.createPolicyResponse) Implements Contracts.ICmisClient.CreatePolicy
      Public MustOverride Function CreateRelationship(request As Messaging.Requests.createRelationship) As Client.Generic.Response(Of Messaging.Responses.createRelationshipResponse) Implements Contracts.ICmisClient.CreateRelationship
      Public MustOverride Function DeleteContentStream(request As Messaging.Requests.deleteContentStream) As Client.Generic.Response(Of Messaging.Responses.deleteContentStreamResponse) Implements Contracts.ICmisClient.DeleteContentStream
      Public MustOverride Function DeleteObject(request As Messaging.Requests.deleteObject) As Client.Generic.Response(Of Messaging.Responses.deleteObjectResponse) Implements Contracts.ICmisClient.DeleteObject
      Public MustOverride Function DeleteTree(request As Messaging.Requests.deleteTree) As Client.Generic.Response(Of Messaging.Responses.deleteTreeResponse) Implements Contracts.ICmisClient.DeleteTree
      Public MustOverride Function GetAllowableActions(request As Messaging.Requests.getAllowableActions) As Client.Generic.Response(Of Messaging.Responses.getAllowableActionsResponse) Implements Contracts.ICmisClient.GetAllowableActions
      Public MustOverride Function GetContentStream(request As Messaging.Requests.getContentStream) As Client.Generic.Response(Of Messaging.Responses.getContentStreamResponse) Implements Contracts.ICmisClient.GetContentStream
      Public MustOverride Function GetContentStreamLink(repositoryId As String, objectId As String, Optional streamId As String = Nothing) As Client.Generic.Response(Of String) Implements Contracts.ICmisClient.GetContentStreamLink
      Public MustOverride Function GetObject(request As Messaging.Requests.getObject) As Client.Generic.Response(Of Messaging.Responses.getObjectResponse) Implements Contracts.ICmisClient.GetObject
      Public MustOverride Function GetObjectByPath(request As Messaging.Requests.getObjectByPath) As Client.Generic.Response(Of Messaging.Responses.getObjectByPathResponse) Implements Contracts.ICmisClient.GetObjectByPath
      Public MustOverride Function GetProperties(request As Messaging.Requests.getProperties) As Client.Generic.Response(Of Messaging.Responses.getPropertiesResponse) Implements Contracts.ICmisClient.GetProperties
      Public MustOverride Function GetRenditions(request As Messaging.Requests.getRenditions) As Client.Generic.Response(Of Messaging.Responses.getRenditionsResponse) Implements Contracts.ICmisClient.GetRenditions
      Public MustOverride Function MoveObject(request As Messaging.Requests.moveObject) As Client.Generic.Response(Of Messaging.Responses.moveObjectResponse) Implements Contracts.ICmisClient.MoveObject
      Public MustOverride Function SetContentStream(request As Messaging.Requests.setContentStream) As Client.Generic.Response(Of Messaging.Responses.setContentStreamResponse) Implements Contracts.ICmisClient.SetContentStream
      Public MustOverride Function UpdateProperties(request As Messaging.Requests.updateProperties) As Client.Generic.Response(Of Messaging.Responses.updatePropertiesResponse) Implements Contracts.ICmisClient.UpdateProperties
#End Region

#Region "Multi"
      Public MustOverride Function AddObjectToFolder(request As Messaging.Requests.addObjectToFolder) As Client.Generic.Response(Of Messaging.Responses.addObjectToFolderResponse) Implements Contracts.ICmisClient.AddObjectToFolder
      Public MustOverride Function RemoveObjectFromFolder(request As Messaging.Requests.removeObjectFromFolder) As Client.Generic.Response(Of Messaging.Responses.removeObjectFromFolderResponse) Implements Contracts.ICmisClient.RemoveObjectFromFolder
#End Region

#Region "Discovery"
      Public MustOverride Function GetContentChanges(request As Messaging.Requests.getContentChanges) As Client.Generic.Response(Of Messaging.Responses.getContentChangesResponse) Implements Contracts.ICmisClient.GetContentChanges
      Public MustOverride Function Query(request As Messaging.Requests.query) As Client.Generic.Response(Of Messaging.Responses.queryResponse) Implements Contracts.ICmisClient.Query
#End Region

#Region "Versioning"
      Public MustOverride Function CancelCheckOut(request As Messaging.Requests.cancelCheckOut) As Client.Generic.Response(Of Messaging.Responses.cancelCheckOutResponse) Implements Contracts.ICmisClient.CancelCheckOut
      Public MustOverride Function CheckIn(request As Messaging.Requests.checkIn) As Client.Generic.Response(Of Messaging.Responses.checkInResponse) Implements Contracts.ICmisClient.CheckIn
      Public MustOverride Function CheckOut(request As Messaging.Requests.checkOut) As Client.Generic.Response(Of Messaging.Responses.checkOutResponse) Implements Contracts.ICmisClient.CheckOut
      Public MustOverride Function GetAllVersions(request As Messaging.Requests.getAllVersions) As Client.Generic.Response(Of Messaging.Responses.getAllVersionsResponse) Implements Contracts.ICmisClient.GetAllVersions
      Public MustOverride Function GetObjectOfLatestVersion(request As Messaging.Requests.getObjectOfLatestVersion) As Client.Generic.Response(Of Messaging.Responses.getObjectOfLatestVersionResponse) Implements Contracts.ICmisClient.GetObjectOfLatestVersion
      Public MustOverride Function GetPropertiesOfLatestVersion(request As Messaging.Requests.getPropertiesOfLatestVersion) As Client.Generic.Response(Of Messaging.Responses.getPropertiesOfLatestVersionResponse) Implements Contracts.ICmisClient.GetPropertiesOfLatestVersion
#End Region

#Region "Relationship"
      Public MustOverride Function GetObjectRelationships(request As Messaging.Requests.getObjectRelationships) As Client.Generic.Response(Of Messaging.Responses.getObjectRelationshipsResponse) Implements Contracts.ICmisClient.GetObjectRelationships
#End Region

#Region "Policy"
      Public MustOverride Function ApplyPolicy(request As Messaging.Requests.applyPolicy) As Client.Generic.Response(Of Messaging.Responses.applyPolicyResponse) Implements Contracts.ICmisClient.ApplyPolicy
      Public MustOverride Function GetAppliedPolicies(request As Messaging.Requests.getAppliedPolicies) As Client.Generic.Response(Of Messaging.Responses.getAppliedPoliciesResponse) Implements Contracts.ICmisClient.GetAppliedPolicies
      Public MustOverride Function RemovePolicy(request As Messaging.Requests.removePolicy) As Client.Generic.Response(Of Messaging.Responses.removePolicyResponse) Implements Contracts.ICmisClient.RemovePolicy
#End Region

#Region "Acl"
      Public MustOverride Function ApplyAcl(request As Messaging.Requests.applyACL) As Client.Generic.Response(Of Messaging.Responses.applyACLResponse) Implements Contracts.ICmisClient.ApplyAcl
      Public MustOverride Function GetAcl(request As Messaging.Requests.getACL) As Client.Generic.Response(Of Messaging.Responses.getACLResponse) Implements Contracts.ICmisClient.GetAcl
#End Region

#Region "Miscellaneous (ICmisClient)"
      Public MustOverride Sub Logout(repositoryId As String) Implements Contracts.ICmisClient.Logout
      Public MustOverride Sub Ping(repositoryId As String) Implements Contracts.ICmisClient.Ping

      Private _registeredPings As New System.Collections.Generic.Dictionary(Of String, Tuple(Of System.Threading.WaitCallback, System.Threading.EventWaitHandle))
      ''' <summary>
      ''' Installs an automatic ping to tell the server that the client is still alive
      ''' </summary>
      ''' <param name="interval">Time-interval in seconds</param>
      ''' <remarks></remarks>
      Public Sub RegisterPing(repositoryId As String, interval As Double) Implements Contracts.ICmisClient.RegisterPing
         If Not String.IsNullOrEmpty(repositoryId) AndAlso interval > 0 Then
            Dim ewh As New System.Threading.EventWaitHandle(False, Threading.EventResetMode.ManualReset)
            Dim callBack As System.Threading.WaitCallback = Sub(state)
                                                               Dim milliseconds As Integer = CInt(interval * 1000.0)
                                                               While Not ewh.WaitOne(milliseconds)
                                                                  Try
                                                                     Ping(repositoryId)
                                                                  Catch ex As Exception
                                                                     'stop
                                                                     UnregisterPing(repositoryId)
                                                                  End Try
                                                               End While
                                                            End Sub
            'stop the current ping-job
            UnregisterPing(repositoryId)
            SyncLock _registeredPings
               'start the new ping job
               System.Threading.ThreadPool.QueueUserWorkItem(callBack)
               _registeredPings.Add(repositoryId, New Tuple(Of System.Threading.WaitCallback, System.Threading.EventWaitHandle)(callBack, ewh))
            End SyncLock
            'the first time we will do it immediately
            Ping(repositoryId)
         End If
      End Sub

      ''' <summary>
      ''' Returns True if the binding supports a succinct representation of properties
      ''' </summary>
      Public MustOverride ReadOnly Property SupportsSuccinct As Boolean Implements Contracts.ICmisClient.SupportsSuccinct
      ''' <summary>
      ''' Returns True if the binding supports token parameters
      ''' </summary>
      Public MustOverride ReadOnly Property SupportsToken As Boolean Implements Contracts.ICmisClient.SupportsToken
      Public Overridable Property Timeout As Integer? Implements Contracts.ICmisClient.Timeout

      ''' <summary>
      ''' Removes the automatic ping
      ''' </summary>
      Public Sub UnregisterPing(repositoryId As String) Implements Contracts.ICmisClient.UnregisterPing
         If Not String.IsNullOrEmpty(repositoryId) Then
            SyncLock _registeredPings
               If _registeredPings.ContainsKey(repositoryId) Then
                  'stop the current ping-job
                  _registeredPings(repositoryId).Item2.Set()
                  _registeredPings.Remove(repositoryId)
               End If
            End SyncLock
         End If
      End Sub

      Public Overridable ReadOnly Property User As String Implements Contracts.ICmisClient.User
         Get
            Return If(_authentication Is Nothing, Nothing, _authentication.User)
         End Get
      End Property
#End Region

#Region "Requests"

#Region "Vendor specific and value mapping"
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="properties"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function TransformRequest(repositoryId As String, properties As Core.Collections.cmisPropertiesType) As Vendors.Vendor.State
         'first: value mapping, second: vendor specific
         Return _vendor.BeginRequest(repositoryId, properties, _valueMapper.MapProperties(repositoryId, enumMapDirection.outgoing, properties))
      End Function

      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values
      ''' </summary>
      ''' <param name="state"></param>
      ''' <remarks></remarks>
      Protected Sub TransformResponse(state As Vendors.Vendor.State,
                                      propertyCollections As Core.Collections.cmisPropertiesType())
         'first: vendor specifics
         _vendor.EndRequest(state, propertyCollections)
         'second: value mapping
         _valueMapper.MapProperties(state.RepositoryId, enumMapDirection.incoming, propertyCollections)
      End Sub
#End Region

      ''' <summary>
      ''' Creates a HttpWebRequest-instance
      ''' </summary>
#If HttpRequestAddRangeShortened Then
      Protected Overridable Function CreateHttpWebRequest(uri As Uri, method As String, data As System.IO.MemoryStream, contentType As String,
                                                          headers As Dictionary(Of String, String), offset As Integer?, length As Integer?) As sn.HttpWebRequest
#Else
      Protected Overridable Function CreateHttpWebRequest(uri As Uri, method As String, data As System.IO.MemoryStream, contentType As String,
                                                          headers As Dictionary(Of String, String), offset As xs_Integer?, length As xs_Integer?) As sn.HttpWebRequest
#End If
         Try
            Dim retVal As sn.HttpWebRequest = CType(sn.WebRequest.Create(uri), sn.HttpWebRequest)

            retVal.Method = method
            retVal.UserAgent = Me.UserAgent
            retVal.AllowAutoRedirect = (_authentication Is Nothing)
            If _connectTimeout.HasValue Then retVal.Timeout = _connectTimeout.Value
            If _readWriteTimeout.HasValue Then retVal.ReadWriteTimeout = _readWriteTimeout.Value
            If Not String.IsNullOrEmpty(contentType) Then retVal.ContentType = contentType
            If headers IsNot Nothing Then
               For Each de As KeyValuePair(Of String, String) In headers
                  retVal.Headers.Add(de.Key, de.Value)
               Next
            End If
            If _authentication IsNot Nothing Then
               retVal.PreAuthenticate = True
               _authentication.Authenticate(retVal)
            End If
            If offset.HasValue AndAlso offset.Value >= 0 Then
               If length.HasValue AndAlso length.Value >= 0 Then
                  retVal.AddRange(offset.Value, offset.Value + length.Value - 1)
               Else
                  retVal.AddRange(offset.Value)
               End If
            End If
            If AppSettings.Compression Then retVal.AutomaticDecompression = Net.DecompressionMethods.GZip Or Net.DecompressionMethods.Deflate
            If data IsNot Nothing Then
               Dim requestStream As IO.Stream

               retVal.SendChunked = True
               requestStream = retVal.GetRequestStream()
               data.Position = 0
               data.CopyTo(requestStream)
               requestStream.Close()
            End If
            If Timeout.HasValue Then retVal.Timeout = Timeout.Value

            Return retVal
         Catch ex As Exception
            Throw New Exception("Unable to access '" & uri.AbsoluteUri & "'", ex)
         End Try
      End Function

#If HttpRequestAddRangeShortened Then
      Protected Overridable Function GetResponse(uri As Uri, method As String, contentWriter As Action(Of IO.Stream), contentType As String,
                                                 headers As Dictionary(Of String, String), offset As Integer?, length As Integer?) As sn.HttpWebResponse
#Else
      Protected Overridable Function GetResponse(uri As Uri, method As String, contentWriter As Action(Of IO.Stream), contentType As String,
                                                 headers As Dictionary(Of String, String), offset As xs_Integer?, length As xs_Integer?) As sn.HttpWebResponse
#End If
         'up to the maximum of 50 redirections; see https://msdn.microsoft.com/de-de/library/system.net.httpwebrequest.maximumautomaticredirections(v=vs.100).aspx
         Dim redirectionCounter As Integer = 50
         Dim retVal As sn.HttpWebResponse
         Dim content As System.IO.MemoryStream

         If contentWriter Is Nothing Then
            content = Nothing
         Else
            content = New System.IO.MemoryStream()
            contentWriter.Invoke(content)
         End If
         Try
            'If a server uses cookies and supports case insensitive uris, the server will answer a request of GetRepositoryInfo() or GetRepositories(), even if the
            'client doesn't use the correct uri the server listens on. Using the AtomPub-binding the uris to the functions GetRepositoryInfo() or GetRepositories()
            'can only be derived from the serviceDocUri the client is configured with. The uris of all other request will be constructed by using uri-templates
            'the server has to provide. If the spelling differs in case sensitivity a problem might occur as can be seen in the following scenario.
            'Configuration of client: serviceDocUri = https://host/CmisServiceUri/
            'Configuration of server: serviceDocUri = https://host/CMISServiceUri/
            'Log on via GetRepositoryInfo to repositoryId repId: https://host/CmisServiceUri/?repositoryId=repId
            'If the server uses cookies, a cookie could be returned with name="sessionId" value=guid. The client will register the cookie to the path
            '/CmisServiceUri/ because that's the absolutePath of the requested uri.
            'If a subsequent call to the function GetObject() is made the client has to use the uri-template "getObjectById". If this uri-template starts with
            'https://host/CMISServiceUri (see server configuration) it will not send the received cookie "sessionId" back to server because of the difference
            'in case sensitivity and as a result the server will reject the request.
            'The supported mechanism of caseInsensitiveCookies adds cookies to the request which exists from prior requests but possess a divergent spelling.
            Dim caseInsensitiveCookies As sn.CookieCollection = _authentication.CaseInsensitiveCookies(uri)

            If caseInsensitiveCookies.Count > 0 Then _authentication.Cookies.Add(caseInsensitiveCookies)
            Do
               Dim request As sn.HttpWebRequest = CreateHttpWebRequest(uri, method, content, contentType,
                                                                       headers, offset, length)

               redirectionCounter -= 1
               retVal = DirectCast(request.GetResponse(), sn.HttpWebResponse)
               'all statuscodes between 300 and 399 handled as redirection
               If retVal.StatusCode < 300 OrElse retVal.StatusCode >= 400 OrElse redirectionCounter = 0 Then
                  _authentication.CaseInsensitiveCookies = retVal.Cookies
                  Return retVal
               End If
               uri = New Uri(retVal.Headers(sn.HttpResponseHeader.Location))
            Loop
         Finally
            If content IsNot Nothing Then content.Dispose()
         End Try
      End Function

#End Region

      Protected _authentication As AuthenticationProvider
      Public ReadOnly Property Authentication As AuthenticationInfo Implements Contracts.ICmisClient.Authentication
         Get
            Return _authentication
         End Get
      End Property

      Public MustOverride ReadOnly Property ClientType As enumClientType Implements Contracts.ICmisClient.ClientType

      Protected _connectTimeout As Integer?
      ''' <summary>
      ''' Timeout HttpWebRequest.Timeout. If not set default is used.
      ''' </summary>
      ''' <returns></returns>
      Public ReadOnly Property ConnectTimeout As Integer? Implements Contracts.ICmisClient.ConnectTimeout
         Get
            Return _connectTimeout
         End Get
      End Property

      Protected _readWriteTimeout As Integer?
      ''' <summary>
      ''' Timeout read or write operations (HttpWebRequest.ReadWriteTimeout). If not set default is used.
      ''' </summary>
      ''' <returns></returns>
      Public ReadOnly Property ReadWriteTimeout As Integer? Implements Contracts.ICmisClient.ReadWriteTimeout
         Get
            Return _readWriteTimeout
         End Get
      End Property

      Protected _serviceDocUri As Uri
      ''' <summary>
      ''' The base address of the cmis-service the client is connected with
      ''' </summary>
      ''' <returns></returns>
      Public ReadOnly Property ServiceDocUri As Uri Implements Contracts.ICmisClient.ServiceDocUri
         Get
            Return _serviceDocUri
         End Get
      End Property

      Protected MustOverride ReadOnly Property UserAgent As String

      Protected _vendor As Vendors.Vendor
      Public ReadOnly Property Vendor As Vendors.Vendor Implements CmisObjectModel.Contracts.ICmisClient.Vendor
         Get
            Return _vendor
         End Get
      End Property
   End Class

   Namespace Generic
      Public MustInherit Class CmisClient(Of TRespositoryInfo)
         Inherits CmisClient

         Private Const csRepositoryCachePattern As String = "urn:f1a34b95d1164b6da869fca38e6a19a3:{0}"

#Region "Constructors"
         Protected Sub New(serviceDocUri As Uri, vendor As enumVendor,
                        authentication As AuthenticationProvider,
                        Optional connectTimeout As Integer? = Nothing,
                        Optional readWriteTimeout As Integer? = Nothing)
            MyBase.New(serviceDocUri, vendor, authentication, connectTimeout, readWriteTimeout)
         End Sub
#End Region

#Region "Cache"
         Protected _syncObject As New Object
         ''' <summary>
         ''' Access to cached repositoryInfo
         ''' </summary>
         ''' <param name="repositoryId"></param>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Protected Property RepositoryInfo(repositoryId As String) As TRespositoryInfo
            Get
               Dim key As String = String.Format(csRepositoryCachePattern,
                                              ServiceURIs.GetServiceUri(_serviceDocUri.AbsoluteUri, ServiceURIs.enumRepositoriesUri.repositoryId).ReplaceUri("repositoryId", repositoryId))
               Dim retVal As Object

               SyncLock _syncObject
                  retVal = System.Runtime.Caching.MemoryCache.Default.Item(key)
               End SyncLock
               If TypeOf retVal Is TRespositoryInfo Then
                  'renew lease time
                  System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                                   SyncLock _syncObject
                                                                      System.Runtime.Caching.MemoryCache.Default.Remove(key)
                                                                      System.Runtime.Caching.MemoryCache.Default.Add(key, retVal, DateTimeOffset.Now.AddSeconds(AppSettings.CacheLeaseTime))
                                                                   End SyncLock
                                                                End Sub)
                  Return CType(retVal, TRespositoryInfo)
               Else
                  Return Nothing
               End If
            End Get
            Set(value As TRespositoryInfo)
               Dim key As String = String.Format(csRepositoryCachePattern,
                                              ServiceURIs.GetServiceUri(_serviceDocUri.AbsoluteUri, ServiceURIs.enumRepositoriesUri.repositoryId).ReplaceUri("repositoryId", repositoryId))
               SyncLock _syncObject
                  System.Runtime.Caching.MemoryCache.Default.Remove(key)
                  If value IsNot Nothing Then
                     System.Runtime.Caching.MemoryCache.Default.Add(key, value, DateTimeOffset.Now.AddSeconds(AppSettings.CacheLeaseTime))
                  End If
               End SyncLock
            End Set
         End Property
#End Region

      End Class
   End Namespace
End Namespace