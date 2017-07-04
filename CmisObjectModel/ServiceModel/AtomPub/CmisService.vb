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
Imports cm = CmisObjectModel.Messaging
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports css = CmisObjectModel.Serialization.SerializationHelper
Imports src = System.Runtime.Caching
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.ServiceModel.AtomPub
   ''' <summary>
   ''' Implements the functionality of the cmis-webservice version 1.1
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisService
      Inherits Base.CmisService
      Implements Contracts.IAtomPubBinding

#Region "Helper-classes"
      ''' <summary>
      ''' Describes the creation-guidance of AtomPub-objects
      ''' </summary>
      ''' <remarks></remarks>
      Private Class AtomPubObjectGeneratingGuidance

         Public Sub New(repositoryId As String, serviceImpl As Contracts.ICmisServicesImpl)
            'initialization
            Me.BaseUri = serviceImpl.BaseUri
            Me.Repository = serviceImpl.RepositoryInfo(repositoryId)
            Me.RepositoryId = repositoryId
            Me.ServiceImpl = serviceImpl
         End Sub
         Public Sub New(repositoryId As String, serviceImpl As Contracts.ICmisServicesImpl,
                        objects As IEnumerable, hasMoreItems As Boolean, numItems As xs_Integer?,
                        links As List(Of ca.AtomLink), urnSuffix As String, methodName As String,
                        Optional changeLogToken As ccg.Nullable(Of String) = Nothing,
                        Optional depth As xs_Integer? = Nothing,
                        Optional filter As ccg.Nullable(Of String) = Nothing,
                        Optional folderId As ccg.Nullable(Of String) = Nothing,
                        Optional includeACL As Boolean? = Nothing,
                        Optional includeAllowableActions As Boolean? = Nothing,
                        Optional includePathSegment As Boolean? = Nothing,
                        Optional includePolicyIds As Boolean? = Nothing,
                        Optional includeProperties As Boolean? = Nothing,
                        Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                        Optional includeRelativePathSegment As Boolean? = Nothing,
                        Optional includeSubRelationshipTypes As Boolean? = Nothing,
                        Optional maxItems As xs_Integer? = Nothing,
                        Optional objectId As ccg.Nullable(Of String) = Nothing,
                        Optional orderBy As ccg.Nullable(Of String) = Nothing,
                        Optional q As ccg.Nullable(Of String) = Nothing,
                        Optional relationshipDirection As Core.enumRelationshipDirection? = Nothing,
                        Optional renditionFilter As ccg.Nullable(Of String) = Nothing,
                        Optional searchAllVersions As Boolean? = Nothing,
                        Optional skipCount As xs_Integer? = Nothing,
                        Optional typeId As ccg.Nullable(Of String) = Nothing,
                        Optional versionSeriesId As ccg.Nullable(Of String) = Nothing)
            Me.New(repositoryId, serviceImpl)
            'initialization
            _currentChangeLogToken = changeLogToken
            _currentDepth = depth
            _currentFilter = filter
            _currentFolderId = folderId
            _currentHasMoreItems = hasMoreItems
            _currentIncludeACL = includeACL
            _currentIncludeAllowableActions = includeAllowableActions
            _currentIncludePathSegment = includePathSegment
            _currentIncludePolicyIds = includePolicyIds
            _currentIncludeProperties = includeProperties
            _currentIncludeRelationships = includeRelationships
            _currentIncludeRelativePathSegment = includeRelativePathSegment
            _currentIncludeSubRelationshipTypes = includeSubRelationshipTypes
            _currentLinks = links
            _currentMaxItems = maxItems
            _currentMethodName = methodName
            _currentNumItems = numItems
            _currentObjectId = objectId
            _currentObjects = objects
            _currentOrderBy = orderBy
            _currentQuery = q
            _currentRelationshipDirection = relationshipDirection
            _currentRenditionFilter = renditionFilter
            _currentSearchAllVersions = searchAllVersions
            _currentSkipCount = skipCount
            _currentTypeId = typeId
            _currentUrnSuffix = urnSuffix
            _currentVersionSeriesId = versionSeriesId
         End Sub

#Region "Transaction"
         ''' <summary>
         ''' Starts a new series of property-modification
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub BeginTransaction()
            _transactions.Push(_currentTransaction)
            _currentTransaction = New List(Of Action)
         End Sub

         ''' <summary>
         ''' Rollback since BeginTransaction()-call
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub EndTransaction()
            'rollback all modification since the last BeginTransaction()-call
            For Each rollbackAction As Action In _currentTransaction
               rollbackAction.Invoke()
            Next
            If _transactions.Count = 0 Then
               _currentTransaction.Clear()
            Else
               _currentTransaction = _transactions.Pop()
            End If
         End Sub

         Private _currentTransaction As New List(Of Action)
         Private _transactions As New Stack(Of List(Of Action))
#End Region

