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

Namespace CmisObjectModel.Client.AtomPub
   ''' <summary>
   ''' Implements the functionality of a cmis-client version 1.1
   ''' </summary>
   ''' <remarks>
   ''' Requested Repositories will be cached in the System.Runtime.Caching.MemoryCache.Default-instance for a duration of
   ''' AppSettings.CacheLeaseTime (value specified in seconds). After this duration the repository is not longer valid and
   ''' has to be renewed
   ''' Limitations using the AtomPub-Binding: search for 'AtomPub binding' in this document
   ''' </remarks>
   Public Class CmisClient
      Inherits Base.Generic.CmisClient(Of ca.AtomWorkspace)

      Private Shared ReadOnly _requestBaseAttributeOverrides As New sxs.XmlAttributeOverrides
      ''' <summary>
      ''' Supported linkRelationshipTypes in the cache
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _supportedLinkRelationshipTypes As New System.Collections.Generic.HashSet(Of String) From {
         LinkRelationshipTypes.Acl,
         LinkRelationshipTypes.AllowableActions,
         LinkRelationshipTypes.ContentStream,
         LinkRelationshipTypes.Down,
         LinkRelationshipTypes.EditMedia,
         LinkRelationshipTypes.FolderTree,
         LinkRelationshipTypes.Policies,
         LinkRelationshipTypes.Relationships,
         LinkRelationshipTypes.Self,
         LinkRelationshipTypes.Up,
         LinkRelationshipTypes.VersionHistory,
         LinkRelationshipTypes.WorkingCopy}

      Shared Sub New()
         'use Namespace cmisw (instead of cmism) for Type derived from Messaging.Request.RequestBase
         Dim attrs As New sxs.XmlAttributes() With {.XmlRoot = New sxs.XmlRootAttribute() With {.Namespace = Namespaces.cmisw}}

         _requestBaseAttributeOverrides.Add(GetType(Messaging.Requests.RequestBase), attrs)
      End Sub

      Public Sub New(serviceDocUri As Uri, vendor As enumVendor,
                     authentication As AuthenticationProvider,
                     Optional connectTimeout As Integer? = Nothing,
                     Optional readWriteTimeout As Integer? = Nothing)
         MyBase.New(serviceDocUri, vendor, authentication, connectTimeout, readWriteTimeout)
      End Sub

#Region "Repository"
      ''' <summary>
      ''' Creates a new type definition that is a subtype of an existing specified parent type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks>The client gets the information about the types-collection via the collectioninfo</remarks>
      Public Overloads Function CreateType(repositoryId As String, type As Core.Definitions.Types.cmisTypeDefinitionType) As Generic.Response(Of ca.AtomEntry)
         'types-collection
         With GetCollectionInfo(repositoryId, CollectionInfos.Types)
            If .Exception Is Nothing Then
               'collection found, but readonly
               If Not CollectionAccepts(.Response, MediaTypes.Entry) Then
                  Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.NotFound, Messaging.enumServiceException.objectNotFound, "Type-collectionInfo is readonly.")
                  Return cmisFault.ToFaultException()
               Else
                  'create the new type
                  Dim retVal As Generic.Response(Of ca.AtomEntry) = Me.Post(.Response.Link, New sss.Atom10ItemFormatter(New ca.AtomEntry(type)),
                                                                            MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)

                  If retVal.Exception Is Nothing Then WriteToCache(repositoryId, Nothing, retVal.Response, _typeLinks)
                  Return retVal
               End If
            Else
               'types-collectionInfo not found
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a new type definition that is a subtype of an existing specified parent type
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Function CreateType(request As cm.Requests.createType) As Generic.Response(Of cmr.createTypeResponse)
         Dim result = CreateType(request.RepositoryId, request.Type)

         If result.Exception Is Nothing Then
            Return New cmr.createTypeResponse() With {.Type = result.Response.Type}
         Else
            Return result.Exception
         End If
      End Function

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function DeleteType(repositoryId As String, typeId As String) As Response
         'get the self relation of specified type to delete the type using this link
         With GetLink(repositoryId, typeId, LinkRelationshipTypes.Self, MediaTypes.Entry, _typeLinks, AddressOf GetTypeDefinition)
            If .Exception Is Nothing Then
               'type found
               Return Delete(New Uri(.Response))
            Else
               'type not found
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Function DeleteType(request As cm.Requests.deleteType) As Generic.Response(Of cmr.deleteTypeResponse)
         With DeleteType(request.RepositoryId, request.TypeId)
            If .Exception Is Nothing Then
               Return New cmr.deleteTypeResponse
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Returns all repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>_serviceDocUri used
      ''' This is the only method which uses a known uri (_serviceDocUri). From this point the client gets informations about
      ''' the repositories supported by the server. From the instance of a repositoryInfoType the client is able to request for
      ''' types and objects via the uri-templates ObjectById and TypeById. The given collectionInfos in the repositoryInfoType
      ''' allows the direct access to checkedOut-, bulkUpdate-, types-, typedescendants-, unfiled- and query-collection.
      ''' All the other methods of the cmis-service uses diverse links sent with every request to objects or types</remarks>
      Public Overloads Function GetRepositories() As Generic.Response(Of ca.AtomServiceDocument)
         Return Me.Get(_serviceDocUri, AddressOf ca.AtomServiceDocument.CreateInstance)
      End Function
      ''' <summary>
      ''' Returns all repositories
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Function GetRepositories(request As cm.Requests.getRepositories) As Generic.Response(Of cmr.getRepositoriesResponse)
         Dim response = GetRepositories()

         If response.Exception Is Nothing Then
            Return New cmr.getRepositoriesResponse() With {.Repositories = (From workspace As sss.Workspace In response.Response.Workspaces
                                                                            Let ws As ca.AtomWorkspace = TryCast(workspace, ca.AtomWorkspace)
                                                                            Where ws IsNot Nothing
                                                                            Select CType(ws.RepositoryInfo, cm.cmisRepositoryEntryType)).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the workspace of specified repository
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="ignoreCache">If True the method ignores cached repository informations</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetRepositoryInfo(repositoryId As String, Optional ignoreCache As Boolean = False) As Generic.Response(Of ca.AtomWorkspace)
         'try to get the info using the cache
         Dim ws As ca.AtomWorkspace = If(ignoreCache, Nothing, RepositoryInfo(repositoryId))

         'workspace of specified repository could not be found in the cache
         If ws Is Nothing Then
            'in 3.7.1  HTTP GET it is defined that a client may add the repositoryId as an optional argument to the
            'AtomPub Service Document resource
            Dim serviceDocUri As New Uri(ServiceURIs.GetServiceUri(_serviceDocUri.OriginalString,
                                                                   ServiceURIs.enumRepositoriesUri.repositoryId).ReplaceUri("repositoryId", repositoryId))
            Dim response As Generic.Response(Of ca.AtomServiceDocument) = Me.Get(serviceDocUri, AddressOf ca.AtomServiceDocument.CreateInstance)

            If response.Exception Is Nothing Then
               'store response into the cache
               For Each ws In response.Response.Workspaces
                  WriteToCache(ws)
               Next
               'try to get workspace
               ws = RepositoryInfo(repositoryId)
            Else
               Return response.Exception
            End If
         End If

         If ws Is Nothing Then
            Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.NotFound, Messaging.enumServiceException.objectNotFound, "Workspace not found.")
            Return cmisFault.ToFaultException()
         Else
            Return ws
         End If
      End Function
      ''' <summary>
      ''' Returns the workspace of specified repository or null
      ''' </summary>
      ''' <param name="ignoreCache">If True the method ignores cached repository informations</param>
      ''' <remarks></remarks>
      Public Overrides Function GetRepositoryInfo(request As cm.Requests.getRepositoryInfo, Optional ignoreCache As Boolean = False) As Generic.Response(Of cmr.getRepositoryInfoResponse)
         Dim response = GetRepositoryInfo(request.RepositoryId, ignoreCache)

         If response.Exception Is Nothing Then
            Return New cmr.getRepositoryInfoResponse() With {.RepositoryInfo = response.Response.RepositoryInfo}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId">If null this function returns all base types, otherwise the children of the speciefied type</param>
      ''' <param name="includePropertyDefinitions"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetTypeChildren(repositoryId As String,
                                                Optional typeId As String = Nothing,
                                                Optional includePropertyDefinitions As Boolean = False,
                                                Optional maxItems As xs_Integer? = Nothing,
                                                Optional skipCount As xs_Integer? = Nothing) As Generic.Response(Of ca.AtomFeed)
         Dim link As String

         If String.IsNullOrEmpty(typeId) Then
            'link to all base types using types-collectionInfo
            With GetCollectionInfo(repositoryId, CollectionInfos.Types)
               If .Exception Is Nothing Then
                  link = .Response.Link.OriginalString
               Else
                  Return .Exception
               End If
            End With
         Else
            'link to children of specified type
            With GetLink(repositoryId, typeId, LinkRelationshipTypes.Down, MediaTypes.Feed, _typeLinks, AddressOf GetTypeDefinition)
               If .Exception Is Nothing Then
                  link = .Response
               Else
                  Return .Exception
               End If
            End With
         End If
         'notice: the typeId-parameter is already handled (GetLink())
         With New ccg.LinkUriBuilder(Of ServiceURIs.enumTypesUri)(link, repositoryId)
            .Add(ServiceURIs.enumTypesUri.includePropertyDefinitions, includePropertyDefinitions)
            .Add(ServiceURIs.enumTypesUri.maxItems, maxItems)
            .Add(ServiceURIs.enumTypesUri.skipCount, skipCount)

            Dim retVal As Generic.Response(Of ca.AtomFeed) = Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance)

            If retVal.Exception Is Nothing Then WriteToCache(repositoryId, retVal.Response, _typeLinks)
            Return retVal
         End With
      End Function
      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the specified type
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Function GetTypeChildren(request As cm.Requests.getTypeChildren) As Generic.Response(Of cmr.getTypeChildrenResponse)
         Dim response = GetTypeChildren(request.RepositoryId, request.TypeId,
                                        request.IncludePropertyDefinitions.HasValue AndAlso request.IncludePropertyDefinitions.Value,
                                        request.MaxItems, request.SkipCount)

         If response.Exception Is Nothing Then
            Return New cmr.getTypeChildrenResponse() With {.Types = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks>uses uritemplate: TypeById</remarks>
      Public Overloads Function GetTypeDefinition(repositoryId As String, typeId As String) As Generic.Response(Of ca.AtomEntry)
         Dim retVal As Generic.Response(Of ca.AtomEntry)

         'ensure that the called repositoryId is available
         With GetRepositoryInfo(repositoryId)
            If .Exception Is Nothing Then
               Dim uriTemplate As String = .Response.UriTemplate(UriTemplates.TypeById).Template
               Dim state As Vendors.Vendor.State = _vendor.BeginRequest(repositoryId, typeId)

               uriTemplate = uriTemplate.ReplaceUriTemplate("id", typeId)
               retVal = Me.Get(New Uri(uriTemplate), AddressOf ca.AtomEntry.CreateInstance)
               If retVal.Exception Is Nothing Then
                  _vendor.EndRequest(state, retVal.Response.Type)
                  WriteToCache(repositoryId, typeId, retVal.Response, _typeLinks)
               End If
            Else
               retVal = .Exception
            End If
         End With

         Return retVal
      End Function
      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetTypeDefinition(request As cm.Requests.getTypeDefinition) As Generic.Response(Of cmr.getTypeDefinitionResponse)
         Dim response = GetTypeDefinition(request.RepositoryId, request.TypeId)

         If response.Exception Is Nothing Then
            Return New cmr.getTypeDefinitionResponse() With {.Type = response.Response.Type}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId">If null this function returns all types, otherwise the descendants of the speciefied type</param>
      ''' <param name="depth"></param>
      ''' <param name="includePropertyDefinitions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetTypeDescendants(repositoryId As String,
                                                   Optional typeId As String = Nothing,
                                                   Optional depth As xs_Integer? = Nothing,
                                                   Optional includePropertyDefinitions As Boolean = False) As Generic.Response(Of ca.AtomFeed)
         Dim link As String

         If String.IsNullOrEmpty(typeId) Then
            'link to all types using typedescendants-collectionInfo
            With GetRepositoryLink(repositoryId, LinkRelationshipTypes.TypeDescendants)
               If .Exception Is Nothing Then
                  link = .Response
               Else
                  Return .Exception
               End If
            End With
         Else
            'typedescendants of specified type
            With GetLink(repositoryId, typeId, LinkRelationshipTypes.Down, MediaTypes.Tree, _typeLinks, AddressOf GetTypeDefinition)
               If .Exception Is Nothing Then
                  link = .Response
               Else
                  Return .Exception
               End If
            End With
         End If
         'notice: the typeId-parameter is already handled (GetLink())
         With New ccg.LinkUriBuilder(Of ServiceURIs.enumTypeDescendantsUri)(link, repositoryId)
            If Not String.IsNullOrEmpty(typeId) Then .Add(ServiceURIs.enumTypeDescendantsUri.depth, depth)
            .Add(ServiceURIs.enumTypeDescendantsUri.includePropertyDefinitions, includePropertyDefinitions)

            Dim retVal As Generic.Response(Of ca.AtomFeed) = Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance)

            If retVal.Exception Is Nothing Then WriteToCache(repositoryId, retVal.Response, _typeLinks)
            Return retVal
         End With
      End Function
      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetTypeDescendants(request As cm.Requests.getTypeDescendants) As Generic.Response(Of cmr.getTypeDescendantsResponse)
         Dim response = GetTypeDescendants(request.RepositoryId, request.TypeId, request.Depth,
                                           request.IncludePropertyDefinitions.HasValue AndAlso request.IncludePropertyDefinitions.Value)
         If response.Exception Is Nothing Then
            Return New cmr.getTypeDescendantsResponse() With {.Types = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                        Let typeContainer As cm.cmisTypeContainer = entry
                                                                        Where typeContainer IsNot Nothing
                                                                        Select typeContainer).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function UpdateType(repositoryId As String, type As Core.Definitions.Types.cmisTypeDefinitionType) As Generic.Response(Of ca.AtomEntry)
         If type Is Nothing OrElse String.IsNullOrEmpty(type.Id) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Argument type MUST NOT be null and type.Id MUST be set.")
            Return cmisFault.ToFaultException()
         End If
         'using self-link to modify type
         With GetLink(repositoryId, type.Id, LinkRelationshipTypes.Self, MediaTypes.Entry, _typeLinks, AddressOf GetTypeDefinition)
            If .Exception Is Nothing Then
               Return Put(New Uri(.Response), New sss.Atom10ItemFormatter(New ca.AtomEntry(type)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function UpdateType(request As cm.Requests.updateType) As Generic.Response(Of cmr.updateTypeResponse)
         Dim response = UpdateType(request.RepositoryId, request.Type)

         If response.Exception Is Nothing Then
            Return New cmr.updateTypeResponse() With {.Type = response.Response.Type}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetCheckedOutDocs(repositoryId As String,
                                                  Optional folderId As String = Nothing,
                                                  Optional maxItems As xs_Integer? = Nothing,
                                                  Optional skipCount As xs_Integer? = Nothing,
                                                  Optional orderBy As String = Nothing,
                                                  Optional filter As String = Nothing,
                                                  Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                  Optional renditionFilter As String = Nothing,
                                                  Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomFeed)
         'the access to checkedOut-collection is defined by checkedOut-collectionInfo
         With GetCollectionInfo(repositoryId, CollectionInfos.CheckedOut)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumCheckedOutUri)(.Response.Link.OriginalString, repositoryId)
                  .Add(ServiceURIs.enumCheckedOutUri.folderId, folderId)
                  .Add(ServiceURIs.enumCheckedOutUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumCheckedOutUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumCheckedOutUri.orderBy, orderBy)
                  .Add(ServiceURIs.enumCheckedOutUri.filter, filter)
                  .Add(ServiceURIs.enumCheckedOutUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumCheckedOutUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumCheckedOutUri.includeAllowableActions, includeAllowableActions)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetCheckedOutDocs(request As cm.Requests.getCheckedOutDocs) As Generic.Response(Of cmr.getCheckedOutDocsResponse)
         Dim response = GetCheckedOutDocs(request.RepositoryId, request.FolderId, request.MaxItems, request.SkipCount, request.OrderBy,
                                          request.Filter, request.IncludeRelationships, request.RenditionFilter,
                                          request.IncludeAllowableActions)
         If response.Exception Is Nothing Then
            Return New cmr.getCheckedOutDocsResponse() With {.Objects = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the list of child objects contained in the specified folder
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetChildren(repositoryId As String, folderId As String,
                                            Optional maxItems As xs_Integer? = Nothing,
                                            Optional skipCount As xs_Integer? = Nothing,
                                            Optional orderBy As String = Nothing,
                                            Optional filter As String = Nothing,
                                            Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                            Optional renditionFilter As String = Nothing,
                                            Optional includeAllowableActions As Boolean? = Nothing,
                                            Optional includePathSegment As Boolean = False) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, folderId, LinkRelationshipTypes.Down, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'notice: the folderId-parameter is already handled (GetLink())
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumChildrenUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumChildrenUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumChildrenUri.orderBy, orderBy)
                  .Add(ServiceURIs.enumChildrenUri.filter, filter)
                  .Add(ServiceURIs.enumChildrenUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumChildrenUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumChildrenUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumChildrenUri.includePathSegment, includePathSegment)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of child objects contained in the specified folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetChildren(request As cm.Requests.getChildren) As Generic.Response(Of cmr.getChildrenResponse)
         Dim response = GetChildren(request.RepositoryId, request.FolderId, request.MaxItems, request.SkipCount, request.OrderBy,
                                    request.Filter, request.IncludeRelationships, request.RenditionFilter, request.IncludeAllowableActions,
                                    request.IncludePathSegment.HasValue AndAlso request.IncludePathSegment.Value)
         If response.Exception Is Nothing Then
            Return New cmr.getChildrenResponse() With {.Objects = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the set of descendant objects containded in the specified folder or any of its child-folders
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="depth"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetDescendants(repositoryId As String, folderId As String,
                                               Optional depth As xs_Integer? = Nothing,
                                               Optional filter As String = Nothing,
                                               Optional includeAllowableActions As Boolean? = Nothing,
                                               Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                               Optional renditionFilter As String = Nothing,
                                               Optional includePathSegment As Boolean = False) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, folderId, LinkRelationshipTypes.Down, MediaTypes.Tree, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'notice: the folderId-parameter is already handled (GetLink())
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumDescendantsUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumDescendantsUri.depth, depth)
                  .Add(ServiceURIs.enumDescendantsUri.filter, filter)
                  .Add(ServiceURIs.enumDescendantsUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumDescendantsUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumDescendantsUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumDescendantsUri.includePathSegment, includePathSegment)


                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the set of descendant objects containded in the specified folder or any of its child-folders
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetDescendants(request As cm.Requests.getDescendants) As Generic.Response(Of cmr.getDescendantsResponse)
         Dim response = GetDescendants(request.RepositoryId, request.FolderId, request.Depth, request.Filter,
                                       request.IncludeAllowableActions, request.IncludeRelationships, request.RenditionFilter,
                                       request.IncludePathSegment.HasValue AndAlso request.IncludePathSegment.Value)

         If response.Exception Is Nothing Then
            Return New cmr.getDescendantsResponse() With {.Objects = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                      Let objectInFolderContainer As cm.cmisObjectInFolderContainerType = entry
                                                                      Where objectInFolderContainer IsNot Nothing
                                                                      Select objectInFolderContainer).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the parent folder object for the specified folder object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetFolderParent(repositoryId As String, folderId As String, Optional filter As String = Nothing) As Generic.Response(Of ca.AtomEntry)
         With GetLink(repositoryId, folderId, LinkRelationshipTypes.Up, MediaTypes.Entry, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'notice: the folderId-parameter is already handled (GetLink())
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumObjectUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumObjectUri.filter, filter)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Dim response As Generic.Response(Of ca.AtomEntry) = Me.Get(.ToUri(), AddressOf ca.AtomEntry.CreateInstance)
                  Dim parentFolderId As String = If(response.Exception Is Nothing, response.Response.ObjectId, Nothing)
                  Return TransformResponse(response, state, parentFolderId, Not String.IsNullOrEmpty(parentFolderId))
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the parent folder object for the specified folder object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetFolderParent(request As cm.Requests.getFolderParent) As Generic.Response(Of cmr.getFolderParentResponse)
         Dim response = GetFolderParent(request.RepositoryId, request.FolderId, request.Filter)

         If response.Exception Is Nothing Then
            Return New cmr.getFolderParentResponse() With {.Object = response.Response.Object}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the set of descendant folder objects contained in the specified folder
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="depth"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetFolderTree(repositoryId As String, folderId As String,
                                              Optional depth As xs_Integer? = Nothing,
                                              Optional filter As String = Nothing,
                                              Optional includeAllowableActions As Boolean? = Nothing,
                                              Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                              Optional renditionFilter As String = Nothing,
                                              Optional includePathSegment As Boolean = False) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, folderId, LinkRelationshipTypes.FolderTree, MediaTypes.Tree, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'notice: the folderId-parameter is already handled (GetLink())
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumFolderTreeUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumFolderTreeUri.depth, depth)
                  .Add(ServiceURIs.enumFolderTreeUri.filter, filter)
                  .Add(ServiceURIs.enumFolderTreeUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumFolderTreeUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumFolderTreeUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumFolderTreeUri.includePathSegment, includePathSegment)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the set of descendant folder objects contained in the specified folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetFolderTree(request As cm.Requests.getFolderTree) As Generic.Response(Of cmr.getFolderTreeResponse)
         Dim response = GetFolderTree(request.RepositoryId, request.FolderId, request.Depth, request.Filter,
                                      request.IncludeAllowableActions, request.IncludeRelationships, request.RenditionFilter,
                                      request.IncludePathSegment.HasValue AndAlso request.IncludePathSegment.Value)
         If response.Exception Is Nothing Then
            Return New cmr.getFolderTreeResponse() With {.Objects = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                     Let objectInFolderContainer As cm.cmisObjectInFolderContainerType = entry
                                                                     Where objectInFolderContainer IsNot Nothing
                                                                     Select objectInFolderContainer).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the parent folder(s) for the specified fileable object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetObjectParents(repositoryId As String, objectId As String,
                                                 Optional filter As String = Nothing,
                                                 Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                 Optional renditionFilter As String = Nothing,
                                                 Optional includeAllowableActions As Boolean? = Nothing,
                                                 Optional includeRelativePathSegment As Boolean? = Nothing) As Generic.Response(Of ca.AtomFeed)
         'non folder object may have more than one parent if the repository supports multifiling. Therefore this function returns a
         'list (feed) of entries. To get the parent of a folder it is recommend to use GetFolderParent()
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Up, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'notice: the objectId-parameter is already handled (GetLink())
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumObjectParentsUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumObjectParentsUri.filter, filter)
                  .Add(ServiceURIs.enumObjectParentsUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumObjectParentsUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumObjectParentsUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumObjectParentsUri.includeRelativePathSegment, includeRelativePathSegment)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the parent folder(s) for the specified fileable object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetObjectParents(request As cm.Requests.getObjectParents) As Generic.Response(Of cmr.getObjectParentsResponse)
         Dim response = GetObjectParents(request.RepositoryId, request.ObjectId, request.Filter, request.IncludeRelationships,
                                         request.RenditionFilter, request.IncludeAllowableActions, request.IncludeRelativePathSegment)
         If response.Exception Is Nothing Then
            Return New cmr.getObjectParentsResponse() With {.Parents = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                        Let parent As cm.cmisObjectParentsType = entry
                                                                        Where parent IsNot Nothing
                                                                        Select parent).ToArray()}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="contentStream"></param>
      ''' <param name="isLastChunk"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks>uses editmedia-link</remarks>
      Public Overloads Function AppendContentStream(repositoryId As String, objectId As String, contentStream As cm.cmisContentStreamType,
                                                    Optional isLastChunk As Boolean = False,
                                                    Optional changeToken As String = Nothing) As Generic.Response(Of cmr.appendContentStreamResponse)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         ElseIf contentStream Is Nothing OrElse contentStream.BinaryStream Is Nothing Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument contentStream MUST NOT be null.")
            Return cmisFault.ToFaultException()
         End If

         Dim headers As Dictionary(Of String, String)

         With GetLink(repositoryId, objectId, LinkRelationshipTypes.EditMedia, contentStream.MimeType, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumContentUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumContentUri.append, True)
                  .Add(ServiceURIs.enumContentUri.changeToken, changeToken)
                  .Add(ServiceURIs.enumContentUri.isLastChunk, isLastChunk)

                  If String.IsNullOrEmpty(contentStream.Filename) Then
                     headers = Nothing
                  Else
                     'If the client wishes to set a new filename, it MAY add a Content-Disposition header, which carries the new filename.
                     'The disposition type MUST be "attachment". The repository SHOULD use the "filename" parameter and SHOULD ignore all other parameters
                     'see 3.11.8.2 HTTP PUT
                     headers = New Dictionary(Of String, String) From {{RFC2231Helper.ContentDispositionHeaderName,
                                                                        RFC2231Helper.EncodeContentDisposition(contentStream.Filename)}}
                  End If

                  Return Put(.ToUri(), contentStream.BinaryStream, contentStream.MimeType, headers, AddressOf cmr.appendContentStreamResponse.CreateInstance)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Appends to the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function AppendContentStream(request As cm.Requests.appendContentStream) As Generic.Response(Of cmr.appendContentStreamResponse)
         Dim response = AppendContentStream(request.RepositoryId, request.ObjectId, request.ContentStream,
                                            request.IsLastChunk.HasValue AndAlso request.IsLastChunk.Value,
                                            request.ChangeToken)
         If response.Exception Is Nothing Then
            If response.Response Is Nothing Then
               Return New cmr.appendContentStreamResponse() With {.ObjectId = request.ObjectId, .ChangeToken = request.ChangeToken}
            Else
               Return New cmr.appendContentStreamResponse() With {.ObjectId = response.Response.ObjectId, .ChangeToken = response.Response.ChangeToken}
            End If
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectIdAndChangeTokens"></param>
      ''' <param name="properties"></param>
      ''' <param name="addSecondaryTypeIds"></param>
      ''' <param name="removeSecondaryTypeIds"></param>
      ''' <returns></returns>
      ''' <remarks>uses collectionInfo for bulkUpdate
      ''' see 3.8.6.1 HTTP POST:
      ''' The property cmis:objectId MUST be set.
      ''' The value MUST be the original object id even if the repository created a new version and therefore generated a new object id.
      ''' New object ids are not exposed by AtomPub binding. 
      ''' The property cmis:changeToken MUST be set if the repository supports change tokens
      ''' </remarks>
      Public Overloads Function BulkUpdateProperties(repositoryId As String, objectIdAndChangeTokens As Core.cmisObjectIdAndChangeTokenType(),
                                                     Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                                     Optional addSecondaryTypeIds As String() = Nothing,
                                                     Optional removeSecondaryTypeIds As String() = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetCollectionInfo(repositoryId, CollectionInfos.Update)
            If .Exception Is Nothing Then
               Dim bulkUpdate As New Core.cmisBulkUpdateType() With {.AddSecondaryTypeIds = addSecondaryTypeIds, .ObjectIdAndChangeTokens = objectIdAndChangeTokens,
                                                                     .Properties = properties, .RemoveSecondaryTypeIds = removeSecondaryTypeIds}
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  Return TransformResponse(Post(.Response.Link, New sss.Atom10ItemFormatter(New ca.AtomEntry(bulkUpdate)),
                                                MediaTypes.Entry, Nothing, AddressOf ca.AtomFeed.CreateInstance), state, False)
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>see 3.8.6.1 HTTP POST:
      ''' The property cmis:objectId MUST be set.
      ''' The value MUST be the original object id even if the repository created a new version and therefore generated a new object id.
      ''' New object ids are not exposed by AtomPub binding. 
      ''' The property cmis:changeToken MUST be set if the repository supports change tokens
      ''' </remarks>
      Public Overrides Function BulkUpdateProperties(request As cm.Requests.bulkUpdateProperties) As Generic.Response(Of cmr.bulkUpdatePropertiesResponse)
         Dim response = BulkUpdateProperties(request.RepositoryId, request.BulkUpdateData.ObjectIdAndChangeTokens,
                                             request.BulkUpdateData.Properties,
                                             request.BulkUpdateData.AddSecondaryTypeIds,
                                             request.BulkUpdateData.RemoveSecondaryTypeIds)
         If response.Exception Is Nothing Then
            Return New cmr.bulkUpdatePropertiesResponse() With {.ObjectIdAndChangeTokens = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                                            Let objectIdAndChangeToken As Core.cmisObjectIdAndChangeTokenType = entry
                                                                                            Where objectIdAndChangeToken IsNot Nothing
                                                                                            Select objectIdAndChangeToken).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createDocument-instance (detected by the accepted MediaTypes.Request-contentType of the unfiled- or
      ''' root-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way to reduce
      ''' communication with the server.</remarks>
      Public Overloads Function CreateDocument(repositoryId As String,
                                               properties As Core.Collections.cmisPropertiesType,
                                               Optional folderId As String = Nothing,
                                               Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                                               Optional versioningState As Core.enumVersioningState? = Nothing,
                                               Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                               Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                               Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(cmisObject.ObjectTypeId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectTypeId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetChildrenOrUnfiledLink(repositoryId, folderId, acceptRequest)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     .Add(ServiceURIs.enumChildrenUri.versioningState, versioningState)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createDocument = New Messaging.Requests.createDocument() With {
                           .AddACEs = addACEs,
                           .ContentStream = contentStream,
                           .FolderId = folderId,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId,
                           .VersioningState = versioningState}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim content As RestAtom.cmisContentType = CType(contentStream, RestAtom.cmisContentType)
                        Dim entry As New ca.AtomEntry(cmisObject, content)
                        Dim headers As Dictionary(Of String, String) = Nothing

                        'transmit ContentStreamFileName, ContentStreamLength as property
                        If contentStream IsNot Nothing Then
                           contentStream.ExtendProperties(cmisObject)
                           If Not String.IsNullOrEmpty(contentStream.Filename) Then
                              'If the client wishes to set a new filename, it MAY add a Content-Disposition header, which carries the new filename.
                              'The disposition type MUST be "attachment". The repository SHOULD use the "filename" parameter and SHOULD ignore all other parameters
                              'see 3.11.8.2 HTTP PUT
                              headers = New Dictionary(Of String, String) From {{RFC2231Helper.ContentDispositionHeaderName,
                                                                                 RFC2231Helper.EncodeContentDisposition(contentStream.Filename)}}
                           End If
                        End If
                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, headers, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreateDocument(request As cm.Requests.createDocument) As Generic.Response(Of cmr.createDocumentResponse)
         Dim response = CreateDocument(request.RepositoryId, request.Properties,
                                       request.FolderId, request.ContentStream,
                                       request.VersioningState, request.Policies,
                                       request.AddACEs, request.RemoveACEs)
         If response.Exception Is Nothing Then
            Return New cmr.createDocumentResponse() With {.Object = response.Response.Object}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="properties"></param>
      ''' <param name="folderId"></param>
      ''' <param name="versioningState"></param>
      ''' <param name="policies"></param>
      ''' <param name="addACEs"></param>
      ''' <param name="removeACEs"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createDocumentFromSource-instance (detected by the accepted MediaTypes.Request-contentType of the
      ''' unfiled- or root-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way
      ''' to reduce communication with the server.</remarks>
      Public Overloads Function CreateDocumentFromSource(repositoryId As String, sourceId As String,
                                                         Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                                         Optional folderId As String = Nothing,
                                                         Optional versioningState As Core.enumVersioningState? = Nothing,
                                                         Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                                         Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                         Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetChildrenOrUnfiledLink(repositoryId, folderId, acceptRequest)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     .Add(ServiceURIs.enumChildrenUri.sourceId, sourceId)
                     .Add(ServiceURIs.enumChildrenUri.versioningState, versioningState)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createDocumentFromSource = New Messaging.Requests.createDocumentFromSource() With {
                           .AddACEs = addACEs,
                           .FolderId = folderId,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId,
                           .SourceId = sourceId,
                           .VersioningState = versioningState}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim entry As New ca.AtomEntry(cmisObject)

                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreateDocumentFromSource(request As cm.Requests.createDocumentFromSource) As Generic.Response(Of cmr.createDocumentFromSourceResponse)
         Dim response = CreateDocumentFromSource(request.RepositoryId, request.SourceId,
                                                 request.Properties, request.FolderId,
                                                 request.VersioningState, request.Policies,
                                                 request.AddACEs, request.RemoveACEs)
         If response.Exception Is Nothing Then
            Return New cmr.createDocumentFromSourceResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createFolder-instance (detected by the accepted MediaTypes.Request-contentType of the
      ''' root-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way to reduce
      ''' communication with the server.</remarks>
      Public Overloads Function CreateFolder(repositoryId As String,
                                             properties As Core.Collections.cmisPropertiesType,
                                             folderId As String,
                                             Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                             Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                             Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(cmisObject.ObjectTypeId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectTypeId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(folderId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Argument folderId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetChildrenOrUnfiledLink(repositoryId, folderId, acceptRequest)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createFolder = New Messaging.Requests.createFolder() With {
                           .AddACEs = addACEs,
                           .FolderId = folderId,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim entry As New ca.AtomEntry(cmisObject)

                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreateFolder(request As cm.Requests.createFolder) As Generic.Response(Of cmr.createFolderResponse)
         Dim response = CreateFolder(request.RepositoryId, request.Properties, request.FolderId,
                                     request.Policies, request.AddACEs, request.RemoveACEs)
         If response.Exception Is Nothing Then
            Return New cmr.createFolderResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Creates a item object of the specified type (given by the cmis:objectTypeId property) in (optionally) the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createItem-instance (detected by the accepted MediaTypes.Request-contentType of the unfiled- or
      ''' root-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way to reduce
      ''' communication with the server.</remarks>
      Public Overloads Function CreateItem(repositoryId As String,
                                           properties As Core.Collections.cmisPropertiesType,
                                           Optional folderId As String = Nothing,
                                           Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                           Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                           Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(cmisObject.ObjectTypeId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectTypeId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetChildrenOrUnfiledLink(repositoryId, folderId, acceptRequest)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createItem = New Messaging.Requests.createItem() With {
                           .AddACEs = addACEs,
                           .FolderId = folderId,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim entry As New ca.AtomEntry(cmisObject)

                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a item object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreateItem(request As cm.Requests.createItem) As Generic.Response(Of cmr.createItemResponse)
         Dim response = CreateItem(request.RepositoryId, request.Properties, request.FolderId, request.Policies, request.AddACEs, request.RemoveACEs)

         If response.Exception Is Nothing Then
            Return New cmr.createItemResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Creates a policy object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createPolicy-instance (detected by the accepted MediaTypes.Request-contentType of the unfiled- or
      ''' root-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way to reduce
      ''' communication with the server.</remarks>
      Public Overloads Function CreatePolicy(repositoryId As String,
                                             properties As Core.Collections.cmisPropertiesType,
                                             Optional folderId As String = Nothing,
                                             Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                             Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                             Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(cmisObject.ObjectTypeId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectTypeId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetChildrenOrUnfiledLink(repositoryId, folderId, acceptRequest)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createPolicy = New Messaging.Requests.createPolicy() With {
                           .AddACEs = addACEs,
                           .FolderId = folderId,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim entry As New ca.AtomEntry(cmisObject)

                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a policy object of the specified type (given by the cmis:objectTypeId property) in (optionally) the specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreatePolicy(request As cm.Requests.createPolicy) As Generic.Response(Of cmr.createPolicyResponse)
         Dim response = CreatePolicy(request.RepositoryId, request.Properties, request.FolderId, request.Policies, request.AddACEs, request.RemoveACEs)

         If response.Exception Is Nothing Then
            Return New cmr.createPolicyResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Creates a relationship object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>addACEs and removeACEs must be written in a second roundtrip except the repository allows to transfer a
      ''' Messaging.Request.createRelationship-instance (detected by the accepted MediaTypes.Request-contentType of the
      ''' relationships-collectionInfo). If the mediatype MediaTypes.Request is allowed this function will prefer this way to reduce
      ''' communication with the server.</remarks>
      Public Overloads Function CreateRelationship(repositoryId As String,
                                                   properties As Core.Collections.cmisPropertiesType,
                                                   Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                                   Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                   Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties, .PolicyIds = policies}
         Dim acceptRequest As Boolean = CollectionAccepts(repositoryId, CollectionInfos.Relationships, MediaTypes.Request)
         Dim sourceIdProperty As Core.Properties.cmisPropertyId = If(properties Is Nothing, Nothing,
                                                                     TryCast(properties.FindProperty(CmisPredefinedPropertyNames.SourceId), Core.Properties.cmisPropertyId))
         Dim sourceId As String = If(sourceIdProperty Is Nothing, Nothing, sourceIdProperty.Value)

         If Not String.IsNullOrEmpty(cmisObject.ObjectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectId & "' MUST NOT be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(cmisObject.ObjectTypeId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.ObjectTypeId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(sourceId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Property '" & CmisPredefinedPropertyNames.SourceId & "' MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, sourceId, LinkRelationshipTypes.Relationships, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               Dim response As Generic.Response(Of ca.AtomEntry)
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                     If acceptRequest Then
                        'the server accepts Messaging.Requests
                        Dim request As Messaging.Requests.createRelationship = New Messaging.Requests.createRelationship() With {
                           .AddACEs = addACEs,
                           .Policies = If(policies Is Nothing, Nothing, policies.Ids),
                           .Properties = properties,
                           .RemoveACEs = removeACEs,
                           .RepositoryId = repositoryId}
                        response = Post(.ToUri(), request, MediaTypes.Request, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                     Else
                        Dim entry As New ca.AtomEntry(cmisObject)

                        response = Post(.ToUri(), New sss.Atom10ItemFormatter(entry), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance)
                        'modify acl in separate roundtrip
                        If response.Exception Is Nothing AndAlso
                           Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, response.Response.ObjectId, addACEs, removeACEs)
                     End If
                     Return TransformResponse(response, state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Creates a relationship object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CreateRelationship(request As cm.Requests.createRelationship) As Generic.Response(Of cmr.createRelationshipResponse)
         Dim response = CreateRelationship(request.RepositoryId, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs)

         If response.Exception Is Nothing Then
            Return New cmr.createRelationshipResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Deletes the content stream for the specified document object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks>uses editmedia-link</remarks>
      Public Overloads Function DeleteContentStream(repositoryId As String, objectId As String,
                                                    Optional changeToken As String = Nothing) As Generic.Response(Of cmr.deleteContentStreamResponse)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, objectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumContentUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumContentUri.changeToken, changeToken)

                  Return Delete(.ToUri(), AddressOf cmr.deleteContentStreamResponse.CreateInstance)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Deletes the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function DeleteContentStream(request As cm.Requests.deleteContentStream) As Generic.Response(Of cmr.deleteContentStreamResponse)
         Return DeleteContentStream(request.RepositoryId, request.ObjectId, request.ChangeToken)
      End Function

      ''' <summary>
      ''' Deletes the specified object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="allVersions"></param>
      ''' <returns></returns>
      ''' <remarks>uses self-link</remarks>
      Public Overloads Function DeleteObject(repositoryId As String, objectId As String,
                                             Optional allVersions As Boolean = True) As Response
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Self, MediaTypes.Entry, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumObjectUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumObjectUri.allVersions, allVersions)

                  Return Delete(.ToUri())
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Deletes the specified object
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Function DeleteObject(request As cm.Requests.deleteObject) As Generic.Response(Of cmr.deleteObjectResponse)
         With DeleteObject(request.RepositoryId, request.ObjectId, Not request.AllVersions.HasValue OrElse request.AllVersions.Value)
            If .Exception Is Nothing Then
               Return New cmr.deleteObjectResponse()
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>In contrast to the implementation of DeleteTree in DotCMIS (Apache Chemistry) solely the
      ''' Folder Tree Feed Resource is used accordingly to the cmis AtomPub-Binding specifications.</remarks>
      Public Overloads Function DeleteTree(repositoryId As String, folderId As String,
                                           Optional allVersions As Boolean = True,
                                           Optional unfileObjects As Core.enumUnfileObject = Core.enumUnfileObject.delete,
                                           Optional continueOnFailure As Boolean = False) As Generic.Response(Of Messaging.failedToDelete)
         With GetLink(repositoryId, folderId, LinkRelationshipTypes.FolderTree, MediaTypes.Tree, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumFolderTreeUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumFolderTreeUri.allVersions, allVersions)
                  .Add(ServiceURIs.enumFolderTreeUri.unfileObjects, unfileObjects)
                  .Add(ServiceURIs.enumFolderTreeUri.continueOnFailure, continueOnFailure)

                  With Delete(.ToUri())
                     If .Exception Is Nothing Then
                        'no error
                        Return New Messaging.failedToDelete()
                     ElseIf .StatusCode = Net.HttpStatusCode.InternalServerError Then
                        'this exception must be returned if something failed beyond this point
                        Dim exception As System.ServiceModel.FaultException = .Exception

                        'get a list of max. 1000 child-objects which deletion failed
                        With GetLink(repositoryId, folderId, LinkRelationshipTypes.Down, MediaTypes.Tree, _objectLinks, AddressOf GetObjectLinksOnly)
                           If .Exception Is Nothing Then
                              With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                                 .Add(ServiceURIs.enumChildrenUri.filter, CmisPredefinedPropertyNames.ObjectId)
                                 .Add(ServiceURIs.enumChildrenUri.includeAllowableActions, False)
                                 .Add(ServiceURIs.enumChildrenUri.includeRelationships, Core.enumIncludeRelationships.none)
                                 .Add(ServiceURIs.enumChildrenUri.renditionFilter, "cmis:none")
                                 .Add(ServiceURIs.enumChildrenUri.includePathSegment, False)
                                 .Add(ServiceURIs.enumChildrenUri.maxItems, 1000)
                                 .Add(ServiceURIs.enumChildrenUri.skipCount, 0)

                                 With Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance)
                                    If .Exception Is Nothing Then
                                       Dim retVal As New Messaging.failedToDelete()

                                       retVal.ObjectIds = (From entry As ca.AtomEntry In .Response.Entries
                                                           Where entry IsNot Nothing
                                                           Select entry.ObjectId).ToArray()
                                       Return New Generic.Response(Of Messaging.failedToDelete)(retVal, MediaTypes.Xml, exception)
                                    Else
                                       Return exception
                                    End If
                                 End With
                              End With
                           Else
                              Return exception
                           End If
                        End With
                     Else
                        'other error
                        Return .Exception
                     End If
                  End With
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function DeleteTree(request As cm.Requests.deleteTree) As Generic.Response(Of cmr.deleteTreeResponse)
         Dim response = DeleteTree(request.RepositoryId, request.FolderId, Not request.AllVersions.HasValue OrElse request.AllVersions.Value,
                                   If(request.UnfileObjects.HasValue, request.UnfileObjects.Value, Nothing),
                                   request.ContinueOnFailure.HasValue AndAlso request.ContinueOnFailure.Value)
         If response.Exception Is Nothing Then
            Return New cmr.deleteTreeResponse() With {.FailedToDelete = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the list of allowable actions for an object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks>uses allowableactions-link</remarks>
      Public Overloads Function GetAllowableActions(repositoryId As String, objectId As String) As Generic.Response(Of Core.cmisAllowableActionsType)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.AllowableActions, MediaTypes.AllowableActions, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumAllowableActionsUri)(.Response, repositoryId)
                  Return Me.Get(.ToUri(), Function(reader)
                                             Dim retVal As New Core.cmisAllowableActionsType
                                             retVal.ReadXml(reader)
                                             Return retVal
                                          End Function)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of allowable actions for an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetAllowableActions(request As cm.Requests.getAllowableActions) As Generic.Response(Of cmr.getAllowableActionsResponse)
         Dim response = GetAllowableActions(request.RepositoryId, request.ObjectId)

         If response.Exception Is Nothing Then
            Return New cmr.getAllowableActionsResponse() With {.AllowableActions = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the content stream for the specified document object, or gets a rendition stream for a specified rendition of a document or folder object.
      ''' Note: Each CMIS protocol binding MAY provide a way for fetching a sub-range within a content stream, in a manner appropriate to that protocol
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="streamId"></param>
      ''' <param name="offset"></param>
      ''' <param name="length"></param>
      ''' <returns></returns>
      ''' <remarks>uses contentstream link-relationship</remarks>
#If HttpRequestAddRangeShortened Then
      Public Overloads Function GetContentStream(repositoryId As String, objectId As String,
                                                 Optional streamId As String = Nothing,
                                                 Optional offset As Integer? = Nothing,
                                                 Optional length As Integer? = Nothing) As Response
#Else
      Public Overloads Function GetContentStream(repositoryId As String, objectId As String,
                                                 Optional streamId As String = Nothing,
                                                 Optional offset As xs_Integer? = Nothing,
                                                 Optional length As xs_Integer? = Nothing) As Response
#End If
         Dim linkResponse As Generic.Response(Of String)

         'first chance: content link of the entry
         linkResponse = GetLink(repositoryId, objectId, LinkRelationshipTypes.ContentStream, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
         If linkResponse.Exception IsNot Nothing Then
            'second chance: edit-media-link
            'see 3.4.3.1 Existing Link Relations; edit-media:
            'When used on a CMIS document resource, this link relation MUST point to the URI for content stream of the CMIS document.
            'This URI MUST be used to set or delete the content stream. This URI MAY be used to retrieve the content stream for the document.
            linkResponse = GetLink(repositoryId, objectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, Nothing, "No content stream.")
         End If
         'contentstream link-relationship, but unspecified mediatype
         With linkResponse
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumContentUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumContentUri.streamId, streamId)
                  Return Me.Get(.ToUri(), offset, length)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the content stream for the specified document object, or gets a rendition stream for a specified rendition of a document or folder object.
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetContentStream(request As cm.Requests.getContentStream) As Generic.Response(Of cmr.getContentStreamResponse)
#If HttpRequestAddRangeShortened Then
         'Range properties (offset, length) of a HttpWebRequest are defined as Integer (see System.Net.HttpWebRequest-class),
         'therefore the xs_Integer has to be transformed
         Dim offset As Integer?
         Dim length As Integer?

         If request.Offset.HasValue Then
            offset = CInt(request.Offset.Value)
         Else
            offset = Nothing
         End If
         If request.Length.HasValue Then
            length = CInt(request.Length.Value)
         Else
            length = Nothing
         End If

         Dim response = GetContentStream(request.RepositoryId, request.ObjectId, request.StreamId, offset, length)
#Else
         Dim response = GetContentStream(request.RepositoryId, request.ObjectId, request.StreamId, request.Offset, request.Length)
#End If

         If response.Exception Is Nothing Then
            'Maybe the filename is sent via Content-Disposition
            Dim headers As System.Net.WebHeaderCollection = If(response.WebResponse Is Nothing, Nothing, response.WebResponse.Headers)
            Dim disposition As String = Nothing
            Dim fileName As String = If(headers Is Nothing, Nothing,
                                        RFC2231Helper.DecodeContentDisposition(headers(RFC2231Helper.ContentDispositionHeaderName), disposition))

            Return New cmr.getContentStreamResponse() With {.ContentStream = New cm.cmisContentStreamType(response.Stream, fileName, response.ContentType)}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the uri to get the content of a cmisDocument
      ''' </summary>
      Public Overrides Function GetContentStreamLink(repositoryId As String, objectId As String, Optional streamId As String = Nothing) As Generic.Response(Of String)
         Dim retVal As Generic.Response(Of String)

         'first chance: content link of the entry
         retVal = GetLink(repositoryId, objectId, LinkRelationshipTypes.ContentStream, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
         If retVal.Exception IsNot Nothing Then
            'second chance: edit-media-link
            'see 3.4.3.1 Existing Link Relations; edit-media:
            'When used on a CMIS document resource, this link relation MUST point to the URI for content stream of the CMIS document.
            'This URI MUST be used to set or delete the content stream. This URI MAY be used to retrieve the content stream for the document.
            retVal = GetLink(repositoryId, objectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, Nothing, "No content stream.")
         End If

         If retVal.Exception Is Nothing Then
            With New ccg.LinkUriBuilder(Of ServiceURIs.enumContentUri)(retVal.Response, repositoryId)
               .Add(ServiceURIs.enumContentUri.streamId, streamId)
               Return .ToUri().AbsoluteUri()
            End With
         Else
            Return retVal
         End If
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetObject(repositoryId As String, objectId As String,
                                          Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                          Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                          Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomEntry)
         Return GetObjectCore(repositoryId, objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions)
      End Function
      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetObject(request As cm.Requests.getObject) As Generic.Response(Of cmr.getObjectResponse)
         Dim response = GetObject(request.RepositoryId, request.ObjectId,
                                request.Filter, request.IncludeRelationships,
                                request.IncludePolicyIds, request.RenditionFilter,
                                request.IncludeACL, request.IncludeAllowableActions)
         If response.Exception Is Nothing Then
            Return New cmr.getObjectResponse() With {.Object = response.Response.Object}
         Else
            Return response.Exception
         End If
      End Function
      Private Function GetObjectCore(repositoryId As String, objectId As String,
                                     Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                     Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                     Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing,
                                     Optional returnVersion As RestAtom.enumReturnVersion? = Nothing) As Generic.Response(Of ca.AtomEntry)
         'ensure that the called repositoryId is available
         With GetRepositoryInfo(repositoryId)
            If .Exception Is Nothing Then
               Dim uriTemplate As String = .Response.UriTemplate(UriTemplates.ObjectById).Template
               Dim state As New Vendors.Vendor.State(repositoryId)

               uriTemplate = uriTemplate.ReplaceUriTemplate("id", objectId,
                                                            "filter", filter,
                                                            "includeRelationships", If(includeRelationships.HasValue, includeRelationships.Value.GetName(), Nothing),
                                                            "includePolicyIds", Convert(includePolicyIds),
                                                            "renditionFilter", renditionFilter,
                                                            "includeACL", Convert(includeACL),
                                                            "includeAllowableActions", Convert(includeAllowableActions),
                                                            "returnVersion", If(returnVersion.HasValue, returnVersion.Value.GetName(), Nothing))
               Return TransformResponse(Me.Get(New Uri(uriTemplate), AddressOf ca.AtomEntry.CreateInstance), state, objectId)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the links for the object and the cmis:objectId-property
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetObjectLinksOnly(repositoryId As String, objectId As String) As Generic.Response(Of ca.AtomEntry)
         Return GetObjectCore(repositoryId, objectId, CmisPredefinedPropertyNames.ObjectId, Core.enumIncludeRelationships.none, False, "cmis:none", False, False)
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="path"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks>uses the ObjectByPath-uritemplate</remarks>
      Public Overloads Function GetObjectByPath(repositoryId As String, path As String,
                                                Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                                Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomEntry)
         'ensure that the called repositoryId is available
         With GetRepositoryInfo(repositoryId)
            If .Exception Is Nothing Then
               Dim state As New Vendors.Vendor.State(repositoryId)
               Dim uriTemplate As String = .Response.UriTemplate(UriTemplates.ObjectByPath).Template

               uriTemplate = uriTemplate.ReplaceUriTemplate("path", path,
                                                            "filter", filter,
                                                            "includeRelationships", If(includeRelationships.HasValue, includeRelationships.Value.GetName(), Nothing),
                                                            "includePolicyIds", Convert(includePolicyIds),
                                                            "renditionFilter", renditionFilter,
                                                            "includeACL", Convert(includeACL),
                                                            "includeAllowableActions", Convert(includeAllowableActions))
               Return TransformResponse(Me.Get(New Uri(uriTemplate), AddressOf ca.AtomEntry.CreateInstance), state)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetObjectByPath(request As cm.Requests.getObjectByPath) As Generic.Response(Of cmr.getObjectByPathResponse)
         Dim response = GetObjectByPath(request.RepositoryId, request.Path, request.Filter, request.IncludeRelationships, request.IncludePolicyIds,
                                        request.RenditionFilter, request.IncludeACL, request.IncludeAllowableActions)
         If response.Exception Is Nothing Then
            Return New cmr.getObjectByPathResponse() With {.Object = response.Response.Object}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the list of properties for the object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks>A subset of GetObject</remarks>
      Public Overloads Function GetProperties(repositoryId As String, objectId As String,
                                              Optional filter As String = Nothing) As Generic.Response(Of Core.Collections.cmisPropertiesType)
         With GetObjectCore(repositoryId, objectId, filter, Core.enumIncludeRelationships.none, False, "cmis:none", False, False)
            If .Exception Is Nothing Then
               Dim cmisraObject As Core.cmisObjectType = If(.Response Is Nothing, Nothing, .Response.Object)

               Return CType(If(cmisraObject Is Nothing, Nothing, cmisraObject.Properties), Core.Collections.cmisPropertiesType)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of properties for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetProperties(request As cm.Requests.getProperties) As Generic.Response(Of cmr.getPropertiesResponse)
         Dim response = GetProperties(request.RepositoryId, request.ObjectId, request.Filter)

         If response.Exception Is Nothing Then
            Return New cmr.getPropertiesResponse() With {.Properties = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the list of associated renditions for the specified object. Only rendition attributes are returned, not rendition stream
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <returns></returns>
      ''' <remarks>A subset of GetObject</remarks>
      Public Overloads Function GetRenditions(repositoryId As String, objectId As String,
                                              Optional renditionFilter As String = Nothing,
                                              Optional maxItems As xs_Integer? = Nothing,
                                              Optional skipCount As xs_Integer? = Nothing) As Generic.Response(Of Core.cmisRenditionType())
         With GetObjectCore(repositoryId, objectId, CmisPredefinedPropertyNames.ObjectId, Core.enumIncludeRelationships.none, False, renditionFilter, False, False)
            If .Exception Is Nothing Then
               Dim cmisraObject As Core.cmisObjectType = If(.Response Is Nothing, Nothing, .Response.Object)
               Dim renditions As Core.cmisRenditionType() = If(cmisraObject Is Nothing, Nothing, cmisraObject.Renditions)

               If renditions IsNot Nothing AndAlso (maxItems.HasValue OrElse skipCount.HasValue) Then
                  Dim nMaxItems As xs_Integer = If(maxItems.HasValue, maxItems.Value, xs_Integer.MaxValue)
                  Dim nSkipCount As xs_Integer = If(skipCount.HasValue, skipCount.Value, 0)
                  Dim length As xs_Integer

#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                  length = renditions.Length
#Else
                  length = renditions.LongLength
#End If
                  If nSkipCount >= length Then
                     renditions = New Core.cmisRenditionType() {}
                  Else
                     Dim newLength As xs_Integer = Math.Min(length - nSkipCount, nMaxItems)
                     Dim cutout As Core.cmisRenditionType() = CType(Array.CreateInstance(GetType(Core.cmisRenditionType), newLength), Core.cmisRenditionType())
                     Array.Copy(renditions, nSkipCount, cutout, 0, newLength)
                     renditions = cutout
                  End If
               End If

               Return renditions
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of associated renditions for the specified object. Only rendition attributes are returned, not rendition stream
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetRenditions(request As cm.Requests.getRenditions) As Generic.Response(Of cmr.getRenditionsResponse)
         Dim response = GetRenditions(request.RepositoryId, request.ObjectId, request.RenditionFilter, request.MaxItems, request.SkipCount)

         If response.Exception Is Nothing Then
            Return New cmr.getRenditionsResponse() With {.Renditions = response.Response}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="targetFolderId"></param>
      ''' <param name="sourceFolderId"></param>
      ''' <returns></returns>
      ''' <remarks>uses the link to the children-collection of the targetFolder</remarks>
      Public Overloads Function MoveObject(repositoryId As String, objectId As String,
                                           targetFolderId As String, sourceFolderId As String) As Generic.Response(Of ca.AtomEntry)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As cm.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         ElseIf String.IsNullOrEmpty(targetFolderId) Then
            Dim cmisFault As cm.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument targetFolderId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, targetFolderId, LinkRelationshipTypes.Down, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumChildrenUri.sourceFolderId, sourceFolderId)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Post(.ToUri(), New sss.Atom10ItemFormatter(New ca.AtomEntry(objectId)),
                                                MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function MoveObject(request As cm.Requests.moveObject) As Generic.Response(Of cmr.moveObjectResponse)
         Dim response = MoveObject(request.RepositoryId, request.ObjectId, request.TargetFolderId, request.SourceFolderId)

         If response.Exception Is Nothing Then
            Return New cmr.moveObjectResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Sets the content stream for the specified document object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="contentStream"></param>
      ''' <param name="overwriteFlag"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks>uses editmedia-link</remarks>
      Public Overloads Function SetContentStream(repositoryId As String, objectId As String, contentStream As cm.cmisContentStreamType,
                                                 Optional overwriteFlag As Boolean = True,
                                                 Optional changeToken As String = Nothing) As Generic.Response(Of cmr.setContentStreamResponse)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         ElseIf contentStream Is Nothing OrElse contentStream.BinaryStream Is Nothing Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument contentStream MUST NOT be null.")
            Return cmisFault.ToFaultException()
         End If

         Dim headers As Dictionary(Of String, String)

         With GetLink(repositoryId, objectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumContentUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumContentUri.changeToken, changeToken)
                  .Add(ServiceURIs.enumContentUri.overwriteFlag, overwriteFlag)

                  If String.IsNullOrEmpty(contentStream.Filename) Then
                     headers = Nothing
                  Else
                     'If the client wishes to set a new filename, it MAY add a Content-Disposition header, which carries the new filename.
                     'The disposition type MUST be "attachment". The repository SHOULD use the "filename" parameter and SHOULD ignore all other parameters
                     'see 3.11.8.2 HTTP PUT
                     headers = New Dictionary(Of String, String) From {{RFC2231Helper.ContentDispositionHeaderName,
                                                                        RFC2231Helper.EncodeContentDisposition(contentStream.Filename)}}
                  End If
                  Return Put(.ToUri(), contentStream.BinaryStream, contentStream.MimeType, headers, AddressOf cmr.setContentStreamResponse.CreateInstance)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Sets the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function SetContentStream(request As cm.Requests.setContentStream) As Generic.Response(Of cmr.setContentStreamResponse)
         Return SetContentStream(request.RepositoryId, request.ObjectId, request.ContentStream,
                                 Not request.OverwriteFlag.HasValue OrElse request.OverwriteFlag.Value,
                                 request.ChangeToken)
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of the specified object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="properties"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function UpdateProperties(repositoryId As String, objectId As String, properties As Core.Collections.cmisPropertiesType,
                                                 Optional changeToken As String = Nothing) As Generic.Response(Of ca.AtomEntry)
         Dim cmisObject As New Core.cmisObjectType With {.Properties = properties}

         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Self, MediaTypes.Entry, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

               Try
                  With New ccg.LinkUriBuilder(Of ServiceURIs.enumObjectUri)(.Response, repositoryId)
                     .Add(ServiceURIs.enumObjectUri.changeToken, changeToken)
                     Return TransformResponse(Put(.ToUri(), New sss.Atom10ItemFormatter(New ca.AtomEntry(cmisObject)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state)
                  End With
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Updates properties and secondary types of the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function UpdateProperties(request As cm.Requests.updateProperties) As Generic.Response(Of cmr.updatePropertiesResponse)
         Dim response = UpdateProperties(request.RepositoryId, request.ObjectId, request.Properties, request.ChangeToken)

         If response.Exception Is Nothing Then
            Return New cmr.updatePropertiesResponse() With {.ObjectId = response.Response.ObjectId, .ChangeToken = response.Response.ChangeToken}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="allVersions"></param>
      ''' <returns></returns>
      ''' <remarks>uses children-collection of specified folder</remarks>
      Public Overloads Function AddObjectToFolder(repositoryId As String, objectId As String, folderId As String,
                                                  Optional allVersions As Boolean = True) As Generic.Response(Of ca.AtomEntry)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, folderId, LinkRelationshipTypes.Down, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumChildrenUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumChildrenUri.allVersions, allVersions)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Post(.ToUri(), New sss.Atom10ItemFormatter(New ca.AtomEntry(objectId)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state, writeToCache:=False)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function AddObjectToFolder(request As cm.Requests.addObjectToFolder) As Generic.Response(Of cmr.addObjectToFolderResponse)
         Dim response = AddObjectToFolder(request.RepositoryId, request.ObjectId, request.FolderId, Not request.AllVersions.HasValue OrElse request.AllVersions.Value)

         If response.Exception Is Nothing Then
            Return New cmr.addObjectToFolderResponse() With {.Object = If(response.Response Is Nothing, Nothing, response.Response.Object)}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="folderId"></param>
      ''' <returns></returns>
      ''' <remarks>uses unfiled-collectionInfo</remarks>
      Public Overloads Function RemoveObjectFromFolder(repositoryId As String, objectId As String,
                                                       Optional folderId As String = Nothing) As Generic.Response(Of ca.AtomEntry)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetCollectionInfo(repositoryId, CollectionInfos.Unfiled)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumUnfiledUri)(.Response.Link.OriginalString, repositoryId)
                  .Add(ServiceURIs.enumUnfiledUri.folderId, folderId)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Post(.ToUri(), New sss.Atom10ItemFormatter(New ca.AtomEntry(objectId)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state, writeToCache:=False)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function RemoveObjectFromFolder(request As cm.Requests.removeObjectFromFolder) As Generic.Response(Of cmr.removeObjectFromFolderResponse)
         Dim response = RemoveObjectFromFolder(request.RepositoryId, request.ObjectId, request.FolderId)

         If response.Exception Is Nothing Then
            Return New cmr.removeObjectFromFolderResponse() With {.Object = If(response.Response Is Nothing, Nothing, response.Response.Object)}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Gets a list of content changes. This service is intended to be used by search crawlers or other applications that need to
      ''' efficiently understand what has changed in the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>uses changes-link of the specified repository</remarks>
      Public Overloads Function GetContentChanges(repositoryId As String,
                                                  Optional filter As String = Nothing,
                                                  Optional changeLogToken As String = Nothing,
                                                  Optional includeProperties As Boolean = False,
                                                  Optional includePolicyIds As Boolean = False,
                                                  Optional includeACL As Boolean? = Nothing,
                                                  Optional maxItems As xs_Integer? = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetRepositoryLink(repositoryId, LinkRelationshipTypes.Changes)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumChangesUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumChangesUri.filter, filter)
                  .Add(ServiceURIs.enumChangesUri.changeLogToken, changeLogToken)
                  .Add(ServiceURIs.enumChangesUri.includeProperties, includeProperties)
                  .Add(ServiceURIs.enumChangesUri.includePolicyIds, includePolicyIds)
                  .Add(ServiceURIs.enumChangesUri.includeACL, includeACL)
                  .Add(ServiceURIs.enumChangesUri.maxItems, maxItems)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state, False)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets a list of content changes. This service is intended to be used by search crawlers or other applications that need to
      ''' efficiently understand what has changed in the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>Notice:
      ''' The ChangeLog Token is specified in the URI specified by the paging link notations.
      ''' Through the AtomPub binding it is not possible to retrieve the ChangeLog Token from the URIs</remarks>
      Public Overrides Function GetContentChanges(request As cm.Requests.getContentChanges) As Generic.Response(Of cmr.getContentChangesResponse)
         Dim response = GetContentChanges(request.RepositoryId, request.Filter, request.ChangeLogToken,
                                          request.IncludeProperties.HasValue AndAlso request.IncludeProperties.Value,
                                          request.IncludePolicyIds.HasValue AndAlso request.IncludePolicyIds.Value,
                                          request.IncludeACL.HasValue AndAlso request.IncludeACL.Value,
                                          request.MaxItems)
         If response.Exception Is Nothing Then
            'through this binding it is not possible to retrieve the ChangeLog Token from the URIs
            Return New cmr.getContentChangesResponse() With {.Objects = response.Response, .ChangeLogToken = Nothing}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Executes a CMIS query statement against the contents of the repository
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="statement"></param>
      ''' <param name="searchAllVersions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <returns></returns>
      ''' <remarks>uses query-collectionInfo; another way to implement this method is to use
      ''' the query-uritemplate as described in 3.7.1.4 Query</remarks>
      Public Overloads Function Query(repositoryId As String, statement As String,
                                      Optional searchAllVersions As Boolean? = Nothing,
                                      Optional includeRelationships As Core.enumIncludeRelationships = Core.enumIncludeRelationships.none,
                                      Optional renditionFilter As String = Nothing,
                                      Optional includeAllowableActions As Boolean = False,
                                      Optional maxItems As xs_Integer? = Nothing,
                                      Optional skipCount As xs_Integer? = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetCollectionInfo(repositoryId, CollectionInfos.Query)
            If .Exception Is Nothing Then
               Dim uri As Uri = .Response.Link

               With New Core.cmisQueryType() With {.Statement = statement, .SearchAllVersions = searchAllVersions,
                                                   .IncludeRelationships = includeRelationships,
                                                   .RenditionFilter = renditionFilter,
                                                   .IncludeAllowableActions = includeAllowableActions,
                                                   .MaxItems = maxItems, .SkipCount = skipCount}
                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Post(uri, .Self, MediaTypes.Query, Nothing, AddressOf ca.AtomFeed.CreateInstance), state, False)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Executes a CMIS query statement against the contents of the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function Query(request As cm.Requests.query) As Generic.Response(Of cmr.queryResponse)
         Dim response = Query(request.RepositoryId, request.Statement, request.SearchAllVersions,
                              If(request.IncludeRelationships.HasValue, request.IncludeRelationships.Value, Core.enumIncludeRelationships.none),
                              request.RenditionFilter, request.IncludeRelationships.HasValue AndAlso request.IncludeAllowableActions.Value,
                              request.MaxItems, request.SkipCount)
         If response.Exception Is Nothing Then
            Return New cmr.queryResponse() With {.Objects = response.Response}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="pwcLinkRequired">If False this method uses the self-link if no workingcopy-link exists. Caution: this behaviour is
      ''' equivalent to a DeleteObject(repositoryId, objectId)-call and should be used for non-compliant repositories only. If the given
      ''' objectId-parameter contains a private working copy id this parameter should be False or better: call DeleteObject().</param>
      ''' <returns></returns>
      ''' <remarks>Following the implementation in DotCMIS (Apache Chemistry) this method prefers the usage of the workingcopy-link, but
      ''' also supports the deletion of the object if neither a workingcopy is found nor is required</remarks>
      Public Overloads Function CancelCheckOut(repositoryId As String, objectId As String,
                                               Optional pwcLinkRequired As Boolean = True) As Response
         Dim link As String

         'object must exists!
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Self, MediaTypes.Entry, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception IsNot Nothing Then
               Return .Exception
            ElseIf Not pwcLinkRequired Then
               link = .Response
            Else
               link = Nothing
            End If
         End With
         'prefer working copy link if available (workaround for non-compliant repositories)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.WorkingCopy, MediaTypes.Entry, _objectLinks, Nothing)
            If .Exception Is Nothing Then
               link = .Response
            End If
         End With

         Return Delete(New Uri(link))
      End Function
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CancelCheckOut(request As cm.Requests.cancelCheckOut) As Generic.Response(Of cmr.cancelCheckOutResponse)
         Dim response = CancelCheckOut(request.RepositoryId, request.ObjectId, request.PWCLinkRequired)

         If response.Exception Is Nothing Then
            Return New cmr.cancelCheckOutResponse()
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Checks-in the Private Working Copy document
      ''' </summary>
      ''' <param name="pwcLinkRequired">If False this method uses the self-link if no workingcopy-link exists. This is basically
      ''' equivalent to an UpdateProperties()-call.</param>
      ''' <returns></returns>
      ''' <remarks>Following the implementation in DotCMIS (Apache Chemistry) this method prefers the usage of the workingcopy-link, but
      ''' also supports update the properties of the object if neither a workingcopy is found nor is required</remarks>
      Public Overloads Function CheckIn(repositoryId As String, objectId As String,
                                        Optional major As Boolean = True,
                                        Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                        Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                                        Optional checkinComment As String = Nothing,
                                        Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                        Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                        Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing,
                                        Optional pwcLinkRequired As Boolean = False) As Generic.Response(Of ca.AtomEntry)
         Dim link As String

         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         'object must exists!
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Self, MediaTypes.Entry, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception IsNot Nothing Then
               Return .Exception
            ElseIf Not pwcLinkRequired Then
               link = .Response
            Else
               link = Nothing
            End If
         End With
         'prefer working copy link if available (workaround for non-compliant repositories)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.WorkingCopy, MediaTypes.Entry, _objectLinks, Nothing)
            If .Exception Is Nothing Then link = .Response
         End With

         With New ccg.LinkUriBuilder(Of ServiceURIs.enumObjectUri)(link, repositoryId)
            .Add(ServiceURIs.enumObjectUri.major, major)
            .Add(ServiceURIs.enumObjectUri.checkin, True)
            .Add(ServiceURIs.enumObjectUri.checkinComment, checkinComment)

            Dim cmisraObject As New Core.cmisObjectType With {.PolicyIds = policies, .Properties = properties}
            Dim headers As Dictionary(Of String, String) = Nothing
            Dim content As RestAtom.cmisContentType
            Dim state As Vendors.Vendor.State = TransformRequest(repositoryId, properties)

            Try
               If contentStream IsNot Nothing AndAlso contentStream.BinaryStream IsNot Nothing AndAlso Not String.IsNullOrEmpty(contentStream.MimeType) Then
                  'transmit ContentStreamFileName, ContentStreamLength as property
                  If properties IsNot Nothing Then contentStream.ExtendProperties(properties)
                  content = CType(contentStream, RestAtom.cmisContentType)
                  If Not String.IsNullOrEmpty(contentStream.Filename) Then
                     'If the client wishes to set a new filename, it MAY add a Content-Disposition header, which carries the new filename.
                     'The disposition type MUST be "attachment". The repository SHOULD use the "filename" parameter and SHOULD ignore all other parameters
                     'see 3.11.8.2 HTTP PUT
                     headers = New Dictionary(Of String, String) From {{RFC2231Helper.ContentDispositionHeaderName,
                                                                        RFC2231Helper.EncodeContentDisposition(contentStream.Filename)}}
                  End If
               Else
                  content = Nothing
               End If

               Dim retVal As Generic.Response(Of ca.AtomEntry) = Put(.ToUri(), New sss.Atom10ItemFormatter(New ca.AtomEntry(cmisraObject, content)),
                                                                     MediaTypes.Entry, headers, AddressOf ca.AtomEntry.CreateInstance)
               If retVal.Exception Is Nothing Then
                  TransformResponse(retVal, state)
                  If Not (addACEs Is Nothing OrElse removeACEs Is Nothing) Then ApplyAcl(repositoryId, retVal.Response.ObjectId, addACEs, removeACEs)
               End If
               Return retVal
            Finally
               If state IsNot Nothing Then state.Rollback()
            End Try
         End With
      End Function
      ''' <summary>
      ''' Checks-in the Private Working Copy document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CheckIn(request As cm.Requests.checkIn) As Generic.Response(Of cmr.checkInResponse)
         Dim response = CheckIn(request.RepositoryId, request.ObjectId, Not request.Major.HasValue OrElse request.Major.Value,
                                request.Properties, request.ContentStream, request.CheckinComment, request.Policies,
                                request.AddACEs, request.RemoveACEs, request.PWCLinkRequired)
         If response.Exception Is Nothing Then
            Return New cmr.checkInResponse() With {.ObjectId = response.Response.ObjectId}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Create a private working copy (PWC) of the document
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks>uses checkedOut-collectionInfo</remarks>
      Public Overloads Function CheckOut(repositoryId As String, objectId As String) As Generic.Response(Of ca.AtomEntry)
         If String.IsNullOrEmpty(objectId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument objectId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetCollectionInfo(repositoryId, CollectionInfos.CheckedOut)
            If .Exception Is Nothing Then
               Dim state As New Vendors.Vendor.State(repositoryId)
               Return TransformResponse(Post(.Response.Link, New sss.Atom10ItemFormatter(New ca.AtomEntry(objectId)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Create a private working copy (PWC) of the document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function CheckOut(request As cm.Requests.checkOut) As Generic.Response(Of cmr.checkOutResponse)
         Dim response = CheckOut(request.RepositoryId, request.ObjectId)

         If response.Exception Is Nothing Then
            Dim originalContentLink As String
            Dim pwcContentLink As String

            'get the contentLink of the document
            With GetLink(request.RepositoryId, request.ObjectId, LinkRelationshipTypes.ContentStream, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
               If .Exception Is Nothing Then
                  originalContentLink = .Response
               Else
                  originalContentLink = GetLink(request.RepositoryId, request.ObjectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, Nothing).Response
               End If
            End With
            'get the contentLink of the pwc
            With GetLink(request.RepositoryId, response.Response.ObjectId, LinkRelationshipTypes.ContentStream, Nothing, _objectLinks, AddressOf GetObjectLinksOnly)
               If .Exception Is Nothing Then
                  pwcContentLink = .Response
               Else
                  pwcContentLink = GetLink(request.RepositoryId, response.Response.ObjectId, LinkRelationshipTypes.EditMedia, Nothing, _objectLinks, Nothing).Response
               End If
            End With

            Return New cmr.checkOutResponse() With {.ObjectId = response.Response.ObjectId, .ContentCopied = originalContentLink <> pwcContentLink}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the list of all document objects in the specified version series, sorted by cmis:creationDate descending
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>If a Private Working Copy exists for the version series and the caller has permissions to access it,
      ''' then it MUST be returned as the first object in the result list</remarks>
      Public Overloads Function GetAllVersions(repositoryId As String, objectId As String,
                                               versionSeriesId As String,
                                               Optional filter As String = Nothing,
                                               Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.VersionHistory, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumAllVersionsUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumAllVersionsUri.versionSeriesId, versionSeriesId)
                  .Add(ServiceURIs.enumAllVersionsUri.filter, filter)
                  .Add(ServiceURIs.enumAllVersionsUri.includeAllowableActions, includeAllowableActions)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Returns the list of all document objects in the specified version series, sorted by cmis:creationDate descending
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetAllVersions(request As cm.Requests.getAllVersions) As Generic.Response(Of cmr.getAllVersionsResponse)
         Dim response = GetAllVersions(request.RepositoryId, request.ObjectId, Nothing, request.Filter, request.IncludeAllowableActions)

         If response.Exception Is Nothing Then
            Return New cmr.getAllVersionsResponse() With {.Objects = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                      Let cmisObject As Core.cmisObjectType = entry.Object
                                                                      Where cmisObject IsNot Nothing
                                                                      Select cmisObject).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetObjectOfLatestVersion(repositoryId As String, objectIdOrVersionSeriesId As String,
                                                         Optional major As Boolean = False,
                                                         Optional filter As String = Nothing,
                                                         Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                         Optional includePolicyIds As Boolean? = Nothing,
                                                         Optional renditionFilter As String = Nothing,
                                                         Optional includeACL As Boolean? = Nothing,
                                                         Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomEntry)
         Return GetObjectCore(repositoryId, objectIdOrVersionSeriesId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL,
                              includeAllowableActions, If(major, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest))
      End Function
      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetObjectOfLatestVersion(request As cm.Requests.getObjectOfLatestVersion) As Generic.Response(Of cmr.getObjectOfLatestVersionResponse)
         Dim response = GetObjectOfLatestVersion(request.RepositoryId, request.ObjectId, request.Major.HasValue AndAlso request.Major.Value,
                                                 request.Filter, request.IncludeRelationships, request.IncludePolicyIds,
                                                 request.RenditionFilter, request.IncludeACL, request.IncludeAllowableActions)
         If response.Exception Is Nothing Then
            Return New cmr.getObjectOfLatestVersionResponse() With {.Object = response.Response.Object}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>A subset of GetObjectOfLatestVersion()</remarks>
      Public Overloads Function GetPropertiesOfLatestVersion(repositoryId As String, objectIdOrVersionSeriesId As String,
                                                             Optional major As Boolean = False,
                                                             Optional filter As String = Nothing) As Generic.Response(Of Core.Collections.cmisPropertiesType)
         With GetObjectOfLatestVersion(repositoryId, objectIdOrVersionSeriesId, major, filter)
            If .Exception Is Nothing Then
               Dim cmisraObject As Core.cmisObjectType = If(.Response Is Nothing, Nothing, .Response.Object)
               Return CType(If(cmisraObject Is Nothing, Nothing, cmisraObject.Properties), Core.Collections.cmisPropertiesType)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetPropertiesOfLatestVersion(request As cm.Requests.getPropertiesOfLatestVersion) As Generic.Response(Of cmr.getPropertiesOfLatestVersionResponse)
         Dim response = GetPropertiesOfLatestVersion(request.RepositoryId, request.ObjectId, request.Major.HasValue AndAlso request.Major.Value, request.Filter)

         If response.Exception Is Nothing Then
            Return New cmr.getPropertiesOfLatestVersionResponse() With {.Properties = response.Response}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Relationship"
      ''' <summary>
      ''' Gets all or a subset of relationships associated with an independent object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>uses relationships-link</remarks>
      Public Overloads Function GetObjectRelationships(repositoryId As String, objectId As String,
                                                       Optional includeSubRelationshipTypes As Boolean = False,
                                                       Optional relationshipDirection As Core.enumRelationshipDirection = Core.enumRelationshipDirection.source,
                                                       Optional typeId As String = Nothing,
                                                       Optional maxItems As xs_Integer? = Nothing,
                                                       Optional skipCount As xs_Integer? = Nothing,
                                                       Optional filter As String = Nothing,
                                                       Optional includeAllowableActions As Boolean? = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Relationships, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumRelationshipsUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumRelationshipsUri.includeSubRelationshipTypes, includeSubRelationshipTypes)
                  .Add(ServiceURIs.enumRelationshipsUri.relationshipDirection, relationshipDirection)
                  .Add(ServiceURIs.enumRelationshipsUri.typeId, typeId)
                  .Add(ServiceURIs.enumRelationshipsUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumRelationshipsUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumRelationshipsUri.filter, filter)
                  .Add(ServiceURIs.enumRelationshipsUri.includeAllowableActions, includeAllowableActions)

                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets all or a subset of relationships associated with an independent object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetObjectRelationships(request As cm.Requests.getObjectRelationships) As Generic.Response(Of cmr.getObjectRelationshipsResponse)
         Dim response = GetObjectRelationships(request.RepositoryId, request.ObjectId,
                                               request.IncludeSubRelationshipTypes.HasValue AndAlso request.IncludeSubRelationshipTypes.Value,
                                               If(request.RelationshipDirection.HasValue, request.RelationshipDirection.Value, Core.enumRelationshipDirection.source),
                                               request.TypeId, request.MaxItems, request.SkipCount, request.Filter, request.IncludeAllowableActions)
         If response.Exception Is Nothing Then
            Return New cmr.getObjectRelationshipsResponse() With {.Objects = response.Response}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a specified policy to an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function ApplyPolicy(repositoryId As String, policyId As String, objectId As String) As Generic.Response(Of ca.AtomEntry)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Policies, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               Dim state As New Vendors.Vendor.State(repositoryId)
               Return TransformResponse(Post(New Uri(.Response), New sss.Atom10ItemFormatter(New ca.AtomEntry(policyId)), MediaTypes.Entry, Nothing, AddressOf ca.AtomEntry.CreateInstance), state, writeToCache:=False)
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Applies a specified policy to an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function ApplyPolicy(request As cm.Requests.applyPolicy) As Generic.Response(Of cmr.applyPolicyResponse)
         Dim response = ApplyPolicy(request.RepositoryId, request.PolicyId, request.ObjectId)

         If response.Exception Is Nothing Then
            Return New cmr.applyPolicyResponse() With {.Object = If(response.Response Is Nothing, Nothing, response.Response.Object)}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Gets the list of policies currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overloads Function GetAppliedPolicies(repositoryId As String, objectId As String,
                                                   Optional filter As String = Nothing) As Generic.Response(Of ca.AtomFeed)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Policies, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumPoliciesUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumPoliciesUri.filter, filter)
                  Dim state As New Vendors.Vendor.State(repositoryId)
                  Return TransformResponse(Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance), state, False)
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Gets the list of policies currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetAppliedPolicies(request As cm.Requests.getAppliedPolicies) As Generic.Response(Of cmr.getAppliedPoliciesResponse)
         Dim response = GetAppliedPolicies(request.RepositoryId, request.ObjectId, request.Filter)

         If response.Exception Is Nothing Then
            Return New cmr.getAppliedPoliciesResponse() With {.Objects = (From entry As ca.AtomEntry In If(response.Response.Entries, New List(Of ca.AtomEntry))
                                                                          Where entry IsNot Nothing
                                                                          Select entry.Object).ToArray()}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Removes a specified policy from an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>This is the only collection where the URI’s of the objects in the collection MUST be specific to that collection.
      ''' A DELETE on the policy object in the collection is a removal of the policy from the object NOT a deletion of the policy object itself</remarks>
      Public Overloads Function RemovePolicy(repositoryId As String, policyId As String, objectId As String) As Response
         Dim policiesUri As Uri

         If String.IsNullOrEmpty(policyId) Then
            Dim cmisFault As Messaging.cmisFaultType = New cm.cmisFaultType(Net.HttpStatusCode.BadRequest, cm.enumServiceException.invalidArgument, "Argument policyId MUST be set.")
            Return cmisFault.ToFaultException()
         End If

         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Policies, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               'store the uri to the policies collection
               policiesUri = New Uri(.Response)

               'get the current list of policies
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumPoliciesUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumPoliciesUri.filter, CmisPredefinedPropertyNames.ObjectId)
                  With Me.Get(.ToUri(), AddressOf ca.AtomFeed.CreateInstance)
                     If .Exception Is Nothing Then
                        Dim feed As ca.AtomFeed = .Response
                        Dim entry As ca.AtomEntry = If(feed Is Nothing, Nothing, feed.Entry(policyId))
                        Dim link As sss.SyndicationLink = If(entry Is Nothing, Nothing, entry.Link(LinkRelationshipTypes.Self))

                        'the self-link of the policy-object can be used if the link belongs to the policies collection
                        If link IsNot Nothing AndAlso link.Uri IsNot Nothing AndAlso
                           String.Compare(link.Uri.Authority, policiesUri.Authority, True) = 0 AndAlso
                           String.Compare(link.Uri.LocalPath, policiesUri.LocalPath, True) = 0 Then
                           Return Delete(link.Uri)
                        End If
                     Else
                        Return .Exception
                     End If
                  End With
               End With

               'second chance: use the policies-collection-link
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumPoliciesUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumPoliciesUri.policyId, policyId)
                  Return Delete(.ToUri())
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Removes a specified policy from an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function RemovePolicy(request As cm.Requests.removePolicy) As Generic.Response(Of cmr.removePolicyResponse)
         Dim response = RemovePolicy(request.RepositoryId, request.PolicyId, request.ObjectId)

         If response.Exception Is Nothing Then
            Return New cmr.removePolicyResponse()
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Acl"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of an object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="addACEs"></param>
      ''' <param name="removeACEs"></param>
      ''' <param name="aclPropagation"></param>
      ''' <returns></returns>
      ''' <remarks>uses acl-link</remarks>
      Public Overloads Function ApplyAcl(repositoryId As String, objectId As String,
                                         addACEs As Core.Security.cmisAccessControlListType, removeACEs As Core.Security.cmisAccessControlListType,
                                         Optional aclPropagation As Core.enumACLPropagation = Core.enumACLPropagation.repositorydetermined) As Generic.Response(Of Core.Security.cmisAccessControlListType)
         'get the current acl
         With GetAcl(repositoryId, objectId, False)
            If .Exception Is Nothing Then
               Dim currentAcl As Core.Security.cmisAccessControlListType = .Response

               With GetLink(repositoryId, objectId, LinkRelationshipTypes.Acl, MediaTypes.Acl, _objectLinks, AddressOf GetObjectLinksOnly)
                  If .Exception Is Nothing Then
                     With New ccg.LinkUriBuilder(Of ServiceURIs.enumACLUri)(.Response, repositoryId)
                        .Add(ServiceURIs.enumACLUri.aclPropagation, aclPropagation)
                        'modify acl
                        Return Put(.ToUri(), currentAcl.Join(addACEs, removeACEs), MediaTypes.Acl, Nothing,
                                   AddressOf Serialization.XmlSerializable.GenericXmlSerializableFactory(Of Core.Security.cmisAccessControlListType))
                     End With
                  Else
                     Return .Exception
                  End If
               End With
            Else
               Return .Self
            End If
         End With
      End Function
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function ApplyAcl(request As cm.Requests.applyACL) As Generic.Response(Of cmr.applyACLResponse)
         Dim response = ApplyAcl(request.RepositoryId, request.ObjectId, request.AddACEs, request.RemoveACEs,
                                 If(request.ACLPropagation.HasValue, request.ACLPropagation.Value, Core.enumACLPropagation.repositorydetermined))
         If response.Exception Is Nothing Then
            '2.2.10.1.3 Outputs (applyACL)
            'exact: An indicator that the ACL returned fully describes the permission for this object.
            'That is, there are no other security constraints applied to this object. Not provided defaults to FALSE. 
            Return New cmr.applyACLResponse() With {.ACL = New Messaging.cmisACLType() With {.ACL = response.Response,
                                                                                             .Exact = If(response.Response Is Nothing, Nothing, response.Response.IsExact)}}
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Get the ACL currently applied to the specified object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="onlyBasicPermissions"></param>
      ''' <returns></returns>
      ''' <remarks>uses acl-link</remarks>
      Public Overloads Function GetAcl(repositoryId As String, objectId As String,
                                       Optional onlyBasicPermissions As Boolean = True) As Generic.Response(Of Core.Security.cmisAccessControlListType)
         With GetLink(repositoryId, objectId, LinkRelationshipTypes.Acl, MediaTypes.Acl, _objectLinks, AddressOf GetObjectLinksOnly)
            If .Exception Is Nothing Then
               With New ccg.LinkUriBuilder(Of ServiceURIs.enumACLUri)(.Response, repositoryId)
                  .Add(ServiceURIs.enumACLUri.onlyBasicPermissions, onlyBasicPermissions)
                  Return Me.Get(.ToUri(), AddressOf Serialization.XmlSerializable.GenericXmlSerializableFactory(Of Core.Security.cmisAccessControlListType))
               End With
            Else
               Return .Exception
            End If
         End With
      End Function
      ''' <summary>
      ''' Get the ACL currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function GetAcl(request As cm.Requests.getACL) As Generic.Response(Of cmr.getACLResponse)
         Dim response = GetAcl(request.RepositoryId, request.ObjectId, Not request.OnlyBasicPermissions.HasValue OrElse request.OnlyBasicPermissions.Value)

         If response.Exception Is Nothing Then
            '2.2.10.2.2 Outputs (getACL)
            'exact: An indicator that the ACL returned fully describes the permission for this object.
            'That is, there are no other security constraints applied to this object. Not provided defaults to FALSE. 
            Return New cmr.getACLResponse() With {.ACL = New cm.cmisACLType() With {.ACL = response.Response,
                                                                                    .Exact = If(response.Response Is Nothing, Nothing, response.Response.IsExact)}}
         Else
            Return response.Exception
         End If
      End Function
#End Region

#Region "Miscellaneous (ICmisClient)"
      Public Overrides ReadOnly Property ClientType As enumClientType
         Get
            Return enumClientType.AtomPub
         End Get
      End Property

      ''' <summary>
      ''' Logs out from repository
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Sub Logout(repositoryId As String)
         Dim uri As New Uri(ServiceURIs.GetServiceUri(_serviceDocUri.OriginalString,
                                                      ServiceURIs.enumRepositoriesUri.repositoryId Or ServiceURIs.enumRepositoriesUri.logout).ReplaceUri("repositoryId", repositoryId, "logout", "true"))
         Me.Get(uri)
      End Sub

      ''' <summary>
      ''' Tells the server, that this client is still alive
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Sub Ping(repositoryId As String)
         Dim uri As New Uri(ServiceURIs.GetServiceUri(_serviceDocUri.OriginalString,
                                                      ServiceURIs.enumRepositoriesUri.repositoryId Or ServiceURIs.enumRepositoriesUri.ping).ReplaceUri("repositoryId", repositoryId, "ping", "true"))
         Me.Get(uri)
      End Sub

      ''' <summary>
      ''' There is no succinct representation of properties defined in AtomPub Binding
      ''' </summary>
      Public Overrides ReadOnly Property SupportsSuccinct As Boolean
         Get
            Return False
         End Get
      End Property
      ''' <summary>
      ''' There is no support of token parameters in AtomPub Binding
      ''' </summary>
      Public Overrides ReadOnly Property SupportsToken As Boolean
         Get
            Return False
         End Get
      End Property

      ''' <summary>
      ''' UserAgent-name of current instance
      ''' </summary>
      Protected Overrides ReadOnly Property UserAgent As String
         Get
            Return "Brügmann Software CmisObjectModel.Client.AtomPub.CmisClient"
         End Get
      End Property
#End Region

#Region "Caches"
      ''' <summary>
      ''' If folderId is set the function returns the link to the Children-collection
      ''' otherwise it returns the link to the Unfiled-collection
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="acceptRequest"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetChildrenOrUnfiledLink(repositoryId As String, folderId As String, ByRef acceptRequest As Boolean) As Generic.Response(Of String)
         If String.IsNullOrEmpty(folderId) Then
            'Unfiled-collection
            With GetCollectionInfo(repositoryId, CollectionInfos.Unfiled)
               If .Exception Is Nothing Then
                  'check for mediatype Request
                  acceptRequest = CollectionAccepts(.Response, MediaTypes.Request)
                  Return .Response.Link.OriginalString
               Else
                  Return .Exception
               End If
            End With
         Else
            'Children-collection
            With GetLink(repositoryId, folderId, LinkRelationshipTypes.Down, MediaTypes.Feed, _objectLinks, AddressOf GetObjectLinksOnly)
               If .Exception Is Nothing Then
                  'check for mediatype Request
                  acceptRequest = CollectionAccepts(repositoryId, CollectionInfos.Root, MediaTypes.Request)
                  Return .Self
               Else
                  Return .Self
               End If
            End With
         End If
      End Function

      ''' <summary>
      ''' Access to specified collectioninfo of specified repository
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="collectionType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetCollectionInfo(repositoryId As String, collectionType As String) As Generic.Response(Of ca.AtomCollectionInfo)
         With GetRepositoryInfo(repositoryId)
            If .Exception Is Nothing Then
               Dim retVal As ca.AtomCollectionInfo = .Response.CollectionInfo(collectionType)

               If retVal Is Nothing Then
                  Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.BadRequest, Messaging.enumServiceException.notSupported, "CollectionInfo '" & collectionType & "' not supported.")
                  Return cmisFault.ToFaultException()
               Else
                  Return retVal
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Access to links of an object or type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <param name="relationshipType"></param>
      ''' <param name="mediaType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetLink(repositoryId As String, id As String, relationshipType As String, mediaType As String,
                               linkCache As Collections.Generic.Cache(Of String, String),
                               getObjectOrType As Func(Of String, String, Generic.Response(Of ca.AtomEntry)),
                               Optional defaultErrorMessage As String = "Nothing wrong. Either this is a bug or threading issue.") As Generic.Response(Of String)
         SyncLock linkCache
            'first chance
            Dim link As String = linkCache.Item(If(repositoryId, ""), If(id, ""), relationshipType, mediaType)

            If String.IsNullOrEmpty(link) AndAlso getObjectOrType IsNot Nothing Then
               'try to get the object and fill the link-cache with object-links
               With getObjectOrType.Invoke(repositoryId, id)
                  If .Exception Is Nothing Then
                     'second chance
                     link = linkCache.Item(If(repositoryId, ""), If(id, ""), If(relationshipType, ""), If(mediaType, ""))
                  Else
                     Return .Exception
                  End If
               End With
            End If

            If String.IsNullOrEmpty(link) Then
               Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.NotFound, Messaging.enumServiceException.objectNotFound, Nothing)

               Select Case linkCache.ValidPathDepth(If(repositoryId, ""), If(id, ""), relationshipType, mediaType)
                  Case 0
                     cmisFault.Message = "Unknown repository."
                  Case 1
                     cmisFault.Message = "Unknown " & If(linkCache Is _objectLinks, "object.", "type.")
                  Case 2
                     cmisFault.Message = "Relationship not supported by the repository for this " & If(linkCache Is _objectLinks, "object.", "type.")
                  Case 3
                     cmisFault.Message = "No link with matching media type."
                  Case Else
                     cmisFault.Message = defaultErrorMessage
               End Select

               Return cmisFault.ToFaultException()
            Else
               Return link
            End If
         End SyncLock
      End Function

      ''' <summary>
      ''' Access to repository-links
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="relationshipType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetRepositoryLink(repositoryId As String, relationshipType As String) As Generic.Response(Of String)
         Dim link As String

         'try to get the object and fill the link-cache with object-links
         With GetRepositoryInfo(repositoryId)
            If .Exception Is Nothing Then
               link = .Response.Link(relationshipType)
            Else
               Return .Exception
            End If
         End With

         If String.IsNullOrEmpty(link) Then
            Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.NotFound, Messaging.enumServiceException.objectNotFound, "Relationship not supported by the repository")
            Return cmisFault.ToFaultException()
         Else
            Return link
         End If
      End Function

      Private _objectLinks As New Collections.Generic.Cache(Of String, String)(AppSettings.CacheSizeObjects << 4,
                                                                               AppSettings.CacheLeaseTime, True)


      Private Shared _typeLinks As New Collections.Generic.Cache(Of String, String)(AppSettings.CacheSizeTypes << 4,
                                                                                    AppSettings.CacheLeaseTime, True)

      ''' <summary>
      ''' Stores the links of objects and typedefinitions
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub WriteToCache(repositoryId As String, id As String,
                               entry As ca.AtomEntry,
                               linkCache As Collections.Generic.Cache(Of String, String))
         'precedence: response if cmisProperty cmis:objectId is returned
         If entry IsNot Nothing Then id = entry.ObjectId.NVL(id)
         If Not (entry Is Nothing OrElse String.IsNullOrEmpty(repositoryId) OrElse String.IsNullOrEmpty(id)) Then
            SyncLock linkCache
               Dim link As ca.AtomLink

               'remove all cache-entries with a path starting with <repositoryId, id>
               linkCache.RemoveAll(repositoryId, id)
               For Each link In entry.Links
                  linkCache.Item(repositoryId, id, If(link.RelationshipType, ""), If(link.MediaType, "")) = link.Uri.AbsoluteUri
               Next
               link = entry.ContentLink
               If link IsNot Nothing Then linkCache.Item(repositoryId, id, If(link.RelationshipType, ""), If(link.MediaType, "")) = link.Uri.AbsoluteUri

               'recursive
               Dim children As ca.AtomFeed = entry.Children
               If children IsNot Nothing Then WriteToCache(repositoryId, children, linkCache)
            End SyncLock
         End If
      End Sub
      ''' <summary>
      ''' Stores the links of each object in feed
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="feed"></param>
      ''' <param name="linkCache"></param>
      ''' <remarks></remarks>
      Private Sub WriteToCache(repositoryId As String, feed As ca.AtomFeed, linkCache As Collections.Generic.Cache(Of String, String))
         If Not (feed Is Nothing OrElse String.IsNullOrEmpty(repositoryId)) Then
            Dim entries As List(Of ca.AtomEntry) = feed.Entries

            If entries IsNot Nothing Then
               SyncLock linkCache
                  For Each entry As ca.AtomEntry In entries
                     Dim objectId As String = entry.ObjectId

                     If Not String.IsNullOrEmpty(objectId) Then
                        WriteToCache(repositoryId, objectId, entry, linkCache)
                     End If
                  Next
               End SyncLock
            End If
         End If
      End Sub

      ''' <summary>
      ''' Stores often used data from the workspace in the cache
      ''' </summary>
      ''' <param name="ws"></param>
      ''' <remarks></remarks>
      Private Sub WriteToCache(ws As ca.AtomWorkspace)
         'repository itself
         RepositoryInfo(If(ws.RepositoryInfo.RepositoryId, "")) = ws
      End Sub
#End Region

#Region "Requests"

#Region "Vendor specific and value mapping"
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values on a successful response
      ''' </summary>
      ''' <param name="response"></param>
      ''' <param name="state"></param>
      ''' <param name="cacheId"></param>
      ''' <param name="writeToCache"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Overloads Function TransformResponse(response As Generic.Response(Of ca.AtomEntry), state As Vendors.Vendor.State,
                                                   Optional cacheId As String = Nothing, Optional writeToCache As Boolean = True) As Generic.Response(Of ca.AtomEntry)
         If response.Exception Is Nothing Then
            Dim repositoryId = state.RepositoryId
            Dim collector As New ca.AtomEntryCollector
            Dim propertyCollections As Core.Collections.cmisPropertiesType() =
               (From entry As ca.AtomEntry In collector.Collect(response)
                Let cmisObject As Core.cmisObjectType = If(entry Is Nothing, Nothing, entry.Object)
                Let propertyCollection As Core.Collections.cmisPropertiesType = If(cmisObject Is Nothing, Nothing, cmisObject.Properties)
                Where propertyCollection IsNot Nothing
                Select propertyCollection).ToArray()
            'vendor specifics, value mapping
            MyBase.TransformResponse(state, propertyCollections)
            If writeToCache Then Me.WriteToCache(repositoryId, cacheId, response.Response, _objectLinks)
         End If

         Return response
      End Function
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values on a successful response
      ''' </summary>
      ''' <param name="response"></param>
      ''' <param name="state"></param>
      ''' <param name="writeToCache"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Overloads Function TransformResponse(response As Generic.Response(Of ca.AtomFeed), state As Vendors.Vendor.State,
                                                   Optional writeToCache As Boolean = True) As Generic.Response(Of ca.AtomFeed)
         If response.Exception Is Nothing Then
            Dim repositoryId = state.RepositoryId
            Dim collector As New ca.AtomEntryCollector
            Dim propertyCollections As Core.Collections.cmisPropertiesType() =
               (From entry As ca.AtomEntry In collector.Collect(response)
                Let cmisObject As Core.cmisObjectType = If(entry Is Nothing, Nothing, entry.Object)
                Let propertyCollection As Core.Collections.cmisPropertiesType = If(cmisObject Is Nothing, Nothing, cmisObject.Properties)
                Where propertyCollection IsNot Nothing
                Select propertyCollection).ToArray()
            'vendor specifics, value mapping
            MyBase.TransformResponse(state, propertyCollections)
            If writeToCache Then Me.WriteToCache(repositoryId, response.Response, _objectLinks)
         End If

         Return response
      End Function
#End Region

#Region "Delete-Requests"
      Private Function Delete(uri As Uri) As Response
         Return Request(uri, "DELETE", Nothing, Nothing, Nothing)
      End Function

      Private Function Delete(Of TResponse)(uri As Uri, responseFactory As Func(Of sx.XmlReader, TResponse)) As Generic.Response(Of TResponse)
         Return Request(Of TResponse)(uri, "DELETE", CType(Nothing, IO.Stream), Nothing, Nothing, responseFactory)
      End Function
#End Region

#Region "Get-Requests"
#If HttpRequestAddRangeShortened Then
      Private Function [Get](uri As Uri,
                             Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Response
#Else
      Private Function [Get](uri As Uri,
                             Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Response
#End If
         Return Request(uri, "GET", Nothing, Nothing, Nothing, offset, length)
      End Function

#If HttpRequestAddRangeShortened Then
      Private Function [Get](Of TResponse)(uri As Uri, responseFactory As Func(Of sx.XmlReader, TResponse),
                                           Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Generic.Response(Of TResponse)
#Else
      Private Function [Get](Of TResponse)(uri As Uri, responseFactory As Func(Of sx.XmlReader, TResponse),
                                           Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Generic.Response(Of TResponse)
#End If
         Return Request(Of TResponse)(uri, "GET", CType(Nothing, IO.Stream), Nothing, Nothing, responseFactory, offset, length)
      End Function
#End Region

#Region "Post-Requests"
      Private Function Post(Of TContent As {New, sxs.IXmlSerializable}, TResponse)(uri As Uri, content As TContent, contentType As String,
                                                                                   headers As Dictionary(Of String, String),
                                                                                   responseFactory As Func(Of sx.XmlReader, TResponse)) As Generic.Response(Of TResponse)
         Return Request(Of TContent, TResponse)(uri, "POST", content, contentType, headers, responseFactory)
      End Function
#End Region

#Region "Put-Requests"
      Private Function Put(Of TResponse)(uri As Uri, content As IO.Stream, contentType As String, headers As Dictionary(Of String, String),
                                         responseFactory As Func(Of sx.XmlReader, TResponse)) As Generic.Response(Of TResponse)
         Return Request(Of TResponse)(uri, "PUT", content, contentType, headers, responseFactory)
      End Function

      Private Function Put(Of TContent As {New, sxs.IXmlSerializable}, TResponse)(uri As Uri, content As TContent,
                                                                                   contentType As String, headers As Dictionary(Of String, String),
                                                                                   responseFactory As Func(Of sx.XmlReader, TResponse)) As Generic.Response(Of TResponse)
         Return Request(Of TContent, TResponse)(uri, "PUT", content, contentType, headers, responseFactory)
      End Function
#End Region

#If HttpRequestAddRangeShortened Then
      Private Function Request(uri As Uri, method As String, content As IO.Stream, contentType As String, headers As Dictionary(Of String, String),
                               Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Response
#Else
      Private Function Request(uri As Uri, method As String, content As IO.Stream, contentType As String, headers As Dictionary(Of String, String),
                               Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Response
#End If
         Dim contentWriter As Action(Of IO.Stream) = If(content Is Nothing, Nothing,
                                                        New Action(Of IO.Stream)(Sub(requestStream As IO.Stream)
                                                                                    content.CopyTo(requestStream)
                                                                                 End Sub))
         Try
            Return New Response(GetResponse(uri, method, contentWriter, contentType, headers, offset, length))
         Catch ex As sn.WebException
            Return New Response(ex)
         End Try
      End Function

#If HttpRequestAddRangeShortened Then
      Private Function Request(Of TResponse)(uri As Uri, method As String, content As IO.Stream, contentType As String, headers As Dictionary(Of String, String),
                                             responseFactory As Func(Of sx.XmlReader, TResponse),
                                             Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Generic.Response(Of TResponse)
#Else
      Private Function Request(Of TResponse)(uri As Uri, method As String, content As IO.Stream, contentType As String, headers As Dictionary(Of String, String),
                                             responseFactory As Func(Of sx.XmlReader, TResponse),
                                             Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Generic.Response(Of TResponse)
#End If
         Dim contentWriter As Action(Of IO.Stream) = If(content Is Nothing, Nothing,
                                                        New Action(Of IO.Stream)(Sub(requestStream As IO.Stream)
                                                                                    content.CopyTo(requestStream)
                                                                                 End Sub))
         Try
            Return New Generic.Response(Of TResponse)(GetResponse(uri, method, contentWriter, contentType, headers, offset, length), responseFactory)
         Catch ex As sn.WebException
            Return New Generic.Response(Of TResponse)(ex)
         End Try
      End Function

#If HttpRequestAddRangeShortened Then
      Private Function Request(Of TContent As {New, sxs.IXmlSerializable}, TResponse)(uri As Uri, method As String, content As TContent,
                                                                                      contentType As String, headers As Dictionary(Of String, String),
                                                                                      responseFactory As Func(Of sx.XmlReader, TResponse),
                                                                                      Optional offset As Integer? = Nothing,
                                                                                      Optional length As Integer? = Nothing) As Generic.Response(Of TResponse)
#Else
      Private Function Request(Of TContent As {New, sxs.IXmlSerializable}, TResponse)(uri As Uri, method As String, content As TContent,
                                                                                      contentType As String, headers As Dictionary(Of String, String),
                                                                                      responseFactory As Func(Of sx.XmlReader, TResponse),
                                                                                      Optional offset As xs_Integer? = Nothing,
                                                                                      Optional length As xs_Integer? = Nothing) As Generic.Response(Of TResponse)
#End If
         Dim contentWriter As Action(Of IO.Stream)

         If content Is Nothing Then
            contentWriter = Nothing
         Else
            contentWriter = Sub(requestStream As IO.Stream)
                               Dim xmlDoc As sx.XmlDocument
                               Dim serializer As New sxs.XmlSerializer(GetType(sx.XmlDocument))

                               If GetType(Messaging.Requests.RequestBase).IsAssignableFrom(GetType(TContent)) Then
                                  'the root namespace of Messaging.Requests.RequestBase instances must be changed from cmism to cmisw
                                  xmlDoc = Serialization.SerializationHelper.ToXmlDocument(content, _requestBaseAttributeOverrides)
                               Else
                                  xmlDoc = Serialization.SerializationHelper.ToXmlDocument(content)
                               End If

                               Using writer As sx.XmlWriter = sx.XmlWriter.Create(requestStream,
                                                                                  New sx.XmlWriterSettings() With {.Encoding = New System.Text.UTF8Encoding(False)})
                                  serializer.Serialize(writer, xmlDoc)
                               End Using
                            End Sub
         End If

         Try
            Return New Generic.Response(Of TResponse)(GetResponse(uri, method, contentWriter, contentType, headers, offset, length), responseFactory)
         Catch ex As sn.WebException
            Return New Generic.Response(Of TResponse)(ex)
         End Try
      End Function

#End Region

      ''' <summary>
      ''' Returns True if the collectionInfo specified by repositoryId and collectionType exists and accepts the specified mediaType
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="collectionType"></param>
      ''' <param name="mediaType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CollectionAccepts(repositoryId As String, collectionType As String, mediaType As String) As Boolean
         'check for mediatype
         With GetCollectionInfo(repositoryId, collectionType)
            Return If(.Exception Is Nothing, CollectionAccepts(.Response, mediaType), False)
         End With
      End Function
      ''' <summary>
      ''' Returns True if the collectionInfo defines mediaType as accepted mediatype
      ''' </summary>
      ''' <param name="collectionInfo"></param>
      ''' <param name="mediaType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CollectionAccepts(collectionInfo As ca.AtomCollectionInfo, mediaType As String) As Boolean
         If collectionInfo IsNot Nothing Then
            For Each acceptedMediaType As String In collectionInfo.Accepts
               If String.Compare(acceptedMediaType, mediaType, True) = 0 Then Return True
            Next
         End If

         Return False
      End Function

   End Class
End Namespace