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
Imports cc = CmisObjectModel.Core
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cs = CmisObjectModel.ServiceModel
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.ServiceModel
   ''' <summary>
   ''' Baseclass for classes that implement Contracts.ICmisServicesImpl
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class CmisServiceImplBase
      Implements Contracts.ICmisServicesImpl

#Region "Constructors"
      Protected Sub New(baseUri As Uri)
         If baseUri Is Nothing OrElse baseUri.OriginalString.EndsWith("/"c) Then
            _baseUri = baseUri
         Else
            _baseUri = New Uri(baseUri.OriginalString & "/")
         End If
      End Sub
#End Region

#Region "Helper-classes"
      ''' <summary>
      ''' Creator of needed cmis-links
      ''' </summary>
      ''' <remarks></remarks>
      Private Class LinkFactory

         Private _id As String
         Private _owner As CmisServiceImplBase
         Private _repositoryId As String
         Private _selfLinkUri As Uri
         Public Sub New(owner As CmisServiceImplBase, repositoryId As String, id As String, selfLinkUri As Uri)
            _owner = owner
            _repositoryId = repositoryId
            _id = id
         End Sub

         ''' <summary>
         ''' Appends navigation-links (first/next/previous/last) if the list only returns a part of the complete cmis list.
         ''' </summary>
         ''' <param name="links"></param>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <remarks></remarks>
         Private Sub AppendNavigationLinks(links As List(Of ca.AtomLink),
                                           currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                           nSkipCount As xs_Integer?, nMaxItems As xs_Integer?)
            'request may be incomplete
            If hasMoreItems OrElse nMaxItems.HasValue Then
               Dim skipCount As xs_Integer = If(nSkipCount.HasValue, nSkipCount.Value, 0)
               Dim maxItems As xs_Integer = If(nMaxItems.HasValue, nMaxItems.Value, currentEntries)
               Dim numCount As xs_Integer = If(nNumCount.HasValue, nNumCount.Value, If(hasMoreItems, xs_Integer.MaxValue, skipCount + currentEntries))
               Dim queryStrings As New List(Of String)
               Dim regEx As New System.Text.RegularExpressions.Regex("\A(maxitems|skipcount)\Z")
               Dim currentRequestUri As Uri = CmisServiceImplBase.CurrentRequestUri
               Dim absolutePath As String = currentRequestUri.AbsolutePath
               Dim uriTemplate As String
               Dim intValue As xs_Integer

               'remove maxItems and skipCount ...
               With System.Web.HttpUtility.ParseQueryString(currentRequestUri.Query)
                  For Each key As String In .AllKeys
                     Dim match As System.Text.RegularExpressions.Match = If(key Is Nothing, Nothing, regEx.Match(key))
                     If match Is Nothing OrElse Not match.Success Then
                        queryStrings.Add(key & "=" & System.Uri.EscapeDataString(.Item(key)))
                     End If
                  Next
               End With
               '... and put them to the end
               queryStrings.Add("maxItems=" & maxItems.ToString() & "&skipCount={skipCount}")
               If Not absolutePath.EndsWith("/"c) Then absolutePath &= "/"
               uriTemplate = String.Format("{0}{1}{2}{3}?{4}", currentRequestUri.Scheme, Uri.SchemeDelimiter, currentRequestUri.Authority, absolutePath, String.Join("&", queryStrings.ToArray()))

               'first
               links.Add(New ca.AtomLink(New Uri(uriTemplate.ReplaceUri("skipCount", "0")), Constants.LinkRelationshipTypes.First, Constants.MediaTypes.Feed))
               'next (only if there are more objects following the current entries)
               intValue = skipCount + currentEntries
               If intValue < numCount Then
                  links.Add(New ca.AtomLink(New Uri(uriTemplate.ReplaceUri("skipCount", intValue.ToString())), Constants.LinkRelationshipTypes.Next, Constants.MediaTypes.Feed))
               End If
               'previous (only if objects in the current feed has been skipped)
               If skipCount > 0 Then
                  intValue = Math.Max(0, skipCount - maxItems)
                  links.Add(New ca.AtomLink(New Uri(uriTemplate.ReplaceUri("skipCount", intValue.ToString())), Constants.LinkRelationshipTypes.Previous, Constants.MediaTypes.Feed))
               End If
               'last (only if the value of numCount is defined)
               If nNumCount.HasValue Then
                  intValue = Math.Max(0, numCount - maxItems)
                  links.Add(New ca.AtomLink(New Uri(uriTemplate.ReplaceUri("skipCount", intValue.ToString())), Constants.LinkRelationshipTypes.Last, Constants.MediaTypes.Feed))
               End If
            End If
         End Sub

         ''' <summary>
         ''' Creates links that MUST be returned from the GetAllVersions()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateAllVersionsLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()

            'via
            retVal.Add(New ca.AtomLink(New Uri(_owner._baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'first, next, previous, last (defined in 3.10.5 All Versions Feed; how?)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetCheckedOutDocs()-request
         ''' </summary>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateCheckedOutLinks(currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                               nSkipCount As xs_Integer?, nMaxItems As xs_Integer?) As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()

            AppendNavigationLinks(retVal, currentEntries, nNumCount, hasMoreItems, nSkipCount, nMaxItems)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetChildren()-request
         ''' </summary>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateChildrenLinks(currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                             nSkipCount As xs_Integer?, nMaxItems As xs_Integer?) As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri
            Dim repositoryInfo As Core.cmisRepositoryInfoType = _owner.RepositoryInfo(_repositoryId)

            'via
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'up
            If _id <> repositoryInfo.RootFolderId Then
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _id)),
                                               LinkRelationshipTypes.Up, MediaTypes.Entry))
            End If
            'down
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.DescendantsUri(ServiceURIs.enumDescendantsUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Down, MediaTypes.Tree))
            'foldertree
            If repositoryInfo.Capabilities.CapabilityGetFolderTree Then
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.FolderTreeUri(ServiceURIs.enumFolderTreeUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _id)),
                                               LinkRelationshipTypes.FolderTree, MediaTypes.Feed))
            End If
            'first, next, previous, last
            AppendNavigationLinks(retVal, currentEntries, nNumCount, hasMoreItems, nSkipCount, nMaxItems)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetContentChanges()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateContentChangesLinks() As List(Of ca.AtomLink)
            Return CreateLinks()
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetDescendants()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateDescendantsLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri
            Dim repositoryInfo As Core.cmisRepositoryInfoType = _owner.RepositoryInfo(_repositoryId)

            'via
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'up
            If _id <> repositoryInfo.RootFolderId Then
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _id)),
                                               LinkRelationshipTypes.Up, MediaTypes.Entry))
            End If
            'down
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ChildrenUri(ServiceURIs.enumChildrenUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Down, MediaTypes.Feed))
            'foldertree
            If repositoryInfo.Capabilities.CapabilityGetFolderTree Then
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.FolderTreeUri(ServiceURIs.enumFolderTreeUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _id)),
                                               LinkRelationshipTypes.FolderTree, MediaTypes.Feed))
            End If

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetFolderTree()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateFolderTreeLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri
            Dim repositoryInfo As Core.cmisRepositoryInfoType = _owner.RepositoryInfo(_repositoryId)

            'via
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'up
            If _id <> repositoryInfo.RootFolderId Then
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _id)),
                                               LinkRelationshipTypes.Up, MediaTypes.Entry))
            End If
            'down
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ChildrenUri(ServiceURIs.enumChildrenUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Down, MediaTypes.Feed))
            'down
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.DescendantsUri(ServiceURIs.enumDescendantsUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Down, MediaTypes.Tree))

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from all cmis-requests with feed or tree results
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private Function CreateLinks() As List(Of ca.AtomLink)
            Dim retVal As New List(Of ca.AtomLink) From {
               New ca.AtomLink(New Uri(_owner._baseUri, ServiceURIs.GetRepositoryInfo.ReplaceUri("repositoryId", _repositoryId)),
                                    Constants.LinkRelationshipTypes.Service, Constants.MediaTypes.Service)}

            If _selfLinkUri IsNot Nothing Then
               'according to guidelines 3.5.1 Feeds
               retVal.Add(New ca.AtomLink(_selfLinkUri, Constants.LinkRelationshipTypes.Self, MediaTypes.Feed))
            End If

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetObjectParents()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateObjectParentLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri

            'via (not defined in 3.10.1 Object Parents Feed; forgotten?)
            retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'first, next, previous, last (defined in 3.10.1 Object Parents Feed; how?)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetObjectRelationships()-request
         ''' </summary>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateObjectRelationshipsLinks(currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                                        nSkipCount As xs_Integer?, nMaxItems As xs_Integer?) As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()

            'first, next, previous, last
            AppendNavigationLinks(retVal, currentEntries, nNumCount, hasMoreItems, nSkipCount, nMaxItems)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetAppliedPolicies()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreatePoliciesLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()

            'via
            retVal.Add(New ca.AtomLink(New Uri(_owner._baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                            LinkRelationshipTypes.Via, MediaTypes.Entry))
            'first, next, previous, last (defined in 3.9.3 Policies Collection; how?)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the Query()-request
         ''' </summary>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateQueryLinks(currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                          nSkipCount As xs_Integer?, nMaxItems As xs_Integer?) As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()

            'first, next, previous, last
            AppendNavigationLinks(retVal, currentEntries, nNumCount, hasMoreItems, nSkipCount, nMaxItems)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetTypeChildren()-request
         ''' </summary>
         ''' <param name="currentEntries"></param>
         ''' <param name="nNumCount"></param>
         ''' <param name="hasMoreItems"></param>
         ''' <param name="nSkipCount"></param>
         ''' <param name="nMaxItems"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateTypeChildrenLinks(currentEntries As xs_Integer, nNumCount As xs_Integer?, hasMoreItems As Boolean,
                                                 nSkipCount As xs_Integer?, nMaxItems As xs_Integer?) As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri

            If Not String.IsNullOrEmpty(_id) Then
               'via
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                               LinkRelationshipTypes.Via, MediaTypes.Entry))
               'down
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeDescendantsUri(ServiceURIs.enumTypeDescendantsUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                               LinkRelationshipTypes.Down, MediaTypes.Tree))
               'up (only if the currentType is not a baseType)
               Dim parentTypeId As String = _owner.GetParentTypeId(_repositoryId, _id)
               If Not String.IsNullOrEmpty(parentTypeId) Then
                  retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", parentTypeId)),
                                                  LinkRelationshipTypes.Up, MediaTypes.Entry))
               End If
            Else
               'down
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeDescendantsUri(ServiceURIs.enumTypeDescendantsUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                               LinkRelationshipTypes.Down, MediaTypes.Tree))
            End If
            'first, next, previous, last
            AppendNavigationLinks(retVal, currentEntries, nNumCount, hasMoreItems, nSkipCount, nMaxItems)

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetTypeDescendants()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateTypeDescendantsLinks() As List(Of ca.AtomLink)
            Dim retVal As List(Of ca.AtomLink) = CreateLinks()
            Dim baseUri As Uri = _owner._baseUri

            If Not String.IsNullOrEmpty(_id) Then
               'via
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                               LinkRelationshipTypes.Via, MediaTypes.Entry))
               'up (only if the currentType is not a baseType)
               Dim parentTypeId As String = _owner.GetParentTypeId(_repositoryId, _id)
               If Not String.IsNullOrEmpty(parentTypeId) Then
                  retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", parentTypeId)),
                                                  LinkRelationshipTypes.Up, MediaTypes.Entry))
               End If
               'down
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypesUri(ServiceURIs.enumTypesUri.typeId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
                                               LinkRelationshipTypes.Down, MediaTypes.Feed))
            Else
               'down
               retVal.Add(New ca.AtomLink(New Uri(baseUri, ServiceURIs.TypesUri(ServiceURIs.enumTypesUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                               LinkRelationshipTypes.Down, MediaTypes.Feed))
            End If

            Return retVal
         End Function

         ''' <summary>
         ''' Creates links that MUST be returned from the GetUnfiledObjects()-request
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateUnfiledObjectsLinks() As List(Of ca.AtomLink)
            Return CreateLinks()
         End Function
      End Class

      ''' <summary>
      ''' LinkUriBuilder for uris based on existing URI and relative URI
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <remarks></remarks>
      Private Class SelfLinkUriBuilder(Of TEnum As Structure)
         Inherits ccg.LinkUriBuilder(Of TEnum)

         Private _baseUri As Uri
         Private _factory As Func(Of TEnum, String)

         Public Sub New(baseUri As Uri, repositoryId As String, factory As Func(Of TEnum, String))
            _baseUri = baseUri
            _factory = factory
            _searchAndReplace = New List(Of String) From {"repositoryId", repositoryId}
         End Sub

         Public Overrides Function ToUri() As Uri
            Return New Uri(_baseUri, _factory.Invoke(CType(CObj(_flags), TEnum)).ReplaceUri(_searchAndReplace.ToArray()))
         End Function
      End Class
#End Region

#Region "Repository-Section (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function CreateType(repositoryId As String, newType As ccdt.cmisTypeDefinitionType) As ccg.Result(Of ccdt.cmisTypeDefinitionType) Implements Contracts.ICmisServicesImpl.CreateType
      Protected MustOverride Function DeleteType(repositoryId As String, typeId As String) As Exception Implements Contracts.ICmisServicesImpl.DeleteType
      Protected MustOverride Function GetRepositories() As ccg.Result(Of Core.cmisRepositoryInfoType()) Implements Contracts.ICmisServicesImpl.GetRepositories
      Protected MustOverride Function GetRepositoryInfo(repositoryId As String) As ccg.Result(Of Core.cmisRepositoryInfoType) Implements Contracts.ICmisServicesImpl.GetRepositoryInfo
      Protected MustOverride Function GetTypeChildren(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean,
                                                      maxItems As xs_Integer?, skipCount As xs_Integer?) As ccg.Result(Of cm.cmisTypeDefinitionListType) Implements Contracts.ICmisServicesImpl.GetTypeChildren
      Protected MustOverride Function GetTypeDefinition(repositoryId As String, typeId As String) As ccg.Result(Of ccdt.cmisTypeDefinitionType) Implements Contracts.ICmisServicesImpl.GetTypeDefinition

      Protected MustOverride Function GetTypeDescendants(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean, depth As xs_Integer?) As ccg.Result(Of cm.cmisTypeContainer) Implements Contracts.ICmisServicesImpl.GetTypeDescendants
      Protected Overridable Function Login(repositoryId As String, authorization As String) As ccg.Result(Of System.Net.HttpStatusCode) Implements Contracts.ICmisServicesImpl.Login
         If String.IsNullOrEmpty(authorization) Then
            Return Net.HttpStatusCode.Forbidden
         Else
            Try
               ssw.WebOperationContext.Current.IncomingRequest.Headers.Remove(sn.HttpRequestHeader.Authorization)
               ssw.WebOperationContext.Current.IncomingRequest.Headers.Add(sn.HttpRequestHeader.Authorization, authorization)
               Return If(GetRepositoryInfo(repositoryId) Is Nothing, Net.HttpStatusCode.Forbidden, Net.HttpStatusCode.OK)
            Catch ex As Exception
               Return ex
            End Try
         End If
      End Function
      <Obsolete("Treat as a mustoverride function.", True)>
      Protected Overridable Function Logout(repositoryId As String) As ccg.Result(Of System.Net.HttpStatusCode) Implements Contracts.ICmisServicesImpl.Logout
         Return Net.HttpStatusCode.OK
      End Function
      <Obsolete("Treat as a mustoverride function.", True)>
      Protected Overridable Function Ping(repositoryId As String) As ccg.Result(Of System.Net.HttpStatusCode) Implements Contracts.ICmisServicesImpl.Ping
         Return Net.HttpStatusCode.OK
      End Function
      Protected MustOverride Function UpdateType(repositoryId As String, modifiedType As ccdt.cmisTypeDefinitionType) As ccg.Result(Of ccdt.cmisTypeDefinitionType) Implements Contracts.ICmisServicesImpl.UpdateType
#End Region

#Region "Navigation (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function GetCheckedOutDocs(repositoryId As String, folderId As String, filter As String,
                                                        maxItems As xs_Integer?, skipCount As xs_Integer?, renditionFilter As String,
                                                        includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.GetCheckedOutDocs
      Protected MustOverride Function GetChildren(repositoryId As String, folderId As String,
                                                  maxItems As xs_Integer?, skipCount As xs_Integer?, filter As String,
                                                  includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                                                  renditionFilter As String, orderBy As String, includePathSegment As Boolean) As ccg.Result(Of cs.cmisObjectInFolderListType) Implements Contracts.ICmisServicesImpl.GetChildren
      Protected MustOverride Function GetDescendants(repositoryId As String, folderId As String, filter As String, depth As xs_Integer?, includeAllowableActions As Boolean?,
                                      includeRelationships As Core.enumIncludeRelationships?, renditionFilter As String, includePathSegment As Boolean) As ccg.Result(Of cs.cmisObjectInFolderContainerType) Implements Contracts.ICmisServicesImpl.GetDescendants
      Protected MustOverride Function GetFolderParent(repositoryId As String, folderId As String, filter As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.GetFolderParent
      Protected MustOverride Function GetFolderTree(repositoryId As String, folderId As String, filter As String, depth As xs_Integer?,
                                                    includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                                                    includePathSegment As Boolean, renditionFilter As String) As ccg.Result(Of cs.cmisObjectInFolderContainerType) Implements Contracts.ICmisServicesImpl.GetFolderTree
      Protected MustOverride Function GetObjectParents(repositoryId As String, objectId As String, filter As String,
                                                       includeAllowableActions As Boolean?,
                                                       includeRelationships As Core.enumIncludeRelationships?,
                                                       renditionFilter As String, includeRelativePathSegment As Boolean?) As ccg.Result(Of cs.cmisObjectParentsType()) Implements Contracts.ICmisServicesImpl.GetObjectParents
      Protected MustOverride Function GetUnfiledObjects(repositoryId As String, maxItems As xs_Integer?, skipCount As xs_Integer?,
                                                        filter As String, includeAllowableActions As Boolean?,
                                                        includeRelationships As Core.enumIncludeRelationships?,
                                                        renditionFilter As String, orderBy As String) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.GetUnfiledObjects
#End Region

#Region "Object (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function AppendContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream,
                                                          mimeType As String, fileName As String,
                                                          isLastChunk As Boolean, changeToken As String) As ccg.Result(Of Messaging.Responses.setContentStreamResponse) Implements Contracts.ICmisServicesImpl.AppendContentStream
      Protected MustOverride Function BulkUpdateProperties(repositoryId As String, data As Core.cmisBulkUpdateType) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.BulkUpdateProperties
      Protected MustOverride Function CreateDocument(repositoryId As String, newDocument As Core.cmisObjectType,
                                                     folderId As String, content As Messaging.cmisContentStreamType,
                                                     versioningState As Core.enumVersioningState?,
                                                     addACEs As Core.Security.cmisAccessControlListType,
                                                     removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreateDocument
      Protected MustOverride Function CreateDocumentFromSource(repositoryId As String, sourceId As String,
                                                               properties As Core.Collections.cmisPropertiesType,
                                                               folderId As String, versioningState As Core.enumVersioningState?,
                                                               policies As String(),
                                                               addACEs As Core.Security.cmisAccessControlListType,
                                                               removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreateDocumentFromSource
      Protected MustOverride Function CreateFolder(repositoryId As String, newFolder As Core.cmisObjectType, parentFolderId As String,
                                                   addACEs As Core.Security.cmisAccessControlListType,
                                                   removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreateFolder
      Protected MustOverride Function CreateItem(repositoryId As String, newItem As Core.cmisObjectType, folderId As String,
                                                 addACEs As Core.Security.cmisAccessControlListType,
                                                 removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreateItem
      Protected MustOverride Function CreatePolicy(repositoryId As String, newPolicy As Core.cmisObjectType, folderId As String,
                                                   addACEs As Core.Security.cmisAccessControlListType,
                                                   removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreatePolicy
      Protected MustOverride Function CreateRelationship(repositoryId As String, newRelationship As Core.cmisObjectType,
                                                         addACEs As Core.Security.cmisAccessControlListType,
                                                         removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CreateRelationship
      Protected MustOverride Function DeleteContentStream(repositoryId As String, objectId As String, changeToken As String) As ccg.Result(Of cm.Responses.deleteContentStreamResponse) Implements Contracts.ICmisServicesImpl.DeleteContentStream
      Protected MustOverride Function DeleteObject(repositoryId As String, objectId As String, allVersions As Boolean) As Exception Implements Contracts.ICmisServicesImpl.DeleteObject
      Protected MustOverride Function DeleteTree(repositoryId As String, folderId As String, allVersions As Boolean, unfileObjects As Core.enumUnfileObject?, continueOnFailure As Boolean) As ccg.Result(Of cm.Responses.deleteTreeResponse) Implements Contracts.ICmisServicesImpl.DeleteTree
      Protected MustOverride Function GetAllowableActions(repositoryId As String, id As String) As ccg.Result(Of Core.cmisAllowableActionsType) Implements Contracts.ICmisServicesImpl.GetAllowableActions
      Protected MustOverride Function GetContentStream(repositoryId As String, objectId As String, streamId As String) As ccg.Result(Of cm.cmisContentStreamType) Implements Contracts.ICmisServicesImpl.GetContentStream
      Protected MustOverride Function GetObject(repositoryId As String, objectId As String, filter As String, includeRelationships As Core.enumIncludeRelationships?,
                                                includePolicyIds As Boolean?, renditionFilter As String, includeACL As Boolean?, includeAllowableActions As Boolean?,
                                                returnVersion As RestAtom.enumReturnVersion?, privateWorkingCopy As Boolean?) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.GetObject
      Protected MustOverride Function GetObjectByPath(repositoryId As String, path As String, filter As String, includeAllowableActions As Boolean?,
                                                      includePolicyIds As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                                                      includeACL As Boolean?, renditionFilter As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.GetObjectByPath
      Protected MustOverride Function MoveObject(repositoryId As String, objectId As String, targetFolderId As String, sourceFolderId As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.MoveObject
      Protected MustOverride Function SetContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream,
                                                       mimeType As String, fileName As String,
                                                       overwriteFlag As Boolean, changeToken As String) As ccg.Result(Of Messaging.Responses.setContentStreamResponse) Implements Contracts.ICmisServicesImpl.SetContentStream
      Protected MustOverride Function UpdateProperties(repositoryId As String, objectId As String, properties As Core.Collections.cmisPropertiesType, changeToken As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.UpdateProperties
#End Region

#Region "Multi (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function AddObjectToFolder(repositoryId As String, objectId As String, folderId As String, allVersions As Boolean) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.AddObjectToFolder
      Protected MustOverride Function RemoveObjectFromFolder(repositoryId As String, objectId As String, folderId As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.RemoveObjectFromFolder
#End Region

#Region "Discovery (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function GetContentChanges(repositoryId As String, filter As String, maxItems As xs_Integer?,
                                                        includeACL As Boolean?, includePolicyIds As Boolean, includeProperties As Boolean,
                                                        ByRef changeLogToken As String) As ccg.Result(Of cs.getContentChanges) Implements Contracts.ICmisServicesImpl.GetContentChanges
      Protected MustOverride Function Query(repositoryId As String, q As String, searchAllVersions As Boolean, includeRelationships As Core.enumIncludeRelationships?,
                                            renditionFilter As String, includeAllowableActions As Boolean, maxItems As xs_Integer?, skipCount As xs_Integer?) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.Query
#End Region

#Region "Versioning (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function CancelCheckOut(repositoryId As String, objectId As String) As Exception Implements Contracts.ICmisServicesImpl.CancelCheckOut
      Protected MustOverride Function CheckIn(repositoryId As String, objectId As String,
                                              properties As Core.Collections.cmisPropertiesType, policies As String(), content As Messaging.cmisContentStreamType,
                                              major As Boolean, checkInComment As String,
                                              Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                              Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CheckIn
      Protected MustOverride Function CheckOut(repositoryId As String, objectId As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.CheckOut
      Protected MustOverride Function GetAllVersions(repositoryId As String, objectId As String, versionSeriesId As String, filter As String, includeAllowableActions As Boolean?) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.GetAllVersions
#End Region

#Region "Relationships (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function GetObjectRelationships(repositoryId As String, objectId As String, includeSubRelationshipTypes As Boolean,
                                                             relationshipDirection As Core.enumRelationshipDirection?,
                                                             typeId As String, maxItems As xs_Integer?, skipCount As xs_Integer?,
                                                             filter As String, includeAllowableActions As Boolean?) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.GetObjectRelationships
#End Region

#Region "Policy (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function ApplyPolicy(repositoryId As String, objectId As String, policyId As String) As ccg.Result(Of cs.cmisObjectType) Implements Contracts.ICmisServicesImpl.ApplyPolicy
      Protected MustOverride Function GetAppliedPolicies(repositoryId As String, objectId As String, filter As String) As ccg.Result(Of cs.cmisObjectListType) Implements Contracts.ICmisServicesImpl.GetAppliedPolicies
      Protected MustOverride Function RemovePolicy(repositoryId As String, objectId As String, policyId As String) As Exception Implements Contracts.ICmisServicesImpl.RemovePolicy
#End Region

#Region "ACL (3.6 Resources Overview in the cmis documentation file)"
      Protected MustOverride Function ApplyACL(repositoryId As String, objectId As String,
                                               addACEs As Core.Security.cmisAccessControlListType, removeACEs As Core.Security.cmisAccessControlListType,
                                               aclPropagation As Core.enumACLPropagation) As ccg.Result(Of Core.Security.cmisAccessControlListType) Implements Contracts.ICmisServicesImpl.ApplyACL
      Protected MustOverride Function GetACL(repositoryId As String, objectId As String, onlyBasicPermissions As Boolean) As ccg.Result(Of Core.Security.cmisAccessControlListType) Implements Contracts.ICmisServicesImpl.GetACL
#End Region

      ''' <summary>
      ''' Adds a cookie to the outgoing response
      ''' </summary>
      ''' <remarks></remarks>
      Protected Friend Shared Sub AddOutgoingCookie(cookie As HttpCookie)
         If cookie IsNot Nothing Then
            CurrentOutgoingCookies.AddOrReplace(cookie)
         End If
      End Sub

      Protected _baseUri As Uri
      ''' <summary>
      ''' Returns the baseUri of the service
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property BaseUri As Uri Implements Contracts.ICmisServicesImpl.BaseUri
         Get
            Return _baseUri
         End Get
      End Property

      ''' <summary>
      ''' Creates a WebFaultException to inform the client about the server-fault
      ''' </summary>
      ''' <param name="serviceException"></param>
      ''' <param name="message"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Shared Function CreateException(serviceException As Messaging.enumServiceException, message As String) As Exception
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(serviceException)

         Return New ssw.WebFaultException(Of cm.cmisFaultType)(New cm.cmisFaultType(httpStatusCode, serviceException, message), httpStatusCode)
      End Function

      ''' <summary>
      ''' Returns the authenticationInfo of the current web-request
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property CurrentAuthenticationInfo As Common.AuthenticationInfo
         Get
            Return Common.AuthenticationInfo.FromCurrentWebRequest()
         End Get
      End Property

      ''' <summary>
      ''' Returns the preferred language of the current request
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property CurrentCultureInfo As Globalization.CultureInfo
         Get
            Const languagePrefix As String = "Accept-Language: "
            Dim language As String = ssw.WebOperationContext.Current.IncomingRequest.Headers(System.Net.HttpRequestHeader.AcceptLanguage)

            If Not String.IsNullOrEmpty(language) AndAlso language.StartsWith(languagePrefix) Then
               Try
                  Return Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(language.Substring(languagePrefix.Length))
               Catch ex As Exception
               End Try
            End If

            'default
            Return My.Application.Culture
         End Get
      End Property

      ''' <summary>
      ''' Returns the cookies sent from client
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property CurrentIncomingCookies As CmisObjectModel.Collections.HttpCookieContainer
         Get
            If System.ServiceModel.Web.WebOperationContext.Current IsNot Nothing AndAlso
               System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest IsNot Nothing Then
               Return New CmisObjectModel.Collections.HttpCookieContainer(System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers, "Cookie")
            Else
               Return New CmisObjectModel.Collections.HttpCookieContainer(Nothing, Nothing)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the cookies that will be returned to the client
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property CurrentOutgoingCookies As CmisObjectModel.Collections.HttpCookieContainer
         Get
            If System.ServiceModel.Web.WebOperationContext.Current IsNot Nothing AndAlso
               System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse IsNot Nothing Then
               Return New CmisObjectModel.Collections.HttpCookieContainer(System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.Headers, "Set-Cookie")
            Else
               Return New CmisObjectModel.Collections.HttpCookieContainer(Nothing, Nothing)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the uri of the current web-request
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared ReadOnly Property CurrentRequestUri As Uri
         Get
            Return ss.OperationContext.Current.IncomingMessageHeaders.To
         End Get
      End Property

      ''' <summary>
      ''' Returns True if the specified object exists in the repository
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride ReadOnly Property Exists(repositoryId As String, objectId As String) As Boolean Implements Contracts.ICmisServicesImpl.Exists

      ''' <summary>
      ''' Returns the BaseObjectType of cmisObject specified by objectId
      ''' </summary>
      Protected MustOverride Function GetBaseObjectType(repositoryId As String, objectId As String) As Core.enumBaseObjectTypeIds Implements Contracts.ICmisServicesImpl.GetBaseObjectType

      ''' <summary>
      ''' Returns the objectId of the object specified by path.
      ''' </summary>
      ''' <param name="path"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride Function GetObjectId(repositoryId As String, path As String) As String Implements Contracts.ICmisServicesImpl.GetObjectId

      ''' <summary>
      ''' Returns the parentTypeId of the specified type or Nothing, if the specified type is a baseType
      ''' </summary>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride Function GetParentTypeId(repositoryId As String, typeId As String) As String Implements Contracts.ICmisServicesImpl.GetParentTypeId

      ''' <summary>
      ''' Returns the cookie for the sessionId or Null, if CmisService does not support a sessionIdCookie
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function GetSessionIdCookieName() As String Implements Contracts.ICmisServicesImpl.GetSessionIdCookieName
         Return Nothing
      End Function

      ''' <summary>
      ''' Returns the author for lists of types or objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride Function GetSystemAuthor() As sss.SyndicationPerson Implements Contracts.ICmisServicesImpl.GetSystemAuthor

      ''' <summary>
      ''' Log exception called before the cmisService throws an exception
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <remarks></remarks>
      Private Sub LogException(ex As Exception) Implements Contracts.ICmisServicesImpl.LogException
         Try
            Dim st As New Diagnostics.StackTrace()
            For Each sf As Diagnostics.StackFrame In st.GetFrames
               Dim method = sf.GetMethod()
               If method.Name <> "LogException" Then
                  LogException(ex, method)
                  Exit For
               End If
            Next
         Catch
         End Try
      End Sub
      Protected MustOverride Sub LogException(ex As Exception, method As System.Reflection.MethodBase)

      ''' <summary>
      ''' Returns the repositoryInfo for the specified repositoryId
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride ReadOnly Property RepositoryInfo(repositoryId As String) As Core.cmisRepositoryInfoType Implements Contracts.ICmisServicesImpl.RepositoryInfo

      ''' <summary>
      ''' Returns the cmisTypeDefinitionType-instance for the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected MustOverride ReadOnly Property TypeDefinition(repositoryId As String, typeId As String) As Core.Definitions.Types.cmisTypeDefinitionType Implements Contracts.ICmisServicesImpl.TypeDefinition

      Public MustOverride Function ValidateUserNamePassword(userName As String, password As String) As Boolean Implements Contracts.ICmisServicesImpl.ValidateUserNamePassword

   End Class
End Namespace