#Region "transactional properties"
         Private _currentChangeLogToken As ccg.Nullable(Of String)
         Private _changeLogTokenStack As New Stack(Of ccg.Nullable(Of String))
         Public Property ChangeLogToken As ccg.Nullable(Of String)
            Get
               Return _currentChangeLogToken
            End Get
            Set(value As ccg.Nullable(Of String))
               _changeLogTokenStack.Push(_currentChangeLogToken)
               _currentChangeLogToken = value
               _currentTransaction.Add(Sub()
                                          _currentChangeLogToken = _changeLogTokenStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentDepth As xs_Integer?
         Private _depthStack As New Stack(Of xs_Integer?)
         Public Property Depth As xs_Integer?
            Get
               Return _currentDepth
            End Get
            Set(value As xs_Integer?)
               _depthStack.Push(_currentDepth)
               _currentDepth = value
               _currentTransaction.Add(Sub()
                                          _currentDepth = _depthStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentFilter As ccg.Nullable(Of String)
         Private _filterStack As New Stack(Of ccg.Nullable(Of String))
         Public Property Filter As ccg.Nullable(Of String)
            Get
               Return _currentFilter
            End Get
            Set(value As ccg.Nullable(Of String))
               _filterStack.Push(_currentFilter)
               _currentFilter = value
               _currentTransaction.Add(Sub()
                                          _currentFilter = _filterStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentFolderId As ccg.Nullable(Of String)
         Private _folderIdStack As New Stack(Of ccg.Nullable(Of String))
         Public Property FolderId As ccg.Nullable(Of String)
            Get
               Return _currentFolderId
            End Get
            Set(value As ccg.Nullable(Of String))
               _folderIdStack.Push(_currentFolderId)
               _currentFolderId = value
               _currentTransaction.Add(Sub()
                                          _currentFolderId = _folderIdStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentHasMoreItems As Boolean
         Private _hasMoreItemsStack As New Stack(Of Boolean)
         Public Property HasMoreItems As Boolean
            Get
               Return _currentHasMoreItems
            End Get
            Set(value As Boolean)
               _hasMoreItemsStack.Push(_currentHasMoreItems)
               _currentHasMoreItems = value
               _currentTransaction.Add(Sub()
                                          _currentHasMoreItems = _hasMoreItemsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeACL As Boolean?
         Private _includeACLStack As New Stack(Of Boolean?)
         Public Property IncludeACL As Boolean?
            Get
               Return _currentIncludeACL
            End Get
            Set(value As Boolean?)
               _includeACLStack.Push(_currentIncludeACL)
               _currentIncludeACL = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeACL = _includeACLStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeAllowableActions As Boolean?
         Private _includeAllowableActionsStack As New Stack(Of Boolean?)
         Public Property IncludeAllowableActions As Boolean?
            Get
               Return _currentIncludeAllowableActions
            End Get
            Set(value As Boolean?)
               _includeAllowableActionsStack.Push(_currentIncludeAllowableActions)
               _currentIncludeAllowableActions = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeAllowableActions = _includeAllowableActionsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludePathSegment As Boolean?
         Private _includePathSegmentStack As New Stack(Of Boolean?)
         Public Property IncludePathSegment As Boolean?
            Get
               Return _currentIncludePathSegment
            End Get
            Set(value As Boolean?)
               _includePathSegmentStack.Push(_currentIncludePathSegment)
               _currentIncludePathSegment = value
               _currentTransaction.Add(Sub()
                                          _currentIncludePathSegment = _includePathSegmentStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludePolicyIds As Boolean?
         Private _includePolicyIdsStack As New Stack(Of Boolean?)
         Public Property IncludePolicyIds As Boolean?
            Get
               Return _currentIncludePolicyIds
            End Get
            Set(value As Boolean?)
               _includePolicyIdsStack.Push(_currentIncludePolicyIds)
               _currentIncludePolicyIds = value
               _currentTransaction.Add(Sub()
                                          _currentIncludePolicyIds = _includePolicyIdsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeProperties As Boolean?
         Private _includePropertiesStack As New Stack(Of Boolean?)
         Public Property IncludeProperties As Boolean?
            Get
               Return _currentIncludeProperties
            End Get
            Set(value As Boolean?)
               _includePropertiesStack.Push(_currentIncludeProperties)
               _currentIncludeProperties = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeProperties = _includePropertiesStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeRelationships As Core.enumIncludeRelationships?
         Private _includeRelationshipsStack As New Stack(Of Core.enumIncludeRelationships?)
         Public Property IncludeRelationships As Core.enumIncludeRelationships?
            Get
               Return _currentIncludeRelationships
            End Get
            Set(value As Core.enumIncludeRelationships?)
               _includeRelationshipsStack.Push(_currentIncludeRelationships)
               _currentIncludeRelationships = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeRelationships = _includeRelationshipsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeRelativePathSegment As Boolean?
         Private _includeRelativePathSegmentStack As New Stack(Of Boolean?)
         Public Property IncludeRelativePathSegment As Boolean?
            Get
               Return _currentIncludeRelativePathSegment
            End Get
            Set(value As Boolean?)
               _includeRelativePathSegmentStack.Push(_currentIncludeRelativePathSegment)
               _currentIncludeRelativePathSegment = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeRelativePathSegment = _includeRelativePathSegmentStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentIncludeSubRelationshipTypes As Boolean?
         Private _includeSubRelationshipTypesStack As New Stack(Of Boolean?)
         Public Property IncludeSubRelationshipTypes As Boolean?
            Get
               Return _currentIncludeSubRelationshipTypes
            End Get
            Set(value As Boolean?)
               _includeSubRelationshipTypesStack.Push(_currentIncludeSubRelationshipTypes)
               _currentIncludeSubRelationshipTypes = value
               _currentTransaction.Add(Sub()
                                          _currentIncludeSubRelationshipTypes = _includeSubRelationshipTypesStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentLinks As List(Of ca.AtomLink)
         Private _linksStack As New Stack(Of List(Of ca.AtomLink))
         Public Property Links As List(Of ca.AtomLink)
            Get
               Return _currentLinks
            End Get
            Set(value As List(Of ca.AtomLink))
               _linksStack.Push(_currentLinks)
               _currentLinks = value
               _currentTransaction.Add(Sub()
                                          _currentLinks = _linksStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentMaxItems As xs_Integer?
         Private _maxItemsStack As New Stack(Of xs_Integer?)
         Public Property MaxItems As xs_Integer?
            Get
               Return _currentMaxItems
            End Get
            Set(value As xs_Integer?)
               _maxItemsStack.Push(_currentMaxItems)
               _currentMaxItems = value
               _currentTransaction.Add(Sub()
                                          _currentMaxItems = _maxItemsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentMethodName As String
         Private _methodNameStack As New Stack(Of String)
         Public Property MethodName As String
            Get
               Return _currentMethodName
            End Get
            Set(value As String)
               _methodNameStack.Push(_currentMethodName)
               _currentMethodName = value
               _currentTransaction.Add(Sub()
                                          _currentMethodName = _methodNameStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentNumItems As xs_Integer?
         Private _numItemsStack As New Stack(Of xs_Integer?)
         Public Property NumItems As xs_Integer?
            Get
               Return _currentNumItems
            End Get
            Set(value As xs_Integer?)
               _numItemsStack.Push(_currentNumItems)
               _currentNumItems = value
               _currentTransaction.Add(Sub()
                                          _currentNumItems = _numItemsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentObjectId As ccg.Nullable(Of String)
         Private _objectIdStack As New Stack(Of ccg.Nullable(Of String))
         Public Property ObjectId As ccg.Nullable(Of String)
            Get
               Return _currentObjectId
            End Get
            Set(value As ccg.Nullable(Of String))
               _objectIdStack.Push(_currentObjectId)
               _currentObjectId = value
               _currentTransaction.Add(Sub()
                                          _currentObjectId = _objectIdStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentObjects As IEnumerable
         Private _objectsStack As New Stack(Of IEnumerable)
         Public Property Objects As IEnumerable
            Get
               Return _currentObjects
            End Get
            Set(value As IEnumerable)
               _objectsStack.Push(_currentObjects)
               _currentObjects = value
               _currentTransaction.Add(Sub()
                                          _currentObjects = _objectsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentOrderBy As ccg.Nullable(Of String)
         Private _orderByStack As New Stack(Of ccg.Nullable(Of String))
         Public Property OrderBy As ccg.Nullable(Of String)
            Get
               Return _currentOrderBy
            End Get
            Set(value As ccg.Nullable(Of String))
               _orderByStack.Push(_currentOrderBy)
               _currentOrderBy = value
               _currentTransaction.Add(Sub()
                                          _currentOrderBy = _orderByStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentQuery As ccg.Nullable(Of String)
         Private _queryStack As New Stack(Of ccg.Nullable(Of String))
         Public Property Query As ccg.Nullable(Of String)
            Get
               Return _currentQuery
            End Get
            Set(value As ccg.Nullable(Of String))
               _queryStack.Push(_currentQuery)
               _currentQuery = value
               _currentTransaction.Add(Sub()
                                          _currentQuery = _queryStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentRelationshipDirection As Core.enumRelationshipDirection?
         Private _relationshipDirectionStack As New Stack(Of Core.enumRelationshipDirection?)
         Public Property RelationshipDirection As Core.enumRelationshipDirection?
            Get
               Return _currentRelationshipDirection
            End Get
            Set(value As Core.enumRelationshipDirection?)
               _relationshipDirectionStack.Push(_currentRelationshipDirection)
               _currentRelationshipDirection = value
               _currentTransaction.Add(Sub()
                                          _currentRelationshipDirection = _relationshipDirectionStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentRenditionFilter As ccg.Nullable(Of String)
         Private _renditionFilterStack As New Stack(Of ccg.Nullable(Of String))
         Public Property RenditionFilter As ccg.Nullable(Of String)
            Get
               Return _currentRenditionFilter
            End Get
            Set(value As ccg.Nullable(Of String))
               _renditionFilterStack.Push(_currentRenditionFilter)
               _currentRenditionFilter = value
               _currentTransaction.Add(Sub()
                                          _currentRenditionFilter = _renditionFilterStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentSearchAllVersions As Boolean?
         Private _searchAllVersionsStack As New Stack(Of Boolean?)
         Public Property SearchAllVersions As Boolean?
            Get
               Return _currentSearchAllVersions
            End Get
            Set(value As Boolean?)
               _searchAllVersionsStack.Push(_currentSearchAllVersions)
               _currentSearchAllVersions = value
               _currentTransaction.Add(Sub()
                                          _currentSearchAllVersions = _searchAllVersionsStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentSkipCount As xs_Integer?
         Private _skipCountStack As New Stack(Of xs_Integer?)
         Public Property SkipCount As xs_Integer?
            Get
               Return _currentSkipCount
            End Get
            Set(value As xs_Integer?)
               _skipCountStack.Push(_currentSkipCount)
               _currentSkipCount = value
               _currentTransaction.Add(Sub()
                                          _currentSkipCount = _skipCountStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentTypeId As ccg.Nullable(Of String)
         Private _typeIdStack As New Stack(Of ccg.Nullable(Of String))
         Public Property TypeId As ccg.Nullable(Of String)
            Get
               Return _currentTypeId
            End Get
            Set(value As ccg.Nullable(Of String))
               _typeIdStack.Push(_currentTypeId)
               _currentTypeId = value
               _currentTransaction.Add(Sub()
                                          _currentTypeId = _typeIdStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentUrnSuffix As String
         Private _UrnSuffixStack As New Stack(Of String)
         Public Property UrnSuffix As String
            Get
               Return _currentUrnSuffix
            End Get
            Set(value As String)
               _UrnSuffixStack.Push(_currentUrnSuffix)
               _currentUrnSuffix = value
               _currentTransaction.Add(Sub()
                                          _currentUrnSuffix = _UrnSuffixStack.Pop()
                                       End Sub)
            End Set
         End Property
         Private _currentVersionSeriesId As ccg.Nullable(Of String)
         Private _versionSeriesIdStack As New Stack(Of ccg.Nullable(Of String))
         Public Property VersionSeriesId As ccg.Nullable(Of String)
            Get
               Return _currentVersionSeriesId
            End Get
            Set(value As ccg.Nullable(Of String))
               _versionSeriesIdStack.Push(_currentVersionSeriesId)
               _currentVersionSeriesId = value
               _currentTransaction.Add(Sub()
                                          _currentVersionSeriesId = _versionSeriesIdStack.Pop()
                                       End Sub)
            End Set
         End Property
#End Region

         Public ReadOnly BaseUri As Uri
         Public ReadOnly Repository As Core.cmisRepositoryInfoType
         Public ReadOnly RepositoryId As String
         Public ReadOnly ServiceImpl As Contracts.ICmisServicesImpl
      End Class
      ''' <summary>
      ''' Creator of needed cmis-links
      ''' </summary>
      ''' <remarks></remarks>
      Private Class LinkFactory

         Private _id As String
         Private _owner As Contracts.ICmisServicesImpl
         Private _repositoryId As String
         Private _selfLinkUri As Uri
         Public Sub New(owner As Contracts.ICmisServicesImpl, repositoryId As String, id As String, selfLinkUri As Uri)
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
            retVal.Add(New ca.AtomLink(New Uri(_owner.BaseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
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
            Dim baseUri As Uri = _owner.BaseUri
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
            Dim baseUri As Uri = _owner.BaseUri
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
            Dim baseUri As Uri = _owner.BaseUri
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
               New ca.AtomLink(New Uri(_owner.BaseUri, ServiceURIs.GetRepositoryInfo.ReplaceUri("repositoryId", _repositoryId)),
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
            Dim baseUri As Uri = _owner.BaseUri

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
            retVal.Add(New ca.AtomLink(New Uri(_owner.BaseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", _repositoryId, "id", _id)),
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
            Dim baseUri As Uri = _owner.BaseUri

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
            Dim baseUri As Uri = _owner.BaseUri

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

#Region "Repository"
      ''' <summary>
      ''' Creates a new type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateType(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateType
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If data Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("type"), serviceImpl)

         Using ms As New IO.MemoryStream
            data.CopyTo(ms)
            Try
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               result = serviceImpl.CreateType(repositoryId, CType(ToAtomEntry(ms, True), ccdt.cmisTypeDefinitionType))
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Dim type As ccdt.cmisTypeDefinitionType = result.Success

                  If type Is Nothing Then
                     Return Nothing
                  Else
                     Dim entry As New ca.AtomEntry(type, type.GetLinks(serviceImpl.BaseUri, repositoryId), serviceImpl.GetSystemAuthor())

                     context.ContentType = MediaTypes.Entry
                     context.StatusCode = Net.HttpStatusCode.Created

                     AddLocation(context, repositoryId, entry.TypeId, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId))

                     Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
                  End If
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            Finally
               ms.Close()
            End Try
         End Using

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="typeId"></param>
      ''' <remarks></remarks>
      Public Sub DeleteType(repositoryId As String, typeId As String) Implements Contracts.IAtomPubBinding.DeleteType
         Dim failure As Exception
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(typeId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("typeId"), serviceImpl)

         Try
            failure = serviceImpl.DeleteType(repositoryId, typeId)
            If failure Is Nothing Then
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.NoContent
            ElseIf Not IsWebException(failure) Then
               failure = cm.cmisFaultType.CreateUnknownException(failure)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               failure = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         If failure IsNot Nothing Then Throw LogException(failure, serviceImpl)
      End Sub

      ''' <summary>
      ''' Returns the CMIS service-documents for all available repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetRepositories() As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetRepositories
         Dim repositoryId As String = GetRequestParameter("repositoryId")

         If String.IsNullOrEmpty(repositoryId) Then
            Return MyBase.GetRepositories(AddressOf SerializeRepositories)
         Else
            'redirect request to address of GetRepositoryInfo()
            Dim location As String = CmisServiceImpl.BaseUri.AbsoluteUri

            location &= If(location.EndsWith("/"), repositoryId, "/" & repositoryId)
            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.Redirect
            ssw.WebOperationContext.Current.OutgoingResponse.Location = location

            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Returns the CMIS service-document for the specified repository
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetRepositoryInfo(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetRepositoryInfo
         Return MyBase.GetRepositoryInfo(repositoryId, AddressOf SerializeRepositories)
      End Function

      ''' <summary>
      ''' Returns all child types of the specified type, if defined, otherwise the basetypes of the repository.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <returns></returns>
      ''' <remarks>
      ''' Optional parameters:
      ''' typeId, includePropertyDefinitions, maxItems, skipCount
      ''' </remarks>
      Public Function GetTypeChildren(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetTypeChildren
         Dim result As ccg.Result(Of cm.cmisTypeDefinitionListType)
         Dim serviceImpl = CmisServiceImpl
         'get the optional parameters from the queryString
         Dim typeId As String = If(GetRequestParameter(ServiceURIs.enumTypesUri.typeId), GetRequestParameter("id"))
         Dim includePropertyDefinitions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumTypesUri.includePropertyDefinitions))
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumTypesUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumTypesUri.skipCount))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         Try
            result = serviceImpl.GetTypeChildren(repositoryId, typeId,
                                                 includePropertyDefinitions.HasValue AndAlso includePropertyDefinitions.Value,
                                                 maxItems, skipCount)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim feed As ca.AtomFeed
               Dim baseUri As Uri = serviceImpl.BaseUri
               Dim typeList As cm.cmisTypeDefinitionListType = If(result.Success, New cm.cmisTypeDefinitionListType)
               Dim types As ccdt.cmisTypeDefinitionType() = typeList.Types
               Dim entries As List(Of ca.AtomEntry) = If(types Is Nothing, New List(Of ca.AtomEntry),
                                                         (From type As ccdt.cmisTypeDefinitionType In types
                                                          Where type IsNot Nothing
                                                          Select New ca.AtomEntry(type, type.GetLinks(baseUri, repositoryId), serviceImpl.GetSystemAuthor())).ToList())
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               With New SelfLinkUriBuilder(Of ServiceURIs.enumTypesUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.TypesUri(queryString))
                  .Add(ServiceURIs.enumTypesUri.typeId, typeId)
                  .Add(ServiceURIs.enumTypesUri.includePropertyDefinitions, includePropertyDefinitions)
                  .Add(ServiceURIs.enumTypesUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumTypesUri.skipCount, skipCount)

                  With New LinkFactory(serviceImpl, repositoryId, typeId, .ToUri())
                     Dim links As List(Of CmisObjectModel.AtomPub.AtomLink) = .CreateTypeChildrenLinks(entries.Count, typeList.NumItems,
                                                                                                       typeList.HasMoreItems, skipCount, maxItems)
                     feed = New ca.AtomFeed("urn:feeds:typeChildren:" & typeId,
                                            "Result of GetTypeChildren('" & repositoryId &
                                                                        "', typeId:='" & typeId &
                                                                        "', includePropertyDefinitions:=" & If(includePropertyDefinitions.HasValue, CStr(includePropertyDefinitions.Value), "Null") &
                                                                        ", maxItems:=" & If(maxItems.HasValue, CStr(maxItems.Value), "Null") &
                                                                        ", skipCount" & If(skipCount.HasValue, CStr(skipCount.Value), "Null") & ")",
                                            DateTimeOffset.Now, entries,
                                            typeList.HasMoreItems, typeList.NumItems, links,
                                            serviceImpl.GetSystemAuthor())
                  End With
               End With

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the type-definition of the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetTypeDefinition(repositoryId As String, typeId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetTypeDefinition
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(typeId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("typeId"), serviceImpl)

         Try
            result = serviceImpl.GetTypeDefinition(repositoryId, typeId)

            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim type As ccdt.cmisTypeDefinitionType = result.Success

               If type Is Nothing Then
                  Return Nothing
               Else
                  Dim entry As New ca.AtomEntry(type, type.GetLinks(serviceImpl.BaseUri, repositoryId), serviceImpl.GetSystemAuthor())
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  context.ContentType = MediaTypes.Entry
                  context.StatusCode = Net.HttpStatusCode.OK

                  Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant object-types under the specified type.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="id">TypeId; optional
      ''' If specified, then the repository MUST return all of descendant types of the speciﬁed type
      ''' If not specified, then the Repository MUST return all types and MUST ignore the value of the depth parameter</param>
      ''' <param name="includePropertyDefinitions">If TRUE, then the repository MUST return the property deﬁnitions for each object-type.
      ''' If FALSE (default), the repository MUST return only the attributes for each object-type</param>
      ''' <param name="depth">The number of levels of depth in the type hierarchy from which to return results. Valid values are
      ''' 1:  Return only types that are children of the type. See also getTypeChildren
      ''' >1: Return only types that are children of the type and descendants up to [depth] levels deep
      ''' -1: Return ALL descendant types at all depth levels in the CMIS hierarchy</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetTypeDescendants(repositoryId As String, id As String, includePropertyDefinitions As String, depth As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetTypeDescendants
         Dim result As ccg.Result(Of cm.cmisTypeContainer)
         Dim serviceImpl = CmisServiceImpl
         Dim nDepth As xs_Integer? = ParseInteger(depth)
         Dim nIncludePropertyDefinitions As Boolean? = ParseBoolean(includePropertyDefinitions)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If nDepth.HasValue AndAlso (nDepth.Value = 0 OrElse nDepth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("The parameter 'depth' MUST NOT be 0 or less than -1", False), serviceImpl)

         Try
            result = serviceImpl.GetTypeDescendants(repositoryId, id, nIncludePropertyDefinitions.HasValue AndAlso nIncludePropertyDefinitions.Value, nDepth)

            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim typeContainer = If(result.Success, New cm.cmisTypeContainer())
               'result.Type ist Nothing, wenn die gesamte Type-Hierarchie des Repositories abgefragt wurde
               Dim feed As ca.AtomFeed = CreateAtomFeed(repositoryId, id, nIncludePropertyDefinitions.HasValue AndAlso nIncludePropertyDefinitions.Value, nDepth,
                                                        If(typeContainer.Type Is Nothing, typeContainer.Children,
                                                           New CmisObjectModel.Messaging.cmisTypeContainer() {typeContainer}),
                                                        serviceImpl.BaseUri)
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="data">A type definition object with the property definitions that are to change.
      ''' Repositories MUST ignore all fields in the type definition except for the type id and the list of properties.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function UpdateType(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.UpdateType
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If data Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("type"), serviceImpl)

         Using ms As New System.IO.MemoryStream
            data.CopyTo(ms)

            Try
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               result = serviceImpl.UpdateType(repositoryId, CType(ToAtomEntry(ms, False), ccdt.cmisTypeDefinitionType))
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Dim type As ccdt.cmisTypeDefinitionType = result.Success

                  If type Is Nothing Then
                     Return Nothing
                  Else
                     Dim entry As New ca.AtomEntry(type, type.GetLinks(serviceImpl.BaseUri, repositoryId), serviceImpl.GetSystemAuthor())

                     context.ContentType = MediaTypes.Entry
                     context.StatusCode = Net.HttpStatusCode.OK
                     AddLocation(context, repositoryId, entry.TypeId, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId))

                     Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
                  End If
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            Finally
               ms.Close()
            End Try
         End Using

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Returns a list of check out object the user has access to.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' The following CMIS Atom extension element MUST be included inside the atom entries:
      ''' cmisra:object inside atom:entry
      ''' The following CMIS Atom extension element MAY be included inside the atom feed:
      ''' cmisra:numItems
      ''' 
      ''' Optional parameters:
      ''' folderId, maxItems, skipCount, orderBy, filter, includeAllowableActions, includeRelationships, renditionFilter
      ''' </remarks>
      Public Function GetCheckedOutDocs(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetCheckedOutDocs
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl
         'get the optional parameters from the queryString
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumCheckedOutUri.folderId), GetRequestParameter("id"))
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumCheckedOutUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumCheckedOutUri.skipCount))
         Dim orderBy As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.orderBy)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumCheckedOutUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumCheckedOutUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.renditionFilter)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         Try
            result = serviceImpl.GetCheckedOutDocs(repositoryId, folderId, filter, maxItems, skipCount,
                                                   renditionFilter, includeAllowableActions, includeRelationships)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objectList As cmisObjectListType = If(result.Success, New cmisObjectListType())
               Dim feed As ca.AtomFeed
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               With New SelfLinkUriBuilder(Of ServiceURIs.enumCheckedOutUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.CheckedOutUri(queryString))
                  .Add(ServiceURIs.enumCheckedOutUri.folderId, folderId)
                  .Add(ServiceURIs.enumCheckedOutUri.filter, filter)
                  .Add(ServiceURIs.enumCheckedOutUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumCheckedOutUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumCheckedOutUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumCheckedOutUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumCheckedOutUri.includeRelationships, includeRelationships)

                  With New LinkFactory(serviceImpl, repositoryId, folderId, .ToUri())
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.Length), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#Else
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.LongLength), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#End If
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "checkedOutDocs:" & folderId, "GetCheckedOutDocs",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            folderId:=If(String.IsNullOrEmpty(folderId), Nothing, New ccg.Nullable(Of String)(folderId)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includeRelationships:=includeRelationships,
                                                            maxItems:=maxItems,
                                                            orderBy:=If(String.IsNullOrEmpty(orderBy), Nothing, New ccg.Nullable(Of String)(orderBy)),
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)),
                                                            skipCount:=skipCount)
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns all children of the specified CMIS object.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <returns>
      ''' Required parameters:
      ''' folderId
      ''' Optional parameters:
      ''' maxItems, skipCount, filter, includeAllowableActions, includeRelationships, renditionFilter, orderBy, includePathSegment
      ''' </returns>
      ''' <remarks></remarks>
      Public Function GetChildren(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetChildren
         Dim result As ccg.Result(Of cmisObjectInFolderListType)
         Dim serviceImpl = CmisServiceImpl
         'get the required ...
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         '... and optional parameters from the queryString
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumChildrenUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumChildrenUri.skipCount))
         Dim filter As String = GetRequestParameter(ServiceURIs.enumChildrenUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumChildrenUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumChildrenUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumChildrenUri.renditionFilter)
         Dim orderBy As String = GetRequestParameter(ServiceURIs.enumChildrenUri.orderBy)
         Dim includePathSegment As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumChildrenUri.includePathSegment))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         Try
            result = serviceImpl.GetChildren(repositoryId, folderId, maxItems, skipCount, filter,
                                             includeAllowableActions, includeRelationships,
                                             renditionFilter, orderBy,
                                             includePathSegment.HasValue AndAlso includePathSegment.Value)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)
               Dim objectList As cmisObjectInFolderListType = If(result.Success, New cmisObjectInFolderListType())

               With New SelfLinkUriBuilder(Of ServiceURIs.enumChildrenUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.ChildrenUri(queryString))
                  .Add(ServiceURIs.enumChildrenUri.folderId, folderId)
                  .Add(ServiceURIs.enumChildrenUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumChildrenUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumChildrenUri.filter, filter)
                  .Add(ServiceURIs.enumChildrenUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumChildrenUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumChildrenUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumChildrenUri.orderBy, orderBy)
                  .Add(ServiceURIs.enumChildrenUri.includePathSegment, includePathSegment)

                  With New LinkFactory(serviceImpl, repositoryId, folderId, .ToUri())
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                     links = .CreateChildrenLinks(If(objects.Objects Is Nothing, 0, objects.Objects.Length), objects.NumItems, objects.HasMoreItems, skipCount, maxItems)
#Else
                     links = .CreateChildrenLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.LongLength), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#End If
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "children:" & folderId, "GetChildren",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            folderId:=If(String.IsNullOrEmpty(folderId), Nothing, New ccg.Nullable(Of String)(folderId)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includeRelationships:=includeRelationships,
                                                            maxItems:=maxItems,
                                                            orderBy:=If(String.IsNullOrEmpty(orderBy), Nothing, New ccg.Nullable(Of String)(orderBy)),
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)),
                                                            skipCount:=skipCount)
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant objects contained in the specified folder or any of its child-folders.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId">The identifier for the folder</param>
      ''' <param name="filter"></param>
      ''' <param name="depth">The number of levels of depth in the type hierarchy from which to return results. Valid values are
      ''' 1:  Return only types that are children of the type. See also getTypeChildren
      ''' >1: Return only types that are children of the type and descendants up to [depth] levels deep
      ''' -1: Return ALL descendant types at all depth levels in the CMIS hierarchy</param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(repositoryId As String, folderId As String, filter As String, depth As String, includeAllowableActions As String,
                                     includeRelationships As String, renditionFilter As String, includePathSegment As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetDescendants
         Dim result As ccg.Result(Of cmisObjectInFolderContainerType)
         Dim serviceImpl = CmisServiceImpl
         Dim nDepth As xs_Integer? = ParseInteger(depth)
         Dim nIncludeAllowableActions As Boolean? = ParseBoolean(includeAllowableActions)
         Dim nIncludePathSegment As Boolean? = ParseBoolean(includePathSegment)
         Dim nIncludeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(includeRelationships)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If nDepth.HasValue AndAlso (nDepth.Value = 0 OrElse nDepth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("The parameter 'depth' MUST NOT be 0 or less than -1", False), serviceImpl)

         Try
            result = serviceImpl.GetDescendants(repositoryId, folderId, filter, nDepth,
                                                nIncludeAllowableActions, nIncludeRelationships,
                                                renditionFilter, nIncludePathSegment.HasValue AndAlso nIncludePathSegment.Value)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim container = If(result.Success, New cmisObjectInFolderContainerType())
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)

               With New SelfLinkUriBuilder(Of ServiceURIs.enumDescendantsUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.DescendantsUri(queryString))
                  .Add(ServiceURIs.enumDescendantsUri.folderId, folderId)
                  .Add(ServiceURIs.enumDescendantsUri.filter, filter)
                  .Add(ServiceURIs.enumDescendantsUri.depth, nDepth)
                  .Add(ServiceURIs.enumDescendantsUri.includeAllowableActions, nIncludeAllowableActions)
                  .Add(ServiceURIs.enumDescendantsUri.includeRelationships, nIncludeRelationships)
                  .Add(ServiceURIs.enumDescendantsUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumDescendantsUri.includePathSegment, nIncludePathSegment)

                  With New LinkFactory(serviceImpl, repositoryId, folderId, .ToUri())
                     links = .CreateDescendantsLinks()

                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, container.Children, False, Nothing, links,
                                                            "descendants:" & folderId, "GetDescendants",
                                                            depth:=nDepth,
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            folderId:=If(String.IsNullOrEmpty(folderId), Nothing, New ccg.Nullable(Of String)(folderId)),
                                                            includeAllowableActions:=nIncludeAllowableActions,
                                                            includePathSegment:=nIncludePathSegment,
                                                            includeRelationships:=nIncludeRelationships,
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)))
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               If feed Is Nothing Then
                  Return Nothing
               Else
                  context.ContentType = MediaTypes.Feed
                  context.StatusCode = Net.HttpStatusCode.OK

                  Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the parent folder-object of the specified folder
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId"></param>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetFolderParent(repositoryId As String, folderId As String, filter As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetFolderParent
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         Try
            result = serviceImpl.GetFolderParent(repositoryId, folderId, filter)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, False)
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant folders contained in the specified folder
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <returns></returns>
      ''' <remarks>
      ''' Required parameters:
      ''' folderId
      ''' Optional parameters:
      ''' filter, depth, includeAllowableActions, includeRelationships, includePathSegment, renditionFilter
      ''' </remarks>
      Public Function GetFolderTree(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetFolderTree
         Dim result As ccg.Result(Of cmisObjectInFolderContainerType)
         Dim serviceImpl = CmisServiceImpl
         'get the required ...
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumFolderTreeUri.folderId), GetRequestParameter("id"))
         '... and optional parameters from the queryString
         Dim filter As String = GetRequestParameter(ServiceURIs.enumFolderTreeUri.filter)
         Dim depth As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumFolderTreeUri.depth))
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includeRelationships))
         Dim includePathSegment As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includePathSegment))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumFolderTreeUri.renditionFilter)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If depth.HasValue AndAlso (depth.Value = 0 OrElse depth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("depth"), serviceImpl)

         Try
            result = serviceImpl.GetFolderTree(repositoryId, folderId, filter, depth,
                                               includeAllowableActions, includeRelationships,
                                               includePathSegment.HasValue AndAlso includePathSegment.Value,
                                               renditionFilter)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim container = If(result.Success, New cmisObjectInFolderContainerType())
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)

               With New SelfLinkUriBuilder(Of ServiceURIs.enumFolderTreeUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.FolderTreeUri(queryString))
                  .Add(ServiceURIs.enumFolderTreeUri.folderId, folderId)
                  .Add(ServiceURIs.enumFolderTreeUri.filter, filter)
                  .Add(ServiceURIs.enumFolderTreeUri.depth, depth)
                  .Add(ServiceURIs.enumFolderTreeUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumFolderTreeUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumFolderTreeUri.includePathSegment, includePathSegment)
                  .Add(ServiceURIs.enumFolderTreeUri.renditionFilter, renditionFilter)

                  With New LinkFactory(serviceImpl, repositoryId, folderId, .ToUri())
                     links = .CreateFolderTreeLinks()

                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, container.Children, False, Nothing, links,
                                                            "tree:" & folderId, "GetFolderTree",
                                                            depth:=depth,
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            folderId:=If(String.IsNullOrEmpty(folderId), Nothing, New ccg.Nullable(Of String)(folderId)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includePathSegment:=includePathSegment,
                                                            includeRelationships:=includeRelationships,
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)))
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               If feed Is Nothing Then
                  Return Nothing
               Else
                  context.ContentType = MediaTypes.Feed
                  context.StatusCode = Net.HttpStatusCode.OK

                  Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the parent folders for the specified object
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeRelativePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' This feed contains a set of atom entries for each parent of the object that MUST contain: 
      ''' cmisra:object inside atom:entry 
      ''' cmisra:relativePathSegment 
      ''' </remarks>
      Public Function GetObjectParents(repositoryId As String, objectId As String, filter As String, includeAllowableActions As String, includeRelationships As String,
                                       renditionFilter As String, includeRelativePathSegment As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetObjectParents
         Dim result As ccg.Result(Of cmisObjectParentsType())
         Dim serviceImpl = CmisServiceImpl
         Dim bIncludeAllowableActions = ParseBoolean(includeAllowableActions)
         Dim bIncludeRelativePathSegment = ParseBoolean(includeRelativePathSegment)
         Dim nIncludeRelationships = ParseEnum(Of Core.enumIncludeRelationships)(includeRelationships)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = CmisServiceImpl.GetObjectParents(repositoryId, objectId, filter, bIncludeAllowableActions,
                                                      nIncludeRelationships, renditionFilter, bIncludeRelativePathSegment)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)
               Dim parents As cmisObjectParentsType() = If(result.Success, New cmisObjectParentsType() {})
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
               Dim numItems As xs_Integer=parents.Length
#Else
               Dim numItems As xs_Integer = parents.LongLength
#End If

               With New SelfLinkUriBuilder(Of ServiceURIs.enumObjectParentsUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.ObjectParentsUri(queryString))
                  .Add(ServiceURIs.enumObjectParentsUri.objectId, objectId)
                  .Add(ServiceURIs.enumObjectParentsUri.filter, filter)
                  .Add(ServiceURIs.enumObjectParentsUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumObjectParentsUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumObjectParentsUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumObjectParentsUri.includeRelativePathSegment, includeRelativePathSegment)

                  With New LinkFactory(serviceImpl, repositoryId, objectId, .ToUri())
                     links = .CreateObjectParentLinks()
                  End With
                  Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                     New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, parents, False, numItems, links,
                                                         "parents:" & objectId, "GetObjectParents",
                                                         filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                         includeAllowableActions:=bIncludeAllowableActions,
                                                         includeRelativePathSegment:=bIncludeRelativePathSegment,
                                                         includeRelationships:=nIncludeRelationships,
                                                         objectId:=objectId,
                                                         renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)))
                  feed = CreateAtomFeed(generatingGuidance)
               End With
               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns a list of all unfiled documents in the repository.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetUnfiledObjects(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetUnfiledObjects
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl
         'optional parameters from the queryString
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumUnfiledUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumUnfiledUri.skipCount))
         Dim filter As String = GetRequestParameter(ServiceURIs.enumUnfiledUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumUnfiledUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumUnfiledUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumUnfiledUri.renditionFilter)
         Dim orderBy As String = GetRequestParameter(ServiceURIs.enumUnfiledUri.orderBy)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         Try
            result = serviceImpl.GetUnfiledObjects(repositoryId, maxItems, skipCount, filter,
                                                   includeAllowableActions, includeRelationships,
                                                   renditionFilter, orderBy)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objectList As cmisObjectListType = If(result.Success, New cmisObjectListType())
               Dim feed As ca.AtomFeed
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               With New SelfLinkUriBuilder(Of ServiceURIs.enumUnfiledUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.UnfiledUri(queryString))
                  .Add(ServiceURIs.enumUnfiledUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumUnfiledUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumUnfiledUri.filter, filter)
                  .Add(ServiceURIs.enumUnfiledUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumUnfiledUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumUnfiledUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumUnfiledUri.orderBy, orderBy)

                  With New LinkFactory(serviceImpl, repositoryId, Nothing, .ToUri())
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.Length), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#Else
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.LongLength), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#End If
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "unfiledObjects", "GetUnfiledObjects",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includeRelationships:=includeRelationships,
                                                            maxItems:=maxItems,
                                                            orderBy:=If(String.IsNullOrEmpty(orderBy), Nothing, New ccg.Nullable(Of String)(orderBy)),
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)),
                                                            skipCount:=skipCount)
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="contentStream"></param>
      ''' <param name="isLastChunk"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function AppendContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream, isLastChunk As Boolean,
                                           changeToken As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.AppendContentStream
         Dim result As ccg.Result(Of cm.Responses.setContentStreamResponse)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If contentStream Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("contentStream"), serviceImpl)

         Try
            Dim mimeType As String = ssw.WebOperationContext.Current.IncomingRequest.ContentType
            Dim fileName As String =
               RFC2231Helper.DecodeContentDisposition(ssw.WebOperationContext.Current.IncomingRequest.Headers(RFC2231Helper.ContentDispositionHeaderName), "")

            result = serviceImpl.AppendContentStream(repositoryId, objectId, contentStream, mimeType, fileName, isLastChunk, changeToken)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, objectId, serviceImpl, result.Success)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function BulkUpdateProperties(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.BulkUpdateProperties
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl
         Dim entry As ca.AtomEntry
         Dim bulkUpdate As Core.cmisBulkUpdateType

         If data Is Nothing Then
            entry = Nothing
            bulkUpdate = Nothing
         Else
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  entry = ToAtomEntry(ms, True)
               Finally
                  ms.Close()
               End Try
            End Using
            bulkUpdate = If(entry Is Nothing, Nothing, entry.BulkUpdate)
         End If

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If bulkUpdate Is Nothing OrElse bulkUpdate.ObjectIdAndChangeTokens Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectIdAndChangeToken"), serviceImpl)

         Try
            result = serviceImpl.BulkUpdateProperties(repositoryId, bulkUpdate)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objects = If(result.Success, New cmisObjectListType())
               Dim genratingGuidance As New AtomPubObjectGeneratingGuidance(repositoryId, CmisServiceImpl, objects.Objects,
                                                                            objects.HasMoreItems, objects.NumItems, Nothing,
                                                                            "bulkUpdates", "BulkUpdateProperties")
               Dim feed As ca.AtomFeed = CreateAtomFeed(genratingGuidance)
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.Created

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a new document in the specified folder or as unfiled document
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <param name="folderId">If specified, the identifier for the folder that MUST be the parent folder for the newly-created document object.
      ''' This parameter MUST be specified if the repository does NOT support the optional "unfiling" capability.</param>
      ''' <param name="versioningState"></param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object, either using the ACL from folderId if specified, or being applied if no folderId is specified.</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object, either using the ACL from folderId if specified, or being ignored if no folderId is specified.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateDocument(repositoryId As String,
                                      folderId As String, versioningState As Core.enumVersioningState?,
                                      data As ca.AtomEntry,
                                      Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                      Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateDocument
         Dim result As ccg.Result(Of cmisObjectType)
         Dim content As Messaging.cmisContentStreamType
         Dim serviceImpl = CmisServiceImpl
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If data Is Nothing OrElse data.Object Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         If data.Content Is Nothing Then
            content = Nothing
         Else
            Dim mimeType As String = data.Content.Mediatype
            Dim fileName As String =
               RFC2231Helper.DecodeContentDisposition(ssw.WebOperationContext.Current.IncomingRequest.Headers(RFC2231Helper.ContentDispositionHeaderName), "")
            content = New Messaging.cmisContentStreamType(data.Content.ToStream(), fileName, mimeType, True)
         End If

         Try
            result = serviceImpl.CreateDocument(repositoryId, data.Object, folderId, content, versioningState, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="properties">The property values that MUST be applied to the object. This list of properties SHOULD only contain properties whose values differ from the source document</param>
      ''' <param name="folderId">If speciﬁed, the identifier for the folder that MUST be the parent folder for the newly-created document object.
      ''' This parameter MUST be specified if the repository does NOT support the optional "unfiling" capability.</param>
      ''' <param name="versioningState"></param>
      ''' <param name="policies">A list of policy ids that MUST be applied to the newly-created document object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object, either using the ACL from folderId if specified, or being ignored if no folderId is specified.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateDocumentFromSource(repositoryId As String, sourceId As String,
                                                properties As Core.Collections.cmisPropertiesType,
                                                folderId As String, versioningState As Core.enumVersioningState?,
                                                policies As String(),
                                                Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateDocumentFromSource
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(sourceId) OrElse Not CmisServiceImpl.Exists(repositoryId, sourceId) Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("sourceId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If

         Try
            result = serviceImpl.CreateDocumentFromSource(repositoryId, sourceId, properties, folderId, versioningState, policies, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <param name="parentFolderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created folder object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created folder object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateFolder(repositoryId As String,
                                    parentFolderId As String,
                                    data As ca.AtomEntry,
                                    Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                    Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateFolder
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(parentFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If data Is Nothing OrElse data.Object Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         Try
            result = serviceImpl.CreateFolder(repositoryId, data.Object, parentFolderId, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates an item object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <param name="folderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created policy object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created policy object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateItem(repositoryId As String,
                                  folderId As String,
                                  data As ca.AtomEntry,
                                  Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                  Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateItem
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If data Is Nothing OrElse data.Object Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         Try
            result = serviceImpl.CreateItem(repositoryId, data.Object, folderId, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a policy object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <param name="folderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created policy object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created policy object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreatePolicy(repositoryId As String,
                                    folderId As String,
                                    data As ca.AtomEntry,
                                    Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                    Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreatePolicy
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If data Is Nothing OrElse data.Object Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         Try
            result = serviceImpl.CreatePolicy(repositoryId, data.Object, folderId, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a relationship object of the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateRelationship(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateRelationship
         Dim result As ccg.Result(Of cmisObjectType) = Nothing
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         If data IsNot Nothing Then
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  'try to interpret data as a request-instance
                  Dim requestBase As Messaging.Requests.RequestBase = ToRequest(ms, repositoryId)
                  Dim entry As ca.AtomEntry

                  If requestBase Is Nothing Then
                     entry = ToAtomEntry(ms, True)
                     If entry Is Nothing OrElse entry.Object Is Nothing Then
                        result = cm.cmisFaultType.CreateInvalidArgumentException("properties")
                     Else
                        result = serviceImpl.CreateRelationship(repositoryId, entry.Object, Nothing, Nothing)
                     End If
                  ElseIf TypeOf requestBase Is Messaging.Requests.createRelationship Then
                     Dim request As Messaging.Requests.createRelationship = CType(requestBase, Messaging.Requests.createRelationship)

                     entry = CType(request, ca.AtomEntry)
                     If entry Is Nothing OrElse entry.Object Is Nothing Then
                        result = cm.cmisFaultType.CreateInvalidArgumentException("properties")
                     Else
                        result = serviceImpl.CreateRelationship(repositoryId, entry.Object, request.AddACEs, request.RemoveACEs)
                     End If
                  End If
                  If result Is Nothing Then
                     result = cm.cmisFaultType.CreateUnknownException()
                  ElseIf result.Failure Is Nothing Then
                     Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
                  End If
               Catch ex As Exception
                  If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                     serviceImpl.LogException(ex)
#End If
                     Throw
                  Else
                     result = cm.cmisFaultType.CreateUnknownException(ex)
                  End If
               Finally
                  ms.Close()
               End Try
            End Using
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      Public Function DeleteContentStream(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.DeleteContentStream
         Dim result As ccg.Result(Of cm.Responses.deleteContentStreamResponse)
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumContentUri.objectId), GetRequestParameter("id"))
         Dim changeToken As String = GetRequestParameter(ServiceURIs.enumContentUri.changeToken)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

            result = serviceImpl.DeleteContentStream(repositoryId, objectId, changeToken)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            Else
               Dim response = result.Success

               If response Is Nothing Then
                  Return Nothing
               Else
                  context.StatusCode = Net.HttpStatusCode.NoContent
                  context.ContentType = MediaTypes.Xml

                  Return css.ToXmlDocument(response)
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Removes the submitted document
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Sub DeleteObject(repositoryId As String) Implements Contracts.IAtomPubBinding.DeleteObject
         Dim serviceImpl = CmisServiceImpl
         Dim failure As Exception
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumObjectUri.objectId), GetRequestParameter("id"))
         Dim allVersions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.allVersions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            failure = serviceImpl.DeleteObject(repositoryId, objectId, Not allVersions.HasValue OrElse allVersions.Value)
            If failure Is Nothing Then
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.NoContent
            ElseIf Not IsWebException(failure) Then
               failure = cm.cmisFaultType.CreateUnknownException(failure)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               failure = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         If failure IsNot Nothing Then Throw LogException(failure, serviceImpl)
      End Sub

      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Function DeleteTree(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.DeleteTree
         Dim result As ccg.Result(Of cm.Responses.deleteTreeResponse)
         Dim serviceImpl = CmisServiceImpl
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumFolderTreeUri.folderId), GetRequestParameter("id"))
         Dim allVersion As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.allVersions))
         Dim unfileObjects As Core.enumUnfileObject? = ParseEnum(Of Core.enumUnfileObject)(GetRequestParameter(ServiceURIs.enumFolderTreeUri.unfileObjects))
         Dim continueOnFailure As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.continueOnFailure))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         Try
            result = serviceImpl.DeleteTree(repositoryId, folderId, Not allVersion.HasValue OrElse allVersion.Value,
                                            unfileObjects, continueOnFailure.HasValue AndAlso continueOnFailure.Value)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim response = result.Success

               If response Is Nothing Then
                  Return Nothing
               Else
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  context.ContentType = MediaTypes.Xml
                  context.StatusCode = response.StatusCode

                  Return css.ToXmlDocument(New Core.Collections.cmisListOfIdsType(response.FailedToDelete.ObjectIds))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
      Private Function DeleteTreeViaDescendantsFeed(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.DeleteTreeViaDescendantsFeed
         Return DeleteTree(repositoryId)
      End Function
      Private Function DeleteTreeViaChildrenFeed(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.DeleteTreeViaChildrenFeed
         Return DeleteTree(repositoryId)
      End Function

      ''' <summary>
      ''' Returns the allowable actions for the specified document.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetAllowableActions(repositoryId As String, id As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetAllowableActions
         Dim result As ccg.Result(Of Core.cmisAllowableActionsType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(id) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.GetAllowableActions(repositoryId, id)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               If result.Success Is Nothing Then
                  Return Nothing
               Else
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  context.ContentType = MediaTypes.AllowableActions
                  context.StatusCode = Net.HttpStatusCode.OK

                  Return css.ToXmlDocument(result.Success)
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the content stream of the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' This returns the content stream. 
      ''' It is RECOMMENDED that HTTP Range requests are supported on this resource.  
      ''' It is RECOMMENDED that HTTP compression is also supported. 
      ''' Please see RFC2616 for more information on HTTP Range requests.
      ''' 
      ''' Required parameters:
      ''' objectId
      ''' Optional parameters:
      ''' streamId
      ''' </remarks>
      Function GetContentStream(repositoryId As String) As IO.Stream Implements Contracts.IAtomPubBinding.GetContentStream
         Dim result As ccg.Result(Of cm.cmisContentStreamType)
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumContentUri.objectId), GetRequestParameter("id"))
         Dim streamId As String = GetRequestParameter(ServiceURIs.enumContentUri.streamId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.GetContentStream(repositoryId, objectId, streamId)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim contentStream As Messaging.cmisContentStreamType = result.Success
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               If contentStream Is Nothing Then
                  Return Nothing
               Else
                  context.ContentType = contentStream.MimeType
                  context.StatusCode = contentStream.StatusCode
                  If Not String.IsNullOrEmpty(contentStream.Filename) Then
                     context.Headers.Add(RFC2231Helper.ContentDispositionHeaderName, RFC2231Helper.EncodeContentDisposition(contentStream.Filename))
                  End If

                  Return contentStream.BinaryStream
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the cmisobject with the specified id.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' requested parameters:
      ''' objectId
      ''' optional parameters:
      ''' filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions
      ''' returnVersion(getObjectOfLatestVersion), major(getObjectOfLatestVersion), versionSeriesId(getObjectOfLatestVersion)
      ''' </remarks>
      Public Function GetObject(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetObject
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl
         'required for the getFolderParent-service
         Dim folderId As String = GetRequestParameter(ServiceURIs.enumObjectUri.folderId)
         'optional parameter
         Dim filter As String = GetRequestParameter(ServiceURIs.enumObjectUri.filter)

         If String.IsNullOrEmpty(folderId) Then
            'get the required ...
            Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumObjectUri.objectId), GetRequestParameter("id"))
            '... and optional parameters from the queryString
            Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumObjectUri.includeRelationships))
            Dim includePolicyIds As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includePolicyIds))
            Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumObjectUri.renditionFilter)
            Dim includeACL As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeACL))
            Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeAllowableActions))
            Dim returnVersion As RestAtom.enumReturnVersion? = ParseEnum(Of RestAtom.enumReturnVersion)(GetRequestParameter(ServiceURIs.enumObjectUri.returnVersion))
            Dim privateWorkingCopy As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.pwc))
            Dim major As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.major))
            Dim versionSeriesId As String = GetRequestParameter(ServiceURIs.enumObjectUri.versionSeriesId)

            'getObjectOfLatestVersion: parameter versionSeriesId is used instead of objectId and parameter major instead of returnVersion
            If Not String.IsNullOrEmpty(versionSeriesId) Then
               If String.IsNullOrEmpty(objectId) Then objectId = versionSeriesId
               returnVersion = If(major.HasValue AndAlso major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest)
            End If
            'invalid arguments
            If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
            If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

            Try
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               result = serviceImpl.GetObject(repositoryId, objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, returnVersion, privateWorkingCopy)
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, False)
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try
         Else
            Return GetFolderParent(repositoryId, folderId, filter)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the object at the specified path
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="path"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="renditionFilter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectByPath(repositoryId As String, path As String, filter As String, includeAllowableActions As String, includePolicyIds As String,
                                      includeRelationships As String, includeACL As String, renditionFilter As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetObjectByPath
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(path) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("path"), serviceImpl)

         Dim nIncludeAllowableActions As Boolean? = ParseBoolean(includeAllowableActions)
         Dim nIncludePolicyIds As Boolean? = ParseBoolean(includePolicyIds)
         Dim nIncludeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(includeRelationships)
         Dim nIncludeACL As Boolean? = ParseBoolean(includeACL)

         Try
            result = serviceImpl.GetObjectByPath(repositoryId, path, filter, nIncludeAllowableActions, nIncludePolicyIds, nIncludeRelationships, nIncludeACL, renditionFilter)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            Else
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, False)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="targetFolderId"></param>
      ''' <param name="sourceFolderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function MoveObject(repositoryId As String, objectId As String, targetFolderId As String, sourceFolderId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.MoveObject
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(targetFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("targetFolderId"), serviceImpl)
         If String.IsNullOrEmpty(sourceFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("sourceFolderId"), serviceImpl)

         Try
            result = serviceImpl.MoveObject(repositoryId, objectId, targetFolderId, sourceFolderId)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Sets the content stream of the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function SetContentStream(repositoryId As String, data As IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.SetContentStream
         Dim serviceImpl = CmisServiceImpl
         'get the required ...
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumContentUri.objectId), GetRequestParameter("id"))
         '... and optional parameters from the queryString
         Dim overwriteFlag As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumContentUri.overwriteFlag))
         Dim changeToken As String = GetRequestParameter(ServiceURIs.enumContentUri.changeToken)
         Dim append As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumContentUri.append))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If data Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("contentStream"), serviceImpl)

         If append.HasValue AndAlso append.Value Then
            Dim isLastChunk As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumContentUri.isLastChunk))
            Return AppendContentStream(repositoryId, objectId, data, isLastChunk.HasValue AndAlso isLastChunk.Value, changeToken)
         Else
            Dim result As ccg.Result(Of cm.Responses.setContentStreamResponse)
            Dim mimeType As String = ssw.WebOperationContext.Current.IncomingRequest.ContentType
            Dim fileName As String =
               RFC2231Helper.DecodeContentDisposition(ssw.WebOperationContext.Current.IncomingRequest.Headers(RFC2231Helper.ContentDispositionHeaderName), "")

            Try
               result = serviceImpl.SetContentStream(repositoryId, objectId, data, mimeType, fileName,
                                                     Not overwriteFlag.HasValue OrElse overwriteFlag.Value, changeToken)
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  If result.Success IsNot Nothing Then objectId = If(result.Success.ObjectId, objectId)
                  Return CreateXmlDocument(repositoryId, objectId, serviceImpl, result.Success)
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try

            'failure
            Throw LogException(result.Failure, serviceImpl)
         End If
      End Function

      ''' <summary>
      ''' Updates the submitted cmis-object
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="changeToken"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function UpdateProperties(repositoryId As String, objectId As String, data As ca.AtomEntry, changeToken As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.UpdateProperties
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If data.Object Is Nothing OrElse data.Object.Properties Is Nothing OrElse data.Object.Properties.Properties Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         Try
            result = serviceImpl.UpdateProperties(repositoryId, objectId, data.Object.Properties, changeToken)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="allVersions">Add all versions of the object to the folder if the repository supports version-specific filing. Defaults to TRUE.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function AddObjectToFolder(repositoryId As String, objectId As String, folderId As String, allVersions As Boolean) As sx.XmlDocument Implements Contracts.IAtomPubBinding.AddObjectToFolder
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         Try
            result = serviceImpl.AddObjectToFolder(repositoryId, objectId, folderId, allVersions)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId">The folder from which the object is to be removed.
      ''' If no value is specified, then the repository MUST remove the object from all folders in which it is currently filed.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function RemoveObjectFromFolder(repositoryId As String, objectId As String, folderId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.RemoveObjectFromFolder
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.RemoveObjectFromFolder(repositoryId, objectId, folderId)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Returns a list of content changes
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="filter"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="includeProperties"></param>
      ''' <param name="changeLogToken">If this parameter is specified, start the changes from the specified token. The changeLogToken is embedded in the paging link relations for normal iteration through the change list. </param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetContentChanges(repositoryId As String, filter As String, maxItems As String, includeACL As String, includePolicyIds As String,
                                        includeProperties As String, changeLogToken As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetContentChanges
         Dim result As ccg.Result(Of getContentChanges)
         Dim serviceImpl = CmisServiceImpl
         Dim nMaxItems As xs_Integer? = ParseInteger(maxItems)
         Dim nIncludeACL As Boolean? = ParseBoolean(includeACL)
         Dim nIncludePolicyIds As Boolean? = ParseBoolean(includePolicyIds)
         Dim nIncludeProperties As Boolean? = ParseBoolean(includeProperties)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         Try
            result = serviceImpl.GetContentChanges(repositoryId, filter, nMaxItems, nIncludeACL,
                                                   nIncludePolicyIds.HasValue AndAlso nIncludePolicyIds.Value,
                                                   nIncludeProperties.HasValue AndAlso nIncludeProperties.Value,
                                                   changeLogToken)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objectList As getContentChanges = If(result.Success, New getContentChanges())
               Dim feed As ca.AtomFeed
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               With New SelfLinkUriBuilder(Of ServiceURIs.enumChangesUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.ChangesUri(queryString))
                  .Add(ServiceURIs.enumChangesUri.filter, filter)
                  .Add(ServiceURIs.enumChangesUri.maxItems, nMaxItems)
                  .Add(ServiceURIs.enumChangesUri.includeACL, nIncludeACL)
                  .Add(ServiceURIs.enumChangesUri.includePolicyIds, nIncludePolicyIds)
                  .Add(ServiceURIs.enumChangesUri.includeProperties, nIncludeProperties)
                  .Add(ServiceURIs.enumChangesUri.changeLogToken, changeLogToken)

                  With New LinkFactory(serviceImpl, repositoryId, Nothing, .ToUri())
                     Dim links = .CreateContentChangesLinks()
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "changes", "GetContentChanges",
                                                            changeLogToken:=If(String.IsNullOrEmpty(changeLogToken), Nothing, New ccg.Nullable(Of String)(changeLogToken)),
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            includeACL:=nIncludeACL,
                                                            includePolicyIds:=nIncludePolicyIds,
                                                            includeProperties:=nIncludeProperties,
                                                            maxItems:=nMaxItems)
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the data described by the specified CMIS query. (GET Request)
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Query(repositoryId As String) As System.Xml.XmlDocument Implements Contracts.IAtomPubBinding.Query
         'get the required ...
         Dim q As String = If(If(GetRequestParameter(ServiceURIs.enumQueryUri.q), GetRequestParameter("query")), GetRequestParameter(ServiceURIs.enumQueryUri.statement))
         '... and optional parameters from the queryString
         Dim searchAllVersion As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumQueryUri.searchAllVersions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumQueryUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumQueryUri.renditionFilter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumQueryUri.includeAllowableActions))
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumQueryUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumQueryUri.skipCount))

         Return Query(System.Net.HttpStatusCode.OK, repositoryId, q, searchAllVersion, includeRelationships, renditionFilter,
                      includeAllowableActions, maxItems, skipCount)
      End Function
      ''' <summary>
      ''' Returns the data described by the specified CMIS query. (POST Request)
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Query(repositoryId As String, data As System.IO.Stream) As System.Xml.XmlDocument Implements Contracts.IAtomPubBinding.Query
         Dim serviceImpl = CmisServiceImpl

         If data Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("statement"), serviceImpl)

         Using ms As New System.IO.MemoryStream
            data.CopyTo(ms)

            Try
               Dim request As Messaging.Requests.query = TryCast(ToRequest(ms, Nothing), Messaging.Requests.query)
               If request Is Nothing OrElse String.IsNullOrEmpty(request.RepositoryId) Then
                  With ConvertData(Of Core.cmisQueryType)(ms, Function(reader)
                                                                 Dim retVal As New Core.cmisQueryType
                                                                 retVal.ReadXml(reader)
                                                                 Return retVal
                                                              End Function)
                     Return Query(System.Net.HttpStatusCode.Created, repositoryId, .Statement, .SearchAllVersions, .IncludeRelationships,
                                  .RenditionFilter, .IncludeAllowableActions, .MaxItems, .SkipCount)
                  End With
               Else
                  With request
                     Return Query(System.Net.HttpStatusCode.Created, repositoryId, .Statement, .SearchAllVersions, .IncludeRelationships,
                                  .RenditionFilter, .IncludeAllowableActions, .MaxItems, .SkipCount)
                  End With
               End If
            Finally
               ms.Close()
            End Try
         End Using
      End Function
      Private Function Query(success As System.Net.HttpStatusCode,
                             repositoryId As String, q As String, searchAllVersions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                             renditionFilter As String, includeAllowableActions As Boolean?, maxItems As xs_Integer?, skipCount As xs_Integer?) As System.Xml.XmlDocument
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(q) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("statement"), serviceImpl)

         Try
            result = serviceImpl.Query(repositoryId, q, searchAllVersions.HasValue AndAlso searchAllVersions.Value, includeRelationships,
                                       renditionFilter, includeAllowableActions.HasValue AndAlso includeAllowableActions.Value,
                                       maxItems, skipCount)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objectList As cmisObjectListType = If(result.Success, New cmisObjectListType())
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               With New SelfLinkUriBuilder(Of ServiceURIs.enumQueryUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.QueryUri(queryString))
                  .Add(ServiceURIs.enumQueryUri.q, q)
                  .Add(ServiceURIs.enumQueryUri.searchAllVersions, searchAllVersions)
                  .Add(ServiceURIs.enumQueryUri.includeRelationships, includeRelationships)
                  .Add(ServiceURIs.enumQueryUri.renditionFilter, renditionFilter)
                  .Add(ServiceURIs.enumQueryUri.includeAllowableActions, includeAllowableActions)
                  .Add(ServiceURIs.enumQueryUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumQueryUri.skipCount, skipCount)

                  With New LinkFactory(serviceImpl, repositoryId, Nothing, .ToUri())
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                     links = .CreateQueryLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.Length), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#Else
                     links = .CreateQueryLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.LongLength), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#End If
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "query", "Query",
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includeRelationships:=includeRelationships,
                                                            maxItems:=maxItems,
                                                            q:=If(String.IsNullOrEmpty(q), Nothing, New ccg.Nullable(Of String)(q)),
                                                            renditionFilter:=If(String.IsNullOrEmpty(renditionFilter), Nothing, New ccg.Nullable(Of String)(renditionFilter)),
                                                            searchAllVersions:=searchAllVersions,
                                                            skipCount:=skipCount)
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               If feed Is Nothing Then
                  Return Nothing
               Else
                  context.StatusCode = success
                  context.ContentType = MediaTypes.Feed
                  'Header: Location, Content-Location
                  Dim uriBuilder As New UriBuilder(BaseUri.Combine(ServiceURIs.QueryUri(ServiceURIs.enumQueryUri.q).ReplaceUri("repositoryId", repositoryId, "q", q)))
                  Dim queryStrings As New List(Of String)

                  If Not String.IsNullOrEmpty(uriBuilder.Query) Then queryStrings.Add(uriBuilder.Query.TrimStart("?"c))
                  If searchAllVersions.HasValue Then queryStrings.Add("searchAllVersions=" & searchAllVersions.Value.ToString().ToLowerInvariant())
                  If includeRelationships.HasValue Then queryStrings.Add("includeRelationships=" & System.Uri.EscapeDataString(includeRelationships.Value.GetName()))
                  If Not String.IsNullOrEmpty(renditionFilter) Then queryStrings.Add("renditionFilter=" & System.Uri.EscapeDataString(renditionFilter))
                  If includeAllowableActions.HasValue Then queryStrings.Add("includeAllowableActions=" & includeAllowableActions.Value.ToString().ToLowerInvariant())
                  If maxItems.HasValue Then queryStrings.Add("maxItems=" & maxItems.Value.ToString())
                  If skipCount.HasValue Then queryStrings.Add("skipCount=" & skipCount.Value.ToString())
                  If queryStrings.Count > 0 Then uriBuilder.Query = String.Join("&", queryStrings.ToArray())
                  context.Location = uriBuilder.Uri.AbsoluteUri
                  context.Headers.Add("Content-Location", context.Location)

                  Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents in the version series to be checked out again.
      ''' If the private working copy has been created by createDocument, cancelCheckOut MUST delete the created document.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks>Handled by DeleteObject()</remarks>
      Public Sub CancelCheckOut(repositoryId As String) Implements Contracts.IAtomPubBinding.CancelCheckOut
         Dim serviceImpl = CmisServiceImpl
         Dim failure As Exception
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumObjectUri.objectId), GetRequestParameter("id"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            failure = serviceImpl.CancelCheckOut(repositoryId, objectId)
            If failure Is Nothing Then
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.NoContent
            ElseIf Not IsWebException(failure) Then
               failure = cm.cmisFaultType.CreateUnknownException(failure)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               failure = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         If failure IsNot Nothing Then Throw LogException(failure, serviceImpl)
      End Sub

      ''' <summary>
      ''' Checks-in the Private Working Copy document.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <param name="major">TRUE (default) if the checked-in document object MUST be a major version.</param>
      ''' <param name="checkInComment"></param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object.</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object.</param>
      ''' <returns></returns>
      ''' <remarks>
      ''' For repositories that do NOT support the optional capabilityPWCUpdatable capability, the properties and contentStream input parameters MUST be
      ''' provided on the checkIn service for updates to happen as part of checkIn.
      ''' Each CMIS protocol binding MUST specify whether the checkin service MUST always include all updatable properties, or only those properties
      ''' whose values are different than the original value of the object.
      ''' </remarks>
      Private Function CheckIn(repositoryId As String, objectId As String,
                               data As ca.AtomEntry, major As Boolean?, checkInComment As String,
                               Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                               Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CheckIn
         Dim content As Messaging.cmisContentStreamType
         Dim cmisObject As Core.cmisObjectType = data.Object

         If data.Content Is Nothing Then
            content = Nothing
         Else
            Dim mimeType As String = data.Content.Mediatype
            Dim fileName As String =
               RFC2231Helper.DecodeContentDisposition(ssw.WebOperationContext.Current.IncomingRequest.Headers(RFC2231Helper.ContentDispositionHeaderName), "")
            content = New Messaging.cmisContentStreamType(data.Content.ToStream(), fileName, mimeType, True)
         End If

         Return CheckIn(repositoryId, objectId,
                        If(cmisObject Is Nothing, Nothing, cmisObject.Properties),
                        If(cmisObject Is Nothing OrElse cmisObject.PolicyIds Is Nothing, Nothing, cmisObject.PolicyIds.Ids),
                        content, major, checkInComment, addACEs, removeACEs)
      End Function

      Private Function CheckIn(repositoryId As String, objectId As String, properties As Core.Collections.cmisPropertiesType,
                               policies As String(), content As Messaging.cmisContentStreamType,
                               major As Boolean?, checkInComment As String,
                               Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                               Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As sx.XmlDocument
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.CheckIn(repositoryId, objectId, properties, policies, content, Not major.HasValue OrElse major.Value, checkInComment, addACEs, removeACEs)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, False)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Checks out the specified CMIS object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CheckOut(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CheckOut
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         Using ms As New System.IO.MemoryStream
            If data IsNot Nothing Then data.CopyTo(ms)

            Try
               Dim document As ca.AtomEntry = If(data Is Nothing, Nothing, ToAtomEntry(ms, True))
               Dim objectId As String = If(document Is Nothing, Nothing, document.ObjectId)

               If String.IsNullOrEmpty(objectId) Then objectId = If(GetRequestParameter(ServiceURIs.enumCheckedOutUri.objectId), GetRequestParameter("id"))
               'invalid arguments
               If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
               If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

               Try
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  result = serviceImpl.CheckOut(repositoryId, objectId)

                  If result Is Nothing Then
                     result = cm.cmisFaultType.CreateUnknownException()
                  ElseIf result.Failure Is Nothing Then
                     Dim entry As ca.AtomEntry = CreateAtomEntry(New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl), result.Success)

                     If entry Is Nothing Then
                        Return Nothing
                     Else
                        context.ContentType = MediaTypes.Entry
                        context.StatusCode = Net.HttpStatusCode.Created
                        With BaseUri.Combine(ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId).ReplaceUri("repositoryId", repositoryId, "id", entry.ObjectId))
                           context.Headers.Add("Content-Location", .AbsoluteUri)
                        End With

                        Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
                     End If
                  End If
               Catch ex As Exception
                  If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                     serviceImpl.LogException(ex)
#End If
                     Throw
                  Else
                     result = cm.cmisFaultType.CreateUnknownException(ex)
                  End If
               End Try
            Finally
               ms.Close()
            End Try
         End Using

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns all Documents in the specified version series.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks>In the cmis documentation the GetAllVersions()-method is described with a required versionSeriesId-parameter.
      ''' Unfortunality this parameter is not defined in the messaging xsd-file, but there is used the objectId.
      ''' So this method only checks if at least one of the parameters is set</remarks>
      Public Function GetAllVersions(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetAllVersions
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl
         'get the required ...
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumAllVersionsUri.objectId), GetRequestParameter("id"))
         Dim versionSeriesId As String = GetRequestParameter(ServiceURIs.enumAllVersionsUri.versionSeriesId)
         '... and optional parameters from the queryString
         Dim filter As String = GetRequestParameter(ServiceURIs.enumAllVersionsUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumAllVersionsUri.includeAllowableActions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) AndAlso String.IsNullOrEmpty(versionSeriesId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId/versionSeriesId"), serviceImpl)

         Try
            result = serviceImpl.GetAllVersions(repositoryId, objectId, versionSeriesId, filter, includeAllowableActions)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
               Dim objectList = If(result.Success, New cmisObjectListType())
               Dim feed As ca.AtomFeed
               Dim links As List(Of ca.AtomLink)

               With New SelfLinkUriBuilder(Of ServiceURIs.enumAllVersionsUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.AllVersionsUri(queryString))
                  .Add(ServiceURIs.enumAllVersionsUri.objectId, objectId)
                  .Add(ServiceURIs.enumAllVersionsUri.versionSeriesId, versionSeriesId)
                  .Add(ServiceURIs.enumAllVersionsUri.filter, filter)
                  .Add(ServiceURIs.enumAllVersionsUri.includeAllowableActions, includeAllowableActions)

                  With New LinkFactory(serviceImpl, repositoryId, versionSeriesId, .ToUri())
                     links = .CreateAllVersionsLinks()
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "allVersions", "GetAllVersions",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            objectId:=If(String.IsNullOrEmpty(objectId), Nothing, New ccg.Nullable(Of String)(objectId)),
                                                            versionSeriesId:=If(String.IsNullOrEmpty(versionSeriesId), Nothing, New ccg.Nullable(Of String)(versionSeriesId)))
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="versionSeriesId"></param>
      ''' <param name="major"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks>Handled by GetObject()
      ''' In the cmis documentation the GetObjectOfLatestVersion()-method is described with a required versionSeriesId-parameter.
      ''' Unfortunality this parameter is not defined in the messaging xsd-file, but there is used the objectId.
      ''' So this method only checks if at least one of the parameters is set and prefers the objectId</remarks>
      Public Function GetObjectOfLatestVersion(repositoryId As String, objectId As String, versionSeriesId As String,
                                               major As Boolean?, filter As String, includeRelationships As Core.enumIncludeRelationships?,
                                               includePolicyIds As Boolean?, renditionFilter As String,
                                               includeACL As Boolean?, includeAllowableActions As Boolean?) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetObjectOfLatestVersion
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) AndAlso String.IsNullOrEmpty(versionSeriesId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId/versionSeriesId"), serviceImpl)

         Try
            Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

            result = serviceImpl.GetObject(repositoryId, objectId.NVL(versionSeriesId), filter, includeRelationships,
                                           includePolicyIds, renditionFilter, includeACL, includeAllowableActions,
                                           If(major.HasValue AndAlso major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest), False)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim entry As ca.AtomEntry = CreateAtomEntry(New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl), result.Success)

               If entry Is Nothing Then
                  Return Nothing
               Else
                  context.ContentType = MediaTypes.Entry
                  context.StatusCode = Net.HttpStatusCode.OK

                  Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Relationships"
      ''' <summary>
      ''' Returns the relationships for the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectRelationships(repositoryId As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetObjectRelationships
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim serviceImpl = CmisServiceImpl
         'get the required ...
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumRelationshipsUri.objectId), GetRequestParameter("id"))
         '... and optional parameters from the queryString
         Dim includeSubRelationshipTypes As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumRelationshipsUri.includeSubRelationshipTypes))
         Dim relationshipDirection As Core.enumRelationshipDirection? = ParseEnum(Of Core.enumRelationshipDirection)(GetRequestParameter(ServiceURIs.enumRelationshipsUri.relationshipDirection))
         Dim typeId As String = GetRequestParameter(ServiceURIs.enumRelationshipsUri.typeId)
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumRelationshipsUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumRelationshipsUri.skipCount))
         Dim filter As String = GetRequestParameter(ServiceURIs.enumRelationshipsUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumRelationshipsUri.includeAllowableActions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.GetObjectRelationships(repositoryId, objectId,
                                                        includeSubRelationshipTypes.HasValue AndAlso includeSubRelationshipTypes.Value,
                                                        relationshipDirection, typeId, maxItems, skipCount, filter, includeAllowableActions)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim objectList As cmisObjectListType = If(result.Success, New cmisObjectListType())
               Dim feed As ca.AtomFeed
               Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

               With New SelfLinkUriBuilder(Of ServiceURIs.enumRelationshipsUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.RelationshipsUri(queryString))
                  .Add(ServiceURIs.enumRelationshipsUri.objectId, objectId)
                  .Add(ServiceURIs.enumRelationshipsUri.includeSubRelationshipTypes, includeSubRelationshipTypes)
                  .Add(ServiceURIs.enumRelationshipsUri.relationshipDirection, relationshipDirection)
                  .Add(ServiceURIs.enumRelationshipsUri.typeId, typeId)
                  .Add(ServiceURIs.enumRelationshipsUri.maxItems, maxItems)
                  .Add(ServiceURIs.enumRelationshipsUri.skipCount, skipCount)
                  .Add(ServiceURIs.enumRelationshipsUri.filter, filter)
                  .Add(ServiceURIs.enumRelationshipsUri.includeAllowableActions, includeAllowableActions)

                  With New LinkFactory(serviceImpl, repositoryId, objectId, .ToUri())
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.Length), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#Else
                     Dim links = .CreateCheckedOutLinks(If(objectList.Objects Is Nothing, 0, objectList.Objects.LongLength), objectList.NumItems, objectList.HasMoreItems, skipCount, maxItems)
