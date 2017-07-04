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
Imports srs = System.Runtime.Serialization

Namespace CmisObjectModel.Constants
   ''' <summary>
   ''' ServiceUri for the CMIS-implementation
   ''' </summary>
   ''' <remarks>
   ''' Required parameters are received through the UriTemplate of the WebGetAttribute/WebInvokeAttribute attribute.
   ''' Optional parameters are sent in the query string or through the HttpContext class.
   ''' The reason for this is that when using the same UriTemplate for both WebGet and WebInvoke, with optional parameters,
   ''' the optional parameters will not be optional and they will be case sensitive. This due to an issue in WCF REST.
   ''' 
   ''' Supported UriTemplates in this file:
   ''' /
   ''' /DebugHelpPage
   ''' /MetadataExchange
   ''' /{repositoryId}
   ''' /{repositoryId}/acl
   ''' /{repositoryId}/allowableactions
   ''' /{repositoryId}/allversions
   ''' /{repositoryId}/checkedout
   ''' /{repositoryId}/changes
   ''' /{repositoryId}/children
   ''' /{repositoryId}/content
   ''' /{repositoryId}/descendants
   ''' /{repositoryId}/foldertree
   ''' /{repositoryId}/object
   ''' /{repositoryId}/objectbypath
   ''' /{repositoryId}/objectparents
   ''' /{repositoryId}/policies
   ''' /{repositoryId}/query
   ''' /{repositoryId}/relationships
   ''' /{repositoryId}/root
   ''' /{repositoryId}/root/{*path}
   ''' /{repositoryId}/type
   ''' /{repositoryId}/typedescendants
   ''' /{repositoryId}/types
   ''' /{repositoryId}/unfiled
   ''' /{repositoryId}/updates
   ''' </remarks>
   Public MustInherit Class ServiceURIs
      Private Sub New()
      End Sub

#Region "Build service uri"
      ''' <summary>
      ''' Extends the given serviceUri with queryStringParameters
      ''' </summary>
      ''' <typeparam name="TEnum">MUST be an enum-type based on integer</typeparam>
      ''' <param name="baseServiceUri"></param>
      ''' <param name="queryStringParameters"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetServiceUri(Of TEnum As Structure)(baseServiceUri As String, queryStringParameters As TEnum) As String
         Dim parameters As New List(Of String)(8)
         Dim currentFlag As Integer = 1
         Dim values As Dictionary(Of Integer, Tuple(Of String, System.Enum)) = GetValues(GetType(TEnum))
         Dim linkParametersValue As Integer = CInt(CObj(queryStringParameters))

         'check each bit
         While linkParametersValue <> 0
            If (currentFlag And linkParametersValue) <> 0 Then
               linkParametersValue = linkParametersValue Xor currentFlag
               If values.ContainsKey(currentFlag) Then
                  parameters.Add(values(currentFlag).Item1 & "={" & GetName(values(currentFlag).Item2) & "}")
               End If
            End If
            currentFlag <<= 1
         End While

         Select Case parameters.Count
            Case 0
               Return baseServiceUri
            Case 1
               Return baseServiceUri & If(baseServiceUri.Contains("?"), "&", "?") & parameters(0)
            Case Else
               Return baseServiceUri & If(baseServiceUri.Contains("?"), "&", "?") & String.Join("&", parameters.ToArray())
         End Select
      End Function

      Private Shared _getValuesResults As New Dictionary(Of Type, Dictionary(Of Integer, Tuple(Of String, System.Enum)))
      ''' <summary>
      ''' Returns the values of an enumType based on the integer type
      ''' </summary>
      ''' <param name="enumType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetValues(enumType As Type) As Dictionary(Of Integer, Tuple(Of String, System.Enum))
         SyncLock _getValuesResults
            If _getValuesResults.ContainsKey(enumType) Then
               Return _getValuesResults(enumType)
            Else
               Dim retVal As New Dictionary(Of Integer, Tuple(Of String, System.Enum))
               Dim members As List(Of System.Reflection.FieldInfo) =
                  (From fi As System.Reflection.FieldInfo In enumType.GetFields(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static)
                   Select fi).ToList

               For Each member As System.Reflection.FieldInfo In members
                  Dim enumValue As System.Enum = CType(member.GetValue(Nothing), System.Enum)
                  Dim value As Integer = CType(member.GetValue(Nothing), Integer)
                  If Not retVal.ContainsKey(value) Then retVal.Add(value, New Tuple(Of String, System.Enum)(member.Name, enumValue))
               Next
               _getValuesResults.Add(enumType, retVal)

               Return retVal
            End If
         End SyncLock
      End Function
#End Region

#Region "Enums"
      <Flags()>
      Public Enum enumAbsoluteObjectUri As Integer
         'predefined
         callback = JSON.enumJSONPredefinedParameter.callback
         cmisaction = JSON.enumJSONPredefinedParameter.cmisaction
         cmisselector = JSON.enumJSONPredefinedParameter.cmisselector
         succinct = JSON.enumJSONPredefinedParameter.succinct
         suppressResponseCodes = JSON.enumJSONPredefinedParameter.suppressResponseCodes
         token = JSON.enumJSONPredefinedParameter.token
      End Enum

      <Flags()>
      Public Enum enumACLUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         aclPropagation = 2
         onlyBasicPermissions = 4

         'special
         applyACL = objectId Or aclPropagation
         getACL = objectId Or onlyBasicPermissions
      End Enum

      <Flags()>
      Public Enum enumAllowableActionsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1

         'special
         getAllowableActions = objectId
      End Enum

      <Flags()>
      Public Enum enumAllVersionsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         versionSeriesId = 2
         filter = 4
         includeAllowableActions = 8

         'special
         getAllVersions = objectId Or versionSeriesId Or filter Or includeAllowableActions
      End Enum

      <Flags()>
      Public Enum enumChangesUri As Integer
         none = 0
         filter = 1
         maxItems = 2
         includeACL = 4
         includePolicyIds = 8
         includeProperties = &H10
         changeLogToken = &H20

         'special
         getContentChanges = filter Or maxItems Or includeACL Or includePolicyIds Or includeProperties Or changeLogToken
      End Enum

      <Flags()>
      Public Enum enumCheckedOutUri As Integer
         none = 0
         filter = 1
         <srs.EnumMember(Value:="id")>
         folderId = 2
         maxItems = 4
         skipCount = 8
         renditionFilter = &H10
         includeAllowableActions = &H20
         includeRelationships = &H40
         orderBy = &H80
         objectId = &H100

         'special
         getCheckedOutDocs = filter Or folderId Or maxItems Or skipCount Or renditionFilter Or includeAllowableActions Or includeRelationships Or orderBy
      End Enum

      <Flags()>
      Public Enum enumChildrenUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         folderId = 1
         filter = 2
         includeRelationships = 4
         renditionFilter = 8
         includePathSegment = &H10
         includeAllowableActions = &H20
         maxItems = &H40
         skipCount = &H80
         orderBy = &H100
         sourceFolderId = &H200
         versioningState = &H400
         objectId = &H800
         targetFolderId = &H1000
         sourceId = &H2000
         allVersions = &H4000

         'special
         addObjectToFolder = objectId Or folderId Or allVersions
         createDocument = folderId Or versioningState
         createDocumentFromSource = sourceId Or createDocument
         createFolder = folderId
         createPolicy = folderId
         getChildren = folderId Or filter Or includeRelationships Or renditionFilter Or includePathSegment Or includeAllowableActions Or maxItems Or skipCount Or orderBy
         moveObject = objectId Or targetFolderId Or sourceFolderId
      End Enum

      <Flags()>
      Public Enum enumContentUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         streamId = 2
         overwriteFlag = 4
         changeToken = 8
         isLastChunk = &H10
         ''' <summary>
         ''' If specified and set to true, appendContentStream is called. Otherwise setContentStream is called.
         ''' </summary>
         ''' <remarks>3.11.8.2 HTTP PUT</remarks>
         append = &H20

         'special
         appendContentStream = objectId Or isLastChunk Or changeToken
         deleteContentStream = objectId Or changeToken
         getContentStream = objectId Or streamId
         setContentStream = objectId Or overwriteFlag Or changeToken Or append
      End Enum

      <Flags()>
      Public Enum enumDescendantsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         folderId = 1
         filter = 2
         depth = 4
         includeAllowableActions = 8
         includeRelationships = &H10
         renditionFilter = &H20
         includePathSegment = &H40

         'special
         [get] = folderId Or filter Or depth Or includeAllowableActions Or includeRelationships Or renditionFilter Or includePathSegment
      End Enum

      <Flags()>
      Public Enum enumFolderTreeUri As Integer
         none = 0
         folderId = 1
         filter = 2
         depth = 4
         includeAllowableActions = 8
         includeRelationships = &H10
         includePathSegment = &H20
         renditionFilter = &H40
         allVersions = &H80
         continueOnFailure = &H100
         unfileObjects = &H200

         'special
         delete = folderId Or allVersions Or continueOnFailure Or unfileObjects
         [get] = folderId Or filter Or depth Or includeAllowableActions Or includeRelationships Or includePathSegment Or renditionFilter
      End Enum

      <Flags()>
      Public Enum enumObjectUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         filter = 2
         includeRelationships = 4
         renditionFilter = 8
         includeAllowableActions = &H10
         includePolicyIds = &H20
         includeACL = &H40
         ''' <summary>
         ''' Used to differentiate between getObject and getObjectOfLatestVersion. Valid values are are described by the schema element cmisra:enumReturnVersion.
         ''' If not specified, return the version specified by the URI.
         ''' </summary>
         ''' <remarks>3.11.2.1 HTTP GET</remarks>
         returnVersion = &H80
         ''' <summary>
         ''' private working copy
         ''' </summary>
         ''' <remarks>
         ''' pwc is a boolean parameter; used in document-links
         ''' </remarks>
         pwc = &H100
         ''' <summary>
         ''' parameter used in deleteObject-service
         ''' </summary>
         ''' <remarks></remarks>
         allVersions = &H200
         ''' <summary>
         ''' parameter used in checkIn- and getObjectOfLatestVersion-service
         ''' </summary>
         ''' <remarks></remarks>
         major = &H400
         ''' <summary>
         ''' parameter used in updateProperties-service
         ''' </summary>
         ''' <remarks></remarks>
         changeToken = &H800
         ''' <summary>
         ''' Used to differentiate between updateProperties or checkIn services. If TRUE, execute checkIn service
         ''' </summary>
         ''' <remarks>3.11.3.2 HTTP PUT</remarks>
         checkin = &H1000
         ''' <summary>
         ''' parameter used in checkIn-service
         ''' </summary>
         ''' <remarks></remarks>
         checkinComment = &H2000
         ''' <summary>
         ''' parameter used in getFolderParent-service
         ''' </summary>
         ''' <remarks></remarks>
         folderId = &H4000
         ''' <summary>
         ''' parameter used in getObjectOfLatestVersion-service
         ''' </summary>
         ''' <remarks></remarks>
         versionSeriesId = &H8000

         'special
         self = objectId
         deleteObject = objectId Or allVersions
         getObject = objectId Or filter Or includeRelationships Or renditionFilter Or includeAllowableActions Or includePolicyIds Or includeACL
         specifyVersion = objectId Or returnVersion
         updateProperties = objectId Or major Or changeToken Or checkinComment
         workingCopy = objectId Or pwc
         ''' <summary>
         ''' </summary>
         ''' <remarks>
         ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
         ''' 3.7.1.1.2 Object By Id
         ''' </remarks>
         getObjectById = getObject Or returnVersion
      End Enum

      <Flags()>
      Public Enum enumObjectByPathUri As Integer
         none = 0
         path = 1
         filter = 2
         includeAllowableActions = 4
         includePolicies = 8
         includeRelationships = &H10
         includeACL = &H20
         renditionFilter = &H40

         'special
         [get] = path Or filter Or includeAllowableActions Or includePolicies Or includeRelationships Or includeACL Or renditionFilter
      End Enum

      <Flags()>
      Public Enum enumObjectParentsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         filter = 2
         includeRelationships = 4
         renditionFilter = 8
         includeAllowableActions = &H10
         includeRelativePathSegment = &H20

         'special
         [get] = objectId Or filter Or includeRelationships Or renditionFilter Or includeAllowableActions Or includeRelativePathSegment
      End Enum

      <Flags()>
      Public Enum enumPoliciesUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         policyId = 2
         filter = 4

         'special
         removePolicy = objectId Or policyId
         getAppliedPolicies = objectId Or filter
         applyPolicy = objectId Or policyId
      End Enum

      <Flags()>
      Public Enum enumQueryUri As Integer
         none = 0
         q = 1
         searchAllVersions = 2
         includeAllowableActions = 4
         includeRelationships = 8
         renditionFilter = &H10
         maxItems = &H20
         skipCount = &H40
         'defined parameter-name in 2.2.6.1 query for the parameter q
         statement = &H80

         'special
         query = q Or searchAllVersions Or includeAllowableActions Or includeRelationships Or renditionFilter Or maxItems Or skipCount
      End Enum

      <Flags()>
      Public Enum enumRelationshipsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         typeId = 2
         includeSubRelationshipTypes = 4
         relationshipDirection = 8
         maxItems = &H10
         skipCount = &H20
         filter = &H40
         includeAllowableActions = &H80

         'special
         getObjectRelationships = objectId Or typeId Or includeSubRelationshipTypes Or relationshipDirection Or maxItems Or skipCount Or filter Or includeAllowableActions
         createRelationship = none
      End Enum

      <Flags()>
      Public Enum enumRepositoriesUri As Integer
         none = 0
         repositoryId = 1
         logout = 2
         ping = 4

         'browser binding
         cmisaction = JSON.enumJSONPredefinedParameter.cmisaction
         token = JSON.enumJSONPredefinedParameter.token
         file = JSON.enumJSONPredefinedParameter.maxValue << 1
      End Enum

      <Flags()>
      Public Enum enumRepositoryUri As Integer
         none = 0
         logout = enumRepositoriesUri.logout
         ping = enumRepositoriesUri.ping

         'browser binding
         callback = JSON.enumJSONPredefinedParameter.callback
         cmisaction = JSON.enumJSONPredefinedParameter.cmisaction
         cmisselector = JSON.enumJSONPredefinedParameter.cmisselector
         succinct = JSON.enumJSONPredefinedParameter.succinct
         suppressResponseCodes = JSON.enumJSONPredefinedParameter.suppressResponseCodes
         token = JSON.enumJSONPredefinedParameter.token
         typeId = JSON.enumJSONPredefinedParameter.maxValue << 1
      End Enum

      <Flags()>
      Public Enum enumRootFolderUri As Integer
         none = 0

         'predefined
         callback = JSON.enumJSONPredefinedParameter.callback
         cmisaction = JSON.enumJSONPredefinedParameter.cmisaction
         cmisselector = JSON.enumJSONPredefinedParameter.cmisselector
         succinct = JSON.enumJSONPredefinedParameter.succinct
         suppressResponseCodes = JSON.enumJSONPredefinedParameter.suppressResponseCodes
         token = JSON.enumJSONPredefinedParameter.token

         <srs.EnumMember(Value:="id")>
         objectId = JSON.enumJSONPredefinedParameter.maxValue << 1
      End Enum

      <Flags()>
      Public Enum enumTypeUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         typeId = 1

         'special
         delete = typeId
         [get] = typeId
         put = none
      End Enum

      <Flags()>
      Public Enum enumTypeDescendantsUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         typeId = 1
         depth = 2
         includePropertyDefinitions = 4

         'special
         [get] = typeId Or depth Or includePropertyDefinitions
      End Enum

      <Flags()>
      Public Enum enumTypesUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         typeId = 1
         includePropertyDefinitions = 2
         maxItems = 4
         skipCount = 8

         'special
         [get] = typeId Or includePropertyDefinitions Or maxItems Or skipCount
      End Enum

      <Flags()>
      Public Enum enumUnfiledUri As Integer
         none = 0
         <srs.EnumMember(Value:="id")>
         objectId = 1
         folderId = 2
         removeFrom = 4
         versioningState = 8
         maxItems = &H10
         skipCount = &H20
         filter = &H40
         includeAllowableActions = &H80
         includeRelationships = &H100
         renditionFilter = &H200
         orderBy = &H400
         sourceId = &H800

         'special
         createUnfiledObject = objectId Or versioningState 'createDocument or createPolicy
         getUnfiledObjects = maxItems Or skipCount Or filter Or includeAllowableActions Or includeRelationships Or renditionFilter Or orderBy
         removeObjectFromFolder = objectId Or folderId
      End Enum
#End Region

#Region "Supported TemplateUris"
      ''' <summary>
      ''' BrowserBinding
      ''' </summary>
      Private Const _absoluteObjectUri As String = _rootFolderUri & "/{*path}"
      Public Shared ReadOnly Property AbsoluteObjectUri(queryStringParameters As enumAbsoluteObjectUri) As String
         Get
            Return GetServiceUri(_absoluteObjectUri, queryStringParameters)
         End Get
      End Property

      Private Const _aclUri As String = "/{repositoryId}/acl"
      Public Shared ReadOnly Property ACLUri(queryStringParameters As enumACLUri) As String
         Get
            Return GetServiceUri(_aclUri, queryStringParameters)
         End Get
      End Property

      Private Const _allowableActionsUri As String = "/{repositoryId}/allowableactions"
      Public Shared ReadOnly Property AllowableActionsUri(queryStringParameters As enumAllowableActionsUri) As String
         Get
            Return GetServiceUri(_allowableActionsUri, queryStringParameters)
         End Get
      End Property

      Private Const _allVersionsUri As String = "/{repositoryId}/allversions"
      Public Shared ReadOnly Property AllVersionsUri(queryStringParameters As enumAllVersionsUri) As String
         Get
            Return GetServiceUri(_allVersionsUri, queryStringParameters)
         End Get
      End Property

      Private Const _changesUri As String = "/{repositoryId}/changes"
      Public Shared ReadOnly Property ChangesUri(queryStringParameters As enumChangesUri) As String
         Get
            Return GetServiceUri(_changesUri, queryStringParameters)
         End Get
      End Property

      Private Const _checkedOutUri As String = "/{repositoryId}/checkedout"
      Public Shared ReadOnly Property CheckedOutUri(queryStringParameters As enumCheckedOutUri) As String
         Get
            Return GetServiceUri(_checkedOutUri, queryStringParameters)
         End Get
      End Property

      Private Const _childrenUri As String = "/{repositoryId}/children"
      Public Shared ReadOnly Property ChildrenUri(queryStringParameters As enumChildrenUri) As String
         Get
            Return GetServiceUri(_childrenUri, queryStringParameters)
         End Get
      End Property

      Private Const _contentUri As String = "/{repositoryId}/content"
      Public Shared ReadOnly Property ContentUri(queryStringParameters As enumContentUri) As String
         Get
            Return GetServiceUri(_contentUri, queryStringParameters)
         End Get
      End Property

      Public Const DebugHelpPageUri As String = "/DebugHelpPage"

      Private Const _descendantsUri As String = "/{repositoryId}/descendants"
      Public Shared ReadOnly Property DescendantsUri(queryStringParameters As enumDescendantsUri) As String
         Get
            Return GetServiceUri(_descendantsUri, queryStringParameters)
         End Get
      End Property

      Private Const _folderTreeUri As String = "/{repositoryId}/foldertree"
      Public Shared ReadOnly Property FolderTreeUri(queryStringParameters As enumFolderTreeUri) As String
         Get
            Return GetServiceUri(_folderTreeUri, queryStringParameters)
         End Get
      End Property

      Public Const MetaDataUri As String = "/MetadataExchange"

      Private Const _objectByPathUri As String = "/{repositoryId}/objectbypath"
      Public Shared ReadOnly Property ObjectByPathUri(queryStringParameter As enumObjectByPathUri) As String
         Get
            Return GetServiceUri(_objectByPathUri, queryStringParameter)
         End Get
      End Property

      Private Const _objectParentsUri As String = "/{repositoryId}/objectparents"
      Public Shared ReadOnly Property ObjectParentsUri(queryStringParameter As enumObjectParentsUri) As String
         Get
            Return GetServiceUri(_objectParentsUri, queryStringParameter)
         End Get
      End Property

      Private Const _objectUri As String = "/{repositoryId}/object"
      Public Shared ReadOnly Property ObjectUri(Optional queryStringParameters As enumObjectUri = enumObjectUri.self) As String
         Get
            Return GetServiceUri(_objectUri, queryStringParameters)
         End Get
      End Property

      Private Const _policiesUri As String = "/{repositoryId}/policies"
      Public Shared ReadOnly Property PoliciesUri(queryStringParameters As enumPoliciesUri) As String
         Get
            Return GetServiceUri(_policiesUri, queryStringParameters)
         End Get
      End Property

      Private Const _queryUri As String = "/{repositoryId}/query"
      Public Shared ReadOnly Property QueryUri(queryStringParameters As enumQueryUri) As String
         Get
            Return GetServiceUri(_queryUri, queryStringParameters)
         End Get
      End Property

      Private Const _relationshipsUri As String = "/{repositoryId}/relationships"
      Public Shared ReadOnly Property RelationshipsUri(queryStringParameters As enumRelationshipsUri) As String
         Get
            Return GetServiceUri(_relationshipsUri, queryStringParameters)
         End Get
      End Property

      Private Const _repositoriesUri As String = "/"
      Public Shared ReadOnly Property RepositoriesUri(queryStringParameters As enumRepositoriesUri) As String
         Get
            Return GetServiceUri(_repositoriesUri, queryStringParameters)
         End Get
      End Property

      Private Const _repositoryUri As String = "/{repositoryId}"
      Public Shared ReadOnly Property RepositoryUri(queryStringParameters As enumRepositoryUri) As String
         Get
            Return GetServiceUri(_repositoryUri, queryStringParameters)
         End Get
      End Property

      Private Const _rootFolderUri As String = "/{repositoryId}/root"
      ''' <summary>
      ''' BrowserBinding
      ''' </summary>
      Public Shared ReadOnly Property RootFolderUri(queryStringParameters As enumRootFolderUri) As String
         Get
            Return GetServiceUri(_rootFolderUri, queryStringParameters)
         End Get
      End Property

      Private Const _typeUri As String = "/{repositoryId}/type"
      Public Shared ReadOnly Property TypeUri(queryStringParameters As enumTypeUri) As String
         Get
            Return GetServiceUri(_typeUri, queryStringParameters)
         End Get
      End Property

      Private Const _typeDescendantsUri As String = "/{repositoryId}/typedescendants"
      Public Shared ReadOnly Property TypeDescendantsUri(queryStringParameters As enumTypeDescendantsUri) As String
         Get
            Return GetServiceUri(_typeDescendantsUri, queryStringParameters)
         End Get
      End Property

      Private Const _typesUri As String = "/{repositoryId}/types"
      Public Shared ReadOnly Property TypesUri(queryStringParameters As enumTypesUri) As String
         Get
            Return GetServiceUri(_typesUri, queryStringParameters)
         End Get
      End Property

      Private Const _unfiledUri As String = "/{repositoryId}/unfiled"
      Public Shared ReadOnly Property UnfiledUri(queryStringParameters As enumUnfiledUri) As String
         Get
            Return GetServiceUri(_unfiledUri, queryStringParameters)
         End Get
      End Property
#End Region

#Region "Repository"
      Public Const GetRepositories As String = _repositoriesUri
      Public Const GetRepositoryInfo As String = _repositoryUri

      Public Const CreateType As String = _typesUri
      Public Const DeleteType As String = _typeUri & "?typeId={id}" 'typeId is required, no optional parameters
      Public Const GetTypeDefinition As String = _typeUri & "?typeId={id}" 'typeId is required, no optional parameters
      Public Const UpdateType As String = _typeUri

      Public Const GetTypeChildren As String = _typesUri

      Public Const GetTypeDescendants As String = _typeDescendantsUri & "?typeId={id}&depth={depth}&includePropertyDefinitions={includePropertyDefinitions}"
#End Region

#Region "Navigation"
      Public Const DeleteTree As String = _folderTreeUri
      Public Const DeleteTreeViaDescendantsFeed As String = _descendantsUri
      Public Const DeleteTreeViaChildrenFeed As String = _childrenUri
      Public Const GetCheckedOutDocs As String = _checkedOutUri
      Public Const GetChildren As String = _childrenUri
      Public Const GetDescendants As String = _descendantsUri & "?folderId={id}&filter={filter}&depth={depth}&includeAllowableActions={includeAllowableActions}&includeRelationships={includeRelationships}&renditionFilter={renditionFilter}&includePathSegment={includePathSegment}"
      Public Const GetFolderTree As String = _folderTreeUri
      Public Const GetObjectParents As String = _objectParentsUri & "?objectId={id}&filter={filter}&includeRelationships={includeRelationships}&renditionFilter={renditionFilter}&includeAllowableActions={includeAllowableActions}&includeRelativePathSegment={includeRelativePathSegment}"
      Public Const GetUnfiledObjects As String = _unfiledUri
#End Region

#Region "Object"
      Public Const AppendContentStream As String = _contentUri
      Public Const BulkUpdateProperties As String = "/{repositoryId}/updates/"
      Public Const CreateOrMoveChildObject As String = _childrenUri
      Public Const CreateUnfiledObjectOrRemoveObjectFromFolder As String = _unfiledUri
      Public Const CreateRelationship As String = _relationshipsUri
      Public Const DeleteContentStream As String = _contentUri
      Public Const DeleteObject As String = _objectUri
      Public Const GetAllowableActions As String = _allowableActionsUri & "?objectId={id}" 'objectId is required, no optional parameters
      Public Const GetContentStream As String = _contentUri
      Public Const GetObject As String = _objectUri
      Public Const GetObjectByPath As String = _objectByPathUri & "?path={path}&filter={filter}&includeAllowableActions={includeAllowableActions}&includeRelationships={includeRelationships}&includePolicyIds={includePolicyIds}&includeACL={includeACL}&renditionFilter={renditionFilter}"
      Public Const SetContentStream As String = _contentUri
      Public Const UpdateProperties As String = _objectUri

      'Browser Binding
      Public Const AbsoluteObject As String = _absoluteObjectUri
      Public Const RootFolder As String = _rootFolderUri
#End Region

#Region "Multi"
      'see CreateOrMoveChildObject (addObjectToFolder), CreateUnfiledObjectOrRemoveObjectFromFolder (removeObjectFromFolder)
#End Region

#Region "Discovery"
      Public Const GetContentChanges As String = _changesUri & "?filter={filter}&maxItems={maxItems}&includeACL={includeACL}&includePolicyIds={includePolicyIds}&includeProperties={includeProperties}&changeLogToken={changeLogToken}"
      Public Const Query As String = _queryUri
#End Region

#Region "Versioning"
      'see DeleteObject (cancelCheckOut), CheckInOrUpdateProperties (checkIn), GetObject (getObjectOfLatestVersion)
      Public Const CheckOut As String = _checkedOutUri
      Public Const GetAllVersions As String = _allVersionsUri
#End Region

#Region "Relationships"
      Public Const GetObjectRelationships As String = _relationshipsUri
#End Region

#Region "Policies"
      Public Const ApplyPolicy As String = _policiesUri
      Public Const RemovePolicy As String = _policiesUri & "?policyId={policyId}&objectId={id}"
      Public Const GetAppliedPolicies As String = _policiesUri & "?objectId={id}"
#End Region

#Region "ACL"
      Public Const GetAcl As String = _aclUri & "?objectId={id}"
      Public Const ApplyAcl As String = _aclUri & "?objectId={id}"
#End Region

   End Class
End Namespace