#End If
                     Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "relationships:" & objectId, "GetObjectRelationships",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            includeAllowableActions:=includeAllowableActions,
                                                            includeSubRelationshipTypes:=includeSubRelationshipTypes,
                                                            maxItems:=maxItems,
                                                            objectId:=If(String.IsNullOrEmpty(objectId), Nothing, New ccg.Nullable(Of String)(objectId)),
                                                            relationshipDirection:=relationshipDirection,
                                                            skipCount:=skipCount,
                                                            typeId:=If(String.IsNullOrEmpty(typeId), Nothing, New ccg.Nullable(Of String)(typeId)))
                     feed = CreateAtomFeed(generatingGuidance)
                  End With
               End With

               context.ContentType = MediaTypes.Feed
               context.StatusCode = Net.HttpStatusCode.OK

               Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a policy to the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' if data is null the objectId- and policyId-parameter MUST be defined in the querystring,
      ''' otherwise an instance of Messaging.Request.applyPolicy or an instance of ca.AtomEntry representing the policy is assumed
      ''' </remarks>
      Public Function ApplyPolicy(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.ApplyPolicy
         Dim result As ccg.Result(Of cmisObjectType)
         Dim serviceImpl = CmisServiceImpl

         Using ms As New System.IO.MemoryStream
            If data IsNot Nothing Then data.CopyTo(ms)

            Try
               Dim request As Messaging.Requests.applyPolicy = If(data Is Nothing, Nothing, TryCast(ToRequest(ms, repositoryId), Messaging.Requests.applyPolicy))
               Dim entry As ca.AtomEntry = If(data Is Nothing OrElse request IsNot Nothing, Nothing, ToAtomEntry(ms, True))
               Dim cmisObject As Core.cmisObjectType = If(entry Is Nothing, Nothing, entry.Object)
               Dim baseTypeId As String = If(cmisObject Is Nothing, Nothing, cmisObject.BaseTypeId)
               Dim objectIsPolicy As Boolean = (baseTypeId = ccdt.cmisTypePolicyDefinitionType.TargetTypeName)
               'queryString
               Dim objectId As String = If(request Is Nothing,
                                           If(Not (String.IsNullOrEmpty(baseTypeId) OrElse objectIsPolicy),
                                              entry.ObjectId, Nothing),
                                           request.ObjectId)
               Dim policyId As String = If(request Is Nothing, If(objectIsPolicy, entry.ObjectId, Nothing), request.PolicyId)

               If String.IsNullOrEmpty(objectId) Then objectId = If(GetRequestParameter(ServiceURIs.enumPoliciesUri.objectId), GetRequestParameter("id"))
               If String.IsNullOrEmpty(policyId) Then policyId = GetRequestParameter(ServiceURIs.enumPoliciesUri.policyId)
               'invalid arguments
               If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
               If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
               If String.IsNullOrEmpty(policyId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("policyId"), serviceImpl)

               Try
                  result = serviceImpl.ApplyPolicy(repositoryId, objectId, policyId)
                  If result Is Nothing Then
                     result = cm.cmisFaultType.CreateUnknownException()
                  ElseIf result.Failure Is Nothing Then
                     Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.Created, True)
                  End If
               Catch ex As Exception
                  If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                     serviceImpl.LogException(ex)
#End If
                     Throw
                  Else
                     result = cm.cmisFaultType.CreateUnknownException(ex)
                  End If
               End Try
            Finally
               ms.Close()
            End Try
         End Using

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns a list of policies applied to the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <returns></returns>
      ''' <remarks>This is the only collection where the URI’s of the objects in the collection MUST be specific to that collection.
      ''' A DELETE on the policy object in the collection is a removal of the policy from the object NOT a deletion of the policy object itself</remarks>
      Public Function GetAppliedPolicies(repositoryId As String, id As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetAppliedPolicies
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(String.IsNullOrEmpty(id), GetRequestParameter(ServiceURIs.enumPoliciesUri.objectId), id)
         'queryString
         Dim filter As String = GetRequestParameter(ServiceURIs.enumPoliciesUri.filter)
         Dim policyId As String = GetRequestParameter(ServiceURIs.enumPoliciesUri.policyId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         If Not String.IsNullOrEmpty(policyId) Then
            'GetAppliedPolicies() returns modified self-links for policy-objects to enable the calling
            'client to remove a policy from an object (using the modified link) but not to remove the
            'policy object itself. Therefore it is possible that the client uses the modified link to
            'get the policy-object
            Dim result As ccg.Result(Of cmisObjectType)
            Dim includeACL As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeACL))
            Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeAllowableActions))
            Dim includePolicyIds As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includePolicyIds))
            Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumObjectUri.filter))
            Dim privateWorkingCopy As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.pwc))
            Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumObjectUri.renditionFilter)
            Dim returnVersion As RestAtom.enumReturnVersion? = ParseEnum(Of RestAtom.enumReturnVersion)(GetRequestParameter(ServiceURIs.enumObjectUri.returnVersion))

            Try
               result = serviceImpl.GetObject(repositoryId, policyId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, returnVersion, privateWorkingCopy)
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Return CreateXmlDocument(repositoryId, serviceImpl, result.Success, Net.HttpStatusCode.OK, False)
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try

            'failure
            Throw LogException(result.Failure, serviceImpl)
         Else
            'normal request
            Dim result As ccg.Result(Of cmisObjectListType)

            'invalid arguments
            If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

            Try
               result = serviceImpl.GetAppliedPolicies(repositoryId, objectId, filter)
               If result Is Nothing Then
                  result = cm.cmisFaultType.CreateUnknownException()
               ElseIf result.Failure Is Nothing Then
                  Dim objectList = If(result.Success, New cmisObjectListType())
                  Dim feed As ca.AtomFeed
                  Dim links As List(Of ca.AtomLink)
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  With New SelfLinkUriBuilder(Of ServiceURIs.enumPoliciesUri)(serviceImpl.BaseUri, repositoryId, Function(queryString) ServiceURIs.PoliciesUri(queryString))
                     .Add(ServiceURIs.enumPoliciesUri.objectId, objectId)
                     .Add(ServiceURIs.enumPoliciesUri.filter, filter)
                     With New LinkFactory(serviceImpl, repositoryId, objectId, .ToUri())
                        links = .CreatePoliciesLinks()

                        Dim generatingGuidance As AtomPubObjectGeneratingGuidance =
                        New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl, objectList, objectList.HasMoreItems, objectList.NumItems, links,
                                                            "policies:" & objectId, "GetAppliedPolicies",
                                                            filter:=If(String.IsNullOrEmpty(filter), Nothing, New ccg.Nullable(Of String)(filter)),
                                                            objectId:=If(String.IsNullOrEmpty(objectId), Nothing, New ccg.Nullable(Of String)(objectId)))
                        feed = CreateAtomFeed(generatingGuidance)
                     End With
                  End With

                  If feed Is Nothing Then
                     Return Nothing
                  Else
                     'modify self-link to avoid prevent clients from deleting the the policy itself from repository
                     For Each policy As ca.AtomEntry In feed.Entries
                        Dim link As sss.SyndicationLink = policy.Link(LinkRelationshipTypes.Self)
                        If link IsNot Nothing Then
                           link.Uri = New Uri(serviceImpl.BaseUri, ServiceURIs.PoliciesUri(ServiceURIs.enumPoliciesUri.objectId Or ServiceURIs.enumPoliciesUri.policyId).ReplaceUri(
                                              "repositoryId", repositoryId, "id", objectId, "policyId", CType(policy.Object, cmisObjectType).ServiceModel.ObjectId))
                        End If
                     Next
                     context.ContentType = MediaTypes.Feed
                     context.StatusCode = Net.HttpStatusCode.OK

                     Return css.ToXmlDocument(New sss.Atom10FeedFormatter(feed))
                  End If
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try

            'failure
            Throw LogException(result.Failure, serviceImpl)
         End If
      End Function

      ''' <summary>
      ''' Removes a policy from the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <param name="policyId"></param>
      ''' <remarks></remarks>
      Public Sub RemovePolicy(repositoryId As String, id As String, policyId As String) Implements Contracts.IAtomPubBinding.RemovePolicy
         Dim failure As Exception
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(String.IsNullOrEmpty(id), GetRequestParameter(ServiceURIs.enumPoliciesUri.objectId), id)
         Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(policyId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("policyId"), serviceImpl)

         Try
            failure = serviceImpl.RemovePolicy(repositoryId, objectId, policyId)
            If failure Is Nothing Then
               context.StatusCode = Net.HttpStatusCode.NoContent
            ElseIf Not IsWebException(failure) Then
               failure = cm.cmisFaultType.CreateUnknownException(failure)
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               failure = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         If failure IsNot Nothing Then Throw LogException(failure, serviceImpl)
      End Sub
#End Region

#Region "ACL"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of document or folder object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ApplyACL(repositoryId As String, id As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.ApplyACL
         Dim result As ccg.Result(Of Core.Security.cmisAccessControlListType) = Nothing
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(String.IsNullOrEmpty(id), GetRequestParameter(ServiceURIs.enumACLUri.objectId), id)
         Dim addACEs As Core.Security.cmisAccessControlListType = Nothing
         Dim removeACEs As Core.Security.cmisAccessControlListType = Nothing
         'queryString
         Dim aclPropagation As Core.enumACLPropagation? = ParseEnum(Of Core.enumACLPropagation)(GetRequestParameter(ServiceURIs.enumACLUri.aclPropagation))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         If data IsNot Nothing Then
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  Dim request As Messaging.Requests.applyACL = TryCast(ToRequest(ms, repositoryId), Messaging.Requests.applyACL)

                  If request Is Nothing Then
                     Dim newACEs As Core.Security.cmisAccessControlListType = ConvertData(Of Core.Security.cmisAccessControlListType)(ms, Function(reader)
                                                                                                                                             Dim retVal As New Core.Security.cmisAccessControlListType
                                                                                                                                             retVal.ReadXml(reader)
                                                                                                                                             Return retVal
                                                                                                                                          End Function)
                     Dim currentACEs = serviceImpl.GetACL(repositoryId, objectId, False)

                     If currentACEs Is Nothing Then
                        addACEs = newACEs
                     ElseIf currentACEs.Failure Is Nothing Then
                        If currentACEs.Success Is Nothing Then
                           addACEs = newACEs
                        Else
                           With currentACEs.Success.Split(newACEs)
                              addACEs = .AddACEs
                              removeACEs = .RemoveACEs
                           End With
                        End If
                     Else
                        result = currentACEs.Failure
                     End If
                  Else
                     addACEs = request.AddACEs
                     removeACEs = request.RemoveACEs
                  End If
               Catch ex As Exception
                  If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                     serviceImpl.LogException(ex)
#End If
                     Throw
                  Else
                     result = cm.cmisFaultType.CreateUnknownException(ex)
                  End If
               Finally
                  ms.Close()
               End Try
            End Using
         End If

         Try
            'don't ignore possible exception from the GetACL()-call
            result = If(result, serviceImpl.ApplyACL(repositoryId, objectId, addACEs, removeACEs,
                                                     If(aclPropagation.HasValue, aclPropagation.Value,
                                                        Core.enumACLPropagation.repositorydetermined)))
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim acl = result.Success

               If acl Is Nothing Then
                  Return Nothing
               Else
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  context.StatusCode = Net.HttpStatusCode.OK
                  context.ContentType = MediaTypes.Acl

                  Return css.ToXmlDocument(acl)
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Get the ACL currently applied to the specified document or folder object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetACL(repositoryId As String, id As String) As sx.XmlDocument Implements Contracts.IAtomPubBinding.GetACL
         Dim result As ccg.Result(Of Core.Security.cmisAccessControlListType)
         Dim serviceImpl = CmisServiceImpl
         Dim objectId As String = If(String.IsNullOrEmpty(id), GetRequestParameter(ServiceURIs.enumACLUri.objectId), id)
         'queryString
         Dim onlyBasicPermissions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumACLUri.onlyBasicPermissions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         Try
            result = serviceImpl.GetACL(repositoryId, objectId, Not onlyBasicPermissions.HasValue OrElse onlyBasicPermissions.Value)
            If result Is Nothing Then
               result = cm.cmisFaultType.CreateUnknownException()
            ElseIf result.Failure Is Nothing Then
               Dim acl = result.Success

               If acl Is Nothing Then
                  Return Nothing
               Else
                  Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

                  context.StatusCode = Net.HttpStatusCode.OK
                  context.ContentType = MediaTypes.Acl

                  Return css.ToXmlDocument(acl)
               End If
            End If
         Catch ex As Exception
            If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
               serviceImpl.LogException(ex)
#End If
               Throw
            Else
               result = cm.cmisFaultType.CreateUnknownException(ex)
            End If
         End Try

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Miscellaneous"
      ''' <summary>
      ''' Handles every POST on object resource.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CheckInOrUpdateProperties(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CheckInOrUpdateProperties
         Dim serviceImpl = CmisServiceImpl
         Dim retVal As sx.XmlDocument = Nothing
         'queryString
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumObjectUri.objectId), GetRequestParameter("id"))
         Dim changeToken As String = GetRequestParameter(ServiceURIs.enumObjectUri.changeToken)
         Dim major As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.major))
         Dim checkIn As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.checkin))
         Dim checkInComment As String = GetRequestParameter(ServiceURIs.enumObjectUri.checkinComment)

         If data Is Nothing Then
            'parameters of checkIn can be defined completely through the queryString
            retVal = Me.CheckIn(repositoryId, objectId, Nothing, Nothing, Nothing, major, checkInComment)
         Else
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  'try to interpret data as a request-instance
                  Dim requestBase As Messaging.Requests.RequestBase = ToRequest(ms, repositoryId)

                  If requestBase Is Nothing Then
                     Dim entry As ca.AtomEntry = ToAtomEntry(ms, False)

                     If checkIn.HasValue AndAlso checkIn.Value Then
                        retVal = Me.CheckIn(repositoryId, objectId, entry, major, checkInComment)
                     Else
                        retVal = Me.UpdateProperties(repositoryId, objectId, entry, changeToken)
                     End If
                  ElseIf TypeOf requestBase Is Messaging.Requests.checkIn Then
                     Dim request As Messaging.Requests.checkIn = CType(requestBase, Messaging.Requests.checkIn)

                     retVal = Me.CheckIn(repositoryId, objectId, request.Properties, request.Policies, request.ContentStream, request.Major, request.CheckinComment, request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.updateProperties Then
                     Dim request As Messaging.Requests.updateProperties = CType(requestBase, Messaging.Requests.updateProperties)
                     retVal = Me.UpdateProperties(repositoryId, objectId, CType(request, ca.AtomEntry), changeToken)
                  End If
               Finally
                  ms.Close()
               End Try
            End Using
         End If

         If retVal Is Nothing Then Throw LogException(cm.cmisFaultType.CreateUnknownException(), serviceImpl)

         Return retVal
      End Function

      ''' <summary>
      ''' Handles every POST on the folder children collection. As defined in 3.9.2.2 HTTP POST the function has to return in the AtomPub-Binding
      ''' an ca.AtomEntry-Object (MediaType: application/atom+xml;type=entry)
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' Handled cmis services:
      ''' createDocument, createDocumentFromSource, createFolder, createPolicy, moveObject
      ''' The function supports data as a serialized ca.AtomEntry-instance or as a serialized request-Object
      ''' out of the Namespace CmisObjectModel.Messaging.Requests
      ''' </remarks>
      Public Function CreateOrMoveChildObject(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateOrMoveChildObject
         Dim serviceImpl = CmisServiceImpl
         Dim retVal As sx.XmlDocument = Nothing
         Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
         'queryString
         Dim allVersions As Boolean? = ParseBoolean(If(GetRequestParameter(ServiceURIs.enumChildrenUri.allVersions), "true"))
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         Dim objectId As String = GetRequestParameter(ServiceURIs.enumChildrenUri.objectId)
         Dim sourceFolderId As String = GetRequestParameter(ServiceURIs.enumChildrenUri.sourceFolderId)
         Dim sourceId As String = GetRequestParameter(ServiceURIs.enumChildrenUri.sourceId)
         Dim targetFolderId As String = GetRequestParameter(ServiceURIs.enumChildrenUri.targetFolderId)
         Dim versioningState As Core.enumVersioningState? = ParseEnum(Of Core.enumVersioningState)(GetRequestParameter(ServiceURIs.enumChildrenUri.versioningState))

         If data Is Nothing Then
            'parameters of addObjectToFolder-, createDocumentFromSource- and moveObject-service can be defined completely through the queryString
            If Not String.IsNullOrEmpty(sourceId) Then
               retVal = CreateDocumentFromSource(repositoryId, sourceId, Nothing, folderId, versioningState, Nothing, Nothing, Nothing)
            ElseIf CmisServiceImpl.Exists(repositoryId, objectId) Then
               'the specified object already exists in the repository: detect if the object should be moved or multi-filed
               If Not String.IsNullOrEmpty(sourceFolderId) AndAlso Not String.IsNullOrEmpty(targetFolderId) Then
                  'moveObject
                  retVal = MoveObject(repositoryId, objectId, targetFolderId, sourceFolderId)
               Else
                  'addObjectToFolder
                  retVal = AddObjectToFolder(repositoryId, objectId, folderId, Not allVersions.HasValue OrElse allVersions.Value)
               End If
            End If
         Else
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  'try to interpret data as a request-instance
                  Dim requestBase As Messaging.Requests.RequestBase = ToRequest(ms, repositoryId)

                  If requestBase Is Nothing Then
                     Dim entry As ca.AtomEntry = ToAtomEntry(ms, True)

                     'higher priority from objectIdProperty if set
                     If entry IsNot Nothing Then objectId = entry.ObjectId.NVL(objectId)
                     If Not String.IsNullOrEmpty(sourceId) Then
                        retVal = CreateDocumentFromSource(repositoryId, sourceId, If(entry.Object Is Nothing, Nothing, entry.Object.Properties),
                                                          folderId, versioningState,
                                                          If(entry.Object Is Nothing OrElse entry.Object.PolicyIds Is Nothing, Nothing, entry.Object.PolicyIds.Ids),
                                                          Nothing, Nothing)
                     ElseIf CmisServiceImpl.Exists(repositoryId, objectId) Then
                        'the specified object already exists in the repository: detect if the object should be moved or multi-filed
                        If Not String.IsNullOrEmpty(sourceFolderId) AndAlso Not String.IsNullOrEmpty(targetFolderId) Then
                           'moveObject
                           retVal = MoveObject(repositoryId, objectId, targetFolderId, sourceFolderId)
                        Else
                           'addObjectToFolder
                           retVal = AddObjectToFolder(repositoryId, objectId, folderId, Not allVersions.HasValue OrElse allVersions.Value)
                        End If
                     Else
                        Dim typeDefinition As Core.Definitions.Types.cmisTypeDefinitionType = CmisServiceImpl.TypeDefinition(repositoryId, If(entry Is Nothing, Nothing, entry.TypeId))

                        If TypeOf typeDefinition Is Core.Definitions.Types.cmisTypeDocumentDefinitionType Then
                           retVal = CreateDocument(repositoryId, folderId, versioningState, entry)
                        ElseIf TypeOf typeDefinition Is Core.Definitions.Types.cmisTypeFolderDefinitionType Then
                           retVal = CreateFolder(repositoryId, folderId, entry)
                        ElseIf TypeOf typeDefinition Is Core.Definitions.Types.cmisTypeItemDefinitionType Then
                           retVal = CreateItem(repositoryId, folderId, entry)
                        ElseIf TypeOf typeDefinition Is Core.Definitions.Types.cmisTypePolicyDefinitionType Then
                           retVal = CreatePolicy(repositoryId, folderId, entry)
                        End If
                     End If
                  ElseIf TypeOf requestBase Is Messaging.Requests.addObjectToFolder Then
                     Dim request As Messaging.Requests.addObjectToFolder = CType(requestBase, Messaging.Requests.addObjectToFolder)
                     retVal = AddObjectToFolder(repositoryId, request.ObjectId, request.FolderId, Not request.AllVersions.HasValue OrElse request.AllVersions.Value)
                  ElseIf TypeOf requestBase Is Messaging.Requests.createDocument Then
                     Dim request As Messaging.Requests.createDocument = CType(requestBase, Messaging.Requests.createDocument)
                     retVal = CreateDocument(repositoryId, request.FolderId, request.VersioningState,
                                             CType(request, ca.AtomEntry),
                                             request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.createDocumentFromSource Then
                     Dim request As Messaging.Requests.createDocumentFromSource = CType(requestBase, Messaging.Requests.createDocumentFromSource)
                     retVal = CreateDocumentFromSource(repositoryId, request.SourceId, request.Properties, request.FolderId, request.VersioningState,
                                                       request.Policies, request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.createFolder Then
                     Dim request As Messaging.Requests.createFolder = CType(requestBase, Messaging.Requests.createFolder)
                     retVal = CreateFolder(repositoryId, request.FolderId, CType(request, ca.AtomEntry), request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.createItem Then
                     Dim request As Messaging.Requests.createItem = CType(requestBase, Messaging.Requests.createItem)
                     retVal = CreateItem(repositoryId, request.FolderId, CType(request, ca.AtomEntry), request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.createPolicy Then
                     Dim request As Messaging.Requests.createPolicy = CType(requestBase, Messaging.Requests.createPolicy)
                     retVal = CreatePolicy(repositoryId, request.FolderId, CType(request, ca.AtomEntry), request.AddACEs, request.RemoveACEs)
                  ElseIf TypeOf requestBase Is Messaging.Requests.moveObject Then
                     Dim request As Messaging.Requests.moveObject = CType(requestBase, Messaging.Requests.moveObject)
                     retVal = MoveObject(repositoryId, request.ObjectId, request.TargetFolderId, request.SourceFolderId)
                  End If
               Finally
                  ms.Close()
               End Try
            End Using
         End If

         If retVal Is Nothing Then Throw LogException(cm.cmisFaultType.CreateUnknownException(), serviceImpl)

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the new object created in the unfiled-resource.
      ''' Handles every POST on the unfiled collection. As defined in 3.9.2.2 HTTP POST the function has to return in the AtomPub-Binding
      ''' an ca.AtomEntry-Object (MediaType: application/atom+xml;type=entry)
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' Handled cmis services:
      ''' createDocument, createDocumentFromSource, createPolicy, removeObjectFromFolder
      ''' The function supports data as a serialized ca.AtomEntry-instance or as a serialized request-Object
      ''' out of the Namespace CmisObjectModel.Messaging.Requests
      ''' Parameter folderId should not be set, if a non existing object should be created.
      ''' </remarks>
      Public Function CreateUnfiledObjectOrRemoveObjectFromFolder(repositoryId As String, data As System.IO.Stream) As sx.XmlDocument Implements Contracts.IAtomPubBinding.CreateUnfiledObjectOrRemoveObjectFromFolder
         Dim serviceImpl = CmisServiceImpl
         'queryString
         Dim objectId As String = If(GetRequestParameter(ServiceURIs.enumUnfiledUri.objectId), GetRequestParameter("id"))
         Dim folderId As String = If(GetRequestParameter(ServiceURIs.enumUnfiledUri.folderId), GetRequestParameter(ServiceURIs.enumUnfiledUri.removeFrom))
         Dim sourceId As String = GetRequestParameter(ServiceURIs.enumUnfiledUri.sourceId)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         If data IsNot Nothing Then
            Using ms As New System.IO.MemoryStream
               data.CopyTo(ms)

               Try
                  'try to interpret data as a request-instance
                  Dim requestBase As Messaging.Requests.RequestBase = ToRequest(ms, repositoryId)

                  If requestBase Is Nothing Then
                     Dim entry As ca.AtomEntry = ToAtomEntry(ms, True)

                     'higher priority from objectIdProperty if set
                     If entry IsNot Nothing Then objectId = entry.ObjectId.NVL(objectId)
                  ElseIf TypeOf requestBase Is Messaging.Requests.removeObjectFromFolder Then
                     Dim request As Messaging.Requests.removeObjectFromFolder = CType(requestBase, Messaging.Requests.removeObjectFromFolder)
                     objectId = request.ObjectId.NVL(objectId)
                     folderId = request.FolderId.NVL(folderId)
                  End If
               Finally
                  ms.Close()
               End Try
            End Using
         End If

         If Not String.IsNullOrEmpty(folderId) Then
            Return RemoveObjectFromFolder(repositoryId, objectId, folderId)
         Else
            Return CreateOrMoveChildObject(repositoryId, data)
         End If
      End Function
#End Region

      ''' <summary>
      ''' Adds the location of the object to the response
      ''' </summary>
      ''' <param name="context"></param>
      ''' <param name="repositoryId"></param>
      ''' <param name="id"></param>
      ''' <param name="uriTemplate">If not set, the uriTemplate of an object is assumed</param>
      ''' <remarks></remarks>
      Private Sub AddLocation(context As ssw.OutgoingWebResponseContext, repositoryId As String, id As String,
                              Optional uriTemplate As String = Nothing)
         If String.IsNullOrEmpty(uriTemplate) Then uriTemplate = ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.objectId)

         With BaseUri.Combine(uriTemplate.ReplaceUri("repositoryId", repositoryId, "id", id))
            context.Location = .AbsoluteUri
         End With
      End Sub

      ''' <summary>
      ''' Transforms data to a XmlReader-instance and returns via createInstance.Invoke() the specified result-type
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="data"></param>
      ''' <param name="createInstance"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ConvertData(Of TResult)(data As System.IO.MemoryStream, createInstance As Func(Of sx.XmlReader, TResult),
                                               Optional attrOverrides As sxs.XmlAttributeOverrides = Nothing) As TResult
         Try
            data.Position = 0

            Dim reader As sx.XmlReader = sx.XmlReader.Create(data)

            If attrOverrides Is Nothing Then
               Return createInstance(reader)
            Else
               Using attributeOverrides As New Serialization.XmlAttributeOverrides(reader, attrOverrides)
                  Return createInstance(reader)
               End Using
            End If
         Catch
            Return Nothing
         End Try
      End Function

      ''' <summary>
      ''' Creates a AtomEntry-instance for the given serviceModelObject
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateAtomEntry(generatingGuidance As AtomPubObjectGeneratingGuidance,
                                       serviceModelObject As Contracts.IServiceModelObject) As ca.AtomEntry
         Dim asEnumerable As Contracts.IServiceModelObjectEnumerable = TryCast(serviceModelObject, Contracts.IServiceModelObjectEnumerable)
         Dim cmisObject As cmisObjectType = If(serviceModelObject Is Nothing, Nothing, serviceModelObject.Object)

         If cmisObject Is Nothing Then
            Return Nothing
         Else
            Dim serviceModel = serviceModelObject.ServiceModel
            Dim objectId As String = serviceModel.ObjectId
            Dim children = CreateAtomFeed(generatingGuidance, serviceModel.VersionSeriesId, asEnumerable)
            Dim includeRelativePathSegment As Boolean = (generatingGuidance.IncludeRelativePathSegment.HasValue AndAlso
                                                         generatingGuidance.IncludeRelativePathSegment.Value)
            Dim includePathSegment As Boolean = (generatingGuidance.IncludePathSegment.HasValue AndAlso
                                                 generatingGuidance.IncludePathSegment.Value)
            Dim links As List(Of ca.AtomLink)

            Select Case serviceModel.BaseObjectType
               Case Core.enumBaseObjectTypeIds.cmisDocument
                  Dim allowableActions As Core.cmisAllowableActionsType = cmisObject.AllowableActions
                  Dim canGetAllVersions As Boolean = allowableActions IsNot Nothing AndAlso
                                                     allowableActions.CanGetAllVersions.HasValue AndAlso
                                                     allowableActions.CanGetAllVersions.Value
                  links = cmisObject.GetDocumentLinks(generatingGuidance.ServiceImpl.BaseUri, generatingGuidance.Repository,
                                                      canGetAllVersions,
                                                      serviceModel.IsLatestVersion,
                                                      serviceModel.VersionSeriesId,
                                                      serviceModel.VersionSeriesCheckedOutId)
               Case Core.enumBaseObjectTypeIds.cmisFolder
                  links = cmisObject.GetFolderLinks(generatingGuidance.ServiceImpl.BaseUri,
                                                    generatingGuidance.Repository, serviceModel.ParentId)
               Case Core.enumBaseObjectTypeIds.cmisItem
                  links = cmisObject.GetItemLinks(generatingGuidance.ServiceImpl.BaseUri,
                                                  generatingGuidance.Repository)
               Case Core.enumBaseObjectTypeIds.cmisPolicy
                  links = cmisObject.GetPolicyLinks(generatingGuidance.ServiceImpl.BaseUri,
                                                    generatingGuidance.Repository)
               Case Core.enumBaseObjectTypeIds.cmisRelationship
                  links = cmisObject.GetRelationshipLinks(generatingGuidance.ServiceImpl.BaseUri,
                                                          generatingGuidance.Repository,
                                                          serviceModel.SourceId, serviceModel.TargetId)
               Case Core.enumBaseObjectTypeIds.cmisSecondary
                  links = cmisObject.GetSecondaryLinks(generatingGuidance.ServiceImpl.BaseUri,
                                                       generatingGuidance.Repository)
               Case Else
                  links = Nothing
            End Select
            Return New ca.AtomEntry("urn:objects:" & objectId,
                                    objectId, serviceModel.Summary, serviceModel.PublishDate, serviceModel.LastUpdatedTime,
                                    cmisObject, serviceModel.ContentLink, children, links,
                                    If(includeRelativePathSegment, serviceModelObject.RelativePathSegment, Nothing),
                                    If(includePathSegment, serviceModelObject.PathSegment, Nothing),
                                    serviceModel.Authors)
         End If
      End Function

      ''' <summary>
      ''' Creates a list of AtomEntries based on objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>Supported objects-enumeration: the items MUST implement the Contracts.IServiceModelObject-interface</remarks>
      Private Function CreateAtomEntryList(generatingGuidance As AtomPubObjectGeneratingGuidance) As List(Of ca.AtomEntry)
         If generatingGuidance.Objects Is Nothing Then
            Return New List(Of ca.AtomEntry)
         Else
            Return (From item As Object In generatingGuidance.Objects
                    Let entry As ca.AtomEntry = CreateAtomEntry(generatingGuidance, TryCast(item, Contracts.IServiceModelObject))
                    Where entry IsNot Nothing
                    Select entry).ToList()
         End If
      End Function

      ''' <summary>
      ''' Creates an AtomFeed-object respecting the generatingGuidance
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateAtomFeed(generatingGuidance As AtomPubObjectGeneratingGuidance) As ca.AtomFeed
         Dim entries = CreateAtomEntryList(generatingGuidance)

         Return New ca.AtomFeed("urn:feeds:" & generatingGuidance.UrnSuffix,
                                "Result of " & generatingGuidance.MethodName & "('" & generatingGuidance.RepositoryId & "'" &
                                    If(generatingGuidance.ChangeLogToken.HasValue, ", changeLogToken:='" & generatingGuidance.ChangeLogToken.Value & "'", Nothing) &
                                    If(generatingGuidance.Filter.HasValue, ", filter:='" & generatingGuidance.Filter.Value & "'", Nothing) &
                                    If(generatingGuidance.FolderId.HasValue, ", folderId:='" & generatingGuidance.FolderId.Value & "'", Nothing) &
                                    If(generatingGuidance.IncludeACL.HasValue, ", includeACL:=" & CStr(generatingGuidance.IncludeACL.Value), Nothing) &
                                    If(generatingGuidance.IncludeAllowableActions.HasValue, ", includeAllowableActions:=" & CStr(generatingGuidance.IncludeAllowableActions.Value), Nothing) &
                                    If(generatingGuidance.IncludePathSegment.HasValue, ", includePathSegment:=" & CStr(generatingGuidance.IncludePathSegment.Value), Nothing) &
                                    If(generatingGuidance.IncludePolicyIds.HasValue, ", includePolicyIds:=" & CStr(generatingGuidance.IncludePolicyIds.Value), Nothing) &
                                    If(generatingGuidance.IncludeProperties.HasValue, ", includeProperties:=" & CStr(generatingGuidance.IncludeProperties.Value), Nothing) &
                                    If(generatingGuidance.IncludeRelationships.HasValue, ", includeRelationships:=" & generatingGuidance.IncludeRelationships.Value.GetName(), Nothing) &
                                    If(generatingGuidance.IncludeRelativePathSegment.HasValue, ", includeRelativePathSegment:=" & CStr(generatingGuidance.IncludeRelativePathSegment.Value), Nothing) &
                                    If(generatingGuidance.IncludeSubRelationshipTypes.HasValue, ", includeSubRelationshipTypes:=" & CStr(generatingGuidance.IncludeSubRelationshipTypes.Value), Nothing) &
                                    If(generatingGuidance.MaxItems.HasValue, ", maxItems:=" & CStr(generatingGuidance.MaxItems.Value), Nothing) &
                                    If(generatingGuidance.ObjectId.HasValue, ", objectId:='" & generatingGuidance.ObjectId.Value & "'", Nothing) &
                                    If(generatingGuidance.OrderBy.HasValue, ", orderBy:='" & generatingGuidance.OrderBy.Value & "'", Nothing) &
                                    If(generatingGuidance.Query.HasValue, ", query:='" & generatingGuidance.Query.Value & "'", Nothing) &
                                    If(generatingGuidance.RelationshipDirection.HasValue, ", relationshipDirection:=" & generatingGuidance.RelationshipDirection.Value.GetName(), Nothing) &
                                    If(generatingGuidance.RenditionFilter.HasValue, ", renditionFilter:='" & generatingGuidance.RenditionFilter.Value & "'", Nothing) &
                                    If(generatingGuidance.SearchAllVersions.HasValue, ", searchAllVersions:=" & CStr(generatingGuidance.SearchAllVersions.Value), Nothing) &
                                    If(generatingGuidance.SkipCount.HasValue, ", skipCount:=" & CStr(generatingGuidance.SkipCount.Value), Nothing) &
                                    If(generatingGuidance.TypeId.HasValue, ", typeId:='" & generatingGuidance.TypeId.Value & "'", Nothing) &
                                    If(generatingGuidance.VersionSeriesId.HasValue, ", versionSeriesId:='" & generatingGuidance.VersionSeriesId.Value & "'", Nothing) & ")",
                                DateTimeOffset.UtcNow, entries, generatingGuidance.HasMoreItems, generatingGuidance.NumItems, generatingGuidance.Links, generatingGuidance.ServiceImpl.GetSystemAuthor())
      End Function

      ''' <summary>
      ''' Creates an AtomFeed-object to encapsulate the children of a folder-object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateAtomFeed(generatingGuidance As AtomPubObjectGeneratingGuidance, folderId As String,
                                      children As Contracts.IServiceModelObjectEnumerable) As ca.AtomFeed
         If children Is Nothing OrElse Not children.ContainsObjects Then
            Return Nothing
         Else
            Try
               'modify several properties
               generatingGuidance.BeginTransaction()
               If generatingGuidance.Depth.HasValue Then
                  Dim depth As xs_Integer = generatingGuidance.Depth.Value
                  If depth > 1 Then generatingGuidance.Depth = depth - 1
               End If
               generatingGuidance.FolderId = folderId
               generatingGuidance.HasMoreItems = children.HasMoreItems
               generatingGuidance.NumItems = children.NumItems
               generatingGuidance.Objects = children
               generatingGuidance.UrnSuffix = "descendants:" & folderId
               With New SelfLinkUriBuilder(Of ServiceURIs.enumDescendantsUri)(generatingGuidance.BaseUri, generatingGuidance.RepositoryId, Function(queryString) ServiceURIs.DescendantsUri(queryString))
                  .Add(ServiceURIs.enumDescendantsUri.folderId, folderId)
                  .Add(ServiceURIs.enumDescendantsUri.filter, generatingGuidance.Filter.Value)
                  .Add(ServiceURIs.enumDescendantsUri.depth, generatingGuidance.Depth)
                  .Add(ServiceURIs.enumDescendantsUri.includeAllowableActions, generatingGuidance.IncludeAllowableActions)
                  .Add(ServiceURIs.enumDescendantsUri.includeRelationships, generatingGuidance.IncludeRelationships)
                  .Add(ServiceURIs.enumDescendantsUri.renditionFilter, generatingGuidance.RenditionFilter.Value)
                  .Add(ServiceURIs.enumDescendantsUri.includePathSegment, generatingGuidance.IncludePathSegment)

                  With New LinkFactory(generatingGuidance.ServiceImpl, generatingGuidance.RepositoryId, generatingGuidance.FolderId, .ToUri())
                     generatingGuidance.Links = .CreateDescendantsLinks()
                  End With
               End With
               Return CreateAtomFeed(generatingGuidance)
            Finally
               'restore descriptor
               generatingGuidance.EndTransaction()
            End Try
         End If
      End Function

      ''' <summary>
      ''' Creates an AtomFeed-object based on containers
      ''' </summary>
      ''' <param name="containers"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateAtomFeed(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean, depth As xs_Integer?,
                                      containers As CmisObjectModel.Messaging.cmisTypeContainer(), baseUri As Uri) As ca.AtomFeed
         If containers Is Nothing OrElse containers.Length = 0 Then
            Return New ca.AtomFeed()
         Else
            Dim serviceImpl As Contracts.ICmisServicesImpl = Me.CmisServiceImpl
            Dim links As List(Of ca.AtomLink)

            '2.2.2.4 getTypeDescendants:
            'If typeId is not specified, then the Repository MUST return all types and MUST ignore the value of the depth parameter
            If String.IsNullOrEmpty(typeId) Then depth = -1
            With New SelfLinkUriBuilder(Of ServiceURIs.enumTypeDescendantsUri)(baseUri, repositoryId, Function(queryString) ServiceURIs.TypeDescendantsUri(queryString))
               .Add(ServiceURIs.enumTypeDescendantsUri.typeId, typeId)
               .Add(ServiceURIs.enumTypeDescendantsUri.includePropertyDefinitions, includePropertyDefinitions)
               .Add(ServiceURIs.enumTypeDescendantsUri.depth, depth)

               With New LinkFactory(serviceImpl, repositoryId, typeId, .ToUri())
                  links = .CreateTypeDescendantsLinks()
               End With
            End With

            Dim entries As List(Of ca.AtomEntry) = (From container As CmisObjectModel.Messaging.cmisTypeContainer In containers
                                                    Where container.Type IsNot Nothing
                                                    Let children As ca.AtomFeed = If(container.Children Is Nothing OrElse container.Children.Length = 0, Nothing,
                                                                                     CreateAtomFeed(repositoryId, container.Type.Id,
                                                                                                    includePropertyDefinitions,
                                                                                                    If(depth.HasValue AndAlso depth.Value > 1, depth.Value - 1, depth),
                                                                                                    container.Children, baseUri))
                                                    Select New ca.AtomEntry(container.Type, children, container.Type.GetLinks(baseUri, repositoryId))).ToList
            Return New ca.AtomFeed("urn:feeds:typeDescendants:" & typeId,
                                   "Result of GetTypeDescendants('" & repositoryId &
                                                                 "', typeId:='" & typeId &
                                                                 "', depth:=" & If(depth.HasValue, CStr(depth.Value), "Null") &
                                                                 ", includePropertyDefinitions:=" & includePropertyDefinitions & ")",
                                   DateTimeOffset.UtcNow, entries, False, entries.Count, links, serviceImpl.GetSystemAuthor())
         End If
      End Function

      ''' <summary>
      ''' erstellt zum serviceModelObject passend ein AtomEntry-Objekt mit Location-Angaben im Context der
      ''' Web-Anfrage
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateXmlDocument(repositoryId As String, serviceImpl As Contracts.ICmisServicesImpl,
                                         cmisObject As cmisObjectType,
                                         status As Net.HttpStatusCode,
                                         addLocation As Boolean) As Xml.XmlDocument
         If cmisObject Is Nothing Then
            Return Nothing
         Else
            Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse
            Dim entry As ca.AtomEntry = CreateAtomEntry(New AtomPubObjectGeneratingGuidance(repositoryId, serviceImpl), cmisObject)

            If entry Is Nothing Then
               Return Nothing
            Else
               context.ContentType = MediaTypes.Entry
               context.StatusCode = status
               If addLocation Then Me.AddLocation(context, repositoryId, cmisObject.ServiceModel.ObjectId)

               Return css.ToXmlDocument(New sss.Atom10ItemFormatter(entry))
            End If
         End If
      End Function

      ''' <summary>
      ''' erstellt zum contentStreamResponse passend ein AtomEntry-Objekt mit Location-Angaben im Context der
      ''' Web-Anfrage
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateXmlDocument(repositoryId As String, objectId As String,
                                         serviceImpl As Contracts.ICmisServicesImpl,
                                         contentStreamResponse As cm.Responses.setContentStreamResponse) As Xml.XmlDocument
         Dim context As ssw.OutgoingWebResponseContext = ssw.WebOperationContext.Current.OutgoingResponse

         If contentStreamResponse Is Nothing Then
            contentStreamResponse = New cm.Responses.setContentStreamResponse(objectId, Nothing, Messaging.enumSetContentStreamResult.NotSet)
         End If
         context.StatusCode = contentStreamResponse.StatusCode
         context.ContentType = MediaTypes.Xml

         With BaseUri.Combine(ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryId, "id", objectId))
            context.Headers.Add("Content-Location", .AbsoluteUri)
         End With
         AddLocation(context, repositoryId, objectId)
         Return css.ToXmlDocument(contentStreamResponse)
      End Function

      ''' <summary>
      ''' Serializes repositories with AtomPub10ServiceDocumentFormatter
      ''' </summary>
      Private Function SerializeRepositories(repositories As Core.cmisRepositoryInfoType()) As sx.XmlDocument
         If repositories Is Nothing Then
            'statuscode already set!
            Return Nothing
         Else
            Dim baseUri As Uri = CmisServiceImpl.BaseUri
            Dim workspaces As ca.AtomWorkspace() =
               (From repositoryInfo As Core.cmisRepositoryInfoType In repositories
                Where repositoryInfo IsNot Nothing
                Select New ca.AtomWorkspace(repositoryInfo.RepositoryName, repositoryInfo,
                                            repositoryInfo.GetCollectionInfos(baseUri),
                                            repositoryInfo.GetLinks(baseUri, Constants.Namespaces.atom, "link"),
                                            repositoryInfo.GetUriTemplates(baseUri))).ToArray()
            With New ca.AtomServiceDocument(workspaces)
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
               ssw.WebOperationContext.Current.OutgoingResponse.ContentType = Constants.MediaTypes.Service
               Return css.ToXmlDocument(CType(.GetFormatter(), sss.AtomPub10ServiceDocumentFormatter))
            End With
         End If
      End Function

      ''' <summary>
      ''' Creates an AtomEntry-instance from data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ToAtomEntry(data As System.IO.MemoryStream, ensurePOSTRuleOfPrecedence As Boolean) As ca.AtomEntry
         Dim retVal As ca.AtomEntry = ConvertData(data, AddressOf ca.AtomEntry.CreateInstance)
         If retVal IsNot Nothing AndAlso ensurePOSTRuleOfPrecedence Then retVal.EnsurePOSTRuleOfPrecedence()
         Return retVal
      End Function

      ''' <summary>
      ''' Creates an AtomFeed-instance from data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ToAtomFeed(data As System.IO.MemoryStream) As ca.AtomFeed
         Return ConvertData(data, AddressOf ca.AtomFeed.CreateInstance)
      End Function

      ''' <summary>
      ''' Creates a RequestBase-instance from data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ToRequest(data As System.IO.MemoryStream, repositoryId As String) As Messaging.Requests.RequestBase
         Dim retVal As Messaging.Requests.RequestBase
         Dim attributeOverrides As New sxs.XmlAttributeOverrides()
         Dim attrs As New sxs.XmlAttributes() With {.XmlRoot = New sxs.XmlRootAttribute() With {.Namespace = ""}} 'ignore Namespace

         attributeOverrides.Add(GetType(Messaging.Requests.RequestBase), attrs)
         retVal = ConvertData(data, AddressOf Messaging.Requests.RequestBase.CreateInstance, attributeOverrides)
         If retVal IsNot Nothing Then retVal.ReadQueryString(repositoryId)
         Return retVal
      End Function

   End Class
End Namespace